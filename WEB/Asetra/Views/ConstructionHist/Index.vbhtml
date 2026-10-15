@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of ConstructionHistViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelConstructionHist.TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
End Code
<script>
    //
    //  工事履歴一覧
    //
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            //エラーメッセージ表示後、ログイン画面に遷移
            return;
        }

        // オプション項目表示非表示設定
        if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            $("#cond").removeClass("in");
            $("#cond").addClass("collapse");
            document.getElementById("SearchCondition").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        // 列定義
        let columnsTop = [
            { id: "@ColKey.ConstructionHistTop.CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_CONST_NO)", field: "CONST_NO", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_NO", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_NO").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.CONST_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_START_DT)", field: "CONST_START_DT", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_END_DT)", field: "CONST_END_DT", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_LENT_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)", field: "CONST_LENT_START_DT", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_LENT_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_LENT_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)", field: "CONST_LENT_END_DT", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_LENT_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_STATUS)", field: "CONST_STATUS", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_STATUS", 70), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_STATUS").DispFlag", formatter: ConstStatusFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_CONTENTS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)", field: "CONST_CONTENTS", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_CONTENTS", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONTENTS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONTENTS").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.CONST_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_EMPLOYEE_NAME)", field: "CONST_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_KYOTEN_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.MNG_LOC_L_CD", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("MNG_LOC_L_CD").DispFlag", formatter: MngLocLFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO1", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO1)", field: "CONST_INFO1", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO1", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO1").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO2", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO2)", field: "CONST_INFO2", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO2", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO2").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO3", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO3)", field: "CONST_INFO3", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO3", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO3").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO3").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO4", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO4)", field: "CONST_INFO4", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO4", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO4").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO4").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO5", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO5)", field: "CONST_INFO5", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO5", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO5").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO5").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO6", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO6)", field: "CONST_INFO6", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO6", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO6").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO6").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO7", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO7)", field: "CONST_INFO7", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO7", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO7").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO7").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO8", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO8)", field: "CONST_INFO8", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO8", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO8").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO8").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO9", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO9)", field: "CONST_INFO9", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO9", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO9").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO9").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO10", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO10)", field: "CONST_INFO10", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO10", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO10").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO10").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO11", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO11)", field: "CONST_INFO11", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO11", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO11").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO11").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO12", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO12)", field: "CONST_INFO12", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO12", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO12").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO12").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO13", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO13)", field: "CONST_INFO13", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO13", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO13").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO13").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO14", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO14)", field: "CONST_INFO14", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO14", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO14").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO14").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO15", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO15)", field: "CONST_INFO15", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO15", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO15").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO15").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO16", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO16)", field: "CONST_INFO16", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO16", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO16").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO16").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO17", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO17)", field: "CONST_INFO17", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO17", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO17").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO17").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO18", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO18)", field: "CONST_INFO18", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO18", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO18").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO18").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO19", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO19)", field: "CONST_INFO19", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO19", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO19").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO19").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_INFO20", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO20)", field: "CONST_INFO20", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_INFO20", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO20").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO20").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistTop.PREV_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_PREV_TEMP_NUM)", field: "PREV_TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.PREV_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("PREV_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("PREV_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.CONST_IN_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_NUM)", field: "CONST_IN_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_IN_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_IN_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_IN_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.NOW_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_NOW_TEMP_NUM)", field: "NOW_TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.NOW_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("NOW_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("NOW_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.CONST_OUT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_NUM)", field: "CONST_OUT_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_OUT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_OUT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_OUT_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.CUR_LENT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_LENT_NUM)", field: "CUR_LENT_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CUR_LENT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CUR_LENT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CUR_LENT_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.CUR_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_TEMP_NUM)", field: "CUR_TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CUR_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CUR_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CUR_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.SITE_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_SITE_EMPLOYEE_NAME)", field: "SITE_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.SITE_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoTop("SITE_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("SITE_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.CONST_CONFIRM_TS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_TS)", field: "CONST_CONFIRM_TS", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_CONFIRM_TS", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_TS").DispFlag", formatter: YMDHMSFormatter },
            { id: "@ColKey.ConstructionHistTop.CONST_CONFIRM_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_EMPLOYEE_NAME)", field: "CONST_CONFIRM_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_CONFIRM_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstructionHistTop.CONST_CONFIRM_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_REMARK)", field: "CONST_CONFIRM_REMARK", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_TOP", "@ColKey.ConstructionHistTop.CONST_CONFIRM_REMARK", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_REMARK").DispFlag" },
        ]

        let columnsBottom = [
            { id: "@ColKey.ConstructionHistBottom.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CHECK_SELECT", 40), sortOrder: -2, formatter: ConstructionHistCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.ConstructionHistBottom.CONST_WORK_TS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_WORK_TS)", field: "CONST_WORK_TS", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_WORK_TS", 190), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_WORK_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_WORK_TS").DispFlag", formatter: YMDHMSFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_WORK_DIV", name: "@Message.getMessage(CommonLabel.LABEL_WORK_CONTENTS)", field: "CONST_WORK_DIV", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_WORK_DIV", 190), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_WORK_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_WORK_DIV").DispFlag", formatter: ConstWorkDivFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_MATCH_DIV", name: "@Message.getMessage(CommonLabel.LABEL_CONST_MATCH_DIV)", field: "CONST_MATCH_DIV", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_MATCH_DIV", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_MATCH_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_MATCH_DIV").DispFlag", formatter: ConstMatchDivFormatter },
            { id: "@ColKey.ConstructionHistBottom.LENT_NUM", name: "@Message.getMessage(CommonLabelInventorySummary.LABEL_NUMBER_OF_LOANS)", field: "LENT_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.LENT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LENT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LENT_NUM").DispFlag", formatter: LentNumFormatter },
            { id: "@ColKey.ConstructionHistBottom.TEMP_REG_NUM", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_REG_NUM)", field: "TEMP_REG_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.TEMP_REG_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_REG_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_REG_NUM").DispFlag", formatter: TempRegNumFormatter },
            { id: "@ColKey.ConstructionHistBottom.LEASE_NUM", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_NUM)", field: "LEASE_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.LEASE_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LEASE_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LEASE_NUM").DispFlag", formatter: LeaseNumFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_IN_MATCH_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_MATCH_NUM)", field: "CONST_IN_MATCH_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_IN_MATCH_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_IN_MATCH_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_IN_MATCH_NUM").DispFlag", formatter: ConstInMatchNumFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_OUT_MATCH_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_MATCH_NUM)", field: "CONST_OUT_MATCH_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_OUT_MATCH_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_OUT_MATCH_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_OUT_MATCH_NUM").DispFlag", formatter: ConstOutMatchNumFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_IN_NUM_HIST", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_NUM)", field: "CONST_IN_NUM_HIST", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_IN_NUM_HIST", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_IN_NUM_HIST").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_IN_NUM_HIST").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.ConstructionHistBottom.TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_NUM)", field: "TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_NUM").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_OUT_NUM_HIST", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_NUM)", field: "CONST_OUT_NUM_HIST", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_OUT_NUM_HIST", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_OUT_NUM_HIST").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_OUT_NUM_HIST").DispFlag", formatter: RightAlignFormatter },
        ]

        // メニューから遷移の場合
        if ("@ViewBag.ConstructionListKey" == "") {
            columnsBottom.push(
            { id: "@ColKey.ConstructionHistBottom.CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_CONST_NO)", field: "CONST_NO", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_NO", 190), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_NO").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.CONST_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_START_DT)", field: "CONST_START_DT", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_END_DT)", field: "CONST_END_DT", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_LENT_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)", field: "CONST_LENT_START_DT", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_LENT_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_LENT_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_LENT_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_LENT_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)", field: "CONST_LENT_END_DT", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_LENT_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_LENT_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_LENT_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_STATUS)", field: "CONST_STATUS", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_STATUS", 70), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_STATUS").DispFlag", formatter: ConstStatusFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_CONTENTS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)", field: "CONST_CONTENTS", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_CONTENTS", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONTENTS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONTENTS").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.CONST_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_EMPLOYEE_NAME)", field: "CONST_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_KYOTEN_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.MNG_LOC_L_CD", 190), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("MNG_LOC_L_CD").DispFlag", formatter: MngLocLFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO1", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO1)", field: "CONST_INFO1", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO1", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO1").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO2", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO2)", field: "CONST_INFO2", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO2", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO2").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO3", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO3)", field: "CONST_INFO3", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO3", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO3").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO3").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO4", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO4)", field: "CONST_INFO4", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO4", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO4").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO4").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO5", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO5)", field: "CONST_INFO5", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO5", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO5").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO5").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO6", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO6)", field: "CONST_INFO6", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO6", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO6").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO6").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO7", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO7)", field: "CONST_INFO7", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO7", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO7").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO7").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO8", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO8)", field: "CONST_INFO8", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO8", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO8").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO8").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO9", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO9)", field: "CONST_INFO9", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO9", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO9").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO9").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO10", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO10)", field: "CONST_INFO10", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO10", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO10").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO10").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO11", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO11)", field: "CONST_INFO11", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO11", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO11").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO11").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO12", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO12)", field: "CONST_INFO12", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO12", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO12").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO12").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO13", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO13)", field: "CONST_INFO13", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO13", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO13").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO13").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO14", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO14)", field: "CONST_INFO14", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO14", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO14").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO14").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO15", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO15)", field: "CONST_INFO15", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO15", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO15").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO15").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO16", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO16)", field: "CONST_INFO16", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO16", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO16").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO16").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO17", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO17)", field: "CONST_INFO17", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO17", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO17").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO17").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO18", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO18)", field: "CONST_INFO18", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO18", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO18").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO18").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO19", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO19)", field: "CONST_INFO19", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO19", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO19").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO19").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_INFO20", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO20)", field: "CONST_INFO20", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_INFO20", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO20").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO20").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionHistBottom.PREV_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_PREV_TEMP_NUM)", field: "PREV_TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.PREV_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("PREV_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("PREV_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.CONST_IN_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_NUM)", field: "CONST_IN_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_IN_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_IN_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_IN_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.NOW_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_NOW_TEMP_NUM)", field: "NOW_TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.NOW_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("NOW_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("NOW_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.CONST_OUT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_NUM)", field: "CONST_OUT_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_OUT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_OUT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_OUT_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.CUR_LENT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_LENT_NUM)", field: "CUR_LENT_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CUR_LENT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CUR_LENT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CUR_LENT_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.CUR_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_TEMP_NUM)", field: "CUR_TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CUR_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CUR_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CUR_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.SITE_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_SITE_EMPLOYEE_NAME)", field: "SITE_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.SITE_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("SITE_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("SITE_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.CONST_CONFIRM_TS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_TS)", field: "CONST_CONFIRM_TS", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_CONFIRM_TS", 190), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_TS").DispFlag", formatter: YMDHMSFormatter },
            { id: "@ColKey.ConstructionHistBottom.CONST_CONFIRM_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_EMPLOYEE_NAME)", field: "CONST_CONFIRM_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_CONFIRM_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstructionHistBottom.CONST_CONFIRM_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_REMARK)", field: "CONST_CONFIRM_REMARK", width: getColumnWidth("@DispId.CONSTRUCTION_HIST_BOTTOM", "@ColKey.ConstructionHistBottom.CONST_CONFIRM_REMARK", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_REMARK").DispFlag" });
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
            autoHeight: true,                       //Gridの高さの自動調整可否
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonDataTop = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.tblConstructionListData))) ');
        let jsonDataBottom = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // 選択状態を復元
        let lstCheckedKey = @Html.Raw(jsonCheckKeys);
        for (let i = 0; i < lstCheckedKey.length; i++) {
            // Key1:工事番号 Key2:作業日時 key3:工事作業区分
            var keys = lstCheckedKey[i].split(",");

            for (let grid_i = 0; grid_i < jsonDataBottom.length; grid_i++) {
                if (keys[0] === jsonDataBottom[grid_i].CONST_NO && keys[1] === String(jsonDataBottom[grid_i].CONST_WORK_TS) && keys[2] === jsonDataBottom[grid_i].CONST_WORK_DIV) {
                    jsonDataBottom[grid_i].DISP_FLAG = lstCheckedKey[i]
                    break;
                }
            }
        }

        // Gridのデータ管理オブジェクトにセット
        let dataViewTop = new Slick.Data.DataView();
        let dataViewBottom = new Slick.Data.DataView();

        // 固定列を設定
        let fixColTop = Number("@Model.FirstOrDefault.FixColCntTop");
        let fixColBottom = Number("@Model.FirstOrDefault.FixColCntBottom");
        if (fixColBottom > 0) {
            fixColBottom = fixColBottom + 1       //選択列を加算
        }


        // Grid表示判定
        if ("@ViewBag.tblConstructionListData.Count" > 0) {
            dataViewTop.beginUpdate();
            dataViewTop.setItems(jsonDataTop, "CONST_NO");
            dataViewTop.endUpdate();
        }

        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataViewBottom.beginUpdate();
            dataViewBottom.setItems(jsonDataBottom, "KEY_VALUE");
            dataViewBottom.endUpdate();
        }

        // Grid初期化
        gridTop = initializeGrid("ListGridsTop", dataViewTop, columnsTop, optionsTop, "@DispId.CONSTRUCTION_HIST_TOP", fixColTop);
        gridBottom = initializeGrid("ListGridsBottom", dataViewBottom, columnsBottom, optionsBottom, "@DispId.CONSTRUCTION_HIST_BOTTOM", fixColBottom);

    });

    // チェックボックス(中央寄せ)
    function ConstructionHistCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.KEY_VALUE);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheck" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" ' +
               (isKeyInclude ? 'checked="checked" ' : '') +
               'onchange="check_onclick(this)" ' +
               '/>' +
               '</div>';
    }

    // 工事状態用formatter
    function ConstStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch (String(dataContext.CONST_STATUS)) {
            case "@CONST_STATUS.Incomplete":
                status = "@Message.getMessage(CommonLabel.VALUE_CONSTRUCTION_STATUS_INCOMPLETE)"
                break;
            case "@CONST_STATUS.Complete":
                status = "@Message.getMessage(CommonLabel.VALUE_CONSTRUCTION_STATUS_COMPLETE)"
                break;
            default:
                status = dataContext.CONST_STATUS;
                break;
        }

        return status;
    }

    // 工事情報用formatter
    function ConstInfoFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(value).replaceAll("$$$", "～");
    }

    // yyyyMMdd→yyyy/MM/dd表示用formatter
    function SlashYMDFormatter(row, cell, value, columnDef, dataContext) {
        let ret = "";
        if (value) {
            ret = toStringDateYMD(value);
        }
        return ret
    }

    // 管理ロケ-ションコード用formatter
    function MngLocLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.MNG_LOC_L_CD) + ' / ' + convertToEmptyIfNull(dataContext.MngLocLName);
    }

    // yyyyMMddHHmmss→yyyy/MM/dd HH:mm:ss表示用formatter
    function YMDHMSFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value).toString()
        if (status != '') {
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            let hhmmss = status.substr(8, 2) + ':' + status.substr(10, 2) + ':' + status.substr(12, 2)
            return yyyy + '/' + mm + '/' + dd + ' ' + hhmmss
        }
    }

    // 工事作業区分用formatter
    function ConstWorkDivFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch (String(dataContext.CONST_WORK_DIV)) {
            case "@ConstWorkDiv.ConstRegist":
                status = "@Message.getMessage(CommonLabel.VALUE_CONST_REGIST)"
                break;
            case "@ConstWorkDiv.TempLink":
                status = "@Message.getMessage(CommonLabel.VALUE_TEMP_LINK)"
                break;
            case "@ConstWorkDiv.Lease":
                status = "@Message.getMessage(CommonLabel.VALUE_LEASE)"
                break;
            case "@ConstWorkDiv.InMatch":
                status = "@Message.getMessage(CommonLabel.VALUE_IN_MATCH)"
                break;
            case "@ConstWorkDiv.TempRegist":
                status = "@Message.getMessage(CommonLabel.VALUE_TEMP_REGIST)"
                break;
            case "@ConstWorkDiv.OutMatch":
                status = "@Message.getMessage(CommonLabel.VALUE_OUT_MATCH)"
                break;
            case "@ConstWorkDiv.MatchReturn":
                status = "@Message.getMessage(CommonLabel.VALUE_MATCH_RETURN)"
                break;
            case "@ConstWorkDiv.Returned":
                status = "@Message.getMessage(CommonLabel.VALUE_RETURNED)"
                break;
            case "@ConstWorkDiv.ConstComplete":
                status = "@Message.getMessage(CommonLabel.VALUE_CONST_COMPLETE)"
                break;
            case "@ConstWorkDiv.AnotherConstLink":
                status = "@Message.getMessage(CommonLabel.VALUE_ANOTHER_CONST_LINK)"
                break;
            case "@ConstWorkDiv.TempRelease":
                status = "@Message.getMessage(CommonLabel.VALUE_TEMP_RELEASE)"
                break;
            default:
                status = dataContext.CONST_WORK_DIV;
                break;
        }

        return status;
    }

    // 照合結果区分用formatter
    function ConstMatchDivFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch (String(dataContext.CONST_WORK_DIV)) {
            case "@ConstWorkDiv.InMatch":
            case "@ConstWorkDiv.OutMatch":
            case "@ConstWorkDiv.MatchReturn":
                switch (String(dataContext.CONST_MATCH_DIV)) {
                    case "@ConstMatchDiv.None":
                        status = "@Message.getMessage(CommonLabel.VALUE_NONE_MATCH)"
                        break;
                    case "@ConstMatchDiv.Ok":
                        status = "@Message.getMessage(CommonLabel.VALUE_MATCH_OK)"
                        break;
                    case "@ConstMatchDiv.Ng":
                        status = "@Message.getMessage(CommonLabel.VALUE_MATCH_NG)"
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        return '<div style="text-align:center; width:100%;">' + status + '</div>';
    }

    // 貸出数用formatter
    function LentNumFormatter(row, cell, value, columnDef, dataContext) {
        let ret = "";

        switch (String(dataContext.CONST_WORK_DIV)) {
            case "@ConstWorkDiv.TempLink":
            case "@ConstWorkDiv.Lease":
                ret = String(dataContext.LENT_NUM)
                break;
            default:
                break;
        }

        return '<div style="text-align:right; width:100%;">' + ret + '</div>';
    }

    // 返却数用formatter
    function LeaseNumFormatter(row, cell, value, columnDef, dataContext) {
        let ret = "";

        switch (String(dataContext.CONST_WORK_DIV)) {
            case "@ConstWorkDiv.MatchReturn":
            case "@ConstWorkDiv.Returned":
                ret = String(dataContext.LEASE_NUM)
                break;
            default:
                break;
        }

        return '<div style="text-align:right; width:100%;">' + ret + '</div>';
    }

    // 仮置き登録数用formatter
    function TempRegNumFormatter(row, cell, value, columnDef, dataContext) {
        let ret = "";

        switch (String(dataContext.CONST_WORK_DIV)) {
            case "@ConstWorkDiv.TempRegist":
            case "@ConstWorkDiv.AnotherConstLink":
            case "@ConstWorkDiv.TempRelease":
                ret = String(dataContext.TEMP_REG_NUM)
                break;
            default:
                break;
        }

        return '<div style="text-align:right; width:100%;">' + ret + '</div>';
    }

    // 持込照合数用formatter
    function ConstInMatchNumFormatter(row, cell, value, columnDef, dataContext) {
        let ret = "";

        switch (String(dataContext.CONST_WORK_DIV)) {
            case "@ConstWorkDiv.InMatch":
                ret = String(dataContext.CONST_IN_MATCH_NUM) + "/" + String(dataContext.CONST_MATCH_TTLNUM)
                break;
            default:
                break;
        }

        return '<div style="text-align:right; width:100%;">' + ret + '</div>';
    }

    // 持帰照合数用formatter
    function ConstOutMatchNumFormatter(row, cell, value, columnDef, dataContext) {
        let ret = "";

        switch (String(dataContext.CONST_WORK_DIV)) {
            case "@ConstWorkDiv.OutMatch":
            case "@ConstWorkDiv.MatchReturn":
                ret = String(dataContext.CONST_OUT_MATCH_NUM) + "/" + String(dataContext.CONST_MATCH_TTLNUM)
                break;
            default:
                break;
        }

        return '<div style="text-align:right; width:100%;">' + ret + '</div>';
    }


    // 状態formatter
    function ConstAssetStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch (String(dataContext.CONST_ASSET_STATUS)) {
            case "@CONST_ASSET_STATUS.Lending":
                status = "@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)"
                break;
            case "@CONST_ASSET_STATUS.Reserve":
                status = "@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RESERVE)"
                break;
            case "@CONST_ASSET_STATUS.HenkyakuSumi":
                status = "@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)"
                break;
            default:
                status = dataContext.CONST_ASSET_STATUS;
                break;
        }

        return status;
    }

    // 仮置き場所用formatter
    function TempInfoFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.CONST_NO) + ',' + convertToEmptyIfNull(dataContext.ASSET_NO);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return convertToEmptyIfNull(dataContext.TEMP_INFO1).replace("$$$", "") + ' ' +
               convertToEmptyIfNull(dataContext.TEMP_INFO2).replace("$$$", "") + ' ' +
               convertToEmptyIfNull(dataContext.TEMP_INFO3).replace("$$$", "") + ' ' +
               convertToEmptyIfNull(dataContext.TEMP_INFO4).replace("$$$", "") + ' ' +
               convertToEmptyIfNull(dataContext.TEMP_INFO5).replace("$$$", "") + ' ' +
               convertToEmptyIfNull(dataContext.TEMP_INFO6).replace("$$$", "") + ' ' +
               convertToEmptyIfNull(dataContext.TEMP_INFO7).replace("$$$", "");
    }

    // 仮置き有無用formatter
    function TempYesOrNoFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (dataContext.TEMP_ID) {
            status = "@Message.getMessage(CommonLabel.VALUE_TEMP_EXISTS)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_TEMP_NOT_EXISTS)"
        }

        return status;
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        $('#form').submit();
    }

    // 詳細ボタンクリック処理
    function btnShowConstAssetHist_onclick() {
        // 選択行のキーを取得
        var keyValue = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(gridBottom.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                var row = checkBoxes[i].parentNode.parentNode;
                keyValue.push(checkBoxes[i].value);
            }
        }
        if (keyValue.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        } else if (keyValue.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }

        //実行
        SetKeys(keyValue[0]);

        return true;
    }

    //選択したチェックボックスのキーをセッションに保存します
    function SetKeys(keys) {
        // 工事資産履歴一覧画面に引き渡す資産番号を設定
        document.getElementById("SelectedKeys").value = keys;

        $('#form').attr("action", "@Url.Content("~/ConstructionHist")/SetKeys");
        $('#form').submit();

    }

    //CSV出力ボタンクリック処理
    function btnIchiranCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/ConstructionHist")/IchiranCsvExport");

    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        //ページ番号のリセット
        ResetPageNum();
        return true;
    }

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
        var checkBoxes = $(gridBottom.getContainerNode()).find(".slick-checkbox");

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

