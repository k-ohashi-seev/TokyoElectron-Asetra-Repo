@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of ConstAssetHistViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelConstAssetHist.TITLE)
End Code
<script>
    //
    //  工事資産履歴一覧
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
            { id: "@ColKey.ConstAssetHistTop.CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_CONST_NO)", field: "CONST_NO", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_NO", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_NO").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.CONST_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_START_DT)", field: "CONST_START_DT", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_END_DT)", field: "CONST_END_DT", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_LENT_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)", field: "CONST_LENT_START_DT", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_LENT_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_LENT_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)", field: "CONST_LENT_END_DT", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_LENT_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_STATUS)", field: "CONST_STATUS", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_STATUS", 70), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_STATUS").DispFlag", formatter: ConstStatusFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_CONTENTS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)", field: "CONST_CONTENTS", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_CONTENTS", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONTENTS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONTENTS").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.CONST_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_EMPLOYEE_NAME)", field: "CONST_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_KYOTEN_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.MNG_LOC_L_CD", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("MNG_LOC_L_CD").DispFlag", formatter: MngLocLFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO1", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO1)", field: "CONST_INFO1", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO1", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO1").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO2", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO2)", field: "CONST_INFO2", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO2", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO2").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO3", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO3)", field: "CONST_INFO3", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO3", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO3").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO3").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO4", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO4)", field: "CONST_INFO4", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO4", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO4").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO4").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO5", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO5)", field: "CONST_INFO5", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO5", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO5").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO5").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO6", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO6)", field: "CONST_INFO6", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO6", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO6").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO6").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO7", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO7)", field: "CONST_INFO7", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO7", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO7").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO7").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO8", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO8)", field: "CONST_INFO8", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO8", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO8").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO8").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO9", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO9)", field: "CONST_INFO9", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO9", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO9").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO9").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO10", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO10)", field: "CONST_INFO10", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO10", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO10").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO10").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO11", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO11)", field: "CONST_INFO11", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO11", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO11").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO11").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO12", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO12)", field: "CONST_INFO12", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO12", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO12").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO12").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO13", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO13)", field: "CONST_INFO13", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO13", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO13").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO13").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO14", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO14)", field: "CONST_INFO14", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO14", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO14").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO14").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO15", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO15)", field: "CONST_INFO15", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO15", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO15").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO15").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO16", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO16)", field: "CONST_INFO16", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO16", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO16").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO16").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO17", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO17)", field: "CONST_INFO17", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO17", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO17").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO17").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO18", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO18)", field: "CONST_INFO18", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO18", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO18").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO18").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO19", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO19)", field: "CONST_INFO19", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO19", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO19").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO19").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_INFO20", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO20)", field: "CONST_INFO20", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_INFO20", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO20").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO20").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetHistTop.PREV_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_PREV_TEMP_NUM)", field: "PREV_TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.PREV_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("PREV_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("PREV_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.CONST_IN_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_NUM)", field: "CONST_IN_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_IN_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_IN_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_IN_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.NOW_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_NOW_TEMP_NUM)", field: "NOW_TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.NOW_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("NOW_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("NOW_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.CONST_OUT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_NUM)", field: "CONST_OUT_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_OUT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_OUT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_OUT_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.CUR_LENT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_LENT_NUM)", field: "CUR_LENT_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CUR_LENT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CUR_LENT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CUR_LENT_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.CUR_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_TEMP_NUM)", field: "CUR_TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CUR_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CUR_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CUR_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.SITE_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_SITE_EMPLOYEE_NAME)", field: "SITE_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.SITE_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoTop("SITE_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("SITE_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.CONST_CONFIRM_TS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_TS)", field: "CONST_CONFIRM_TS", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_CONFIRM_TS", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_TS").DispFlag", formatter: YMDHMSFormatter },
            { id: "@ColKey.ConstAssetHistTop.CONST_CONFIRM_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_EMPLOYEE_NAME)", field: "CONST_CONFIRM_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_CONFIRM_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetHistTop.CONST_CONFIRM_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_REMARK)", field: "CONST_CONFIRM_REMARK", width: getColumnWidth("@DispId.CONST_ASSET_HIST_TOP", "@ColKey.ConstAssetHistTop.CONST_CONFIRM_REMARK", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_REMARK").DispFlag" },

        ]

        let columnsMiddle = [
            { id: "@ColKey.ConstAssetHistMiddle.CONST_WORK_TS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_WORK_TS)", field: "CONST_WORK_TS", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.CONST_WORK_TS", 190), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("CONST_WORK_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("CONST_WORK_TS").DispFlag", formatter: YMDHMSFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.CONST_WORK_DIV", name: "@Message.getMessage(CommonLabel.LABEL_WORK_CONTENTS)", field: "CONST_WORK_DIV", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.CONST_WORK_DIV", 190), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("CONST_WORK_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("CONST_WORK_DIV").DispFlag", formatter: ConstWorkDivFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.CONST_MATCH_DIV", name: "@Message.getMessage(CommonLabel.LABEL_CONST_MATCH_DIV)", field: "CONST_MATCH_DIV", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.CONST_MATCH_DIV", 100), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("CONST_MATCH_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("CONST_MATCH_DIV").DispFlag", formatter: ConstMatchDivFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.LENT_NUM", name: "@Message.getMessage(CommonLabelInventorySummary.LABEL_NUMBER_OF_LOANS)", field: "LENT_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.LENT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("LENT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("LENT_NUM").DispFlag", formatter: LentNumFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.TEMP_REG_NUM", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_REG_NUM)", field: "TEMP_REG_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.TEMP_REG_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("TEMP_REG_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("TEMP_REG_NUM").DispFlag", formatter: TempRegNumFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.LEASE_NUM", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_NUM)", field: "LEASE_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.LEASE_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("LEASE_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("LEASE_NUM").DispFlag", formatter: LeaseNumFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.CONST_IN_MATCH_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_MATCH_NUM)", field: "CONST_IN_MATCH_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.CONST_IN_MATCH_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("CONST_IN_MATCH_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("CONST_IN_MATCH_NUM").DispFlag", formatter: ConstInMatchNumFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.CONST_OUT_MATCH_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_MATCH_NUM)", field: "CONST_OUT_MATCH_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.CONST_OUT_MATCH_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("CONST_OUT_MATCH_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("CONST_OUT_MATCH_NUM").DispFlag", formatter: ConstOutMatchNumFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.CONST_IN_NUM_HIST", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_NUM)", field: "CONST_IN_NUM_HIST", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.CONST_IN_NUM_HIST", 100), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("CONST_IN_NUM_HIST").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("CONST_IN_NUM_HIST").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_NUM)", field: "TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("TEMP_NUM").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.ConstAssetHistMiddle.CONST_OUT_NUM_HIST", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_NUM)", field: "CONST_OUT_NUM_HIST", width: getColumnWidth("@DispId.CONST_ASSET_HIST_MIDDLE", "@ColKey.ConstAssetHistMiddle.CONST_OUT_NUM_HIST", 100), sortOrder: "@Model.FirstOrDefault.GridInfoMiddle("CONST_OUT_NUM_HIST").DispOrder", visible: "@Model.FirstOrDefault.GridInfoMiddle("CONST_OUT_NUM_HIST").DispFlag", formatter: RightAlignFormatter },

        ]

        let columnsBottom = [
            { id: "@ColKey.ConstAssetHistBottom.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.ASSET_NO", 160), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NO").DispFlag" },
            { id: "@ColKey.ConstAssetHistBottom.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_DRUG_NAME_SHORT)", field: "ASSET_NAME", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.ASSET_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetHistBottom.CONST_WORK_DIV", name: "@Message.getMessage(CommonLabel.LABEL_WORK_CONTENTS)", field: "CONST_WORK_DIV", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.CONST_WORK_DIV", 190), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_WORK_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_WORK_DIV").DispFlag", formatter: ConstWorkDivFormatter },
            { id: "@ColKey.ConstAssetHistBottom.CONST_MATCH_DIV", name: "@Message.getMessage(CommonLabel.LABEL_CONST_MATCH_DIV)", field: "CONST_MATCH_DIV", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.CONST_MATCH_DIV", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_MATCH_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_MATCH_DIV").DispFlag", formatter: ConstMatchDivFormatter },
            { id: "@ColKey.ConstAssetHistBottom.CONST_MATCH_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_STATUS)", field: "CONST_MATCH_STATUS", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.CONST_MATCH_STATUS", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_MATCH_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_MATCH_STATUS").DispFlag", formatter: ConstMatchStatusFormatter },
            { id: "@ColKey.ConstAssetHistBottom.TEMP_FLAG", name: "@Message.getMessage(CommonLabel.LABEL_TEMP)", field: "TEMP_FLAG", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.TEMP_FLAG", 130), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_FLAG").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_FLAG").DispFlag", formatter: ConstTempFlagFormatter },
            { id: "@ColKey.ConstAssetHistBottom.TEMP_INFO1", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO1)", field: "TEMP_INFO1", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.TEMP_INFO1", 200), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO1").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.ConstAssetHistBottom.TEMP_INFO2", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO2)", field: "TEMP_INFO2", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.TEMP_INFO2", 200), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO2").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.ConstAssetHistBottom.TEMP_INFO3", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO3)", field: "TEMP_INFO3", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.TEMP_INFO3", 200), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO3").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO3").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.ConstAssetHistBottom.TEMP_INFO4", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO4)", field: "TEMP_INFO4", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.TEMP_INFO4", 200), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO4").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO4").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.ConstAssetHistBottom.TEMP_INFO5", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO5)", field: "TEMP_INFO5", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.TEMP_INFO5", 200), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO5").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO5").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.ConstAssetHistBottom.TEMP_INFO6", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO6)", field: "TEMP_INFO6", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.TEMP_INFO6", 200), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO6").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO6").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.ConstAssetHistBottom.TEMP_INFO7", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO7)", field: "TEMP_INFO7", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.TEMP_INFO7", 200), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO7").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO7").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.ConstAssetHistBottom.TEMP_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_REMARK)", field: "TEMP_REMARK", width: getColumnWidth("@DispId.CONST_ASSET_HIST_BOTTOM", "@ColKey.ConstAssetHistBottom.TEMP_REMARK", 150), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_REMARK").DispFlag" },

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

        let optionsMiddle = {
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
        let jsonDataMiddle = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.tblConstructionHistData))) ');
        let jsonDataBottom = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // Gridのデータ管理オブジェクトにセット
        let dataViewTop = new Slick.Data.DataView();
        let dataViewMiddle = new Slick.Data.DataView();
        let dataViewBottom = new Slick.Data.DataView();

        // 固定列を設定
        let fixColTop = Number("@Model.FirstOrDefault.FixColCntTop");
        let fixColMiddle = Number("@Model.FirstOrDefault.FixColCntMiddle");
        let fixColBottom = Number("@Model.FirstOrDefault.FixColCntBottom");

        // Grid表示判定
        if ("@ViewBag.tblConstructionListData.Count" > 0) {
            dataViewTop.beginUpdate();
            dataViewTop.setItems(jsonDataTop, "CONST_NO");
            dataViewTop.endUpdate();
        }

        if ("@ViewBag.tblConstructionHistData.Count" > 0) {
            dataViewMiddle.beginUpdate();
            dataViewMiddle.setItems(jsonDataMiddle, "KEY_VALUE");
            dataViewMiddle.endUpdate();
        }

        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataViewBottom.beginUpdate();
            dataViewBottom.setItems(jsonDataBottom, "ASSET_NO");
            dataViewBottom.endUpdate();
        }

        // Grid初期化
        gridTop = initializeGrid("ListGridsTop", dataViewTop, columnsTop, optionsTop, "@DispId.CONST_ASSET_HIST_TOP", fixColTop);
        gridMiddle = initializeGrid("ListGridsMiddle", dataViewMiddle, columnsMiddle, optionsMiddle, "@DispId.CONST_ASSET_HIST_MIDDLE", fixColMiddle);
        gridBottom = initializeGrid("ListGridsBottom", dataViewBottom, columnsBottom, optionsBottom, "@DispId.CONST_ASSET_HIST_BOTTOM", fixColBottom);


    });

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
                    case "@ConstMatchDiv.OutOfMatchList":
                        status = "@Message.getMessage(CommonLabel.VALUE_OUT_OF_MATCH_LIST)"
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
                ret = String(dataContext.CONST_IN_MATCH_NUM) + "/" + String(dataContext.CONST_IN_NUM_HIST)
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
                ret = String(dataContext.CONST_OUT_MATCH_NUM) + "/" + String(dataContext.CONST_OUT_NUM_HIST)
                break;
            default:
                break;
        }

        return '<div style="text-align:right; width:100%;">' + ret + '</div>';
    }

    // 状態formatter
    function ConstMatchStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch (String(dataContext.CONST_MATCH_STATUS)) {
            case "@CONST_MATCH_STATUS.Storage":
                status = "@Message.getMessage(CommonLabel.VALUE_STORAGE_RETURNED)"
                break;
            case "@CONST_MATCH_STATUS.Lending":
                status = "@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)"
                break;
            case "@CONST_MATCH_STATUS.TempRelease":
                status = "@Message.getMessage(CommonLabel.VALUE_TEMP_RELEASE)"
                break;
            default:
                status = dataContext.CONST_ASSET_STATUS;
                break;
        }

        return status;
    }

    // 仮置フラグ用formatter
    function ConstTempFlagFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch (String(dataContext.TEMP_FLAG)) {
            case "@TempFlg.Cd.Yes":
                status = "@Message.getMessage(CommonLabel.VALUE_DURING_TEMP)"
                break;
            case "@TempFlg.Cd.No":
                status = "@Message.getMessage(CommonLabel.VALUE_NOT_TEMP)"
                break;
            default:
                break;
        }

        return '<div style="text-align:center; width:100%;">' + status + '</div>';
    }

    // 仮置き情報用formatter
    function TempInfoFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(value).replaceAll("$$$", "");
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/ConstAssetHist")/CsvExport");
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;

        $('#form').submit();
    }
</script>

<div id="asset-detail">
    @Using (Html.BeginForm("Index", "ConstAssetHist", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb10" style="width:100%">
            <div id="view-area-button-parent">
                <div class="view-area-button-right" style="margin-right:20px">
                    <input class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
                    <input class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "ConstructionHist")'">
                </div>
            </div>
        </div>

        @<div>
            <div id="ListGridsTop" style="height:auto;padding-bottom: 20px;" />
        </div>
        @<div class="list-view">
            <div id="ListGridsMiddle" style="height:auto;padding-bottom: 20px;" />
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
