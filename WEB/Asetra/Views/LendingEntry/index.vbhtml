@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of LendingEntryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    If Model.FirstOrDefault.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK Then
        If Model.FirstOrDefault.SenimotoKbn = SenimotoKbn.LendingList Then
            ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_SHOSAI)
        Else
            ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU)
        End If
    Else
        ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU)
    End If
End Code
<script>
    let grid;

    //
    //  予約申請登録/変更
    //
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            // エラーメッセージ表示後、ライセンスコントローラーを呼び出す
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        //貸出番号・管理ロケーション・ユーザー貸出開始日・ユーザー貸出終了日書き込み不可
        //2022/7/25 sv sato upd start
        //document.getElementById("ReserveIdString").readOnly = true;
        //$("#ReserveIdString").addClass("disable");
        //設定マスタの値で活性状態を設定
        if ("@ViewBag.CanDspOptionData1" == "False") {
            document.getElementById("ReserveIdString").readOnly = true;
            $("#ReserveIdString").addClass("disable");
        }
        //2022/7/25 sv sato upd end

        document.getElementById("LocLName").readOnly = true;
        $("#LocLName").addClass("disable");
        document.getElementById("UserStartDtString").readOnly = true;
        $("#UserStartDtString").addClass("disable");
        document.getElementById("UserEndDtString").readOnly = true;
        $("#UserEndDtString").addClass("disable");
        // スーパー管理者以外、書き込み不可
        if ("@ViewBag.AuthDiv" != "@AUTH_DIV.SuperAdmin") {
            $("#REQ_EMP_CD").attr("disabled", "disabled");
        }

        // 書き込み不可設定
        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_UPDATE_CLICK") {
            //引継テキストボックス書き込み不可
            document.getElementById("HikiEmployeeNm").readOnly = true;
            $("#HikiEmployeeNm").addClass("disable");

            // 予約申請変更登録時、書き込み不可
            $("#REQ_EMP_CD").attr("disabled", "disabled");

            // 明細に未貸出以外がある時、実貸出開始日書き込み不可
            if ("@Model.FirstOrDefault.isNotEnableRealStartDt" == "True") {
                document.getElementById("RealStartDtString").readOnly = true;
                $("#RealStartDtString").addClass("disable");
            }

            if ("@Model.FirstOrDefault.AuthProcFlg" == "@AuthProcFlg.ShokaiNomi") {
                // 照会のみ時、書き込み不可
                document.getElementById("RealStartDtString").readOnly = true;
                $("#RealStartDtString").addClass("disable");
                $("#CHARGE_EMP_CD").attr("disabled", "disabled");
                $("#LEASE_PLACE_CD").attr("disabled", "disabled");
                $("#LEASE_USE").attr("disabled", "disabled");
                document.getElementById("DEST_NAME1").readOnly = true;
                $("#DEST_NAME1").addClass("disable");
                document.getElementById("DEST_NAME2").readOnly = true;
                $("#DEST_NAME2").addClass("disable");
                document.getElementById("DEST_ZIP_CODE").readOnly = true;
                $("#DEST_ZIP_CODE").addClass("disable");
                document.getElementById("DEST_ADDRESS1").readOnly = true;
                $("#DEST_ADDRESS1").addClass("disable");
                document.getElementById("DEST_ADDRESS2").readOnly = true;
                $("#DEST_ADDRESS2").addClass("disable");
                document.getElementById("DEST_ADDRESS3").readOnly = true;
                $("#DEST_ADDRESS3").addClass("disable");
                document.getElementById("DEST_PHONE_NUMBER").readOnly = true;
                $("#DEST_PHONE_NUMBER").addClass("disable");
                //2023/03/22 Kawamoto ADD START
                document.getElementById("TO_DEST_NAME").readOnly = true;
                $("#TO_DEST_NAME").addClass("disable");
                $("#RET_LEASE_PLACE_CD").attr("disabled", "disabled");
                document.getElementById("RET_ZIP_CODE").readOnly = true;
                $("#RET_ZIP_CODE").addClass("disable");
                document.getElementById("RET_ADDRESS1").readOnly = true;
                $("#RET_ADDRESS1").addClass("disable");
                document.getElementById("RET_ADDRESS2").readOnly = true;
                $("#RET_ADDRESS2").addClass("disable");
                document.getElementById("RET_ADDRESS3").readOnly = true;
                $("#RET_ADDRESS3").addClass("disable");
                //2023/03/22 Kawamoto ADD END
                document.getElementById("REMARK").readOnly = true;
                $("#REMARK").addClass("disable");
                document.getElementById("REMARK_INTERNAL").readOnly = true;
                $("#REMARK_INTERNAL").addClass("disable");
                if ("@Model.FirstOrDefault.SenimotoKbn" == "@SenimotoKbn.LendingList") {
                    if ("@ViewBag.UpdateBtnEnable" == "False") {
                        document.getElementById("DEST_NO").readOnly = true;
                        $("#DEST_NO").addClass("disable");
                    }
                } else {
                    document.getElementById("DEST_NO").readOnly = true;
                    $("#DEST_NO").addClass("disable");
                }
                document.getElementById("RealEndDtString").readOnly = true;
                $("#RealEndDtString").addClass("disable");
            } else if ("@Model.FirstOrDefault.AuthProcFlg" == "@AuthProcFlg.JiAll") {
                // 制限有り変更時、一部書き込み不可
                document.getElementById("RealStartDtString").readOnly = true;
                $("#RealStartDtString").addClass("disable");
                $("#CHARGE_EMP_CD").attr("disabled", "disabled");
                $("#LEASE_PLACE_CD").attr("disabled", "disabled");
                $("#LEASE_USE").attr("disabled", "disabled");
                //document.getElementById("DEST_NAME1").readOnly = true;
                //$("#DEST_NAME1").addClass("disable");
                //document.getElementById("DEST_NAME2").readOnly = true;
                //$("#DEST_NAME2").addClass("disable");
                document.getElementById("DEST_ZIP_CODE").readOnly = true;
                $("#DEST_ZIP_CODE").addClass("disable");
                document.getElementById("DEST_ADDRESS1").readOnly = true;
                $("#DEST_ADDRESS1").addClass("disable");
                document.getElementById("DEST_ADDRESS2").readOnly = true;
                $("#DEST_ADDRESS2").addClass("disable");
                document.getElementById("DEST_ADDRESS3").readOnly = true;
                $("#DEST_ADDRESS3").addClass("disable");
                document.getElementById("DEST_PHONE_NUMBER").readOnly = true;
                $("#DEST_PHONE_NUMBER").addClass("disable");
                //2023/03/22 Kawamoto ADD START
                $("#RET_LEASE_PLACE_CD").attr("disabled", "disabled");
                document.getElementById("RET_ZIP_CODE").readOnly = true;
                $("#RET_ZIP_CODE").addClass("disable");
                document.getElementById("RET_ADDRESS1").readOnly = true;
                $("#RET_ADDRESS1").addClass("disable");
                document.getElementById("RET_ADDRESS2").readOnly = true;
                $("#RET_ADDRESS2").addClass("disable");
                document.getElementById("RET_ADDRESS3").readOnly = true;
                $("#RET_ADDRESS3").addClass("disable");
                //2023/03/22 Kawamoto ADD END
                document.getElementById("REMARK").readOnly = true;
                $("#REMARK").addClass("disable");
            }

        }

        // 一覧表示
        CreateGrid()

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessCreate") {
            //登録
            // メッセージ表示後、予約状況照会画面を呼び出す
            showMessageAndSeni("@Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE)", "@Url.Content("~/LendingStatus")/Index");
            return;
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessEdit") {
            //変更
            if ("@Model.FirstOrDefault.SenimotoKbn" == "@SenimotoKbn.LendingList") {
                // 予約詳細画面からの変更
                // メッセージ表示
                showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)");
            } else {
                // 予約申請登録画面での変更
                // メッセージ表示後、予約状況照会画面を呼び出す
                showMessageAndSeni("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)", "@Url.Content("~/LendingStatus")/Index");
            }
            return;
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessDelete") {
            //削除
            // メッセージ表示後、予約状況照会画面を呼び出す
            showMessageAndSeni("@Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE)", "@Url.Content("~/LendingStatus")/Index");
            return;
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessLending") {
            //貸出
            // メッセージ表示
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_LENDING)");
            @*showMessageAndSeni("@Message.getMessage(CommonLabel.MSG_SUCCESS_LENDING)", "@Url.Content("~/LendingList")/Index");*@
            return;
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessReturned") {
            //返却
            // メッセージ表示
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_RETURNED)");
            @*showMessageAndSeni("@Message.getMessage(CommonLabel.MSG_SUCCESS_RETURNED)", "@Url.Content("~/LendingList")/Index");*@
            return;
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            // String.Formatしたい文言の場合、直接エラーコードに文言を入れているので修正。
            var Sentou = "@Model.FirstOrDefault.ErrCode"
            if (Sentou.slice(0, 2) == "WM") {
                showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
            } else {
                showErrMessage("@Model.FirstOrDefault.ErrCode", "#@Model.FirstOrDefault.ErrTarget")
            }
            //showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")

        }
    });

    function CreateGrid(data) {
        // 既存のグリッド要素をクリア
        var gridElement = document.getElementById('ListGrids');
        gridElement.innerHTML = '';

        // 列定義
        let columns = [
            { id: "@ColKey.LendingEntry.CHECK_SELECT", name: "", field: "DISP_FLAG", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.LendingEntry.CHECK_SELECT", 40), sortOrder: -2, formatter: LendEntryCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.LendingEntry.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.LENDING_ENTRY", "@ColKey.LendingEntry.ASSET_NO", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag" },
            { id: "@ColKey.LendingEntry.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)", field: "AssetName", width: getColumnWidth("@DispId.LENDING_ENTRY", "@ColKey.LendingEntry.ASSET_NAME", 250), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag" },
            { id: "@ColKey.LendingEntry.SERIAL_NO", name: "@Message.getMessage(CommonLabel.LABEL_SERIAL_NO)", field: "SerialNo", width: getColumnWidth("@DispId.LENDING_ENTRY", "@ColKey.LendingEntry.SERIAL_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SERIAL_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SERIAL_NO").DispFlag" },
            { id: "@ColKey.LendingEntry.LEASE_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)", field: "RESERVE_STATUS", width: getColumnWidth("@DispId.LENDING_ENTRY", "@ColKey.LendingEntry.LEASE_STATUS", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_STATUS").DispFlag", formatter: LeaseStatusFormatter },
            { id: "@ColKey.LendingEntry.LEASE_KIKAN", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_KIKAN)", field: "REAL_START_DT", width: getColumnWidth("@DispId.LENDING_ENTRY", "@ColKey.LendingEntry.LEASE_KIKAN", 250), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_KIKAN").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_KIKAN").DispFlag", formatter: LeaseKikanFormatter },
            { id: "@ColKey.LendingEntry.ASSET_LEASE_LIMIT", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_LEASE_LIMIT)", field: "LeaseKigen", width: getColumnWidth("@DispId.LENDING_ENTRY", "@ColKey.LendingEntry.ASSET_LEASE_LIMIT", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_LEASE_LIMIT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_LEASE_LIMIT").DispFlag" },
            { id: "@ColKey.LendingEntry.EMPLOYEE_CD", name: "@Message.getMessage(CommonLabel.LABEL_EMP)", field: "EmployeeName", width: getColumnWidth("@DispId.LENDING_ENTRY", "@ColKey.LendingEntry.EMPLOYEE_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("EMPLOYEE_CD").DispFlag" },
        ]

        // Gridのオプション機能の設定
        let options = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: true,                       //Gridの高さの自動調整可否
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        let jsonData = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.ReserveDetailData.Count" > 0) {
            // 安全に使用できるようにJSON文字列をエンコード
            if (data) {
                jsonData = data
            } else {
                jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model.FirstOrDefault.ReserveDetailData))) ');
            }
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列を加算
        }

        grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.LENDING_ENTRY", fixCol);

    }

    // チェックボックス(中央寄せ)
    function LendEntryCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.ASSET_NO);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheck" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" ' +
               (isKeyInclude ? 'checked="checked" ' : '') +
               '/>' +
               '</div>';
    }

    // 貸出状態用formatter
    function LeaseStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch (dataContext.RESERVE_STATUS) {
            case "@ReserveStatus.Mi":
                status = "@Message.getMessage(CommonLabel.VALUE_NOT_RENTED)"
                break;
            case "@ReserveStatus.Lending":
                status = "@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)"
                break;
            case "@ReserveStatus.HenkyakuSumi":
                if (dataContext.HANDOVER_DIV === "@HandOverDiv.HikitsugiMoto") {
                    status = "@Message.getMessage(CommonLabel.LABEL_HIKITSUGI)"
                } else {
                    status = "@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)"
                }

                break;
        }

        return status;
    }

    // 貸出期間用formatter
    function LeaseKikanFormatter(row, cell, value, columnDef, dataContext) {
        let sFromDt = "";
        let sFromColor = "black";
        if (dataContext.JuufukuAriFrom) {
            sFromColor = "red"
        }

        let sNakaColor = "black";
        if (dataContext.JuufukuAriNaka) {
            sNakaColor = "red"
        }

        let sToColor = "black";
        if (dataContext.JuufukuAriTo) {
            sToColor = "red"
        }

        if (dataContext.REAL_START_DT != 0 && dataContext.REAL_START_DT >= 10000101) {
            sFromDt = toStringDateYMD(dataContext.REAL_START_DT)
        } else {
            sFromDt = "　　　　　"
        }

        let sToDt = "";
        if (dataContext.REAL_END_DT != 0 && dataContext.REAL_END_DT >= 10000101) {
            sToDt = toStringDateYMD(dataContext.REAL_END_DT)
        }

        let sNaka = "～";
        if (dataContext.REAL_START_DT == 0 && dataContext.REAL_END_DT == 0) {
            sNaka = "";
        }

        let sComment = sFromDt;
        if (!sFromDt.trim == "" && sToDt == "") {
            sComment = sFromDt & "～" & sToDt
        }

        return '<span style="color:' + sFromColor + ';">' + sFromDt + '</span>' +
             '<span style="color:' + sNakaColor + ';">' + sNaka + '</span>' +
             '<span style="color:' + sToColor + ';">' + sToDt + '</span>';
    }

    // 登録ボタンクリック処理
    function btnCreate_onclick() {

        // 明細テーブルを取得
        var rowCount = grid.getDataLength();
        // 明細が１行もない場合
        if (rowCount == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_MEISAI_EMPTY)")
            return;
        }

        //確認メッセージを出し、ＯＫなら変更処理
        // ダイアログのメッセージを設定
        if (document.getElementById("WarnMainteKbn").value == "" ) {
            $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)");
        } else {
            if (document.getElementById("WarnMainteKbn").value === "@MainteKbn.Mainte") {
                $("#show-dialog").html("@String.Format(Message.getMessage(CommonLabel.MSG_INS_CONFIRM_WARN_MAINTE_KBN), Message.getMessage(CommonLabel.VALUE_MAINTE))");
            } else if (document.getElementById("WarnMainteKbn").value === "@MainteKbn.BlaRegist"){
                $("#show-dialog").html("@String.Format(Message.getMessage(CommonLabel.MSG_INS_CONFIRM_WARN_MAINTE_KBN), Message.getMessage(CommonLabel.VALUE_BLAREGIST))");
            } else if (document.getElementById("WarnMainteKbn").value === "@MainteKbn.PreRegist1"){
                $("#show-dialog").html("@String.Format(Message.getMessage(CommonLabel.MSG_INS_CONFIRM_WARN_MAINTE_KBN), Message.getMessage(CommonLabel.VALUE_PREREGIST1))");
            } else if (document.getElementById("WarnMainteKbn").value === "@MainteKbn.PreRegist2"){
                $("#show-dialog").html("@String.Format(Message.getMessage(CommonLabel.MSG_INS_CONFIRM_WARN_MAINTE_KBN), Message.getMessage(CommonLabel.VALUE_PREREGIST2))");
            }
        }

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $("#REQ_EMP_CD").removeAttr("disabled");
                    $(this).dialog("close");
                    $('#form').attr("action", "@Url.Content("~/LendingEntry")/Create");
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
        @*showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/LendingEntry")/Create")*@
        return true;
    }

    // 変更ボタンクリック処理
    function btnEdit_onclick() {

        // 明細テーブルを取得
        var rowCount = grid.getDataLength();
        // 明細が１行もない場合
        if (rowCount == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_MEISAI_EMPTY)")
            return;
        }

        //確認メッセージを出し、ＯＫなら変更処理
        // ダイアログのメッセージを設定

        if (document.getElementById("WarnMainteKbn").value == "" ) {
                $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)");
        } else {
            if (document.getElementById("WarnMainteKbn").value === "@MainteKbn.Mainte") {
                    $("#show-dialog").html("@String.Format(Message.getMessage(CommonLabel.MSG_UPD_CONFIRM_WARN_MAINTE_KBN), Message.getMessage(CommonLabel.VALUE_MAINTE))");
            } else if (document.getElementById("WarnMainteKbn").value === "@MainteKbn.BlaRegist"){
                    $("#show-dialog").html("@String.Format(Message.getMessage(CommonLabel.MSG_UPD_CONFIRM_WARN_MAINTE_KBN), Message.getMessage(CommonLabel.VALUE_BLAREGIST))");
            } else if (document.getElementById("WarnMainteKbn").value === "@MainteKbn.PreRegist1"){
                    $("#show-dialog").html("@String.Format(Message.getMessage(CommonLabel.MSG_UPD_CONFIRM_WARN_MAINTE_KBN), Message.getMessage(CommonLabel.VALUE_PREREGIST1))");
            } else if (document.getElementById("WarnMainteKbn").value === "@MainteKbn.PreRegist2"){
                    $("#show-dialog").html("@String.Format(Message.getMessage(CommonLabel.MSG_UPD_CONFIRM_WARN_MAINTE_KBN), Message.getMessage(CommonLabel.VALUE_PREREGIST2))");
            }
        }


        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $("#REQ_EMP_CD").removeAttr("disabled");
                    $("#CHARGE_EMP_CD").removeAttr("disabled");
                    $("#LEASE_PLACE_CD").removeAttr("disabled");
                    $("#LEASE_USE").removeAttr("disabled");
                    $("#LOC_M_CD").removeAttr("disabled");
                    $("#RET_LEASE_PLACE_CD").removeAttr("disabled");    //2023/03/22 Kawamoto ADD
                    $(this).dialog("close");
                    $('#form').attr("action", "@Url.Content("~/LendingEntry")/Create");
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
        @*showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/LendingEntry")/Create")*@
        return true;
    }

    // 削除ボタンクリック処理
    function btnDelete_onclick() {
        //確認メッセージを出し、ＯＫなら変更処理
        // ダイアログのメッセージを設定
        $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)");

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $("#REQ_EMP_CD").removeAttr("disabled");
                    $("#CHARGE_EMP_CD").removeAttr("disabled");
                    $("#LEASE_PLACE_CD").removeAttr("disabled");
                    $("#LEASE_USE").removeAttr("disabled");
                    $("#LOC_M_CD").removeAttr("disabled");
                    $("#RET_LEASE_PLACE_CD").removeAttr("disabled");    //2023/03/22 Kawamoto ADD
                    $(this).dialog("close");
                    $('#form').attr("action", "@Url.Content("~/LendingEntry")/Delete");
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
    }

    // 履歴ボタンクリック処理
    function btnRireki_onclick() {
        $('#form').attr("action", "@Url.Content("~/LendingEntry")/ShowLendingRireki");
        $('#form').submit();
    }

    // 貸出ボタンクリック処理
    function btnKashidashi_onclick() {

        // td内のtrをループ。
        var text = [];
        var gridData = grid.getData();
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                if (gridData[i]["RESERVE_STATUS"] != "@ReserveStatus.Mi") {
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SELECTED_NOT_STATUS_MI)")
                    return;
                } else {
                    text.push(checkBoxes[i].value);
                }
            }
        }

        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }

        // 隠し項目にセット
        document.getElementById("SelectKeys").value = text;

        //確認メッセージを出し、ＯＫなら貸出処理
        // ダイアログのメッセージを設定
        $("#show-dialog").html("@Message.getMessage(CommonLabel.LENDING_CONFIRM)");

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $("#REQ_EMP_CD").removeAttr("disabled");
                    $("#CHARGE_EMP_CD").removeAttr("disabled");
                    $("#LEASE_PLACE_CD").removeAttr("disabled");
                    $("#LEASE_USE").removeAttr("disabled");
                    $("#LOC_M_CD").removeAttr("disabled");
                    $("#RET_LEASE_PLACE_CD").removeAttr("disabled");    //2023/03/22 Kawamoto ADD
                    $(this).dialog("close");
                    $('#form').attr("action", "@Url.Content("~/LendingEntry")/Lending");
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
    }

    // 返却ボタンクリック処理
    function btnHenkyaku_onclick() {
        // td内のtrをループ。
        var text = [];
        var gridData = grid.getData();
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                if (gridData[i]["RESERVE_STATUS"] != "@ReserveStatus.Lending") {
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SELECTED_NOT_STATUS_LENDING)")
                    return;
                } else {
                    text.push(checkBoxes[i].value);
                }
            }
        }

        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }

        // 隠し項目にセット
        document.getElementById("SelectKeys").value = text;

        //確認メッセージを出し、ＯＫなら返却処理
        // ダイアログのメッセージを設定
        $("#show-dialog").html("@Message.getMessage(CommonLabel.RETURNED_CONFIRM)");

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $("#REQ_EMP_CD").removeAttr("disabled");
                    $("#CHARGE_EMP_CD").removeAttr("disabled");
                    $("#LEASE_PLACE_CD").removeAttr("disabled");
                    $("#LEASE_USE").removeAttr("disabled");
                    $("#LOC_M_CD").removeAttr("disabled");
                    $("#RET_LEASE_PLACE_CD").removeAttr("disabled");    //2023/03/22 Kawamoto ADD
                    $(this).dialog("close");
                    $('#form').attr("action", "@Url.Content("~/LendingEntry")/Returned");
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
    }


    // 一覧削除ボタンクリック処理
    function btnDeleteIchiran_onclick() {
        //テーブルの選択行を取得する。
        var text = [];
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
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
                    $(this).dialog("close");

                    //選択行のデータを更新し表示する(Ajax非同期通信)
                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/LendingEntry")/DeleteMeisai',
                        cache: false,
                        data: { lstAsset: text },
                        success: function (data, dataType) {
                            this;
                            //データが存在しない場合エラー
                            if (data == null) {
                                return;
                            }

                            // エラーがあった場合
                            if (data.ErrCode != "") {
                                // エラーメッセージ
                                showErrMessage(data.ErrCode);
                                return;
                            }

                            // リストを更新
                            UpdateGridList(data)

                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            this;
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

    // 個別期間延長ボタンクリック処理
    function btnKikanEnchou_onclick() {

        //テーブルの選択行を取得する。
        var text = [];
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }

        // 日付NULLチェック
        var FromDt = document.getElementById("RealStartDtString").value
        if (!FromDt) {
            // エラーメッセージ
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_NOT_ENTERED_DATE_FROM)")
            return;
        }

        //選択行のデータエラーチェック(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingEntry")/KobetsuKikanEnchoCheck',
            cache: false,
            data: { lstAsset: text },
            success: function (data, dataType) {
                this;
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // エラーがあった場合
                if (data.ErrCode != "") {
                    // エラーメッセージ
                    showErrMessage(data.ErrCode);
                    return;
                } else {

                    //画面を初期化する
                    document.getElementById("EndDtString").value = "";

                    //モーダルウインドウを開く
                    ShowKikanEnchoModalWindow();
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });

    }
    // 個別期間延長  変更ボタンクリック処理
    function btnKikanEnchoEdit_onclick() {
        var FromDt = document.getElementById("RealStartDtString").value
        var ToDt = document.getElementById("EndDtString").value;
        var AuthProcFlg = document.getElementById("AuthProcFlg").value; // '2021/12/01 nv miyaji add
        var LeaseUse = document.getElementById("LEASE_USE").value; // '2021/12/07 nv miyaji add
        var MngLocLCd = document.getElementById("MNG_LOC_L_CD").value; // '2021/12/16 nv miyaji add

        // 日付NULLチェック
        if (!ToDt) {
            // エラーメッセージ
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_NOT_ENTERED_DATE_TO)")

            //モーダルウインドウを開く
            ShowKikanEnchoModalWindow();
            return;
        }

        //テーブルの選択行を取得する。
        var text = [];
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }

        //選択行のデータを更新し表示する(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingEntry")/SetKobetsuRealEndDt',
            cache: false,
            data: { sFromDt: FromDt, sToDt: ToDt, lstAsset: text, sAuthProcFlg: AuthProcFlg, sLeaseUse: LeaseUse, sMngLocLCd: MngLocLCd },
            success: function (data, dataType) {
                this;
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // エラーがあった場合
                if (data.ErrCode != "") {
                    // エラーメッセージ
                    // ダイアログのメッセージを設定
                    $("#show-message").html(data.ErrCode);
                    // ダイアログを作成
                    $("#show-message").dialog({
                        modal: true,
                        title: strErrMessageTitle,
                        dialogClass: "show-message-wrap",
                        buttons: {
                            "OK": function () {
                                $(this).dialog("close");

                                //モーダルウインドウを開く
                                ShowKikanEnchoModalWindow();

                                return;
                            }
                        }
                    });

                    return;
                }

                // リストを更新
                UpdateGridList(data)

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    // 直接引継ボタンクリック処理
    function btnHikitsugi_onclick() {
        var RealEndDt = document.getElementById("RealEndDtString").value

        // 日付NULLチェック
        if (!RealEndDt) {
            // エラーメッセージ
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_REAL_END_DT_EMPTY)")

            return;
        }

        //テーブルの選択行を取得する。
        var text = [];
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }

        var ReserveId = document.getElementById("RESERVE_ID").value

        //選択行のデータのエラーチェック(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingEntry")/HikitsugiCheck',
            cache: false,
            data: { lReserveId: ReserveId, lstAsset: text },
            success: function (data, dataType) {
                this;
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // エラーがあった場合
                if (data.ErrCode != "") {
                    // エラーメッセージ
                    showErrMessage(data.ErrCode);
                    return;
                } else {
                    //画面を初期化する
                    document.getElementById("HikiDtString").value = "";
                    document.getElementById("HikiEmployeeCd").value = data.HikiEmployeeCd;
                    document.getElementById("HikiEmployeeNm").value = data.HikiEmployeeNm;

                    //モーダルウインドウを開く
                    ShowHikitsugiModalWindow();
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });

    }

    // 直接引継  変更ボタンクリック処理
    function btnHikitsugiEdit_onclick() {
        var HikiDt = document.getElementById("HikiDtString").value
        var EmployeeCd = document.getElementById("HikiEmployeeCd").value
        var ReserveId = document.getElementById("RESERVE_ID").value
        var RealStartDt = document.getElementById("RealStartDtString").value

        // 日付NULLチェック
        if (!HikiDt) {
            // エラーメッセージ
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_REQ_WATASHI_DAY)")

            //モーダルウインドウを開く
            ShowHikitsugiModalWindow();
            return;
        }

        // 実貸出開始日が引継日と同じ場合、エラー
        if (HikiDt == RealStartDt) {
            // エラーメッセージ
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SAME_WATASHI_DAY)")

            //モーダルウインドウを開く
            ShowHikitsugiModalWindow();
            return;
        }

        // 担当者NULLチェック
        if (!EmployeeCd) {
            // エラーメッセージ
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_REQ_TANTO)")

            //モーダルウインドウを開く
            ShowHikitsugiModalWindow();
            return;
        }


        //テーブルの選択行を取得する。
        var text = [];
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingEntry")/SetHikitsugi',
            cache: false,
            data: { lReserveId: ReserveId, sHikiDt: HikiDt, sEmployeeCd: EmployeeCd, lstAsset: text },
            success: function (data, dataType) {
                this;

                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // エラーがあった場合
                if (data.ErrCode != "") {

                    // エラーメッセージ
                    // ダイアログのメッセージを設定
                    $("#show-message").html(data.ErrCode);
                    // ダイアログを作成
                    $("#show-message").dialog({
                        modal: true,
                        title: strErrMessageTitle,
                        dialogClass: "show-message-wrap",
                        buttons: {
                            "OK": function () {
                                $(this).dialog("close");

                                //モーダルウインドウを開く
                                ShowHikitsugiModalWindow();

                                return;
                            }
                        }
                    });

                } else {
                    // リストを更新
                    UpdateGridList(data)
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });

    }

    // 資産追加ボタンクリック処理
    function btnAddAsset_onclick() {
        var AssetNo = document.getElementById("AddAssetNo").value
        var FromDt = document.getElementById("RealStartDtString").value
        var ToDt = document.getElementById("RealEndDtString").value
        var ReserveId = document.getElementById("RESERVE_ID").value
        var MngLocLCd = document.getElementById("MNG_LOC_L_CD").value

        // 資産番号が入力されていない場合、エラーメッセージ
        if (!AssetNo) {
            // エラーメッセージ
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_REQ_ADD_ASSET_NO)")
            return;
        }
        //選択行のデータを更新し表示する(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingEntry")/AddMeisai',
            cache: false,
            data: { lReserveId: ReserveId, sAddAssetNo: AssetNo, sFromDt: FromDt, sToDt: ToDt, sMngLocLCd: MngLocLCd },
            success: function (data, dataType) {
                this;
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // エラーがあった場合
                if (data.ErrCode != "") {
                    // エラーメッセージ
                    showErrMessage(data.ErrCode);
                    return;
                }

                // リストを更新
                UpdateGridList(data)

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });

    }

    // 実貸出開始日　変更時処理
    function RealStartDt_onchange() {
        var FromDt = document.getElementById("RealStartDtString").value
        var ToDt = document.getElementById("RealEndDtString").value
        var ReserveId = document.getElementById("RESERVE_ID").value
        var UserToDt = document.getElementById("UserEndDtString").value;

        // ユーザー貸出開始日を更新
        document.getElementById("UserStartDtString").value = FromDt

        //選択行のデータを更新し表示する(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingEntry")/SetRealStartDt',
            cache: false,
            data: { lReserveId: ReserveId, sFromDt: FromDt, sToDt: ToDt, sUserToDt: UserToDt },
            success: function (data, dataType) {
                this;
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                document.getElementById("WarnMainteKbn").value = data.WarnMainteKbn

                // エラーがあった場合
                if (data.ErrCode != "") {
                    // エラーメッセージ
                    showErrMessage(data.ErrCode);
                    return;
                }

                // ユーザー貸出終了日を更新
                if (data.UserEndDtString) {
                    document.getElementById("UserEndDtString").value = data.UserEndDtString
                }

                // リストを更新
                UpdateGridList(data)

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    // 実貸出終了日　変更時処理
    function RealEndDt_onchange() {
        var FromDt = document.getElementById("RealStartDtString").value;
        var ToDt = document.getElementById("RealEndDtString").value;
        var ReserveId = document.getElementById("RESERVE_ID").value
        var AuthProcFlg = document.getElementById("AuthProcFlg").value; // '2021/12/01 nv miyaji add

        //選択行のデータを更新し表示する(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingEntry")/SetRealEndDt',
            cache: false,
            data: { sFromDt: FromDt, sToDt: ToDt, sAuthProcFlg: AuthProcFlg },
            success: function (data, dataType) {
                this;
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // ユーザー貸出終了日を更新
                document.getElementById("UserEndDtString").value = data.UserEndDtString

                document.getElementById("WarnMainteKbn").value = data.WarnMainteKbn

                // エラーがあった場合
                if (data.ErrCode != "") {
                    // 変更前の値をセットする。
                    document.getElementById("RealEndDtString").value = data.RealEndDtString
                    // エラーメッセージ
                    showErrMessage(data.ErrCode);

                    // 期間延長最大回数のエラー、設定外のエラー以外の場合、グリッドを再表示しない。
                    @*if (data.ErrCode != "@Message.getMessage(CommonLabel.MSG_ERR_KIKAN_ENCHO_OVER)") {*@  // 2022/06/16 nv miyaji
                    if (data.ErrCode != "@Message.getMessage(CommonLabel.MSG_ERR_KIKAN_ENCHO_OVER)" && data.ErrCode != "@Message.getMessage(CommonLabel.MSG_ERR_DATE_SETTEIGAI)") {  // 2022/06/16 nv miyaji
                        return;
                    }
                }

                // リストを更新
                UpdateGridList(data)

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    // 送り先(貸出先)コンボボックス変更時処理
    function LeasePlace_onchange() {
        var LeasePlaceCd = document.getElementById("LEASE_PLACE_CD").value

        // 空の場合
        if (!LeasePlaceCd) {
            // 貸出先情報をクリア
            document.getElementById("DEST_NAME1").value = "";
            document.getElementById("DEST_NAME2").value = "";
            document.getElementById("DEST_ADDRESS1").value = "";
            document.getElementById("DEST_ADDRESS2").value = "";
            document.getElementById("DEST_ADDRESS3").value = "";
            document.getElementById("DEST_ZIP_CODE").value = "";
            document.getElementById("DEST_PHONE_NUMBER").value = "";
            return;
        }

        //選択行のデータを更新し表示する(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingEntry")/GetLeasePlaceInfo',
            cache: false,
            data: { sLeasePlaseCd: LeasePlaceCd },
            success: function (data, dataType) {
                this;
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // エラーがあった場合
                if (data.ErrCode != "") {
                    // エラーメッセージ
                    showErrMessage(data.ErrCode);
                    return;
                }

                // 貸出先情報をセット
                document.getElementById("DEST_NAME1").value = data.DEST_NAME1;
                document.getElementById("DEST_NAME2").value = data.DEST_NAME2;
                document.getElementById("DEST_ADDRESS1").value = data.DEST_ADDRESS1;
                document.getElementById("DEST_ADDRESS2").value = data.DEST_ADDRESS2;
                document.getElementById("DEST_ADDRESS3").value = data.DEST_ADDRESS3;
                document.getElementById("DEST_ZIP_CODE").value = data.DEST_ZIP_CODE;
                document.getElementById("DEST_PHONE_NUMBER").value = data.DEST_PHONE_NUMBER;
                document.getElementById("TO_DEST_NAME").value = data.TO_DEST_NAME;              //2023/03/22 Kawamoto ADD

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //2023/03/22 Kawamoto ADD START
    // 返送先コンボボックス変更時処理
    function RetLeasePlace_onchange() {
        var RetLeasePlaceCd = document.getElementById("RET_LEASE_PLACE_CD").value

        // 空の場合
        if (!RetLeasePlaceCd) {
            // 返送先情報をクリア
            document.getElementById("RET_ZIP_CODE").value = "";
            document.getElementById("RET_ADDRESS1").value = "";
            document.getElementById("RET_ADDRESS2").value = "";
            document.getElementById("RET_ADDRESS3").value = "";
            return;
        }

        //選択行のデータを更新し表示する(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingEntry")/GetLeasePlaceInfo',
            cache: false,
            data: { sLeasePlaseCd: RetLeasePlaceCd },
            success: function (data, dataType) {
                this;
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                // エラーがあった場合
                if (data.ErrCode != "") {
                    // エラーメッセージ
                    showErrMessage(data.ErrCode);
                    return;
                }

                // 返送先情報をセット
                document.getElementById("RET_ZIP_CODE").value = data.DEST_ZIP_CODE;
                document.getElementById("RET_ADDRESS1").value = data.DEST_ADDRESS1;
                document.getElementById("RET_ADDRESS2").value = data.DEST_ADDRESS2;
                document.getElementById("RET_ADDRESS3").value = data.DEST_ADDRESS3;

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }
    //2023/03/22 Kawamoto ADD END

    // リストを更新
    function UpdateGridList(data) {

        CreateGrid(data.ReserveDetailData)

        // 明細数の表示変更
        document.getElementById("MeisaiSuu").innerHTML = "@Message.getMessage(CommonLabel.LABEL_KENSUU)" + data.ReserveDetailData.length + "@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)";
    }

    // テキストエリアを改行表示にする処理    '2022/02/25 nv miyaji add
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
</script>

<div id="lending-entry">
    @Using (Html.BeginForm("Index", "LendingEntry", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
    @Html.AntiForgeryToken()
    @<div class="view-area mb20">
        <div id="view-area-button-parent">
            <div class="view-area-button-left">
                @If Model.FirstOrDefault.SenimotoKbn = SenimotoKbn.LendingList Then
                    @If ViewBag.UpdateBtnEnable = True Then
                        @<input Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">
                    End If
                Else
                    @If Model.FirstOrDefault.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK Then
                    @If Model.FirstOrDefault.AuthProcFlg <> AuthProcFlg.ShokaiNomi Then
                    @<input Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">
                    @If ViewBag.CantEdit = False Then
                        @<input Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="btnDelete_onclick()">
                    End If
                    @<div style="display:inline-block;width:200px;"></div>
                    End If
                    @<input Class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RIREKI)" onclick="btnRireki_onclick()">
                    Else
                    @<input Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_TOUROKU)" onclick="btnCreate_onclick()">
                    End If
                End If
            </div>
            <div class="view-area-button-right">
                @If Model.FirstOrDefault.SenimotoKbn = SenimotoKbn.LendingList Then
                    @<input Class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "LendingList")'">
                Else
                    @<input Class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "LendingStatus")'">
                End If
                @*<input class="defButton return-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="btnReturn_onclick()">*@
            </div>
        </div>
    </div>
    @<div Class="search-wrap">
        <div class="search-border">
            <Table Class="detail-table">
                <tr Class="detail-row">
                    <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER)</td>
                    @*2022/7/25 sv sato upd start*@
                    @*<td Class="condition-col require" style="width:20px;"></td>*@
                    @If ViewBag.CanDspOptionData1 Then
                        @<td Class="condition-col require" style="width:20px;">※</td>
                    Else
                        @<td Class="condition-col require" style="width:20px;"></td>
                    End If
                    @*2022/7/25 sv sato upd end*@
                    <td Class="condition-col" style="width:420px;">
                        @*2022/7/25 sv sato upd start*@
                        @*@Html.TextBoxFor(Function(model) model.FirstOrDefault.ReserveIdString, New With {.htmlAttributes = New With {.style = "width: 190px;"}})*@
                        @Html.EditorFor(Function(model) model.FirstOrDefault.ReserveIdString, New With {.htmlAttributes = New With {.style = "width: 190px;", .maxlength = 18}})
                        @*2022/7/25 sv sato upd end*@
                    </td>
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_MNG_LOC)</td>
                    <td class="condition-col" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.LocLName, New With {.htmlAttributes = New With {.style = "width: 190px;"}})
                    </td>
                </tr>
                <tr Class="detail-row">
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REQ_EMP)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">@Html.DropDownListFor(Function(model) model.FirstOrDefault.REQ_EMP_CD, DirectCast(ViewBag.ReqEmployeeList, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 200px;"})</td>
                </tr>
                <tr Class="detail-row">
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_LEASE_KIKAN_JISSHITSU)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.RealStartDtString, New With {.htmlAttributes = New With {.type = "date", .onchange = "RealStartDt_onchange()"}})
                        ～
                        @Html.EditorFor(Function(model) model.FirstOrDefault.RealEndDtString, New With {.htmlAttributes = New With {.type = "date", .onchange = "RealEndDt_onchange()"}})
                    </td>
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_LEASE_KIKAN_USER)</td>
                    <td class="condition-col" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.UserStartDtString, New With {.htmlAttributes = New With {.type = "date"}})
                        ～
                        @Html.EditorFor(Function(model) model.FirstOrDefault.UserEndDtString, New With {.htmlAttributes = New With {.type = "date"}})
                    </td>
                </tr>
                <tr Class="detail-row">
                    @*担当者*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_EMP)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">@Html.DropDownListFor(Function(model) model.FirstOrDefault.CHARGE_EMP_CD, DirectCast(ViewBag.ChargeEmployeeList, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 200px;"})</td>
                </tr>
                @* 2023/03/22 Kawamoto UPD START *@
                <tr Class="detail-row">
                    <!--<td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_LEASE_PLACE)</td>-->
                    @*送り先*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_SEND)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">@Html.DropDownListFor(Function(model) model.FirstOrDefault.LEASE_PLACE_CD, DirectCast(ViewBag.LeasePlaceList, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 200px;", .onchange = "LeasePlace_onchange()"})</td>
                </tr>
                <tr Class="detail-row">
                    @*送り先名１*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_SEND_NAME)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.DEST_NAME1, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 16}})
                    </td>
                    @*貸出確認書宛名*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_TO_DEST_NAME)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.TO_DEST_NAME, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 16}})
                    </td>
                </tr>
                <tr Class="detail-row">
                    @*送り先名２*@
                    <td class="condition-col" style="width:230px;"></td>
                    <td class="condition-col require" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.DEST_NAME2, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 16}})
                    </td>
                </tr>
                <tr Class="detail-row">
                    @*貸出用途*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">@Html.DropDownListFor(Function(model) model.FirstOrDefault.LEASE_USE, DirectCast(ViewBag.LeaseUse, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 200px;"})</td>
                </tr>
                <tr Class="detail-row">
                    @*送り先電話番号*@
                    <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_SEND_PHONE_NUMBER)</td>
                    <td class="condition-col require" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.DEST_PHONE_NUMBER, New With {.htmlAttributes = New With {.style = "width: 190px;", .maxlength = 14, .oninput = "editNumberKigou(this)"}})
                    </td>
                    @*返送先*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_RETURN_PLACE)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">@Html.DropDownListFor(Function(model) model.FirstOrDefault.RET_LEASE_PLACE_CD, DirectCast(ViewBag.RetLeasePlaceList, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 200px;", .onchange = "RetLeasePlace_onchange()"})</td>
                </tr>
                <tr Class="detail-row">
                    @*送り先郵便番号*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_SEND_ZIP_CODE)</td>
                    <td class="condition-col" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.DEST_ZIP_CODE, New With {.htmlAttributes = New With {.style = "width: 190px;", .maxlength = 8, .oninput = "editNumberAlphabetHyphen(this)"}})
                    </td>
                    @*返送先郵便番号*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ZIP_CODE)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.RET_ZIP_CODE, New With {.htmlAttributes = New With {.style = "width: 190px;", .maxlength = 8, .oninput = "editNumberAlphabetHyphen(this)"}})
                    </td>
                </tr>
                <tr Class="detail-row">
                    @*送り先住所１*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_SEND_ADDRESS)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.DEST_ADDRESS1, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 16}})
                    </td>
                    @*返送先住所１*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ADDRESS)</td>
                    <td class="condition-col require" style="width:20px;">※</td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.RET_ADDRESS1, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 16}})
                    </td>
                </tr>
                <tr Class="detail-row">
                    @*送り先住所２*@
                    <td class="condition-col" style="width:230px;"></td>
                    <td class="condition-col" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.DEST_ADDRESS2, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 16}})
                    </td>
                    @*返送先住所２*@
                    <td class="condition-col" style="width:230px;"></td>
                    <td class="condition-col" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.RET_ADDRESS2, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 16}})
                    </td>
                </tr>
                <tr Class="detail-row">
                    @*送り先住所３*@
                    <td class="condition-col" style="width:230px;"></td>
                    <td class="condition-col" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.DEST_ADDRESS3, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 16}})
                    </td>
                    @*返送先住所３*@
                    <td class="condition-col" style="width:230px;"></td>
                    <td class="condition-col" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.RET_ADDRESS3, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 16}})
                    </td>
                </tr>
                @* 2023/03/22 Kawamoto UPD END *@
                <tr Class="detail-row" style="height:73px;">
                    @*送り先備考*@
                    <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_SEND_BIKO)</td>
                    <td class="condition-col require" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.TextAreaFor(Function(model) model.FirstOrDefault.REMARK, New With {.style = "width: 400px;resize: none", .rows = "3", .maxlength = 255})
                        @*@Html.EditorFor(Function(model) model.FirstOrDefault.REMARK, New With {.htmlAttributes = New With {.style = "width: 400px;", .rows = "3", .maxlength = 255}})*@
                    </td>
                    @*社内用備考*@
                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_SHANAI_BIKO)</td>
                    <td class="condition-col" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @*@Html.EditorFor(Function(model) model.FirstOrDefault.REMARK_INTERNAL, New With {.htmlAttributes = New With {.style = "width: 400px;", .rows = "3", .maxlength = 255}})*@
                        @Html.TextAreaFor(Function(model) model.FirstOrDefault.REMARK_INTERNAL, New With {.style = "width: 400px;resize: none", .rows = "3", .maxlength = 255})
                    </td>
                </tr>
                <tr Class="detail-row">
                    @*送り状No*@
                    <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_DEST_NO)</td>
                    <td class="condition-col require" style="width:20px;"></td>
                    <td class="condition-col" style="width:420px;">
                        @Html.EditorFor(Function(model) model.FirstOrDefault.DEST_NO, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 100}})
                    </td>
                </tr>
            </Table>
        </div>
    </div>
    @<div class="view-area mb20">
        <div id="view-area-button-parent">
            @If Model.FirstOrDefault.SenimotoKbn = SenimotoKbn.LendingList Then
                @If ViewBag.KashidashiBtnEnable = True Then
                    @<input id="BUTTON_KASHIDASHI" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_KASHIDASHI)" onclick="btnKashidashi_onclick()">
                End If
                @If ViewBag.HenkyakuBtnEnable = True Then
                    @<input id="BUTTON_HENKYAKU" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_HENKYAKU)" onclick="btnHenkyaku_onclick()">
                End If
            Else
                @If Model.FirstOrDefault.AuthProcFlg <> AuthProcFlg.ShokaiNomi Then
                    @<div Class="view-area-button-left">
                         @If ViewBag.CantEdit = False Then
                            @<input Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DELETE_ICHIRAN)" onclick="btnDeleteIchiran_onclick()">
                         End If
                        @If Model.FirstOrDefault.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK Then
                            @<div style="display:inline-block;width:100px;"></div>
                            @<input Class="defButton update-btn" type="button" style="width:200px;" value="@Message.getMessage(CommonLabel.BUTTON_KOBETSU_KIKAN_ENCHOU)" onclick="btnKikanEnchou_onclick()">
                            @<input Class="defButton update-btn" type="button" style="width:200px;" value="@Message.getMessage(CommonLabel.BUTTON_CHOKUSETSU_HIKITSUGI)" onclick="btnHikitsugi_onclick()">
                        End If
                    </div>
                    @<div Class="view-area-button-right">
                         @If ViewBag.CantEdit = False Then
                            @Html.EditorFor(Function(model) model.FirstOrDefault.AddAssetNo, New With {.htmlAttributes = New With {.style = "width: 190px;", .maxlength = 24}})
                            @<input Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_ADD_ASSET)" onclick="btnAddAsset_onclick()">
                         End If
                    </div>
                End If
            End If
        </div>
    </div>
    @*ページャー表示*@
    @*@PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)*@
    @*@Html.Label(Message.getMessage(CommonLabel.LABEL_SELECT_COUNT), New With {.style = "font-size:12px;"})
        @Code
            Dim lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
            Dim CheckedCount As String = "0"
            If Not IsNothing(lstCheckedKey) Then
                CheckedCount = lstCheckedKey.Count.ToString
            End If
            @Html.Label(CheckedCount, New With {.id = "selectcount", .style = "margin-right:15px;font-size:12px;"})
        End Code*@
    @<div class="list-view">
        @If Model.FirstOrDefault.ReserveDetailData Is Nothing Then
    @<div id="MeisaiSuu">@Message.getMessage(CommonLabel.LABEL_KENSUU) 0 @Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)</div>
        Else
    @<div id="MeisaiSuu">@Message.getMessage(CommonLabel.LABEL_KENSUU)@Model.FirstOrDefault.ReserveDetailData.Count@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)</div>
        End If

        <div id="ListGrids" style="height:auto;padding-bottom: 20px;" />
    </div>

    @*引継ぎモーダル画面*@
    @<div id="modal-hikitsugi-content">
        <div class="mb10">
            <div><div style="width:100px;display:inline-block;">@Message.getMessage(CommonLabel.LABEL_UKEWATASHI_DAY)</div><span id="modal-hikitsugi-status">@Html.EditorFor(Function(model) model.FirstOrDefault.HikiDtString, New With {.htmlAttributes = New With {.type = "date"}})</span></div>
            <div style="margin-top:5px;"><div style="width:100px;display:inline-block;">@Message.getMessage(CommonLabel.LABEL_EMP)</div><span id="modal-hikitsugi-status">@Html.EditorFor(Function(model) model.FirstOrDefault.HikiEmployeeNm, New With {.htmlAttributes = New With {.style = "width: 190px;", .maxlength = 24}})</span></div>        
            @*<div style="margin-top:5px;"><div style="width:100px;display:inline-block;">@Message.getMessage(CommonLabel.LABEL_EMP)</div><span id="modal-hikitsugi-status">@Html.DropDownListFor(Function(model) model.FirstOrDefault.HikiEmployeeCd, DirectCast(ViewBag.HikiEmployeeList, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 200px;"})</span></div>*@
        </div>
        <div Class="PopupButton float-left mr10">
            <input id="modal-hikitsugi-edit" Class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnHikitsugiEdit_onclick()">
        </div>
        <div Class="PopupButton float-right">
            <input id="modal-close" Class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CLOSE)">
        </div>
    </div>

    @*個別期間延長モーダル画面*@
    @<div id="modal-kikanencho-content">
        <div class="mb10">
            <div><div style="width:150px;display:inline-block;">@Message.getMessage(CommonLabel.LABEL_LENDING_END_DATE)</div><span id="modal-kikanencho-status">@Html.EditorFor(Function(model) model.FirstOrDefault.EndDtString, New With {.htmlAttributes = New With {.type = "date"}})</span></div>
        </div>
        <div Class="PopupButton float-left mr10">
            <input id="modal-kikanencho-edit" Class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnKikanEnchoEdit_onclick()">
        </div>
        <div Class="PopupButton float-right">
            <input id="modal-close" Class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CLOSE)">
        </div>
    </div>

    'チェックボックス選択値の引き渡し用隠し項目
    @<input id="SelectKeys" name="SelectKeys" type="hidden">

    @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.RESERVE_ID)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.REAL_START_DT)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.MNG_LOC_L_CD)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.REAL_END_DT)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.USER_START_DT)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.USER_END_DT)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.REQUEST_DT)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.LEASE_CHG_TIMES)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.SenimotoKbn)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.AuthProcFlg)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.RESERVE_STATUS)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.isNotEnableRealStartDt)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.HikiEmployeeCd)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.INS_DATETIME)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.INS_EMPLOYEE_CD)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.INS_EMPLOYEE_NAME)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_DATETIME)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_CD)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_NAME)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_TASK)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.WarnMainteKbn)
    End Using

</div>
