/*******************************************
 * 複数入力ダイアログ（共通）
 *
 * ■ボタンから呼び出す場合（HTMLのみで可）
 *   <button type="button" data-multi-input="#テキストボックスのID" data-multi-title="タイトル">複数</button>
 *
 * ■JSから呼び出す場合
 *   MultiInputDialog.open({
 *       title: "資産番号",
 *       values: ["A001", "A002"],
 *       onSelect: function (values) { ... }
 *   });
 *
 *   //[CC431] ADD Start 20261013
 * ■入力欄の文字種・文字数を制限する場合（指定したボタン・呼び出しだけ有効。指定しなければ制限なし）
 *   data-multi-maxlength="24"            ・・・入力欄 1 つ（1 件の値）の最大文字数。超えた状態で選択を押すと、
 *                                            ダイアログ内のメッセージ欄（.multi-input-message）にエラーを表示し、閉じない
 *   data-multi-edit="editAssetNo"        ・・・入力欄の文字種を制限する関数名（common.js の editXxx($this) 形式）
 *   ※制限を指定した場合は、区切り文字「|」も入力できない
 *   例）<button type="button" data-multi-input="#SearchAssetNo" data-multi-maxlength="24" data-multi-edit="editAssetNo">複数</button>
 *   JS の場合は open の options に maxLength: 24, edit: "editAssetNo" を指定する
 *   //[CC431] ADD End 20261013
 *
 * ■テキストボックス・プルダウン内の×（クリアボタン）
 *   <span class="clearable">
 *       <input type="text" ... />   または   <select ...>（値が空の選択肢が必要）</select>
 *       <span class="clearable-clear">×</span>
 *   </span>
 *   ・値が空、または入力不可（disabled）の場合は×を表示しない
 *   ・JS で値や入力可否を変更した場合は refreshClearable(target) で×の表示を更新する
 ********************************************/
