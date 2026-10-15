/*******************************************
 * Select2 共通設定
 *
 * ■×ボタン付きSelect2
 *   select に class="select2-clearable" を付けるだけで適用される。
 *   ・選択解除用に空の選択肢が必要（DropDownListFor の optionLabel に "" を指定）
 *   ・未選択時の表示を変える場合は data-placeholder を指定
 *   ・幅は親要素いっぱい（100%）。select の style に width を指定した場合はその幅
 *
 *   例）@Html.DropDownListFor(Function(m) m.XXX, list, "",
 *           New With {.class = "cond-control select2-clearable", .data_placeholder = "コードを選択"})
 *
 * ■複数選択の×ボタン付きSelect2
 *   ListBoxFor（select multiple）に同じく class="select2-clearable" を付ける。
 *   ・プロパティは配列（String() など）にする
 *   ・空の選択肢は不要
 *   ・枠の高さは普段1行分、選択数が増えたら2行分まで伸び、それ以上は枠内を縦スクロールする（Content\Site.css）
 *     枠にマウスを乗せると選択中の値（「コード 名称」）を全件ツールチップ表示する
 *
 *   例）@Html.ListBoxFor(Function(m) m.XXX, list, New With {.class = "cond-control select2-clearable"})
 *
 * ■表示
 *   ・選択肢（リスト）：option の text（「コード 名称」）をそのまま表示
 *   ・選択後（単一選択）：option の text（「コード 名称」）をそのまま表示
 *   ・選択後（複数選択のタグ）：option の value（コード）のみ表示
 *   ・SelectListItem は .Value = コード、.Text = "コード 名称" で作成する
 *     例）list.Select(Function(x) New SelectListItem With {.Value = x.Code, .Text = x.Code & " " & x.Name})
 *
 * ■後から追加した要素に適用する場合
 *   Select2Common.init("#追加した要素を含むコンテナ");
 ********************************************/
