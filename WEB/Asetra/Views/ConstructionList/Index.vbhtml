@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of ConstructionListViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelConstructionList.TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
    Dim inputItemList = DirectCast(ViewBag.inputItemList, IEnumerable(Of SearchInputItemModel))
End Code
<script>
    let gridReserve;

    $(function () {
        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        // オプション項目表示非表示設定
        if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            $("#cond").removeClass("in");
            $("#cond").addClass("collapse");
            document.getElementById("SearchCondition").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }

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
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessComplete") {
            //工事完了
            showResultMessage("@Message.getMessage(CommonLabelConstructionList.MSG_SUCCESS_CONST_COMPLETE)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
                //登録画面を表示
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@Message.getMessage(CommonLabelConstructionList.TITLE_CONSTUCTION_REGISTRATION)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_TOUROKU)";
                //メッセージ設定
                document.getElementById("lblCannotAddReserve").innerHTML = "@Message.getMessage(CommonLabelConstructionList.MSG_CANNOT_ADD_RESERVE)";

                //削除ボタンを非表示
                $("#modal-delete-btn").css("display", "none");

                // 追加、解除、個数変更ボタンを使用不可
                $("#modal-temp-add-btn").prop("disabled", true);
                $("#modal-temp-add-btn").attr('style', 'background-color:#cecece;');
                $("#modal-temp-add-btn").css("cursor", "default");
                $("#modal-release-btn").prop("disabled", true);
                $("#modal-release-btn").attr('style', 'background-color:#cecece;');
                $("#modal-release-btn").css("cursor", "default");
                $("#modal-reserve-add-btn").prop("disabled", true);
                $("#modal-reserve-add-btn").attr('style', 'background-color:#cecece;');
                $("#modal-reserve-add-btn").css("cursor", "default");
                $("#modal-qty-change-btn").prop("disabled", true);
                $("#modal-qty-change-btn").attr('style', 'background-color:#cecece;');
                $("#modal-qty-change-btn").css("cursor", "default");
                $("#modal-reserve-del-btn").prop("disabled", true);
                $("#modal-reserve-del-btn").attr('style', 'background-color:#cecece;');
                $("#modal-reserve-del-btn").css("cursor", "default");

                //仮置一覧テーブル初期化
                if("@ViewBag.TemporaryLeaveFlg" == "Y"){
                    initializeTempListGrid();
                }

                //予約一覧テーブル初期化
                if("@ViewBag.MatchingReserveFlg" == "Y"){
                    initializeReserveListGrid();
                }
            } else {
                //変更画面を表示
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@Message.getMessage(CommonLabelConstructionList.TITLE_CONSTUCTION_UPDATE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_UPDATE)";
                //メッセージ設定
                document.getElementById("lblCannotAddReserve").innerHTML = "";

                // 権限による処理区分
                if ("@ViewBag.AuthProcFlg" == "@AuthProcFlg.All" || "@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiAll") {
                    // 権限による処理区分が「編集・照会可能」の場合
                    //削除ボタンを表示
                    $("#modal-delete-btn").css("display", "block");
                } else {
                    // 権限による処理区分が「照会のみ」の場合（本来ここに入ることはないはずだが、念のため）
                    // 変更ボタンを非表示
                    $("#modal-create-btn").css("display", "none");
                    // 削除ボタンを非表示
                    $("#modal-delete-btn").css("display", "none");
                };

                //仮置一覧テーブル初期化
                if("@ViewBag.TemporaryLeaveFlg" == "Y"){
                    initializeTempListGrid(document.getElementById("CONST_NO").value);
                }

                //予約一覧テーブル初期化
                if("@ViewBag.MatchingReserveFlg" == "Y"){
                    initializeReserveListGrid(document.getElementById("CONST_NO").value);
                }

                ChkConstLentDt()
            }

            ShowModalWindow()

            if ("@Model.FirstOrDefault.ErrTarget".search("Target_") >= 0) {
                showErrMessage("@Model.FirstOrDefault.ErrCode", "#@Model.FirstOrDefault.ErrTarget".replace("Target_", ""))
            } else {
                showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
                //範囲入力時のエラー設定
                $("#@(Model.FirstOrDefault.ErrTarget)_FROM").addClass("input-error");
                $("#@(Model.FirstOrDefault.ErrTarget)_TO").addClass("input-error");
            }

        } else {
            //ロード前のボタン名を設定する
            document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"
        }

        //仮置一覧から遷移の場合
        if("@ViewBag.ModalOpenFlg" == "Y"){
            //変更画面を表示
            //タイトルの設定
            document.getElementById("popup-title").innerHTML = "@Message.getMessage(CommonLabelConstructionList.TITLE_CONSTUCTION_UPDATE)";
            document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_UPDATE)";

            // 権限による処理区分
            if ("@ViewBag.AuthProcFlg" == "@AuthProcFlg.All" || "@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiAll") {
                // 権限による処理区分が「編集・照会可能」の場合
                //削除ボタンを表示
                $("#modal-delete-btn").css("display", "block");
            } else {
                // 権限による処理区分が「照会のみ」の場合（本来ここに入ることはないはずだが、念のため）
                // 変更ボタンを非表示
                $("#modal-create-btn").css("display", "none");
                // 削除ボタンを非表示
                $("#modal-delete-btn").css("display", "none");
            };

            //仮置一覧テーブル初期化
            if("@ViewBag.TemporaryLeaveFlg" == "Y"){
                initializeTempListGrid(document.getElementById("CONST_NO").value);
            }

            //予約一覧テーブル初期化
            if("@ViewBag.MatchingReserveFlg" == "Y"){
                initializeReserveListGrid(document.getElementById("CONST_NO").value);
            }

            ChkConstLentDt()

            ShowModalWindow()
        }

        //隠し項目に格納
        document.getElementById("LoadingImg").value = '@Url.Content("~/Images/")' + "Loading.gif";

        btnCsvImportEnabled(true);

        // 列定義
        let columns = [
            { id: "@ColKey.ConstructionList.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CHECK_SELECT", 40), sortOrder: -2, formatter: ConstructionListCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.ConstructionList.CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_CONST_NO)", field: "CONST_NO", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_NO", 190), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_NO").DispFlag" },
            { id: "@ColKey.ConstructionList.CONST_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_START_DT)", field: "CONST_START_DT", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionList.CONST_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_END_DT)", field: "CONST_END_DT", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionList.CONST_LENT_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)", field: "CONST_LENT_START_DT", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_LENT_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_LENT_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_LENT_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionList.CONST_LENT_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)", field: "CONST_LENT_END_DT", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_LENT_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_LENT_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_LENT_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.ConstructionList.CONST_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_STATUS)", field: "CONST_STATUS", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_STATUS", 70), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_STATUS").DispFlag", formatter: ConstStatusFormatter },
            { id: "@ColKey.ConstructionList.CONST_CONTENTS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)", field: "CONST_CONTENTS", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_CONTENTS", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_CONTENTS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_CONTENTS").DispFlag" },
            { id: "@ColKey.ConstructionList.CONST_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_EMPLOYEE_NAME)", field: "CONST_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstructionList.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_KYOTEN_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.MNG_LOC_L_CD", 190), sortOrder: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag", formatter: MngLocLFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO1", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO1)", field: "CONST_INFO1", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO1", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO1").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO2", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO2)", field: "CONST_INFO2", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO2", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO2").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO3", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO3)", field: "CONST_INFO3", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO3", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO3").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO4", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO4)", field: "CONST_INFO4", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO4", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO4").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO5", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO5)", field: "CONST_INFO5", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO5", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO5").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO6", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO6)", field: "CONST_INFO6", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO6", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO6").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO7", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO7)", field: "CONST_INFO7", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO7", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO7").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO8", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO8)", field: "CONST_INFO8", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO8", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO8").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO9", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO9)", field: "CONST_INFO9", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO9", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO9").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO10", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO10)", field: "CONST_INFO10", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO10", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO10").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO11", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO11)", field: "CONST_INFO11", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO11", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO11").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO11").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO12", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO12)", field: "CONST_INFO12", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO12", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO12").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO12").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO13", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO13)", field: "CONST_INFO13", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO13", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO13").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO13").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO14", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO14)", field: "CONST_INFO14", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO14", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO14").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO14").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO15", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO15)", field: "CONST_INFO15", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO15", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO15").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO15").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO16", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO16)", field: "CONST_INFO16", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO16", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO16").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO16").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO17", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO17)", field: "CONST_INFO17", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO17", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO17").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO17").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO18", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO18)", field: "CONST_INFO18", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO18", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO18").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO18").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO19", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO19)", field: "CONST_INFO19", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO19", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO19").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO19").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.CONST_INFO20", name: "@Message.getMessage(CommonLabel.LABEL_CONST_INFO20)", field: "CONST_INFO20", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_INFO20", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_INFO20").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_INFO20").DispFlag", formatter: ConstInfoFormatter },
            { id: "@ColKey.ConstructionList.PREV_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_PREV_TEMP_NUM)", field: "PREV_TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.PREV_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("PREV_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PREV_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstructionList.CONST_IN_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_IN_NUM)", field: "CONST_IN_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_IN_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_IN_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_IN_NUM").DispFlag" },
            { id: "@ColKey.ConstructionList.NOW_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_NOW_TEMP_NUM)", field: "NOW_TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.NOW_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("NOW_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("NOW_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstructionList.CONST_OUT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CONST_OUT_NUM)", field: "CONST_OUT_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_OUT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_OUT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_OUT_NUM").DispFlag" },
            { id: "@ColKey.ConstructionList.CUR_LENT_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_LENT_NUM)", field: "CUR_LENT_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CUR_LENT_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("CUR_LENT_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CUR_LENT_NUM").DispFlag" },
            { id: "@ColKey.ConstructionList.CUR_TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_CUR_TEMP_NUM)", field: "CUR_TEMP_NUM", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CUR_TEMP_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("CUR_TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CUR_TEMP_NUM").DispFlag" },
            { id: "@ColKey.ConstructionList.SITE_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_SITE_EMPLOYEE_NAME)", field: "SITE_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.SITE_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfo("SITE_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SITE_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstructionList.CONST_CONFIRM_TS", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_TS)", field: "CONST_CONFIRM_TS", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_CONFIRM_TS", 190), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_CONFIRM_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_CONFIRM_TS").DispFlag", formatter: ConstConfirmTsFormatter },
            { id: "@ColKey.ConstructionList.CONST_CONFIRM_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_EMPLOYEE_NAME)", field: "CONST_CONFIRM_EMPLOYEE_NAME", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_CONFIRM_EMPLOYEE_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_CONFIRM_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_CONFIRM_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.ConstructionList.CONST_CONFIRM_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_CONST_CONFIRM_REMARK)", field: "CONST_CONFIRM_REMARK", width: getColumnWidth("@DispId.CONSTRUCTION_LIST", "@ColKey.ConstructionList.CONST_CONFIRM_REMARK", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_CONFIRM_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_CONFIRM_REMARK").DispFlag" },
        ];

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

        // 選択状態を復元
        let lstCheckedKey = @Html.Raw(jsonCheckKeys);
        for (let i = 0; i < lstCheckedKey.length; i++) {
            // Key1:工事番号
            var keys = lstCheckedKey[i].split(",");

            for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                if (keys[0] === jsonData[grid_i].CONST_NO) {
                    jsonData[grid_i].DISP_FLAG = lstCheckedKey[i]
                    break;
                }
            }
        }

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "CONST_NO");
            dataView.endUpdate();
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列を加算
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.CONSTRUCTION_LIST", fixCol);

    });

    // チェックボックス(中央寄せ)
    function ConstructionListCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.CONST_NO);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheck" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" ' +
               (isKeyInclude ? 'checked="checked" ' : '') +
               'onchange="check_onclick(this)" ' +
               '/>' +
               '</div>';
    }

    // チェックボックス(仮置一覧)
    function TempListCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.TEMP_ID);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheckTemporary" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" ' +
               '/>' +
               '</div>';
    }

    // チェックボックス(予約一覧)
    function ReserveListCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.PRODUCT_NO);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheckReservation" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" ' +
               '/>' +
               '</div>';
    }


    // チェックボックス(資産予約)
    function AssetReserveCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.PRODUCT_NO);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheckReservation" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" ' +
               (isKeyInclude ? 'checked="checked" ' : '') +
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

    // 仮置き場所
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

    // 仮置き数用formatter
    function TempNumFormatter( row, cell, value, columnDef, dataContext )  {
        return '<a href="#" onclick="linkTempNum_onclick(\'' + dataContext.TEMP_ID + '\')" class="blue-link">' + dataContext.TEMP_NUM + '</a>';
    }

    // エラー用formatter
    function ErrorFormatter(row, cell, value, columnDef, dataContext) {
        let ret = "";

        if (dataContext.RESERVATION_NUM > dataContext.AVAILABLE_RESERVATION_NUM) {
            ret = "@Message.getMessage(CommonLabelConstructionList.LABEL_QUANTITY_SHORTAGE)";
        }

        return ret;
    }

    // テキスト表示(右寄せ)
    function RightAlignIfNullFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:right; width:100%;">' + convertToEmptyIfNull(value) + '</div>';
    }

    //CSVデータ取込画面　取込開始ボタンクリック処理
    function btnCsvImportStart_onclick() {
        // ダイアログのメッセージを設定
        strMessage = "@Message.getMessage(CommonLabel.MSG_ASSET_CSVINPUT)".replace("@@@@CR@@@@", "<BR>");
        $("#show-dialog").html(strMessage);

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $(this).dialog("close");

                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Ok";
                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORT)";

                    // td内のtrをループ。rowsコレクションで,行位置取得。
                    var tblTbody = document.getElementById('csv-err-ichiran');
                    var rowCount = tblTbody.rows.length
                    for (var i=0; i<rowCount; i++) {
                        if( i < "@DataInport.DISP_ROW_COUNT"){
                            // tr内のtdをループ。cellsコレクションで行内セル位置取得。
                            for (var j=0, colLen=tblTbody.rows[i].cells.length ; j<colLen; j++) {
                                //i行目のj列目のセルをクリア
                                tblTbody.rows[i].cells[j].innerHTML ="　";
                                tblTbody.rows[i].cells[j].title ="　";
                            }
                        }else{
                            //i行目のj列目のセルを削除
                            tblTbody.rows["@DataInport.DISP_ROW_COUNT"].remove();
                        }

                    }

                    //CSVデータを取得し表示する(Ajax非同期通信)
                    var $form = $(this).closest('form')
                    var formData = new FormData($form.get(0));
                    formData.append("CsvFile", $('#CsvFile')[0].files[0]);

                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/ConstructionList")/ImportCsvFile',
                        data: formData,
                        dataType: 'json',
                        processData: false,
                        contentType: false,
                        cache: false,
                        success: function (data, dataType) {
                            this;

                            switch (data[0].ResultCd) {
                                case @DataInport.DataInportResultCd.OK:
                                    // 正常終了
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTOK)";
                                    break;
                                case @DataInport.DataInportResultCd.NoSelect:
                                    // CSVファイル未選択
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SELECT)";
                                    break;
                                case @DataInport.DataInportResultCd.Empty:
                                    // CSVファイルが空
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_EMPTY)";
                                    break;
                                case @DataInport.DataInportResultCd.NotCsv:
                                    // CSVファイル以外
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_NOTCSV)";
                                    break;
                                case @DataInport.DataInportResultCd.KoumokuSuuErr:
                                    // 項目数エラー
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU)";
                                    break;
                                case @DataInport.DataInportResultCd.DataNG:
                                    // エラー有
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTNG)";

                                    for (var icnt = 0; icnt < data.length; icnt++) {
                                        if( icnt < "@DataInport.DISP_ROW_COUNT"){
                                            //icnt行目の各項目に値設定
                                            tblTbody.rows[icnt].cells[0].innerHTML =data[icnt].ErrRowIdx;
                                            tblTbody.rows[icnt].cells[0].title = data[icnt].ErrRowIdx;
                                            tblTbody.rows[icnt].cells[1].innerHTML =data[icnt].ErrKoumoku;
                                            tblTbody.rows[icnt].cells[1].title = data[icnt].ErrKoumoku;
                                            tblTbody.rows[icnt].cells[2].innerHTML =data[icnt].ErrGaiyou;
                                            tblTbody.rows[icnt].cells[2].title =data[icnt].ErrGaiyou;
                                        }else{
                                            $("#csv-err-ichiran").append('<tr><td class="col1" title=' + data[icnt].ErrRowIdx + '>'+ data[icnt].ErrRowIdx +'</td><td class="col2" title=' + data[icnt].ErrKoumoku + '>'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" title=' + data[icnt].ErrGaiyou + '>'+ data[icnt].ErrGaiyou +'</td></tr>');
                                        }

                                    }
                                    break;
                                case @DataInport.DataInportResultCd.LoginSession:
                                    // ログインセッション切れ
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_LOGININFO_EMPTY)";
                                    break;
                                case @DataInport.DataInportResultCd.CsvMapMei:
                                    // CSVマッピング明細マスタエラー
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_CSVMAPMEI)";
                                    break;
                                case @DataInport.DataInportResultCd.CsvMapCharCd:
                                    // CSVマッピングマスタ文字コードエラー
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_CHARCD)";
                                    break;
                                case @DataInport.DataInportResultCd.SettingInfo:
                                    // 設定マスタの文字コード情報取得エラー
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = data[0].ErrGaiyou;
                                    break;
                            }
                            if (data[0].ResultCd != "@DataInport.DataInportResultCd.OK" &&
                                data[0].ResultCd != "@DataInport.DataInportResultCd.NoSelect") {
                                //正常終了以外の場合、CSVデータ取込ボタンを非活性にする
                                btnCsvImportEnabled(false)
                            }
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            this;
                            document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                            document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SOUTEIGAI)";
                            btnCsvImportEnabled(false)
                        }
                    });
                    return true;
                },
                "Cancel": function () {
                    $(this).dialog("close");
                    return false;
                }
            },
            open: function () {      // キャンセルボタンにフォーカスをあてる
                $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').focus();
            }
        });

    }

    //CSVモーダル CSVデータ取込ボタン押下可否設定処理
    function btnCsvImportEnabled(enable) {
        if (enable) {
            //活性
            $("#modal-import-btn").attr("disabled", false)
        } else {
            //非活性
            $("#modal-import-btn").attr("disabled", "disabled")
            $("#modal-import-btn").css("background-color","#cecece");
        }
    }

    //CSVデータ取込ボタンクリック処理
    function btnCsvImport_onclick() {
        document.getElementById("popup-title-csv").innerHTML = "@ViewBag.Title";
        document.getElementById("modal-import-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)";
        document.getElementById("CsvFile").value = "";          //ファイル選択をクリア
        document.getElementById("modal-msg").innerHTML = "";    //メッセージをクリア
        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Nomal";   //メッセージの背景色を白にする

        // td内のtrをループ。rowsコレクションで,行位置取得。
        var tblTbody = document.getElementById('csv-err-ichiran');
        var rowCount = tblTbody.rows.length
        for (var i=0; i<rowCount; i++) {
            if (i < "@DataInport.DISP_ROW_COUNT") {
                // tr内のtdをループ。cellsコレクションで行内セル位置取得。
                for (var j = 0, colLen = tblTbody.rows[i].cells.length ; j < colLen; j++) {
                    //i行目のj列目のセルをクリア
                    tblTbody.rows[i].cells[j].innerHTML = "　";
                    tblTbody.rows[i].cells[j].title ="　";
                }
            } else {
                //i行目のj列目のセルを削除
                tblTbody.rows["@DataInport.DISP_ROW_COUNT"].remove();
            }

        }

        //CSV取込画面を表示
        ShowCsvModalWindow()
    }

    //CSV出力ボタンクリック処理
    function btnIchiranCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/ConstructionList")/IchiranCsvExport");

    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

    //検索条件リンククリック時処理_資産予約画面
    function SearchConditionAssetReserve_onclick() {
        if (document.getElementById("SearchConditionAssetReserve").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchConditionAssetReserve").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchConditionAssetReserve").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
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

    //工事完了ボタンクリック処理
    function btnConstComplete_onclick() {
        document.getElementById("popup-title-const-complete").innerHTML = "@Message.getMessage(CommonLabelConstructionList.TITLE_CONST_COMPLETE)";
        document.getElementById("CONST_CONFIRM_REMARK").value = "";    //工事備考をクリア
        document.getElementById("modal-msg").innerHTML = "";    //メッセージをクリア
        $("CONST_CONFIRM_REMARK").removeClass("input-error")
        //チェックボックスの状態からキーを設定する
        setting_checklist();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }else if (document.getElementById("selectcount").innerHTML > 1) { //テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }

        // カンマ区切りの文字列を設定
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstructionList")/ShowConstComplete',
            dataType: 'json',
            data: { SelectKeys: document.getElementById("SelectKeys").value, NoneSelectKeys: document.getElementById("NoneSelectKeys").value},
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                } else {
                    // 照会のみのデータがあればエラーメッセージ
                    if(data == "auth-error"){
                        showErrMessage("@Message.getMessage(CommonLabelConstructionList.MSG_ERR_SELECTED_NOT_UPD_CONST)")
                        return false;
                    }
                    // 完了済みの工事があればエラーメッセージ
                    if(data == "error"){
                        showErrMessage("@Message.getMessage(CommonLabelConstructionList.MSG_CONST_COMPLETED)")
                        return false;
                    }

                    // 未返却の資産があればダイアログにメッセージ
                    if(data == "unreturned"){
                        document.getElementById("msg-const-complete").innerHTML = "@Message.getMessage(CommonLabelConstructionList.MSG_UNRETURNED_CONFIRM)";
                    }

                    //モーダルウインドウを開く
                    ShowConstCompleteModalWindow();
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });

        return true;
    }

    // 完了ボタンクリック処理
    function btnConstCompleteOK_onclick() {
        if (document.getElementById("CONST_CONFIRM_REMARK").value == ""){
            showErrMessage("@Message.getMessage(CommonLabelConstructionList.MSG_ERR_NOT_ENTEREDCONFIRM_REMARK)", "#CONST_CONFIRM_REMARK")
            return
        }

        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonLabelConstructionList.MSG_CONST_COMPLET_CONFIRM)", "@Url.Content("~/ConstructionList")/ConstComplete");
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        $("#@(Model.FirstOrDefault.ErrTarget)_FROM").removeClass("input-error");
        $("#@(Model.FirstOrDefault.ErrTarget)_TO").removeClass("input-error");

        //ページ番号のリセット
        ResetPageNum();
        return true;
    }

    // 資産照会ボタンクリック処理
    function btnShowConstAssetList_onclick() {
        //テーブルの選択行を取得する。
        var constNo = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                //「, (カンマ)」で区切って分割する
                var result = checkBoxes[i].value.split(',');
                constNo.push(result[0]);
            }
        }

        if (constNo.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (constNo.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        document.getElementById("SelectedConstNo").value = constNo[0];

        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/ConstructionList")/ShowConstAssetList");
        $('#form').submit();
        $('#form').attr("action", befAction);
        return true;
    }

    // 履歴照会ボタンクリック処理
    function btnShowConstructionHist_onclick() {
        //テーブルの選択行を取得する。
        var constNo = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                //「, (カンマ)」で区切って分割する
                var result = checkBoxes[i].value.split(',');
                constNo.push(result[0]);
            }
        }

        if (constNo.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (constNo.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        document.getElementById("SelectedConstNo").value = constNo[0];

        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/ConstructionList")/ShowConstructionHist");
        $('#form').submit();
        $('#form').attr("action", befAction);
        return true;
    }

    //帳票出力ボタンクリック処理
    function btnReportOutput_onclick() {
        //チェックボックスの状態からキーを設定する
        setting_checklist();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        // カンマ区切りの文字列を設定
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstructionList")/ShowReportOutput',
            dataType: 'json',
            data: { SelectKeys: document.getElementById("SelectKeys").value, NoneSelectKeys: document.getElementById("NoneSelectKeys").value},
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                } else {
                    // 未完了の工事があればエラーメッセージ
                    if(data == "error"){
                        showErrMessage("@Message.getMessage(CommonLabelConstructionList.MSG_CONST_UNCOMPLETED)")
                        return false;
                    }

                    //確認ダイアログを表示
                    return showConfirm("@Message.getMessage(CommonInfoMsg.REPORT_OUTPUT_COFIRM)", "@Url.Content("~/ConstructionList")/ReportOutput");
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });

        return true;
    }

    //登録ボタンクリック処理
    function btnCreate_onclick() {

        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@Message.getMessage(CommonLabelConstructionList.TITLE_CONSTUCTION_REGISTRATION)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_TOUROKU)";
        // 登録ボタンを表示
        $("#modal-create-btn").css("display", "block");
        $("#modal-create-btn").prop("disabled", false);
        $("#modal-create-btn").css("background-color", "");
        $("#modal-create-btn").css("cursor", "pointer");

        //メッセージ設定
        document.getElementById("lblCannotAddReserve").innerHTML = "@Message.getMessage(CommonLabelConstructionList.MSG_CANNOT_ADD_RESERVE)";

        //項目表示状況初期化
        initializeDisable()

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        //削除ボタンを非表示
        $("#modal-delete-btn").css("display", "none");

        // 追加、解除、個数変更ボタンを使用不可
        $("#modal-temp-add-btn").prop("disabled", true);
        $("#modal-temp-add-btn").attr('style', 'background-color:#cecece;');
        $("#modal-temp-add-btn").css("cursor", "default");
        $("#modal-release-btn").prop("disabled", true);
        $("#modal-release-btn").attr('style', 'background-color:#cecece;');
        $("#modal-release-btn").css("cursor", "default");
        $("#modal-reserve-add-btn").prop("disabled", true);
        $("#modal-reserve-add-btn").attr('style', 'background-color:#cecece;');
        $("#modal-reserve-add-btn").css("cursor", "default");
        $("#modal-qty-change-btn").prop("disabled", true);
        $("#modal-qty-change-btn").attr('style', 'background-color:#cecece;');
        $("#modal-qty-change-btn").css("cursor", "default");
        $("#modal-reserve-del-btn").prop("disabled", true);
        $("#modal-reserve-del-btn").attr('style', 'background-color:#cecece;');
        $("#modal-reserve-del-btn").css("cursor", "default");

        // 生成要求前に工事番号を初期化
        document.getElementById("CONST_NO").value = "";

        // 工事番号の生成要求を非同期で実施
        let promise = $.ajax({
            url: '@Url.Content("~/ConstructionList")/RequestConstNo',
            type: 'POST',
        });

        promise.always(function (constNo) {
            // 受け取った工事番号は一旦使用しないので必須チェックにならないようダミーの文字列をセット
            if (constNo != "") {
                document.getElementById("CONST_NO").value = "_";

                document.getElementById("CONST_NO").readOnly = true;

                // 工事番号を灰色のフォント表示
                document.getElementById("CONST_NO").style.color = "#cecece";
            }
        });

        //画面を初期化する
        document.getElementById("ConstStartDtString").value = "";
        $("#ConstStartDtString").removeClass("input-error")
        document.getElementById("ConstEndDtString").value = "";
        $("#ConstEndDtString").removeClass("input-error")
        document.getElementById("ConstLentStartDtString").value = "";
        $("#ConstLentStartDtString").removeClass("input-error")
        document.getElementById("ConstLentEndDtString").value = "";
        $("#ConstLentEndDtString").removeClass("input-error")
        document.getElementById("CONST_CONTENTS").value = "";
        document.getElementById("CONST_EMPLOYEE_CD").value = "";
        document.getElementById("MNG_LOC_L_CD").value = "";
        if (document.getElementById("CONST_INFO1") != null) document.getElementById("CONST_INFO1").value = "@(inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO1_FROM") != null) document.getElementById("CONST_INFO1_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO1_TO") != null) document.getElementById("CONST_INFO1_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO2") != null) document.getElementById("CONST_INFO2").value = "@(inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO2_FROM") != null) document.getElementById("CONST_INFO2_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO2_TO") != null) document.getElementById("CONST_INFO2_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO3") != null) document.getElementById("CONST_INFO3").value = "@(inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO3_FROM") != null) document.getElementById("CONST_INFO3_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO3_TO") != null) document.getElementById("CONST_INFO3_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO4") != null) document.getElementById("CONST_INFO4").value = "@(inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO4_FROM") != null) document.getElementById("CONST_INFO4_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO4_TO") != null) document.getElementById("CONST_INFO4_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO5") != null) document.getElementById("CONST_INFO5").value = "@(inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO5_FROM") != null) document.getElementById("CONST_INFO5_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO5_TO") != null) document.getElementById("CONST_INFO5_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO6") != null) document.getElementById("CONST_INFO6").value = "@(inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO6_FROM") != null) document.getElementById("CONST_INFO6_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO6_TO") != null) document.getElementById("CONST_INFO6_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO7") != null) document.getElementById("CONST_INFO7").value = "@(inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO7_FROM") != null) document.getElementById("CONST_INFO7_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO7_TO") != null) document.getElementById("CONST_INFO7_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO8") != null) document.getElementById("CONST_INFO8").value = "@(inputItemList.Where(Function(x) x.KEY1 = "8").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO8_FROM") != null) document.getElementById("CONST_INFO8_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "8").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO8_TO") != null) document.getElementById("CONST_INFO8_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "8").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO9") != null) document.getElementById("CONST_INFO9").value = "@(inputItemList.Where(Function(x) x.KEY1 = "9").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO9_FROM") != null) document.getElementById("CONST_INFO9_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "9").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO9_TO") != null) document.getElementById("CONST_INFO9_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "9").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO10") != null) document.getElementById("CONST_INFO10").value = "@(inputItemList.Where(Function(x) x.KEY1 = "10").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO10_FROM") != null) document.getElementById("CONST_INFO10_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "10").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO10_TO") != null) document.getElementById("CONST_INFO10_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "10").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO11") != null) document.getElementById("CONST_INFO11").value = "@(inputItemList.Where(Function(x) x.KEY1 = "11").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO11_FROM") != null) document.getElementById("CONST_INFO11_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "11").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO11_TO") != null) document.getElementById("CONST_INFO11_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "11").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO12") != null) document.getElementById("CONST_INFO12").value = "@(inputItemList.Where(Function(x) x.KEY1 = "12").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO12_FROM") != null) document.getElementById("CONST_INFO12_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "12").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO12_TO") != null) document.getElementById("CONST_INFO12_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "12").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO13") != null) document.getElementById("CONST_INFO13").value = "@(inputItemList.Where(Function(x) x.KEY1 = "13").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO13_FROM") != null) document.getElementById("CONST_INFO13_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "13").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO13_TO") != null) document.getElementById("CONST_INFO13_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "13").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO14") != null) document.getElementById("CONST_INFO14").value = "@(inputItemList.Where(Function(x) x.KEY1 = "14").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO14_FROM") != null) document.getElementById("CONST_INFO14_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "14").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO14_TO") != null) document.getElementById("CONST_INFO14_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "14").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO15") != null) document.getElementById("CONST_INFO15").value = "@(inputItemList.Where(Function(x) x.KEY1 = "15").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO15_FROM") != null) document.getElementById("CONST_INFO15_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "15").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO15_TO") != null) document.getElementById("CONST_INFO15_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "15").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO16") != null) document.getElementById("CONST_INFO16").value = "@(inputItemList.Where(Function(x) x.KEY1 = "16").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO16_FROM") != null) document.getElementById("CONST_INFO16_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "16").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO16_TO") != null) document.getElementById("CONST_INFO16_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "16").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO17") != null) document.getElementById("CONST_INFO17").value = "@(inputItemList.Where(Function(x) x.KEY1 = "17").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO17_FROM") != null) document.getElementById("CONST_INFO17_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "17").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO17_TO") != null) document.getElementById("CONST_INFO17_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "17").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO18") != null) document.getElementById("CONST_INFO18").value = "@(inputItemList.Where(Function(x) x.KEY1 = "18").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO18_FROM") != null) document.getElementById("CONST_INFO18_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "18").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO18_TO") != null) document.getElementById("CONST_INFO18_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "18").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO19") != null) document.getElementById("CONST_INFO19").value = "@(inputItemList.Where(Function(x) x.KEY1 = "19").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO19_FROM") != null) document.getElementById("CONST_INFO19_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "19").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO19_TO") != null) document.getElementById("CONST_INFO19_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "19").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO20") != null) document.getElementById("CONST_INFO20").value = "@(inputItemList.Where(Function(x) x.KEY1 = "20").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO20_FROM") != null) document.getElementById("CONST_INFO20_FROM").value = "@(inputItemList.Where(Function(x) x.KEY1 = "20").FirstOrDefault.DATA2)";
        if (document.getElementById("CONST_INFO20_TO") != null) document.getElementById("CONST_INFO20_TO").value = "@(inputItemList.Where(Function(x) x.KEY1 = "20").FirstOrDefault.DATA2)";

        //仮置一覧テーブル初期化
        if("@ViewBag.TemporaryLeaveFlg" == "Y"){
            initializeTempListGrid();
        }

        //予約一覧テーブル初期化
        if("@ViewBag.MatchingReserveFlg" == "Y"){
            initializeReserveListGrid();
        }

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-Error");
        $("#@(Model.FirstOrDefault.ErrTarget)_FROM").removeClass("input-error");
        $("#@(Model.FirstOrDefault.ErrTarget)_TO").removeClass("input-error");

        //登録ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_CREATE_CLICK";

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //変更ボタンクリック処理
    function btnEdit_onclick(isEditMode) {

        //テーブルの選択行を取得する。
        var constNo = [];
        var updFlg = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                //「, (カンマ)」で区切って分割する
                var result = checkBoxes[i].value.split(',');
                constNo.push(result[0]);
                updFlg.push(grid.getData().getItem(i).UPD_FLG);
            }
        }

        if (constNo.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (constNo.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        } else if (updFlg[0] != "@UpdFlg.Yes" && isEditMode == true){//選択行の更新権限がない場合
            showErrMessage("@Message.getMessage(CommonLabelConstructionList.MSG_ERR_SELECTED_NOT_UPD_CONST)")
            return;
        }

        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@Message.getMessage(CommonLabelConstructionList.TITLE_CONSTUCTION_UPDATE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_UPDATE)";
        //メッセージ設定
        document.getElementById("lblCannotAddReserve").innerHTML = "";

        //項目表示状況初期化
        initializeDisable()

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        //日付チェック状態を初期化する
        $("#ConstStartDtString").removeClass("input-error")
        $("#ConstEndDtString").removeClass("input-error")
        $("#ConstLentStartDtString").removeClass("input-error")
        $("#ConstLentEndDtString").removeClass("input-error")

        //工事番号書き込み不可
        document.getElementById("CONST_NO").readOnly = true;
        $("#CONST_NO").addClass("disable");

        //削除ボタンを表示
        // 権限による処理区分
        if(("@ViewBag.AuthProcFlg" == "@AuthProcFlg.All"|| "@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiAll") && isEditMode == true){
            // 権限による処理区分が「編集・照会可能」の場合
            // 変更ボタンを表示
            $("#modal-create-btn").css("display", "block");
            $("#modal-create-btn").prop("disabled", false);
            $("#modal-create-btn").css("background-color", "");
            $("#modal-create-btn").css("cursor", "pointer");

            //削除ボタンを表示
            $("#modal-delete-btn").css("display", "block");
            $("#modal-delete-btn").prop("disabled", false);
            $("#modal-delete-btn").css("background-color", "");
            $("#modal-delete-btn").css("cursor", "default");

            //仮置き追加ボタン活性
            $("#modal-temp-add-btn").prop("disabled", false);
            $("#modal-temp-add-btn").css("background-color", "");
            $("#modal-temp-add-btn").css("cursor", "pointer");
            //仮置き解除ボタン活性
            $("#modal-release-btn").prop("disabled", false);
            $("#modal-release-btn").css("background-color", "");
            $("#modal-release-btn").css("cursor", "pointer");
            //予約追加ボタン活性
            $("#modal-reserve-add-btn").prop("disabled", false);
            $("#modal-reserve-add-btn").css("background-color", "");
            $("#modal-reserve-add-btn").css("cursor", "pointer");
            //予約個数変更ボタン活性
            $("#modal-qty-change-btn").prop("disabled", false);
            $("#modal-qty-change-btn").css("background-color", "");
            $("#modal-qty-change-btn").css("cursor", "pointer");
            //予約削除ボタン活性
            $("#modal-reserve-del-btn").prop("disabled", false);
            $("#modal-reserve-del-btn").css("background-color", "");
            $("#modal-reserve-del-btn").css("cursor", "pointer");
        }else{
            // 権限による処理区分が「照会のみ」の場合
            // 変更ボタンを非表示
            $("#modal-create-btn").css("display", "none");
            // 削除ボタンを非表示
            $("#modal-delete-btn").css("display", "none");
            // 追加、解除、個数変更ボタンを使用不可
            $("#modal-temp-add-btn").prop("disabled", true);
            $("#modal-temp-add-btn").attr('style', 'background-color:#cecece;');
            $("#modal-temp-add-btn").css("cursor", "default");
            $("#modal-release-btn").prop("disabled", true);
            $("#modal-release-btn").attr('style', 'background-color:#cecece;');
            $("#modal-release-btn").css("cursor", "default");
            $("#modal-reserve-add-btn").prop("disabled", true);
            $("#modal-reserve-add-btn").attr('style', 'background-color:#cecece;');
            $("#modal-reserve-add-btn").css("cursor", "default");
            $("#modal-qty-change-btn").prop("disabled", true);
            $("#modal-qty-change-btn").attr('style', 'background-color:#cecece;');
            $("#modal-qty-change-btn").css("cursor", "default");
            $("#modal-reserve-del-btn").prop("disabled", true);
            $("#modal-reserve-del-btn").attr('style', 'background-color:#cecece;');
            $("#modal-reserve-del-btn").css("cursor", "default");

            //各項目を非活性にする
            document.getElementById("CONST_CONTENTS").readOnly = true;
            $("#CONST_CONTENTS").addClass("disable");
            document.getElementById("ConstStartDtString").readOnly = true;
            $("#ConstStartDtString").addClass("disable");
            document.getElementById("ConstEndDtString").readOnly = true;
            $("#ConstEndDtString").addClass("disable");
            document.getElementById("ConstLentStartDtString").readOnly = true;
            $("#ConstLentStartDtString").addClass("disable");
            document.getElementById("ConstLentEndDtString").readOnly = true;
            $("#ConstLentEndDtString").addClass("disable");

            if (document.getElementById("CONST_INFO1") != null) {
                document.getElementById("CONST_INFO1").readOnly = true;
                $("#CONST_INFO1").addClass("disable");
            }else {
                document.getElementById("CONST_INFO1_FROM").readOnly = true;
                $("#CONST_INFO1_FROM").addClass("disable");
                document.getElementById("CONST_INFO1_TO").readOnly = true;
                $("#CONST_INFO1_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO2") != null) {
                document.getElementById("CONST_INFO2").readOnly = true;
                $("#CONST_INFO2").addClass("disable");
            }else {
                document.getElementById("CONST_INFO2_FROM").readOnly = true;
                $("#CONST_INFO2_FROM").addClass("disable");
                document.getElementById("CONST_INFO2_TO").readOnly = true;
                $("#CONST_INFO2_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO3") != null) {
                document.getElementById("CONST_INFO3").readOnly = true;
                $("#CONST_INFO3").addClass("disable");
            }else {
                document.getElementById("CONST_INFO3_FROM").readOnly = true;
                $("#CONST_INFO3_FROM").addClass("disable");
                document.getElementById("CONST_INFO3_TO").readOnly = true;
                $("#CONST_INFO3_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO4") != null) {
                document.getElementById("CONST_INFO4").readOnly = true;
                $("#CONST_INFO4").addClass("disable");
            }else {
                document.getElementById("CONST_INFO4_FROM").readOnly = true;
                $("#CONST_INFO4_FROM").addClass("disable");
                document.getElementById("CONST_INFO4_TO").readOnly = true;
                $("#CONST_INFO4_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO5") != null) {
                document.getElementById("CONST_INFO5").readOnly = true;
                $("#CONST_INFO5").addClass("disable");
            }else {
                document.getElementById("CONST_INFO5_FROM").readOnly = true;
                $("#CONST_INFO5_FROM").addClass("disable");
                document.getElementById("CONST_INFO5_TO").readOnly = true;
                $("#CONST_INFO5_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO6") != null) {
                document.getElementById("CONST_INFO6").readOnly = true;
                $("#CONST_INFO6").addClass("disable");
            }else {
                document.getElementById("CONST_INFO6_FROM").readOnly = true;
                $("#CONST_INFO6_FROM").addClass("disable");
                document.getElementById("CONST_INFO6_TO").readOnly = true;
                $("#CONST_INFO6_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO7") != null) {
                document.getElementById("CONST_INFO7").readOnly = true;
                $("#CONST_INFO7").addClass("disable");
            }else {
                document.getElementById("CONST_INFO7_FROM").readOnly = true;
                $("#CONST_INFO7_FROM").addClass("disable");
                document.getElementById("CONST_INFO7_TO").readOnly = true;
                $("#CONST_INFO7_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO8") != null) {
                document.getElementById("CONST_INFO8").readOnly = true;
                $("#CONST_INFO8").addClass("disable");
            }else {
                document.getElementById("CONST_INFO8_FROM").readOnly = true;
                $("#CONST_INFO8_FROM").addClass("disable");
                document.getElementById("CONST_INFO8_TO").readOnly = true;
                $("#CONST_INFO8_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO9") != null) {
                document.getElementById("CONST_INFO9").readOnly = true;
                $("#CONST_INFO9").addClass("disable");
            }else {
                document.getElementById("CONST_INFO9_FROM").readOnly = true;
                $("#CONST_INFO9_FROM").addClass("disable");
                document.getElementById("CONST_INFO9_TO").readOnly = true;
                $("#CONST_INFO9_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO10") != null) {
                document.getElementById("CONST_INFO10").readOnly = true;
                $("#CONST_INFO10").addClass("disable");
            }else {
                document.getElementById("CONST_INFO10_FROM").readOnly = true;
                $("#CONST_INFO10_FROM").addClass("disable");
                document.getElementById("CONST_INFO10_TO").readOnly = true;
                $("#CONST_INFO10_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO11") != null) {
                document.getElementById("CONST_INFO11").readOnly = true;
                $("#CONST_INFO11").addClass("disable");
            }else {
                document.getElementById("CONST_INFO11_FROM").readOnly = true;
                $("#CONST_INFO11_FROM").addClass("disable");
                document.getElementById("CONST_INFO11_TO").readOnly = true;
                $("#CONST_INFO11_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO12") != null) {
                document.getElementById("CONST_INFO12").readOnly = true;
                $("#CONST_INFO12").addClass("disable");
            }else {
                document.getElementById("CONST_INFO12_FROM").readOnly = true;
                $("#CONST_INFO12_FROM").addClass("disable");
                document.getElementById("CONST_INFO12_TO").readOnly = true;
                $("#CONST_INFO12_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO13") != null) {
                document.getElementById("CONST_INFO13").readOnly = true;
                $("#CONST_INFO13").addClass("disable");
            }else {
                document.getElementById("CONST_INFO13_FROM").readOnly = true;
                $("#CONST_INFO13_FROM").addClass("disable");
                document.getElementById("CONST_INFO13_TO").readOnly = true;
                $("#CONST_INFO13_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO14") != null) {
                document.getElementById("CONST_INFO14").readOnly = true;
                $("#CONST_INFO14").addClass("disable");
            }else {
                document.getElementById("CONST_INFO14_FROM").readOnly = true;
                $("#CONST_INFO14_FROM").addClass("disable");
                document.getElementById("CONST_INFO14_TO").readOnly = true;
                $("#CONST_INFO14_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO15") != null) {
                document.getElementById("CONST_INFO15").readOnly = true;
                $("#CONST_INFO15").addClass("disable");
            }else {
                document.getElementById("CONST_INFO15_FROM").readOnly = true;
                $("#CONST_INFO15_FROM").addClass("disable");
                document.getElementById("CONST_INFO15_TO").readOnly = true;
                $("#CONST_INFO15_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO16") != null) {
                document.getElementById("CONST_INFO16").readOnly = true;
                $("#CONST_INFO16").addClass("disable");
            }else {
                document.getElementById("CONST_INFO16_FROM").readOnly = true;
                $("#CONST_INFO16_FROM").addClass("disable");
                document.getElementById("CONST_INFO16_TO").readOnly = true;
                $("#CONST_INFO16_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO17") != null) {
                document.getElementById("CONST_INFO17").readOnly = true;
                $("#CONST_INFO17").addClass("disable");
            }else {
                document.getElementById("CONST_INFO17_FROM").readOnly = true;
                $("#CONST_INFO17_FROM").addClass("disable");
                document.getElementById("CONST_INFO17_TO").readOnly = true;
                $("#CONST_INFO17_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO18") != null) {
                document.getElementById("CONST_INFO18").readOnly = true;
                $("#CONST_INFO18").addClass("disable");
            }else {
                document.getElementById("CONST_INFO18_FROM").readOnly = true;
                $("#CONST_INFO18_FROM").addClass("disable");
                document.getElementById("CONST_INFO18_TO").readOnly = true;
                $("#CONST_INFO18_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO19") != null) {
                document.getElementById("CONST_INFO19").readOnly = true;
                $("#CONST_INFO19").addClass("disable");
            }else {
                document.getElementById("CONST_INFO19_FROM").readOnly = true;
                $("#CONST_INFO19_FROM").addClass("disable");
                document.getElementById("CONST_INFO19_TO").readOnly = true;
                $("#CONST_INFO19_TO").addClass("disable");
            }

            if (document.getElementById("CONST_INFO20") != null) {
                document.getElementById("CONST_INFO20").readOnly = true;
                $("#CONST_INFO20").addClass("disable");
            }else {
                document.getElementById("CONST_INFO20_FROM").readOnly = true;
                $("#CONST_INFO20_FROM").addClass("disable");
                document.getElementById("CONST_INFO20_TO").readOnly = true;
                $("#CONST_INFO20_TO").addClass("disable");
            }

            //コンボボックス非活性
            $("#CONST_EMPLOYEE_CD").addClass("dropdown-disabled");
            $("#MNG_LOC_L_CD").addClass("dropdown-disabled");
        };

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        $("#@(Model.FirstOrDefault.ErrTarget)_FROM").removeClass("input-error");
        $("#@(Model.FirstOrDefault.ErrTarget)_TO").removeClass("input-error");

        // 工事番号を黒色のフォント表示（工事番号自動生成で別色に変更したことの復帰処理)
        document.getElementById("CONST_NO").style.color = "black";

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/ConstructionList")/GetEditData',
                cache: false,
                data: { ID: constNo[0] },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }

                    //取得したデータを表示する
                    document.getElementById("CONST_NO").value = data.CONST_NO;
                    document.getElementById("ConstStartDtString").value = data.ConstStartDtString;
                    document.getElementById("ConstEndDtString").value = data.ConstEndDtString;
                    document.getElementById("BeforeConstLentStartDtString").value = data.BeforeConstLentStartDtString;
                    document.getElementById("BeforeConstLentEndDtString").value = data.BeforeConstLentEndDtString;
                    document.getElementById("ConstLentStartDtString").value = data.ConstLentStartDtString;
                    document.getElementById("ConstLentEndDtString").value = data.ConstLentEndDtString;
                    document.getElementById("CONST_CONTENTS").value = data.CONST_CONTENTS;
                    document.getElementById("CONST_EMPLOYEE_CD").value = data.CONST_EMPLOYEE_CD;
                    document.getElementById("MNG_LOC_L_CD").value = data.MNG_LOC_L_CD;

                    // 照会モードで開く時、管理ロケーションドロップダウンリストの項目を再設定する。
                    var LocMngLocL = ""
                    LocMngLocL =document.getElementById("MNG_LOC_L_CD");
                    if (!isEditMode && data.MnglocLDropdownData != null){
                        LocMngLocL.options.length = 0;
                        let opBlank = document.createElement("option");
                        opBlank.value = "";
                        opBlank.text = "";
                        LocMngLocL.appendChild(opBlank);
                        for (var i = 0; i < data.MnglocLDropdownData.length; i++) {
                            let op = document.createElement("option");
                            op.value = data.MnglocLDropdownData[i].LOC_CD;
                            op.text = data.MnglocLDropdownData[i].LOC_NAME;
                            LocMngLocL.appendChild(op);
                        }
                    }

                    if (document.getElementById("CONST_INFO1") != null) {
                        document.getElementById("CONST_INFO1").value = data.CONST_INFO1;
                    } else {
                        document.getElementById("CONST_INFO1_FROM").value = data.CONST_INFO1_FROM;
                        document.getElementById("CONST_INFO1_TO").value = data.CONST_INFO1_TO;
                    }
                    if (document.getElementById("CONST_INFO2") != null) {
                        document.getElementById("CONST_INFO2").value = data.CONST_INFO2;
                    } else {
                        document.getElementById("CONST_INFO2_FROM").value = data.CONST_INFO2_FROM;
                        document.getElementById("CONST_INFO2_TO").value = data.CONST_INFO2_TO;
                    }
                    if (document.getElementById("CONST_INFO3") != null) {
                        document.getElementById("CONST_INFO3").value = data.CONST_INFO3;
                    } else {
                        document.getElementById("CONST_INFO3_FROM").value = data.CONST_INFO3_FROM;
                        document.getElementById("CONST_INFO3_TO").value = data.CONST_INFO3_TO;
                    }
                    if (document.getElementById("CONST_INFO4") != null) {
                        document.getElementById("CONST_INFO4").value = data.CONST_INFO4;
                    } else {
                        document.getElementById("CONST_INFO4_FROM").value = data.CONST_INFO4_FROM;
                        document.getElementById("CONST_INFO4_TO").value = data.CONST_INFO4_TO;
                    }
                    if (document.getElementById("CONST_INFO5") != null) {
                        document.getElementById("CONST_INFO5").value = data.CONST_INFO5;
                    } else {
                        document.getElementById("CONST_INFO5_FROM").value = data.CONST_INFO5_FROM;
                        document.getElementById("CONST_INFO5_TO").value = data.CONST_INFO5_TO;
                    }
                    if (document.getElementById("CONST_INFO6") != null) {
                        document.getElementById("CONST_INFO6").value = data.CONST_INFO6;
                    } else {
                        document.getElementById("CONST_INFO6_FROM").value = data.CONST_INFO6_FROM;
                        document.getElementById("CONST_INFO6_TO").value = data.CONST_INFO6_TO;
                    }
                    if (document.getElementById("CONST_INFO7") != null) {
                        document.getElementById("CONST_INFO7").value = data.CONST_INFO7;
                    } else {
                        document.getElementById("CONST_INFO7_FROM").value = data.CONST_INFO7_FROM;
                        document.getElementById("CONST_INFO7_TO").value = data.CONST_INFO7_TO;
                    }
                    if (document.getElementById("CONST_INFO8") != null) {
                        document.getElementById("CONST_INFO8").value = data.CONST_INFO8;
                    } else {
                        document.getElementById("CONST_INFO8_FROM").value = data.CONST_INFO8_FROM;
                        document.getElementById("CONST_INFO8_TO").value = data.CONST_INFO8_TO;
                    }
                    if (document.getElementById("CONST_INFO9") != null) {
                        document.getElementById("CONST_INFO9").value = data.CONST_INFO9;
                    } else {
                        document.getElementById("CONST_INFO9_FROM").value = data.CONST_INFO9_FROM;
                        document.getElementById("CONST_INFO9_TO").value = data.CONST_INFO9_TO;
                    }
                    if (document.getElementById("CONST_INFO10") != null) {
                        document.getElementById("CONST_INFO10").value = data.CONST_INFO10;
                    } else {
                        document.getElementById("CONST_INFO10_FROM").value = data.CONST_INFO10_FROM;
                        document.getElementById("CONST_INFO10_TO").value = data.CONST_INFO10_TO;
                    }
                    if (document.getElementById("CONST_INFO11") != null) {
                        document.getElementById("CONST_INFO11").value = data.CONST_INFO11;
                    } else {
                        document.getElementById("CONST_INFO11_FROM").value = data.CONST_INFO11_FROM;
                        document.getElementById("CONST_INFO11_TO").value = data.CONST_INFO11_TO;
                    }
                    if (document.getElementById("CONST_INFO12") != null) {
                        document.getElementById("CONST_INFO12").value = data.CONST_INFO12;
                    } else {
                        document.getElementById("CONST_INFO12_FROM").value = data.CONST_INFO12_FROM;
                        document.getElementById("CONST_INFO12_TO").value = data.CONST_INFO12_TO;
                    }
                    if (document.getElementById("CONST_INFO13") != null) {
                        document.getElementById("CONST_INFO13").value = data.CONST_INFO13;
                    } else {
                        document.getElementById("CONST_INFO13_FROM").value = data.CONST_INFO13_FROM;
                        document.getElementById("CONST_INFO13_TO").value = data.CONST_INFO13_TO;
                    }
                    if (document.getElementById("CONST_INFO14") != null) {
                        document.getElementById("CONST_INFO14").value = data.CONST_INFO14;
                    } else {
                        document.getElementById("CONST_INFO14_FROM").value = data.CONST_INFO14_FROM;
                        document.getElementById("CONST_INFO14_TO").value = data.CONST_INFO14_TO;
                    }
                    if (document.getElementById("CONST_INFO15") != null) {
                        document.getElementById("CONST_INFO15").value = data.CONST_INFO15;
                    } else {
                        document.getElementById("CONST_INFO15_FROM").value = data.CONST_INFO15_FROM;
                        document.getElementById("CONST_INFO15_TO").value = data.CONST_INFO15_TO;
                    }
                    if (document.getElementById("CONST_INFO16") != null) {
                        document.getElementById("CONST_INFO16").value = data.CONST_INFO16;
                    } else {
                        document.getElementById("CONST_INFO16_FROM").value = data.CONST_INFO16_FROM;
                        document.getElementById("CONST_INFO16_TO").value = data.CONST_INFO16_TO;
                    }
                    if (document.getElementById("CONST_INFO17") != null) {
                        document.getElementById("CONST_INFO17").value = data.CONST_INFO17;
                    } else {
                        document.getElementById("CONST_INFO17_FROM").value = data.CONST_INFO17_FROM;
                        document.getElementById("CONST_INFO17_TO").value = data.CONST_INFO17_TO;
                    }
                    if (document.getElementById("CONST_INFO18") != null) {
                        document.getElementById("CONST_INFO18").value = data.CONST_INFO18;
                    } else {
                        document.getElementById("CONST_INFO18_FROM").value = data.CONST_INFO18_FROM;
                        document.getElementById("CONST_INFO18_TO").value = data.CONST_INFO18_TO;
                    }
                    if (document.getElementById("CONST_INFO19") != null) {
                        document.getElementById("CONST_INFO19").value = data.CONST_INFO19;
                    } else {
                        document.getElementById("CONST_INFO19_FROM").value = data.CONST_INFO19_FROM;
                        document.getElementById("CONST_INFO19_TO").value = data.CONST_INFO19_TO;
                    }
                    if (document.getElementById("CONST_INFO20") != null) {
                        document.getElementById("CONST_INFO20").value = data.CONST_INFO20;
                    } else {
                        document.getElementById("CONST_INFO20_FROM").value = data.CONST_INFO20_FROM;
                        document.getElementById("CONST_INFO20_TO").value = data.CONST_INFO20_TO;
                    }

                    document.getElementById("CONST_STATUS").value = data.CONST_STATUS;
                    document.getElementById("PREV_TEMP_NUM").value = data.PREV_TEMP_NUM;
                    document.getElementById("CONST_IN_NUM").value = data.CONST_IN_NUM;
                    document.getElementById("NOW_TEMP_NUM").value = data.NOW_TEMP_NUM;
                    document.getElementById("CONST_OUT_NUM").value = data.CONST_OUT_NUM;
                    document.getElementById("CUR_LENT_NUM").value = data.CUR_LENT_NUM;
                    document.getElementById("CUR_TEMP_NUM").value = data.CUR_TEMP_NUM;
                    document.getElementById("SITE_EMPLOYEE_CD").value = data.SITE_EMPLOYEE_CD;
                    document.getElementById("SITE_EMPLOYEE_NAME").value = data.SITE_EMPLOYEE_NAME;
                    document.getElementById("CONST_CONFIRM_TS").value = data.CONST_CONFIRM_TS;
                    document.getElementById("CONST_CONFIRM_EMPLOYEE_CD").value = data.CONST_CONFIRM_EMPLOYEE_CD;
                    document.getElementById("CONST_CONFIRM_EMPLOYEE_NAME").value = data.CONST_CONFIRM_EMPLOYEE_NAME;
                    document.getElementById("CONST_CONFIRM_REMARK").value = data.CONST_CONFIRM_REMARK;
                    document.getElementById("TASK_TS").value = data.TASK_TS;
                    document.getElementById("INS_DATETIME").value = formatDate(new Date(data.InsDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("INS_EMPLOYEE_CD").value = data.INS_EMPLOYEE_CD;
                    document.getElementById("INS_EMPLOYEE_NAME").value = data.INS_EMPLOYEE_NAME;
                    document.getElementById("UPD_DATETIME").value = formatDate(new Date(data.UpdDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("UPD_EMPLOYEE_CD").value = data.UPD_EMPLOYEE_CD;
                    document.getElementById("UPD_EMPLOYEE_NAME").value = data.UPD_EMPLOYEE_NAME;
                    document.getElementById("UPD_TASK").value = data.RenewProcess;

                    //工事状態:完了の場合
                    if(String(data.CONST_STATUS) == "@CONST_STATUS.Complete"){
                        // 戻る以外のボタンを使用不可
                        $("#modal-create-btn").prop("disabled", true);
                        $("#modal-create-btn").attr('style', 'background-color:#cecece;');
                        $("#modal-create-btn").css("cursor", "default");
                        $("#modal-delete-btn").prop("disabled", true);
                        $("#modal-delete-btn").attr('style', 'background-color:#cecece;');
                        $("#modal-delete-btn").css("cursor", "default");
                        $("#modal-temp-add-btn").prop("disabled", true);
                        $("#modal-temp-add-btn").attr('style', 'background-color:#cecece;');
                        $("#modal-temp-add-btn").css("cursor", "default");
                        $("#modal-release-btn").prop("disabled", true);
                        $("#modal-release-btn").attr('style', 'background-color:#cecece;');
                        $("#modal-release-btn").css("cursor", "default");
                        $("#modal-reserve-add-btn").prop("disabled", true);
                        $("#modal-reserve-add-btn").attr('style', 'background-color:#cecece;');
                        $("#modal-reserve-add-btn").css("cursor", "default");
                        $("#modal-qty-change-btn").prop("disabled", true);
                        $("#modal-qty-change-btn").attr('style', 'background-color:#cecece;');
                        $("#modal-qty-change-btn").css("cursor", "default");
                        $("#modal-reserve-del-btn").prop("disabled", true);
                        $("#modal-reserve-del-btn").attr('style', 'background-color:#cecece;');
                        $("#modal-reserve-del-btn").css("cursor", "default");
                    }

                    //仮置き数テキスト表示フラグ
                    var textFlg = (String(data.CONST_STATUS) == "@CONST_STATUS.Complete" || !isEditMode) ? true : false;

                    //仮置一覧テーブル初期化
                    if("@ViewBag.TemporaryLeaveFlg" == "Y"){
                        initializeTempListGrid(data.CONST_NO, textFlg);
                    }

                    //予約一覧テーブル初期化
                    if("@ViewBag.MatchingReserveFlg" == "Y"){
                        initializeReserveListGrid(data.CONST_NO);
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }

        //変更ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_UPDATE_CLICK";

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //登録、変更画面の項目表示状態初期化
    function initializeDisable(){
        //全ての項目の表示状態をリセット
        //各項目を活性にする
        document.getElementById("CONST_NO").readOnly = false;
        $("#CONST_NO").removeClass("disable");
        document.getElementById("CONST_CONTENTS").readOnly = false;
        $("#CONST_CONTENTS").removeClass("disable");
        document.getElementById("ConstStartDtString").readOnly = false;
        $("#ConstStartDtString").removeClass("disable");
        document.getElementById("ConstEndDtString").readOnly = false;
        $("#ConstEndDtString").removeClass("disable");
        document.getElementById("ConstLentStartDtString").readOnly = false;
        $("#ConstLentStartDtString").removeClass("disable");
        document.getElementById("ConstLentEndDtString").readOnly = false;
        $("#ConstLentEndDtString").removeClass("disable");

        if (document.getElementById("CONST_INFO1") != null) {
            document.getElementById("CONST_INFO1").readOnly = false;
            $("#CONST_INFO1").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO1_FROM").readOnly = false;
            $("#CONST_INFO1_FROM").removeClass("disable");
            document.getElementById("CONST_INFO1_TO").readOnly = false;
            $("#CONST_INFO1_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO2") != null) {
            document.getElementById("CONST_INFO2").readOnly = false;
            $("#CONST_INFO2").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO2_FROM").readOnly = false;
            $("#CONST_INFO2_FROM").removeClass("disable");
            document.getElementById("CONST_INFO2_TO").readOnly = false;
            $("#CONST_INFO2_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO3") != null) {
            document.getElementById("CONST_INFO3").readOnly = false;
            $("#CONST_INFO3").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO3_FROM").readOnly = false;
            $("#CONST_INFO3_FROM").removeClass("disable");
            document.getElementById("CONST_INFO3_TO").readOnly = false;
            $("#CONST_INFO3_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO4") != null) {
            document.getElementById("CONST_INFO4").readOnly = false;
            $("#CONST_INFO4").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO4_FROM").readOnly = false;
            $("#CONST_INFO4_FROM").removeClass("disable");
            document.getElementById("CONST_INFO4_TO").readOnly = false;
            $("#CONST_INFO4_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO5") != null) {
            document.getElementById("CONST_INFO5").readOnly = false;
            $("#CONST_INFO5").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO5_FROM").readOnly = false;
            $("#CONST_INFO5_FROM").removeClass("disable");
            document.getElementById("CONST_INFO5_TO").readOnly = false;
            $("#CONST_INFO5_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO6") != null) {
            document.getElementById("CONST_INFO6").readOnly = false;
            $("#CONST_INFO6").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO6_FROM").readOnly = false;
            $("#CONST_INFO6_FROM").removeClass("disable");
            document.getElementById("CONST_INFO6_TO").readOnly = false;
            $("#CONST_INFO6_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO7") != null) {
            document.getElementById("CONST_INFO7").readOnly = false;
            $("#CONST_INFO7").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO7_FROM").readOnly = false;
            $("#CONST_INFO7_FROM").removeClass("disable");
            document.getElementById("CONST_INFO7_TO").readOnly = false;
            $("#CONST_INFO7_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO8") != null) {
            document.getElementById("CONST_INFO8").readOnly = false;
            $("#CONST_INFO8").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO8_FROM").readOnly = false;
            $("#CONST_INFO8_FROM").removeClass("disable");
            document.getElementById("CONST_INFO8_TO").readOnly = false;
            $("#CONST_INFO8_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO9") != null) {
            document.getElementById("CONST_INFO9").readOnly = false;
            $("#CONST_INFO9").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO9_FROM").readOnly = false;
            $("#CONST_INFO9_FROM").removeClass("disable");
            document.getElementById("CONST_INFO9_TO").readOnly = false;
            $("#CONST_INFO9_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO10") != null) {
            document.getElementById("CONST_INFO10").readOnly = false;
            $("#CONST_INFO10").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO10_FROM").readOnly = false;
            $("#CONST_INFO10_FROM").removeClass("disable");
            document.getElementById("CONST_INFO10_TO").readOnly = false;
            $("#CONST_INFO10_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO11") != null) {
            document.getElementById("CONST_INFO11").readOnly = false;
            $("#CONST_INFO11").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO11_FROM").readOnly = false;
            $("#CONST_INFO11_FROM").removeClass("disable");
            document.getElementById("CONST_INFO11_TO").readOnly = false;
            $("#CONST_INFO11_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO12") != null) {
            document.getElementById("CONST_INFO12").readOnly = false;
            $("#CONST_INFO12").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO12_FROM").readOnly = false;
            $("#CONST_INFO12_FROM").removeClass("disable");
            document.getElementById("CONST_INFO12_TO").readOnly = false;
            $("#CONST_INFO12_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO13") != null) {
            document.getElementById("CONST_INFO13").readOnly = false;
            $("#CONST_INFO13").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO13_FROM").readOnly = false;
            $("#CONST_INFO13_FROM").removeClass("disable");
            document.getElementById("CONST_INFO13_TO").readOnly = false;
            $("#CONST_INFO13_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO14") != null) {
            document.getElementById("CONST_INFO14").readOnly = false;
            $("#CONST_INFO14").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO14_FROM").readOnly = false;
            $("#CONST_INFO14_FROM").removeClass("disable");
            document.getElementById("CONST_INFO14_TO").readOnly = false;
            $("#CONST_INFO14_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO15") != null) {
            document.getElementById("CONST_INFO15").readOnly = false;
            $("#CONST_INFO15").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO15_FROM").readOnly = false;
            $("#CONST_INFO15_FROM").removeClass("disable");
            document.getElementById("CONST_INFO15_TO").readOnly = false;
            $("#CONST_INFO15_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO16") != null) {
            document.getElementById("CONST_INFO16").readOnly = false;
            $("#CONST_INFO16").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO16_FROM").readOnly = false;
            $("#CONST_INFO16_FROM").removeClass("disable");
            document.getElementById("CONST_INFO16_TO").readOnly = false;
            $("#CONST_INFO16_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO17") != null) {
            document.getElementById("CONST_INFO17").readOnly = false;
            $("#CONST_INFO17").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO17_FROM").readOnly = false;
            $("#CONST_INFO17_FROM").removeClass("disable");
            document.getElementById("CONST_INFO17_TO").readOnly = false;
            $("#CONST_INFO17_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO18") != null) {
            document.getElementById("CONST_INFO18").readOnly = false;
            $("#CONST_INFO18").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO18_FROM").readOnly = false;
            $("#CONST_INFO18_FROM").removeClass("disable");
            document.getElementById("CONST_INFO18_TO").readOnly = false;
            $("#CONST_INFO18_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO19") != null) {
            document.getElementById("CONST_INFO19").readOnly = false;
            $("#CONST_INFO19").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO19_FROM").readOnly = false;
            $("#CONST_INFO19_FROM").removeClass("disable");
            document.getElementById("CONST_INFO19_TO").readOnly = false;
            $("#CONST_INFO19_TO").removeClass("disable");
        }

        if (document.getElementById("CONST_INFO20") != null) {
            document.getElementById("CONST_INFO20").readOnly = false;
            $("#CONST_INFO20").removeClass("disable");
        }else {
            document.getElementById("CONST_INFO20_FROM").readOnly = false;
            $("#CONST_INFO20_FROM").removeClass("disable");
            document.getElementById("CONST_INFO20_TO").readOnly = false;
            $("#CONST_INFO20_TO").removeClass("disable");
        }

        //コンボボックス活性
        $("#CONST_EMPLOYEE_CD").removeClass("dropdown-disabled");
        $("#MNG_LOC_L_CD").removeClass("dropdown-disabled");

    }

    //仮置一覧テーブル初期化
    function initializeTempListGrid(constNo, textFlg) {
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstructionList")/GetTemporaryDataByConstNo',
            cache: false,
            data: { sConstNo: constNo },
            success: function (data, dataType) {
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // 列定義
                let columns = [
                    { id: "@ColKey.ConstructionList.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", width: 40, sortOrder: 1, formatter: TempListCheckboxFormatter, cssClass: "first-column", resizable: false },
                    { name: "@Message.getMessage(CommonLabel.LABEL_TEMP_DT)", field: "TEMP_DT", width: 140, sortOrder: 2, formatter:SlashYMDFormatter, resizable: false},
                    { name: "@Message.getMessage(CommonLabelConstructionList.LABEL_CONST_PIC)", field: "WORK_EMPLOYEE_NAME", width: 140, sortOrder: 3, resizable: false},
                    { name: "@Message.getMessage(CommonLabel.LABEL_WORK_CONTENTS)", field: "CONST_CONTENTS", width: 200, sortOrder: 4, resizable: false},
                    { name: "@Message.getMessage(CommonLabel.LABEL_TEMP_STORAGE_PLACE)", field: "TEMP_INFO1", width: 230, sortOrder: 5, formatter: TempInfoFormatter, resizable: false},
                    { name: "@Message.getMessage(CommonLabel.LABEL_TEMP_NUM)", field: "TEMP_NUM", width: 150, sortOrder: 6, formatter: TempNumFormatter, resizable: false },
                ];

                // Gridのオプション機能の設定
                let options = {
                    enableCellNavigation: true,             //グリッド内のセル間移動可否
                    enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
                    multiColumnSort: false,                 //複数列のソート可否
                    autoHeight: false,                      //Gridの高さの自動調整可否
                    editable: false,                        //セルの編集可否
                    autoCommitEdit: false,                  //セル編集の自動コミット可否
                };

                // 安全に使用できるようにJSON文字列をエンコード
                let jsonData = data;

                // Gridのデータ管理オブジェクトにセット
                let dataView = new Slick.Data.DataView();

                // Grid表示判定
                if (data && data.length > 0) {
                    dataView.beginUpdate();
                    dataView.setItems(jsonData, "TEMP_ID");
                    dataView.endUpdate();
                }

                // Grid初期化
                gridTempList = initializeGrid("TemporaryListGrids", dataView, columns, options, "", 0);

                if(textFlg){
                    $(".slick-cell > a").removeClass("blue-link");
                    $(".slick-cell > a").css("pointer-events","None");
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //予約情報一覧テーブル初期化
    function initializeReserveListGrid(constNo) {
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstructionList")/GetReserveDataByConstNo',
            cache: false,
            data: { sConstNo: constNo},
            success: function (data, dataType) {
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // 列定義
                let columns = [
                    { id: "@ColKey.ConstructionList.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", width: 40, sortOrder: 1, formatter: ReserveListCheckboxFormatter, cssClass: "first-column", resizable: false },
                    { name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)", field: "PRODUCT_NO", width: 140, sortOrder: 2, resizable: false},
                    { name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NAME)", field: "ASSET_NAME", width: 300, sortOrder: 3, resizable: false},
                    { name: "@Message.getMessage(CommonLabel.LABEL_RESERVATION_NUM)", field: "RESERVATION_NUM", width: 140, sortOrder: 4, resizable: false},
                    { name: "@Message.getMessage(CommonLabel.LABEL_AVAILABLE_RESERVATION_NUM)", field: "AVAILABLE_RESERVATION_NUM", width: 140, sortOrder: 5, resizable: false},
                    { name: "@Message.getMessage(CommonLabel.LABEL_ERROR)", width: 140, sortOrder: 5, formatter: ErrorFormatter, resizable: false},
                ];

                // Gridのオプション機能の設定
                let options = {
                    enableCellNavigation: true,             //グリッド内のセル間移動可否
                    enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
                    multiColumnSort: false,                 //複数列のソート可否
                    autoHeight: false,                      //Gridの高さの自動調整可否
                    editable: false,                        //セルの編集可否
                    autoCommitEdit: false,                  //セル編集の自動コミット可否
                };

                // 安全に使用できるようにJSON文字列をエンコード
                let jsonData = data;

                // Gridのデータ管理オブジェクトにセット
                let dataView = new Slick.Data.DataView();

                // Grid表示判定
                if (data && data.length > 0) {
                    dataView.beginUpdate();
                    dataView.setItems(jsonData, "PRODUCT_NO");
                    dataView.endUpdate();
                }

                // 行やセルにメタデータをセットするためのイベント
                dataView.getItemMetadata = function(row) {
                    let item = this.getItem(row);

                    // 予約数量
                    if (item.RESERVATION_NUM > item.AVAILABLE_RESERVATION_NUM) {
                        return {cssClasses: 'slick-quantity-shortage'};
                    }

                    return null;
                }

                // Grid初期化
                gridReserveList = initializeGrid("ReserveListGrids", dataView, columns, options, "", 0);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //資産予約画面一覧テーブル初期化
    function initializeAssetReserveListGrid(productNo) {
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstructionList")/SearchReserveData',
            cache: false,
            data: { sConstNo: document.getElementById("CONST_NO").value, sProductNo: document.getElementById("SearchProductNo").value, sAssetName: document.getElementById("SearchAssetName").value },
            success: function (data, dataType) {
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // 列定義
                let columns = [
                    { id: "@ColKey.ConstructionList.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: 40, sortOrder: 1, formatter: AssetReserveCheckboxFormatter, cssClass: "first-column", resizable: false },
                    { name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)", field: "PRODUCT_NO", width: 140, sortOrder: 2, resizable: false},
                    { name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NAME)", field: "ASSET_NAME", width: 140, sortOrder: 3, resizable: false},
                    { name: "@Message.getMessage(CommonLabelInventorySummary.LABEL_NUMBER_OF_ASSETS)", field: "ASSET_NUM", width: 100, sortOrder: 4, formatter: RightAlignIfNullFormatter, resizable: false},
                    { name: "@Message.getMessage(CommonLabel.LABEL_AVAILABLE_RESERVATION_NUM)", field: "AVAILABLE_RESERVATION_NUM", width: 100, sortOrder: 5, formatter: RightAlignIfNullFormatter, resizable: false},
                ];

                // Gridのオプション機能の設定
                let options = {
                    enableCellNavigation: true,             //グリッド内のセル間移動可否
                    enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
                    multiColumnSort: false,                 //複数列のソート可否
                    autoHeight: false,                      //Gridの高さの自動調整可否
                    editable: false,                        //セルの編集可否
                    autoCommitEdit: false,                  //セル編集の自動コミット可否
                };

                // 安全に使用できるようにJSON文字列をエンコード
                let jsonData = data;

                // 選択状態を復元
                for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                    if (productNo === jsonData[grid_i].PRODUCT_NO) {
                        jsonData[grid_i].DISP_FLAG = productNo
                        break;
                    }
                }

                // Gridのデータ管理オブジェクトにセット
                let dataView = new Slick.Data.DataView();

                // Grid表示判定
                if (data && data.length > 0) {
                    dataView.beginUpdate();
                    dataView.setItems(jsonData, "PRODUCT_NO");
                    dataView.endUpdate();
                }

                // Grid初期化
                gridAssetReserveList = initializeGrid("AssetReserveListGrids", dataView, columns, options, "", 0);
                gridReserve = gridAssetReserveList;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    // 仮置情報:追加ボタンクリック処理
    function btnTempAdd_onclick() {
        document.getElementById("SelectedConstNo").value = document.getElementById("CONST_NO").value;

        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/ConstructionList")/ShowTemporaryList");
        $('#form').submit();
        $('#form').attr("action", befAction);
        return true;
    }

    // 仮置情報:解除ボタンクリック処理
    function btnTempRelease_onclick() {
        var keys = [];
        var nonekeys = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(gridTempList.getContainerNode()).find(".slick-checkbox");

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

        if (keys.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }else if (keys.length > 1) { //複数行選択時場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        //確認メッセージ
        showConfirmOkEvt("@Message.getMessage(CommonLabel.MSG_RELEASE_CONFIRM)").then(function () {
            // カンマ区切りの文字列を設定
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/ConstructionList")/ReleaseTemp',
                dataType: 'json',
                data: { SelectKeys: keys.join(","), ConstNo: document.getElementById("CONST_NO").value},
                cache: false,
                success: function (data, dataType) {
                    //仮置一覧テーブル初期化
                    initializeTempListGrid(document.getElementById("CONST_NO").value);
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        },
            function() {
                return true;
            }
        );

        return true;
    }

    // 仮置情報:仮置き数クリック処理
    function linkTempNum_onclick(TempId) {
        document.getElementById("SelectedConstNo").value = document.getElementById("CONST_NO").value;
        document.getElementById("SelectedTempId").value = TempId;

        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/ConstructionList")/ShowTempAssetList");
        $('#form').submit();
        $('#form').attr("action", befAction);
        return true;
    }

    // 予約情報:追加ボタンクリック処理
    function btnReserveAdd_onclick() {
        //画面を初期化する
        document.getElementById("popup-title-asset-reserve").innerHTML = "@Message.getMessage(CommonLabelConstructionList.TITLE_ASSET_RESERVE)";
        document.getElementById("SearchProductNo").value = "";
        document.getElementById("SearchAssetName").value = "";
        document.getElementById("ReserveNum").value = "";

        //モーダルウインドウを開く
        ShowAssetReserveModalWindow();

        //初期検索処理
        initializeAssetReserveListGrid();
        return true;
    }

    // 予約情報:個数変更ボタンクリック処理
    function btnQtyChange_onclick() {

        var keys = [];
        var nonekeys = [];
        var reserveNum = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(gridReserveList.getContainerNode()).find(".slick-checkbox");

        for (var i = 0; i < checkBoxes.length; i++) {
            //チェックボックスの場合
            if (checkBoxes[i].type == "checkbox") {
                //キーを取得する
                var result = checkBoxes[i].value

                if (checkBoxes[i].checked) {
                    //選択されている場合
                    keys.push(result);
                    reserveNum.push(gridReserveList.getData().getItemById(result).RESERVATION_NUM);
                } else {
                    //選択されていない場合
                    nonekeys.push(result);
                }
            }
        }

        if (keys.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (keys.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }

        //画面を初期化する
        document.getElementById("popup-title-asset-reserve").innerHTML = "@Message.getMessage(CommonLabelConstructionList.TITLE_ASSET_RESERVE)";
        document.getElementById("SearchProductNo").value = "";
        document.getElementById("SearchAssetName").value = "";
        document.getElementById("ReserveNum").value = reserveNum[0];

        //モーダルウインドウを開く
        ShowAssetReserveModalWindow();

        //初期検索処理
        initializeAssetReserveListGrid(keys[0]);
        return true;
    }

    // 予約情報:解除ボタンクリック処理
    function btnReserveRelease_onclick() {

        var keys = [];
        var nonekeys = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(gridReserveList.getContainerNode()).find(".slick-checkbox");

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

        if (keys.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }

        //確認メッセージ
        showConfirmOkEvt("@Message.getMessage(CommonLabel.MSG_RELEASE_CONFIRM)").then(function () {
            document.getElementById("SelectedProductNo").value = keys.join(",");
            document.getElementById("ReserveNum").value = 0;

            // カンマ区切りの文字列を設定
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/ConstructionList")/AddAssetReserve',
                dataType: 'json',
                data: $("#form").serialize(),
                cache: false,
                processData: false,
                success: function (data, dataType) {
                    if (data == null) {
                        return;
                    } else if (data == 'ok'){
                        // 登録成功
                        //予約一覧テーブル初期化
                        initializeReserveListGrid(document.getElementById("CONST_NO").value);
                    } else {
                        // 登録失敗
                        showErrMessage(data)
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $(location).attr('href', '@Url.Content("~/Error")/Index');
                }
            });
        },
            function() {
                return true;
            }
        );

        return true;
    }

    //削除ボタンクリック処理
    function btnDelete_onclick() {

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        // 確認ダイアログ表示チェック
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/ConstructionList")/ShowDeleteDialog',
            dataType: 'json',
            data: { constNo: document.getElementById("CONST_NO").value},
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                } else {
                    // 仮置きテーブルの登録工事番号に一致するデータがあればエラーメッセージ
                    if(data == "error"){
                        showErrMessage("@Message.getMessage(CommonLabelConstructionList.MSG_EXIST_REG_CONST_NO)")
                        return false;
                    }

                    //確認メッセージを出し、ＯＫなら削除処理
                    // ダイアログのメッセージを設定
                    $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)");

                    // ダイアログを作成
                    $("#show-dialog").dialog({
                        modal: true,
                        title: strConfirmTitle,
                        dialogClass: "show-confirm-wrap",
                        buttons: {
                            "OK": function () {
                                var befAction = $('#form').attr("action");
                                $(this).dialog("close");
                                $('#form').attr("action", "@Url.Content("~/ConstructionList")/Delete");
                                $('#form').submit();
                                $('#form').attr("action", befAction);

                                //チェックボックスの状態からキーを設定する
                                setting_checklist();

                                return true;
                            },
                            "Cancel": function () {
                                $(this).dialog("close");
                                return false;
                            }
                        },
                        open: function () {      // キャンセルボタンにフォーカスをあてる
                            $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').focus();
                        }
                    });
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });

        return true;
    }

    //資産予約:登録ボタン押下時
    function btnReserveRegist_onclick() {
        var keys = [];
        var nonekeys = [];
        var selected = -1;
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(gridAssetReserveList.getContainerNode()).find(".slick-checkbox");

        for (var i = 0; i < checkBoxes.length; i++) {
            //チェックボックスの場合
            if (checkBoxes[i].type == "checkbox") {
                //キーを取得する
                var result = checkBoxes[i].value

                if (checkBoxes[i].checked) {
                    //選択されている場合
                    keys.push(result);
                    selected = i;
                } else {
                    //選択されていない場合
                    nonekeys.push(result);
                }
            }
        }

        if (keys.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (keys.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }

        //予約数チェック状態を初期化する
        $("#ReserveNum").removeClass("input-error")

        //予約数チェック
        if(document.getElementById("ReserveNum").value == ""){
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_NOT_ENTERD)")
            $("#ReserveNum").addClass("input-error");
            return false;
        }

        let dataViewRes = gridReserve.getData();
        /*let possibleNum = dataViewRes.getItem(selected)["AVAILABLE_RESERVATION_NUM"];*/
        /*let warnMainteNum = dataViewRes.getItem(selected)["WARN_MAINTE_NUM"];*/
        let allData = dataViewRes.getItems();
        let selectedItem = allData.find(item => item.PRODUCT_NO === keys[0]);
        let warnMainteAllNum = selectedItem.WARN_MAINTE_NUM;
        let possibleNum = selectedItem.AVAILABLE_RESERVATION_NUM;
        if(document.getElementById("ReserveNum").value > possibleNum){
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_RESERVE_POSSIBLE_OVER)")
            $("#ReserveNum").addClass("input-error");
            return false;
        }

        //警告メンテナンスデータがある場合は確認メッセージを変える
        let confirumMessage = "";
        //予約されない数 = 予約可能数 - 追加予定数
        let num = (possibleNum - Number(document.getElementById("ReserveNum").value)).toString()
        //警告メンテナンス期間に予約する件数
        let warnMainteNum = '0';

        //警告数の合計
        if(warnMainteAllNum != "0" && warnMainteAllNum >= num){
            //実際に予約される警告数 = 警告数の合計 - 予約されない数
            warnMainteNum = (warnMainteAllNum - num).toString();
        }

        if (warnMainteNum != "0") {
            confirumMessage = "@Message.getMessage(CommonLabel.MSG_INS_CONFIRM_WARN_MAINTE_NUM)";
            confirumMessage = wardReplace(confirumMessage, warnMainteNum.toString());
        } else {
            confirumMessage = "@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)";
        }

        //確認メッセージ
        showConfirmOkEvt(confirumMessage).then(function () {
            document.getElementById("SelectedProductNo").value = keys[0];

            // カンマ区切りの文字列を設定
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/ConstructionList")/AddAssetReserve',
                dataType: 'json',
                data: $("#form").serialize(),
                cache: false,
                processData: false,
                success: function (data, dataType) {
                    if (data == null) {
                        return;
                    } else if (data == 'ok'){
                        // 登録成功
                        showMessageAndFunc("@Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE)", function () {
                            //[#modal-asset-reserve-content]をフェードアウトした後
                            $("#modal-asset-reserve-content").fadeOut("slow", function () {
                                //[#modal-overlay]の位置指定を変更（背面に移動）
                                $("#modal-overlay").css({ "z-index": 1 });
                            });
                            //予約一覧テーブル初期化
                            initializeReserveListGrid(document.getElementById("CONST_NO").value);
                        });
                    } else {
                        // 登録失敗
                        showErrMessage(data)
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $(location).attr('href', '@Url.Content("~/Error")/Index');
                }
            });
        },
            function() {
                return true;
            }
        );
    }

    function wardReplace(str, ...args) {
        return str.replace(/\{(\d+)\}/g, (match, index) => {
            return typeof args[index] !== "undefined" ? args[index] : match;
        });
    }

    //資産予約:閉じるボタン押下時
    function btnReserveClose_onclick() {
        //予約一覧テーブル初期化
        initializeReserveListGrid(document.getElementById("CONST_NO").value);
    }

    //モーダル画面　変更ボタンクリック処理
    function btnUpdate_onclick() {

        //日付チェック状態を初期化する
        $("#ConstStartDtString").removeClass("input-error")
        $("#ConstEndDtString").removeClass("input-error")
        $("#ConstLentStartDtString").removeClass("input-error")
        $("#ConstLentEndDtString").removeClass("input-error")

        // 工事日付チェック
        var arrConstStartDtString = document.getElementById("ConstStartDtString").value.split("/");
        var arrConstEndDtString = document.getElementById("ConstEndDtString").value.split("/");
        var arrConstLentStartDtString = document.getElementById("ConstLentStartDtString").value.split("/");
        var arrConstLentEndDtString = document.getElementById("ConstLentEndDtString").value.split("/");

        //工事終了日チェック
        if(arrConstStartDtString != "" && arrConstEndDtString != ""){
            // 両方とも空白でない時
            // 期間前後チェック
            if (arrConstStartDtString > arrConstEndDtString) {
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_CONST_END_DT_KAKOBI)")
                $("#ConstEndDtString").addClass("input-error");
                return false;
            }
        }
        //工事貸出開始日チェック
        if(arrConstStartDtString != "" && arrConstLentStartDtString != ""){
            // 両方とも空白でない時
            // 期間前後チェック
            if (arrConstStartDtString < arrConstLentStartDtString) {
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_CONST_LENT_START_DT_MIRAIBI)")
                $("#ConstLentStartDtString").addClass("input-error");
                return false;
            }
        }
        //工事貸出終了日チェック
        if(arrConstLentStartDtString != "" && arrConstLentEndDtString != ""){
            // 両方とも空白でない時
            // 期間前後チェック
            if (arrConstLentStartDtString > arrConstLentEndDtString) {
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_CONST_LENT_END_DT_KAKOBI)")
                $("#ConstLentEndDtString").addClass("input-error");
                return false;
            }
        }
        if(arrConstEndDtString != "" && arrConstLentEndDtString != ""){
            // 両方とも空白でない時
            // 期間前後チェック
            if (arrConstEndDtString > arrConstLentEndDtString) {
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_CONST_LENT_END_DT_KAKOBI_CONST_END_DT)")
                $("#ConstLentEndDtString").addClass("input-error");
                return false;
            }
        }

        if(document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_UPDATE_CLICK"){

            // カンマ区切りの文字列を設定
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/ConstructionList")/GetWarningData',
                dataType: 'json',
                data: $("#form").serialize(),
                cache: false,
                processData: false,
                success: function (data, dataType) {

                    //警告データ件数あるかないか
                    if(data == 0){
                        //確認メッセージを出し、ＯＫなら変更処理
                        // ダイアログのメッセージを設定
                        $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)");
                    }else{
                        //確認メッセージを出し、ＯＫなら変更処理
                        // ダイアログのメッセージを設定
                        confirumMessage = "@Message.getMessage(CommonLabel.MSG_UPD_CONFIRM_WARN_MAINTE_NUM)";
                        confirumMessage = wardReplace(confirumMessage, data.toString());
                        $("#show-dialog").html(confirumMessage);
                    }


                    // ダイアログを作成
                    $("#show-dialog").dialog({
                        modal: true,
                        title: strConfirmTitle,
                        dialogClass: "show-confirm-wrap",
                        buttons: {
                            "OK": function () {
                                var befAction = $('#form').attr("action");
                                $(this).dialog("close");
                                $('#form').attr("action", "@Url.Content("~/ConstructionList")/Create");
                                $('#form').submit();
                                $('#form').attr("action", befAction);
                                return true;
                            },
                            "Cancel": function () {
                                $(this).dialog("close");
                                return false;
                            }
                        },
                        open: function () {      // キャンセルボタンにフォーカスをあてる
                            $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').focus();
                        }
                    });
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $(location).attr('href', '@Url.Content("~/Error")/Index');
                }
            });

        }else{
            //変更するときに警告期間にかぶらないか確認
            showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/ConstructionList")/Create")
        }
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

    //作業日変更時
    function ChangeWorkDay(WorkDay) {
        if(WorkDay.id == "ConstStartDtString" && document.getElementById("ConstStartDtString").value){
            var tempDate = new Date(document.getElementById("ConstStartDtString").value)
            tempDate.setDate(tempDate.getDate() - @ViewBag.ConstLentTermFrom)
            document.getElementById("ConstLentStartDtString").value = tempDate.toLocaleDateString("ja-JP", {year:   "numeric",month: "2-digit",day: "2-digit"}).replaceAll('/', '-')
        }else if(WorkDay.id == "ConstEndDtString" && document.getElementById("ConstEndDtString").value){
            var tempDate = new Date(document.getElementById("ConstEndDtString").value)
            tempDate.setDate(tempDate.getDate() + @ViewBag.ConstLentTermTo)
            document.getElementById("ConstLentEndDtString").value = tempDate.toLocaleDateString("ja-JP", {year:   "numeric",month: "2-digit",day: "2-digit"}).replaceAll('/', '-')
        }
        ChkConstLentDt()
        return;
    }

    //実質貸出期間変更チェック
    function ChkConstLentDt() {
        //登録、照会画面の場合は不要
        if(document.getElementById("popup-title").innerHTML == "@Message.getMessage(CommonLabelConstructionList.TITLE_CONSTUCTION_REGISTRATION)" ||
            $("#modal-create-btn").css('display') == 'none'){
            return;
        }

        if(document.getElementById("ConstLentStartDtString").value == document.getElementById("BeforeConstLentStartDtString").value &&
            document.getElementById("ConstLentEndDtString").value == document.getElementById("BeforeConstLentEndDtString").value){
            document.getElementById("lblCannotChangeAddReserve").innerHTML = "";
            // 追加、解除、個数変更ボタンを使用可
            $("#modal-reserve-add-btn").prop("disabled", false);
            $("#modal-reserve-add-btn").attr('style', '');
            $("#modal-reserve-add-btn").css("cursor", "");
            $("#modal-qty-change-btn").prop("disabled", false);
            $("#modal-qty-change-btn").attr('style', '');
            $("#modal-qty-change-btn").css("cursor", "");
        }else{
            document.getElementById("lblCannotChangeAddReserve").innerHTML = "@Message.getMessage(CommonLabelConstructionList.MSG_CANNOT_CHANGE_ADD_RESERVE)";
            // 追加、解除、個数変更ボタンを使用不可
            $("#modal-reserve-add-btn").prop("disabled", true);
            $("#modal-reserve-add-btn").attr('style', 'background-color:#cecece;');
            $("#modal-reserve-add-btn").css("cursor", "default");
            $("#modal-qty-change-btn").prop("disabled", true);
            $("#modal-qty-change-btn").attr('style', 'background-color:#cecece;');
            $("#modal-qty-change-btn").css("cursor", "default");
        }
        return;
    }

    // テキストエリアを改行表示にする処理
    document.addEventListener('keydown', e => {
        const selection = getSelection();
        const t = e.target;
        if (t.closest('textarea')) {
            const v = t.value;
            const s = t.selectionStart;
            if (e.keyCode == 13) {
                e.preventDefault();
                t.value = v.substring(0, s) + "\n" + v.substring(s);
                t.selectionStart = s + 1;
                t.selectionEnd = s + 1;
            }
        }
    });

    //変更画面戻る押下
    function btnClose_onclick(){
        //工事番号を初期化
        document.getElementById("CONST_NO").value = "";
    }

