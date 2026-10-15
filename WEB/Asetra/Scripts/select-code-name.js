/*******************************************
 * select 表示共通処理（選択肢＝「コード 名称」／選択後＝コードのみ）
 *
 * ■対象
 *   Select2 を使わない通常の select（コンボボックス）に class="select-code-only" を付ける。
 *   ・リストを開いているとき … option の text（「コード 名称」）を表示
 *   ・閉じているとき         … 選択中の option の value（＝コード）のみを表示
 *
 *   例）@Html.DropDownListFor(Function(m) m.XXX, list, "",
 *           New With {.class = "cond-control select-code-only"})
 *
 *   ※SelectListItem は .Value = コード、.Text = "コード 名称" で作成する
 *     例）list.Select(Function(x) New SelectListItem With {.Value = x.Code, .Text = x.Code & " " & x.Name})
 *
 * ■補足
 *   select は閉じた状態の表示も「選択中の option の text」そのものなので、
 *   開いたとき／閉じたときで option の text を入れ替えることで実現している。
 *   Select2 を使う場合は templateSelection で切り替えられるため、この処理は不要
 *   （Scripts\select2-common.js を参照）。
 *
 * ■後から追加した要素に適用する場合
 *   SelectCodeName.init("#追加した要素を含むコンテナ");
 ********************************************/
var SelectCodeName = (function ($) {

    var TARGET = "select.select-code-only";

    /********************************
     * 適用
     *********************************/
    function init(container) {
        $(container || document).find(TARGET).each(function () {
            var $select = $(this);

            // 適用済みの場合は何もしない
            if ($select.data("codeOnlyApplied")) {
                return;
            }
            $select.data("codeOnlyApplied", true);

            // 元の text（「コード 名称」）を退避してから、コードのみの表示にする
            $select.find("option").each(function () {
                $(this).data("fullText", this.text);
            });
            collapse($select);
        });
    }

    /********************************
     * リストを開いたとき
     *   すべての選択肢を「コード 名称」に戻す
     *********************************/
    function expand($select) {
        $select.find("option").each(function () {
            var fullText = $(this).data("fullText");
            if (fullText !== undefined) {
                this.text = fullText;
            }
        });
    }

    /********************************
     * リストを閉じたとき
     *   選択中の選択肢のみコード（value）の表示にする
     *********************************/
    function collapse($select) {
        expand($select);

        var option = $select.find("option:selected")[0];
        if (option && option.value !== "") {
            option.text = option.value;
        }
    }

    $(function () {
        $(document)
            .on("mousedown focus keydown", TARGET, function () {
                expand($(this));
            })
            .on("change blur", TARGET, function () {
                collapse($(this));
            });

        init();
    });

    return {
        init: init
    };

})(jQuery);