var Select2Common = (function ($) {

    // 未選択時の表示（data-placeholder 未指定時）
    var DEFAULT_PLACEHOLDER = "選択してください";

    /********************************
     * 選択肢（リスト）の表示
     *   option の text（「コード 名称」）をそのまま表示する
     *********************************/
    function formatResult(data) {
        return data.text;
    }

    /********************************
     * 選択後の表示（複数選択のタグ）
     *   option の value（＝コード）のみを表示する。
     *   ※単一選択の選択後は formatResult と同じく「コード 名称」を表示する
     *   ※value が空の選択肢（プレースホルダー・選択解除用）は text をそのまま表示
     *********************************/
    function formatSelection(data) {
        return data.id || data.text;
    }

    /********************************
     * 言語設定
     *   ×ボタンのツールチップは文言マスタを使う。
     *   ・複数選択：CommonLabel.LABEL_TOOLTIP_ALL_CLEAR（_Layout.vbhtml の strSelect2AllClearTooltip）
     *   ・単一選択：CommonLabel.LABEL_TOOLTIP_CLEAR（_Layout.vbhtml の strSelect2ClearTooltip）
     *   ※配列の前にある設定が優先され、それ以外は ja の文言になる。
     *     変数が未定義の画面では ja の文言（すべてのアイテムを削除）のまま
     *********************************/
    function getLanguage(isMultiple) {
        var tooltip = isMultiple
            ? (typeof strSelect2AllClearTooltip !== "undefined" ? strSelect2AllClearTooltip : "")
            : (typeof strSelect2ClearTooltip !== "undefined" ? strSelect2ClearTooltip : "");

        if (!tooltip) {
            return 'ja';
        }
        return [{
            removeAllItems: function () {
                return tooltip;
            }
        }, 'ja'];
    }

    /********************************
     * ×ボタン付きSelect2を適用
     *********************************/
    function init(container) {
        $(container || document).find("select.select2-clearable").each(function () {
            var $select = $(this);

            // 適用済みの場合は何もしない
            if ($select.hasClass("select2-hidden-accessible")) {
                return;
            }

            $select.select2({
                language: getLanguage($select.prop("multiple")),
                allowClear: true,
                placeholder: $select.data("placeholder") || DEFAULT_PLACEHOLDER,  // allowClear には placeholder の指定が必要
                width: this.style.width ? "style" : "100%",  // style に width の指定があればその幅（モーダル画面など）
                templateResult: formatResult,        // 選択肢（リスト）は「コード 名称」をそのまま表示
                templateSelection: $select.prop("multiple") ? formatSelection : formatResult  // 選択後は 単一選択＝「コード 名称」、複数選択のタグ＝コードのみ
            });

            // 複数選択は枠の高さを2行分までとし、それ以上は縦スクロールさせるため（Content\Site.css）、
            // 枠のツールチップに選択中の値（「コード 名称」）を全件表示する
            if ($select.prop("multiple")) {
                $select.on("change.select2common", function () {
                    updateMultipleTitle($select);
                });
                $select.on("select2:open.select2common", function () {
                    scrollMultipleToBottom($select);  // 入力欄（末尾）が見えるようにする
                });
                updateMultipleTitle($select);
            }
        });
    }

    /********************************
     * 複数選択：枠のツールチップを選択中の値で更新
     *********************************/
    function updateMultipleTitle($select) {
        var texts = $select.find("option:selected").map(function () {
            return $(this).text();
        }).get();

        $select.next(".select2-container").find(".select2-selection--multiple")
            .attr("title", texts.join("\n"));

        scrollMultipleToBottom($select);  // 最後に選択した値が見えるようにする
    }

    /********************************
     * 複数選択：選択値の一覧を末尾までスクロール
     *********************************/
    function scrollMultipleToBottom($select) {
        var $rendered = $select.next(".select2-container").find(".select2-selection__rendered");
        $rendered.scrollTop($rendered.prop("scrollHeight"));
    }

    /********************************
     * Select2の表示のみ更新（onchange は発生させない）
     *   select の値を JS で変更した後に呼び出す。
     *   ※jQuery の trigger("change.select2") は、名前空間付きでも onchange 属性のハンドラー
     *     （onchange="ChangeLocationL(this)" など）を呼び出してしまうため、一時的に外してから実行する
     *********************************/
    function refreshSelect2(target) {
        $(target).each(function () {
            var onchange = this.onchange;
            this.onchange = null;
            try {
                $(this).trigger("change.select2");
            } finally {
                this.onchange = onchange;
            }
        });
    }

    /********************************
     * 複数選択：選択値を取得（未選択時は空の配列）
     *********************************/
    function getMultiSelectValues(target) {
        return $(target).val() || [];
    }

    /********************************
     * 複数選択：選択値を設定
     *   onchange は発生させず、Select2の表示（とツールチップ）のみ更新する。
     *   選択肢を入れ替えた場合も呼び出す。
     *********************************/
    function setMultiSelectValues(target, values) {
        var $select = $(target);
        $select.val(values && values.length > 0 ? values : null);
        refreshSelect2($select);
        updateMultipleTitle($select);
    }

    /********************************
     * 複数選択：選択値をクリア（onchange は発生させない）
     *********************************/
    function clearMultiSelect(target) {
        setMultiSelectValues(target, null);
    }

    $(function () {
        init();
    });

    return {
        init: init,
        refreshSelect2: refreshSelect2,
        getMultiSelectValues: getMultiSelectValues,
        setMultiSelectValues: setMultiSelectValues,
        clearMultiSelect: clearMultiSelect
    };

})(jQuery);

/*******************************************
 * Select2の操作（各画面から呼び出す共通関数）
 *   target にはセレクタ文字列（"#SearchLocM" など）または要素を指定する。
 *
 *   refreshSelect2(target)                JS で値を変更した後、Select2の表示のみ更新（onchange は発生させない）
 *                                         ※$(target).trigger("change.select2") は onchange 属性のハンドラーも
 *                                           呼び出してしまうため、代わりにこちらを使用する
 *   getMultiSelectValues(target)          複数選択：選択値を配列で取得（未選択時は []）
 *   setMultiSelectValues(target, values)  複数選択：選択値を設定（onchange は発生させず、表示のみ更新）
 *   clearMultiSelect(target)              複数選択：選択値をクリア（onchange は発生させず、表示のみ更新）
 ********************************************/
function refreshSelect2(target) {
    Select2Common.refreshSelect2(target);
}

function getMultiSelectValues(target) {
    return Select2Common.getMultiSelectValues(target);
}

function setMultiSelectValues(target, values) {
    Select2Common.setMultiSelectValues(target, values);
}

function clearMultiSelect(target) {
    Select2Common.clearMultiSelect(target);
}
