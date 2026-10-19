@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of AssetHistoryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelAssetHistory.TITLE)
End Code
<script>
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

        //20230904 ADD Start Shirakawa
        //上限を超えている場合エラーメッセージ
        if ("@ViewBag.isLimitFlg" == "True") {
            showErrMessage("@String.Format(Message.getMessage(CommonLabel.MSG_ERR_LIMIT_DATA), ViewBag.cntList, ViewBag.cntLimit)")

            //上限を超えている場合はCSV出力できないようにする。
            $("#csvBtn").prop("disabled", true);
            $("#csvBtn").attr('style', 'background-color:#cecece;');
            $("#csvBtn").css("cursor", "default");
        }
        //20230904 ADD End Shirakawa

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        // ロケーション大区分非選択時は中区分を非活性
        // ※中区分・小区分は複数選択（Select2）のため、共通関数（Scripts\select2-common.js）で値を操作する
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocL").value == "") {
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        // ロケーション中区分非選択時は小区分を非活性
        //[CC431] UPD Start 20261007
        // ※場所から選ぶ ON の場合は、小区分非選択時に中区分を非活性
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocL").value == "" || (!isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocM").length == 0)) {
            clearMultiSelect("#SearchLocS");
            $("#SearchLocS").attr("disabled", "disabled");
        }
        if (isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocS").length == 0) {
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        //[CC431] UPD End 20261007

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        // 列定義
        let columns = [
            { id: "@ColKey.AssetHistory.PROCESS_DT", name: "@Message.getMessage(CommonLabel.LABEL_TASK_TS)", field: "PROCESS_DT", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.PROCESS_DT", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("PROCESS_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PROCESS_DT").DispFlag", formatter: ProcessDtFormatter },
            { id: "@ColKey.AssetHistory.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.ASSET_NO", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag" },
            { id: "@ColKey.AssetHistory.SUB_ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_SUB_ASSET_NO)", field: "SUB_ASSET_NO", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.SUB_ASSET_NO", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispFlag" },
            { id: "@ColKey.AssetHistory.PROCESS_DIV", name: "@Message.getMessage(CommonLabel.LABEL_PROCESS_DIV)", field: "PROCESS_DIV", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.PROCESS_DIV", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("PROCESS_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PROCESS_DIV").DispFlag", formatter: ProcessDivFormatter },
            { id: "@ColKey.AssetHistory.PROCESS_TOOL", name: "@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL)", field: "PROCESS_TOOL", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.PROCESS_TOOL", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("PROCESS_TOOL").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PROCESS_TOOL").DispFlag", formatter: ProcessToolDivFormatter },
            { id: "@ColKey.AssetHistory.LEASE_PLACE_CD", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD_NAME)", field: "LEASE_PLACE_CD", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.LEASE_PLACE_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispFlag", formatter: LeasePlaceCdFormatter },
            { id: "@ColKey.AssetHistory.REMARK", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_REMARK)", field: "REMARK", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK").DispFlag" },
            { id: "@ColKey.AssetHistory.LEASE_EMPLOYEE_CD", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_EMPLOYEE_CD_NAME)", field: "LEASE_EMPLOYEE_CD", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.LEASE_EMPLOYEE_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_EMPLOYEE_CD").DispFlag", formatter: LeaseEmployeeCdFormatter },
            { id: "@ColKey.AssetHistory.LEASE_KIKAN", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_KIKAN)", field: "LEASE_DT", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.LEASE_KIKAN", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_DT").DispFlag", formatter: LeaseKikanFormatter },
            { id: "@ColKey.AssetHistory.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_MNG_LOC_L_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.MNG_LOC_L_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag", formatter: MngLocCdFormatter },
            { id: "@ColKey.AssetHistory.LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_L_COM_NAME)", field: "LOC_L_CD", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.LOC_L_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag", formatter: LocLCdFormatter },
            { id: "@ColKey.AssetHistory.LOC_M_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_NAME)", field: "LOC_M_CD", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.LOC_M_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispFlag", formatter: LocMCdFormatter },
            { id: "@ColKey.AssetHistory.LOC_S_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_NAME)", field: "LOC_S_CD", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.LOC_S_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispFlag", formatter: LocSCdFormatter },
            { id: "@ColKey.AssetHistory.CAT_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)", field: "CAT_L_CD", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.CAT_L_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag", formatter: CatLFormatter},
            { id: "@ColKey.AssetHistory.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)", field: "ASSET_NAME", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.ASSET_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag" },
            { id: "@ColKey.AssetHistory.MAKER", name: "@Message.getMessage(CommonLabel.LABEL_MAKER)", field: "MAKER", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.MAKER", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("MAKER").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAKER").DispFlag" },
            { id: "@ColKey.AssetHistory.PRODUCT_NO", name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)", field: "PRODUCT_NO", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.PRODUCT_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispFlag" },
            { id: "@ColKey.AssetHistory.MODEL_NO", name: "@Message.getMessage(CommonLabel.LABEL_MODEL_NO)", field: "MODEL_NO", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.MODEL_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispFlag" },
            { id: "@ColKey.AssetHistory.ASSET_ID", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_ID)", field: "ASSET_ID", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.ASSET_ID", 480), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_ID").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_ID").DispFlag" },
            { id: "@ColKey.AssetHistory.UPD_EMPLOYEE_CD", name: "@Message.getMessage(CommonLabel.LABEL_EMPLOYEE_CD_NAME)", field: "UPD_EMPLOYEE_CD", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.UPD_EMPLOYEE_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("UPD_EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("UPD_EMPLOYEE_CD").DispFlag", formatter: UpdEmployeeCdFormatter },
            { id: "@ColKey.AssetHistory.REMARK1", name: "@Message.getMessage(CommonLabel.LABEL_REMARK1)", field: "REMARK1", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK1", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK1").DispFlag" },
            { id: "@ColKey.AssetHistory.REMARK2", name: "@Message.getMessage(CommonLabel.LABEL_REMARK2)", field: "REMARK2", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK2", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK2").DispFlag" },
            { id: "@ColKey.AssetHistory.REMARK3", name: "@Message.getMessage(CommonLabel.LABEL_REMARK3)", field: "REMARK3", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK3", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK3").DispFlag" },
            { id: "@ColKey.AssetHistory.REMARK4", name: "@Message.getMessage(CommonLabel.LABEL_REMARK4)", field: "REMARK4", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK4", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK4").DispFlag" },
            { id: "@ColKey.AssetHistory.REMARK5", name: "@Message.getMessage(CommonLabel.LABEL_REMARK5)", field: "REMARK5", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK5", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK5").DispFlag" },
            { id: "@ColKey.AssetHistory.REMARK6", name: "@Message.getMessage(CommonLabel.LABEL_REMARK6)", field: "REMARK6", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK6", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK6").DispFlag" },
            { id: "@ColKey.AssetHistory.REMARK7", name: "@Message.getMessage(CommonLabel.LABEL_REMARK7)", field: "REMARK7", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK7", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK7").DispFlag" },
            { id: "@ColKey.AssetHistory.REMARK8", name: "@Message.getMessage(CommonLabel.LABEL_REMARK8)", field: "REMARK8", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK8", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK8").DispFlag" },
            { id: "@ColKey.AssetHistory.REMARK9", name: "@Message.getMessage(CommonLabel.LABEL_REMARK9)", field: "REMARK9", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK9", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK9").DispFlag" },
            { id: "@ColKey.AssetHistory.REMARK10", name: "@Message.getMessage(CommonLabel.LABEL_REMARK10)", field: "REMARK10", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.REMARK10", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK10").DispFlag" },
        ]

        // 薬品機能ONの場合
        if ("@ViewBag.DrugsFunc" == "True") {
            columns.push(
            { id: "@ColKey.AssetHistory.WEIGHT_VAL", name: "@Message.getMessage(CommonLabel.LABEL_WEIGHT_VAL)", field: "DUE_QTY", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.WEIGHT_VAL", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("WEIGHT_VAL").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WEIGHT_VAL").DispFlag", formatter: WeightValFormatter },
            { id: "@ColKey.AssetHistory.WORKER_CD", name: "@Message.getMessage(CommonLabel.LABEL_WORKER_CD_NAME)", field: "WORKER_CD", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.WORKER_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("WORKER_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WORKER_CD").DispFlag", formatter: WorkerCdFormatter },
            { id: "@ColKey.AssetHistory.WORKING_HOURS", name: "@Message.getMessage(CommonLabel.LABEL_WORKING_HOURS)", field: "WORKER_MINUTE", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.WORKING_HOURS", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("WORKING_HOURS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WORKING_HOURS").DispFlag" },
            { id: "@ColKey.AssetHistory.WORK_SUMMARY", name: "@Message.getMessage(CommonLabel.LABEL_WORK_SUMMARY)", field: "WORKER_OVERVIEW", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.WORK_SUMMARY", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("WORK_SUMMARY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WORK_SUMMARY").DispFlag" },
            { id: "@ColKey.AssetHistory.RETURN_REMARK1", name: "@Message.getMessage(CommonLabel.LABEL_RETURN_REMARK1)", field: "WORKER_REMARK1", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.RETURN_REMARK1", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("RETURN_REMARK1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("RETURN_REMARK1").DispFlag" },
            { id: "@ColKey.AssetHistory.RETURN_REMARK2", name: "@Message.getMessage(CommonLabel.LABEL_RETURN_REMARK2)", field: "WORKER_REMARK2", width: getColumnWidth("@DispId.ASSET_HISTORY", "@ColKey.AssetHistory.RETURN_REMARK2", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("RETURN_REMARK2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("RETURN_REMARK2").DispFlag" });
        }

        // Gridのオプション機能の設定
        let options = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: false,                      //Gridの高さの自動調整可否（ヘッダー固定のためfalse）
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // Grid表示判定
        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "PROCESS_LOG_ID");
            dataView.endUpdate();
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.ASSET_HISTORY", fixCol);

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

        //ローカルストレージから復元
        const saved = localStorage.getItem("AssetHistorySearchConditions");
        if("@Model.FirstOrDefault.IsRestoreOwnSearchConditions" == "True") {
            var lcd = document.getElementById("SearchLocL");
            //最初の要素（×での選択解除用に追加した空の選択肢は除く）
            var firstLoc = $(lcd).find("option").filter(function () { return this.value !== ""; })[0];
            if (saved) {
                const cond = JSON.parse(saved);
                if (!cond.SearchLocL || cond.SearchLocL.trim() === "") {
                    //ローカルストレージが空の場合は最初の要素を選択
                    if (firstLoc) {
                        lcd.value = firstLoc.value;
                    }
                } else {
                    //ローカルストレージが空でない場合
                    var found = false;
                    for (var i = 0; i < lcd.options.length; i++) {
                        //ローカルストレージに該当する値があれば選択
                        if (lcd.options[i].value == cond.SearchLocL) {
                            lcd.value = cond.SearchLocL;
                            found = true;
                            break;
                        }
                    }
                    if (!found && firstLoc) {
                        //見つからなかった場合は最初の要素を選択
                        lcd.value = firstLoc.value;
                    }
                }
            } else {
                //ローカルストレージが空の場合は最初の要素を選択
                if (firstLoc) {
                    lcd.value = firstLoc.value;
                }
            }

            //選択した内容をSelect2の表示に反映
            //※trigger("change.select2") だと onchange（ChangeLocationL）も呼ばれ、中区分・小区分がクリアされるため refreshSelect2 を使用
            refreshSelect2(lcd);

            //ロケ大の値が変わった時のメソッド実行（中区分・小区分は複数選択のため配列で渡す）
            ChangeLocationL(lcd, getMultiSelectValues("#SearchLocM"), getMultiSelectValues("#SearchLocS"), true);
        }

    });

    //ローカルストレージ保存
    function localStorageSave() {
        let cond = {SearchLocL: document.getElementById("SearchLocL").value};

        //ローカルストレージを保存
        localStorage.setItem("AssetHistorySearchConditions", JSON.stringify(cond));

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
            case "@ProcessDiv.XI":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_XI)"
                break;
            case "@ProcessDiv.XU":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_XU)"
                break;
            case "@ProcessDiv.XD":
                div = "@Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_XD)"
                break;
            case "@ProcessDiv.DC":
                div = "@Message.getMessage(CommonLabel.VALUE_REMAIN_CHECK)"
                break;
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

    // カテゴリ大分類コード用formatter
    function CatLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_L_CD) + ' ' + convertToEmptyIfNull(dataContext.CAT_NAME);
    }

    // ログイン社員コード/名formatter
    function UpdEmployeeCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.UPD_EMPLOYEE_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.UPD_EMPLOYEE_NAME)
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/AssetHistory")/CsvExport");

    }

    //全選択ボタンクリック時処理
    function btnSelectAll_onclick() {
        var chkFlag = false;
        var btn = document.getElementById("btnSelectAll");
        if (btn.value == "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)") {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_OFF)";
            chkFlag = true;
        } else {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
        }

        //処理区分チェックボックスの選択状態を設定
        $('.chkProcDiv').prop('checked', chkFlag)

        //非同期で現在の検索条件での全件を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetHistory")/SelectAll',
            data: { blnSelectAll: chkFlag },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                //件数を設定する
                document.getElementById("selectcount").innerHTML = data;

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }


    //検索項目欄　詳細検索クリック時処理
    function DetailAccordion_onclick() {
        if (document.getElementById("DetailAccordion").innerHTML == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "none");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)";
        } else {
            $(".detail-accordion").css("display", "flex");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)";
        }
        document.getElementById("DetailOpenFlg").value = document.getElementById("DetailAccordion").innerText;
    }

    $(function () {
        if ("@Model.FirstOrDefault.DetailOpenFlg" == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "flex");
            document.getElementById("DetailAccordion").innerHTML = "@Model.FirstOrDefault.DetailOpenFlg";
        }
    });
    //[CC431] ADD Start 20261015
    $(function () {
        // 検索条件の開閉状態の復元（OpenFlg が「閉」の文言のときだけ閉じた状態にする。既定は開）
        if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            $("#cond").removeClass("in");
            $("#cond").addClass("collapse");
            document.getElementById("SearchCondition").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }
        // 検索の再表示で、エラーなし・1 件以上のときは検索条件を折りたたむ（仕様変更1）。
        // エラー・0 件のときは追加しない（検索前の状態＝OpenFlg のまま）。ページ変更は検索ボタンの値を送らないため対象外
        if ("@Request.Form("actionname")" == "@Message.getMessage(CommonLabel.BUTTON_SEARCH)"
            && "@Model.FirstOrDefault.ErrCode" == "" && Number("@Model.FirstOrDefault.TotalCount") > 0) {
            $("#cond").removeClass("in");
            $("#cond").addClass("collapse");
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)";
            document.getElementById("OpenFlg").value = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)";
        }
    });
    //[CC431] ADD End 20261015
    //検索ボタンクリック処理
    function btnSearch_onclick() {
        // 処理区分の選択状態をチェック '2019/10/15 nv miyaji LC追加
        if (!document.getElementById("SearchProcessDivMI").checked
            && !document.getElementById("SearchProcessDivMU").checked
            && !document.getElementById("SearchProcessDivCU").checked
            && !document.getElementById("SearchProcessDivMR").checked
            && !document.getElementById("SearchProcessDivT1").checked
            && !document.getElementById("SearchProcessDivT2").checked
            && !document.getElementById("SearchProcessDivBR").checked
            && !document.getElementById("SearchProcessDivMD").checked   @* 2019/12/19 nv miyaji add *@
            && !document.getElementById("SearchProcessDivCD").checked   @* 2019/12/19 nv miyaji add *@
            && !document.getElementById("SearchProcessDivLP").checked
            && !document.getElementById("SearchProcessDivLI").checked
            && !document.getElementById("SearchProcessDivUI").checked
            && !document.getElementById("SearchProcessDivIC").checked
        ) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_PROCESS_DIV_UNSELECTED)")
            return false;
        }
        // 処理ツールの選択状態をチェック
        if (!document.getElementById("SearchProcessToolDivAll").checked
            && !document.getElementById("SearchProcessToolDivWeb").checked
            && !document.getElementById("SearchProcessToolDivHT").checked) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_PROCESS_TOOL_DIV_UNSELECTED)")
            return false;
        }

        // 日付の前後チェック　'2019/09/25 nv miyaji add
        var arrDateFrom = document.getElementById("SearchDateFrom").value;
        var arrDateTo = document.getElementById("SearchDateTo").value;
        if (arrDateFrom != "" && arrDateTo != "") {
            // 両方とも空白でない時
            // 期間前後チェック
            if (arrDateFrom > arrDateTo) {
                showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")
                //$("#SearchLeaseLimitFrom").addClass("input-error");
                return false;
            }
        }

        // ロケーション(会社)必須チェック
        var locL = document.getElementById("SearchLocL").value;
        if (locL.trim() === "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERRNO_SELECTED_LOC_L)")
            return false;
        }

        //画面ブロックを表示
        showLoading();

        //ページ番号のリセット
        ResetPageNum();

        //ローカルストレージに検索条件保存
        localStorageSave();

        @*//20230904 ADD Start Shirakawa
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetHistory")/SearchClick',
        });
        //20230904 ADD End Shirakawa*@

        return true;
    }

    ////検索条件リンククリック時処理
    //function SearchCondition_onclick() {
    //    if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
    //        document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
    //    } else {
    //        document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
    //    }
    //}
    //[CC431] ADD Start 20261015
    //検索条件リンククリック時処理（リンクの文言を入れ替え、開閉状態を hidden OpenFlg に退避する）
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
        document.getElementById("OpenFlg").value = $.trim(document.getElementById("SearchCondition").innerText);
    }
    //[CC431] ADD End 20261015


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

        //画面ブロックを表示
        showLoading();

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //大区分選択
    //※中区分・小区分は複数選択（Select2）のため、initialM / initialS は配列で渡す
    function ChangeLocationL(locL, initialM, initialS, doAutoSearch) {
        //[CC431] UPD Start 20261007
        // 中区分・小区分をクリアし、場所から選ぶの ON／OFF に合わせて選択肢を取得する（initialM / initialS は選び直す値）
        resetSearchLocOptions("Loc", initialM, initialS, function () {
            // 検索処理実行（初期表示の復元時のみ。会社を選び直した場合・×で選択解除した場合は検索しない）
            if (doAutoSearch && document.getElementById('FirstSearch').value == "True") {
                document.getElementById('Search_Button').click();
            }
        });
        //[CC431] UPD End 20261007
    }

    //中区分選択
    //※小区分は複数選択（Select2）のため、initialS は配列で渡す
    function ChangeLocationM(locM, initialS, doAutoSearch) {
        var lCode = document.getElementById("SearchLocL").value;
        // 選択中の中区分コード（複数選択のため「,」区切りで渡す）
        var mCode = getMultiSelectValues(locM).join(",");

        // 小区分をクリア
        clearMultiSelect("#SearchLocS");
        $("#SearchLocS").prop("disabled", true);

        // 選択されていない場合はここで終了
        if (!mCode) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetHistory")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationS", selectLCode: lCode, selectMCode: mCode },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocS = document.getElementById("SearchLocS");
                    SearchLocS.options.length = 0;

                    // 複数選択では選択解除用の空の選択肢は不要
                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocS.appendChild(op);
                    }
                }
                // 小区分データ取得
                $("#SearchLocS").removeAttr("disabled");
                // 選択肢の入れ替えをSelect2に反映（onchange は発生させない）
                setMultiSelectValues(SearchLocS, initialS);
                if (doAutoSearch) {
                    // 検索処理実行
                    if (document.getElementById('FirstSearch').value == "True") {
                        document.getElementById('Search_Button').click();
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //[CC431] ADD Start 20261007
    //検索条件 ロケーション（原価センタ・場所）の連動
    //  場所から選ぶ OFF：中区分（m）で小区分（s）の選択肢を絞り込む
    //  場所から選ぶ ON ：小区分（s）で中区分（m）の選択肢を絞り込む
    //  絞り込む側の選択値を変えたら、絞り込まれる側の選択値は使えるものを残し、使えなくなったものを消す
    //  xhr：選択肢を取得中の通信（続けて選び直したときに、古い応答で選択肢が上書きされないよう中止する）
    var searchLocPairs = {
        Loc: { l: "#SearchLocL", m: "#SearchLocM", s: "#SearchLocS", reverse: "#SearchLocReverse", xhr: null }
    };

    //場所から選ぶが ON か（チェックボックスがない場合は OFF）
    function isSearchLocReverse(key) {
        return $(searchLocPairs[key].reverse).prop("checked") === true;
    }

    //中区分・小区分の選択値を変えたとき
    //  変えた側が絞り込む側（OFF なら中区分、ON なら小区分）のときだけ、反対側の選択肢を作り直す
    function onSearchLocChange(key, level) {
        var fromLevel = isSearchLocReverse(key) ? "@LOC_DIV.LocationS" : "@LOC_DIV.LocationM";
        if (level !== fromLevel) {
            return;
        }
        refreshSearchLocOptions(key);
    }

    //絞り込まれる側の選択肢を、絞り込む側の選択値で作り直す
    //  keepValues：絞り込まれる側で選び直す値（省略時は今の選択値。使えるものだけ残る）
    //  callback：選択肢を作り終えたら呼ぶ関数（省略可）
    function refreshSearchLocOptions(key, keepValues, callback) {
        var pair = searchLocPairs[key];
        var reverse = isSearchLocReverse(key);
        var from = reverse ? pair.s : pair.m;
        var to = reverse ? pair.m : pair.s;
        var toLevel = reverse ? "@LOC_DIV.LocationM" : "@LOC_DIV.LocationS";
        if (!keepValues) {
            keepValues = getMultiSelectValues(to);
        }

        abortSearchLocRequest(key);

        // 絞り込む側が未選択の場合は、絞り込まれる側をクリアして非活性
        var codes = getMultiSelectValues(from);
        if (codes.length == 0) {
            clearMultiSelect(to);
            $(to).prop("disabled", true);
            if (callback) {
                callback();
            }
            return;
        }

        loadSearchLocOptions(key, toLevel, codes, to, keepValues, callback);
    }

    //場所から選ぶを変えたとき
    //  ON にしたら：場所の選択値は残し、原価センタはクリア。OFF にしたら：原価センタの選択値は残し、場所はクリア
    //  残した側の選択肢を全件にしてから、その選択値で反対側の選択肢を作る
    function onSearchLocReverseChange(key) {
        var pair = searchLocPairs[key];
        var reverse = isSearchLocReverse(key);
        var from = reverse ? pair.s : pair.m;
        var fromLevel = reverse ? "@LOC_DIV.LocationS" : "@LOC_DIV.LocationM";
        var to = reverse ? pair.m : pair.s;

        abortSearchLocRequest(key);

        clearMultiSelect(to);
        $(to).prop("disabled", true);

        // 会社が未選択の場合は取得しない
        if (!$(pair.l).val()) {
            return;
        }

        loadSearchLocOptions(key, fromLevel, [], from, getMultiSelectValues(from), function () {
            refreshSearchLocOptions(key);
        });
    }

    //会社を変えたとき（初期表示の復元を含む）
    //  原価センタ・場所をクリアし（場所から選ぶの ON／OFF は残す）、絞り込む側の選択肢を取得してから、反対側の選択肢を作る
    //  keepM／keepS：選び直す値（初期表示で、検索後の選択値を復元する場合。会社を変えた場合は省略）
    //  callback：選択肢を作り終えたら呼ぶ関数（省略可。初期表示の自動検索に使う）
    function resetSearchLocOptions(key, keepM, keepS, callback) {
        var pair = searchLocPairs[key];
        var reverse = isSearchLocReverse(key);
        var from = reverse ? pair.s : pair.m;
        var fromLevel = reverse ? "@LOC_DIV.LocationS" : "@LOC_DIV.LocationM";

        abortSearchLocRequest(key);

        clearMultiSelect(pair.m);
        $(pair.m).prop("disabled", true);
        clearMultiSelect(pair.s);
        $(pair.s).prop("disabled", true);

        // 会社が未選択の場合はここで終了
        if (!$(pair.l).val()) {
            if (callback) {
                callback();
            }
            return;
        }

        loadSearchLocOptions(key, fromLevel, [], from, (reverse ? keepS : keepM) || [], function () {
            refreshSearchLocOptions(key, (reverse ? keepM : keepS) || [], callback);
        });
    }

    //選択肢を取得中の通信を中止する
    function abortSearchLocRequest(key) {
        var pair = searchLocPairs[key];
        if (pair.xhr) {
            var xhr = pair.xhr;
            pair.xhr = null;
            xhr.abort();
        }
    }

    //選択肢を取得して入れ替え、keepValues のうち選択肢にあるものだけを選択し直す（onchange は発生させない）
    //  codes：絞り込みに使う反対側の選択値（絞り込む側の選択肢を取得する場合はサーバー側で使わない）
    //  callback：取得し終えたら呼ぶ関数（省略可。中止した通信では呼ばない）
    function loadSearchLocOptions(key, level, codes, target, keepValues, callback) {
        var pair = searchLocPairs[key];
        var xhr = $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetHistory")/GetSearchLocationOptions',
            data: { selectLCode: $(pair.l).val(), level: level, reverse: isSearchLocReverse(key), codes: codes.join(",") },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (pair.xhr !== xhr) {
                    return;    // 中止した通信、または古い通信の応答
                }
                pair.xhr = null;
                if (data != null) {
                    var select = $(target)[0];
                    select.options.length = 0;
                    // 複数選択のため選択解除用の空の選択肢は不要
                    for (var i = 0; i < data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data[i].LOC_CD;
                        op.text = data[i].LOC_NAME;
                        select.appendChild(op);
                    }
                    $(target).prop("disabled", false);
                    // 選択肢にない値は jQuery が外すため、使える値だけが残る
                    setMultiSelectValues(target, keepValues);
                }

                if (callback) {
                    callback();
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                if (pair.xhr !== xhr) {
                    return;    // 中止した通信、または古い通信の応答
                }
                pair.xhr = null;
                // 中区分・小区分を使用しない設定（応答が空）の場合もここに来るため、続きの処理は行う
                if (callback) {
                    callback();
                }
            }
        });
        pair.xhr = xhr;
    }
    //[CC431] ADD End 20261007

    //チェックボックスクリック処理
    function check_onclick(element) {
        var checkconfirm = element.checked;
        var count = document.getElementById("selectcount").innerHTML;
        if (checkconfirm) {
            count++;
        } else {
            count--;
        }
        document.getElementById("selectcount").innerHTML = count;
    }

    //チェックボックスの選択状態を設定する
    function setting_checklist() {
        var keys = [];
        var nonekeys = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");

        for (var i = 0; i < checkBoxes.length; i++) {
            //チェックボックスの場合
            if (checkBoxes[i].type == "checkbox") {
                //キーを取得する
                var result = checkBoxes[i].value

                if (checkBoxes[i].checked) {
                    //選択されている場合
                    keys.push(result);
                } else {
                    //選択されていない場合
                    nonekeys.push(result);
                }
            }
        }

        // スラッシュ区切りの文字列を設定
        document.getElementById("SelectKeys").value = keys.join("/");
        document.getElementById("NoneSelectKeys").value = nonekeys.join("/");

        return keys;
    }

