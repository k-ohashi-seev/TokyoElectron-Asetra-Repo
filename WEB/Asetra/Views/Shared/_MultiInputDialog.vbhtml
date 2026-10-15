@Imports Asetra.Models
@Imports Asetra.Misc
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
End Code
@*
    複数入力ダイアログ（共通）
    ・_Layout.vbhtml で読み込んでいるため、各画面でこの部分ビューを読み込む必要はない
    ・呼び出し方は Scripts\multi-input-dialog.js を参照
*@
<div class="modal fade" id="multi-input-dialog" tabindex="-1" role="dialog" data-backdrop="static">
    <div class="modal-dialog modal-sm" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h4 class="modal-title"></h4>
            </div>
            <div class="modal-body">
                @* 入力欄は JS で生成する *@
                <div class="multi-input-rows"></div>
            </div>
            @*[CC431] ADD Start 20261013*@
            @* 入力欄の最大文字数を超えたときのエラー表示欄（通常は空。Scripts\multi-input-dialog.js が表示・消去する） *@
            <div class="multi-input-message" data-message="@Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)"></div>
            @*[CC431] ADD End 20261013*@
            <div class="modal-footer">
                @* ボタンの色は Content\Site.css の「複数入力のボタン色」で指定 *@
                <button type="button" class="btn btn-default multi-input-cancel" data-dismiss="modal">@Message.getMessage(CommonLabel.BUTTON_CANCEL)</button>
                <button type="button" class="btn btn-primary multi-input-select">@Message.getMessage(CommonLabel.BUTTON_MULTI_INPUT_SELECT)</button>
            </div>
        </div>
    </div>
</div>
