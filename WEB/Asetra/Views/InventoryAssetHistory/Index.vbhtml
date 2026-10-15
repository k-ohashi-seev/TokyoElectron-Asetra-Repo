@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of InventoryAssetHistoryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelInventoryAssetHistory.LABEL_TITLE)
End Code
<script>
    //
    //  棚卸実績照会
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
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton";

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget");
        }

        // 大区分非選択時は中区分を非活性
        @*if (document.getElementById("SearchLocL").value == "") {  // 2020/03/17 nv miyaji upd*@
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocL").value == "") {  // 2020/03/17 nv miyaji upd
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        // 中区分非選択時は小区分を非活性
        @*if (document.getElementById("SearchLocM").value == "") {*@  // 2020/03/17 nv miyaji upd
        //[CC431] UPD Start 20261007
        // ※場所から選ぶ ON の場合は、小区分非選択時に中区分を非活性
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes" || (!isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocM").length == 0)) {  // 2020/03/17 nv miyaji upd
            clearMultiSelect("#SearchLocS");
            $("#SearchLocS").attr("disabled", "disabled");
        }
        if (isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocS").length == 0) {
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        //[CC431] UPD End 20261007

        //[CC431] ADD Start 20261009
        // 詳細検索エリアの開閉状態の復元（DetailOpenFlg が「▲詳細検索」のときだけ開いた状態にする。既定は閉）
        if ("@Model.FirstOrDefault.DetailOpenFlg" == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "flex");
            document.getElementById("DetailAccordion").innerHTML = "@Model.FirstOrDefault.DetailOpenFlg";
        }
        //[CC431] ADD End 20261009

        ////大区分コンボボックス非活性
        //$("#SearchLocL").addClass("dropdown-disabled");

        //列定義
        let columns = [
            { id: "@ColKey.InventoryAssetHistory.ASSET_NO", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_NO)', field: "ASSET_NO", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.ASSET_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.SUB_ASSET_NO", name: '@Message.getMessage(CommonLabel.LABEL_SUB_ASSET_NO)', field: "SUB_ASSET_NO", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.SUB_ASSET_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.ASSET_STATUS_NAME", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_STATUS)', field: "ASSET_STATUS_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.ASSET_STATUS_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_STATUS_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_STATUS_NAME").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.PHYINV_STATUS_NAME", name: '@Message.getMessage(CommonLabelInventoryAssetHistory.LABEL_JITSUTANA_STATUS)', field: "PHYINV_STATUS_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.PHYINV_STATUS_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_STATUS_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_STATUS_NAME").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.BOOK_LOCATION_L_CODE_NAME", name: '@Message.getMessage(CommonLabel.LABEL_BOOK_LOCATION_L_CODE_NAME)', field: "LOC_L_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.BOOK_LOCATION_L_CODE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_L_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_L_CODE_NAME").DispFlag", formatter: BookLocationLCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.BOOK_LOCATION_M_CODE_NAME", name: '@Message.getMessage(CommonLabel.LABEL_BOOK_LOCATION_M_CODE_NAME)', field: "LOC_M_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.BOOK_LOCATION_M_CODE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_M_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_M_CODE_NAME").DispFlag", formatter: BookLocationMCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.BOOK_LOCATION_S_CODE_NAME", name: '@Message.getMessage(CommonLabel.LABEL_BOOK_LOCATION_S_CODE_NAME)', field: "LOC_S_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.BOOK_LOCATION_S_CODE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_S_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_S_CODE_NAME").DispFlag", formatter: BookLocationSCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.PHYINV_LOCATION_L_CODE_NAME", name: '@Message.getMessage(CommonLabel.LABEL_PHYINV_LOCATION_L_CODE_NAME)', field: "PHYINV_LOC_L_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.PHYINV_LOCATION_L_CODE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_L_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_L_CODE_NAME").DispFlag", formatter: PhyinvLocationLCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.PHYINV_LOCATION_M_CODE_NAME", name: '@Message.getMessage(CommonLabel.LABEL_PHYINV_LOCATION_M_CODE_NAME)', field: "PHYINV_LOC_M_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.PHYINV_LOCATION_M_CODE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_M_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_M_CODE_NAME").DispFlag", formatter: PhyinvLocationMCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.PHYINV_LOCATION_S_CODE_NAME", name: '@Message.getMessage(CommonLabel.LABEL_PHYINV_LOCATION_S_CODE_NAME)', field: "PHYINV_LOC_S_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.PHYINV_LOCATION_S_CODE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_S_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_S_CODE_NAME").DispFlag", formatter: PhyinvLocationSCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.CAT_L_CD", name: '@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)', field: "CAT_L_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.CAT_L_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag", formatter: CategoryCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.IS_TANGIBLE", name: '@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)', field: "IS_TANGIBLE", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.IS_TANGIBLE", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("IS_TANGIBLE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("IS_TANGIBLE").DispFlag", formatter: IsTangibleFormatter },
            { id: "@ColKey.InventoryAssetHistory.ASSET_NAME", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)', field: "ASSET_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.ASSET_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.PHYINV_DATE", name: '@Message.getMessage(CommonLabelInventoryAssetHistory.LABEL_PHYINV_DATE)', field: "PHYINV_DATE", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.PHYINV_DATE", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_DATE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_DATE").DispFlag", formatter: PhyinvDateFormatter },
            { id: "@ColKey.InventoryAssetHistory.PHYINV_PERSON_NAME", name: '@Message.getMessage(CommonLabelInventoryAssetHistory.LABEL_PHYINV_PERSON_NAME)', field: "PHYINV_EMPLOYEE_NAME", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.PHYINV_PERSON_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_PERSON_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_PERSON_NAME").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM1", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM1)', field: "FREE_ITEM1", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM1", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM2", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)', field: "FREE_ITEM2", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM2", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM3", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM3)', field: "FREE_ITEM3", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM3", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM4", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM4)', field: "FREE_ITEM4", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM4", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM5", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM5)', field: "FREE_ITEM5", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM5", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM6", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6)', field: "FREE_ITEM6", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM6", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispFlag", formatter:FreeItem6CodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM7", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM7)', field: "FREE_ITEM7", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM7", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM8", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM8)', field: "FREE_ITEM8", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM8", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM9", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM9)', field: "FREE_ITEM9", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM9", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM10", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM10)', field: "FREE_ITEM10", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM10", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM11", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM11)', field: "FREE_ITEM11", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM11", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispFlag", formatter: CommaFormatter, cssClass: "cell-right" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM12", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM12)', field: "FREE_ITEM12", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM12", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispFlag", formatter: CommaFormatter, cssClass: "cell-right" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM13", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM13)', field: "FREE_ITEM13", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM13", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM14", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM14)', field: "FREE_ITEM14", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM14", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM15", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)', field: "FREE_ITEM15", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM15", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM16", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM16)', field: "FREE_ITEM16", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM16", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM17", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM17)', field: "FREE_ITEM17", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM17", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM18", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)', field: "FREE_ITEM18", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM18", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM19", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)', field: "FREE_ITEM19", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM19", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM20", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM20)', field: "FREE_ITEM20", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM20", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM20").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM20").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM21", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM21)', field: "FREE_ITEM21", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM21", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM21").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM21").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM22", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM22)', field: "FREE_ITEM22", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM22", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM22").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM22").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM23", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM23)', field: "FREE_ITEM23", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM23", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM23").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM23").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM24", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM24)', field: "FREE_ITEM24", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM24", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM24").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM24").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM25", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM25)', field: "FREE_ITEM25", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM25", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM25").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM25").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM26", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM26)', field: "FREE_ITEM26", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM26", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM26").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM26").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM27", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM27)', field: "FREE_ITEM27", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM27", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM27").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM27").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM28", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM28)', field: "FREE_ITEM28", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM28", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM28").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM28").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM29", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM29)', field: "FREE_ITEM29", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM29", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM29").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM29").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM30", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM30)', field: "FREE_ITEM30", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM30", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM30").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM30").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM31", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM31)', field: "FREE_ITEM31", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM31", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM31").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM31").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM32", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM32)', field: "FREE_ITEM32", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM32", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM32").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM32").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM33", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM33)', field: "FREE_ITEM33", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM33", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM33").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM33").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM34", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM34)', field: "FREE_ITEM34", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM34", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM34").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM34").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM35", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM35)', field: "FREE_ITEM35", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM35", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM35").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM35").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM36", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM36)', field: "FREE_ITEM36", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM36", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM36").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM36").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM37", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM37)', field: "FREE_ITEM37", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM37", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM37").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM37").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM38", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM38)', field: "FREE_ITEM38", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM38", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM38").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM38").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM39", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM39)', field: "FREE_ITEM39", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM39", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM39").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM39").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM40", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM40)', field: "FREE_ITEM40", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM40", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM40").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM40").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM41", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM41)', field: "FREE_ITEM41", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM41", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM41").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM41").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM42", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM42)', field: "FREE_ITEM42", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM42", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM42").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM42").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM43", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM43)', field: "FREE_ITEM43", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM43", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM43").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM43").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM44", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM44)', field: "FREE_ITEM44", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM44", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM44").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM44").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM45", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM45)', field: "FREE_ITEM45", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM45", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM45").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM45").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM46", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM46)', field: "FREE_ITEM46", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM46", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM46").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM46").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM47", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM47)', field: "FREE_ITEM47", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM47", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM47").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM47").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM48", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM48)', field: "FREE_ITEM48", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM48", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM48").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM48").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM49", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM49)', field: "FREE_ITEM49", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM49", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM49").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM49").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM50", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM50)', field: "FREE_ITEM50", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM50", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM50").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM50").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM51", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM51)', field: "FREE_ITEM51", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM51", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM51").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM51").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM52", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM52)', field: "FREE_ITEM52", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM52", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM52").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM52").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM53", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM53)', field: "FREE_ITEM53", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM53", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM53").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM53").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM54", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM54)', field: "FREE_ITEM54", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM54", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM54").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM54").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM55", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM55)', field: "FREE_ITEM55", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM55", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM55").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM55").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM56", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM56)', field: "FREE_ITEM56", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM56", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM56").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM56").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM57", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM57)', field: "FREE_ITEM57", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM57", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM57").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM57").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM58", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM58)', field: "FREE_ITEM58", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM58", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM58").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM58").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM59", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM59)', field: "FREE_ITEM59", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM59", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM59").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM59").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.FREE_ITEM60", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM60)', field: "FREE_ITEM60", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.FREE_ITEM60", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM60").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM60").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.TAG_LINK_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_TAG_LINK_STATUS)", field: "TAG_LINK_STATUS", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.TAG_LINK_STATUS", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("TAG_LINK_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TAG_LINK_STATUS").DispFlag", formatter: TagLinkStatusFormatter},
            { id: "@ColKey.InventoryAssetHistory.OPE_STATUS", name: '@Message.getMessage(CommonLabel.LABEL_OPE_STATUS)', field: "OPE_STATUS", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.OPE_STATUS", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("OPE_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("OPE_STATUS").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.DIVERSION_REMOVE", name: '@Message.getMessage(CommonLabel.LABEL_DIVERSION_OR_REMOVE)', field: "DIVERSION_REMOVE", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.DIVERSION_REMOVE", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("DIVERSION_REMOVE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DIVERSION_REMOVE").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMOVE_DATE", name: '@Message.getMessage(CommonLabel.LABEL_REMOVE_DATE)', field: "REMOVE_DATE", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMOVE_DATE", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMOVE_DATE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMOVE_DATE").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMOVE_REASON", name: '@Message.getMessage(CommonLabel.LABEL_REMOVE_REASON)', field: "REMOVE_REASON", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMOVE_REASON", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMOVE_REASON").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMOVE_REASON").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMOVE_CONTENT", name: '@Message.getMessage(CommonLabel.LABEL_REMOVE_CONTENT)', field: "REMOVE_CONTENT", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMOVE_CONTENT", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMOVE_CONTENT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMOVE_CONTENT").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.CHANGE_LOC_M_CD", name: '@Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_M_CD_NAME)', field: "CHANGE_LOC_M_CD", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.CHANGE_LOC_M_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_M_CD").DispFlag", formatter:ChangeLocMCdCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.CHANGE_LOC_S_CD", name: '@Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_S_CD_NAME)', field: "CHANGE_LOC_S_CD", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.CHANGE_LOC_S_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_S_CD").DispFlag", formatter: ChangeLocSCdCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.CHANGE_EMPLOYEE", name: '@Message.getMessage(CommonLabel.LABEL_CHANGE_EMPLOYEE_CD_NAME)', field: "CHANGE_EMPLOYEE", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.CHANGE_EMPLOYEE", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CHANGE_EMPLOYEE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHANGE_EMPLOYEE").DispFlag", formatter: ChangeEmployeeCodeNameFormatter },
            { id: "@ColKey.InventoryAssetHistory.REMARK1", name: '@Message.getMessage(CommonLabel.LABEL_REMARK1)', field: "REMARK1", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK1", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK1").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMARK2", name: '@Message.getMessage(CommonLabel.LABEL_REMARK2)', field: "REMARK2", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK2", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK2").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMARK3", name: '@Message.getMessage(CommonLabel.LABEL_REMARK3)', field: "REMARK3", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK3", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK3").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMARK4", name: '@Message.getMessage(CommonLabel.LABEL_REMARK4)', field: "REMARK4", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK4", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK4").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMARK5", name: '@Message.getMessage(CommonLabel.LABEL_REMARK5)', field: "REMARK5", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK5", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK5").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMARK6", name: '@Message.getMessage(CommonLabel.LABEL_REMARK6)', field: "REMARK6", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK6", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK6").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMARK7", name: '@Message.getMessage(CommonLabel.LABEL_REMARK7)', field: "REMARK7", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK7", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK7").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMARK8", name: '@Message.getMessage(CommonLabel.LABEL_REMARK8)', field: "REMARK8", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK8", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK8").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMARK9", name: '@Message.getMessage(CommonLabel.LABEL_REMARK9)', field: "REMARK9", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK9", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK9").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.REMARK10", name: '@Message.getMessage(CommonLabel.LABEL_REMARK10)', field: "REMARK10", width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.REMARK10", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK10").DispFlag" },
            //[CC431] ADD Start 20261009
            { id: "@ColKey.InventoryAssetHistory.PHYINV_TOOL_DIV", name: '@Message.getMessage(CommonLabel.LABEL_INV_TOOL_DIV)', field: "PHYINV_TOOL_DIV", formatter: PhyinvToolDivFormatter, width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.PHYINV_TOOL_DIV", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_TOOL_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_TOOL_DIV").DispFlag" },
            { id: "@ColKey.InventoryAssetHistory.ENT_EMPLOYEE", name: '@Message.getMessage(CommonLabel.LABEL_INPUT_EMPLOYEE_CD_NAME)', field: "ENT_EMPLOYEE_CD", formatter: EntEmployeeFormatter, width: getColumnWidth("@DispId.INVENTORY_ASSET_HISTORY", "@ColKey.InventoryAssetHistory.ENT_EMPLOYEE", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("ENT_EMPLOYEE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ENT_EMPLOYEE").DispFlag" },
            //[CC431] ADD End 20261009

        ]

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

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol       //選択列を加算
        }

        let data = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.INVENTORY_ASSET_HISTORY", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, "@DispId.INVENTORY_ASSET_HISTORY", fixCol);
        }

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

    });

    //実棚ステータス用formatter
    function AssetNoFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            if (dataContext.PHYINV_STATUS == 'E') {
                return "<span style='color: red;'>" + value + "</span>"
            } else {
                return value
            }

        }
    }

    //帳簿ロケーション大区分コード/名用formatter BOOK_LOCATION_L_CODE_NAME
    function BookLocationLCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.LOC_L_CD + ' ' + value
        }
    }

    //帳簿ロケーション中区分コード/名用formatter
    function BookLocationMCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.LOC_M_CD + ' ' + value
        }
    }

    //帳簿ロケーション小区分コード/名用formatter
    function BookLocationSCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.LOC_S_CD + ' ' + value
        }
    }

    //実棚ロケーション大区分コード/名用formatter
    function PhyinvLocationLCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.PHYINV_LOC_L_CD + ' ' + value
        }
    }

    //実棚ロケーション中区分コード/名用formatter
    function PhyinvLocationMCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.PHYINV_LOC_M_CD + ' ' + value
        }
    }

    //実棚ロケーション小区分コード/名用formatter
    function PhyinvLocationSCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.PHYINV_LOC_S_CD + ' ' + value
        }
    }

    //カテゴリ大コード/名用formatter
    function CategoryCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.CAT_L_CD + ' ' + value
        }
    }

    // タグ紐付け状態用formatter
    function TagLinkStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (String(dataContext.TAG_LINK_STATUS) === "0") {
            status = "@Message.getMessage(CommonLabel.VALUE_MI)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_SUMI)"
        }

        return status;
    }

    //管理者社員/名用formatter
    function FreeItem6CodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return value + ' ' + dataContext.FREE_ITEM6_NAME
        }
    }

    //変更後課コード/名用formatter
    function ChangeLocMCdCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return value + ' ' + dataContext.CHANGE_LOC_M_NAME
        }
    }

    //変更後設置場所コード/名用formatter
    function ChangeLocSCdCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return value + ' ' + dataContext.CHANGE_LOC_S_NAME
        }
    }

    //[CC431] ADD Start 20261009
    // 棚卸実施端末（H:HT、W:Web）。想定外の値はそのまま表示する
    function PhyinvToolDivFormatter(row, cell, value, columnDef, dataContext) {
        if (value == "@ProcessToolDiv.HT") {
            return "<div>@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)</div>";
        } else if (value == "@ProcessToolDiv.Web") {
            return "<div>@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)</div>";
        }
        return "<div>" + convertToEmptyIfNull(value) + "</div>";
    }

    // 入力作業者コード/名（名称は社員M）。社員Mにないコードもコードは表示する
    function EntEmployeeFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(value) == "") {
            return "";
        }
        return "<div>" + value + " " + convertToEmptyIfNull(dataContext.ENT_EMPLOYEE_NAME) + "</div>";
    }

    // カンマ区切り（取得金額、当月末簿価）。数値でない値はそのまま表示する（AssetList と同じ）
    function CommaFormatter(row, cell, value, columnDef, dataContext) {
        var sValue = convertToEmptyIfNull(value);
        if (sValue == '') {
            return '';
        }
        var nValue = Number(sValue);
        if (isNaN(nValue)) {
            return sValue;
        }
        var aParts = sValue.split('.');
        var nDecimals = (aParts.length > 1) ? aParts[1].length : 0;
        return nValue.toLocaleString('ja-JP', { minimumFractionDigits: nDecimals, maximumFractionDigits: nDecimals });
    }
    //[CC431] ADD End 20261009

    //変更後使用社員番号/名用formatter
    function ChangeEmployeeCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return value + ' ' + dataContext.CHANGE_EMPLOYEE_NAME
        }
    }

    //有形・無形区分用formatter
    function IsTangibleFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";
        switch (String(dataContext.IS_TANGIBLE)) {
            case "@TANGIBLE.TANGIBLE":
                status = "@Message.getMessage(CommonLabel.LABEL_TANGIBLE_ASSET_NAME)"
                break;
            case "@TANGIBLE.NOT_TANGIBLE":
                status = "@Message.getMessage(CommonLabel.LABEL_INTANGIBLE_ASSET_NAME)"
                break;
        }
        return  status ;
    }

    //実棚日時用formatter  status.substr(0, 19)PHYINV_DATE
    function PhyinvDateFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value).toString()
        if (status != '') {
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            let hhmm = status.substr(8, 2) + ':' + status.substr(10, 2)
            return yyyy + '/' + mm + '/' + dd + ' ' + hhmm
        }
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick(startDecTs) {
        //確認メッセージ
        return showConfirmFileDownload("@Message.getMessage(CommonLabel.MSG_ASSET_CSVOUTPUT)", "@Url.Content("~/InventoryAssetHistory")/CsvExport" + "?startDecTs=" + startDecTs);
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        //[CC431] ADD Start 20261009
        // 日付範囲の前後チェック（From・To の両方が入力されていて From＞To の場合はエラー。片方のみの指定は可）
        // ※type="date" の値は yyyy-MM-dd 固定のため、文字列比較で前後を判定できる
        var dateRangeChecks = [
            { from: "SearchPhyinvDateFrom", to: "SearchPhyinvDateTo", label: "@Message.getMessage(CommonLabel.LABEL_PHYINV_DATE_SEARCH)" },
            { from: "SearchFreeItem2From", to: "SearchFreeItem2To", label: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)" },
            { from: "SearchFreeItem18From", to: "SearchFreeItem18To", label: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)" },
            { from: "SearchFreeItem19From", to: "SearchFreeItem19To", label: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)" }
        ];
        for (var i = 0; i < dateRangeChecks.length; i++) {
            var chk = dateRangeChecks[i];
            var dateFrom = document.getElementById(chk.from).value;
            var dateTo = document.getElementById(chk.to).value;
            if (dateFrom != "" && dateTo != "" && dateFrom > dateTo) {
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_DATE_ORDER_ITEM)".format(chk.label), "#" + chk.from);
                return false;
            }
        }
        //[CC431] ADD End 20261009

        //画面ブロックを表示
        showLoading();

        //ページ番号のリセット
        ResetPageNum();

        //20230904 ADD Start Shirakawa
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryAssetHistory")/SearchClick',
        });
        //20230904 ADD End Shirakawa

        return true;
    }

    //[CC431] ADD Start 20261009
    //詳細検索エリアの開閉切り替え
    //  リンクの文言（▼詳細検索／▲詳細検索）を現在の状態として扱い、クリックのたびに反転させる。
    //  開閉状態は hidden 項目 DetailOpenFlg に退避し、検索・ページング後も同じ状態で復元する。
    //  閉じていても入力欄は form 内にあるため、検索条件として送信される。
    function DetailAccordion_onclick() {
        if (document.getElementById("DetailAccordion").innerHTML == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "none");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)"
        } else {
            $(".detail-accordion").css("display", "flex");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)"
        }
        document.getElementById("DetailOpenFlg").value = document.getElementById("DetailAccordion").innerText;
    }
    //[CC431] ADD End 20261009

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        //画面ブロックを表示
        showLoading();

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //検索条件の権限ラジオボタンクリック処理
    function rdoSelect_onclick(element) {
        //クリックされたラジオボタンの値を隠し項目に格納する
        document.getElementById("SearchKengen").value = element.value;
    }

    //大区分選択
    //※中区分・小区分は複数選択のため、選択値の操作は getMultiSelectValues / setMultiSelectValues / clearMultiSelect（Scripts\select2-common.js）を使用
    function ChangeLocationL(locL) {
        // 中区分をクリア
        clearMultiSelect("#SearchLocM");
        $("#SearchLocM").prop("disabled", true);
        // 小区分をクリア
        clearMultiSelect("#SearchLocS");
        $("#SearchLocS").prop("disabled", true);

        // 選択されていない場合はここで終了
        if (!locL) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryAssetHistory")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationM", selectLCode: locL, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocM = document.getElementById("SearchLocM");
                    SearchLocM.options.length = 0;

                    // 複数選択のため選択解除用の空の選択肢は不要
                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocM.appendChild(op);
                    }
                }
                // 中区分データ取得
                $("#SearchLocM").removeAttr("disabled");
                // 選択肢の入れ替えをSelect2に反映（未選択）
                clearMultiSelect("#SearchLocM");
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //中区分選択
    function ChangeLocationM(locM) {
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
            url: '@Url.Content("~/InventoryAssetHistory")/ChangeLocation',
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

                    // 複数選択のため選択解除用の空の選択肢は不要
                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocS.appendChild(op);
                    }
                }
                // 小区分データ取得
                $("#SearchLocS").removeAttr("disabled");
                // 選択肢の入れ替えをSelect2に反映（未選択）
                clearMultiSelect("#SearchLocS");
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
            url: '@Url.Content("~/InventoryAssetHistory")/GetSearchLocationOptions',
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

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

