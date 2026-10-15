/**
 * Claude・Cursor のレビュー結果を集計し、コミットへのコメントとメール本文を作るスクリプト。
 *
 * 入力(環境変数):
 *   HAS_DIFF          : レビュー対象の変更があるか(true / false)
 *   CLAUDE_OUTCOME    : Claude レビューの手順の結果(success / failure / skipped)
 *   CURSOR_OUTCOME    : Cursor レビューの手順の結果(success / failure / skipped)
 *   CURSOR_SKIPPED    : true なら Cursor は未設定(CURSOR_API_KEY が無い)のため実行していない
 *   CLAUDE_MODEL / CURSOR_MODEL : 指定したモデル(実行の記録からモデル名が取れなかった時の表示に使う)
 *   PUSHER_EMAIL      : push した人の GitHub 上のメールアドレス(問題がある時だけ送る)
 *   MAIL_ALWAYS_TO    : push した人に加えて、問題がある時に送る宛先(カンマ区切りで複数可)
 *   MAIL_MIN_SEVERITY : メールを送る指摘の重要度の下限(high / medium / low。未設定なら medium)
 *   MAIL_SERVER       : 送信に使うメールサーバー(未設定なら警告を出す。送信自体はワークフロー側で止める)
 *   GITHUB_TOKEN, GITHUB_REPOSITORY, GITHUB_SHA, GITHUB_RUN_ID, GITHUB_SERVER_URL
 *   DRY_RUN=1         : コメント投稿をしない(ローカル確認用)
 *   RESULTS_DIR       : 下の入力ファイルを置いたフォルダ(未設定なら今のフォルダ)
 * 入力(ファイル):
 *   claude-stream.jsonl       : Claude の実行の記録(使ったモデル名と、決めた形で返したレビュー結果を取り出す)
 *   cursor-stream.jsonl       : Cursor の実行の記録(使ったモデル名とレビュー結果を取り出す)
 * 出力:
 *   review.md   : 指摘の全文(Actions 画面の表示とコミットコメントに使う)
 *   mail.txt    : メール本文(判定・件数・指摘の全文・リンク)
 *   GITHUB_OUTPUT に mail_to / mail_subject / has_issues / ai_failed / ai_failed_names
 * メールは MAIL_MIN_SEVERITY 以上のレビューの指摘があった時に、push した人と MAIL_ALWAYS_TO に送る。
 * AI 自体が失敗しただけの時は MAIL_ALWAYS_TO(管理者)だけに送る
 */
const fs = require("fs");
const path = require("path");

const SEV_ORDER = ["high", "medium", "low"];
const SEV_LABEL = { high: "重大", medium: "要注意", low: "軽微" };

/**
 * レビューを行う AI(この順番でレビューし、この順番で表示する)
 *   stream  : 実行の記録(1行に1つの JSON)。最初の init イベントに使ったモデル名が入っている
 *   model   : 指定したモデルの環境変数名(記録からモデル名が取れなかった時に表示する)
 *   hint    : 失敗した時に表示する、よくある原因と直し方
 */
const REVIEWERS = [
  {
    name: "Claude", stream: "claude-stream.jsonl",
    outcome: "CLAUDE_OUTCOME", skipped: "", model: "CLAUDE_MODEL",
    hint: "Secrets の CLAUDE_CODE_OAUTH_TOKEN の期限切れ(約1年)・利用上限・時間切れ(25分)が考えられます。期限切れの時は PC で claude setup-token を実行し、登録し直してください",
  },
  {
    name: "Cursor", stream: "cursor-stream.jsonl",
    outcome: "CURSOR_OUTCOME", skipped: "CURSOR_SKIPPED", model: "CURSOR_MODEL",
    hint: "Secrets の CURSOR_API_KEY の無効化・利用上限・時間切れ(25分)が考えられます。キーが無効な時は Cursor のダッシュボードで発行し直し、登録し直してください",
  },
];

/** 各 AI の状態の表示 */
const STATUS_LABEL = {
  ok: "成功",
  failed: "失敗(ログを確認してください)",
  skipped: "未設定のため実行せず",
  notarget: "レビュー対象外",
};

function env(name, def = "") {
  return process.env[name] ?? def;
}