</script>

<div id="construction-list">
    @Using (Html.BeginForm("Index", "ConstructionList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20" style="width:100%">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    <div>
                        @If ViewBag.AuthProcFlg = AuthProcFlg.All Or ViewBag.AuthProcFlg = AuthProcFlg.JiAll Then
                            @<input Class="defButton create-btn" type="button" id="btnCreate" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                            @<input Class="defButton update-btn" type="button" id="update-btn" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick(true)">
                        End If
                        <input class="defButton detail-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DETAIL)" onclick="btnEdit_onclick(false)">
                        @If ViewBag.AuthProcFlg = AuthProcFlg.All Or ViewBag.AuthProcFlg = AuthProcFlg.JiAll Then
                            @<input id="constCompleteBtn" class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabelConstructionList.BUTTON_CONST_COMPLETE)" onclick="btnConstComplete_onclick()">
                        End If
                        <input class="defButton const-asset-btn" type="button" value="@Message.getMessage(CommonLabelConstructionList.BUTTON_ASSET_INQUILY)" onclick="btnShowConstAssetList_onclick()">
                        <input class="defButton const-history-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_HISTORY)" onclick="btnShowConstructionHist_onclick()">
                        @If ViewBag.AuthProcFlg = AuthProcFlg.All Or ViewBag.AuthProcFlg = AuthProcFlg.JiAll Then
                            @<input Class="defButton csv-import-btn" type="button" id="btnCsvImport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">
                        End If
                        <input id="csvBtn" class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnIchiranCsvExport_onclick()">
                        @If ViewBag.ConstPrintFlg = ValidFlg.Cd.Valid Then
                            @<input id="report-btn" class="defButton submit-button" type="button" value="@Message.getMessage(CommonLabel.BUTTON_REPORT_OUTPUT)" onclick="btnReportOutput_onclick()">
                        End If
                    </div>
                </div>                  
            </div>
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
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstNo, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_START_DT").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_START_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstStartDt, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_END_DT").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_END_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstEndDt, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_LENT_START_DT").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstLentStartDt, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_LENT_END_DT").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstLentEndDt, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_STATUS").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_STATUS)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchConstStatus, DirectCast(ViewBag.ConstStatusList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_CONTENTS").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstContents, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_EMPLOYEE_NAME").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_EMPLOYEE_NAME)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstEmployeeName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_KYOTEN_CD_NAME)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchMngLocL, DirectCast(ViewBag.modelMngLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO1").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO1)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO1").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo1, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "1").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO2").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO2)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO2").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo2, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "2").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO3").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO3)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO3").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo3, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "3").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO4").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO4)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO4").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo4, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "4").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO5").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO5)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO5").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo5, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "5").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO6").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO6)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO6").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo6, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "6").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO7").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO7)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO7").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo7, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "7").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO8").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO8)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO8").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo8, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "8").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO9").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO9)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO9").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo9, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "9").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO10").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO10)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO10").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo10, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "10").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO11").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO11)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO11").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo11, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "11").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO12").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO12)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO12").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo12, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "12").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO13").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO13)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO13").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo13, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "13").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO14").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO14)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO14").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo14, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "14").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO15").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO15)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO15").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo15, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "15").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO16").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO16)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO16").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo16, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "16").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO17").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO17)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO17").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo17, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "17").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO18").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO18)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO18").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo18, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "18").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO19").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO19)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO19").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo19, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "19").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_INFO20").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_INFO20)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("CONST_INFO20").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchConstInfo20, DirectCast(ViewBag.searchItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "20").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
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
             <div id="ListGrids" style="height:auto;padding-bottom: 20px;" />
        </div>
            'チェックボックス選択値の引き渡し用隠し項目
        @<input id="SelectKeys" name="SelectKeys" type="hidden">
        @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden">
            'ローディング画像引き渡し用隠し項目
        @<input id="LoadingImg" name="LoadingImg" type="hidden">
            '画面遷移時、工事番号引き渡し用隠し項目
        @<input id="SelectedConstNo" name="SelectedConstNo" type="hidden">
            '画面遷移時、仮置きID引き渡し用隠し項目
        @<input id="SelectedTempId" name="SelectedTempId" type="hidden">
            '資産予約時、品番引き渡し用隠し項目
        @<input id="SelectedProductNo" name="SelectedProductNo" type="hidden">

            '工事完了モーダル画面
        @<div id="modal-const-complete-content">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title-const-complete"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
            @CODE
                'モデルの値を設定
                Dim constCompleteSessionModel As ConstructionListViewModel = Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST)
                If Not IsNothing(constCompleteSessionModel) AndAlso Model(0).ErrCode <> String.Empty OrElse ViewBag.ModalOpenFlg = "Y" Then
                    Model.Add(constCompleteSessionModel)
                Else
                    Model.Add(New ConstructionListViewModel())
                End If
            END CODE
            <div id="msg-const-complete" style="color:red;"></div>
            <br />
            <div class="grid-head-col">@Message.getMessage(CommonLabelConstructionList.LABEL_COMPLETION_REMARKS)</div>
            <div class="mb10">
                @Html.EditorFor(Function(model) model.Last.CONST_CONFIRM_REMARK, New With {.htmlAttributes = New With {.maxlength = 100, .style = "width: 550px;"}})
            </div>
            <br />
            <div Class="PopupButton float-left">
                <input id="modal-ok" class="defButton" type="button" value="@Message.getMessage(CommonLabelConstructionList.BUTTON_COMPLETE)" onclick="btnConstCompleteOK_onclick()">
            </div>
            <div Class="PopupButton float-right">
                <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_CLOSE)">
            </div>
        </div>

                'CSV取込のモーダル画面
        @<div id="modal-csv-content" class="modal-constructionlist">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title-csv"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
            <div Class="PopupButton clearfix mb10">
                <div class="float-left">
                    <input type="file" name="CsvFile" id="CsvFile" />
                </div>
                <div class="float-right">
                    <input id="modal-import-btn" class="defButton CsvImport" style="width:120px" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImportStart_onclick()">
                </div>
            </div>

            <div class="ichiran clearfix mb10">
                <div id="div_koumoku_h">
                    <table class="ich_table" id="csv_tbl_koumoku_h">
                        <tbody>
                            <tr>
                                <th class="col1">@Message.getMessage(CommonLabel.LABEL_ROW)</th>
                                <th class="col2">@Message.getMessage(CommonLabel.LABEL_DATA)</th>
                                <th class="col3">@Message.getMessage(CommonLabel.LABEL_ERRORGAIYOU)</th>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div style="height:133px; overflow-y:scroll;">
                    <div id="div_koumoku_d">
                        <table class="ich_table" id="csv_tbl_koumoku_d">
                            <tbody id="csv-err-ichiran">
                                @*エラーグリッド一覧表示*@
                                @for icnt As Integer = 1 To DataInport.DISP_ROW_COUNT
                                    @<tr><td Class="col1">　</td><td class="col2">　</td><td class="col3">　</td></tr>
                                Next
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>

            <div id="modal-msg" class="mb10">

            </div>

            <div Class="PopupButton float-right">
                <input id="csvimport-modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="fileNmClear()">
            </div>
        </div>

                '登録・更新のモーダル画面
        @<div id="modal-content" class="modal-constructionlist" style="width: 1390px;">
    <div class="popup-title-wrap">
        <p class="Line3"></p>
        <p class="Line4"></p>
        <h2 id="popup-title"></h2>
        <p class="Line3"></p>
        <p class="Line4"></p>
    </div>
    @CODE
        'モデルの値を設定
        Dim SessionModel As ConstructionListViewModel = Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST)
        If Not IsNothing(constCompleteSessionModel) AndAlso (Model(0).ErrCode <> String.Empty OrElse ViewBag.ModalOpenFlg = "Y") Then
            Model.Add(SessionModel)
        Else
            Model.Add(New ConstructionListViewModel())
        End If
    END CODE
    @Html.AntiForgeryToken()

    <Table>
        <tr Class="popup-row">
            <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_NO)</td>

            @If ViewBag.ConstNoSelfReg = "Y" Then
                @<td width="20" Class="require" style="display:inline-block;"></td>
            Else
                @<td width="20" Class="require" style="display:inline-block;">※</td>
            End If
            <td style="display:inline-block; width:420px;">
                @If ViewBag.ConstNoSelfReg = "Y" Then
                    @If Model.FirstOrDefault.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                        @Html.EditorFor(Function(model) model.Last.CONST_NO, New With {.htmlAttributes = New With {.style = "width: 190px;color:#cecece;background-color:#cecece;", .readOnly = True, .maxlength = 20, .oninput = "editHanEi(this)"}})
                    Else
                        @Html.EditorFor(Function(model) model.Last.CONST_NO, New With {.htmlAttributes = New With {.style = "width: 190px;background-color:#cecece;", .readOnly = True, .maxlength = 20, .oninput = "editHanEi(this)"}})
                    End If
                Else
                    @Html.EditorFor(Function(model) model.Last.CONST_NO, New With {.htmlAttributes = New With {.style = "width: 190px;", .maxlength = 20, .oninput = "editHanEi(this)"}})
                End If
            </td>
            <td class="require" id="lblCannotChangeAddReserve" style="display:inline-block;"></td>
        </tr>
    </Table>

    <Table>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)</td>
            <td width="20" Class="require" style="display:inline-block;">※</td>
            <td style="display:inline-block; width:1096px;">@Html.EditorFor(Function(model) model.Last.CONST_CONTENTS, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 1078px;"}})</td>
        </tr>
    </Table>

    <Table align="left">
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabelConstructionList.LABEL_CONST_WORK_DAY)</td>
            <td width="20" Class="require" style="display:inline-block;">※</td>
            <td style="display:inline-block; width:420px;">
                @Html.EditorFor(Function(model) model.Last.ConstStartDtString, New With {.htmlAttributes = New With {.type = "date", .style = "width: 190px;", .onchange = "ChangeWorkDay(this)"}})
                ～
                @Html.EditorFor(Function(model) model.Last.ConstEndDtString, New With {.htmlAttributes = New With {.type = "date", .style = "width: 190px;", .onchange = "ChangeWorkDay(this)"}})
            </td>
        </tr>
    </Table>
    <Table align="right">
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_LEASE_KIKAN_JISSHITSU)</td>
            <td width="20" Class="require" style="display:inline-block;">※</td>
            <td style="display:inline-block; width:420px;">
                @Html.EditorFor(Function(model) model.Last.ConstLentStartDtString, New With {.htmlAttributes = New With {.type = "date", .style = "width: 190px;", .onchange = "ChkConstLentDt()"}})
                ～
                @Html.EditorFor(Function(model) model.Last.ConstLentEndDtString, New With {.htmlAttributes = New With {.type = "date", .style = "width: 190px;", .onchange = "ChkConstLentDt()"}})
            </td>
        </tr>
    </Table>

    <Table align="left">
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabelConstructionList.LABEL_CONST_PIC)</td>
            <td width="20" class="require" style="display:inline-block;">※</td>
            <td style="display:inline-block; width:420px;">@Html.DropDownListFor(Function(model) model.Last.CONST_EMPLOYEE_CD, DirectCast(ViewBag.modalmodelEmployee, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 402px;"})</td>
        </tr>
    </Table>
    <Table align="right">
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_KYOTEN_CD_NAME)</td>
            <td width="20" class="require" style="display:inline-block;">※</td>
            <td style="display:inline-block; width:420px;">@Html.DropDownListFor(Function(model) model.Last.MNG_LOC_L_CD, DirectCast(ViewBag.modalmodelMngLocL, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 402px;"})</td>
        </tr>
    </Table>

    <Table align="left">
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO1)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO1_FROM, inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO1_TO, inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO1, inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO2)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO2_FROM, inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO2_TO, inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO2, inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO3)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO3_FROM, inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO3_TO, inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO3, inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO4)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO4_FROM, inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO4_TO, inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO4, inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO5)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO5_FROM, inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO5_TO, inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO5, inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO6)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO6_FROM, inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO6_TO, inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO6, inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO7)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO7_FROM, inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO7_TO, inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO7, inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO8)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "8").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO8_FROM, inputItemList.Where(Function(x) x.KEY1 = "8").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO8_TO, inputItemList.Where(Function(x) x.KEY1 = "8").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO8, inputItemList.Where(Function(x) x.KEY1 = "8").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO9)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "9").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO9_FROM, inputItemList.Where(Function(x) x.KEY1 = "9").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO9_TO, inputItemList.Where(Function(x) x.KEY1 = "9").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO9, inputItemList.Where(Function(x) x.KEY1 = "9").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO10)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "10").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO10_FROM, inputItemList.Where(Function(x) x.KEY1 = "10").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO10_TO, inputItemList.Where(Function(x) x.KEY1 = "10").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO10, inputItemList.Where(Function(x) x.KEY1 = "10").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
    </Table>
    <Table align="right">
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO11)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "11").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO11_FROM, inputItemList.Where(Function(x) x.KEY1 = "11").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO11_TO, inputItemList.Where(Function(x) x.KEY1 = "11").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO11, inputItemList.Where(Function(x) x.KEY1 = "11").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO12)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "12").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO12_FROM, inputItemList.Where(Function(x) x.KEY1 = "12").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO12_TO, inputItemList.Where(Function(x) x.KEY1 = "12").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO12, inputItemList.Where(Function(x) x.KEY1 = "12").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO13)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "13").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO13_FROM, inputItemList.Where(Function(x) x.KEY1 = "13").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO13_TO, inputItemList.Where(Function(x) x.KEY1 = "13").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO13, inputItemList.Where(Function(x) x.KEY1 = "13").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO14)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "14").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO14_FROM, inputItemList.Where(Function(x) x.KEY1 = "14").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO14_TO, inputItemList.Where(Function(x) x.KEY1 = "14").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO14, inputItemList.Where(Function(x) x.KEY1 = "14").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO15)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "15").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO15_FROM, inputItemList.Where(Function(x) x.KEY1 = "15").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO15_TO, inputItemList.Where(Function(x) x.KEY1 = "15").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO15, inputItemList.Where(Function(x) x.KEY1 = "15").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO16)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "16").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO16_FROM, inputItemList.Where(Function(x) x.KEY1 = "16").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO16_TO, inputItemList.Where(Function(x) x.KEY1 = "16").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO16, inputItemList.Where(Function(x) x.KEY1 = "16").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO17)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "17").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO17_FROM, inputItemList.Where(Function(x) x.KEY1 = "17").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO17_TO, inputItemList.Where(Function(x) x.KEY1 = "17").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO17, inputItemList.Where(Function(x) x.KEY1 = "17").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO18)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "18").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO18_FROM, inputItemList.Where(Function(x) x.KEY1 = "18").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO18_TO, inputItemList.Where(Function(x) x.KEY1 = "18").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO18, inputItemList.Where(Function(x) x.KEY1 = "18").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO19)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "19").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO19_FROM, inputItemList.Where(Function(x) x.KEY1 = "19").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO19_TO, inputItemList.Where(Function(x) x.KEY1 = "19").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO19, inputItemList.Where(Function(x) x.KEY1 = "19").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
        <tr Class="popup-row">
            <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_CONST_INFO20)</td>
            <td width="20" style="display:inline-block;"></td>
            <td style="display:inline-block; width:420px;">
                @If inputItemList.Where(Function(x) x.KEY1 = "20").FirstOrDefault.DATA1 = Misc.InputType.AREA Then
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO20_FROM, inputItemList.Where(Function(x) x.KEY1 = "20").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                    @Html.Raw(" ～ ")
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO20_TO, inputItemList.Where(Function(x) x.KEY1 = "20").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 190px;"}})
                Else
                    @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.CONST_INFO20, inputItemList.Where(Function(x) x.KEY1 = "20").FirstOrDefault, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 402px;"}})
                End If
            </td>
        </tr>
    </Table>
    @* 仮置き情報 *@
    <Table align="left" style="display:@(If(ViewBag.TemporaryLeaveFlg = "Y", "display", "None"))">
        <tr Class="popup-row">
            <td class="head-col" style="height:105px;width:230px;display: flex;align-items: center;">@Message.getMessage(CommonLabelConstructionList.LABEL_TEMP_STORAGE_INFO)</td>
        </tr>
    </Table>
    <span style="float:left;margin:3px 50px 0px 20px;display:@(If(ViewBag.TemporaryLeaveFlg = "Y", "display", "None"));">
        <input id="modal-temp-add-btn" class="defButton submit-button" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_ADD)" onclick="return btnTempAdd_onclick()">
    </span>
    <span id="lblCannotAddReserve" style="float:left;margin-top:3px;display:@(If(ViewBag.TemporaryLeaveFlg = "Y", "display", "None"));" class="require">
    </span>
    <span style="float:right;margin:3px 200px 0px 0px;display:@(If(ViewBag.TemporaryLeaveFlg = "Y", "display", "None"));">
        <input id="modal-release-btn" class="defButton submit-button" type="button" name="ButtonName" value="@Message.getMessage(CommonLabelConstructionList.BUTTON_RELEASE)" onclick="return btnTempRelease_onclick()">
    </span>
    <div align="left" class="list-view" style="display:@(If(ViewBag.TemporaryLeaveFlg = "Y", "display", "None"))">
        <div id="TemporaryListGrids" class="float-left" style="height:79px;width:917px;margin:3px 0px 0px 20px; position: relative;"></div>
    </div>
    @* 予約情報 *@
    <Table align="left" style="clear: both;display:@(If(ViewBag.MatchingReserveFlg = "Y", "display", "None"))">
        <tr Class="popup-row">
            <td class="head-col" style="height:105px;width:230px;display: flex;align-items: center;">@Message.getMessage(CommonLabelConstructionList.LABEL_RESERVATION_INFO)</td>
        </tr>
    </Table>
    <span style="float:left;margin:3px 20% 0px 20px;display:@(If(ViewBag.MatchingReserveFlg = "Y", "display", "None"));">
        <input id="modal-reserve-add-btn" class="defButton submit-button" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_ADD)" onclick="return btnReserveAdd_onclick()">
    </span>
    <span style="float:left;margin:3px 20% 0px 0px;display:@(If(ViewBag.MatchingReserveFlg = "Y", "display", "None"));">
        <input id="modal-qty-change-btn" class="defButton submit-button" type="button" name="ButtonName" value="@Message.getMessage(CommonLabelConstructionList.BUTTON_QUANTITY_CHANGE)" onclick="return btnQtyChange_onclick()">
    </span>
    <span style="float:left;margin:3px 0px 0px 0px;display:@(If(ViewBag.MatchingReserveFlg = "Y", "display", "None"));">
        <input id="modal-reserve-del-btn" class="defButton submit-button" type="button" name="ButtonName" value="@Message.getMessage(CommonLabelConstructionList.BUTTON_RELEASE)" onclick="return btnReserveRelease_onclick()">
    </span>
    <div align="left" class="list-view" style="display:@(If(ViewBag.MatchingReserveFlg = "Y", "display", "None"))">
        <div id="ReserveListGrids" class="float-left" style="height:79px;width:917px;margin:3px 0px 0px 20px; position: relative;"></div>
    </div>
    <div Class="PopupButton" style="clear:both;">
        <span style="float:left;margin:15px 20% 0 100px;">
            <input id="modal-create-btn" class="defButton modal-create-btn submit-button" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_TOUROKU)" onclick="return btnUpdate_onclick()">
        </span>
        <span style="float:left;margin:15px 20% 0 0;">
            <input id="modal-delete-btn" class="defButton submit-button" type="button" formaction="~/ConstructionList/Delete" name="Delete" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="return btnDelete_onclick()">
        </span>
        <span style="float:left;margin:15px 10% 0 0; ">
            <input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="return btnClose_onclick()">
        </span>
    </div>