</script>

<div id="inventory-asset-history">
    @Using (Html.BeginForm("Index", "InventoryAssetHistory", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    @* CSV出力 *@
                    <input id="csvBtn" class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="return btnCsvExport_onclick(@Model.FirstOrDefault.SelectStartDecTs)">
                    @* 戻る *@
                    <input class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "InventoryHistory")'">
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
                        @* condition-flex：複数選択で項目の高さが伸びても段が崩れないよう flex で並べる（Content\Site.css） *@
                        <div class="col-md-12 condition-flex">
                            @* ロケーション 大区分 *@
                            <div class="col-sm-2 filter-combo-area" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L)</div>
                                <div Class="grid-condition-col">
                                    @* select2-clearable クラスで入力して絞り込めるSelect2になる（Scripts\select2-common.js）。選択肢・選択後とも「コード 名称」を表示 *@
                                    @* ×で選択解除するため、空の選択肢を追加している（本画面は棚卸対象のロケーション固定のため非活性） *@
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocL, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeLocationL(this.value)", .class = "cond-control select2-clearable", .disabled = "disabled"})
                                </div>
                            </div>
                            @* ロケーション 中区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                            <div class="col-sm-2 combo-area" style="display: block">
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
                            <div class="col-sm-2 combo-area" style="display: block">
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
                            @* カテゴリー 大分類（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                            <div class="col-sm-2 combo-area" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                                <div class="grid-condition-col">
                                    @* ListBoxFor で複数選択（select multiple） *@
                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), New With {.class = "cond-control select2-clearable"})
                                </div>
                            </div>
                            @* 実棚ステータス *@
                            <div style="display: block">
                                <div class="col-sm-3" style="min-width:405px; height: 56px;">
                                    <div class="grid-head-col">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_JITSUTANA_STATUS)</div>
                                    <div class="grid-condition-col">
                                        <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchPhyinvStatusSelect, "ALL") @Message.getMessage(CommonLabel.LABEL_SELECT_ALL)</div>
                                        <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchPhyinvStatusSelect, PHYINV_STATUS.Normal) @Message.getMessage(CommonLabel.VALUE_JITSUTANA_STATUS_SEIJO)</div>
                                        <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchPhyinvStatusSelect, PHYINV_STATUS.Warning) @Message.getMessage(CommonLabel.VALUE_JITSUTANA_STATUS_ARI)</div>
                                        <div class="input-select-item-last" style="color:#ff0000;">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchPhyinvStatusSelect, PHYINV_STATUS.NoSpot) @Message.getMessage(CommonLabel.VALUE_JITSUTANA_STATUS_NASHI)</div>
                                    </div>
                                </div>
                            </div>
                            @*[CC431] ADD Start 20261009*@
                            @* 実棚日（日付範囲。From のみ・To のみの指定も可） *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PHYINV_DATE_SEARCH)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvDateFrom, New With {.class = "cond-control", .type = "date"})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvDateTo, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            @* 実棚者コード *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PHYINV_EMPLOYEE_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvEmployee, New With {.class = "cond-control", .maxlength = 15})
                                </div>
                            </div>
                            @* 棚卸実施端末 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_INV_TOOL_DIV)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchPhyinvToolDiv, DirectCast(ViewBag.PhyinvToolDivList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @*[CC431] ADD End 20261009*@
                            @* 資産番号 *@
                            <div class="col-sm-2" style="display: block">
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
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>
                            @* 有形・無形 *@
                            <div class="col-sm-2" style="display: block; height: 56px;">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)</div>
                                <div class="grid-condition-col">
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchTangibleCheck)@Message.getMessage(CommonLabel.LABEL_TANGIBLE_ASSET_NAME)</div>
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchNotTangibleCheck)@Message.getMessage(CommonLabel.LABEL_INTANGIBLE_ASSET_NAME)</div>
                                </div>
                            </div>
                            @* 貸与資産状態区分 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM10)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchFreeItem10, DirectCast(ViewBag.FreeItem10List, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @* 資本化日付 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)</div>
                                @*[CC431] UPD Start 20261009*@
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem2From, New With {.class = "cond-control", .type = "date"})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem2To, New With {.class = "cond-control", .type = "date"})
                                </div>
                                @*[CC431] UPD End 20261009*@
                            </div>
                            @* 稼働区分名 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM4)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem4, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 型式 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM5)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem5, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 管理社員番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem6, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* シリアルNo *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM7)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem7, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* WBS番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM8)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem8, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 備考 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM9)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem9, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 取得金額 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM11)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem11From, New With {.htmlAttributes = New With {.max = "999999999999999999999.99", .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                    @Html.Raw("～")
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem11To, New With {.htmlAttributes = New With {.max = "999999999999999999999.99", .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                </div>
                            </div>
                            @* 当月末簿価 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM12)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem12From, New With {.htmlAttributes = New With {.max = "999999999999999999999.99", .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                    @Html.Raw("～")
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem12To, New With {.htmlAttributes = New With {.max = "999999999999999999999.99", .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                </div>
                            </div>
                            @* 装置管理番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM14)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem14, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 資産グループコード *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem15, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 貸与先 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM16)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem16, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 貸与先名 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM17)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem17, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 開始日(貸与) *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)</div>
                                @*[CC431] UPD Start 20261009*@
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem18From, New With {.class = "cond-control", .type = "date"})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem18To, New With {.class = "cond-control", .type = "date"})
                                </div>
                                @*[CC431] UPD End 20261009*@
                            </div>
                            @* 終了日(貸与) *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)</div>
                                @*[CC431] UPD Start 20261009*@
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem19From, New With {.class = "cond-control", .type = "date"})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem19To, New With {.class = "cond-control", .type = "date"})
                                </div>
                                @*[CC431] UPD End 20261009*@
                            </div>
                            @* 貸与回数 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM20)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem20From, New With {.class = "cond-control", .oninput = "editNumber(this)", .style = "text-align: right;", .maxlength = 2})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem20To, New With {.class = "cond-control", .oninput = "editNumber(this)", .style = "text-align: right;", .maxlength = 2})
                                </div>
                            </div>
                            @* 備考欄 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM21)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem21, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 市区町村コード名称 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM39)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem39, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* CAPS番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM42)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem42, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 資産リース区分 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM37)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem37, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* タグ紐付状態 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TAG_LINK_STATUS)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                                    @* 選択肢の「全て」の値が空のため、選択解除用の空の選択肢は追加しない（×で「全て」に戻る。「全て」選択中は×を表示しない） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchTagLinkStatus, DirectCast(ViewBag.TagLinkStatusList, IEnumerable(Of SelectListItem)), New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @*[CC431] ADD Start 20261009*@
                            @* 棚卸ノート（部分一致） *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM27)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem27, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 入力作業者（前方一致） *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_INPUT_EMPLOYEE_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchEntEmployee, New With {.class = "cond-control", .maxlength = 15, .oninput = "editHanEi(this)"})
                                </div>
                            </div>
                            @*[CC431] ADD End 20261009*@
                        </div>
                    </div>
                    @*[CC431] ADD Start 20261009*@
                    @* 並び順（ラジオボタン17種。「項目の文言＋昇順の文言」で表示する。選択値は SearchSortOrder、Controller の GetSortColumn で ORDER BY に変換） *@
                    <div class="row" id="item-sort">
                        <div class="col-md-12">
                            <div class="grid-head-col sort-head-col">@Message.getMessage(CommonLabel.LABEL_SORT_ORDER)</div>
                            <div id="search-sort" class="grid-condition-col sort-condition-col" style="margin-left: 15px; margin-right: 15px; width: calc(100% - 30px);">
                                @Code
                                    Dim sSortAsc As String = Message.getMessage(CommonLabel.LABEL_SORT_ASC)
                                    Dim lstSort As New List(Of KeyValuePair(Of Integer, String)) From {
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.CatLAsc, Message.getMessage(CommonLabel.LABEL_CAT_L)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.AssetNoAsc, Message.getMessage(CommonLabel.LABEL_ASSET_NO)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.LocMAsc, Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_CODE)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.LocSAsc, Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_CODE)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.FreeItem2Asc, Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.FreeItem6Asc, Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6_CD)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.PhyinvEmployeeAsc, Message.getMessage(CommonLabel.LABEL_PHYINV_EMPLOYEE_CD)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark1Asc, Message.getMessage(CommonLabel.LABEL_REMARK1)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark2Asc, Message.getMessage(CommonLabel.LABEL_REMARK2)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark3Asc, Message.getMessage(CommonLabel.LABEL_REMARK3)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark4Asc, Message.getMessage(CommonLabel.LABEL_REMARK4)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark5Asc, Message.getMessage(CommonLabel.LABEL_REMARK5)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark6Asc, Message.getMessage(CommonLabel.LABEL_REMARK6)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark7Asc, Message.getMessage(CommonLabel.LABEL_REMARK7)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark8Asc, Message.getMessage(CommonLabel.LABEL_REMARK8)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark9Asc, Message.getMessage(CommonLabel.LABEL_REMARK9)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark10Asc, Message.getMessage(CommonLabel.LABEL_REMARK10))
                                    }
                                End Code
                                @For Each kvSort As KeyValuePair(Of Integer, String) In lstSort
                                    @<div class="input-select-item">
                                        @Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchSortOrder, kvSort.Key)@(kvSort.Value & sSortAsc)
                                    </div>
                                Next
                            </div>
                        </div>
                    </div>
                    @* 詳細検索エリア（備考1～備考10。複数テキスト入力「|」区切り、部分一致）。クリックで表示／非表示を切り替え、初期は非表示 *@
                    <div class="row">
                        <div class="col-md-12">
                            <a href="#" id="DetailAccordion" onclick="DetailAccordion_onclick();return false;">@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)</a>
                        </div>
                    </div>
                    <div class="row detail-accordion" id="item-conditions-detail" style="display:none;">
                        <div class="col-md-12 condition-flex">
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
                                        @* 「|」区切りで複数入力可。×でクリア、複数ボタンで複数入力ダイアログを表示（Scripts\multi-input-dialog.js） *@
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
                        </div>
                    </div>
                    @*[CC431] ADD End 20261009*@
                    @* 検索ボタン *@
                    <div Class="view-area-label" style="height: 25px"></div>
                    <input Class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>
        @* 各数量表示 *@
        @<div Class="search-wrap">
            <table Class="detail-table" border="0">
                <tbody>
                    <tr Class="detail-row">
                      @*<td style="background-color:#ffffff; padding:3px; font-weight:bold; width:90px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_BOOK_QUANTITY) &nbsp;=</td>
                        <td class="quantity-col">@ViewBag.LogicInvNum</td> yamada*@
                    <td style="background-color:#ffffff; padding:3px; font-weight:bold; width:200px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_BOOK_QUANTITY) &nbsp;=&nbsp; @ViewBag.LogicInvNum</td>
                      @*<td style="background-color:#ffffff; padding:3px; font-weight:bold; width:70px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_NORMAL_QUANTITY) &nbsp;=</td>
                        <td class="quantity-col">@ViewBag.PhyinvNum</td> yamada*@
                    <td style="background-color:#ffffff; padding:3px; font-weight:bold; width:230px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_NORMAL_QUANTITY) &nbsp;=&nbsp; @ViewBag.PhyinvNum</td>
                      @*<td style="background-color:#ffffff; padding:3px; font-weight:bold; width:90px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_LOCATION_DIFF_QUANTITY) &nbsp;=</td>
                        <td class="quantity-col">@ViewBag.LocDiffNum</td> yamada*@
                    <td style="background-color:#ffffff; padding:3px; font-weight:bold; width:280px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_LOCATION_DIFF_QUANTITY) &nbsp;=&nbsp; @ViewBag.LocDiffNum</td>
                      @*<td style="background-color:#ffffff; padding:3px; font-weight:bold; width:90px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_NONE_QUANTITY) &nbsp;=</td>
                        <td class="quantity-col">@ViewBag.NoItemNum</td> yamada*@
                    <td style="background-color:#ffffff; padding:3px; font-weight:bold; width:270px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_NONE_QUANTITY) &nbsp;=&nbsp; @ViewBag.NoItemNum</td>
                </tr>
            </tbody>
        </table>
    </div>

    @*ページャー表示*@
    @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
    @<div class="list-view">
        <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
    </div>

    @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.SelectStartDecTs)
    @*[CC431] ADD Start 20261009*@
    @Html.HiddenFor(Function(model) model.FirstOrDefault.DetailOpenFlg)
    @*[CC431] ADD End 20261009*@

    End Using
</div>