/** 入力ファイルのパス(RESULTS_DIR の中) */
function inputPath(file) {
  return path.join(env("RESULTS_DIR", "."), file);
}

/**
 * 結果の JSON を文字列から取り出す。前後に説明文やコードブロックの記号が付いていても読めるよう、
 * そのまま読めない時は最初の { から最後の } までを読む
 */
function parseJsonLoosely(text) {
  try {
    return JSON.parse(text);
  } catch {
    const start = text.indexOf("{");
    const end = text.lastIndexOf("}");
    if (start < 0 || end <= start) {
      return null;
    }
    try {
      return JSON.parse(text.slice(start, end + 1));
    } catch {
      return null;
    }
  }
}

/**
 * 実行の記録(1行に1つの JSON)から、使ったモデル名(init イベント)と最後の結果の文章(result イベント)を取り出す。
 * 決めた形で返した結果(Claude の --json-schema の structured_output)があれば、それを結果の文章にする。
 * 記録が無い・読めない行は無視する
 */
function readStream(file) {
  let text;
  try {
    text = fs.readFileSync(inputPath(file), "utf8");
  } catch {
    return { model: "", result: "" };
  }
  let model = "";
  let result = "";
  for (const line of text.split(/\r?\n/)) {
    let ev;
    try {
      ev = JSON.parse(line);
    } catch {
      continue;
    }
    if (ev && ev.type === "system" && ev.subtype === "init" && ev.model && !model) {
      model = String(ev.model);
    }
    if (ev && ev.type === "result") {
      if (ev.structured_output && typeof ev.structured_output === "object") {
        result = JSON.stringify(ev.structured_output);
      } else if (typeof ev.result === "string") {
        result = ev.result;
      }
    }
  }
  return { model, result };
}

/** AI の結果の文章を読む。無い・壊れている場合は null を返す */
function loadReview(text) {
  const data = text ? parseJsonLoosely(text) : null;
  if (data === null || typeof data !== "object" || Array.isArray(data)) {
    return null;
  }
  const issues = (Array.isArray(data.issues) ? data.issues : []).filter(
    (i) => i !== null && typeof i === "object" && !Array.isArray(i),
  );
  for (const i of issues) {
    if (!SEV_ORDER.includes(i.severity)) {
      i.severity = "low";
    }
  }
  return { summary: String(data.summary ?? ""), issues };
}

function links() {
  const server = env("GITHUB_SERVER_URL", "https://github.com");
  const repo = env("GITHUB_REPOSITORY");
  return {
    runUrl: `${server}/${repo}/actions/runs/${env("GITHUB_RUN_ID")}`,
    commitUrl: `${server}/${repo}/commit/${env("GITHUB_SHA")}`,
  };
}

function countBySeverity(issues) {
  const counts = {};
  for (const s of SEV_ORDER) {
    counts[s] = issues.filter((i) => i.severity === s).length;
  }
  return counts;
}

/** 表示するモデル名。実行の記録から取れた名前、無ければ指定したモデル、それも無ければ不明 */
function modelLabel(r, streamModel) {
  return streamModel || env(r.model).trim() || "不明(標準のモデル)";
}

/**
 * 1つの AI の状態(成功・失敗・未設定・対象外)、使ったモデル、指摘、重要度ごとの件数をまとめる。
 * 件数は AI ごとに数える(同じ問題を2つの AI が指摘すると合計が二重になるため、合計はしない)
 */
function reviewerStatus(r, hasDiff) {
  const base = { name: r.name, hint: r.hint, model: "", review: null, issues: [], counts: countBySeverity([]) };
  if (!hasDiff) {
    return { ...base, status: "notarget" };
  }
  if (r.skipped && env(r.skipped) === "true") {
    return { ...base, status: "skipped" };
  }
  const stream = readStream(r.stream);
  const review = loadReview(stream.result);
  const ok = env(r.outcome) === "success" && review !== null;
  const issues = ok ? review.issues : [];
  return {
    ...base,
    status: ok ? "ok" : "failed",
    model: modelLabel(r, stream.model),
    review,
    issues,
    counts: countBySeverity(issues),
  };
}