</div>

                '資産予約モーダル画面
        @<div id="modal-asset-reserve-content" style="width: 800px;">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title-asset-reserve"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
            @CODE
                'モデルの値を設定
                Dim assetReserveSessionModel As ConstructionListViewModel = Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST)
                If Not IsNothing(assetReserveSessionModel) AndAlso Model(0).ErrCode <> String.Empty OrElse ViewBag.ModalOpenFlg = "Y" Then
                    Model.Add(assetReserveSessionModel)
                Else
                    Model.Add(New ConstructionListViewModel())
                End If
            END CODE
             @* 検索条件 *@
             <div class="search-wrap">
                 <a id="SearchConditionAssetReserve" data-toggle="collapse" href="#cond-asset-reserve" aria-expanded="true" onclick="SearchConditionAssetReserve_onclick()">
                     @Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)
                 </a>
                 <div class="search-border">
                     <div id="cond-asset-reserve" class="container collapse in">
                         <div class="row" id="item-conditions-asset-reserve">
                             <div class="col-md-12">
                                 <div class="col-sm-4">
                                     <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)</div>
                                     <div class="grid-condition-col">
                                         <input class="cond-control" id="SearchProductNo" maxlength="30" name="SearchProductNo" type="text" value="">
                                     </div>
                                 </div>
                                 <div class="col-sm-4">
                                     <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PRODUCT_NAME)</div>
                                     <div class="grid-condition-col">
                                         <input class="cond-control" id="SearchAssetName" maxlength="60" name="SearchAssetName" type="text" value="">
                                     </div>
                                 </div>
                                 <div class="col-sm-4">
                                     <input class="float-right defButton submit-button" name="actionname" type="button" style="margin-top: 28px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return initializeAssetReserveListGrid()" />
                                 </div>
                             </div>
                         </div>
                     </div>
                 </div>
             </div>
            <br />
            <div align="left" class="list-view">
                <div id="AssetReserveListGrids" class="float-left" style="height:194px;width:537px;padding-bottom: 40px; position: relative;"></div>
            </div>
            <div class="float-left" style="padding:3px;clear:both;vertical-align:middle">@Message.getMessage(CommonLabel.LABEL_RESERVATION_NUM)</div>
            <div class="grid-condition-col float-left" style="width:120px">
                <input class="text-control" id="ReserveNum" name="ReserveNum" type="text" value="" oninput="editNumber(this)">
                <br /><br />
            </div>
            <div Class="PopupButton float-left" style="clear:both">
                <input id="modal-ok" class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_TOUROKU)" onclick="btnReserveRegist_onclick()">
            </div>
            <div Class="PopupButton float-right">
                <input id="asset-reserve-modal-close" class="defButton asset-reserve-modal-close"  type="button" value="@Message.getMessage(CommonLabel.BUTTON_CLOSE)"  onclick="btnReserveClose_onclick()">
            </div>
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OpenFlg)

        @Html.HiddenFor(Function(model) model.Last.CONST_STATUS)
        @Html.HiddenFor(Function(model) model.Last.PREV_TEMP_NUM)
        @Html.HiddenFor(Function(model) model.Last.CONST_IN_NUM)
        @Html.HiddenFor(Function(model) model.Last.NOW_TEMP_NUM)
        @Html.HiddenFor(Function(model) model.Last.CONST_OUT_NUM)
        @Html.HiddenFor(Function(model) model.Last.CUR_LENT_NUM)
        @Html.HiddenFor(Function(model) model.Last.CUR_TEMP_NUM)
        @Html.HiddenFor(Function(model) model.Last.SITE_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.SITE_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.CONST_CONFIRM_TS)
        @Html.HiddenFor(Function(model) model.Last.CONST_CONFIRM_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.CONST_CONFIRM_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.CONST_CONFIRM_REMARK)
        @Html.HiddenFor(Function(model) model.Last.TASK_TS)
        @Html.HiddenFor(Function(model) model.Last.INS_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_TASK)
        @Html.HiddenFor(Function(model) model.Last.BeforeConstLentStartDtString)
        @Html.HiddenFor(Function(model) model.Last.BeforeConstLentEndDtString)
                End Using
</div>
