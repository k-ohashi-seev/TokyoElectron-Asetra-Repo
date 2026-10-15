@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of GatePassLogViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelGatePassLog.TITLE)
End Code
<script>
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        let columns;
        let dispId;

        if ("@Model.FirstOrDefault.GATE_DIV" == "@GateDiv.Cd.WG") {
            //表示画面区分設定
            dispId = "@DispId.GATE_PASS_LOG_WAVE_GATE"
            //列定義
            columns = [
                { id: "@ColKey.GatePassLog.GATE_NAME", name: '@Message.getMessage(CommonLabel.LABEL_PASSED_GATE)', field: "GATE_NAME", width: getColumnWidth("@DispId.GATE_PASS_LOG_WAVE_GATE", "@ColKey.GatePassLog.GATE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_GATE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_GATE_NAME").DispFlag" },
                { id: "@ColKey.GatePassLog.GATE_PASS_DT", name: '@Message.getMessage(CommonLabel.LABEL_GATE_PASS_DTTM)', field: "GATE_PASS_DT", width: getColumnWidth("@DispId.GATE_PASS_LOG_WAVE_GATE", "@ColKey.GatePassLog.GATE_PASS_DT", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_GATE_PASS_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_GATE_PASS_DT").DispFlag", formatter: GatePassDtFormatter },
                { id: "@ColKey.GatePassLog.GATE_DATA", name: '@Message.getMessage(CommonLabel.LABEL_GATE_DATA)', field: "GATE_DATA", width: getColumnWidth("@DispId.GATE_PASS_LOG_WAVE_GATE", "@ColKey.GatePassLog.GATE_DATA", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_GATE_DATA").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_GATE_DATA").DispFlag", formatter: GateDataFormatter },
                { id: "@ColKey.GatePassLog.ASSET_NO", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_NO)', field: "ASSET_NO", width: getColumnWidth("@DispId.GATE_PASS_LOG_WAVE_GATE", "@ColKey.GatePassLog.ASSET_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_ASSET_NO").DispFlag" },
                { id: "@ColKey.GatePassLog.ASSET_NAME", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)', field: "ASSET_NAME", width: getColumnWidth("@DispId.GATE_PASS_LOG_WAVE_GATE", "@ColKey.GatePassLog.ASSET_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_ASSET_NAME").DispFlag" },
                { id: "@ColKey.GatePassLog.MAKER", name: '@Message.getMessage(CommonLabel.LABEL_MAKER)', field: "MAKER", width: getColumnWidth("@DispId.GATE_PASS_LOG_WAVE_GATE", "@ColKey.GatePassLog.MAKER", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_MAKER").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_MAKER").DispFlag" },
                { id: "@ColKey.GatePassLog.PRODUCT_NO", name: '@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)', field: "PRODUCT_NO", width: getColumnWidth("@DispId.GATE_PASS_LOG_WAVE_GATE", "@ColKey.GatePassLog.PRODUCT_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_PRODUCT_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_PRODUCT_NO").DispFlag" },
                { id: "@ColKey.GatePassLog.MODEL_NO", name: '@Message.getMessage(CommonLabel.LABEL_MODEL_NO)', field: "MODEL_NO", width: getColumnWidth("@DispId.GATE_PASS_LOG_WAVE_GATE", "@ColKey.GatePassLog.MODEL_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_MODEL_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_MODEL_NO").DispFlag" },
                { id: "@ColKey.GatePassLog.GATE_REMARK", name: '@Message.getMessage(CommonLabel.GATE_REMARK)', field: "ERR_CONTENTS", width: getColumnWidth("@DispId.GATE_PASS_LOG_WAVE_GATE", "@ColKey.GatePassLog.GATE_REMARK", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_GATE_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_GATE_REMARK").DispFlag" },
            ]

        } else {
            //表示画面区分設定
            dispId = "@DispId.GATE_PASS_LOG_SPEED_WAY"
            //列定義
            columns = [
                { id: "@ColKey.GatePassLog.GATE_NAME", name: '@Message.getMessage(CommonLabel.LABEL_PASSED_GATE)', field: "GATE_NAME", width: getColumnWidth("@DispId.GATE_PASS_LOG_SPEED_WAY", "@ColKey.GatePassLog.GATE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_GATE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_GATE_NAME").DispFlag" },
                { id: "@ColKey.GatePassLog.GATE_PASS_DT", name: '@Message.getMessage(CommonLabel.LABEL_GATE_PASS_DTTM)', field: "GATE_PASS_DT", width: getColumnWidth("@DispId.GATE_PASS_LOG_SPEED_WAY", "@ColKey.GatePassLog.GATE_PASS_DT", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_GATE_PASS_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_GATE_PASS_DT").DispFlag", formatter: GatePassDtFormatter },
                { id: "@ColKey.GatePassLog.GATE_DATA", name: '@Message.getMessage(CommonLabel.LABEL_GATE_DATA)', field: "GATE_DATA", width: getColumnWidth("@DispId.GATE_PASS_LOG_SPEED_WAY", "@ColKey.GatePassLog.GATE_DATA", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_GATE_DATA").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_GATE_DATA").DispFlag", formatter: GateDataFormatter },
                { id: "@ColKey.GatePassLog.ASSET_NO", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_NO)', field: "ASSET_NO", width: getColumnWidth("@DispId.GATE_PASS_LOG_SPEED_WAY", "@ColKey.GatePassLog.ASSET_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_ASSET_NO").DispFlag" },
                { id: "@ColKey.GatePassLog.ASSET_NAME", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)', field: "ASSET_NAME", width: getColumnWidth("@DispId.GATE_PASS_LOG_SPEED_WAY", "@ColKey.GatePassLog.ASSET_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_ASSET_NAME").DispFlag" },
                { id: "@ColKey.GatePassLog.GATE_REMARK", name: '@Message.getMessage(CommonLabel.GATE_REMARK)', field: "ERR_CONTENTS", width: getColumnWidth("@DispId.GATE_PASS_LOG_SPEED_WAY", "@ColKey.GatePassLog.GATE_REMARK", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_GATE_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_GATE_REMARK").DispFlag" },
            ]
        }

        // Gridのオプション機能の設定
        let options = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: true,                       //Gridの高さの自動調整可否
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol       //選択列を加算
        }

        let data = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, dispId, fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, dispId, fixCol);
        }

    });

    //通過日時用formatter GATE_PASS_DT
    function GatePassDtFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(dataContext.GATE_PASS_TM) != '' && convertToEmptyIfNull(value)) {
            let hh = dataContext.GATE_PASS_TM.substr(0, 2)
            let mm = dataContext.GATE_PASS_TM.substr(2, 2)
            return toStringDateYMD(value) + ' ' + hh + ':' + mm;
        }
    }

    //データ用formatter
    function GateDataFormatter(row, cell, value, columnDef, dataContext) {
        let status;
        let color;

        switch (dataContext.GATE_PASS_DIV) {
            case '@GatePassDiv.Cd.ER':
                status = "<span style='color: red;'>" + '@Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_ER)' + "</span>";
                break;
            case '@GatePassDiv.Cd.IO':
                status = '@Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_IO)';
                break;
            case '@GatePassDiv.Cd.LR':
                status = '@Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_LR)';
                break;
            case '@GatePassDiv.Cd.SE':
                status = "<span style='color: red;'>" + '@Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_SE)' + "</span>";
                break;
        }
        return status
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        // 通過日範囲チェック
        var arrGatePassDtFrom = document.getElementById("SearchGatePassDtFrom").value.split("/");
        var arrGatePassDtTo = document.getElementById("SearchGatePassDtTo").value.split("/");
        if (arrGatePassDtFrom != "" && arrGatePassDtTo != "") {
            // 両方とも空白でない時
            // 期間前後チェック
            if (arrGatePassDtFrom > arrGatePassDtTo) {
                showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")
                return false;
            }
        }

        // データの選択状態をチェック
        //20220415 UPD Start Kawamoto v2.1
        if ("@Model.FirstOrDefault.GATE_DIV" == "@GateDiv.Cd.WG") {
            //if ("@Model.FirstOrDefault.GATE_PASS_DIV" == "@GateDiv.Cd.WG") {
            //20220415 UPD End Kawamoto v2.1
            if (!document.getElementById("SearchGatePassDivER").checked
                && !document.getElementById("SearchGatePassDivIO").checked
                && !document.getElementById("SearchGatePassDivSE").checked      //20220328 ADD Kawamoto v2.1
                && !document.getElementById("SearchGatePassDivLR").checked) {
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_DATA_UNSELECTED)")
                return false;
            }
        } else {
            //20220328 ADD Start Kawamoto v2.1
            if (!document.getElementById("SearchGatePassDivER").checked
                && !document.getElementById("SearchGatePassDivSE").checked
                && !document.getElementById("SearchGatePassDivLR").checked) {
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_DATA_UNSELECTED)")
                return false;
            }
            //20220328 ADD End Kawamoto v2.1
        }

        //ページ番号のリセット
        ResetPageNum();
        return true;
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/GatePassLog")/CsvExport");

    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

</script>

<div id="gate-pass-log">
    @Using (Html.BeginForm("Index", "GatePassLog", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    <input class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
                </div>
            </div>
        </div>
        @* 検索条件 *@
        @<div Class="search-wrap">
             <a id="SearchCondition" data-toggle="collapse" href="#cond" aria-expanded="true" onclick="SearchCondition_onclick()">
                 @Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)
             </a>
            <div class="search-border">
                <div id="cond" class="container collapse in">
                    <div class="row" id="item-conditions">
                        <div class="col-md-12">
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("WG_GATE_NAME").DispFlag = "1" AndAlso Model.FirstOrDefault.GATE_DIV = GateDiv.Cd.WG OrElse Model.FirstOrDefault.GridInfo("SW_GATE_NAME").DispFlag = "1" AndAlso Model.FirstOrDefault.GATE_DIV = GateDiv.Cd.SW, "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_GATE)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCode, DirectCast(ViewBag.gate, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>

                            @* 資産番号 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("WG_ASSET_NO").DispFlag = "1" AndAlso Model.FirstOrDefault.GATE_DIV = GateDiv.Cd.WG OrElse Model.FirstOrDefault.GridInfo("SW_ASSET_NO").DispFlag = "1" AndAlso Model.FirstOrDefault.GATE_DIV = GateDiv.Cd.SW, "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetNo, New With {.class = "cond-control", .maxlength = 24, .oninput = "editAssetNo(this)"})
                                </div>
                            </div>

                            @* 資産名称 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("WG_ASSET_NAME").DispFlag = "1" AndAlso Model.FirstOrDefault.GATE_DIV = GateDiv.Cd.WG OrElse Model.FirstOrDefault.GridInfo("SW_ASSET_NAME").DispFlag = "1" AndAlso Model.FirstOrDefault.GATE_DIV = GateDiv.Cd.SW, "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>

                            @* 通過日 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_GATE_PASS_DT)</div>
                                <div class="grid-condition-col-common">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchGatePassDtFrom, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                    @Html.Raw("～")
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchGatePassDtTo, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                </div>
                            </div>

                            @* データ *@
                            @If Model.FirstOrDefault.GATE_DIV = GateDiv.Cd.WG Then
                                @<div Class="col-sm-4" style="max-width:600px;">
                                    <div Class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_GATE_DATA)</div>
                                    <div Class="grid-condition-col">
                                        <div Class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchGatePassDivLR) @Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_LR)</div>
                                        <div Class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchGatePassDivER) @Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_ER)</div>
                                        <div Class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchGatePassDivIO) @Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_IO)</div>
                                        <div Class="input-select-item-last">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchGatePassDivSE) @Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_SE)</div>
                                    </div>
                                </div>
                            Else
                                @<div Class="col-sm-3" style="max-width:450px;">
                                    <div Class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_GATE_DATA)</div>
                                    <div Class="grid-condition-col">
                                        <div Class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchGatePassDivLR) @Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_LR)</div>
                                        <div Class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchGatePassDivER) @Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_ER)</div>
                                        <div Class="input-select-item-last">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchGatePassDivSE) @Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_SE)</div>
                                    </div>
                                </div>
End If

                        </div>
                    </div>
                    <div class="view-area-label" style="height: 25px"></div>
                    <input class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>
        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div class="list-view">
            <div id="ListGrids" style="height:auto;padding-bottom: 20px;" />
        </div>

                                        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
                                                @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
                                                @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
                                                @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
                                                @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
                                                @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
                                                @Html.HiddenFor(Function(model) model.Last.UPD_TASK)
                                                @Html.HiddenFor(Function(model) model.FirstOrDefault.GATE_DIV)
                                         End Using
</div>
