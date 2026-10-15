@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of TempAssetListViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelTempAssetList.TITLE)
End Code
<script>
    //
    //  仮置資産一覧
    //
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            //エラーメッセージ表示後、ログイン画面に遷移
            return;
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        // 列定義
        let columnsTop = [
            { id: "@ColKey.TempAssetListTop.REL_CONST_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_REL_CONST_STATUS)", field: "REL_CONST_NO", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.REL_CONST_STATUS", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("REL_CONST_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("REL_CONST_STATUS").DispFlag", formatter: RelConstStatusFormatter },
            { id: "@ColKey.TempAssetListTop.REL_CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_REL_CONST_NO)", field: "REL_CONST_NO", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.REL_CONST_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("REL_CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("REL_CONST_NO").DispFlag" },
            { id: "@ColKey.TempAssetListTop.REG_CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_REG_CONST_NO)", field: "REG_CONST_NO", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.REG_CONST_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("REG_CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("REG_CONST_NO").DispFlag" },
            { id: "@ColKey.TempAssetListTop.TEMP_DT", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_DT)", field: "TEMP_DT", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.TempAssetListTop.CONST_CONTENTS", name: "@Message.getMessage(CommonLabel.LABEL_WORK_CONTENTS)", field: "CONST_CONTENTS", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.CONST_CONTENTS", 300), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONTENTS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONTENTS").DispFlag" },
            { id: "@ColKey.TempAssetListTop.TEMP_INFO1", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO1)", field: "TEMP_INFO1", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_INFO1", 200), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO1").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TempAssetListTop.TEMP_INFO2", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO2)", field: "TEMP_INFO2", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_INFO2", 200), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO2").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TempAssetListTop.TEMP_INFO3", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO3)", field: "TEMP_INFO3", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_INFO3", 200), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO3").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO3").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TempAssetListTop.TEMP_INFO4", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO4)", field: "TEMP_INFO4", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_INFO4", 200), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO4").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO4").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TempAssetListTop.TEMP_INFO5", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO5)", field: "TEMP_INFO5", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_INFO5", 200), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO5").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO5").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TempAssetListTop.TEMP_INFO6", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO6)", field: "TEMP_INFO6", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_INFO6", 200), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO6").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO6").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TempAssetListTop.TEMP_INFO7", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO7)", field: "TEMP_INFO7", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_INFO7", 200), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO7").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_INFO7").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TempAssetListTop.TEMP_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_REMARK)", field: "TEMP_REMARK", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_REMARK", 150), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_REMARK").DispFlag" },
            { id: "@ColKey.TempAssetListTop.TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_NUM)", field: "TEMP_NUM", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_TOP", "@ColKey.TemporaryList.TEMP_NUM", 150), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TEMP_NUM").DispFlag" },

        ]

        let columnsBottom = [
            { id: "@ColKey.TempAssetListBottom.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_BOTTOM", "@ColKey.TempAssetListBottom.ASSET_NO", 160), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NO").DispFlag" },
            { id: "@ColKey.TempAssetListBottom.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_DRUG_NAME_SHORT)", field: "ASSET_NAME", width: getColumnWidth("@DispId.TEMP_ASSET_LIST_BOTTOM", "@ColKey.TempAssetListBottom.ASSET_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NAME").DispFlag" },
        ]

        // Gridのオプション機能の設定
        let optionsTop = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: true,                       //Gridの高さの自動調整可否
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        let optionsBottom = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: true,                       //Gridの高さの自動調整可否
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonDataTop = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.tblTemporaryListData))) ');
        let jsonDataBottom = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // Gridのデータ管理オブジェクトにセット
        let dataViewTop = new Slick.Data.DataView();
        let dataViewBottom = new Slick.Data.DataView();

        // 固定列を設定
        let fixColTop = Number("@Model.FirstOrDefault.FixColCntTop");
        let fixColBottom = Number("@Model.FirstOrDefault.FixColCntBottom");

        // Grid表示判定
        if ("@ViewBag.tblTemporaryListData" != null & "@ViewBag.tblTemporaryListData.Count" > 0) {
            dataViewTop.beginUpdate();
            dataViewTop.setItems(jsonDataTop, "TEMP_ID");
            dataViewTop.endUpdate();
        }

        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataViewBottom.beginUpdate();
            dataViewBottom.setItems(jsonDataBottom, "ASSET_NO");
            dataViewBottom.endUpdate();
        }

        // Grid初期化
        gridTop = initializeGrid("ListGridsTop", dataViewTop, columnsTop, optionsTop, "@DispId.TEMP_ASSET_LIST_TOP", fixColTop);
        gridBottom = initializeGrid("ListGridsBottom", dataViewBottom, columnsBottom, optionsBottom, "@DispId.TEMP_ASSET_LIST_BOTTOM", fixColBottom);


    });

    // 仮置き紐付状態用formatter
    function RelConstStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (dataContext.REL_CONST_NO) {
            status = "@Message.getMessage(CommonLabel.VALUE_REL_CONSTRUCTION_STATUS_LINKED)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_REL_CONSTRUCTION_STATUS_UNLINKED)"
        }

        return status;
    }

    // 仮置き情報用formatter
    function TempInfoFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(value).replaceAll("$$$", "");
    }

    // yyyyMMdd→yyyy/MM/dd表示用formatter
    function SlashYMDFormatter(row, cell, value, columnDef, dataContext) {
        let ret = "";
        if (value) {
            ret = toStringDateYMD(value);
        }
        return ret

    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/TempAssetList")/CsvExport");
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;

        $('#form').submit();
    }
</script>

<div id="asset-detail">
    @Using (Html.BeginForm("Index", "TempAssetList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb10" style="width:100%">
            <div id="view-area-button-parent">
                <div class="view-area-button-right" style="margin-right:20px">
                    <input class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
                    <input class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "TemporaryList")'">
                </div>
            </div>
        </div>
        @<div>
            <div id="ListGridsTop" style="height:auto;padding-bottom: 20px;" />
        </div>
        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div class="list-view">
            <div id="ListGridsBottom" style="height:auto;padding-bottom: 20px;" />
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
        @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_TASK)
    End Using
</div>
