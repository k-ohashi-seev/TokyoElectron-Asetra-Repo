@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of SettingViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelSetting.TITLE)
End Code
<script>
    //
    //設定マスタメンテ画面
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
            if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
                //登録画面を表示
                if (document.getElementById("CopyFlag").value == "1") {

                    //タイトルの設定
                    document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
                    document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
                    //KEY-DIVを書き込み不可
                    document.getElementById("KEY_DIV").readOnly = true;
                    $("#KEY_DIV").removeClass("input-error");
                    $("#KEY_DIV").addClass("disable");
                    //KEY1,KEY2,KEY3を書き込み可
                    document.getElementById("KEY1").readOnly = false;
                    //$("#KEY1").addClass("input-error");
                    $("#KEY1").removeClass("disable");
                    document.getElementById("KEY2").readOnly = false;
                    //$("#KEY2").addClass("input-error");
                    $("#KEY2").removeClass("disable");
                    document.getElementById("KEY3").readOnly = false;
                    //$("#KEY3").addClass("input-error");
                    $("#KEY3").removeClass("disable");

                    ShowModalWindow()

                    //$('.copy-btn').trigger("click")
                    //btnCopyCreate_onclick();
                } else {
                    //タイトルの設定
                    document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
                    document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

                    //KEY-DIV,KEY1,KEY2,KEY3を書き込み可
                    document.getElementById("KEY_DIV").readOnly = false;
                    $("#KEY_DIV").removeClass("disable");
                    document.getElementById("KEY1").readOnly = false;
                    $("#KEY1").removeClass("disable");
                    document.getElementById("KEY2").readOnly = false;
                    $("#KEY2").removeClass("disable");
                    document.getElementById("KEY3").readOnly = false;
                    $("#KEY3").removeClass("disable");

                    ShowModalWindow()
                    //$('.create-btn').trigger("click")
                    //btnCreate_onclick();
                }
            } else {

                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

                //KEY-DIV,KEY1,KEY2,KEY3を書き込み不可
                document.getElementById("KEY_DIV").readOnly = true;
                $("#KEY_DIV").removeClass("input-error");
                $("#KEY_DIV").addClass("disable");

                document.getElementById("KEY1").readOnly = true;
                $("#KEY1").removeClass("input-error");
                $("#KEY1").addClass("disable");

                document.getElementById("KEY2").readOnly = true;
                $("#KEY2").removeClass("input-error");
                $("#KEY2").addClass("disable");

                document.getElementById("KEY3").readOnly = true;
                $("#KEY3").removeClass("input-error");
                $("#KEY3").addClass("disable");

                //変更画面を表示
                ShowModalWindow()

                //変更画面を表示
                //$('.update-btn').trigger("click")
            }

            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
            document.getElementById("ErrCode").value = "";

        }

        // 列定義

        let columns = [
            { id: "@ColKey.SETTING.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.CHECK_SELECT", 40), sortOrder: -2, formatter: SettingCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.SETTING.FUNC_CAT_NAME", name: "@Message.getMessage(CommonLabelSetting.LABEL_FUNC_CAT_NAME)", field: "FUNC_CAT_NAME", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.FUNC_CAT_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("FUNC_CAT_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FUNC_CAT_NAME").DispFlag" },
            { id: "@ColKey.SETTING.KEY_DIV", name: "@Message.getMessage(CommonLabelSetting.LABEL_KEY_DIV)", field: "KEY_DIV", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.KEY_DIV", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("KEY_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("KEY_DIV").DispFlag" },
            { id: "@ColKey.SETTING.KEY1", name: "@Message.getMessage(CommonLabelSetting.LABEL_KEY1)", field: "KEY1", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.KEY1", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("KEY1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("KEY1").DispFlag" },
            { id: "@ColKey.SETTING.KEY2", name: "@Message.getMessage(CommonLabelSetting.LABEL_KEY2)", field: "KEY2", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.KEY2", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("KEY2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("KEY2").DispFlag" },
            { id: "@ColKey.SETTING.KEY3", name: "@Message.getMessage(CommonLabelSetting.LABEL_KEY3)", field: "KEY3", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.KEY3", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("KEY3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("KEY3").DispFlag" },
            { id: "@ColKey.SETTING.REMARK", name: "@Message.getMessage(CommonLabelSetting.LABEL_REMARK)", field: "REMARK", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.REMARK", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK").DispFlag" },
            { id: "@ColKey.SETTING.DATA1", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA1)", field: "DATA1", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA1", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA1").DispFlag" },
            { id: "@ColKey.SETTING.DATA2", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA2)", field: "DATA2", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA2", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA2").DispFlag" },
            { id: "@ColKey.SETTING.DATA3", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA3)", field: "DATA3", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA3", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA3").DispFlag" },
            { id: "@ColKey.SETTING.DATA4", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA4)", field: "DATA4", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA4", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA4").DispFlag" },
            { id: "@ColKey.SETTING.DATA5", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA5)", field: "DATA5", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA5", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA5").DispFlag" },
            { id: "@ColKey.SETTING.DATA6", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA6)", field: "DATA6", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA6", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA6").DispFlag" },
            { id: "@ColKey.SETTING.DATA7", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA7)", field: "DATA7", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA7", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA7").DispFlag" },
            { id: "@ColKey.SETTING.DATA8", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA8)", field: "DATA8", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA8", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA8").DispFlag" },
            { id: "@ColKey.SETTING.DATA9", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA9)", field: "DATA9", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA9", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA9").DispFlag" },
            { id: "@ColKey.SETTING.DATA10", name: "@Message.getMessage(CommonLabelSetting.LABEL_DATA10)", field: "DATA10", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DATA10", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DATA10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DATA10").DispFlag" },
            { id: "@ColKey.SETTING.SETTING_OBJECT_DIV", name: "@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV)", field: "SETTING_OBJECT_DIV", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.SETTING_OBJECT_DIV", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("SETTING_OBJECT_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SETTING_OBJECT_DIV").DispFlag", formatter: SettingObjectDivFormatter },
            { id: "@ColKey.SETTING.UPD_AUTHORITY_FLG", name: "@Message.getMessage(CommonLabelSetting.LABEL_UPD_AUTHORITY_FLG)", field: "UPD_AUTHORITY_FLG", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.UPD_AUTHORITY_FLG", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("UPD_AUTHORITY_FLG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("UPD_AUTHORITY_FLG").DispFlag", formatter: UpdAuthorityFlgFormatter },
            { id: "@ColKey.SETTING.DEL_AUTHORITY_FLG", name: "@Message.getMessage(CommonLabelSetting.LABEL_DEL_AUTHORITY_FLG)", field: "DEL_AUTHORITY_FLG", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.DEL_AUTHORITY_FLG", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DEL_AUTHORITY_FLG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DEL_AUTHORITY_FLG").DispFlag", formatter: DelAuthorityFlgFormatter },
            { id: "@ColKey.SETTING.INS_DATETIME", name: "@Message.getMessage(CommonLabel.LABEL_INS_DATETIME)", field: "INS_DATETIME", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.INS_DATETIME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("INS_DATETIME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INS_DATETIME").DispFlag", formatter: InsDatetimeFormatter },
            { id: "@ColKey.SETTING.INS_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_INS_EMPLOYEE_NAME)", field: "INS_EMPLOYEE_NAME", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.INS_EMPLOYEE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("INS_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INS_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.SETTING.UPD_DATETIME", name: "@Message.getMessage(CommonLabel.LABEL_UPD_DATETIME)", field: "UPD_DATETIME", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.UPD_DATETIME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("UPD_DATETIME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("UPD_DATETIME").DispFlag", formatter: UpdDatetimeFormatter },
            { id: "@ColKey.SETTING.UPD_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_UPD_EMPLOYEE_NAME)", field: "UPD_EMPLOYEE_NAME", width: getColumnWidth("@DispId.SETTING", "@ColKey.SETTING.INS_EMPLOYEE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("UPD_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("UPD_EMPLOYEE_NAME").DispFlag" },
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
            fixCol = fixCol + 1       //選択列と変更列を加算
        }

        let data = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.SETTING", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, "@DispId.SETTING", fixCol);
        }

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

    });

    //選択用formatter
    function SettingCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.KEY_DIV) + ',' + convertToEmptyIfNull(dataContext.KEY1) + ',' + convertToEmptyIfNull(dataContext.KEY2) + ',' + convertToEmptyIfNull(dataContext.KEY3)
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;">' +
               '<input type="checkbox" class="slick-checkbox" ' +
               'value="' + key + '" ' +
               (value == 1 ? 'checked="checked"' : '') + ' />' +
               '</div>';
    }

    //登録日時用formatter INS_DATETIME
    function InsDatetimeFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value)
        if (status != '') {
            status = status.replace('T', ' ')
            status = status.replaceAll('-', '/')
            return status.substr(0, 19)
        }
    }

    //登録日時用formatter UPD_DATETIME
    function UpdDatetimeFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value)
        if (status != '') {
            status = status.replace('T', ' ')
            status = status.replaceAll('-', '/')
            return status.substr(0, 19)
        }
    }


    //設定対象区分用formatter SETTING_OBJECT_DIV
    function SettingObjectDivFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        switch (dataContext.SETTING_OBJECT_DIV) {
            case "@SETTING_OBJECT_DIV.HT":
                status = '@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_HT)'
                break;
            case "@SETTING_OBJECT_DIV.PC":
                status = '@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_PC)'
                break;
            case "@SETTING_OBJECT_DIV.WEB":
                status = '@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_WEB)'
                break;
            case "@SETTING_OBJECT_DIV.ALL":
                status = '@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_ALL)'
                break;

        }
        return status
    }

    //更新可否区分用formatter SETTING_OBJECT_DIV
    function UpdAuthorityFlgFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        switch (dataContext.UPD_AUTHORITY_FLG) {
            case "@UPD_AUTHORITY_FLG.Yes":
                status = '@Message.getMessage(CommonLabelSetting.LABEL_UPD_AUTHORITY_FLG_Y)'
                break;
            case "@UPD_AUTHORITY_FLG.No":
                status = '@Message.getMessage(CommonLabelSetting.LABEL_UPD_AUTHORITY_FLG_N)'
                break;
        }
        return status
    }

    //削除可否区分用formatter SETTING_OBJECT_DIV
    function DelAuthorityFlgFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        switch (dataContext.DEL_AUTHORITY_FLG) {
            case "@DEL_AUTHORITY_FLG.Yes":
                status = '@Message.getMessage(CommonLabelSetting.LABEL_DEL_AUTHORITY_FLG_Y)'
                break;
            case "@DEL_AUTHORITY_FLG.No":
                status = '@Message.getMessage(CommonLabelSetting.LABEL_DEL_AUTHORITY_FLG_N)'
                break;
        }
        return status
    }




    //検索ボタンクリック処理
    function btnSearch_onclick() {
        //ページ番号のリセット
        ResetPageNum();

        //2024/4/18 Add Start Iseda
        //設定対象区分のチェック状態
        var keysSettingObjectDiv = [];
        var lstSettingObjectDiv = document.getElementById("setting-object-div");
        var checkSettingObjectDiv = lstSettingObjectDiv.getElementsByTagName("INPUT");
        for (var i = 0; i < checkSettingObjectDiv.length; i++) {
            if (checkSettingObjectDiv[i].checked) {
                keysSettingObjectDiv.push(checkSettingObjectDiv[i].value);
            }
        }
        //設定対象区分が未選択の場合はエラー
        if (keysSettingObjectDiv.length == 0) {
            showErrMessage("@Message.getMessage(CommonLabelSetting.MSG_ERR_ID_SELECT_SETTING_OBJECT_DIV)")
            return false;
        }
        //2024/4/18 Add END Iseda

        return true;
    }

    function ClearCopyFlag(flg) {
        document.getElementById("CopyFlag").value = flg;
        document.getElementById("ErrCode").value = "";
        return true;
    }

    //コピー新規登録ボタンクリック処理
    function btnCopyCreate_onclick() {
        //テーブルの選択行を取得する。
        if (document.getElementById("ErrCode").value == "") {
            var text = [];
            //--2019/11/21 nv miyaji upd start
            // SlickGridのチェックボックスを取得
            var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
            for (var i = 0; i < checkBoxes.length; i++) {
                if (checkBoxes[i].checked) {
                    text.push(checkBoxes[i].value);
                }
            }
            //$("[type=checkbox]:checked").each(function () {
            //    text.push($(this).val());
            //});
            //--2019/11/21 nv miyaji upd end
            if (text.length == 0) { //テーブルの選択行がない場合
                showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
                return false;
            } else if (text.length > 1) {//テーブルの選択行が複数の場合
                showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
                return false;
            }
        }
        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
        //KEY-DIVを書き込み不可
        document.getElementById("KEY_DIV").readOnly = true;
        $("#KEY_DIV").removeClass("input-error");
        $("#KEY_DIV").addClass("disable");
        //KEY1,KEY2,KEY3を書き込み可
        document.getElementById("KEY1").readOnly = false;
        //$("#KEY1").addClass("input-error");
        $("#KEY1").removeClass("disable");
        document.getElementById("KEY2").readOnly = false;
        //$("#KEY2").addClass("input-error");
        $("#KEY2").removeClass("disable");
        document.getElementById("KEY3").readOnly = false;
        //$("#KEY3").addClass("input-error");
        $("#KEY3").removeClass("disable");

        document.getElementById("REMARK").readOnly = false;
        $("#REMARK").removeClass("input-error");
        $("#REMARK").removeClass("disable");

        document.getElementById("DATA1").readOnly = false;
        $("#DATA1").removeClass("input-error");
        $("#DATA1").removeClass("disable");

        document.getElementById("DATA2").readOnly = false;
        $("#DATA2").removeClass("input-error");
        $("#DATA2").removeClass("disable");

        document.getElementById("DATA3").readOnly = false;
        $("#DATA3").removeClass("input-error");
        $("#DATA3").removeClass("disable");

        document.getElementById("DATA4").readOnly = false;
        $("#DATA4").removeClass("input-error");
        $("#DATA4").removeClass("disable");

        document.getElementById("DATA5").readOnly = false;
        $("#DATA5").removeClass("input-error");
        $("#DATA5").removeClass("disable");

        document.getElementById("DATA6").readOnly = false;
        $("#DATA6").removeClass("input-error");
        $("#DATA6").removeClass("disable");

        document.getElementById("DATA7").readOnly = false;
        $("#DATA7").removeClass("input-error");
        $("#DATA7").removeClass("disable");

        document.getElementById("DATA8").readOnly = false;
        $("#DATA8").removeClass("input-error");
        $("#DATA8").removeClass("disable");

        document.getElementById("DATA9").readOnly = false;
        $("#DATA9").removeClass("input-error");
        $("#DATA9").removeClass("disable");

        document.getElementById("DATA10").readOnly = false;
        $("#DATA10").removeClass("input-error");
        $("#DATA10").removeClass("disable");

        $("#FUNC_CAT_NAME1").prop("disabled", false);
        $("#FUNC_CAT_NAME1").removeClass("input-error");
        $("#FUNC_CAT_NAME1").removeClass("disable");

        $("#FUNC_CAT_NAME2").prop("disabled", false);
        $("#FUNC_CAT_NAME2").removeClass("input-error");
        $("#FUNC_CAT_NAME2").removeClass("disable");

        $("#FUNC_CAT_NAME3").prop("disabled", false);
        $("#FUNC_CAT_NAME3").removeClass("input-error");
        $("#FUNC_CAT_NAME3").removeClass("disable");
        //入力可否を切り替えたプルダウンの×（クリアボタン）の表示を更新する
        refreshClearable("#FUNC_CAT_NAME1, #FUNC_CAT_NAME2, #FUNC_CAT_NAME3");

        document.getElementById("FUNC_CAT_SORT").readOnly = false;
        $("#FUNC_CAT_SORT").removeClass("input-error");
        $("#FUNC_CAT_SORT").removeClass("disable");

        $("#SETTING_OBJECT_DIV_HT").prop("disabled", false);
        $("#SETTING_OBJECT_DIV_HT").removeClass("input-error");
        $("#SETTING_OBJECT_DIV_HT").removeClass("disable");

        $("#SETTING_OBJECT_DIV_PC").prop("disabled", false);
        $("#SETTING_OBJECT_DIV_PC").removeClass("input-error");
        $("#SETTING_OBJECT_DIV_PC").removeClass("disable");

        $("#SETTING_OBJECT_DIV_WEB").prop("disabled", false);
        $("#SETTING_OBJECT_DIV_WEB").removeClass("input-error");
        $("#SETTING_OBJECT_DIV_WEB").removeClass("disable");

        $("#SETTING_OBJECT_DIV_ALL").prop("disabled", false);
        $("#SETTING_OBJECT_DIV_ALL").removeClass("input-error");
        $("#SETTING_OBJECT_DIV_ALL").removeClass("disable");

        $("#UPD_AUTHORITY_FLG_Y").prop("disabled", false);
        $("#UPD_AUTHORITY_FLG_Y").removeClass("input-error");
        $("#UPD_AUTHORITY_FLG_Y").removeClass("disable");

        $("#UPD_AUTHORITY_FLG_N").prop("disabled", false);
        $("#UPD_AUTHORITY_FLG_N").removeClass("input-error");
        $("#UPD_AUTHORITY_FLG_N").removeClass("disable");

        $("#DEL_AUTHORITY_FLG_Y").prop("disabled", false);
        $("#DEL_AUTHORITY_FLG_Y").removeClass("input-error");
        $("#DEL_AUTHORITY_FLG_Y").removeClass("disable");

        $("#DEL_AUTHORITY_FLG_N").prop("disabled", false);
        $("#DEL_AUTHORITY_FLG_N").removeClass("input-error");
        $("#DEL_AUTHORITY_FLG_N").removeClass("disable");

        $("#modal-create-btn").css("display", "block");

        $("#FUNC_CAT_SORT").removeClass("input-error");

        //削除ボタンを表示
        //$("#modal-delete-btn").css("display", "block");

        //エラーの場合検索しない
        @*//if ("@Model.FirstOrDefault.ErrCode" == "") {*@
        if (document.getElementById("ErrCode").value == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/Setting")/GetEditData',
                cache: false,
                data: { KEYS: text[0] },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }
                    //取得したデータを表示する
                    document.getElementById("KEY_DIV").value = data.KEY_DIV;
                    document.getElementById("KEY1").value = data.KEY1;
                    document.getElementById("KEY2").value = data.KEY2;
                    document.getElementById("KEY3").value = data.KEY3;
                    document.getElementById("REMARK").value = data.REMARK;
                    document.getElementById("DATA1").value = data.DATA1;
                    document.getElementById("DATA2").value = data.DATA2;
                    document.getElementById("DATA3").value = data.DATA3;
                    document.getElementById("DATA4").value = data.DATA4;
                    document.getElementById("DATA5").value = data.DATA5;
                    document.getElementById("DATA6").value = data.DATA6;
                    document.getElementById("DATA7").value = data.DATA7;
                    document.getElementById("DATA8").value = data.DATA8;
                    document.getElementById("DATA9").value = data.DATA9;
                    document.getElementById("DATA10").value = data.DATA10;

                    //2024/4/18 Add Start Iseda
                    document.getElementById("FUNC_CAT_NAME1").value = data.FUNC_CAT_NAME1;
                    document.getElementById("FUNC_CAT_NAME2").value = data.FUNC_CAT_NAME2;
                    document.getElementById("FUNC_CAT_NAME3").value = data.FUNC_CAT_NAME3;
                    //値を設定したプルダウンの×（クリアボタン）の表示を更新する
                    refreshClearable("#FUNC_CAT_NAME1, #FUNC_CAT_NAME2, #FUNC_CAT_NAME3");
                    document.getElementById("FUNC_CAT_SORT").value = data.FUNC_CAT_SORT;

                    //設定対象区分ラジオボタンのチェック状態設定
                    if (data.SETTING_OBJECT_DIV == "@SETTING_OBJECT_DIV.HT") {
                        $('[id=SETTING_OBJECT_DIV_HT]').prop('checked', true);
                    } else if (data.SETTING_OBJECT_DIV == "@SETTING_OBJECT_DIV.PC") {
                        $('[id=SETTING_OBJECT_DIV_PC]').prop('checked', true);
                    } else if (data.SETTING_OBJECT_DIV == "@SETTING_OBJECT_DIV.WEB") {
                        $('[id=SETTING_OBJECT_DIV_WEB]').prop('checked', true);
                    } else if (data.SETTING_OBJECT_DIV == "@SETTING_OBJECT_DIV.ALL") {
                        $('[id=SETTING_OBJECT_DIV_ALL]').prop('checked', true);
                    }

                    //更新可否ラジオボタンのチェック状態設定
                    if (data.UPD_AUTHORITY_FLG == "@UPD_AUTHORITY_FLG.Yes") {
                        $('[id=UPD_AUTHORITY_FLG_Y]').prop('checked', true);
                    } else {
                        $('[id=UPD_AUTHORITY_FLG_N]').prop('checked', true);
                    }

                    //削除可否ラジオボタンのチェック状態設定
                    if (data.DEL_AUTHORITY_FLG == "@DEL_AUTHORITY_FLG.Yes") {
                        $('[id=DEL_AUTHORITY_FLG_Y]').prop('checked', true);
                    } else {
                        $('[id=DEL_AUTHORITY_FLG_N]').prop('checked', true);
                    }
                    //2024/4/18 Add END Iseda

                    document.getElementById("INS_DATETIME").value = data.EDIT_INS_DATETIME;
                    document.getElementById("INS_EMPLOYEE_CD").value = data.EDIT_INS_EMPLOYEE_CD;
                    document.getElementById("INS_EMPLOYEE_NAME").value = data.EDIT_INS_EMPLOYEE_NAME;
                    document.getElementById("UPD_DATETIME").value = data.EDIT_UPD_DATETIME;
                    document.getElementById("UPD_EMPLOYEE_CD").value = data.EDIT_UPD_EMPLOYEE_CD;
                    document.getElementById("UPD_EMPLOYEE_NAME").value = data.EDIT_UPD_EMPLOYEE_NAME;
                    document.getElementById("UPD_TASK").value = data.RenewProcess;
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }
        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        //登録ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_CREATE_CLICK";
        // ダイアログを表示
        ShowModalWindow();
        return true;
    }

    //登録ボタンクリック処理
    function btnCreate_onclick() {

        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

        //KEY-DIV,KEY1,KEY2,KEY3を書き込み可
        document.getElementById("KEY_DIV").readOnly = false;
        $("#KEY_DIV").removeClass("disable");
        document.getElementById("KEY1").readOnly = false;
        $("#KEY1").removeClass("disable");
        document.getElementById("KEY2").readOnly = false;
        $("#KEY2").removeClass("disable");
        document.getElementById("KEY3").readOnly = false;
        $("#KEY3").removeClass("disable");

        document.getElementById("REMARK").readOnly = false;
        $("#REMARK").removeClass("input-error");
        $("#REMARK").removeClass("disable");

        document.getElementById("DATA1").readOnly = false;
        $("#DATA1").removeClass("input-error");
        $("#DATA1").removeClass("disable");

        document.getElementById("DATA2").readOnly = false;
        $("#DATA2").removeClass("input-error");
        $("#DATA2").removeClass("disable");

        document.getElementById("DATA3").readOnly = false;
        $("#DATA3").removeClass("input-error");
        $("#DATA3").removeClass("disable");

        document.getElementById("DATA4").readOnly = false;
        $("#DATA4").removeClass("input-error");
        $("#DATA4").removeClass("disable");

        document.getElementById("DATA5").readOnly = false;
        $("#DATA5").removeClass("input-error");
        $("#DATA5").removeClass("disable");

        document.getElementById("DATA6").readOnly = false;
        $("#DATA6").removeClass("input-error");
        $("#DATA6").removeClass("disable");

        document.getElementById("DATA7").readOnly = false;
        $("#DATA7").removeClass("input-error");
        $("#DATA7").removeClass("disable");

        document.getElementById("DATA8").readOnly = false;
        $("#DATA8").removeClass("input-error");
        $("#DATA8").removeClass("disable");

        document.getElementById("DATA9").readOnly = false;
        $("#DATA9").removeClass("input-error");
        $("#DATA9").removeClass("disable");

        document.getElementById("DATA10").readOnly = false;
        $("#DATA10").removeClass("input-error");
        $("#DATA10").removeClass("disable");

        $("#FUNC_CAT_NAME1").prop("disabled", false);
        $("#FUNC_CAT_NAME1").removeClass("input-error");
        $("#FUNC_CAT_NAME1").removeClass("disable");

        $("#FUNC_CAT_NAME2").prop("disabled", false);
        $("#FUNC_CAT_NAME2").removeClass("input-error");
        $("#FUNC_CAT_NAME2").removeClass("disable");

        $("#FUNC_CAT_NAME3").prop("disabled", false);
        $("#FUNC_CAT_NAME3").removeClass("input-error");
        $("#FUNC_CAT_NAME3").removeClass("disable");
        //入力可否を切り替えたプルダウンの×（クリアボタン）の表示を更新する
        refreshClearable("#FUNC_CAT_NAME1, #FUNC_CAT_NAME2, #FUNC_CAT_NAME3");

        document.getElementById("FUNC_CAT_SORT").readOnly = false;
        $("#FUNC_CAT_SORT").removeClass("input-error");
        $("#FUNC_CAT_SORT").removeClass("disable");

        $("#SETTING_OBJECT_DIV_HT").prop("disabled", false);
        $("#SETTING_OBJECT_DIV_HT").removeClass("input-error");
        $("#SETTING_OBJECT_DIV_HT").removeClass("disable");

        $("#SETTING_OBJECT_DIV_PC").prop("disabled", false);
        $("#SETTING_OBJECT_DIV_PC").removeClass("input-error");
        $("#SETTING_OBJECT_DIV_PC").removeClass("disable");

        $("#SETTING_OBJECT_DIV_WEB").prop("disabled", false);
        $("#SETTING_OBJECT_DIV_WEB").removeClass("input-error");
        $("#SETTING_OBJECT_DIV_WEB").removeClass("disable");

        $("#SETTING_OBJECT_DIV_ALL").prop("disabled", false);
        $("#SETTING_OBJECT_DIV_ALL").removeClass("input-error");
        $("#SETTING_OBJECT_DIV_ALL").removeClass("disable");

        $("#UPD_AUTHORITY_FLG_Y").prop("disabled", false);
        $("#UPD_AUTHORITY_FLG_Y").removeClass("input-error");
        $("#UPD_AUTHORITY_FLG_Y").removeClass("disable");

        $("#UPD_AUTHORITY_FLG_N").prop("disabled", false);
        $("#UPD_AUTHORITY_FLG_N").removeClass("input-error");
        $("#UPD_AUTHORITY_FLG_N").removeClass("disable");

        $("#DEL_AUTHORITY_FLG_Y").prop("disabled", false);
        $("#DEL_AUTHORITY_FLG_Y").removeClass("input-error");
        $("#DEL_AUTHORITY_FLG_Y").removeClass("disable");

        $("#DEL_AUTHORITY_FLG_N").prop("disabled", false);
        $("#DEL_AUTHORITY_FLG_N").removeClass("input-error");
        $("#DEL_AUTHORITY_FLG_N").removeClass("disable");

        $("#modal-create-btn").css("display", "block");

        $("#FUNC_CAT_SORT").removeClass("input-error");

        //画面を初期化する
        //if (document.getElementById("CopyFlag").value == "") {
        if (document.getElementById("CopyFlag").value == "" && document.getElementById("ErrCode").value == "") {

            document.getElementById("KEY_DIV").value = "";
            document.getElementById("KEY1").value = "";
            document.getElementById("KEY2").value = "";
            document.getElementById("KEY3").value = "";
            document.getElementById("REMARK").value = "";
            document.getElementById("DATA1").value = "";
            document.getElementById("DATA2").value = "";
            document.getElementById("DATA3").value = "";
            document.getElementById("DATA4").value = "";
            document.getElementById("DATA5").value = "";
            document.getElementById("DATA6").value = "";
            document.getElementById("DATA7").value = "";
            document.getElementById("DATA8").value = "";
            document.getElementById("DATA9").value = "";
            document.getElementById("DATA10").value = "";

            //2024/4/18 Add Start Iseda
            document.getElementById("FUNC_CAT_NAME1").value = "";
            document.getElementById("FUNC_CAT_NAME2").value = "";
            document.getElementById("FUNC_CAT_NAME3").value = "";
            //値を設定したプルダウンの×（クリアボタン）の表示を更新する
            refreshClearable("#FUNC_CAT_NAME1, #FUNC_CAT_NAME2, #FUNC_CAT_NAME3");
            document.getElementById("FUNC_CAT_SORT").value = "";
            $('[id=SETTING_OBJECT_DIV_HT]').prop('checked', true);
            $('[id=SETTING_OBJECT_DIV_PC]').prop('checked', false);
            $('[id=SETTING_OBJECT_DIV_WEB]').prop('checked', false);
            $('[id=SETTING_OBJECT_DIV_ALL]').prop('checked', false);
            $('[id=UPD_AUTHORITY_FLG_Y]').prop('checked', true);
            $('[id=UPD_AUTHORITY_FLG_N]').prop('checked', false);
            $('[id=DEL_AUTHORITY_FLG_Y]').prop('checked', true);
            $('[id=DEL_AUTHORITY_FLG_N]').prop('checked', false);
            //2024/4/18 Add END Iseda
        }
        //エラー情報のクリア
        //document.getElementById("ErrCode") = "";
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        //登録ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_CREATE_CLICK";
        // ダイアログを表示
        ShowModalWindow();
        return true;
    }

    //変更ボタンクリック処理
    function btnEdit_onclick() {
        //テーブルの選択行を取得する。
        var text = [];
        //--2019/11/21 nv miyaji upd start
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        //$("[type=checkbox]:checked").each(function () {
        //    text.push($(this).val());
        //});
        //--2019/11/21 nv miyaji upd end

        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        } else if (text.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }

        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

        //KEY-DIV,KEY1,KEY2,KEY3を書き込み不可
        document.getElementById("KEY_DIV").readOnly = true;
        $("#KEY_DIV").removeClass("input-error");
        $("#KEY_DIV").addClass("disable");

        document.getElementById("KEY1").readOnly = true;
        $("#KEY1").removeClass("input-error");
        $("#KEY1").addClass("disable");

        document.getElementById("KEY2").readOnly = true;
        $("#KEY2").removeClass("input-error");
        $("#KEY2").addClass("disable");

        document.getElementById("KEY3").readOnly = true;
        $("#KEY3").removeClass("input-error");
        $("#KEY3").addClass("disable");


        @*$("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");*@
        $("#FUNC_CAT_SORT").removeClass("input-error");


        //削除ボタンを表示
        //$("#modal-delete-btn").css("display", "block");

        //エラーの場合検索しない
        @*if ("@Model.FirstOrDefault.ErrCode" == "") {*@
        if (document.getElementById("ErrCode").value == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/Setting")/GetEditData',
                cache: false,
                data: { KEYS: text[0] },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }

                    //取得したデータを表示する
                    document.getElementById("KEY_DIV").value = data.KEY_DIV;
                    document.getElementById("KEY1").value = data.KEY1;
                    document.getElementById("KEY2").value = data.KEY2;
                    document.getElementById("KEY3").value = data.KEY3;
                    document.getElementById("REMARK").value = data.REMARK;
                    document.getElementById("DATA1").value = data.DATA1;
                    document.getElementById("DATA2").value = data.DATA2;
                    document.getElementById("DATA3").value = data.DATA3;
                    document.getElementById("DATA4").value = data.DATA4;
                    document.getElementById("DATA5").value = data.DATA5;
                    document.getElementById("DATA6").value = data.DATA6;
                    document.getElementById("DATA7").value = data.DATA7;
                    document.getElementById("DATA8").value = data.DATA8;
                    document.getElementById("DATA9").value = data.DATA9;
                    document.getElementById("DATA10").value = data.DATA10;

                    //2024/4/18 Add Start Iseda
                    document.getElementById("FUNC_CAT_NAME1").value = data.FUNC_CAT_NAME1;
                    document.getElementById("FUNC_CAT_NAME2").value = data.FUNC_CAT_NAME2;
                    document.getElementById("FUNC_CAT_NAME3").value = data.FUNC_CAT_NAME3;
                    //値を設定したプルダウンの×（クリアボタン）の表示を更新する
                    refreshClearable("#FUNC_CAT_NAME1, #FUNC_CAT_NAME2, #FUNC_CAT_NAME3");
                    document.getElementById("FUNC_CAT_SORT").value = data.FUNC_CAT_SORT;

                    //設定対象区分ラジオボタンのチェック状態設定
                    if (data.SETTING_OBJECT_DIV == "@SETTING_OBJECT_DIV.HT") {
                        $('[id=SETTING_OBJECT_DIV_HT]').prop('checked', true);
                    } else if (data.SETTING_OBJECT_DIV == "@SETTING_OBJECT_DIV.PC") {
                        $('[id=SETTING_OBJECT_DIV_PC]').prop('checked', true);
                    } else if (data.SETTING_OBJECT_DIV == "@SETTING_OBJECT_DIV.WEB") {
                        $('[id=SETTING_OBJECT_DIV_WEB]').prop('checked', true);
                    } else if (data.SETTING_OBJECT_DIV == "@SETTING_OBJECT_DIV.ALL") {
                        $('[id=SETTING_OBJECT_DIV_ALL]').prop('checked', true);
                    }

                    //更新可否ラジオボタンのチェック状態設定
                    if (data.UPD_AUTHORITY_FLG == "@UPD_AUTHORITY_FLG.Yes") {
                        $('[id=UPD_AUTHORITY_FLG_Y]').prop('checked', true);
                    } else if (data.UPD_AUTHORITY_FLG == "@UPD_AUTHORITY_FLG.No") {
                        $('[id=UPD_AUTHORITY_FLG_N]').prop('checked', true);
                    }

                    //更新可否ラジオボタンのチェック状態設定
                    if (data.DEL_AUTHORITY_FLG == "@DEL_AUTHORITY_FLG.Yes") {
                        $('[id=DEL_AUTHORITY_FLG_Y]').prop('checked', true);
                    } else if (data.DEL_AUTHORITY_FLG == "@DEL_AUTHORITY_FLG.No") {
                        $('[id=DEL_AUTHORITY_FLG_N]').prop('checked', true);
                    }
                    //2024/4/18 Add END Iseda

                    document.getElementById("INS_DATETIME").value = data.EDIT_INS_DATETIME;
                    document.getElementById("INS_EMPLOYEE_CD").value = data.EDIT_INS_EMPLOYEE_CD;
                    document.getElementById("INS_EMPLOYEE_NAME").value = data.EDIT_INS_EMPLOYEE_NAME;
                    document.getElementById("UPD_DATETIME").value = data.EDIT_UPD_DATETIME;
                    document.getElementById("UPD_EMPLOYEE_CD").value = data.EDIT_UPD_EMPLOYEE_CD;
                    document.getElementById("UPD_EMPLOYEE_NAME").value = data.EDIT_UPD_EMPLOYEE_NAME;
                    document.getElementById("UPD_TASK").value = data.RenewProcess;

                    //2024/4/18 Add Start Iseda
                    //更新権限がないデータは押下不可に
                    if (data.UPD_AUTHORITY_FLG == "@UPD_AUTHORITY_FLG.No") {
                        document.getElementById("REMARK").readOnly = true;
                        $("#REMARK").removeClass("input-error");
                        $("#REMARK").addClass("disable");

                        document.getElementById("DATA1").readOnly = true;
                        $("#DATA1").removeClass("input-error");
                        $("#DATA1").addClass("disable");

                        document.getElementById("DATA2").readOnly = true;
                        $("#DATA2").removeClass("input-error");
                        $("#DATA2").addClass("disable");

                        document.getElementById("DATA3").readOnly = true;
                        $("#DATA3").removeClass("input-error");
                        $("#DATA3").addClass("disable");

                        document.getElementById("DATA4").readOnly = true;
                        $("#DATA4").removeClass("input-error");
                        $("#DATA4").addClass("disable");

                        document.getElementById("DATA5").readOnly = true;
                        $("#DATA5").removeClass("input-error");
                        $("#DATA5").addClass("disable");

                        document.getElementById("DATA6").readOnly = true;
                        $("#DATA6").removeClass("input-error");
                        $("#DATA6").addClass("disable");

                        document.getElementById("DATA7").readOnly = true;
                        $("#DATA7").removeClass("input-error");
                        $("#DATA7").addClass("disable");

                        document.getElementById("DATA8").readOnly = true;
                        $("#DATA8").removeClass("input-error");
                        $("#DATA8").addClass("disable");

                        document.getElementById("DATA9").readOnly = true;
                        $("#DATA9").removeClass("input-error");
                        $("#DATA9").addClass("disable");

                        document.getElementById("DATA10").readOnly = true;
                        $("#DATA10").removeClass("input-error");
                        $("#DATA10").addClass("disable");

                        $("#FUNC_CAT_NAME1").prop("disabled", true);
                        $("#FUNC_CAT_NAME1").removeClass("input-error");
                        $("#FUNC_CAT_NAME1").addClass("disable");

                        $("#FUNC_CAT_NAME2").prop("disabled", true);
                        $("#FUNC_CAT_NAME2").removeClass("input-error");
                        $("#FUNC_CAT_NAME2").addClass("disable");

                        $("#FUNC_CAT_NAME3").prop("disabled", true);
                        $("#FUNC_CAT_NAME3").removeClass("input-error");
                        $("#FUNC_CAT_NAME3").addClass("disable");
                        //入力可否を切り替えたプルダウンの×（クリアボタン）の表示を更新する（入力不可の場合は×を表示しない）
                        refreshClearable("#FUNC_CAT_NAME1, #FUNC_CAT_NAME2, #FUNC_CAT_NAME3");

                        document.getElementById("FUNC_CAT_SORT").readOnly = true;
                        $("#FUNC_CAT_SORT").removeClass("input-error");
                        $("#FUNC_CAT_SORT").addClass("disable");

                        $("#SETTING_OBJECT_DIV_HT").prop("disabled", true);
                        $("#SETTING_OBJECT_DIV_HT").removeClass("input-error");
                        $("#SETTING_OBJECT_DIV_HT").addClass("disable");

                        $("#SETTING_OBJECT_DIV_PC").prop("disabled", true);
                        $("#SETTING_OBJECT_DIV_PC").removeClass("input-error");
                        $("#SETTING_OBJECT_DIV_PC").addClass("disable");

                        $("#SETTING_OBJECT_DIV_WEB").prop("disabled", true);
                        $("#SETTING_OBJECT_DIV_WEB").removeClass("input-error");
                        $("#SETTING_OBJECT_DIV_WEB").addClass("disable");

                        $("#SETTING_OBJECT_DIV_ALL").prop("disabled", true);
                        $("#SETTING_OBJECT_DIV_ALL").removeClass("input-error");
                        $("#SETTING_OBJECT_DIV_ALL").addClass("disable");

                        $("#UPD_AUTHORITY_FLG_Y").prop("disabled", true);
                        $("#UPD_AUTHORITY_FLG_Y").removeClass("input-error");
                        $("#UPD_AUTHORITY_FLG_Y").addClass("disable");

                        $("#UPD_AUTHORITY_FLG_N").prop("disabled", true);
                        $("#UPD_AUTHORITY_FLG_N").removeClass("input-error");
                        $("#UPD_AUTHORITY_FLG_N").addClass("disable");

                        $("#DEL_AUTHORITY_FLG_Y").prop("disabled", true);
                        $("#DEL_AUTHORITY_FLG_Y").removeClass("input-error");
                        $("#DEL_AUTHORITY_FLG_Y").addClass("disable");

                        $("#DEL_AUTHORITY_FLG_N").prop("disabled", true);
                        $("#DEL_AUTHORITY_FLG_N").removeClass("input-error");
                        $("#DEL_AUTHORITY_FLG_N").addClass("disable");

                        $("#modal-create-btn").css("display", "none");

                    } else {

                        document.getElementById("REMARK").readOnly = false;
                        $("#REMARK").removeClass("input-error");
                        $("#REMARK").removeClass("disable");

                        document.getElementById("DATA1").readOnly = false;
                        $("#DATA1").removeClass("input-error");
                        $("#DATA1").removeClass("disable");

                        document.getElementById("DATA2").readOnly = false;
                        $("#DATA2").removeClass("input-error");
                        $("#DATA2").removeClass("disable");

                        document.getElementById("DATA3").readOnly = false;
                        $("#DATA3").removeClass("input-error");
                        $("#DATA3").removeClass("disable");

                        document.getElementById("DATA4").readOnly = false;
                        $("#DATA4").removeClass("input-error");
                        $("#DATA4").removeClass("disable");

                        document.getElementById("DATA5").readOnly = false;
                        $("#DATA5").removeClass("input-error");
                        $("#DATA5").removeClass("disable");

                        document.getElementById("DATA6").readOnly = false;
                        $("#DATA6").removeClass("input-error");
                        $("#DATA6").removeClass("disable");

                        document.getElementById("DATA7").readOnly = false;
                        $("#DATA7").removeClass("input-error");
                        $("#DATA7").removeClass("disable");

                        document.getElementById("DATA8").readOnly = false;
                        $("#DATA8").removeClass("input-error");
                        $("#DATA8").removeClass("disable");

                        document.getElementById("DATA9").readOnly = false;
                        $("#DATA9").removeClass("input-error");
                        $("#DATA9").removeClass("disable");

                        document.getElementById("DATA10").readOnly = false;
                        $("#DATA10").removeClass("input-error");
                        $("#DATA10").removeClass("disable");

                        $("#FUNC_CAT_NAME1").prop("disabled", false);
                        $("#FUNC_CAT_NAME1").removeClass("input-error");
                        $("#FUNC_CAT_NAME1").removeClass("disable");

                        $("#FUNC_CAT_NAME2").prop("disabled", false);
                        $("#FUNC_CAT_NAME2").removeClass("input-error");
                        $("#FUNC_CAT_NAME2").removeClass("disable");

                        $("#FUNC_CAT_NAME3").prop("disabled", false);
                        $("#FUNC_CAT_NAME3").removeClass("input-error");
                        $("#FUNC_CAT_NAME3").removeClass("disable");
                        //入力可否を切り替えたプルダウンの×（クリアボタン）の表示を更新する
                        refreshClearable("#FUNC_CAT_NAME1, #FUNC_CAT_NAME2, #FUNC_CAT_NAME3");

                        document.getElementById("FUNC_CAT_SORT").readOnly = false;
                        $("#FUNC_CAT_SORT").removeClass("input-error");
                        $("#FUNC_CAT_SORT").removeClass("disable");

                        $("#SETTING_OBJECT_DIV_HT").prop("disabled", false);
                        $("#SETTING_OBJECT_DIV_HT").removeClass("input-error");
                        $("#SETTING_OBJECT_DIV_HT").removeClass("disable");

                        $("#SETTING_OBJECT_DIV_PC").prop("disabled", false);
                        $("#SETTING_OBJECT_DIV_PC").removeClass("input-error");
                        $("#SETTING_OBJECT_DIV_PC").removeClass("disable");

                        $("#SETTING_OBJECT_DIV_WEB").prop("disabled", false);
                        $("#SETTING_OBJECT_DIV_WEB").removeClass("input-error");
                        $("#SETTING_OBJECT_DIV_WEB").removeClass("disable");

                        $("#SETTING_OBJECT_DIV_ALL").prop("disabled", false);
                        $("#SETTING_OBJECT_DIV_ALL").removeClass("input-error");
                        $("#SETTING_OBJECT_DIV_ALL").removeClass("disable");

                        $("#UPD_AUTHORITY_FLG_Y").prop("disabled", false);
                        $("#UPD_AUTHORITY_FLG_Y").removeClass("input-error");
                        $("#UPD_AUTHORITY_FLG_Y").removeClass("disable");

                        $("#UPD_AUTHORITY_FLG_N").prop("disabled", false);
                        $("#UPD_AUTHORITY_FLG_N").removeClass("input-error");
                        $("#UPD_AUTHORITY_FLG_N").removeClass("disable");

                        $("#DEL_AUTHORITY_FLG_Y").prop("disabled", false);
                        $("#DEL_AUTHORITY_FLG_Y").removeClass("input-error");
                        $("#DEL_AUTHORITY_FLG_Y").removeClass("disable");

                        $("#DEL_AUTHORITY_FLG_N").prop("disabled", false);
                        $("#DEL_AUTHORITY_FLG_N").removeClass("input-error");
                        $("#DEL_AUTHORITY_FLG_N").removeClass("disable");

                        $("#modal-create-btn").css("display", "block");

                    }
                    //2024/4/18 Add END Iseda

                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }
        //document.getElementById("ErrCode").value = "";
        //変更ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_UPDATE_CLICK";
        // ダイアログを表示
        ShowModalWindow();
        return true;
    }

    //登録ボタンクリック処理（確認メッセージ）
    function btnCreateModal_onclick() {
        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
            showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/Setting")/Create")
        } else {
            showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/Setting")/Create")
        }
    }

    //削除ボタンクリック処理
    function btnDelete_onclick() {
        //テーブルの選択行を取得する。
        var text = [];
        var keyDiv = [];
        var delAuthorityFlg = true;
        var keyDivOnlyFlg = true;
        //--2019/11/21 nv miyaji upd start
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            var item = grid.getDataItem(i)
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
                keyDiv.push(item.KEY_DIV)
                if (item.DEL_AUTHORITY_FLG == "N") {
                    delAuthorityFlg = false
                }
            }
        }

        //$("[type=checkbox]:checked").each(function () {
        //    text.push($(this).val());
        //});
        //--2019/11/21 nv miyaji upd end

        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        //削除権限なし
        if (!delAuthorityFlg) {
            showErrMessage("@Message.getMessage(CommonLabelSetting.MSG_ERR_ID_NO_DELETE_AUTHORITY)")
            return false;
        }

        //種別に1つしかないデータの確認
        if (text.length != 0) {
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/Setting")/DelErrorCheck',
                dataType: 'json',
                cache: false,
                data: { deleteData: keyDiv },
                success: function (data, dataType) {
                    if (data.ErrCode == "@CommonLabelSetting.MSG_ERR_ID_DATA_ONLY_ONE") {
                        showConfirmDeleteOnlyKeyDiv("@Message.getMessage(CommonLabelSetting.MSG_ERR_ID_DATA_ONLY_ONE)", text)

                    } else {

                        var keys = "";
                        for (let i = 0; i < text.length; i++) {
                            if (i > 0) { keys = keys + ":"; }
                            keys = keys + text[i];
                        }
                        
                        document.getElementById("DeleteKeys").value = keys;

                        //確認メッセージ
                        showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/Setting")/Delete");
                    }

                },

                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }

    }


    function showConfirmDeleteOnlyKeyDiv(strMessage, text) {

        // ダイアログのメッセージを設定
        strMessage = strMessage.replace("@@CR@@", "<BR>");
        $("#show-dialog").html(strMessage);

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $(this).dialog("close");

                    var keys = "";
                    for (let i = 0; i < text.length; i++) {
                        if (i > 0) { keys = keys + ":"; }
                        keys = keys + text[i];
                    }

                    document.getElementById("DeleteKeys").value = keys;

                    //確認メッセージ
                    showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/Setting")/Delete");

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

    function delonlyFlg(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        $('#form').submit();   //2020/02/20 nv miyaji
    }


    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