var MultiInputDialog = (function ($) {

    // 区切り文字
    var SEPARATOR = "|";

    // 入力欄の数
    var ROW_COUNT = 10;

    // 選択ボタン押下時のコールバック
    var selectCallback = null;

    //[CC431] ADD Start 20261013
    // 入力欄の制限（0 / 空は制限なし）
    var currentMaxLength = 0;
    var currentEdit = "";
    //[CC431] ADD End 20261013

    /********************************
     * 区切り文字で分割（空要素は除く）
     *********************************/
    function split(value) {
        return (value || "").split(SEPARATOR)
            .map(function (v) { return $.trim(v); })
            .filter(function (v) { return v !== ""; });
    }

    /********************************
     * 区切り文字で結合（空要素は除く）
     *********************************/
    function join(values) {
        return values
            .map(function (v) { return $.trim(v); })
            .filter(function (v) { return v !== ""; })
            .join(SEPARATOR);
    }

    //[CC431] ADD Start 20261013
    /********************************
     * メッセージ欄の表示切替
     *   show が true の場合、メッセージ欄（.multi-input-message の data-message）を表示する。false の場合は空にする
     *********************************/
    function showMessage(show) {
        var $message = $("#multi-input-dialog .multi-input-message");
        $message.text(show ? ($message.data("message") || "") : "");
    }

    /********************************
     * 最大文字数を超えている入力欄を取得（制限なしの場合は空）
     *********************************/
    function getOverItems() {
        if (currentMaxLength <= 0) {
            return $();
        }
        return $("#multi-input-dialog .multi-input-item").filter(function () {
            return $(this).val().length > currentMaxLength;
        });
    }
    //[CC431] ADD End 20261013

    /********************************
     * ダイアログを開く
     *********************************/
    function open(options) {
        var $dialog = $("#multi-input-dialog");
        var values = options.values || [];

        $dialog.find(".modal-title").text(options.title || "複数選択");

        //[CC431] ADD Start 20261013
        currentMaxLength = parseInt(options.maxLength, 10) || 0;
        currentEdit = options.edit || "";
        showMessage(false);
        //[CC431] ADD End 20261013

        // 入力欄を生成（既存の値が入力欄の数より多い場合は、その分だけ増やす）
        var $rows = $dialog.find(".multi-input-rows").empty();
        var count = Math.max(ROW_COUNT, values.length);
        for (var i = 0; i < count; i++) {
            //[CC431] UPD Start 20261013
            // 既存の値が最大文字数を超えていても、そのまま表示する（maxlength は新しく入力する文字にだけ効く）
            var $item = $("<input type='text' class='form-control multi-input-item'>")
                .val(values[i] || "")
                .appendTo($rows);
            if (currentMaxLength > 0) {
                $item.attr("maxlength", currentMaxLength);
            }
            //[CC431] UPD End 20261013
        }

        selectCallback = options.onSelect;
        $dialog.modal("show");
    }

    /********************************
     * 選択ボタン押下
     *********************************/
    function select() {
        var $dialog = $("#multi-input-dialog");

        //[CC431] ADD Start 20261013
        // 最大文字数を超えている入力欄があれば、メッセージ欄にエラーを表示して閉じない
        var $over = getOverItems();
        if ($over.length > 0) {
            showMessage(true);
            $over.first().focus();
            return;
        }
        //[CC431] ADD End 20261013

        var values = $dialog.find(".multi-input-item").map(function () {
            return $(this).val();
        }).get();

        if (selectCallback) {
            selectCallback(split(join(values)));
        }
        $dialog.modal("hide");
    }

    /********************************
     * クリアボタン（×）の表示切替
     *   値が空、または入力不可（disabled）の場合は×を表示しない
     *   target にはセレクタ文字列・要素・jQuery オブジェクトを指定できる（複数可）
     *********************************/
    function refreshClear(target) {
        $(target).each(function () {
            var $input = $(this);
            $input.siblings(".clearable-clear")
                .toggle(($input.val() || "") !== "" && !$input.prop("disabled"));
        });
    }

    $(function () {
        var $dialog = $("#multi-input-dialog");

        // 表示時に先頭の入力欄にフォーカス
        $dialog.on("shown.bs.modal", function () {
            $dialog.find(".multi-input-item:first").focus();
        });

        // 選択ボタン
        $dialog.on("click", ".multi-input-select", select);

        // 入力欄でEnterキー押下時は選択ボタンと同じ動作
        $dialog.on("keydown", ".multi-input-item", function (e) {
            if (e.keyCode === 13) {
                e.preventDefault();
                select();
            }
        });

        //[CC431] ADD Start 20261013
        // 入力欄の制限（文字種・区切り文字の除去）。貼り付けも input で拾う
        $dialog.on("input", ".multi-input-item", function () {
            if (!currentEdit && currentMaxLength <= 0) {
                return;
            }
            if (currentEdit && typeof window[currentEdit] === "function") {
                window[currentEdit](this);
            }
            // 区切り文字は入力させない（選択後に 2 件に分かれてしまうため）
            if (this.value.indexOf(SEPARATOR) >= 0) {
                this.value = this.value.split(SEPARATOR).join("");
            }
            // 最大文字数を超えた入力欄がなくなったら、メッセージを消す
            if (getOverItems().length === 0) {
                showMessage(false);
            }
        });
        //[CC431] ADD End 20261013

        // 閉じたらコールバックを解除
        $dialog.on("hidden.bs.modal", function () {
            selectCallback = null;
        });

        // [data-multi-input] ボタン押下でダイアログを開く
        $(document).on("click", "[data-multi-input]", function () {
            var $target = $($(this).data("multi-input"));
            open({
                title: $(this).data("multi-title"),
                //[CC431] ADD Start 20261013
                maxLength: $(this).data("multi-maxlength"),
                edit: $(this).data("multi-edit"),
                //[CC431] ADD End 20261013
                values: split($target.val()),
                onSelect: function (values) {
                    $target.val(join(values)).trigger("change");
                }
            });
        });

        // クリアボタン（×）
        $(document).on("input change", ".clearable input, .clearable select", function () {
            refreshClear($(this));
        });
        $(document).on("click", ".clearable-clear", function () {
            var $input = $(this).siblings("input, select");
            // 入力不可の場合は何もしない
            if ($input.prop("disabled") || $input.prop("readonly")) {
                return;
            }
            $input.val("").trigger("change").focus();
        });
        refreshClear(".clearable input, .clearable select");
    });

    return {
        SEPARATOR: SEPARATOR,
        open: open,
        split: split,
        join: join,
        refreshClear: refreshClear
    };

})(jQuery);

/*******************************************
 * テキストボックス・プルダウン内の×（クリアボタン）の表示を更新する（各画面から呼び出す共通関数）
 *   入力・選択（input / change）時は自動で更新されるため、次の場合に呼び出す。
 *   ・JS で値を設定し、change を発生させない場合
 *   ・入力可否（disabled）を切り替えた場合（入力不可の場合は×を表示しない）
 *
 *   例）refreshClearable("#REMOVE_REASON");
 *       refreshClearable("#OPE_STATUS, #DIVERSION_REMOVE, #REMOVE_REASON");
 ********************************************/
function refreshClearable(target) {
    MultiInputDialog.refreshClear(target);
}
