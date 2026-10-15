@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of InventoryLocHistoryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_TITLE)
End Code
<script>
    //
    //  棚卸実績ロケ別一覧
    //
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            // 2019/12/24 miyaji upd start エラーメッセージ表示後、ライセンスコントローラーを呼び出すように修正
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            //エラーメッセージ表示後、ログイン画面に遷移
            @*showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/Login")/Index");*@
            // 2019/12/24 miyaji upd end
            return;
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget");
        }

        // 列定義
        let columnsTop = [
            { id: "@ColKey.InventoryLocHistoryTop.START_DEC_TS", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_START_DEC_TS)', field: "START_DEC_TS", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_TOP", "@ColKey.InventoryLocHistoryTop.START_DEC_TS", 150), sortOrder: "@Model.FirstOrDefault.GridInfoTop("START_DEC_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("START_DEC_TS").DispFlag", formatter: StartDecTsFormatter, cssClass: "first-column" },
            { id: "@ColKey.InventoryLocHistoryTop.START_DEC_NAME", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_START_DEC_NAME)', field: "START_DEC_NAME", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_TOP", "@ColKey.InventoryLocHistoryTop.START_DEC_NAME", 150), sortOrder: "@Model.FirstOrDefault.GridInfoTop("START_DEC_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("START_DEC_NAME").DispFlag" },
            { id: "@ColKey.InventoryLocHistoryTop.INV_NAME", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_INV_NAME)', field: "INV_NAME", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_TOP", "@ColKey.InventoryLocHistoryTop.INV_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfoTop("INV_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("INV_NAME").DispFlag" },
            { id: "@ColKey.InventoryLocHistoryTop.INV_CONFIRM_TS", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_INV_CONFIRM_TS)', field: "INV_CONFIRM_TS", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_TOP", "@ColKey.InventoryLocHistoryTop.INV_CONFIRM_TS", 150), sortOrder: "@Model.FirstOrDefault.GridInfoTop("INV_CONFIRM_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("INV_CONFIRM_TS").DispFlag", formatter: InvConfirmTsFormatter },
            { id: "@ColKey.InventoryLocHistoryTop.INV_CONFIRM_NAME", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_INV_CONFIRM_NAME)', field: "INV_CONFIRM_NAME", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_TOP", "@ColKey.InventoryLocHistoryTop.INV_CONFIRM_NAME", 150), sortOrder: "@Model.FirstOrDefault.GridInfoTop("INV_CONFIRM_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("INV_CONFIRM_NAME").DispFlag" },
        ]

        let columnsBottom = [
            { id: "@ColKey.InventoryLocHistoryBottom.LOC_L_CD", name: '@Message.getMessage(CommonLabel.LABEL_INV_LOC_L_KUBUN)', field: "LOC_L_CD", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_BOTTOM", "@ColKey.InventoryLocHistoryBottom.LOC_L_CD", 300), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LOC_L_CD").DispFlag", formatter: LocCdNameFormatter, cssClass: "first-column" },
            { id: "@ColKey.InventoryLocHistoryBottom.BOOK_INVENTORY_NUMBER", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_BOOK_QUANTITY)', field: "BOOK_INVENTORY_NUMBER", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_BOTTOM", "@ColKey.InventoryLocHistoryBottom.BOOK_INVENTORY_NUMBER", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("BOOK_INVENTORY_NUMBER").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("BOOK_INVENTORY_NUMBER").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.InventoryLocHistoryBottom.NUMBER_OF_LOANS_WITHIN", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_NUMBER_OF_LOANS_WITHIN)', field: "NUMBER_OF_LOANS_WITHIN", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_BOTTOM", "@ColKey.InventoryLocHistoryBottom.NUMBER_OF_LOANS_WITHIN", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("NUMBER_OF_LOANS_WITHIN").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("NUMBER_OF_LOANS_WITHIN").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.InventoryLocHistoryBottom.ACTUAL_NUMBER_OF_SHELVES", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_ACTUAL_NUMBER_OF_SHELVES)', field: "ACTUAL_NUMBER_OF_SHELVES", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_BOTTOM", "@ColKey.InventoryLocHistoryBottom.ACTUAL_NUMBER_OF_SHELVES", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ACTUAL_NUMBER_OF_SHELVES").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ACTUAL_NUMBER_OF_SHELVES").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.InventoryLocHistoryBottom.NORMAL_NUMBER", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_NORMAL_NUMBER)', field: "NORMAL_NUMBER", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_BOTTOM", "@ColKey.InventoryLocHistoryBottom.NORMAL_NUMBER", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("NORMAL_NUMBER").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("NORMAL_NUMBER").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.InventoryLocHistoryBottom.LOCATION_DIFFERENCE_NUMBER", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_LOCATION_DIFFERENCE_NUMBER)', field: "LOCATION_DIFFERENCE_NUMBER", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_BOTTOM", "@ColKey.InventoryLocHistoryBottom.LOCATION_DIFFERENCE_NUMBER", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LOCATION_DIFFERENCE_NUMBER").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LOCATION_DIFFERENCE_NUMBER").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.InventoryLocHistoryBottom.OUT_OF_STOCK", name: '@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_OUT_OF_STOCK)', field: "OUT_OF_STOCK", width: getColumnWidth("@DispId.INVENTORY_LOC_HISTORY_BOTTOM", "@ColKey.InventoryLocHistoryBottom.OUT_OF_STOCK", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("OUT_OF_STOCK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("OUT_OF_STOCK").DispFlag", formatter: RightAlignFormatter },

        ]

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
        let jsonData_0 = [jsonData[0]]

        // 固定列を設定
        let fixColTop = Number("@Model.FirstOrDefault.FixColCntTop");
        let fixColBottom = Number("@Model.FirstOrDefault.FixColCntBottom");

        let data = []

        // Grid表示判定
        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            gridTop = initializeGrid("ListGridsTop", jsonData_0, columnsTop, options, "@DispId.INVENTORY_LOC_HISTORY_TOP", fixColTop);
            gridBottom = initializeGrid("ListGridsBottom", jsonData, columnsBottom, options, "@DispId.INVENTORY_LOC_HISTORY_BOTTOM", fixColBottom);
        } else {
            gridTop = initializeGrid("ListGridsTop", data, columnsTop, options, "@DispId.INVENTORY_LOC_HISTORY_TOP", fixColTop);
            gridBottom = initializeGrid("ListGridsBottom", data, columnsBottom, options, "@DispId.INVENTORY_LOC_HISTORY_BOTTOM", fixColBottom);
        }

    });

    //棚卸開始宣言日時用formatter 
    function StartDecTsFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value).toString()
        if (status != '') {
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            let hhmm = status.substr(8, 2) + ':' + status.substr(10, 2)
            return yyyy + '/' + mm + '/' + dd + ' ' + hhmm
        }
    }

    //棚卸確定日時用formatter  
    function InvConfirmTsFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value).toString()
        if (status != '') {
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            let hhmm = status.substr(8, 2) + ':' + status.substr(10, 2)
            return yyyy + '/' + mm + '/' + dd + ' ' + hhmm
        }
    }

    //ロケ大区分用formatter 
    function LocCdNameFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(value) + ' ' + convertToEmptyIfNull(dataContext.LOC_NAME)
    }

    // テキスト表示(右寄せ)
    function RightAlignFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return '<div style="text-align:right; width:100%;">' + value + '</div>';
        }
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/InventoryLocHistory")/CsvExport");
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;

        $('#form').submit();
    }
