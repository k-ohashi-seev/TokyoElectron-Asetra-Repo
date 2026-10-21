/**
 * Claude・Cursor のレビュー結果を集計し、コミットへのコメント・メール本文・Issue を作るスクリプト。
 * 引数で役割を切り替える:
 *   node report.js            : 集計(report ジョブ。下の入力・出力)
 *   node report.js open-items : 今回変更したファイルにある未対応の Issue の項目を open-issues.json に書き出す(prepare ジョブ。
 *                               CHANGED_FILES に変更したファイルの一覧のファイルを渡す)
 *   node report.js matome     : 2つの AI の同じ指摘を AI にまとめさせる時の指示文 matome-prompt.txt と
 *                               返答の形 matome-schema.json を書き出す(matome ジョブ。まとめる必要が無ければ書かない)
 *   node report.js close-if-done : ISSUE_NUMBER の Issue の項目すべてにチェックが付いていれば閉じる
 *                               (issue-close.yml。最後の項目を人が手でチェックした時に閉じるため)
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
 *   ISSUE_MIN_SEVERITY: Issue にする指摘の重要度の下限(high / medium / low。未設定なら medium)
 *   GITHUB_TOKEN, GITHUB_REPOSITORY, GITHUB_SHA, GITHUB_RUN_ID, GITHUB_SERVER_URL
 *   DRY_RUN=1         : コメント投稿・Issue の書き込みをしない(ローカル確認用)
 *   RESULTS_DIR       : 下の入力ファイルを置いたフォルダ(未設定なら今のフォルダ)
 * 入力(ファイル):
 *   claude-stream.jsonl       : Claude の実行の記録(使ったモデル名と、決めた形で返したレビュー結果を取り出す)
 *   cursor-stream.jsonl       : Cursor の実行の記録(使ったモデル名とレビュー結果を取り出す)
 *   open-issues.json          : AI に直ったかを判断させた、未対応の項目の一覧(open-items で作る)
 *   matome-stream.jsonl       : 同じ問題の指摘を AI がまとめた結果(matome ジョブ。無ければまとめずに Issue にする)
 * Issue:
 *   ISSUE_MIN_SEVERITY 以上の新しい指摘を、push ごとに1件の Issue(チェックリスト)にする。
 *   open-issues.json の項目は、成功した AI すべてが「直った」とした時だけチェックを付け、全部に付いたら閉じる
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
  // 未対応の項目が直ったかの判断。番号が文字列で返ってきても読めるようにする
  const checks = (Array.isArray(data.checks) ? data.checks : [])
    .filter((c) => c !== null && typeof c === "object")
    .map((c) => ({ issue: Number(c.issue), item: Number(c.item), fixed: c.fixed === true, reason: String(c.reason ?? "") }));
  return { summary: summaryParts(data.summary), issues, checks };
}

/**
 * まとめを「変更内容・確認した範囲・結論」に分けて取り出す。
 * 古い形式(まとめが1つの文章)の結果は、結論として扱う
 */
function summaryParts(s) {
  if (typeof s === "string") {
    return { changes: [], checked: "", conclusion: s };
  }
  const obj = s !== null && typeof s === "object" && !Array.isArray(s) ? s : {};
  return {
    changes: (Array.isArray(obj.changes) ? obj.changes : []).map((c) => String(c ?? "").trim()).filter(Boolean),
    checked: String(obj.checked ?? "").trim(),
    conclusion: String(obj.conclusion ?? "").trim(),
  };
}

/**
 * まとめの表示行。コミットのコメント(Markdown)とメール(文字だけ)の両方でそのまま読める形にする。
 * 箇条書きの中に番号を入れ子にすると GitHub では i, ii と表示されるため、入れ子にせず空行で区切る
 */