/** メールを送る重要度の下限(high / medium / low)。未設定・不正な値の時は medium */
function mailMinSeverity() {
  const s = env("MAIL_MIN_SEVERITY").trim().toLowerCase();
  return SEV_ORDER.includes(s) ? s : "medium";
}

function collectStatus() {
  const hasDiff = env("HAS_DIFF") === "true";
  const reviewers = REVIEWERS.map((r) => reviewerStatus(r, hasDiff));

  // 判定の理由(コミットのコメントに表示する)。件数は AI ごとに出す
  // mailReasons はそのうちメールを送る理由(コードの問題)だけ。AI 自体の失敗は含めない
  // failures は失敗した AI(MAIL_ALWAYS_TO だけに知らせ、実行結果も「失敗」にする)
  const problems = [];
  const mailReasons = [];
  const failures = [];
  const minSeverity = mailMinSeverity();
  for (const r of reviewers) {
    if (r.status === "failed") {
      problems.push(`${r.name}レビュー失敗`);
      failures.push(r);
    } else if (r.issues.length > 0) {
      problems.push(`${r.name}指摘${r.issues.length}件`);
      // MAIL_MIN_SEVERITY 以上の指摘がある時だけメールを送る
      const mailCount = r.issues.filter(
        (i) => SEV_ORDER.indexOf(i.severity) <= SEV_ORDER.indexOf(minSeverity),
      ).length;
      if (mailCount > 0) {
        mailReasons.push(`${r.name}指摘${r.issues.length}件`);
      }
    }
  }
  return { hasDiff, reviewers, problems, mailReasons, failures };
}

/** 失敗した AI の名前(「Claude / Cursor」のように区切る) */
function failureNames(st) {
  return st.failures.map((r) => r.name).join(" / ");
}

/** 失敗した AI ごとの、よくある原因と直し方の行 */
function failureHintLines(st) {
  return st.failures.map((r) => `${r.name}: ${r.hint}`);
}

/** 重要度の高い順に並べた指摘 */
function sortedIssues(issues) {
  return [...issues].sort((a, b) => SEV_ORDER.indexOf(a.severity) - SEV_ORDER.indexOf(b.severity));
}

/** 指摘の各項目を取り出す。古い形式(message / suggestion)の結果でも表示できるようにする */
function issueParts(i) {
  return {
    problem: String(i.problem || i.message || ""),
    improvement: String(i.improvement || i.suggestion || ""),
    code: String(i.fixed_code || "").replace(/^\n+|\n+$/g, ""),
    where: `${i.file ?? "?"}:${i.line ?? "?"}`,
  };
}

/**
 * 各 AI の状態の1行。
 * 例:「Claude(モデル: claude-opus-5-5): 成功 指摘2件(重大 1 / 要注意 1 / 軽微 0)」
 */
function statusText(r) {
  const who = r.model ? `${r.name}(モデル: ${r.model})` : r.name;
  if (r.status !== "ok") {
    return `${who}: ${STATUS_LABEL[r.status]}`;
  }
  const c = r.counts;
  return `${who}: 成功 指摘${r.issues.length}件(重大 ${c.high} / 要注意 ${c.medium} / 軽微 ${c.low})`;
}

const MAX_COMMENT_CHARS = 60000; // GitHub のコメントの文字数上限(約6.5万字)に余裕を持たせる

/** コード例を囲む記号。コード内に ``` があっても崩れないよう長さを調整する */
function fence(code) {
  let n = 3;
  while (code.includes("`".repeat(n))) {
    n++;
  }
  return "`".repeat(n);
}