</script>

<div id="asset-history">
    @Using (Html.BeginForm("Index", "AssetHistory", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mbyuu20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    <input id="csvBtn" class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
                </div>
            </div>
        </div>
        @* 検索条件 *@
        @<div Class="search-wrap">
            <a id="SearchCondition" data-toggle="collapse" href="#cond" aria-expanded="true" onclick="SearchCondition_onclick()">
                @*[CC431] UPD Start 20261015 他の画面に合わせ、開閉で入れ替わる文言（▼検索条件／▲検索条件）にする*@
                @Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)
                @*[CC431] UPD End 20261015*@
            </a>
            <div class="search-border">
                <div id="cond" class="container collapse in">
                    @* condition-flex：複数選択で項目の高さが伸びても段が崩れないよう flex で並べる（Content\Site.css） *@
                    @*[CC431] UPD Start 20261015 並び順と先頭位置を揃えるため、項目を col-md-12 に入れる（実棚照会と同じ作り）*@
                    <div class="row" id="item-conditions">
                        <div class="col-md-12 condition-flex">
                    @*[CC431] UPD End 20261015*@
                        @* ロケーション 大区分*@
                        <div class="col-sm-2 filter-combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L)<font Class="label-require">※</font></div>
                            <div Class="grid-condition-col">
                                @* select2-clearable クラスで入力して絞り込めるSelect2になる（Scripts\select2-common.js）。選択肢・選択後とも「コード 名称」を表示 *@
                                @* ×で選択解除するため、空の選択肢を追加している *@
                                @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocL, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeLocationL(this)", .class = "cond-control select2-clearable"})
                            </div>
                        </div>
                        @* ロケーション 中区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                        <div class="col-sm-2 combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag = "1" AndAlso Model.FirstOrDefault.GridInfo("LOC_M_CD").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_M)</div>
                            <div Class="grid-condition-col">
                                @* ListBoxFor で複数選択（select multiple）。select2-clearable クラスで×ボタン付きSelect2になる。空の選択肢は不要 *@
                                @*[CC431] UPD Start 20261007*@
                                @* 選択値を変えたら、場所から選ぶ OFF のとき場所の選択肢を作り直す *@
                                @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchLocM, DirectCast(ViewBag.modelLocM, IEnumerable(Of SelectListItem)), New With {.onchange = "onSearchLocChange('Loc', '" & LOC_DIV.LocationM & "')", .class = "cond-control select2-clearable"})
                                @*[CC431] UPD End 20261007*@
                            </div>
                        </div>
                        @* ロケーション 小区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                        <div class="col-sm-2 combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag = "1" AndAlso Model.FirstOrDefault.GridInfo("LOC_M_CD").DispFlag = "1" AndAlso Model.FirstOrDefault.GridInfo("LOC_S_CD").DispFlag = "1", "display", "None"))">
                            @*[CC431] UPD Start 20261007*@
                            <div class="grid-head-col">
                                @Message.getMessage(CommonLabel.LABEL_LOC_S)
                                @* 「場所から選ぶ」チェックボックス（ON：場所で原価センタの選択肢を絞り込む）。中区分・小区分の両方を使用する場合だけ表示 *@
                                @* head-check：見出し右端の緑のチェックボックス（Content\Site.css） *@
                                @If ViewBag.LocUsage_M = Misc.ItemUse.Yes AndAlso ViewBag.LocUsage_S = Misc.ItemUse.Yes Then
                                    @<label class="head-check">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLocReverse, New With {.onchange = "onSearchLocReverseChange('Loc')"})@Message.getMessage(CommonLabel.LABEL_SELECT_FROM_PLACE)</label>
                                End If
                            </div>
                            <div class="grid-condition-col">
                                @* ListBoxFor で複数選択（select multiple）。選択値を変えたら、場所から選ぶ ON のとき原価センタの選択肢を作り直す *@
                                @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchLocS, DirectCast(ViewBag.modelLocS, IEnumerable(Of SelectListItem)), New With {.onchange = "onSearchLocChange('Loc', '" & LOC_DIV.LocationS & "')", .class = "cond-control select2-clearable"})
                            </div>
                            @*[CC431] UPD End 20261007*@
                        </div>
                        @* カテゴリー 大区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area）*@
                        <div class="col-sm-2 combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                            <div Class="grid-condition-col">
                                @* ListBoxFor で複数選択（select multiple）。select2-clearable クラスで×ボタン付きSelect2になる。空の選択肢は不要 *@
                                @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), New With {.class = "cond-control select2-clearable"})
                            </div>
                        </div>                    
                        @* 期間 *@
                        <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("PROCESS_DT").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PERIOD)</div>
                            <div class="grid-condition-col-asset">
                                @Html.EditorFor(Function(model) model.FirstOrDefault.SearchDateFrom, New With {.htmlAttributes = New With {.class = "cond-control", .type = "date"}})
                                ～
                                @Html.EditorFor(Function(model) model.FirstOrDefault.SearchDateTo, New With {.htmlAttributes = New With {.class = "cond-control", .type = "date"}})
                            </div>
                        </div>
                    @*[CC431] ADD Start 20261015 col-md-12 の閉じタグ*@
                        </div>
                    @*[CC431] ADD End 20261015*@
                    </div>
                    @*[CC431] UPD Start 20261015 並び順と先頭位置を揃えるため、項目を col-md-12 に入れる（実棚照会と同じ作り）*@
                    <div class="row" id="item-conditions">
                        <div class="col-md-12">
                    @*[CC431] UPD End 20261015*@
                        @* 処理区分 *@
                        <div style="display:@(If(Model.FirstOrDefault.GridInfo("PROCESS_DIV").DispFlag = "1", "display", "None"))">
                            @*[CC431] UPD Start 20261015 見出しも画面いっぱいの幅（col-md-12）にする*@
                            <div class="col-md-12">
                            @*[CC431] UPD End 20261015*@
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PROCESS_DIV)</div>
                            </div>
                            <div class="col-md-12">
                                <div class="grid-condition-col">
                                    @*[CC431] UPD Start 20261015 処理区分を画面いっぱいの幅にする（固定幅 1250px を 100% に変更）*@
                                    <td class="condition-col radio-group" style="width:100%;">
                                    @*[CC431] UPD End 20261015*@
                                        <div class="col">
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivLP, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LP)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivLI, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LI)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivUI, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_UI)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivMI, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MI)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivMU, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MU)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivCU, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CU)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivMR, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MR)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivMD, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MD)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivCD, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CD)</div>
                                            @*[CC431] ADD Start 20261015 2 段目の項目（タグラベル発行＆紐付以降・全選択ボタン）を 1 段目に移動*@
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivT1, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_T1)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivT2, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_T2)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivBR, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_RETIRE)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessDivIC, New With {.class = "chkProcDiv"}) @Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_IC)</div>
                                            @* 全選択/全解除ボタンの表記文字設定 *@
                                            @Code
                                                Dim AllSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG)
                                                Dim sDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                                                If AllSelectFlg Then
                                                    sDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                                                End If
                                            End Code
                                            <input id="btnSelectAll" Class="defButton submit-button" type="button" value="@sDispStr" onclick="btnSelectAll_onclick()" />
                                            @*[CC431] ADD End 20261015*@
                                        </div>
                                    </td>
                                </div>
                            </div>
                        </div>
                    @*[CC431] ADD Start 20261015 col-md-12 の閉じタグ*@
                        </div>
                    @*[CC431] ADD End 20261015*@
                    </div>
                    @*[CC431] DEL 20261015 処理区分の 2 段目（タグラベル発行＆紐付ほか・全選択ボタン）を削除し、1 段目に移動*@
                    @*[CC431] UPD Start 20261015 並び順と先頭位置を揃えるため、項目を col-md-12 に入れる（実棚照会と同じ作り）*@
                    <div class="row" id="item-conditions">
                        <div class="col-md-12">
                    @*[CC431] UPD End 20261015*@
                        @* 処理分類 *@
                        <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("PROCESS_TOOL").DispFlag = "1", "display", "None"));max-width:300px;">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL)</div>
                            <div class="grid-condition-col">
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchProcessToolDiv, "", New With {.id = "SearchProcessToolDivAll"}) @Message.getMessage(CommonLabel.LABEL_SELECT_ALL)</div>
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchProcessToolDiv, ProcessToolDiv.Web, New With {.id = "SearchProcessToolDivWeb"}) @Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)</div>
                                @*<div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchProcessToolDiv, ProcessToolDiv.CS, New With {.id = "SearchProcessToolDivCS"}) @Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS)</div>*@
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchProcessToolDiv, ProcessToolDiv.HT, New With {.id = "SearchProcessToolDivHT"}) @Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)</div>
                            </div>
                        </div>
                    @*[CC431] ADD Start 20261015 col-md-12 の閉じタグ*@
                        </div>
                    @*[CC431] ADD End 20261015*@
                    </div>
                    @*[CC431] UPD Start 20261015 並び順と先頭位置を揃えるため、項目を col-md-12 に入れる（実棚照会と同じ作り）*@
                    <div class="row" id="item-conditions">
                        <div class="col-md-12">
                    @*[CC431] UPD End 20261015*@
                        @* 資産番号 *@
                        <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NO)</div>
                            <div class="grid-condition-col-multi">
                                @* 「|」区切りで複数入力可。×でクリア、複数ボタンで複数入力ダイアログを表示（Scripts\multi-input-dialog.js） *@
                                <span class="clearable">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetNo, New With {.class = "cond-control", .oninput = "editAssetNoMultiInput(this)"})
                                    <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                </span>
                                @*[CC431] UPD Start 20261013 複数入力ダイアログの文字数（24）・文字種（editAssetNo）の制限を追加*@
                                <button type="button" class="btn btn-default multi-input-button" data-multi-input="#SearchAssetNo" data-multi-title="@Message.getMessage(CommonLabel.LABEL_ASSET_NO)" data-multi-maxlength="24" data-multi-edit="editAssetNo">@Message.getMessage(CommonLabel.BUTTON_MULTI_INPUT)</button>
                                @*[CC431] UPD End 20261013*@
                            </div>
                        </div>
                        @* 資産名称 *@
                        <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</div>
                            <div class="grid-condition-col">
                                @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetName, New With {.class = "cond-control", .maxlength = 60})
                            </div>
                        </div>
                        @* ログイン者名 *@
                        <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("UPD_EMPLOYEE_CD").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabelOperationLog.LABEL_LOGIN_EMPLOYEE_NAME)</div>
                            <div class="grid-condition-col">
                                @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchEmployeeNm, New With {.class = "cond-control", .maxlength = 60})
                            </div>
                        </div>
                    @*[CC431] ADD Start 20261015 col-md-12 の閉じタグ*@
                        </div>
                    @*[CC431] ADD End 20261015*@
                    </div>
                    @*[CC431] ADD Start 20261015*@
                    @* 並び順（ラジオボタン15種。処理日時は降順、ほかは昇順。選択値は SearchSortOrder、Controller の GetOrderBy で ORDER BY に変換） *@
                    <div class="row" id="item-sort">
                        <div class="col-md-12">
                            <div class="grid-head-col sort-head-col">@Message.getMessage(CommonLabel.LABEL_SORT_ORDER)</div>
                            <div id="search-sort" class="grid-condition-col sort-condition-col" style="margin-left: 15px; margin-right: 15px; width: calc(100% - 30px);">
                                @Code
                                    Dim sSortAsc As String = Message.getMessage(CommonLabel.LABEL_SORT_ASC)
                                    Dim sSortDesc As String = Message.getMessage(CommonLabel.LABEL_SORT_DESC)
                                    Dim lstSort As New List(Of KeyValuePair(Of Integer, String)) From {
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.ProcessDtDesc, Message.getMessage(CommonLabel.LABEL_TASK_TS) & sSortDesc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.CatLAsc, Message.getMessage(CommonLabel.LABEL_CAT_L) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.AssetNoAsc, Message.getMessage(CommonLabel.LABEL_ASSET_NO) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.LocMAsc, Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_CODE) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.LocSAsc, Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_CODE) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark1Asc, Message.getMessage(CommonLabel.LABEL_REMARK1) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark2Asc, Message.getMessage(CommonLabel.LABEL_REMARK2) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark3Asc, Message.getMessage(CommonLabel.LABEL_REMARK3) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark4Asc, Message.getMessage(CommonLabel.LABEL_REMARK4) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark5Asc, Message.getMessage(CommonLabel.LABEL_REMARK5) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark6Asc, Message.getMessage(CommonLabel.LABEL_REMARK6) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark7Asc, Message.getMessage(CommonLabel.LABEL_REMARK7) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark8Asc, Message.getMessage(CommonLabel.LABEL_REMARK8) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark9Asc, Message.getMessage(CommonLabel.LABEL_REMARK9) & sSortAsc),
                                        New KeyValuePair(Of Integer, String)(AssetHistorySort.Remark10Asc, Message.getMessage(CommonLabel.LABEL_REMARK10) & sSortAsc)
                                    }
                                End Code
                                @For Each kvSort As KeyValuePair(Of Integer, String) In lstSort
                                    @<div class="input-select-item">
                                        @Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchSortOrder, kvSort.Key)@kvSort.Value
                                    </div>
                                Next
                            </div>
                        </div>
                    </div>
                    @*[CC431] ADD End 20261015*@
                    <div class="row">
                        <div class="col-md-12">
                            <a href="#" id="DetailAccordion" onclick="DetailAccordion_onclick();return false;">@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)</a>
                        </div>
                    </div>
                    @*[CC431] UPD Start 20261015 並び順と先頭位置を揃えるため、項目を col-md-12 に入れる（実棚照会と同じ作り）*@
                    <div class="row detail-accordion" id="item-conditions-detail" style="display:none;">
                        <div class="col-md-12 condition-flex">
                    @*[CC431] UPD End 20261015*@
                        @Code
                            Dim lstRemark As New List(Of KeyValuePair(Of String, String)) From {
                                New KeyValuePair(Of String, String)("SearchRemark1", Message.getMessage(CommonLabel.LABEL_REMARK1)),
                                New KeyValuePair(Of String, String)("SearchRemark2", Message.getMessage(CommonLabel.LABEL_REMARK2)),
                                New KeyValuePair(Of String, String)("SearchRemark3", Message.getMessage(CommonLabel.LABEL_REMARK3)),
                                New KeyValuePair(Of String, String)("SearchRemark4", Message.getMessage(CommonLabel.LABEL_REMARK4)),
                                New KeyValuePair(Of String, String)("SearchRemark5", Message.getMessage(CommonLabel.LABEL_REMARK5)),
                                New KeyValuePair(Of String, String)("SearchRemark6", Message.getMessage(CommonLabel.LABEL_REMARK6)),
                                New KeyValuePair(Of String, String)("SearchRemark7", Message.getMessage(CommonLabel.LABEL_REMARK7)),
                                New KeyValuePair(Of String, String)("SearchRemark8", Message.getMessage(CommonLabel.LABEL_REMARK8)),
                                New KeyValuePair(Of String, String)("SearchRemark9", Message.getMessage(CommonLabel.LABEL_REMARK9)),
                                New KeyValuePair(Of String, String)("SearchRemark10", Message.getMessage(CommonLabel.LABEL_REMARK10))
                            }
                            Dim dicRemarkValue As New Dictionary(Of String, String) From {
                                {"SearchRemark1", Model.FirstOrDefault.SearchRemark1},
                                {"SearchRemark2", Model.FirstOrDefault.SearchRemark2},
                                {"SearchRemark3", Model.FirstOrDefault.SearchRemark3},
                                {"SearchRemark4", Model.FirstOrDefault.SearchRemark4},
                                {"SearchRemark5", Model.FirstOrDefault.SearchRemark5},
                                {"SearchRemark6", Model.FirstOrDefault.SearchRemark6},
                                {"SearchRemark7", Model.FirstOrDefault.SearchRemark7},
                                {"SearchRemark8", Model.FirstOrDefault.SearchRemark8},
                                {"SearchRemark9", Model.FirstOrDefault.SearchRemark9},
                                {"SearchRemark10", Model.FirstOrDefault.SearchRemark10}
                            }
                        End Code
                        @For Each kvRemark As KeyValuePair(Of String, String) In lstRemark
                            @<div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@kvRemark.Value</div>
                                <div class="grid-condition-col-multi">
                                    <span class="clearable">
                                        @Html.TextBox(kvRemark.Key, dicRemarkValue(kvRemark.Key), New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                    @*[CC431] UPD Start 20261013 複数入力ダイアログの文字数（255）の制限を追加*@
                                    <button type="button" class="btn btn-default multi-input-button" data-multi-input="#@kvRemark.Key" data-multi-title="@kvRemark.Value" data-multi-maxlength="255">@Message.getMessage(CommonLabel.BUTTON_MULTI_INPUT)</button>
                                    @*[CC431] UPD End 20261013*@
                                </div>
                            </div>
                        Next
                    @*[CC431] ADD Start 20261015 col-md-12 の閉じタグ*@
                        </div>
                    @*[CC431] ADD End 20261015*@
                    </div>
                    <div Class="search-btn">
                        <input class="defButton submit-button" id="Search_Button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                    </div>
                </div>
            </div>
        </div>
        @<input type="hidden" id="DetailOpenFlg" name="DetailOpenFlg" value="@Model.FirstOrDefault().DetailOpenFlg" />
        @*[CC431] ADD Start 20261015*@
        @<input type="hidden" id="OpenFlg" name="OpenFlg" value="@Model.FirstOrDefault().OpenFlg" />
        @*[CC431] ADD End 20261015*@
        @<input type="hidden" id="IsRestoreOwnSearchConditions" name="IsRestoreOwnSearchConditions" value="@Model.FirstOrDefault().IsRestoreOwnSearchConditions.ToString()" />
        @<input type="hidden" id="FirstSearch" name="FirstSearch" value="@Model.FirstOrDefault().FirstSearch.ToString()" />

        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div class="list-view">
            <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
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