</script>
<style type="text/css">
    /* モーダルの×付きプルダウン：プルダウンの幅に合わせて×の位置を決める（×がプルダウンの枠外に出ないようにする） */
    #modal-content .clearable {
        display: inline-block;
        max-width: 100%;
    }
</style>

<div id="setting-master">
@Using (Html.BeginForm("Index", "Setting", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20">
             <div id="view-area-button-parent">
                 <div  class="view-area-button-left">
                     <input class="defButton copy-btn" style="width:120px" type="button" value="@Message.getMessage(CommonLabel.BUTTON_COPY_CREATE)" onclick="ClearCopyFlag('1'); return btnCopyCreate_onclick()">
                     <input class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="ClearCopyFlag(''); return btnCreate_onclick()">
                     <input class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="ClearCopyFlag(''); return btnEdit_onclick()">
                     <input class="defButton delete-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="ClearCopyFlag(''); return btnDelete_onclick()">
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
                         <div class="col-md-12">
                        @If Model.FirstOrDefault.GridInfo("KEY_DIV").DispFlag = "1" OrElse
        Model.FirstOrDefault.GridInfo("FUNC_CAT_NAME").DispFlag = "1" Then
                            @<div Class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("KEY_DIV").DispFlag = "1", "display", "None"))">
                                 <div Class="grid-head-col">@Message.getMessage(CommonLabelSetting.LABEL_KEY_DIV)</div>
                                 <div Class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchKeyDiv, New With {.class = "cond-control", .maxlength = 30})
                                 </div>
                             </div>

                            @<div Class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("FUNC_CAT_NAME").DispFlag = "1", "display", "None"))">
                                <div Class="grid-head-col">@Message.getMessage(CommonLabelSetting.LABEL_FUNC_CAT_NAME)</div>
                                <div Class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchFuncCatName, DirectCast(ViewBag.FuncCatNameList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                        End If

                             <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("SETTING_OBJECT_DIV").DispFlag = "1", "display", "None"));max-width:300px;">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV)</div>
                                 <div id="setting-object-div" class="grid-condition-col">
                                     <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchSettingObjectDivHT)@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_HT)</div>
                                     <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchSettingObjectDivPC)@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_PC)</div>
                                     <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchSettingObjectDivWEB)@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_WEB)</div>
                                     <div class="input-select-item-last">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchSettingObjectDivALL)@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_ALL)</div>
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
                     @*@PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)*@
                     @<div Class="list-view">
                         @* 高さはヘッダー固定の処理（enableFixedHeader）で画面の残り領域に合わせる *@
                         <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
                     </div>

    'CSV取込のモーダル画面
                     @<div id="modal-csv-content">
                         <div class="popup-title-wrap">
                             <p class="Line3"></p>
                             <p class="Line4"></p>
                             <h2 id="popup-title-csv"></h2>
                             <p class="Line3"></p>
                             <p class="Line4"></p>
                         </div>
                         @CODE
                             'モデルの値を設定
                             Dim csvSessionModel As SettingViewModel = Session(CommonSessionKey.CREAGE_SETTING)
                             If Not IsNothing(csvSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                                 Model.Add(csvSessionModel)
                             Else
                                 Model.Add(New SettingViewModel())
                             End If
                         END CODE
                         @*@Html.AntiForgeryToken()*@
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
                     @<div id="modal-content">
                         <div class="popup-title-wrap">
                             <p class="Line3"></p>
                             <p class="Line4"></p>
                             <h2 id="popup-title"></h2>
                             <p class="Line3"></p>
                             <p class="Line4"></p>
                         </div>
                         @CODE
                             'モデルの値を設定
                             Dim SessionModel As SettingViewModel = Session(CommonSessionKey.CREAGE_SETTING)
                             If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                                 Model.Add(SessionModel)
                             Else
                                 Model.Add(New SettingViewModel())
                             End If
                         END CODE
                         @Html.AntiForgeryToken()
                         <Table>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_KEY_DIV)</td>
                                 <td width="20" style="display:inline-block;"><font Class="require">※</font></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.KEY_DIV, New With {.htmlattributes = New With {.size = 62, .maxlength = 30}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_KEY1)</td>
                                 <td width="20" style="display:inline-block;"><font Class="require">※</font></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.KEY1, New With {.htmlattributes = New With {.size = 30, .maxlength = 30}})</td>
                                 <td width="10" style="display:inline-block;"></td>
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_KEY2)</td>
                                 <td width="20" style="display:inline-block;"><font Class="require">※</font></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.KEY2, New With {.htmlattributes = New With {.size = 30, .maxlength = 30}})</td>
                                 <td width="10" style="display:inline-block;"></td>
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_KEY3)</td>
                                 <td width="20" class="require" style="display:inline-block;">※</td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.KEY3, New With {.htmlattributes = New With {.size = 30, .maxlength = 30}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_REMARK)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.REMARK, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA1)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA1, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA2)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA2, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA3)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA3, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA4)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA4, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA5)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA5, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA6)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA6, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA7)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA7, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA8)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA8, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA9)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA9, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DATA10)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.DATA10, New With {.htmlattributes = New With {.size = 155, .maxlength = 255}})</td>
                             </tr>
                             @* 2024/04/18 sv iseda upd start *@
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_FUNC_CAT_NAME1)</td>
                                 <td width="20" style="display:inline-block;"><font Class="require">※</font></td>
                                 <td width="270px" style="display:inline-block;">
                                     @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                     <span class="clearable">
                                         @Html.DropDownListFor(Function(model) model.Last.FUNC_CAT_NAME1, DirectCast(ViewBag.funcCatNameList1, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 250px;"})
                                         <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                     </span>
                                 </td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_FUNC_CAT_NAME2)</td>
                                 <td width="20" style="display:inline-block;"></td>
                                 <td width="270px" style="display:inline-block;">
                                     <span class="clearable">
                                         @Html.DropDownListFor(Function(model) model.Last.FUNC_CAT_NAME2, DirectCast(ViewBag.funcCatNameList2, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 250px;"})
                                         <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                     </span>
                                 </td>
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_FUNC_CAT_NAME3)</td>
                                 <td width="270px" style="display:inline-block;">
                                     <span class="clearable">
                                         @Html.DropDownListFor(Function(model) model.Last.FUNC_CAT_NAME3, DirectCast(ViewBag.funcCatNameList3, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 250px;"})
                                         <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                     </span>
                                 </td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_FUNC_CAT_SORT)</td>
                                 <td width="20" style="display:inline-block;"><font Class="require">※</font></td>
                                 <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.FUNC_CAT_SORT, New With {.htmlattributes = New With {.maxlength = 5, .oninput = "editNumber(this)", .type = "text", .style = "width:60px;"}})</td>
                                 <td width="10" style="display:inline-block;"></td>
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV)</td>
                                 <td width="20" style="display:inline-block;"><font Class="require">※</font></td>
                                 <td width="60px" style="display:inline-block;">
                                     @Html.RadioButtonFor(Function(model) model.Last.SETTING_OBJECT_DIV, SETTING_OBJECT_DIV.HT, New With {.id = "SETTING_OBJECT_DIV_HT"})@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_HT)
                                 </td>
                                 <td width="60px" style="display:inline-block;">
                                     @Html.RadioButtonFor(Function(model) model.Last.SETTING_OBJECT_DIV, SETTING_OBJECT_DIV.PC, New With {.id = "SETTING_OBJECT_DIV_PC"})@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_PC)
                                 </td>
                                 <td width="60px" style="display:inline-block;">
                                     @Html.RadioButtonFor(Function(model) model.Last.SETTING_OBJECT_DIV, SETTING_OBJECT_DIV.WEB, New With {.id = "SETTING_OBJECT_DIV_WEB"})@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_WEB)
                                 </td>
                                 <td width="60px" style="display:inline-block;">
                                     @Html.RadioButtonFor(Function(model) model.Last.SETTING_OBJECT_DIV, SETTING_OBJECT_DIV.ALL, New With {.id = "SETTING_OBJECT_DIV_ALL"})@Message.getMessage(CommonLabelSetting.LABEL_SETTING_OBJECT_DIV_ALL)
                                 </td>
                             </tr>
                             <tr Class="popup-row">
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_UPD_AUTHORITY_FLG)</td>
                                 <td width="20" style="display:inline-block;"><font Class="require">※</font></td>
                                 <td width="90px" style="display:inline-block;">
                                     @Html.RadioButtonFor(Function(model) model.Last.UPD_AUTHORITY_FLG, UPD_AUTHORITY_FLG.Yes, New With {.id = "UPD_AUTHORITY_FLG_Y"})@Message.getMessage(CommonLabelSetting.LABEL_UPD_AUTHORITY_FLG_Y)
                                 </td>
                                 <td width="90px" style="display:inline-block;">
                                     @Html.RadioButtonFor(Function(model) model.Last.UPD_AUTHORITY_FLG, UPD_AUTHORITY_FLG.No, New With {.id = "UPD_AUTHORITY_FLG_N"})@Message.getMessage(CommonLabelSetting.LABEL_UPD_AUTHORITY_FLG_N)
                                 </td>
                                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelSetting.LABEL_DEL_AUTHORITY_FLG)</td>
                                 <td width="20" style="display:inline-block;"><font Class="require">※</font></td>
                                 <td width="90px" style="display:inline-block;">
                                     @Html.RadioButtonFor(Function(model) model.Last.DEL_AUTHORITY_FLG, DEL_AUTHORITY_FLG.Yes, New With {.id = "DEL_AUTHORITY_FLG_Y"})@Message.getMessage(CommonLabelSetting.LABEL_DEL_AUTHORITY_FLG_Y)
                                 </td>
                                 <td width="90px" style="display:inline-block;">
                                     @Html.RadioButtonFor(Function(model) model.Last.DEL_AUTHORITY_FLG, DEL_AUTHORITY_FLG.No, New With {.id = "DEL_AUTHORITY_FLG_N"})@Message.getMessage(CommonLabelSetting.LABEL_DEL_AUTHORITY_FLG_N)
                                 </td>
                             </tr>
                             @* 2024/04/18 sv iseda upd end *@
                         </Table>

                         @*@Html.HiddenFor(Function(model) model.FirstOrDefault.INS_DATETIME)
                    @Html.HiddenFor(Function(model) model.FirstOrDefault.INS_EMPLOYEE_CD)
                    @Html.HiddenFor(Function(model) model.FirstOrDefault.INS_EMPLOYEE_NAME)
                    @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_DATETIME)
                    @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_CD)
                    @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_NAME)
                    @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_TASK)*@
                         @Html.HiddenFor(Function(model) model.Last.INS_DATETIME)
                         @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_CD)
                         @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_NAME)
                         @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
                         @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
                         @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
                         @Html.HiddenFor(Function(model) model.Last.UPD_TASK)

                         @*<div Class="PopupButton" style="margin:15px auto; width:220px;"> yamada*@
                         <div Class="PopupButton" style="margin:15px auto; width:260px;">
                             <div style="float:left;margin:0 20px 0 0;">
                                 @*<input id="modal-create-btn" class="defButton modal-close modal-create-btn submit-button" formaction="~/Setting/Create" type="submit" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)">*@
                                 <input id="modal-create-btn" class="defButton  modal-create-btn submit-button" formaction="~/Setting/Create" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnCreateModal_onclick()">
                             </div>
                             <div style="float:left;">
                                 <input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
                             </div>
                             <div style="clear:both;"></div>
                         </div>
                     </div>

                     @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
                     @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
                     @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
                     @Html.HiddenFor(Function(model) model.FirstOrDefault.SearchKeyDiv)
                     @Html.HiddenFor(Function(model) model.FirstOrDefault.DeleteKeys)
                     @Html.HiddenFor(Function(model) model.FirstOrDefault.ErrCode)
                     @Html.HiddenFor(Function(model) model.FirstOrDefault.CopyFlag)
                     @Html.HiddenFor(Function(model) model.FirstOrDefault.keyDivOnlyFlg)


                     End Using
                 </div>

