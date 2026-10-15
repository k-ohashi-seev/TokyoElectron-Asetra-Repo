@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of ConstAssetListViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelConstAssetList.TITLE)
End Code
<script>
    //
    //  工事資産一覧
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

        // ロケーション大区分非選択時は中区分を非活性
        if (document.getElementById("SearchLocL").value == "") {
            document.getElementById("SearchLocM").value = ""
            $("#SearchLocM").attr("disabled", "disabled");
        }
        // ロケーション中区分非選択時は小区分を非活性
        if (document.getElementById("SearchLocM").value == "") {
            document.getElementById("SearchLocS").value = "";
            $("#SearchLocS").attr("disabled", "disabled");
        }

        // カテゴリー大分類非選択時は中区分を非活性
        if (document.getElementById("SearchCatL").value == "") {
            document.getElementById("SearchCatM").value = ""
            $("#SearchCatM").attr("disabled", "disabled");
        }
        // カテゴリー中分類非選択時は小区分を非活性
        if (document.getElementById("SearchCatM").value == "") {
            document.getElementById("SearchCatS").value = "";
            $("#SearchCatS").attr("disabled", "disabled");
        }

        // 列定義
        let columnsTop = [
            { id: "@ColKey.ConstAssetListTop.CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_CONST_NO)", field: "CONST_NO", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_NO", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_NO").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.CONST_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_START_DT)", field: "CONST_START_DT", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_END_DT)", field: "CONST_END_DT", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_LENT_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)", field: "CONST_LENT_START_DT", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_LENT_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_LENT_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)", field: "CONST_LENT_END_DT", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_LENT_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_LENT_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_STATUS)", field: "CONST_STATUS", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_STATUS", 70), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_STATUS").DispFlag", formatter: ConstStatusFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_CONTENTS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)", field: "CONST_CONTENTS", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_CONTENTS", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONTENTS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONTENTS").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.CONST_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_EMPLOYEE_NAME)", field: "CONST_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_KYOTEN_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.MNG_LOC_L_CD", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("MNG_LOC_L_CD").DispFlag", formatter: MngLocLFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO1", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO1)", field: "CONST_INFO1", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO1", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO1").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO2", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO2)", field: "CONST_INFO2", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO2", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO2").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO3", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO3)", field: "CONST_INFO3", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO3", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO3").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO3").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO4", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO4)", field: "CONST_INFO4", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO4", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO4").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO4").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO5", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO5)", field: "CONST_INFO5", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO5", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO5").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO5").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO6", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO6)", field: "CONST_INFO6", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO6", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO6").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO6").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO7", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO7)", field: "CONST_INFO7", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO7", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO7").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO7").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO8", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO8)", field: "CONST_INFO8", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO8", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO8").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO8").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO9", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO9)", field: "CONST_INFO9", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO9", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO9").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO9").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO10", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO10)", field: "CONST_INFO10", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO10", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO10").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO10").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO11", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO11)", field: "CONST_INFO11", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO11", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO11").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO11").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO12", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO12)", field: "CONST_INFO12", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO12", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO12").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO12").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO13", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO13)", field: "CONST_INFO13", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO13", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO13").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO13").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO14", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO14)", field: "CONST_INFO14", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO14", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO14").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO14").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO15", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO15)", field: "CONST_INFO15", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO15", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO15").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO15").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO16", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO16)", field: "CONST_INFO16", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO16", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO16").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO16").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO17", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO17)", field: "CONST_INFO17", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO17", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO17").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO17").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO18", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO18)", field: "CONST_INFO18", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO18", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO18").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO18").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO19", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO19)", field: "CONST_INFO19", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO19", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO19").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO19").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_INFO20", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO20)", field: "CONST_INFO20", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_INFO20", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO20").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_INFO20").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListTop.PREV_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_PREV_TEMP_NUM)", field: "PREV_TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.PREV_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("PREV_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("PREV_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.CONST_IN_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_NUM)", field: "CONST_IN_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_IN_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_IN_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_IN_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.NOW_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_NOW_TEMP_NUM)", field: "NOW_TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.NOW_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("NOW_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("NOW_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.CONST_OUT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_NUM)", field: "CONST_OUT_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_OUT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_OUT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_OUT_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.CUR_LENT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_LENT_NUM)", field: "CUR_LENT_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CUR_LENT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CUR_LENT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CUR_LENT_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.CUR_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_TEMP_NUM)", field: "CUR_TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CUR_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CUR_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CUR_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.SITE_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_SITE_EMPLOYEE_NAME)", field: "SITE_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.SITE_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoTop("SITE_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("SITE_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.CONST_CONFIRM_TS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_TS)", field: "CONST_CONFIRM_TS", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_CONFIRM_TS", 190), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_TS").DispFlag", formatter: ConstConfirmTsFormatter },
            { id: "@ColKey.ConstAssetListTop.CONST_CONFIRM_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_EMPLOYEE_NAME)", field: "CONST_CONFIRM_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_CONFIRM_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetListTop.CONST_CONFIRM_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_REMARK)", field: "CONST_CONFIRM_REMARK", width: getColumnWidth("@DispId.CONST_ASSET_LIST_TOP", "@ColKey.ConstAssetListTop.CONST_CONFIRM_REMARK", 470), sortOrder: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoTop("CONST_CONFIRM_REMARK").DispFlag" },
        ]

        let columnsBottom = [
            { id: "@ColKey.ConstAssetListBottom.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.ASSET_NO", 160), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NO").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)", field: "ASSET_NAME", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.ASSET_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("ASSET_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.CONST_ASSET_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_STATUS)", field: "CONST_ASSET_STATUS", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_ASSET_STATUS", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_ASSET_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_ASSET_STATUS").DispFlag", formatter: ConstAssetStatusFormatter },
            { id: "@ColKey.ConstAssetListBottom.TEMP_YES_OR_NO", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_YES_OR_NO)", field: "TEMP_ID", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.TEMP_YES_OR_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_YES_OR_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_YES_OR_NO").DispFlag", formatter: TempYesOrNoFormatter },
            { id: "@ColKey.ConstAssetListBottom.TEMP_INFO", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_STORAGE_PLACE)", field: "TEMP_INFO1", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.TEMP_INFO", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_INFO").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.TEMP_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_REMARK)", field: "TEMP_REMARK", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.TEMP_REMARK", 240), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("TEMP_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("TEMP_REMARK").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.LOC_L_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_LOC_L)", field: "LOC_L_CD", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.LOC_L_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LOC_L_CD").DispFlag", formatter: LocLFormatter },
            { id: "@ColKey.ConstAssetListBottom.LOC_M_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_LOC_M)", field: "LOC_M_CD", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.LOC_M_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LOC_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LOC_M_CD").DispFlag", formatter: LocMFormatter },
            { id: "@ColKey.ConstAssetListBottom.LOC_S_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_LOC_S)", field: "LOC_S_CD", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.LOC_S_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LOC_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LOC_S_CD").DispFlag", formatter: LocSFormatter },
            { id: "@ColKey.ConstAssetListBottom.CAT_L_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CAT_L)", field: "CAT_L_CD", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CAT_L_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CAT_L_CD").DispFlag", formatter: CatLFormatter },
            { id: "@ColKey.ConstAssetListBottom.CAT_M_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CAT_M)", field: "CAT_M_CD", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CAT_M_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CAT_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CAT_M_CD").DispFlag", formatter: CatMFormatter },
            { id: "@ColKey.ConstAssetListBottom.CAT_S_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CAT_S)", field: "CAT_S_CD", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CAT_S_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CAT_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CAT_S_CD").DispFlag", formatter: CatSFormatter },
            { id: "@ColKey.ConstAssetListBottom.MAKER", name: "@Message.getMessage(CommonLabel.LABEL_MAKER)", field: "MAKER", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.MAKER", 230), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("MAKER").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("MAKER").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.PRODUCT_NO", name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)", field: "PRODUCT_NO", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.PRODUCT_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("PRODUCT_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("PRODUCT_NO").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.MODEL_NO", name: "@Message.getMessage(CommonLabel.LABEL_MODEL_NO)", field: "MODEL_NO", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.MODEL_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("MODEL_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("MODEL_NO").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.LEASE_NO", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_NO)", field: "LEASE_NO", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.LEASE_NO", 140), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LEASE_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LEASE_NO").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.LEASE_LIMIT", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)", field: "LEASE_LIMIT", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.LEASE_LIMIT", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("LEASE_LIMIT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("LEASE_LIMIT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM1", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_KOUNYU_DATE)", field: "FREE_ITEM1", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM1", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM1").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM2", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM2)", field: "FREE_ITEM2", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM2", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM2").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM3", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM3)", field: "FREE_ITEM3", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM3", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM3").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM3").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM4", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM4)", field: "FREE_ITEM4", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM4", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM4").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM4").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM5", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM5)", field: "FREE_ITEM5", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM5", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM5").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM5").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM6", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM6)", field: "FREE_ITEM6", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM6", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM6").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM6").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM7", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM7)", field: "FREE_ITEM7", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM7", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM7").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM7").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM8", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM8)", field: "FREE_ITEM8", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM8", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM8").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM8").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM9", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM9)", field: "FREE_ITEM9", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM9", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM9").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM9").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM10", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM10)", field: "FREE_ITEM10", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM10", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM10").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM10").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM11", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM11)", field: "FREE_ITEM11", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM11", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM11").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM11").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM12", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM12)", field: "FREE_ITEM12", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM12", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM12").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM12").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM13", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM13)", field: "FREE_ITEM13", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM13", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM13").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM13").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM14", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM14)", field: "FREE_ITEM14", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM14", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM14").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM14").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM15", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM15)", field: "FREE_ITEM15", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM15", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM15").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM15").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM16", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM16)", field: "FREE_ITEM16", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM16", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM16").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM16").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM17", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM17)", field: "FREE_ITEM17", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM17", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM17").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM17").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM18", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM18)", field: "FREE_ITEM18", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM18", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM18").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM18").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.FREE_ITEM19", name: "@Message.getMessage(CommonLabel.LABEL_FREE_ITEM19)", field: "FREE_ITEM19", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.FREE_ITEM19", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM19").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("FREE_ITEM19").DispFlag" },
        ]

        // メニューから遷移の場合
        if ("@ViewBag.ConstructionListKey" == "") {
            columnsBottom.push(
            { id: "@ColKey.ConstAssetListBottom.CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_CONST_NO)", field: "CONST_NO", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_NO", 190), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_NO").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.CONST_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_START_DT)", field: "CONST_START_DT", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_END_DT)", field: "CONST_END_DT", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_LENT_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)", field: "CONST_LENT_START_DT", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_LENT_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_LENT_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_LENT_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_LENT_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)", field: "CONST_LENT_END_DT", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_LENT_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_LENT_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_LENT_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_STATUS)", field: "CONST_STATUS", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_STATUS", 70), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_STATUS").DispFlag", formatter: ConstStatusFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_CONTENTS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)", field: "CONST_CONTENTS", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_CONTENTS", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONTENTS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONTENTS").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.CONST_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_EMPLOYEE_NAME)", field: "CONST_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_KYOTEN_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.MNG_LOC_L_CD", 190), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("MNG_LOC_L_CD").DispFlag", formatter: MngLocLFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO1", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO1)", field: "CONST_INFO1", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO1", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO1").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO1").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO2", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO2)", field: "CONST_INFO2", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO2", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO2").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO2").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO3", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO3)", field: "CONST_INFO3", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO3", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO3").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO3").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO4", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO4)", field: "CONST_INFO4", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO4", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO4").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO4").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO5", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO5)", field: "CONST_INFO5", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO5", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO5").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO5").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO6", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO6)", field: "CONST_INFO6", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO6", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO6").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO6").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO7", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO7)", field: "CONST_INFO7", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO7", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO7").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO7").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO8", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO8)", field: "CONST_INFO8", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO8", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO8").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO8").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO9", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO9)", field: "CONST_INFO9", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO9", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO9").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO9").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO10", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO10)", field: "CONST_INFO10", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO10", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO10").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO10").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO11", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO11)", field: "CONST_INFO11", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO11", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO11").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO11").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO12", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO12)", field: "CONST_INFO12", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO12", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO12").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO12").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO13", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO13)", field: "CONST_INFO13", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO13", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO13").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO13").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO14", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO14)", field: "CONST_INFO14", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO14", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO14").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO14").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO15", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO15)", field: "CONST_INFO15", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO15", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO15").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO15").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO16", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO16)", field: "CONST_INFO16", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO16", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO16").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO16").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO17", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO17)", field: "CONST_INFO17", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO17", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO17").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO17").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO18", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO18)", field: "CONST_INFO18", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO18", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO18").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO18").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO19", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO19)", field: "CONST_INFO19", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO19", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO19").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO19").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_INFO20", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO20)", field: "CONST_INFO20", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_INFO20", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO20").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_INFO20").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstAssetListBottom.PREV_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_PREV_TEMP_NUM)", field: "PREV_TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.PREV_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("PREV_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("PREV_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.CONST_IN_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_NUM)", field: "CONST_IN_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_IN_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_IN_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_IN_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.NOW_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_NOW_TEMP_NUM)", field: "NOW_TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.NOW_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("NOW_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("NOW_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.CONST_OUT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_NUM)", field: "CONST_OUT_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_OUT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_OUT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_OUT_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.CUR_LENT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_LENT_NUM)", field: "CUR_LENT_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CUR_LENT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CUR_LENT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CUR_LENT_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.CUR_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_TEMP_NUM)", field: "CUR_TEMP_NUM", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CUR_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CUR_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CUR_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.SITE_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_SITE_EMPLOYEE_NAME)", field: "SITE_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.SITE_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("SITE_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("SITE_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.CONST_CONFIRM_TS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_TS)", field: "CONST_CONFIRM_TS", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_CONFIRM_TS", 190), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_TS").DispFlag", formatter: ConstConfirmTsFormatter },
            { id: "@ColKey.ConstAssetListBottom.CONST_CONFIRM_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_EMPLOYEE_NAME)", field: "CONST_CONFIRM_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_CONFIRM_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstAssetListBottom.CONST_CONFIRM_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_REMARK)", field: "CONST_CONFIRM_REMARK", width: getColumnWidth("@DispId.CONST_ASSET_LIST_BOTTOM", "@ColKey.ConstAssetListBottom.CONST_CONFIRM_REMARK", 470), sortOrder: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfoBottom("CONST_CONFIRM_REMARK").DispFlag" });
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

        // Gridのデータ管理オブジェクトにセット
        let dataViewTop = new Slick.Data.DataView();
        let dataViewBottom = new Slick.Data.DataView();

        // 固定列を設定
        let fixColTop = Number("@Model.FirstOrDefault.FixColCntTop");
        let fixColBottom = Number("@Model.FirstOrDefault.FixColCntBottom");

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
        gridTop = initializeGrid("ListGridsTop", dataViewTop, columnsTop, optionsTop, "@DispId.CONST_ASSET_LIST_TOP", fixColTop);
        gridBottom = initializeGrid("ListGridsBottom", dataViewBottom, columnsBottom, optionsBottom, "@DispId.CONST_ASSET_LIST_BOTTOM", fixColBottom);

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

    //工事完了日時用formatter
    function ConstConfirmTsFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value).toString()
        if (status != '') {
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            let hhmmss = status.substr(8, 2) + ':' + status.substr(10, 2) + ':' + status.substr(12, 2)
            return yyyy + '/' + mm + '/' + dd + ' ' + hhmmss
        }
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

    // ロケ-ション大区分コード用formatter
    function LocLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_L_CD) + ' / ' + convertToEmptyIfNull(dataContext.LocLName);
    }

    // ロケ-ション中区分コード用formatter
    function LocMFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_M_CD) + ' / ' + convertToEmptyIfNull(dataContext.LocMName);
    }

    // ロケ-ション小区分コード用formatter
    function LocSFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_S_CD) + ' / ' + convertToEmptyIfNull(dataContext.LocSName);
    }

    // カテゴリ大分類コード用formatter
    function CatLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_L_CD) + ' / ' + convertToEmptyIfNull(dataContext.CatLName);
    }

    // カテゴリ中分類コード用formatter
    function CatMFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_M_CD) + ' / ' + convertToEmptyIfNull(dataContext.CatMName);
    }

    // カテゴリ小分類コード用formatter
    function CatSFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_S_CD) + ' / ' + convertToEmptyIfNull(dataContext.CatSName);
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        $('#form').submit();
    }

    //CSV出力ボタンクリック処理
    function btnIchiranCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/ConstAssetList")/IchiranCsvExport");

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

    //大区分選択
    function ChangeLocationL(locL, initialM, initialS) {
        // 中区分をクリア
        document.getElementById("SearchLocM").value = "";
        $("#SearchLocM").attr("disabled", "disabled");
        // 小区分をクリア
        document.getElementById("SearchLocS").value = "";
        $("#SearchLocS").attr("disabled", "disabled");


        // 選択されていない場合はここで終了
        if (!locL.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstAssetList")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationM", selectLCode: locL.value, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocM = document.getElementById("SearchLocM");

                    SearchLocM.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchLocM.appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocM.appendChild(op);
                    }
                }
                // 中区分データ取得
                $("#SearchLocM").removeAttr("disabled");
                SearchLocM.value = data.locMDropdownData;
                if (initialM) {
                    SearchLocM.value = initialM;
                    ChangeLocationM(SearchLocM, initialS);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //中区分選択
    function ChangeLocationM(locM, initialS) {
        var lCode = document.getElementById("SearchLocL").value;

        // 小区分をクリア
        document.getElementById("SearchLocS").value = "";
        $("#SearchLocS").attr("disabled", "disabled");


        // 選択されていない場合はここで終了
        if (!locM.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstAssetList")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationS", selectLCode: lCode, selectMCode: locM.value },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocS = document.getElementById("SearchLocS");
                    SearchLocS.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchLocS.appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocS.appendChild(op);
                    }
                }
                // 小区分データ取得
                $("#SearchLocS").removeAttr("disabled");


                SearchLocS.value = data.locSDropdownData;
                if (initialS) {
                    SearchLocS.value = initialS;
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //カテゴリー大分類選択
    function ChangeCategoryL(catL, initialM, initialS) {
        // 中分類をクリア
        document.getElementById("SearchCatM").value = "";
        $("#SearchCatM").attr("disabled", "disabled");
        // 小分類をクリア
        document.getElementById("SearchCatS").value = "";
        $("#SearchCatS").attr("disabled", "disabled");

        // 選択されていない場合はここで終了
        if (!catL.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstAssetList")/ChangeCategory',
            data: { categoryLevel: "@CAT_DIV.CategoryM", selectLCode: catL.value, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchCatM = document.getElementById("SearchCatM");

                    SearchCatM.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchCatM.appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].CAT_CD;
                        op.text = data.Data[i].CAT_NAME;
                        SearchCatM.appendChild(op);
                    }
                }
                // 中分類データ取得
                $("#SearchCatM").removeAttr("disabled");

                SearchCatM.value = data.catMDropdownData;
                if (initialM) {
                    SearchCatM.value = initialM;
                    ChangeCategoryM(SearchCatM, initialS);
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //カテゴリー中分類選択
    function ChangeCategoryM(catM, initialS) {
        var lCode = document.getElementById("SearchCatL").value;

        // 小分類をクリア
            document.getElementById("SearchCatS").value = "";
            $("#SearchCatS").attr("disabled", "disabled");

        // 選択されていない場合はここで終了
        if (!catM.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstAssetList")/ChangeCategory',
            data: { categoryLevel: "@CAT_DIV.CategoryS", selectLCode: lCode, selectMCode: catM.value },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchCatS = document.getElementById("SearchCatS");

                    SearchCatS.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchCatS.appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].CAT_CD;
                        op.text = data.Data[i].CAT_NAME;
                        SearchCatS.appendChild(op);
                    }
                }
                // 小分類データ取得
                $("#SearchCatS").removeAttr("disabled");

                SearchCatS.value = data.catSDropdownData;
                if (initialS) {
                    SearchCatS.value = initialS;
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

</script>

<div id="asset-detail">
    @Using (Html.BeginForm("Index", "ConstAssetList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb10" style="width:100%">
            <div id="view-area-button-parent">
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
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("ASSET_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetNo, New With {.class = "cond-control", .maxlength = 24, .oninput = "editAssetNo(this)"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("ASSET_NAME").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CONST_ASSET_STATUS").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_STATUS)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchConstAssetStatus, DirectCast(ViewBag.ConstAssetStatusList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("TEMP_YES_OR_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_YES_OR_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchTempYesOrNo, DirectCast(ViewBag.TempYesOrNoList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("TEMP_INFO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_STORAGE_PLACE)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchTempInfo, New With {.class = "cond-control", .maxlength = 1020})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("TEMP_REMARK").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_REMARK)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchTempRemark, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("LOC_L_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocL, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control", .onchange = "ChangeLocationL(this)"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("LOC_M_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_M)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocM, DirectCast(ViewBag.modelLocM, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control", .onchange = "ChangeLocationM(this)"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("LOC_S_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_S)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocS, DirectCast(ViewBag.modelLocS, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CAT_L_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control", .onchange = "ChangeCategoryL(this)"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CAT_M_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_M)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatM, DirectCast(ViewBag.modelCatM, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control", .onchange = "ChangeCategoryM(this)"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("CAT_S_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_S)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatS, DirectCast(ViewBag.modelCatS, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("MAKER").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_MAKER)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchMaker, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("PRODUCT_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchProductNo, New With {.class = "cond-control", .maxlength = 30, .oninput = "editPassword(this)"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("MODEL_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_MODEL_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchModelNo, New With {.class = "cond-control", .maxlength = 30})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("LEASE_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchLeaseNo, New With {.class = "cond-control", .maxlength = 30})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("LEASE_LIMIT").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchLeaseLimit, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM1").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_KOUNYU_DATE)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem1, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM2").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM2)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem2, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM3").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM3)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem3, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM4").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM4)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem4, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM5").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM5)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem5, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM6").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM6)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem6, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM7").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM7)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem7, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM8").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM8)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem8, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM9").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM9)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem9, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM10").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM10)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem10, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM11").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM11)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem11, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM12").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM12)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem12, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM13").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM13)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem13, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM14").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM14)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem14, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM15").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM15)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem15, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM16").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM16)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem16, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM17").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM17)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem17, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM18").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM18)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem18, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfoBottom("FREE_ITEM19").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_FREE_ITEM19)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem19, New With {.class = "cond-control", .maxlength = 255})
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
        @<div class="list-view">
            <div id="ListGridsBottom" style="height:auto;padding-bottom: 20px;" />
        </div>

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