<div id="asset-detail">
    @Using (Html.BeginForm("Index", "ConstructionHist", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb10" style="width:100%">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    <div>
                        <input class="defButton detail-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DETAIL)" onclick="btnShowConstAssetHist_onclick()">
                    </div>
                </div>
                <div class="view-area-button-right" style="margin-right:20px">
                    <input class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnIchiranCsvExport_onclick()">
                    <input class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" style="display:@(If(String.IsNullOrEmpty(ViewBag.ConstructionListKey), "None", "display"))" onclick="location.href='@Url.Action("Index", "ConstructionList")'">
                </div>
            </div>
        </div>
        @<div>
            <div id="ListGridsTop" style="height:auto;padding-bottom: 20px;display:@(If(String.IsNullOrEmpty(ViewBag.ConstructionListKey), "None", "display"))" />
        </div>
        @* 検索条件 *@
        @<div class="search-wrap">
            <a id="SearchCondition" data-toggle="collapse" href="#cond" aria-expanded="true" onclick="SearchCondition_onclick()">
                @Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)
            </a>
            <div class="search-border">
                <div id="cond" class="container collapse in">
                    <div class="row" id="item-conditions">
                        <div class="col-md-12">
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_NO").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstNo, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_START_DT").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_START_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstStartDt, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_END_DT").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_END_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstEndDt, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_LENT_START_DT").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstLentStartDt, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_LENT_END_DT").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstLentEndDt, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_STATUS").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_STATUS)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchConstStatus, DirectCast(ViewBag.ConstStatusList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_CONTENTS").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstContents, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_EMPLOYEE_NAME").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_EMPLOYEE_NAME)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstEmployeeName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("MNG_LOC_L_CD").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_KYOTEN_CD_NAME)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchMngLocL, DirectCast(ViewBag.modelMngLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO1").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO1)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO1").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo1, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "1").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO2").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO2)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO2").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo2, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "2").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO3").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO3)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO3").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo3, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "3").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO4").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO4)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO4").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo4, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "4").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO5").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO5)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO5").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo5, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "5").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO6").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO6)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO6").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo6, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "6").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO7").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO7)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO7").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo7, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "7").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO8").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO8)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO8").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo8, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "8").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO9").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO9)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO9").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo9, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "9").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO10").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO10)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO10").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo10, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "10").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO11").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO11)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO11").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo11, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "11").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO12").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO12)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO12").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo12, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "12").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO13").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO13)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO13").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo13, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "13").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO14").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO14)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO14").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo14, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "14").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO15").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO15)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO15").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo15, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "15").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO16").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO16)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO16").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo16, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "16").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO17").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO17)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO17").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo17, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "17").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO18").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO18)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO18").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo18, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "18").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO19").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO19)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO19").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo19, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "19").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO20").DispFlag = "1" AndAlso String.IsNullOrEmpty(ViewBag.ConstructionListKey), "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO20)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfoBottom("CONST_INFO20").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo20, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "20").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_WORK_TS").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_WORK_TS)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstWorkTs, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_WORK_DIV").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_WORK_CONTENTS)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchConstWorkDiv, DirectCast(ViewBag.ConstWorkDivList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_MATCH_DIV").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_MATCH_DIV)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchConstMatchDiv, DirectCast(ViewBag.ConstMatchDivList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="view-area-label" style="height: 25px"></div>
                    <input class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>
        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @Html.Label(Message.getMessage(CommonLabel.LABEL_SELECT_COUNT), New With {.style = "font-size:12px;"})
        @Code
            Dim lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
            Dim CheckedCount As String = "0"
            If Not IsNothing(lstCheckedKey) Then
                CheckedCount = lstCheckedKey.Count.ToString
            End If
            @Html.Label(CheckedCount, New With {.id = "selectcount", .style = "margin-right:15px;font-size:12px;"})
        End Code

        @<div class="list-view">
            <div id="ListGridsBottom" style="height:auto;padding-bottom: 20px;" />
        </div>

            'チェックボックス選択値の引き渡し用隠し項目
        @<input id="SelectKeys" name="SelectKeys" type="hidden">
        @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden">
            '工事資産履歴一覧画面遷移時、チェックボックスキー引き渡し用隠し項目
        @<input id="SelectedKeys" name="SelectedKeys" type="hidden">

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OpenFlg)

        @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_TASK)
    End Using
</div>