function makeReviewMd(st) {
  const { runUrl } = links();
  const lines = [`## 🤖 自動コードレビュー (${env("GITHUB_SHA").slice(0, 7)})`, ""];
  for (const r of st.reviewers) {
    lines.push(`- ${statusText(r)}`);
  }
  if (!st.hasDiff) {
    lines.push("- まとめ: レビュー対象の変更が無いためレビュー対象外です");
  }
  lines.push(`- 判定: ${st.problems.length ? "⚠️ " + st.problems.join(" / ") : "✅ 問題なし"}`);
  lines.push("");

  if (st.failures.length) {
    lines.push("## 失敗した AI の確認先", "");
    lines.push(...failureHintLines(st).map((h) => `- ${h}`), "");
  }

  for (const r of st.reviewers) {
    if (r.status !== "ok") {
      continue;
    }
    lines.push(`## ${r.name} のレビュー(モデル: ${r.model})`, "");
    if (r.review.summary) {
      lines.push(`まとめ: ${r.review.summary}`, "");
    }
    sortedIssues(r.issues).forEach((i, idx) => {
      const p = issueParts(i);
      lines.push(
        "---",
        `### ${idx + 1}. 【${SEV_LABEL[i.severity]}】${i.category ?? ""} — \`${p.where}\``,
        "",
        `**指摘**: ${p.problem}`,
        "",
        `**改善案**: ${p.improvement}`,
      );
      if (p.code) {
        const f = fence(p.code);
        lines.push("", "改善後のコード例:", f, p.code, f);
      }
      lines.push("");
    });
  }
  lines.push("---", `[実行結果の詳細](${runUrl})`);

  let text = lines.join("\n") + "\n";
  const chars = Array.from(text); // 絵文字などを途中で切らないよう文字単位で数える
  if (chars.length > MAX_COMMENT_CHARS) {
    text =
      chars.slice(0, MAX_COMMENT_CHARS).join("") +
      `\n\n…(長すぎるため省略しました。[実行結果の詳細](${runUrl}) を確認してください)\n`;
  }
  return text;
}

/** メール本文:判定・件数に加えて、AI ごとの指摘・改善案・コード例を全文載せる(文字だけの形式) */
function makeMail(st) {
  const { runUrl, commitUrl } = links();
  const repo = env("GITHUB_REPOSITORY");
  const short = env("GITHUB_SHA").slice(0, 7);
  const failed = st.failures.map((r) => `${r.name}レビュー失敗`);
  let subject;
  if (st.mailReasons.length) {
    subject = `[要確認] ${repo} (${short}) ${[...st.mailReasons, ...failed].join(" / ")}`;
  } else if (failed.length) {
    subject = `[レビュー失敗] ${repo} (${short}) ${failed.join(" / ")}`;
  } else if (!st.hasDiff) {
    subject = `[対象外] ${repo} (${short}) レビュー対象の変更なし`;
  } else {
    subject = `[問題なし] ${repo} (${short}) 自動レビュー完了`;
  }
  const lines = [
    `リポジトリ: ${repo}`,
    `コミット: ${short}`,
    "",
    `判定: ${st.mailReasons.length ? "要確認" : failed.length ? "レビュー失敗" : st.hasDiff ? "問題なし" : "レビュー対象外"}`,
  ];
  for (const r of st.reviewers) {
    lines.push(statusText(r));
  }
  lines.push("");

  if (st.failures.length) {
    lines.push("#".repeat(60), "# 失敗した AI の確認先", "#".repeat(60));
    lines.push(...failureHintLines(st), "");
  }

  for (const r of st.reviewers) {
    if (r.status !== "ok") {
      continue;
    }
    lines.push("#".repeat(60), `# ${r.name} のレビュー(モデル: ${r.model})`, "#".repeat(60));
    if (r.review.summary) {
      lines.push(`まとめ: ${r.review.summary}`);
    }
    lines.push("");
    const issues = sortedIssues(r.issues);
    issues.forEach((i, idx) => {
      const p = issueParts(i);
      lines.push(
        "=".repeat(60),
        `${idx + 1}. 【${SEV_LABEL[i.severity]}】${i.category ?? ""}`,
        `場所: ${i.file ?? "?"} の ${i.line ?? "?"} 行目`,
        "",
        "■ 指摘",
        p.problem,
        "",
        "■ 改善案",
        p.improvement,
      );
      if (p.code) {
        lines.push("", "■ 改善後のコード例", "-".repeat(40));
        lines.push(...p.code.split(/\r?\n/).map((ln) => "    " + ln));
        lines.push("-".repeat(40));
      }
      lines.push("");
    });
    if (issues.length > 0) {
      lines.push("=".repeat(60), "");
    }
  }

  lines.push("▼ 結果を見る", runUrl, "▼ コミットを見る", commitUrl, "");
  return { subject, body: lines.join("\n") };
}

