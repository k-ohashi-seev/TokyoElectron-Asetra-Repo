@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType DrugSpecListViewModel
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelDrugSpecList.TITLE)
End Code

<script>
    //
    //指定数量管理表出力画面
    //
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.ErrCode" != "" && "@Model.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            //エラーメッセージ表示後、ライセンスコントローラーを呼び出す
            showLoginMessage("@Message.getMessage(Model.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.OnCreteButton"

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.ErrCode)", "#@Model.ErrTarget")
        }
    });

    function btnReportOutput_onclick() {
        //◆期限
        var searchDateFrom = document.getElementById("SearchDateFrom").value;
        var searchDateTo = document.getElementById("SearchDateTo").value;

        //未入力チェック
        if (searchDateFrom == "" && searchDateTo == "") {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_ENTERED_PERIOD_FROM_OR_TO)")
            return false
        }

        // 期間前後チェック
        if (searchDateFrom != "" && searchDateTo != "") {
            // 両方とも空白でない時
            if (searchDateFrom > searchDateTo) {
                showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")
                return false;
            }
        }

        //◆保管場所
        var storageLoc = document.getElementById("SelectStorageLoc").value;

        //未入力チェック
        if (storageLoc == "") {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_STORAGE_LOC)")
            return false
        }

        return showConfirm("@Message.getMessage(CommonInfoMsg.REPORT_OUTPUT_COFIRM)", "@Url.Content("~/DrugSpecList")/ReportOutput");
    }

</script>

<div id="specifed-qty-list-output">
@Using (Html.BeginForm("Index", "DrugSpecList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
    @Html.AntiForgeryToken()
    @* 画面上部のボタン *@
    @<div class="view-area mb20">
        <div id="view-area-button-parent">
            <div class="view-area-button-right">
                <input class="defButton submit-button" type="button" value="@Message.getMessage(CommonLabel.BUTTON_REPORT_OUTPUT)" onclick="btnReportOutput_onclick()">
            </div>
        </div>
    </div>
    @* 出力条件 *@
    @<div Class="search-wrap">
        <p class="search-title"> @Message.getMessage(CommonLabel.TITLE_OUTPUT_CONDITIONS) </p>
         <div class="search-border">
             <div id="cond" class="container collapse in">
                 <div class="row" id="item-conditions">
                     <div class="col-md-12">
                         <div class="col-sm-3">
                             <div class="grid-head-col"><div width="20" class="require" style="display:inline-block;">※</div>@Message.getMessage(CommonLabel.LABEL_PERIOD)</div>
                             <div class="grid-condition-col-common">
                                 @Html.EditorFor(Function(model) model.SearchDateFrom, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                 @Html.Raw("～")
                                 @Html.EditorFor(Function(model) model.SearchDateTo, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                             </div>
                         </div>

                         @* ロケーション 保管場所 *@
                         <div class="col-sm-2">
                             <div class="grid-head-col"><span width="20" class="require" style="display:inline-block;">※</span>@(Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) + " " + Message.getMessage(CommonLabel.LABEL_STORAGE_LOCATION))</div>
                             <div Class="grid-condition-col">
                                 @Html.DropDownListFor(Function(model) model.SelectStorageLoc, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                             </div>
                         </div>
                     </div>
                 </div>
             </div>


                     </div>
                 </div>

                 @Html.HiddenFor(Function(model) model.OnCreteButton)
                 End Using


             </div>