function summaryLines(s) {
  const blocks = [];
  if (s.changes.length) {
    blocks.push(["変更内容:", ...s.changes.map((c, idx) => `${idx + 1}. ${c}`)]);
  }
  if (s.checked) {
    blocks.push([`確認した範囲: ${s.checked}`]);
  }
  if (s.conclusion) {
    blocks.push([`結論: ${s.conclusion}`]);
  }
  return blocks.flatMap((b, idx) => (idx ? ["", ...b] : b));
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

/** 環境変数で指定した重要度の下限(high / medium / low)。未設定・不正な値の時は medium */
function minSeverity(name) {
  const s = env(name).trim().toLowerCase();
  return SEV_ORDER.includes(s) ? s : "medium";
}

/** 重要度が下限以上か */
function atLeast(severity, min) {
  return SEV_ORDER.indexOf(severity) <= SEV_ORDER.indexOf(min);
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
  const mailMin = minSeverity("MAIL_MIN_SEVERITY");
  for (const r of reviewers) {
    if (r.status === "failed") {
      problems.push(`${r.name}レビュー失敗`);
      failures.push(r);
    } else if (r.issues.length > 0) {
      problems.push(`${r.name}指摘${r.issues.length}件`);
      // MAIL_MIN_SEVERITY 以上の指摘がある時だけメールを送る
      const mailCount = r.issues.filter((i) => atLeast(i.severity, mailMin)).length;
      if (mailCount > 0) {
        mailReasons.push(`${r.name}指摘${r.issues.length}件`);
      }
    }
  }
  return { hasDiff, reviewers, problems, mailReasons, failures, issueLines: [] };
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

  if (st.issueLines.length) {
    lines.push("## Issue", "");
    lines.push(...st.issueLines.map((l) => `- ${l}`), "");
  }

  for (const r of st.reviewers) {
    if (r.status !== "ok") {
      continue;
    }
    lines.push(`## ${r.name} のレビュー(モデル: ${r.model})`, "");
    const sl = summaryLines(r.review.summary);
    if (sl.length) {
      lines.push(...sl, "");
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

  if (st.issueLines.length) {
    lines.push("#".repeat(60), "# Issue", "#".repeat(60));
    lines.push(...st.issueLines, "");
  }

  for (const r of st.reviewers) {
    if (r.status !== "ok") {
      continue;
    }
    lines.push("#".repeat(60), `# ${r.name} のレビュー(モデル: ${r.model})`, "#".repeat(60));
    lines.push(...summaryLines(r.review.summary), "");
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

// ============================================================
// Issue の管理
//   指摘は push ごとに1件の Issue(チェックリスト)にする。
//   以降の push では、指摘のあるファイルを変更した時だけ AI に直ったかを判断させる(Actions の時間を延ばさないため)
// ============================================================

/** AI レビューの Issue に付けるラベル */
const LABEL = "ai-review";
/** 1回の push で AI に直ったかを判断させる項目の上限(多すぎると時間がかかるため。古い Issue から順に選ぶ) */
const MAX_CHECK_ITEMS = 20;
/** Issue の本文の最後に隠して書く、項目の一覧(JSON を base64 にしたもの) */
const DATA_RE = /<!-- ai-review-data:([A-Za-z0-9+/=]+) -->/;

/** 重複のまとめを AI に頼む時の指示文。この後に材料の JSON を付けて渡す */
const MATOME_PROMPT = `あなたはコードレビューの指摘を整理する係です。コードは読まず、下の材料の文章だけで判断してください。

材料の findings は、2つの AI(Claude と Cursor)が同じ変更を別々にレビューした指摘(id は「AI 名-番号」)。
open_items は、以前のレビューで指摘し、まだ直っていない項目(id は「#Issue番号-項目番号」)。

同じ問題を指している指摘をまとめる。
- 同じ問題とは、同じ原因を指していて、1か所を直せばどちらも解消するもの。言い方や行番号が少し違っても同じ問題として扱う。
- ファイルが違うものはまとめない。同じファイルの近い場所でも、原因が違うもの(例: 0 で割る問題と、値があふれる問題)はまとめない。
- findings の指摘が open_items の項目と同じ問題なら、その2つを同じグループに入れる。1つのグループに open_items の項目は1つまで。

最後の返答として {"groups": [{"ids": ["Claude-1", "Cursor-2"], "main": "Claude-1"}]} の形の JSON を返す。
- groups には、2つ以上をまとめたグループだけを入れる。まとめる相手がいないものは入れない。まとめるものが無ければ []。
- main には、グループの中の findings から、説明が一番わかりやすい指摘の id を入れる(open_items の id は入れない)。

材料:
`;

/** 重複のまとめの返答の形(claude の --json-schema に渡す) */
const MATOME_SCHEMA = {
  type: "object",
  additionalProperties: false,
  required: ["groups"],
  properties: {
    groups: {
      type: "array",
      items: {
        type: "object",
        additionalProperties: false,
        required: ["ids", "main"],
        properties: { ids: { type: "array", items: { type: "string" } }, main: { type: "string" } },
      },
    },
  },
};

/** GitHub の API を呼ぶ(このリポジトリの中のパスを渡す) */
async function api(method, apiPath, body) {
  const res = await fetch(`https://api.github.com/repos/${env("GITHUB_REPOSITORY")}${apiPath}`, {
    method,
    headers: { Authorization: `Bearer ${env("GITHUB_TOKEN")}`, Accept: "application/vnd.github+json" },
    body: body === undefined ? undefined : JSON.stringify(body),
    signal: AbortSignal.timeout(30000),
  });
  if (!res.ok) {
    throw new Error(`${method} ${apiPath}: HTTP ${res.status} ${await res.text()}`);
  }
  return res.status === 204 ? null : res.json();
}

/** RESULTS_DIR の JSON ファイルを読む。無い・壊れている場合は null を返す */
function readJson(file) {
  try {
    return JSON.parse(fs.readFileSync(inputPath(file), "utf8"));
  } catch {
    return null;
  }
}

/** 文章の1行目を max 文字までに縮める */
function shortText(text, max) {
  const chars = Array.from(String(text).split(/\r?\n/)[0].trim());
  return chars.length > max ? chars.slice(0, max).join("") + "…" : chars.join("");
}

/** チェックリストの1行の先頭。この印で項目を見分ける */
function itemMarker(id, done) {
  return `- [${done ? "x" : " "}] <!-- ai-item:${id} -->`;
}

/** Issue の本文から項目を取り出す。done はチェックが付いているか(AI が直ったと判断した、または人が手で付けた) */
function parseItems(body) {
  const text = body || "";
  const m = DATA_RE.exec(text);
  if (!m) {
    return [];
  }
  let data;
  try {
    data = JSON.parse(Buffer.from(m[1], "base64").toString("utf8"));
  } catch {
    return [];
  }
  return (Array.isArray(data.items) ? data.items : []).map((it) => ({
    ...it,
    done: text.includes(itemMarker(it.id, true)) || text.includes(`- [X] <!-- ai-item:${it.id} -->`),
  }));
}

/**
 * prepare ジョブ(node report.js open-items):
 * 開いている AI レビューの Issue のうち、今回変更したファイル(CHANGED_FILES に一覧)にある未対応の項目を
 * open-issues.json に書き出す。AI はこれを読んで、各項目が直ったかを判断する
 */
async function exportOpenItems() {
  const changed = new Set(fs.readFileSync(env("CHANGED_FILES"), "utf8").split(/\r?\n/).filter(Boolean));
  let items = [];
  try {
    for (let page = 1; page <= 10; page++) {
      const issues = await api(
        "GET",
        `/issues?labels=${LABEL}&state=open&sort=created&direction=asc&per_page=100&page=${page}`,
      );
      for (const issue of issues.filter((i) => !i.pull_request)) {
        for (const it of parseItems(issue.body)) {
          if (!it.done && changed.has(it.file)) {
            items.push({
              issue: issue.number, item: it.id, file: it.file, line: it.line,
              category: it.category, problem: it.problem,
            });
          }
        }
      }
      if (issues.length < 100) {
        break;
      }
    }
  } catch (e) {
    // Issue が読めなくてもレビューは行う(直ったかの判断だけ行わない)
    console.log(`::warning::AI レビューの Issue を読めませんでした。今回は直ったかの判断を行いません: ${e.message}`);
    items = [];
  }
  if (items.length > MAX_CHECK_ITEMS) {
    console.log(`::notice::未対応の項目が ${items.length} 件あるため、古い順に ${MAX_CHECK_ITEMS} 件だけ判断します`);
    items = items.slice(0, MAX_CHECK_ITEMS);
  }
  fs.writeFileSync("open-issues.json", JSON.stringify(items, null, 2) + "\n", "utf8");
  console.log(`今回変更したファイルにある未対応の項目: ${items.length} 件`);
}

/** Issue にする指摘(ISSUE_MIN_SEVERITY 以上)。重複をまとめる時の目印に「AI 名-番号」の ID を付ける */
function issueFindings(st) {
  const min = minSeverity("ISSUE_MIN_SEVERITY");
  const findings = [];
  for (const r of st.reviewers.filter((x) => x.status === "ok")) {
    r.issues
      .filter((i) => atLeast(i.severity, min))
      .forEach((i, idx) => {
        findings.push({ id: `${r.name}-${idx + 1}`, name: r.name, issue: i, file: String(i.file ?? ""), line: Number(i.line) || 0 });
      });
  }
  return findings;
}

/** 未対応の項目の一覧(open-issues.json) */
function openItems() {
  return (readJson("open-issues.json") || []).filter((c) => c && Number.isInteger(c.issue) && Number.isInteger(c.item));
}

/** 未対応の項目の ID(「#Issue番号-項目番号」) */
function openId(c) {
  return `#${c.issue}-${c.item}`;
}

/** 重複をまとめる必要があるか(指摘のあるファイルに、ほかの指摘か未対応の項目もある時だけ) */
function needsMatome(findings, checked) {
  const count = new Map();
  for (const x of [...findings, ...checked]) {
    count.set(x.file, (count.get(x.file) || 0) + 1);
  }
  return findings.some((f) => count.get(f.file) > 1);
}

/**
 * matome ジョブ(node report.js matome):
 * 重複をまとめる必要がある時だけ、AI への指示文(matome-prompt.txt)と返答の形(matome-schema.json)を書き出し、
 * GITHUB_OUTPUT に needed=true を書く
 */
function exportMatomeInput() {
  const findings = issueFindings(collectStatus());
  const checked = openItems();
  const needed = needsMatome(findings, checked);
  const out = env("GITHUB_OUTPUT");
  if (out) {
    fs.appendFileSync(out, `needed=${needed}\n`, "utf8");
  }
  if (!needed) {
    console.log("同じファイルに複数の指摘が無いため、重複のまとめは行いません");
    return;
  }
  const material = {
    findings: findings.map((f) => ({
      id: f.id, file: f.file, line: f.line, category: f.issue.category ?? "", problem: issueParts(f.issue).problem,
    })),
    open_items: checked.map((c) => ({
      id: openId(c), file: c.file, line: c.line, category: c.category ?? "", problem: String(c.problem ?? ""),
    })),
  };
  fs.writeFileSync("matome-prompt.txt", MATOME_PROMPT + JSON.stringify(material, null, 2) + "\n", "utf8");
  fs.writeFileSync("matome-schema.json", JSON.stringify(MATOME_SCHEMA), "utf8");
  console.log(`指摘 ${material.findings.length} 件・未対応の項目 ${material.open_items.length} 件の重複をまとめます`);
}

/** matome ジョブで AI がまとめた結果。無い・壊れている場合は null を返す */
function loadMatome() {
  const data = parseJsonLoosely(readStream("matome-stream.jsonl").result);
  if (data === null || typeof data !== "object" || !Array.isArray(data.groups)) {
    return null;
  }
  return data.groups
    .filter((g) => g && typeof g === "object" && Array.isArray(g.ids))
    .map((g) => ({ ids: g.ids.map(String), main: String(g.main ?? "") }));
}

/** 同じ問題の指摘をまとめた1項目。先頭の指摘の説明を代表として表示する */
function makeGroup(members) {
  return {
    file: members[0].file,
    line: members[0].line,
    severity: SEV_ORDER.find((s) => members.some((f) => f.issue.severity === s)),
    category: String(members[0].issue.category ?? ""),
    by: [...new Set(members.map((f) => f.name))],
    details: members,
  };
}

/**
 * 指摘を Issue の項目にまとめる。AI がまとめた結果(matome)に従い、無ければ1つずつ別の項目にする。
 * 未対応の項目と同じ問題の指摘は新しい項目にせず、その項目の ID を again に入れる(直っていないとみなす)。
 * ただし AI すべてが直ったとした項目(fixedIds)とはまとめず、その指摘は新しい項目にする
 */
function planGroups(findings, checked, matome, fixedIds = new Set()) {
  const byId = new Map(findings.map((f) => [f.id, f]));
  const opens = new Map(checked.map((c) => [openId(c), c]));
  const used = new Set();
  const groups = [];
  const again = new Set();
  for (const g of matome || []) {
    // 代表(main)を先頭にする。別のファイルの指摘は同じ問題とみなさない
    const members = g.ids
      .filter((id) => byId.has(id) && !used.has(id))
      .sort((a, b) => (b === g.main) - (a === g.main))
      .map((id) => byId.get(id))
      .filter((f, _, arr) => f.file === arr[0].file);
    if (!members.length) {
      continue;
    }
    members.forEach((f) => used.add(f.id));
    const open = g.ids
      .map((id) => opens.get(id))
      .find((c) => c && c.file === members[0].file && !fixedIds.has(openId(c)));
    if (open) {
      again.add(openId(open));
    } else {
      groups.push(makeGroup(members));
    }
  }
  for (const f of findings.filter((x) => !used.has(x.id))) {
    groups.push(makeGroup([f]));
  }
  return { groups, again };
}

/**
 * Issue の本文。先頭にチェックリスト、続けて指摘の全文、最後に項目の一覧を隠して書く
 * (次の push で、項目の場所と内容を AI に渡すため)
 */
function makeIssueBody(groups) {
  const sha = env("GITHUB_SHA");
  const { runUrl, commitUrl } = links();
  const items = groups.map((g, idx) => ({
    id: idx + 1, file: g.file, line: g.line, severity: g.severity, category: g.category,
    problem: shortText(issueParts(g.details[0].issue).problem, 500),
  }));
  const head = [
    `AI レビューで見つかった指摘です(コミット [${sha.slice(0, 7)}](${commitUrl}))。`,
    "指摘のあるファイルを変更して push すると、AI が直ったかを判断してチェックを付けます。全部にチェックが付くと自動で閉じます。",
    "AI の判断を待たずに対応済みにする時は、手でチェックを付けてください。",
    "",
    ...groups.map(
      (g, idx) =>
        `${itemMarker(idx + 1, false)} ${idx + 1}. 【${SEV_LABEL[g.severity]}】${g.category} \`${g.file}:${g.line}\` ` +
        `${shortText(items[idx].problem, 60)}(${g.by.join("・")})`,
    ),
    "",
  ];
  const details = [];
  groups.forEach((g, idx) => {
    details.push("---", `### ${idx + 1}. 【${SEV_LABEL[g.severity]}】${g.category} — \`${g.file}:${g.line}\``, "");
    // 説明は代表の1つだけ載せ、同じ問題を指摘したほかの AI の説明は折りたたむ
    const [main, ...others] = g.details;
    const p = issueParts(main.issue);
    details.push(`**指摘**(${main.name}): ${p.problem}`, "", `**改善案**: ${p.improvement}`);
    if (p.code) {
      const f = fence(p.code);
      details.push("", "改善後のコード例:", f, p.code, f);
    }
    for (const d of others) {
      const q = issueParts(d.issue);
      details.push(
        "", `<details><summary>${d.name} も同じ問題を指摘しています(説明を見る)</summary>`, "",
        `**指摘**: ${q.problem}`, "", `**改善案**: ${q.improvement}`,
      );
      if (q.code) {
        const f = fence(q.code);
        details.push("", "改善後のコード例:", f, q.code, f);
      }
      details.push("", "</details>");
    }
    details.push("");
  });
  const data = `<!-- ai-review-data:${Buffer.from(JSON.stringify({ sha, items }), "utf8").toString("base64")} -->`;
  const tail = ["---", `[実行結果の詳細](${runUrl})`, "", data];
  const body = [...head, ...details, ...tail].join("\n") + "\n";
  if (Array.from(body).length <= MAX_COMMENT_CHARS) {
    return body;
  }
  // 長すぎる時は詳細を省き、チェックリストと項目の一覧は残す
  return [...head, "(指摘の詳細は長すぎるため省略しました。実行結果の詳細を確認してください)", "", ...tail].join("\n") + "\n";
}

/** AI ごとの判断と理由の行(「- Claude: 〜」を AI ごとに1行) */
function verdictLines(c) {
  return c.verdicts.map(
    (v) => `- ${v.name}: ${v.check ? `${v.check.fixed ? "直った" : "直っていない"}(${v.check.reason})` : "判断なし"}`,
  );
}

/** 新しい指摘の Issue を作る。表示用の1行を返す */
async function createIssue(groups, dry) {
  const title = `[AIレビュー] ${env("GITHUB_SHA").slice(0, 7)} の指摘(${groups.length}件)`;
  const body = makeIssueBody(groups);
  if (dry) {
    console.log(`(Issue の作成はスキップ)\n${title}\n${body}`);
    return `Issue「${title}」を作る予定です(作成はスキップ)`;
  }
  try {
    await api("POST", "/labels", { name: LABEL, color: "d93f0b", description: "AI レビューの指摘(自動で作成・クローズ)" });
  } catch (e) {
    if (!e.message.includes("HTTP 422")) { // 422 はラベルが既にある時
      throw e;
    }
  }
  const issue = await api("POST", "/issues", { title, body, labels: [LABEL] });
  return `#${issue.number} を作りました(指摘 ${groups.length} 件) ${issue.html_url}`;
}

/**
 * 既存の Issue に、直ったかの判断をコメントし、直った項目にチェックを付け、全部に付いたら閉じる。表示用の行を返す。
 * AI の判断中に人が手でチェックを付けた項目は、直っていないとは書かず「手でチェック済み」とする
 */
async function updateIssue(number, results, dry) {
  // 人が手でチェックを付けた場合もあるので、最新の本文を読み直してから判断を書く
  const issue = dry ? null : await api("GET", `/issues/${number}`);
  const doneIds = new Set(parseItems(issue?.body).filter((it) => it.done).map((it) => it.id));
  const fixed = results.filter((c) => c.fixed);
  const manual = results.filter((c) => !c.fixed && doneIds.has(c.item));
  const notFixed = results.length - fixed.length - manual.length;
  const lines = [];
  if (fixed.length) {
    lines.push(`#${number} の ${fixed.length} 件が直ったと判断し、チェックを付けました`);
  }
  if (manual.length) {
    lines.push(`#${number} の ${manual.length} 件は手でチェック済みです`);
  }
  if (notFixed) {
    lines.push(`#${number} の ${notFixed} 件はまだ直っていません`);
  }
  const comment = [
    `コミット ${env("GITHUB_SHA").slice(0, 7)} の自動レビューで、変更したファイルにある項目が直ったかを判断しました` +
      `(直った ${fixed.length} 件 / 直っていない ${notFixed} 件` +
      `${manual.length ? ` / 手でチェック済み ${manual.length} 件` : ""})。`,
    "",
  ];
  // 項目ごとに見出しで区切り、結果を先に、指摘の内容と AI ごとの判断をその下に分けて書く
  for (const c of results) {
    const state = c.fixed
      ? "✅ 直った"
      : doneIds.has(c.item)
        ? "☑️ 手でチェック済み(AI の判断中にチェックが付きました)"
        : c.again
          ? "❌ 直っていない(同じ問題がまた指摘されました)"
          : "❌ 直っていない";
    comment.push(
      "---",
      `### ${c.item}. ${state} — \`${c.file}:${c.line}\``,
      "",
      `**指摘**: ${String(c.problem ?? "").trim()}`,
      "",
      "**AI の判断**",
      ...verdictLines(c),
      "",
    );
  }
  if (dry) {
    console.log(`(Issue #${number} の更新はスキップ)\n${comment.join("\n")}`);
    return lines;
  }

  let body = issue.body || "";
  for (const c of fixed) {
    body = body.replace(itemMarker(c.item, false), itemMarker(c.item, true));
  }
  if (fixed.length) {
    await api("PATCH", `/issues/${number}`, { body });
  }
  const items = parseItems(body);
  // 手でチェックを付けた時に issue-close.yml が先に閉じていれば、閉じ直さない
  const allDone = issue.state === "open" && items.length > 0 && items.every((it) => it.done);
  if (allDone) {
    comment.push("", "すべての項目にチェックが付いたため、この Issue を閉じます。");
  }
  await api("POST", `/issues/${number}/comments`, { body: comment.join("\n") });
  if (allDone) {
    await api("PATCH", `/issues/${number}`, { state: "closed", state_reason: "completed" });
    lines.push(`#${number} はすべての項目が直ったため閉じました`);
  }
  return lines;
}

/**
 * issue-close.yml(node report.js close-if-done):
 * ISSUE_NUMBER の Issue の項目すべてにチェックが付いていれば、コメントを書いて閉じる。
 * AI の判断で最後のチェックが付いた時は updateIssue が閉じるので、ここは人が手で最後のチェックを付けた時のためのもの
 */
async function closeIfDone() {
  const number = env("ISSUE_NUMBER");
  // 編集された時点から状態が変わっていることもあるので、最新の Issue を読み直して判断する
  const issue = await api("GET", `/issues/${number}`);
  const items = parseItems(issue.body);
  if (issue.state !== "open" || !items.length || !items.every((it) => it.done)) {
    console.log(`#${number} はまだチェックの付いていない項目があるため、閉じません`);
    return;
  }
  if (env("DRY_RUN")) {
    console.log(`(#${number} を閉じるのはスキップ)`);
    return;
  }
  await api("POST", `/issues/${number}/comments`, { body: "すべての項目にチェックが付いたため、この Issue を閉じます。" });
  await api("PATCH", `/issues/${number}`, { state: "closed", state_reason: "completed" });
  console.log(`#${number} はすべての項目にチェックが付いたため閉じました`);
}

/**
 * Issue の作成・更新。表示用の行(コミットのコメントとメールに載せる)を返す。
 * 未対応の項目は、成功した AI すべてが「直った」とした時だけ直ったとする(AI の勘違いで閉じないため)。
 * 同じ問題がまた指摘された時は直っていないとみなし、その指摘は新しい項目にしない
 * (AI すべてが直ったとした項目は除く。その時の指摘は新しい項目にする)
 */
async function syncIssues(st) {
  const ok = st.reviewers.filter((r) => r.status === "ok");
  if (!st.hasDiff || !ok.length) {
    return [];
  }
  const dry = Boolean(env("DRY_RUN")) || !env("GITHUB_TOKEN");
  const checked = openItems();
  const findings = issueFindings(st);
  const matome = loadMatome();
  const judged = checked.map((c) => {
    const verdicts = ok.map((r) => ({
      name: r.name,
      check: r.review.checks.find((x) => x.issue === c.issue && x.item === c.item),
    }));
    return { ...c, verdicts, aiFixed: verdicts.every((v) => v.check && v.check.fixed) };
  });
  // コードを読んだ AI すべてが直ったとした項目は、文章だけで判断する matome がまとめても直ったとする
  const fixedIds = new Set(judged.filter((c) => c.aiFixed).map(openId));
  const { groups, again } = planGroups(findings, checked, matome, fixedIds);

  const results = judged.map(({ aiFixed, ...c }) => {
    const isAgain = again.has(openId(c));
    return { ...c, again: isAgain, fixed: !isAgain && aiFixed };
  });

  const lines = [];
  if (!matome && needsMatome(findings, checked)) {
    lines.push("重複のまとめに失敗したため、指摘をまとめずに Issue にしました");
  }
  for (const number of [...new Set(results.map((c) => c.issue))]) {
    try {
      lines.push(...(await updateIssue(number, results.filter((c) => c.issue === number), dry)));
    } catch (e) {
      console.log(`::warning::Issue #${number} の更新に失敗しました: ${e.message}`);
      lines.push(`#${number} の更新に失敗しました(実行結果の詳細を確認してください)`);
    }
  }
  if (groups.length) {
    try {
      lines.push(await createIssue(groups, dry));
    } catch (e) {
      console.log(`::warning::Issue の作成に失敗しました。Settings → General で Issues が有効か確認してください: ${e.message}`);
      lines.push("Issue の作成に失敗しました(実行結果の詳細を確認してください)");
    }
  }
  return lines;
}

async function main() {
  const st = collectStatus();
  st.issueLines = await syncIssues(st);
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

// 引数で役割を切り替える(引数なし: 集計 / open-items: prepare ジョブ / matome: matome ジョブ / close-if-done: issue-close.yml)
const mode = process.argv[2];
if (mode === "open-items") {
  exportOpenItems();
} else if (mode === "matome") {
  exportMatomeInput();
} else if (mode === "close-if-done") {
  closeIfDone().catch((e) => {
    console.log(`::error::Issue を閉じられませんでした: ${e.message}`);
    process.exitCode = 1;
  });
} else {
  main();
}