async function postCommitComment(body) {
  const token = env("GITHUB_TOKEN");
  const repo = env("GITHUB_REPOSITORY");
  const sha = env("GITHUB_SHA");
  if (env("DRY_RUN") || !(token && repo && sha)) {
    console.log("(コミットへのコメント投稿はスキップ)");
    return;
  }
  try {
    const res = await fetch(`https://api.github.com/repos/${repo}/commits/${sha}/comments`, {
      method: "POST",
      headers: { Authorization: `Bearer ${token}`, Accept: "application/vnd.github+json" },
      body: JSON.stringify({ body }),
      signal: AbortSignal.timeout(30000),
    });
    if (!res.ok) {
      throw new Error(`HTTP ${res.status} ${await res.text()}`);
    }
  } catch (e) {
    // コメントに失敗してもメールは送る
    console.log(`::warning::コミットへのコメント投稿に失敗しました: ${e.message}`);
  }
}

/**
 * メールの宛先。レビューの指摘がある時は、MAIL_ALWAYS_TO の宛先と push した本人に送る(同じアドレスは1通にまとめる)。
 * AI 自体の失敗だけの時は、仕組みの管理者として MAIL_ALWAYS_TO の宛先だけに送る。
 * 問題なし・レビュー対象外の時は誰にも送らない
 */
function mailRecipients(st) {
  if (!st.hasDiff || !(st.mailReasons.length || st.failures.length)) {
    return [];
  }
  const recipients = [];
  const add = (addr) => {
    if (addr && !recipients.includes(addr)) {
      recipients.push(addr);
    }
  };
  // カンマ・空白・改行のどれで区切っても良い
  for (const addr of env("MAIL_ALWAYS_TO").split(/[\s,]+/)) {
    add(addr);
  }
  if (!st.mailReasons.length) {
    if (!recipients.length) {
      console.log(`::warning::${failureNames(st)} のレビューが失敗しましたが、Variables の MAIL_ALWAYS_TO が未設定のため、失敗のメールは送りません`);
    }
    return recipients;
  }
  add(pusherAddress());
  return recipients;
}

/** push した本人のアドレス。メールが届かないアドレスの場合は警告を出して空文字を返す */
function pusherAddress() {
  const addr = env("PUSHER_EMAIL").trim();
  if (!addr) {
    console.log("::warning::push した人のメールアドレスが取得できなかったため、その人にはメールを送りません");
    return "";
  }
  if (addr.endsWith("@users.noreply.github.com")) {
    console.log(
      `::warning::push した人のアドレス ${addr} は GitHub の非公開用アドレスのためメールが届きません。その人の GitHub の Settings → Emails で「Keep my email addresses private」のチェックを外してください`,
    );
    return "";
  }
  return addr;
}

async function main() {
  const st = collectStatus();
  const reviewMd = makeReviewMd(st);
  const { subject, body } = makeMail(st);

  fs.writeFileSync("review.md", reviewMd, "utf8");
  fs.writeFileSync("mail.txt", body, "utf8");
  const summary = env("GITHUB_STEP_SUMMARY");
  if (summary) {
    fs.appendFileSync(summary, reviewMd, "utf8");
  }
  await postCommitComment(reviewMd);

  const to = mailRecipients(st).join(",");
  if (to && !env("MAIL_SERVER").trim()) {
    console.log("::warning::Variables の MAIL_SERVER が未設定のため、メールは送りません。Settings → Secrets and variables → Actions の Variables タブで MAIL_SERVER を登録してください");
  }
  const out = env("GITHUB_OUTPUT");
  if (out) {
    fs.appendFileSync(
      out,
      `mail_to=${to}\nmail_subject=${subject}\nhas_issues=${st.mailReasons.length ? "true" : "false"}\n` +
        `ai_failed=${st.failures.length ? "true" : "false"}\nai_failed_names=${failureNames(st)}\n`,
      "utf8",
    );
  }
  console.log(reviewMd);
  console.log("----- メール -----");
  console.log(`宛先: ${to || "(送信しない)"}\n件名: ${subject}\n${body}`);
}

main();
