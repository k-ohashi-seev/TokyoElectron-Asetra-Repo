@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of AssetDetailViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelAssetHistory.TITLE)
End Code
<script>
    //
    //  資産履歴照会
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
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        // 列定義
        let columnsTop = [
            { id: "@ColKey.AssetDetailTop.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.ASSET_NO", 160), sortOrder: "@Model.FirstOrDefault.GridInfoTop("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("ASSET_NO").DispFlag" },
            { id: "@ColKey.AssetDetailTop.SUB_ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_SUB_ASSET_NO)", field: "SUB_ASSET_NO", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.SUB_ASSET_NO", 160), sortOrder: "@Model.FirstOrDefault.GridInfoTop("SUB_ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("SUB_ASSET_NO").DispFlag" },
            { id: "@ColKey.AssetDetailTop.ASSET_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_STATUS)", field: "ASSET_STATUS", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.ASSET_STATUS", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("ASSET_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("ASSET_STATUS").DispFlag", formatter: AssetStatusFormatter },
            { id: "@ColKey.AssetDetailTop.ASSET_ID_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_ID_STATUS)", field: "EPC_ENC_STATUS", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.ASSET_ID_STATUS", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("EPC_ENC_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("EPC_ENC_STATUS").DispFlag", formatter: EpcEncStatusFormatter },
            { id: "@ColKey.AssetDetailTop.TAG_LINK_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_TAG_LINK_STATUS)", field: "TAG_LINK_STATUS", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.TAG_LINK_STATUS", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("TAG_LINK_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("TAG_LINK_STATUS").DispFlag", formatter: TagLinkStatusFormatter },
            { id: "@ColKey.AssetDetailTop.LEASE_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)", field: "LEASE_STATUS", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.LEASE_STATUS", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("LEASE_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("LEASE_STATUS").DispFlag", formatter: LeaseStatusFormatter },
            { id: "@ColKey.AssetDetailTop.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)", field: "ASSET_NAME", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.ASSET_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfoTop("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("ASSET_NAME").DispFlag" },
            { id: "@ColKey.AssetDetailTop.CAT_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)", field: "CAT_L_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.CAT_L_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CAT_L_CD").DispFlag", formatter: CatLCdFormatter },
            { id: "@ColKey.AssetDetailTop.CAT_M_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_M_CD_NAME)", field: "CAT_M_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.CAT_M_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CAT_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CAT_M_CD").DispFlag", formatter: CatMCdFormatter },
            { id: "@ColKey.AssetDetailTop.CAT_S_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_S_CD_NAME)", field: "CAT_S_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.CAT_S_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CAT_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CAT_S_CD").DispFlag", formatter: CatSCdFormatter },
            { id: "@ColKey.AssetDetailTop.MAKER", name: "@Message.getMessage(CommonLabel.LABEL_MAKER)", field: "MAKER", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.MAKER", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("MAKER").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("MAKER").DispFlag" },
            { id: "@ColKey.AssetDetailTop.PRODUCT_NO", name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)", field: "PRODUCT_NO", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.PRODUCT_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("PRODUCT_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("PRODUCT_NO").DispFlag" },
            { id: "@ColKey.AssetDetailTop.MODEL_NO", name: "@Message.getMessage(CommonLabel.LABEL_MODEL_NO)", field: "MODEL_NO", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.MODEL_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("MODEL_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("MODEL_NO").DispFlag" },
            { id: "@ColKey.AssetDetailTop.LEASE_CNT", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_COUNT)", field: "LEASE_CNT", width: getColumnWidth("@DispId.ASSET_DETAIL_TOP", "@ColKey.AssetDetailTop.LEASE_CNT", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("LEASE_CNT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("LEASE_CNT").DispFlag", formatter: LeaseCntFormatter },

        ]

        let columnsBottom = [
            { id: "@ColKey.AssetDetailBottom.PROCESS_DT", name: "@Message.getMessage(CommonLabel.LABEL_TASK_TS)", field: "PROCESS_DT", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.PROCESS_DT", 140), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_DT").DispFlag", formatter: ProcessDtFormatter },
            { id: "@ColKey.AssetDetailBottom.PROCESS_DIV", name: "@Message.getMessage(CommonLabel.LABEL_PROCESS_DIV)", field: "PROCESS_DIV", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.PROCESS_DIV", 150), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_DIV").DispFlag", formatter: ProcessDivFormatter },
            { id: "@ColKey.AssetDetailBottom.PROCESS_TOOL", name: "@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL)", field: "PROCESS_TOOL", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.PROCESS_TOOL", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_TOOL").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("PROCESS_TOOL").DispFlag", formatter: ProcessToolDivFormatter },
            { id: "@ColKey.AssetDetailBottom.LEASE_PLACE_CD", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD_NAME)", field: "LEASE_PLACE_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.LEASE_PLACE_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LEASE_PLACE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LEASE_PLACE_CD").DispFlag", formatter: LeasePlaceCdFormatter },
            { id: "@ColKey.AssetDetailBottom.REMARK", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_REMARK)", field: "REMARK", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK", 140), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.LEASE_EMPLOYEE_CD", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_EMPLOYEE_CD_NAME)", field: "LEASE_EMPLOYEE_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.LEASE_EMPLOYEE_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LEASE_EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LEASE_EMPLOYEE_CD").DispFlag", formatter: LeaseEmployeeCdFormatter },
            { id: "@ColKey.AssetDetailBottom.LEASE_KIKAN", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_KIKAN)", field: "LEASE_DT", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.LEASE_KIKAN", 180), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LEASE_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LEASE_DT").DispFlag", formatter: LeaseKikanFormatter },
            @*{ id: "@ColKey.AssetDetailBottom.WEIGHT_VAL", name: "@Message.getMessage(CommonLabel.LABEL_WEIGHT)", field: "WEIGHT_VAL", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.WEIGHT_VAL", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("WEIGHT_VAL").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("WEIGHT_VAL").DispFlag", formatter: WeightValFormatter},
            { id: "@ColKey.AssetDetailBottom.WORKER_CD", name: "@Message.getMessage(CommonLabel.LABEL_WORKER_CD_NAME)", field: "WORKER_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.WORKER_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("WORKER_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("WORKER_CD").DispFlag", formatter: WorkerCdFormatter },
            { id: "@ColKey.AssetDetailBottom.WORKING_HOURS", name: "@Message.getMessage(CommonLabel.LABEL_WORKING_HOURS)", field: "WORKING_HOURS", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.WORKING_HOURS", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("WORKING_HOURS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("WORKING_HOURS").DispFlag"},
            { id: "@ColKey.AssetDetailBottom.WORK_SUMMARY", name: "@Message.getMessage(CommonLabel.LABEL_WORK_SUMMARY)", field: "WORK_SUMMARY", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.WORK_SUMMARY", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("WORK_SUMMARY").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("WORK_SUMMARY").DispFlag"},*@
            { id: "@ColKey.AssetDetailBottom.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_MNG_LOC_L_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.MNG_LOC_L_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("MNG_LOC_L_CD").DispFlag", formatter: MngLocCdFormatter },
            { id: "@ColKey.AssetDetailBottom.LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_L_CD_NAME)", field: "LOC_L_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.LOC_L_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LOC_L_CD").DispFlag", formatter: LocLCdFormatter },
            { id: "@ColKey.AssetDetailBottom.LOC_M_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_M_CD_NAME)", field: "LOC_M_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.LOC_M_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LOC_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LOC_M_CD").DispFlag", formatter: LocMCdFormatter },
            { id: "@ColKey.AssetDetailBottom.LOC_S_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_S_CD_NAME)", field: "LOC_S_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.LOC_S_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LOC_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LOC_S_CD").DispFlag", formatter: LocSCdFormatter },
            { id: "@ColKey.AssetDetailBottom.ASSET_ID", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_ID)", field: "ASSET_ID", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.ASSET_ID", 480), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ASSET_ID").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ASSET_ID").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.UPD_EMPLOYEE_CD", name: "@Message.getMessage(CommonLabel.LABEL_EMPLOYEE_CD_NAME)", field: "UPD_EMPLOYEE_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.UPD_EMPLOYEE_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("UPD_EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("UPD_EMPLOYEE_CD").DispFlag", formatter: UpdEmployeeCdFormatter },
            { id: "@ColKey.AssetDetailBottom.REMARK1", name: "@Message.getMessage(CommonLabel.LABEL_REMARK1)", field: "REMARK1", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK1", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK1").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.REMARK2", name: "@Message.getMessage(CommonLabel.LABEL_REMARK2)", field: "REMARK2", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK2", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK2").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.REMARK3", name: "@Message.getMessage(CommonLabel.LABEL_REMARK3)", field: "REMARK3", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK3", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK3").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK3").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.REMARK4", name: "@Message.getMessage(CommonLabel.LABEL_REMARK4)", field: "REMARK4", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK4", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK4").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK4").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.REMARK5", name: "@Message.getMessage(CommonLabel.LABEL_REMARK5)", field: "REMARK5", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK5", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK5").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK5").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.REMARK6", name: "@Message.getMessage(CommonLabel.LABEL_REMARK6)", field: "REMARK6", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK6", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK6").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK6").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.REMARK7", name: "@Message.getMessage(CommonLabel.LABEL_REMARK7)", field: "REMARK7", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK7", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK7").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK7").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.REMARK8", name: "@Message.getMessage(CommonLabel.LABEL_REMARK8)", field: "REMARK8", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK8", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK8").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK8").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.REMARK9", name: "@Message.getMessage(CommonLabel.LABEL_REMARK9)", field: "REMARK9", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK9", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK9").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK9").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.REMARK10", name: "@Message.getMessage(CommonLabel.LABEL_REMARK10)", field: "REMARK10", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.REMARK10", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("REMARK10").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("REMARK10").DispFlag" },

        ]

        // 薬品機能ONの場合
        if ("@ViewBag.DrugsFunc" == "True") {
            columnsBottom.push(
            { id: "@ColKey.AssetDetailBottom.WEIGHT_VAL", name: "@Message.getMessage(CommonLabel.LABEL_WEIGHT_VAL)", field: "DUE_QTY", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.WEIGHT_VAL", 150), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("WEIGHT_VAL").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("WEIGHT_VAL").DispFlag", formatter: WeightValFormatter },
            { id: "@ColKey.AssetDetailBottom.WORKER_CD", name: "@Message.getMessage(CommonLabel.LABEL_WORKER_CD_NAME)", field: "WORKER_CD", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.WORKER_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("WORKER_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("WORKER_CD").DispFlag", formatter: WorkerCdFormatter },
            { id: "@ColKey.AssetDetailBottom.WORKING_HOURS", name: "@Message.getMessage(CommonLabel.LABEL_WORKING_HOURS)", field: "WORKER_MINUTE", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.WORKING_HOURS", 150), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("WORKING_HOURS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("WORKING_HOURS").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.WORK_SUMMARY", name: "@Message.getMessage(CommonLabel.LABEL_WORK_SUMMARY)", field: "WORKER_OVERVIEW", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.WORK_SUMMARY", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("WORK_SUMMARY").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("WORK_SUMMARY").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.RETURN_REMARK1", name: "@Message.getMessage(CommonLabel.LABEL_RETURN_REMARK1)", field: "WORKER_REMARK1", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.RETURN_REMARK1", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("RETURN_REMARK1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("RETURN_REMARK1").DispFlag" },
            { id: "@ColKey.AssetDetailBottom.RETURN_REMARK2", name: "@Message.getMessage(CommonLabel.LABEL_RETURN_REMARK2)", field: "WORKER_REMARK2", width: getColumnWidth("@DispId.ASSET_DETAIL_BOTTOM", "@ColKey.AssetDetailBottom.RETURN_REMARK2", 270), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("RETURN_REMARK2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("RETURN_REMARK2").DispFlag" });
        }

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
            autoHeight: false,                      //Gridの高さの自動調整可否（ヘッダー固定のためfalse）
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonDataTop = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.MstAssetData))) ');
        let jsonDataBottom = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // Gridのデータ管理オブジェクトにセット
        let dataViewTop = new Slick.Data.DataView();
        let dataViewBottom = new Slick.Data.DataView();

        // 固定列を設定
        let fixColTop = Number("@Model.FirstOrDefault.FixColCntTop");
        let fixColBottom = Number("@Model.FirstOrDefault.FixColCntBottom");

        // Grid表示判定
        if ("@ViewBag.MstAssetData" != null & "@ViewBag.MstAssetData.Count" > 0) {
            dataViewTop.beginUpdate();
            dataViewTop.setItems(jsonDataTop, "ASSET_NO");
            dataViewTop.endUpdate();
        }

        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataViewBottom.beginUpdate();
            dataViewBottom.setItems(jsonDataBottom, "PROCESS_LOG_ID");
            dataViewBottom.endUpdate();
        }

        // Grid初期化
        gridTop = initializeGrid("ListGridsTop", dataViewTop, columnsTop, optionsTop, "@DispId.ASSET_DETAIL_TOP", fixColTop);
        gridBottom = initializeGrid("ListGridsBottom", dataViewBottom, columnsBottom, optionsBottom, "@DispId.ASSET_DETAIL_BOTTOM", fixColBottom);

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(gridBottom, "ListGridsBottom");


    });

    // 資産状態formatter
    function AssetStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch (String(dataContext.ASSET_STATUS)) {
            case "@ASSET_STATUS.Preparation":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_JUNBI)"
                break;
            case "@ASSET_STATUS.Storage":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_HOKAN)"
                break;
            case "@ASSET_STATUS.Lending":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_LEASE)"
                break;
            case "@ASSET_STATUS.Removed":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_RETIRE)"
                break;
            default:
                status = dataContext.ASSET_STATUS;
                break;
        }

        return status;
    }

    // 資産ID状態formatter
    function EpcEncStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (String(dataContext.EPC_ENC_STATUS) === "0") {
            status = "@Message.getMessage(CommonLabel.VALUE_MI)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_SUMI)"
        }

        return status;
    }

    // タグ紐付け状態formatter
    function TagLinkStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (String(dataContext.TAG_LINK_STATUS) === "0") {
            status = "@Message.getMessage(CommonLabel.VALUE_MI)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_SUMI)"
        }

        return status;
    }

    // 貸出状態formatter
    function LeaseStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (String(dataContext.LEASE_STATUS) === "0") {
            status = "@Message.getMessage(CommonLabel.VALUE_FUKA)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_KANOU)"
        }

        return status;
    }

    // カテゴリー大分類コード/名formatter
    function CatLCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CAT_L_NAME)
    }

    // カテゴリー中分類コード/名formatter
    function CatMCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_M_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CAT_M_NAME)
    }

    // カテゴリー小分類コード/名formatter
    function CatSCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_S_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CAT_S_NAME)
    }

    // 処理日時formatter
    function ProcessDtFormatter(row, cell, value, columnDef, dataContext) {
        let processDt = toStringDateYMD(String(dataContext.PROCESS_DT)) + ' ' +
                        String(dataContext.PROCESS_TM.substring(0, 2) + ':' +
                        dataContext.PROCESS_TM.substring(2, 4));
        return toFormatDate(processDt, "yyyy/MM/dd HH:mm");
    }

    // 貸出回数formatter
    function LeaseCntFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:right; width:100%;">' + dataContext.LEASE_CNT + '</div>'
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
            case "@ProcessDiv.RI":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_RI)"
                break;
            case "@ProcessDiv.RU":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_RU)"
                break;
            case "@ProcessDiv.RD":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_RD)"
                break;
            case "@ProcessDiv.CD":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CD)"
                break;
            case "@ProcessDiv.DC":
                div = "@Message.getMessage(CommonLabel.VALUE_REMAIN_CHECK)"
            case "@ProcessDiv.LP":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LP)"
                break;
            case "@ProcessDiv.LI":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LI)"
                break;
            case "@ProcessDiv.UI":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_UI)"
                break;
            case "@ProcessDiv.IC":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_IC)"
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

    // 貸出先コード/名formatter
    function LeasePlaceCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LEASE_PLACE_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LEASE_PLACE_NAME)
    }

    // 貸出先担当社員コード/名formatter
    function LeaseEmployeeCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LEASE_EMPLOYEE_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LEASE_EMPLOYEE_NAME)
    }

    // 貸出期間formatter
    function LeaseKikanFormatter(row, cell, value, columnDef, dataContext) {
        let sLeaseDt = "　　　　　"; // 空白を埋めるためのスペース
        if (dataContext.LEASE_DT) {
            sLeaseDt = toStringDateYMD(String(dataContext.LEASE_DT));
        }

        let sLeaseDueDt = "";
        if (dataContext.LEASE_DUE_DT) {
            sLeaseDueDt = toStringDateYMD(String(dataContext.LEASE_DUE_DT));
        }

        let sLeaseComment = sLeaseDt;
        if (!(sLeaseDt.trim() === "" && sLeaseDueDt === "")) {
            sLeaseComment = sLeaseDt + "～" + sLeaseDueDt;
        }

        return sLeaseComment;
    }

    // 重量formatter
    function WeightValFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:right; width:100%;">' + convertToEmptyIfNull(dataContext.DUE_QTY) + '</div>'
    }

    // 作業者コード/名formatter
    function WorkerCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.WORKER_EMPLOYEE_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.WORKER_EMPLOYEE_NAME)
    }

    // 管理ロケーションコード/名formatter
    function MngLocCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.MNG_LOC_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.MNG_LOC_L_NAME)
    }

    // ロケーション大区分コード/名formatter
    function LocLCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LOC_L_NAME)
    }

    // ロケーション中区分コード/名formatter
    function LocMCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_M_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LOC_M_NAME)
    }

    // ロケーション小区分コード/名formatter
    function LocSCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_S_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LOC_S_NAME)
    }

    // ログイン社員コード/名formatter
    function UpdEmployeeCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.UPD_EMPLOYEE_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.UPD_EMPLOYEE_NAME)
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/AssetDetail")/CsvExport");

    }

    ////検索ボタンクリック処理
    //function btnSearch_onclick() {
    //    //ページ番号のリセット
    //    ResetPageNum();
    //    return true;
    //}

    ////クリアボタンクリック処理
    //function btnClear_onclick() {
    //    //検索条件を初期化
    //    document.getElementById("SearchCode").value = "";
    //    document.getElementById("SearchName").value = "";
    //    document.getElementById("SearchBusho").value = "";
    //    document.getElementById("SearchKengen").value = "";
    //    document.getElementById("CsvPath").value = "";
    //    document.getElementById("PageNum").value = 1;

    //    //ページ番号のリセット
    //    ResetPageNum();

    //    return true;
    //}


    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        $('#form').submit();   //2020/02/20 nv miyaji
    }
</script>

<div id="asset-detail">
    @Using (Html.BeginForm("Index", "AssetDetail", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb10">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    <input class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
                    <input class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "AssetList")'">
                </div>
            </div>
        </div>
        @<div>
            <div id="ListGridsTop" style="height:auto;padding-bottom: 20px;" />
        </div>
        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div class="list-view">
            @* 高さはヘッダー固定の処理（enableFixedHeader）で画面の残り領域に合わせる *@
            <div id="ListGridsBottom" style="height:auto;margin-bottom: 20px;" />
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
