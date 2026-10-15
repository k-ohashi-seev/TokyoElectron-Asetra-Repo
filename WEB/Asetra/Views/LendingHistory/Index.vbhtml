@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of LendingHistoryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HISTORY)
End Code
<script>
    //
    //予約履歴画面
    //
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            //エラーメッセージ表示後、ライセンスコントローラーを呼び出す
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessCreate") {
            //登録
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessEdit") {
            //変更
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessDelete") {
            //削除
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }


        // 列定義
        let columnsTop = [
            { id: "@ColKey.LendingHistoryTop.RESERVE_ID", name: "@Message.getMessage(CommonLabel.LABEL_RESERVE_NO)", field: "RESERVE_ID", width: getColumnWidth("@DispId.LENDING_HISTORY_TOP", "@ColKey.LendingHistoryTop.RESERVE_ID", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("RESERVE_ID").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("RESERVE_ID").DispFlag" },
            { id: "@ColKey.LendingHistoryTop.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_MNG_LOC)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.LENDING_HISTORY_TOP", "@ColKey.LendingHistoryTop.MNG_LOC_L_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfoTop("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("MNG_LOC_L_CD").DispFlag", formatter: MngLocCdFormatter },
            { id: "@ColKey.LendingHistoryTop.REQ_EMP_CD", name: "@Message.getMessage(CommonLabel.LABEL_REQ_EMP)", field: "REQ_EMP_CD", width: getColumnWidth("@DispId.LENDING_HISTORY_TOP", "@ColKey.LendingHistoryTop.REQ_EMP_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfoTop("REQ_EMP_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("REQ_EMP_CD").DispFlag", formatter: ReqEmpCdFormatter },

        ]

        let columnsBottom = [
            { id: "@ColKey.LendingHistoryBottom.PROCESS_DT", name: "@Message.getMessage(CommonLabel.LABEL_TASK_TS)", field: "PROCESS_DT", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.PROCESS_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_DT").DispFlag", formatter: ProcessDtFormatter },
            { id: "@ColKey.LendingHistoryBottom.PROCESS_DIV", name: "@Message.getMessage(CommonLabel.LABEL_PROCESS_DIV)", field: "PROCESS_DIV", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.PROCESS_DIV", 110), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_DIV").DispFlag", formatter: ProcessDivFormatter },
            { id: "@ColKey.LendingHistoryBottom.PROCESS_TOOL", name: "@Message.getMessage(CommonLabel.LABEL_PROCESS_CLASS)", field: "PROCESS_TOOL_DIV", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.PROCESS_TOOL", 110), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_TOOL").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_TOOL").DispFlag", formatter: ProcessToolDivFormatter },
            { id: "@ColKey.LendingHistoryBottom.CHARGE_EMP_CD", name: "@Message.getMessage(CommonLabel.LABEL_EMP)", field: "CHARGE_EMP_CD", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.CHARGE_EMP_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CHARGE_EMP_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CHARGE_EMP_CD").DispFlag", formatter: ChargeEmpCdFormatter },
            { id: "@ColKey.LendingHistoryBottom.REAL_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE)", field: "REAL_START_DT", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.REAL_START_DT", 110), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REAL_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REAL_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingHistoryBottom.REAL_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE)", field: "REAL_END_DT", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.REAL_END_DT", 110), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REAL_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REAL_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingHistoryBottom.USER_START_DT", name: "@Message.getMessage(CommonLabel.USER_START_DT)", field: "USER_START_DT", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.USER_START_DT", 110), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("USER_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("USER_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingHistoryBottom.USER_END_DT", name: "@Message.getMessage(CommonLabel.USER_END_DT)", field: "USER_END_DT", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.USER_END_DT", 110), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("USER_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("USER_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingHistoryBottom.LEASE_PLACE_CD", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_PLACE)", field: "LEASE_PLACE_CD", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.LEASE_PLACE_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LEASE_PLACE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LEASE_PLACE_CD").DispFlag", formatter: LeasePlaceCdFormatter },
            { id: "@ColKey.LendingHistoryBottom.LEASE_USE", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO)", field: "LEASE_USE", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.LEASE_USE", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LEASE_USE").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LEASE_USE").DispFlag" },
            { id: "@ColKey.LendingHistoryBottom.DEST_NAME", name: "@Message.getMessage(CommonLabel.LABEL_SEND_NAME)", field: "DEST_NAME", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.DEST_NAME", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("DEST_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("DEST_NAME").DispFlag" },
            { id: "@ColKey.LendingHistoryBottom.DEST_ADDRESS", name: "@Message.getMessage(CommonLabel.LABEL_SEND_ADDRESS)", field: "DEST_ADDRESS", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.DEST_ADDRESS", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("DEST_ADDRESS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("DEST_ADDRESS").DispFlag" },
            { id: "@ColKey.LendingHistoryBottom.REMARK", name: "@Message.getMessage(CommonLabel.LABEL_SEND_BIKO)", field: "REMARK", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.REMARK", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK").DispFlag" },
            { id: "@ColKey.LendingHistoryBottom.REMARK_INTERNAL", name: "@Message.getMessage(CommonLabel.LABEL_SHANAI_BIKO)", field: "REMARK_INTERNAL", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.REMARK_INTERNAL", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK_INTERNAL").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK_INTERNAL").DispFlag" },
            { id: "@ColKey.LendingHistoryBottom.DEST_NO", name: "@Message.getMessage(CommonLabel.LABEL_DEST_NO)", field: "DEST_NO", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.DEST_NO", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("DEST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("DEST_NO").DispFlag" },
            { id: "@ColKey.LendingHistoryBottom.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.ASSET_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NO").DispFlag" },
            { id: "@ColKey.LendingHistoryBottom.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)", field: "ASSET_NAME", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.ASSET_NAME", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NAME").DispFlag" },
            { id: "@ColKey.LendingHistoryBottom.SERIAL_NO", name: "@Message.getMessage(CommonLabel.LABEL_SERIAL_NO)", field: "SERIAL_NO", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.SERIAL_NO", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("SERIAL_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("SERIAL_NO").DispFlag" },
            { id: "@ColKey.LendingHistoryBottom.DTL_REAL_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_REAL_START_DT)", field: "DTL_REAL_START_DT", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.DTL_REAL_START_DT", 140), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("DTL_REAL_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("DTL_REAL_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingHistoryBottom.DTL_REAL_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_REAL_END_DT)", field: "DTL_REAL_END_DT", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.DTL_REAL_END_DT", 140), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("DTL_REAL_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("DTL_REAL_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingHistoryBottom.DTL_LENDING_DT", name: "@Message.getMessage(CommonLabel.LABEL_REAL_LENDING_DT)", field: "DTL_LENDING_DT", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.DTL_LENDING_DT", 90), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("DTL_LENDING_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("DTL_LENDING_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingHistoryBottom.DTL_RETURN_DT", name: "@Message.getMessage(CommonLabel.LABEL_REAL_RETURN_DT)", field: "DTL_RETURN_DT", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.DTL_RETURN_DT", 90), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("DTL_RETURN_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("DTL_RETURN_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingHistoryBottom.DTL_CHARGE_EMP_CD", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_EMPLOYEE)", field: "DTL_CHARGE_EMP_CD", width: getColumnWidth("@DispId.LENDING_HISTORY_BOTTOM", "@ColKey.LendingHistoryBottom.DTL_CHARGE_EMP_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("DTL_CHARGE_EMP_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("DTL_CHARGE_EMP_CD").DispFlag", formatter: DtlChargeEmpCdFormatter },

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
        let jsonDataTop = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(New List(Of LendingHistoryViewModel) From {Model(0)}))) ');
        let jsonDataBottom = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // Gridのデータ管理オブジェクトにセット
        let dataViewTop = new Slick.Data.DataView();
        let dataViewBottom = new Slick.Data.DataView();

        // 固定列を設定
        let fixColTop = Number("@Model.FirstOrDefault.FixColCntTop");
        let fixColBottom = Number("@Model.FirstOrDefault.FixColCntBottom");

        // Grid表示判定
        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataViewTop.beginUpdate();
            dataViewTop.setItems(jsonDataTop, "RESERVE_LOG_ID");
            dataViewTop.endUpdate();

            dataViewBottom.beginUpdate();
            dataViewBottom.setItems(jsonDataBottom, "RESERVE_LOG_ID");
            dataViewBottom.endUpdate();
        }

        // Grid初期化
        gridTop = initializeGrid("ListGridsTop", dataViewTop, columnsTop, optionsTop, "@DispId.LENDING_HISTORY_TOP", fixColTop);
        gridBottom = initializeGrid("ListGridsBottom", dataViewBottom, columnsBottom, optionsBottom, "@DispId.LENDING_HISTORY_BOTTOM", fixColBottom);

    });

    // 管理ロケーションformatter
    function MngLocCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.MNG_LOC_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.MNG_LOC_L_NAME)
    }

    // 申請者formatter
    function ReqEmpCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.REQ_EMP_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.REQ_EMP_NAME)
    }

    // 処理日時formatter
    function ProcessDtFormatter(row, cell, value, columnDef, dataContext) {
        let processDt = toStringDateYMD(String(dataContext.PROCESS_DT)) + ' ' +
                        String(dataContext.PROCESS_TM.substring(0, 2) + ':' +
                        dataContext.PROCESS_TM.substring(2, 4));
        return toFormatDate(processDt, "yyyy/MM/dd HH:mm");
    }

    // 処理区分formatter
    function ProcessDivFormatter(row, cell, value, columnDef, dataContext) {
        let div = "";

        switch (String(dataContext.PROCESS_DIV)) {
            case "@ProcessDiv.MI":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MI)"
                break;
            case "@ProcessDiv.CI":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CI)"
                break;
            case "@ProcessDiv.MU":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MU)"
                break;
            case "@ProcessDiv.CU":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CU)"
                break;
            case "@ProcessDiv.MR":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MR)"
                break;
            case "@ProcessDiv.CR":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CR)"
                break;
            case "@ProcessDiv.MD":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MD)"
                break;
            case "@ProcessDiv.T1":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_T1)"
                break;
            case "@ProcessDiv.T2":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_T2)"
                break;
            case "@ProcessDiv.BS":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_BS)"
                break;
            case "@ProcessDiv.BO":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_BO)"
                break;
            case "@ProcessDiv.BI":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_BI)"
                break;
            case "@ProcessDiv.BR":
                div = "@Message.getMessage(CommonLabel.VALUE_RETIRE)"
                break;
            case "@ProcessDiv.LC":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LC)"
                break;
            case "@ProcessDiv.CD":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CD)"
                break;
            default:
                div = "　";
                break;
        }

        return div;
    }

    // 処理ツール区分formatter
    function ProcessToolDivFormatter(row, cell, value, columnDef, dataContext) {
        let div = "";

        switch (String(dataContext.PROCESS_TOOL_DIV)) {
            case "@ProcessToolDiv.Web":
                div = "@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)"
                break;
            case "@ProcessToolDiv.CS":
                div = "@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS)"
                break;
            case "@ProcessToolDiv.HT":
                div = "@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)"
                break;
            default:
                div = dataContext.PROCESS_TOOL_DIV;
                break;
        }

        return div;
    }

    // 担当者formatter
    function ChargeEmpCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CHARGE_EMP_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CHARGE_EMP_NAME)
    }

    // yyyyMMdd→yyyy/MM/dd表示用formatter
    function SlashYMDFormatter(row, cell, value, columnDef, dataContext) {
        return toStringDateYMD(value);
    }

    // 貸出先formatter
    function LeasePlaceCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LEASE_PLACE_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LEASE_PLACE_NAME)
    }

    // 資産担当者formatter
    function DtlChargeEmpCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.DTL_CHARGE_EMP_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.DTL_CHARGE_EMP_NAME)
    }


    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/LendingHistory")/CsvExport");
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;
        $('#form').submit();
    }

</script>

<div id="lending-history">
    @Using (Html.BeginForm("Index", "LendingHistory", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    <input class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
                    <input class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "LendingEntry")'">
                </div>
            </div>
        </div>

        @<div class="list-view">
            <div id="ListGridsTop" style="height:auto;padding-bottom: 20px;"></div>
            <div id="ListGridsBottom" style="height:auto;padding-bottom: 20px;"></div>
        </div>


                @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
                        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
                        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
                        @*@Html.HiddenFor(Function(model) model.Last.INS_DATETIME)
                        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_CD)
                        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_NAME)
                        @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
                        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
                        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
                        @Html.HiddenFor(Function(model) model.Last.UPD_TASK)*@
    End Using
</div>