</script>

<div id="inventory-historyloc">
    @Using (Html.BeginForm("Index", "InventoryLocHistory", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    @* 実棚照会 *@
                    <input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_SHOW_INVENTORY_INQUIRY)" onclick="location.href='@Url.Action("Init", "InventoryAssetHistory")'">

                    @* CSV出力 *@
                    <input class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="return btnCsvExport_onclick()">
                    @*<input class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="location.href='@Url.Action("CsvExport", "InventoryLocHistory")'">*@

                    @* 戻る *@
                    <input class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "InventoryHistory")'">
                </div>
            </div>
        </div>

        @* 各数量表示 *@

        @<div class="search-wrap" style="height:50px;margin-bottom:30px">
            <div class="list-view">
                <div id="ListGridsTop" style="height:auto;padding-bottom: 20px;"></div>
            </div>

        </div>

        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)

        @<div class="list-view">
            <div id="ListGridsBottom" style="height:auto;padding-bottom: 20px;"></div>
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)

        @Html.HiddenFor(Function(model) model.FirstOrDefault.START_DEC_TS)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.START_DEC_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.START_DEC_NAME)

        @Html.HiddenFor(Function(model) model.FirstOrDefault.INV_CONFIRM_TS)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.INV_CONFIRM_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.INV_CONFIRM_NAME)

        @Html.HiddenFor(Function(model) model.FirstOrDefault.BOOK_INVENTORY_NUMBER)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.NUMBER_OF_LOANS_WITHIN)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.NORMAL_NUMBER)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.LOCATION_DIFFERENCE_NUMBER)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OUT_OF_STOCK)

        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_TASK)

        @Html.HiddenFor(Function(model) model.FirstOrDefault.SHOW_START_DEC_TS)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.SHOW_START_DEC_NAME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.SHOW_INV_NAME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.SHOW_INV_CONFIRM_TS)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.SHOW_INV_CONFIRM_NAME)

                                            End Using
</div>
