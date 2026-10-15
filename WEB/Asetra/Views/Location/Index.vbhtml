@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of LocationViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLocation.TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
End Code
<script>
    //
    //ロケーションマスタメンテ画面
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
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessLocEdit") {
            //資産マスタ削除
            var cd = "@TempData("LocLCd")";
            document.getElementById("EditLocLCd").value = cd;

            // ダイアログのメッセージを設定
            strMessage = "@Message.getMessage(CommonLabelLocation.MSG_ALERT_DEL_ASSET)".replace("@@CR@@", "<BR>");
            $("#show-dialog").html(strMessage);

            // ダイアログを作成
            $("#show-dialog").dialog({
                modal: true,
                title: strConfirmTitle,
                dialogClass: "show-confirm-wrap",
                buttons: {
                    "OK": function () {
                        var befAction = $('#form').attr("action");
                        $(this).dialog("close");
                        $('#form').attr("action", "@Url.Content("~/Location")/DeleteAsset");
                        $('#form').submit();
                        $('#form').attr("action", befAction);
                        return true;
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                        showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)")
                        return false;
                    }
                },
                open: function () {      // キャンセルボタンにフォーカスをあてる
                    $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').focus();
                }
            });

        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessDelete") {
            //削除
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessHakko") {
            //発行
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_HAKKO)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

                if ($('#LOC_DIV_L').prop('checked')) {
                    //--2019/11/21 nv miyaji add start
                    // 大分類選択時、通常表示
                    $("#td-LSentaku").show();
                    $("#td-NotLSentaku").hide();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    //--2019/11/21 nv miyaji add end
                    //中区分：書き込み不可
                    document.getElementById("LOC_M_CD").readOnly = true;
                    $("#LOC_M_CD").addClass("disable");
                    //小区分：書き込み不可
                    document.getElementById("LOC_S_CD").readOnly = true;
                    $("#LOC_S_CD").addClass("disable");

                    //共通ロケーション：使用可 2025/10/14 DEL Kudo
                    // $("#LocShareFlg").prop("disabled", false);
                    //共通ロケーション：使用可 '2025/10/14 ADD Kudo RFIDタグ拠点
                    $("#LocRfidFlg").prop("disabled", false);
                    $("#OwnInvFlg").prop("disabled", false);
                    //メール送信先：書き込み可
                    $("#MAIL_SEND_ADDRESS").attr("readonly", false);
                    $("#MAIL_SEND_ADDRESS").removeClass("disable");
                    //貸出回数：書き込み可
                    $("#MAINTE_LEASE_NUM").attr("readonly", false);
                    $("#MAINTE_LEASE_NUM").removeClass("disable");
                    //貸出日数：書き込み可
                    $("#MAINTE_LEASE_DAY").attr("readonly", false);
                    $("#MAINTE_LEASE_DAY").removeClass("disable");
                    //稼働日考慮(貸出日数)：使用可
                    $("#MainteLeaseDayWorkFlg").prop("disabled", false);
                    //メンテナンス日数：書き込み可
                    $("#MAINTE_DAY").attr("readonly", false);
                    $("#MAINTE_DAY").removeClass("disable");
                    //稼働日考慮(メンテナンス日数)：使用可
                    $("#MainteDayWorkFlg").prop("disabled", false);
                    //2023/02/08 ADD Start Shirakawa
                    //指定数量：書き込み可
                    $("#SPECIFIED_QTY").attr("readonly", false);
                    $("#SPECIFIED_QTY").removeClass("disable");
                    //2023/02/08 ADD End Shirakawa

                } else if ($('#LOC_DIV_M').prop('checked')) {
                    //--2019/11/21 nv miyaji add start
                    // 中分類選択時、大分類をコンボボックス表示にする
                    $("#td-LSentaku").hide();
                    $("#td-NotLSentaku").show();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    //--2019/11/21 nv miyaji add end
                    //中区分：書き込み可
                    document.getElementById("LOC_M_CD").readOnly = false;
                    $("#LOC_M_CD").removeClass("disable");
                    //小区分：書き込み不可
                    document.getElementById("LOC_S_CD").readOnly = true;
                    $("#LOC_S_CD").addClass("disable");
                    //共通ロケーション：使用不可
                   // $("#LocShareFlg").prop("disabled", true);
                    //'2025/10/14 ADD Kudo RFIDタグ拠点
                    $("#LocRfidFlg").prop("disabled", true);
                    $("#OwnInvFlg").prop("disabled", true);
                    //メール送信先：書き込み不可
                    $("#MAIL_SEND_ADDRESS").attr("readonly", true);
                    $("#MAIL_SEND_ADDRESS").addClass("disable");
                    //貸出回数：書き込み不可
                    $("#MAINTE_LEASE_NUM").attr("readonly", true);
                    $("#MAINTE_LEASE_NUM").addClass("disable");
                    //貸出日数：書き込み不可
                    $("#MAINTE_LEASE_DAY").attr("readonly", true);
                    $("#MAINTE_LEASE_DAY").addClass("disable");
                    //稼働日考慮(貸出日数)：使用不可
                    $("#MainteLeaseDayWorkFlg").prop("disabled", true);
                    //メンテナンス日数：書き込み不可
                    $("#MAINTE_DAY").attr("readonly", true);
                    $("#MAINTE_DAY").addClass("disable");
                    //稼働日考慮(メンテナンス日数)：使用不可
                    $("#MainteDayWorkFlg").prop("disabled", true);
                    //2023/02/08 ADD Start Shirakawa
                    //指定数量：書き込み不可
                    $("#SPECIFIED_QTY").attr("readonly", true);
                    $("#SPECIFIED_QTY").addClass("disable");
                    //2023/02/08 ADD End Shirakawa

                } else if ($('#LOC_DIV_S').prop('checked')) {
                    //--2019/11/21 nv miyaji add start
                    // 小分類選択時、大分類・中分類をコンボボックス表示にする
                    $("#td-LSentaku").hide();
                    $("#td-NotLSentaku").show();
                    $("#td-SSentaku").show();
                    $("#td-NotSSentaku").hide();
                    //--2019/11/21 nv miyaji add end
                    //中区分：書き込み可
                    document.getElementById("LOC_M_CD").readOnly = false;
                    $("#LOC_M_CD").removeClass("disable");
                    //小区分：書き込み可
                    document.getElementById("LOC_S_CD").readOnly = false;
                    $("#LOC_S_CD").removeClass("disable");
                    //共通ロケーション：使用不可
                    //$("#LocShareFlg").prop("disabled", true);
                    //2025/10/14 ADD Kudo RFIDタグ拠点
                    $("#LocRfidFlg").prop("disabled", true);
                    $("#OwnInvFlg").prop("disabled", true);
                    //メール送信先：書き込み不可
                    $("#MAIL_SEND_ADDRESS").attr("readonly", true);
                    $("#MAIL_SEND_ADDRESS").addClass("disable");
                    //貸出回数：書き込み不可
                    $("#MAINTE_LEASE_NUM").attr("readonly", true);
                    $("#MAINTE_LEASE_NUM").addClass("disable");
                    //貸出日数：書き込み不可
                    $("#MAINTE_LEASE_DAY").attr("readonly", true);
                    $("#MAINTE_LEASE_DAY").addClass("disable");
                    //稼働日考慮(貸出日数)：使用不可
                    $("#MainteLeaseDayWorkFlg").prop("disabled", true);
                    //メンテナンス日数：書き込み不可
                    $("#MAINTE_DAY").attr("readonly", true);
                    $("#MAINTE_DAY").addClass("disable");
                    //稼働日考慮(メンテナンス日数)：使用不可
                    $("#MainteDayWorkFlg").prop("disabled", true);
                    //2023/02/08 ADD Start Shirakawa
                    //指定数量：書き込み不可
                    $("#SPECIFIED_QTY").attr("readonly", true);
                    $("#SPECIFIED_QTY").addClass("disable");
                    //2023/02/08 ADD End Shirakawa
                }
                // 設定M値による中区分の非活性
                if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes") {
                    $("#LOC_M_CD").attr("disabled", "disabled");
                    $("#LOC_M_CD").addClass("disable");
                }
                // 設定M値による小区分の非活性
                if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes") {
                    $("#LOC_S_CD").attr("disabled", "disabled");
                    $("#LOC_S_CD").addClass("disable");
                }

                //削除ボタンを非表示
                $("#modal-delete-btn").css("display", "none");

                //登録画面を表示
                ShowModalWindow()
                //$('.create-btn').trigger("click")
            } else {
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

                // ロケ区分選択値による非活性の設定
                if ($('#LOC_DIV_L').prop('checked')) {
                    //--2019/11/21 nv miyaji add start
                    // 大分類選択時、通常表示
                    $("#td-LSentaku").show();
                    $("#td-NotLSentaku").hide();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    //--2019/11/21 nv miyaji add end
                    $('#LOC_DIV_L').prop('disabled', false);
                    $('#LOC_DIV_M').prop('disabled', true);
                    $('#LOC_DIV_S').prop('disabled', true);
                    //共通ロケーション：使用可
                   // $("#LocShareFlg").prop("disabled", false);
                    //2025/10/14 ADD Kudo RFIDタグ拠点
                    $("#LocRfidFlg").prop("disabled", false);
                    $("#OwnInvFlg").prop("disabled", false);
                    //メール送信先：書き込み不可
                    $("#MAIL_SEND_ADDRESS").attr("readonly", true);
                    $("#MAIL_SEND_ADDRESS").addClass("disable");
                    //貸出回数：書き込み可
                    $("#MAINTE_LEASE_NUM").attr("readonly", false);
                    $("#MAINTE_LEASE_NUM").removeClass("disable");
                    //貸出日数：書き込み可
                    $("#MAINTE_LEASE_DAY").attr("readonly", false);
                    $("#MAINTE_LEASE_DAY").removeClass("disable");
                    //稼働日考慮(貸出日数)：使用可
                    $("#MainteLeaseDayWorkFlg").prop("disabled", false);
                    //メンテナンス日数：書き込み可
                    $("#MAINTE_DAY").attr("readonly", false);
                    $("#MAINTE_DAY").removeClass("disable");
                    //稼働日考慮(メンテナンス日数)：使用可
                    $("#MainteDayWorkFlg").prop("disabled", false);
                    //2023/02/08 ADD Start Shirakawa
                    //指定数量：書き込み可
                    $("#SPECIFIED_QTY").attr("readonly", false);
                    $("#SPECIFIED_QTY").removeClass("disable");
                    //2023/02/08 ADD End Shirakawa

                } else if ($('#LOC_DIV_M').prop('checked')) {
                    // Upd Start 2020.03.09 Iwamoto
                    ////--2019/11/21 nv miyaji add start
                    //// 中分類選択時、大分類をコンボボックス表示にする
                    //$("#td-LSentaku").hide();
                    //$("#td-NotLSentaku").show();
                    //$("#td-SSentaku").hide();
                    //$("#td-NotSSentaku").show();
                    ////--2019/11/21 nv miyaji add end
                    // 各区分コンボボックスを非表示にする
                    $("#td-LSentaku").show();
                    $("#td-NotLSentaku").hide();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    // Upd End 2020.03.09 Iwamoto
                    $('#LOC_DIV_L').prop('disabled', true);
                    $('#LOC_DIV_M').prop('disabled', false);
                    $('#LOC_DIV_S').prop('disabled', true);
                    //共通ロケーション：使用不可
                    //$("#LocShareFlg").prop("disabled", true);
                    //2025/10/14 ADD Kudo RFIDタグ拠点
                    $("#LocRfidFlg").prop("disabled", true);
                    $("#OwnInvFlg").prop("disabled", true);
                    //メール送信先：書き込み不可
                    $("#MAIL_SEND_ADDRESS").attr("readonly", true);
                    $("#MAIL_SEND_ADDRESS").addClass("disable");
                    //貸出回数：書き込み不可
                    $("#MAINTE_LEASE_NUM").attr("readonly", true);
                    $("#MAINTE_LEASE_NUM").addClass("disable");
                    //貸出日数：書き込み不可
                    $("#MAINTE_LEASE_DAY").attr("readonly", true);
                    $("#MAINTE_LEASE_DAY").addClass("disable");
                    //稼働日考慮(貸出日数)：使用不可
                    $("#MainteLeaseDayWorkFlg").prop("disabled", true);
                    //メンテナンス日数：書き込み不可
                    $("#MAINTE_DAY").attr("readonly", true);
                    $("#MAINTE_DAY").addClass("disable");
                    //稼働日考慮(メンテナンス日数)：使用不可
                    $("#MainteDayWorkFlg").prop("disabled", true);
                    //2023/02/08 ADD Start Shirakawa
                    //指定数量：書き込み不可
                    $("#SPECIFIED_QTY").attr("readonly", true);
                    $("#SPECIFIED_QTY").addClass("disable");
                    //2023/02/08 ADD End Shirakawa

                } else if ($('#LOC_DIV_S').prop('checked')) {
                    // Upd Start 2020.03.09 Iwamoto
                    ////--2019/11/21 nv miyaji add start
                    //// 小分類選択時、大分類・中分類をコンボボックス表示にする
                    //$("#td-LSentaku").hide();
                    //$("#td-NotLSentaku").show();
                    //$("#td-SSentaku").show();
                    //$("#td-NotSSentaku").hide();
                    ////--2019/11/21 nv miyaji add end
                    // 各区分コンボボックスを非表示にする
                    $("#td-LSentaku").show();
                    $("#td-NotLSentaku").hide();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    // Upd End 2020.03.09 Iwamoto
                    $('#LOC_DIV_L').prop('disabled', true);
                    $('#LOC_DIV_M').prop('disabled', true);
                    $('#LOC_DIV_S').prop('disabled', false);
                    //共通ロケーション：使用不可
                    //$("#LocShareFlg").prop("disabled", true);
                    //'2025/10/14 ADD Kudo RFIDタグ拠点
                    $("#LocRfidFlg").prop("disabled", true);
                    $("#OwnInvFlg").prop("disabled", true);
                    //メール送信先：書き込み不可
                    $("#MAIL_SEND_ADDRESS").attr("readonly", true);
                    $("#MAIL_SEND_ADDRESS").addClass("disable");
                    //貸出回数：書き込み不可
                    $("#MAINTE_LEASE_NUM").attr("readonly", true);
                    $("#MAINTE_LEASE_NUM").addClass("disable");
                    //貸出日数：書き込み不可
                    $("#MAINTE_LEASE_DAY").attr("readonly", true);
                    $("#MAINTE_LEASE_DAY").addClass("disable");
                    //稼働日考慮(貸出日数)：使用不可
                    $("#MainteLeaseDayWorkFlg").prop("disabled", true);
                    //メンテナンス日数：書き込み不可
                    $("#MAINTE_DAY").attr("readonly", true);
                    $("#MAINTE_DAY").addClass("disable");
                    //稼働日考慮(メンテナンス日数)：使用不可
                    $("#MainteDayWorkFlg").prop("disabled", true);
                    //2023/02/08 ADD Start Shirakawa
                    //指定数量：書き込み不可
                    $("#SPECIFIED_QTY").attr("readonly", true);
                    $("#SPECIFIED_QTY").addClass("disable");
                    //2023/02/08 ADD End Shirakawa

                }
                //各区分コード書き込み不可
                document.getElementById("LOC_L_CD").readOnly = true;
                $("#LOC_L_CD").addClass("disable");
                document.getElementById("LOC_M_CD").readOnly = true;
                $("#LOC_M_CD").addClass("disable");
                document.getElementById("LOC_S_CD").readOnly = true;
                $("#LOC_S_CD").addClass("disable");

                //削除ボタンを表示
                $("#modal-delete-btn").css("display", "block");

                //変更画面を表示
                ShowModalWindow()
                //$('.update-btn').trigger("click")
            }
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        if("@Model.FirstOrDefault.GridInfo("LOC_DIV").DispFlag" === "1") {
            // 大区分非選択時と設定M値による中区分を非活性  2019/08/14 UPDATE INOUE
            //if (document.getElementById("SearchLocL").value == "") {
            //    document.getElementById("SearchLocM").value = "";
            //    $("#SearchLocM").attr("disabled", "disabled");
            //}
            if("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocL").value == "") {
                document.getElementById("SearchLocM").value = "";
                refreshSelect2("#SearchLocM");
                $("#SearchLocM").attr("disabled", "disabled");
            }

            // 大区分非選択時と設定M値による中区分を非活性  2019/08/14 UPDATE INOUE
            //if (document.getElementById("SearchLocM").value == "") {
            //    document.getElementById("SearchLocS").value = "";
            //    $("#SearchLocS").attr("disabled", "disabled");
            //}
            if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocM").value == "") {
                document.getElementById("SearchLocS").value = "";
                refreshSelect2("#SearchLocS");
                $("#SearchLocS").attr("disabled", "disabled");
            }
        }

        // 設定M値による表示指定非活性
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes") {
            $("#SearchDispSelect_LM").attr("disabled", "disabled");
            $("#SearchDispSelect_LMS").attr("disabled", "disabled");
            $("#SearchDispSelect_M").attr("disabled", "disabled");
        }
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes") {
            $("#SearchDispSelect_LMS").attr("disabled", "disabled");
            $("#SearchDispSelect_S").attr("disabled", "disabled");
        }

        btnCsvImportEnabled(true)       // 2022/7/25 sv sato add

        // 列定義
        let columns = [
            { id: "@ColKey.Location.CHECK_SELECT", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_SELECT)</div>', field: "DISP_FLAG", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.CHECK_SELECT", 40), sortOrder: -2, formatter: LocationCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.Location.LOC_DIV", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_LOC_DIV_NAME)</div>', field: "LOC_DIV", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.LOC_DIV", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_DIV").DispFlag", formatter: LocDivFormatter},
            { id: "@ColKey.Location.LOC_CD", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_LOC_CD)</div>', field: "LOC_CD", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.LOC_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_CD").DispFlag", formatter: LocCdFormatter},
            { id: "@ColKey.Location.LOC_NAME", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_LOC_NAME)</div>', field: "LOC_NAME", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.LOC_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_NAME").DispFlag", formatter: LocNameFormatter},
            { id: "@ColKey.Location.LOC_RFID_FLG", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_LOC_RFID_FLG)</div>', field: "LOC_RFID_FLG", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.LOC_RFID_FLG", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_RFID_FLG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_RFID_FLG").DispFlag", formatter: LocRfidFlgFomatter },
            { id: "@ColKey.Location.OWN_INV_FLG", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabelLocation.LABEL_OWN_INV_FLG)</div>', field: "OWN_INV_FLG", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.OWN_INV_FLG", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("OWN_INV_FLG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("OWN_INV_FLG").DispFlag", formatter: OwnInvFlgFomatter },
            { id: "@ColKey.Location.MAINTE_LEASE_NUM", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_NUM)</div>', field: "MAINTE_LEASE_NUM", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.MAINTE_LEASE_NUM", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("MAINTE_LEASE_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAINTE_LEASE_NUM").DispFlag", formatter: textRightAlignFormatter },
            { id: "@ColKey.Location.MAINTE_LEASE_DAY", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY)</div>', field: "MAINTE_LEASE_DAY", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.MAINTE_LEASE_DAY", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("MAINTE_LEASE_DAY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAINTE_LEASE_DAY").DispFlag", formatter: MainteLeaseDayFomatter},
            { id: "@ColKey.Location.MAINTE_DAY", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)</div>', field: "MAINTE_DAY", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.MAINTE_DAY", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("MAINTE_DAY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAINTE_DAY").DispFlag", formatter: MainteDayFomatter},
            { id: "@ColKey.Location.SPECIFIED_QTY", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_SPECIFIED_QTY)</div>', field: "SPECIFIED_QTY", width: getColumnWidth("@DispId.LOCATION", "@ColKey.Location.SPECIFIED_QTY", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SPECIFIED_QTY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SPECIFIED_QTY").DispFlag", formatter: SpecifiedQtyFormatter},
        ];

        // Gridのオプション機能の設定
        //let options = {
        //    enableCellNavigation: true,             //グリッド内のセル間移動可否
        //    enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
        //    multiColumnSort: false,                 //複数列のソート可否
        //    autoHeight: true,                       //Gridの高さの自動調整可否
        //    editable: false,                        //セルの編集可否
        //    autoCommitEdit: false,                  //セル編集の自動コミット可否
        //};
        let options = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: false,                       //Gridの高さの自動調整可否
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列を加算
        }

        // 選択状態を復元
        let lstCheckedKey = @Html.Raw(jsonCheckKeys);
        for (let i = 0; i < lstCheckedKey.length; i++) {
            // Key1:ロケ区分 Key2:ロケ大区分コード key3:ロケ中区分コード key4:ロケ小区分コード
            var keys = lstCheckedKey[i].split(",");

            for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                if (keys[0] === jsonData[grid_i].LOC_DIV && keys[1] === jsonData[grid_i].LOC_L_CD && keys[2] === jsonData[grid_i].LOC_M_CD && keys[3] === jsonData[grid_i].LOC_S_CD) {
                    jsonData[grid_i].DISP_FLAG = lstCheckedKey[i]
                    break;
                }
            }
        }

        let data = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.LOCATION", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, "@DispId.LOCATION", fixCol);
        }
        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");
         //ローカルストレージから復元
        const saved = localStorage.getItem("LocationSearchConditions");
        if ("@Model.FirstOrDefault.IsRestoreOwnSearchConditions" == "True") {
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
            //ロケ大の値が変わった時のメソッド実行
            ChangeLocationL(lcd, document.getElementById("SearchLocM").value, document.getElementById("SearchLocS").value, true);
            //btnSearch_onclick(this);
            //$('#form').submit();
       }
    });

    //ローカルストレージ保存
    //検索時にロケ大区分の検索条件を保存2025/10/15 Kudo
    function localStorageSave() {
        let cond = { SearchLocL: document.getElementById("SearchLocL").value };

        //ローカルストレージを保存
        localStorage.setItem("LocationSearchConditions", JSON.stringify(cond));
    }

    //テキスト（右寄せ）
    function textRightAlignFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return '<div style="text-align:right; width:100%;">' + value + '</div>';
        }
    }

    //テキスト（中央寄せ）
    function textCenterAlignFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return '<div style="text-align:center; width:100%;">' + value + '</div>';
        }
    }

    // チェックボックス(中央寄せ)
    function LocationCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.LOC_DIV) + ',' + convertToEmptyIfNull(dataContext.LOC_L_CD) + ',' + convertToEmptyIfNull(dataContext.LOC_M_CD) + ',' + convertToEmptyIfNull(dataContext.LOC_S_CD);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheck" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" '+
               (isKeyInclude ? 'checked="checked" ' : '') +
               'onchange="check_onclick(this)" ' +
               '/>' +
               '</div>';
    }

    //ロケ区分用formatter LOC_DIV
    function LocDivFormatter(row, cell, value, columnDef, dataContext) {
        let status = ""
        switch(dataContext.LOC_DIV) {
            case "@LOC_DIV.LocationL":
                status = "@Message.getMessage(CommonLabel.VALUE_LOC_L)"
                break;
            case "@LOC_DIV.LocationM":
                status = "@Message.getMessage(CommonLabel.VALUE_LOC_M)"
                break;
            case "@LOC_DIV.LocationS":
                status = "@Message.getMessage(CommonLabel.VALUE_LOC_S)"
                break;
        }
        return status
    }

    //ロケーションコード用formatter
    function LocCdFormatter(row, cell, value, columnDef, dataContext) {
        let locCd = dataContext.LOC_L_CD.trim()
        let locM = convertToEmptyIfNull(dataContext.LOC_M_CD.trim())
        if (locM != "") {
            locCd = locCd + ' - ' + dataContext.LOC_M_CD.trim()
        }
        let locS = convertToEmptyIfNull(dataContext.LOC_S_CD.trim())
        if (locS != "") {
            locCd = locCd + ' - ' + dataContext.LOC_S_CD.trim()
        }
        return locCd
    }

    //ロケーション名用formatter LOC_NAME
    function LocNameFormatter(row, cell, value, columnDef, dataContext) {
        let locLName = convertToEmptyIfNull(dataContext.LOC_L_NAME).trim()
        let locMName = convertToEmptyIfNull(dataContext.LOC_M_NAME).trim()
        let locSName = convertToEmptyIfNull(dataContext.LOC_S_NAME).trim()
        let locName = locLName
        switch(dataContext.LOC_DIV) {
            case "@LOC_DIV.LocationM":
                if (locName.length != 0) {
                    locName += ' - '
                }
                locName += locMName
                break;
            case "@LOC_DIV.LocationS":
                if (locName.length != 0) {
                    locName += ' - '
                }
                locName += locMName
                if (locName.length != 0) {
                    locName += ' - '
                }
                locName += locSName
                break;
        }
        return locName
    }

    @*//共通/拠点用formatter
    function LocShareFlgFomatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        switch(dataContext.LOC_SHARE_FLG) {
            case "@LocShareFlg.Yes":
                status = '@Message.getMessage(CommonLabel.VALUE_LOC_SHARE_FLG_Y)'
                break;
            case "@LocShareFlg.No":
                status = '@Message.getMessage(CommonLabel.VALUE_LOC_SHARE_FLG_N)'
                break;
        }
        return '<div style="text-align:center; width:100%;">' + status + '</div>'
    }*@

        //'2025/10/14 ADD Kudo RFIDタグ
    function LocRfidFlgFomatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        switch (dataContext.LOC_RFID_FLG) {
            case "@LocRfidFlg.Yes":
                status = '@Message.getMessage(CommonLabel.VALUE_LOC_RFID_FLG)'
                break;
        }
        return '<div style="text-align:center; width:100%;">' + status + '</div>'
    }


    function OwnInvFlgFomatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        switch (dataContext.OWN_INV_FLG) {
            case "@OwnInvFlg.Yes":
                status = '@Message.getMessage(CommonLabelLocation.LABEL_OWN_INV_ONLY)'
                break;
        }
        return '<div style="text-align:center; width:100%;">' + status + '</div>'
    }
    //メンテ発生貸出日数formatter MAINTE_LEASE_DAY
    function MainteLeaseDayFomatter(row, cell, value, columnDef, dataContext) {
        let mainteLeaseDay = ""
        let mainteLeaseDayWorkFlag = convertToEmptyIfNull(dataContext.MAINTE_LEASE_DAY_WORK_FLAG)
        if (mainteLeaseDayWorkFlag != "" && value != null) {
            mainteLeaseDay = String(value) + "@Message.getMessage(CommonLabel.LABEL_DAY)"
            if (dataContext.MAINTE_LEASE_DAY_WORK_FLAG == "@WorkDayFlg.Yes") {
                mainteLeaseDay = mainteLeaseDay + "(" + "@Message.getMessage(CommonLabel.LABEL_WORK_DAYS)" + ")"
            }
        }
        return '<div style="text-align:center; width:100%;">' + mainteLeaseDay + '</div>'
    }

    //メンテ日数用formatter MAINTE_DAY
    function MainteDayFomatter(row, cell, value, columnDef, dataContext) {
        let mainteDay = ""
        let mainteDayWorkFlag = convertToEmptyIfNull(dataContext.MAINTE_DAY_WORK_FLAG)
        if (mainteDayWorkFlag != "" && value != null) {
            mainteDay = String(value) + "@Message.getMessage(CommonLabel.LABEL_DAY)"
            if (dataContext.MAINTE_DAY_WORK_FLAG == "@WorkDayFlg.Yes") {
                mainteDay = mainteDay + "(" + "@Message.getMessage(CommonLabel.LABEL_WORK_DAYS)" + ")"
            }
        }
        return '<div style="text-align:center; width:100%;">' + mainteDay + '</div>'
    }

    //数量（備考）用formatter SPECIFIED_QTY
    function SpecifiedQtyFormatter(row, cell, value, columnDef, dataContext) {
        let specifiedQty = ''
        if (dataContext.SPECIFIED_QTY != null) {
            specifiedQty = dataContext.SPECIFIED_QTY.toFixed(3)
        }
        return '<div style="text-align:right; width:100%;">' + specifiedQty + '</div>';
    }

    //CSVデータ取込画面　取込開始ボタンクリック処理
    function btnCsvImportStart_onclick() {
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
            url: '@Url.Content("~/Location")/ImportCsvFile',
            data: formData,
            dataType: 'json',
            processData: false,
            contentType: false,
            cache: false,
            success: function (data, dataType) {
                this;

                switch (data[0].ResultCd) {
                    case @DataInport.DataInportResultCd.OK:
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTOK)";
                        break;
                    case @DataInport.DataInportResultCd.NoSelect:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SELECT)";
                        break;
                    case @DataInport.DataInportResultCd.Empty:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_EMPTY)";
                        break;
                    case @DataInport.DataInportResultCd.NotCsv:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_NOTCSV)";
                        break;
                    case @DataInport.DataInportResultCd.KoumokuSuuErr:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU)";
                        break;
                    case @DataInport.DataInportResultCd.ImportKengenErr:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_ADDKENGEN)";
                        break;
                    case @DataInport.DataInportResultCd.DataNG:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTNG)";

                        for (var icnt = 0; icnt < data.length; icnt++) {
                            if( icnt < "@DataInport.DISP_ROW_COUNT"){
                                //icnt行目の各項目に値設定
                                var dispErrRowIdx = "";
                                if (data[icnt].ErrRowIdx != null) {
                                    dispErrRowIdx = data[icnt].ErrRowIdx;
                                }
                                //tblTbody.rows[icnt].cells[0].innerHTML =data[icnt].ErrRowIdx;
                                tblTbody.rows[icnt].cells[0].innerHTML =dispErrRowIdx;
                                tblTbody.rows[icnt].cells[0].title = dispErrRowIdx;
                                tblTbody.rows[icnt].cells[1].innerHTML =data[icnt].ErrKoumoku;
                                tblTbody.rows[icnt].cells[1].title = data[icnt].ErrKoumoku;
                                tblTbody.rows[icnt].cells[2].innerHTML =data[icnt].ErrGaiyou;
                                tblTbody.rows[icnt].cells[2].title =data[icnt].ErrGaiyou;
                            }else{
                                var dispErrRowIdx = "";
                                if (data[icnt].ErrRowIdx != null) {
                                    dispErrRowIdx = data[icnt].ErrRowIdx;
                                }
                                //$("#csv-err-ichiran").append('<tr><td class="col1">'+ data[icnt].ErrRowIdx +'</td><td class="col2" >'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" >'+ data[icnt].ErrGaiyou +'</td></tr>');
                                //$("#csv-err-ichiran").append('<tr><td class="col1">'+ dispErrRowIdx +'</td><td class="col2" >'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" >'+ data[icnt].ErrGaiyou +'</td></tr>');
                                $("#csv-err-ichiran").append('<tr><td class="col1" title=' + dispErrRowIdx + '>'+ dispErrRowIdx +'</td><td class="col2" title=' + data[icnt].ErrKoumoku + '>'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" title=' + data[icnt].ErrGaiyou + '>'+ data[icnt].ErrGaiyou +'</td></tr>');
                            }

                        }

                        break;
                    case @DataInport.DataInportResultCd.LoginSession:
                        // ログインセッション切れの場合
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_LOGININFO_EMPTY)";
                        break;
                }
                // 2022/7/25 sv sato add start
                if (data[0].ResultCd != "@DataInport.DataInportResultCd.OK" &&
                    data[0].ResultCd != "@DataInport.DataInportResultCd.NoSelect") {
                    //正常終了以外の場合、CSVデータ取込ボタンを非活性にする
                    btnCsvImportEnabled(false)
                }
                // 2022/7/25 sv sato add end
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
                document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SOUTEIGAI)";
                btnCsvImportEnabled(false)           // ADD sv sato 2022/7/25
            }
        });
    }

    //2022/7/25 sv sato add start
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
    //2022/7/25 sv sato add end

    //CSVデータ取込ボタンクリック処理
    function btnCsvImport_onclick() {
        document.getElementById("popup-title-csv").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)";
        document.getElementById("modal-import-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)";
        document.getElementById("CsvFile").value = "";          //ファイル選択をクリア
        document.getElementById("modal-msg").innerHTML = "";    //メッセージをクリア
        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Nomal";   //メッセージの背景色を白にする

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

        //CSV取込画面を表示
        ShowCsvModalWindow()
    }

    //全データCSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.CSVEXPORT_ALL_CONFIRM)", "@Url.Content("~/Location")/CsvExport");
    }

    //ロケーションラベル発行
    function btnLocLabel_onclick() {
        document.getElementById("popup-title-prt").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SELECT_PRINTER)";

        //--2020/02/12 nv miyaji update start
        //var keys = [];
        //var grid = document.getElementById("location-detail");
        //var checkBoxes = grid.getElementsByTagName("INPUT");
        //for (var i = 0; i < checkBoxes.length; i++) {
        //    if (checkBoxes[i].checked) {
        //        var row = checkBoxes[i].parentNode.parentNode;
        //        keys.push(checkBoxes[i].value);
        //    }
        //}
        //チェックボックスの状態からキーを設定する
        setting_checklist();

        //if (keys.length == 0) { //テーブルの選択行がない場合
        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }
        //--2020/02/12 nv miyaji update end
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Location")/ShowPrinterList',
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                } else {
                    var PrinterKey = document.getElementById("PrinterKey");
                    PrinterKey.options.length = 0;
                    for (var i = 0; i < data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data[i].KEY1;
                        op.text = data[i].DATA1;
                        document.getElementById("PrinterKey").appendChild(op);
                    }
                    if (data.length == 1) {
                        document.getElementById("PrinterKey").value = data[0].KEY1;
                        btnPrinterOK_onclick();
                    } else {
                        // 前回その端末で選択したプリンタを選択する
                        const savedKey = localStorage.getItem("LocationPrintKey");
                        if (savedKey) {
                            document.getElementById("PrinterKey").value = savedKey;
                        }
                        //モーダルウインドウを開く
                        ShowPRTModalWindow();
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    // プリンタ選択
    function btnPrinterOK_onclick() {
        var prtKey = document.getElementById("PrinterKey").value;
        //--2020/02/12 nv miyaji コメントアウト start
        @*var keys = [];
        var grid = document.getElementById("location-detail");
        var checkBoxes = grid.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                var row = checkBoxes[i].parentNode.parentNode;
                keys.push(checkBoxes[i].value);
            }
        }
        if (keys.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }
        // スラッシュで区切った文字列を設定
        document.getElementById("SelectKeys").value = keys.join("/");*@
        //--2020/02/12 nv miyaji コメントアウト end
        // プリンタ選択状態保存
        localStorage.setItem("LocationPrintKey", prtKey);
        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/Location")/CsvExportForLabel");
        $('#form').submit();
        $('#form').attr("action", befAction);
    }

    //全選択ボタンクリック時処理
    function btnSelectAll_onclick() {
        var chkFlag = false;    //2020/02/12 nv miyaji add
        var btn = document.getElementById("btnSelectAll");
        if( btn.value == "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)"){
            btn.value ="@Message.getMessage(CommonLabel.BUTTON_ALL_OFF)";
            $('input[name="checkEdit"]').prop('checked',true)
            chkFlag = true;    //2020/02/12 nv miyaji add
        }else{
            btn.value ="@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
            $('input[name="checkEdit"]').prop('checked',false)
        }

        //--2020/02/12 nv miyaji add start
        //非同期で現在の検索条件での全件を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Location")/SelectAll',
            data: {blnSelectAll:chkFlag},
            dataType: 'json',
            cache: false,
            beforeSend: function () {
                //画面ブロックを表示
                showLoading();
            },
            success: function (data, dataType) {
                //件数を設定する
                document.getElementById("selectcount").innerHTML = data;

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            },
            complete: function () {
                //画面ブロックを解除
                hideLoading();
            }
        });
        //--2020/02/12 nv miyaji add end
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
         // ロケーション(会社)必須チェック
        var locL = document.getElementById("SearchLocL").value;
        if (locL.trim() === "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERRNO_SELECTED_LOC_L)")
            return false;
        }

        //画面ブロックを表示
        showLoading();

        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        //ローカルストレージに検索条件保存
        localStorageSave();

        //ページ番号のリセット
        ResetPageNum();

        return true;
    }

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

    //登録ボタンクリック処理
    function btnCreate_onclick() {
        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

        //ロケ中区分、小区分コードを書き込み不可
        document.getElementById("LOC_M_CD").readOnly = true;
        $("#LOC_M_CD").addClass("disable");
        document.getElementById("LOC_S_CD").readOnly = true;
        $("#LOC_S_CD").addClass("disable");

        // 設定M値による中区分の非活性
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes") {
            $('[id=LOC_DIV_M]').prop('disabled', true);
        }
        // 設定M値による小区分の非活性
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes") {
            $('[id=LOC_DIV_S]').prop('disabled', true);
        }

        // 初期値設定
        $('[id=LOC_DIV_L]').prop('checked', true);
        $('[id=LOC_DIV_M]').prop('checked', false);
        $('[id=LOC_DIV_S]').prop('checked', false);
        document.getElementById("LOC_L_CD").value = ""
        document.getElementById("LOC_M_CD").value = ""
        document.getElementById("LOC_S_CD").value = ""
        document.getElementById("LOC_NAME").value = ""
        document.getElementById("DISP_LOC_L_NAME").value = ""
        document.getElementById("DISP_LOC_M_NAME").value = ""
        document.getElementById("DISP_LOC_S_NAME").value = ""
        @* Add Start 2021.08.23 Iwamoto *@
       //("#LocShareFlg").prop("checked", false);
        @* 2025/10/14 ADD Kudo RFIDタグ拠点 *@
        $("#LocRfidFlg").prop("checked", false);
        $("#OwnInvFlg").prop("checked", false);


        document.getElementById("MAIL_SEND_ADDRESS").value = ""
        document.getElementById("MAINTE_LEASE_NUM").value = ""
        document.getElementById("MAINTE_LEASE_DAY").value = ""
        $("#MainteLeaseDayWorkFlg").prop("checked", false);
        document.getElementById("MAINTE_DAY").value = ""
        $("#MainteDayWorkFlg").prop("checked", false);
        @* Add End 2021.08.23 Iwamoto *@
        document.getElementById("SPECIFIED_QTY").value = ""                   @* 2023/02/08 ADD Shirakawa*@

        //削除ボタンを非表示
        $("#modal-delete-btn").css("display", "none");

        //--2019/11/21 nv miyaji add start
        // 大選択時、通常表示
        $("#td-LSentaku").show();
        $("#td-NotLSentaku").hide();
        $("#td-SSentaku").hide();
        $("#td-NotSSentaku").show();
        //--2019/11/21 nv miyaji add end

        //画面を初期化する
        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_UPDATE_CLICK") {
            //エラー情報のクリア
            $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        }

        //登録ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_CREATE_CLICK";

        //--2020/02/12 nv miyaji add
        //チェックボックスの状態からキーを設定する
        setting_checklist();

        //モーダルウインドウを開く
        ShowModalWindow()

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
            return;
        } else if (text.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

        //各区分コード書き込み不可
        document.getElementById("LOC_L_CD").readOnly = true;
        $("#LOC_L_CD").addClass("disable");
        document.getElementById("LOC_M_CD").readOnly = true;
        $("#LOC_M_CD").addClass("disable");
        document.getElementById("LOC_S_CD").readOnly = true;
        $("#LOC_S_CD").addClass("disable");

        // 2019/11/21 nv miyaji 各区分コンボボックスを非表示にする
        $("#td-LSentaku").show();
        $("#td-NotLSentaku").hide();
        $("#td-SSentaku").hide();
        $("#td-NotSSentaku").show();

        @* 2021/09/27 nv iwamoto update start *@
        ////削除ボタンを表示
        //$("#modal-delete-btn").css("display", "block");
        // 権限による処理区分が「編集・照会可能」でない場合
        if ("@ViewBag.AuthProcFlg" === "@AuthProcFlg.ShokaiNomi"){
            // 変更ボタンを非表示
            $("#modal-create-btn").css("display", "none");
            // 削除ボタンを非表示
            $("#modal-delete-btn").css("display", "none");
        }else{
            //削除ボタンを表示
            $("#modal-delete-btn").css("display", "block");
        };
        @* 2021/09/27 nv iwamoto update end *@

        //画面を初期化する
        document.getElementById("LOC_L_CD").value = ""
        document.getElementById("LOC_M_CD").value = ""
        document.getElementById("LOC_S_CD").value = ""
        document.getElementById("LOC_NAME").value = ""
        document.getElementById("DISP_LOC_L_NAME").value = ""
        document.getElementById("DISP_LOC_M_NAME").value = ""
        document.getElementById("DISP_LOC_S_NAME").value = ""

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        $("#LOC_L_CD").removeClass("input-error");
        $("#LOC_M_CD").removeClass("input-error");
        $("#LOC_S_CD").removeClass("input-error");
        $("#LOC_NAME").removeClass("input-error");

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/Location")/GetEditData',
                cache: false,
                data: { ID: text[0] },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }

                    //取得したデータを表示する
                    //document.getElementById("LOC_DIV").value = data.LOC_DIV;
                    document.getElementById("LOC_L_CD").value = data.LOC_L_CD;
                    document.getElementById("LOC_M_CD").value = data.LOC_M_CD;
                    document.getElementById("LOC_S_CD").value = data.LOC_S_CD;
                    document.getElementById("LOC_NAME").value = data.LOC_NAME;
                    //  '2025/10/14 DEL Kudo
                    @* Add Start 2021.08.23 Iwamoto *@
                    @*if (data.LOC_SHARE_FLG == "@LocShareFlg.Yes") {
                        $("#LocShareFlg").prop("checked", true);
                    }else{
                        $("#LocShareFlg").prop("checked", false);
                    }*@
                    //2025/10/14 ADD Kudo RFIDタグ拠点
                    if (data.LOC_RFID_FLG == "@LocRfidFlg.Yes") {
                        $("#LocRfidFlg").prop("checked", true);
                    } else {
                        $("#LocRfidFlg").prop("checked", false);
                    }
                    document.getElementById("LOC_RFID_FLG").value = data.LOC_RFID_FLG;
                    if (data.OWN_INV_FLG == "@OwnInvFlg.Yes") {
                        $("#OwnInvFlg").prop("checked", true);
                    } else {
                        $("#OwnInvFlg").prop("checked", false);
                    }
                    document.getElementById("OWN_INV_FLG").value = data.OWN_INV_FLG;

                    document.getElementById("MAIL_SEND_ADDRESS").value = data.MAIL_SEND_ADDRESS;
                    document.getElementById("MAINTE_LEASE_NUM").value = data.MAINTE_LEASE_NUM;
                    document.getElementById("MAINTE_LEASE_DAY").value = data.MAINTE_LEASE_DAY;
                    if (data.MAINTE_LEASE_DAY_WORK_FLAG == "@WorkDayFlg.Yes") {
                        $("#MainteLeaseDayWorkFlg").prop("checked", true);
                    }else{
                        $("#MainteLeaseDayWorkFlg").prop("checked", false);
                    }
                    document.getElementById("MAINTE_DAY").value = data.MAINTE_DAY;
                    if (data.MAINTE_DAY_WORK_FLAG == "@WorkDayFlg.Yes") {
                        $("#MainteDayWorkFlg").prop("checked", true);
                    }else{
                        $("#MainteDayWorkFlg").prop("checked", false);
                    }
                    @* Add End 2021.08.23 Iwamoto *@
                    document.getElementById("SPECIFIED_QTY").value = data.SPECIFIED_QTY;            @*2023/02/08 ADD Shirakawa*@
                    document.getElementById("INS_DATETIME").value = formatDate(new Date(data.InsDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("INS_EMPLOYEE_CD").value = data.INS_EMPLOYEE_CD;
                    document.getElementById("INS_EMPLOYEE_NAME").value = data.INS_EMPLOYEE_NAME;
                    document.getElementById("UPD_DATETIME").value = formatDate(new Date(data.UpdDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("UPD_EMPLOYEE_CD").value = data.UPD_EMPLOYEE_CD;
                    document.getElementById("UPD_EMPLOYEE_NAME").value = data.UPD_EMPLOYEE_NAME;
                    document.getElementById("UPD_TASK").value = data.RenewProcess;

                    document.getElementById("DISP_LOC_L_NAME").value = data.LocLName;
                    document.getElementById("DISP_LOC_M_NAME").value = data.LocMName;
                    document.getElementById("DISP_LOC_S_NAME").value = data.LocSName;

                    //ロケーション区分による設定
                    switch (data.LOC_DIV) {
                        case "@LOC_DIV.LocationL":
                            $("#LOC_DIV_L").prop("checked", true);
                            $("#LOC_DIV_M").prop("checked", false);
                            $("#LOC_DIV_S").prop("checked", false);

                            $('#LOC_DIV_L').prop('disabled', false);
                            $('#LOC_DIV_M').prop('disabled', true);
                            $('#LOC_DIV_S').prop('disabled', true);

                            //$("#LocShareFlg").prop("disabled", false);
                            //2025/10/14 ADD Kudo RFIDタグ拠点
                            $("#LocRfidFlg").prop("disabled", false);
                            $("#OwnInvFlg").prop("disabled", false);
                            $("#MAIL_SEND_ADDRESS").attr("readonly", false);
                            $("#MAIL_SEND_ADDRESS").removeClass("disable");
                            $("#MAINTE_LEASE_NUM").attr("readonly", false);
                            $("#MAINTE_LEASE_NUM").removeClass("disable");
                            $("#MAINTE_LEASE_DAY").attr("readonly", false);
                            $("#MAINTE_LEASE_DAY").removeClass("disable");
                            $("#MainteLeaseDayWorkFlg").prop("disabled", false);
                            $("#MAINTE_DAY").attr("readonly", false);
                            $("#MAINTE_DAY").removeClass("disable");
                            $("#MainteDayWorkFlg").prop("disabled", false);
                            @* 2023/02/08 ADD Start Shirakawa*@
                            $("#SPECIFIED_QTY").attr("readonly", false);
                            $("#SPECIFIED_QTY").removeClass("disable");
                            @* 2023/02/08 ADD End Shirakawa*@
                            break;

                        case "@LOC_DIV.LocationM":
                            $("#LOC_DIV_L").prop("checked", false);
                            $("#LOC_DIV_M").prop("checked", true);
                            $("#LOC_DIV_S").prop("checked", false);

                            $('#LOC_DIV_L').prop('disabled', true);
                            $('#LOC_DIV_M').prop('disabled', false);
                            $('#LOC_DIV_S').prop('disabled', true);

                            //$("#LocShareFlg").prop("disabled", true);
                            //2025/10/14 ADD Kudo RFIDタグ拠点
                            $("#LocRfidFlg").prop("disabled", true);
                            $("#OwnInvFlg").prop("disabled", true);
                            $("#MAIL_SEND_ADDRESS").attr("readonly", true);
                            $("#MAIL_SEND_ADDRESS").addClass("disable");
                            $("#MAINTE_LEASE_NUM").attr("readonly", true);
                            $("#MAINTE_LEASE_NUM").addClass("disable");
                            $("#MAINTE_LEASE_DAY").attr("readonly", true);
                            $("#MAINTE_LEASE_DAY").addClass("disable");
                            $("#MainteLeaseDayWorkFlg").prop("disabled", true);
                            $("#MAINTE_DAY").attr("readonly", true);
                            $("#MAINTE_DAY").addClass("disable");
                            $("#MainteDayWorkFlg").prop("disabled", true);
                            @* 2023/02/08 ADD Start Shirakawa*@
                            $("#SPECIFIED_QTY").attr("readonly", true);
                            $("#SPECIFIED_QTY").addClass("disable");
                            @* 2023/02/08 ADD End Shirakawa*@
                            break;

                        case "@LOC_DIV.LocationS":
                            $("#LOC_DIV_L").prop("checked", false);
                            $("#LOC_DIV_M").prop("checked", false);
                            $("#LOC_DIV_S").prop("checked", true);

                            $('#LOC_DIV_L').prop('disabled', true);
                            $('#LOC_DIV_M').prop('disabled', true);
                            $('#LOC_DIV_S').prop('disabled', false);

                            //$("#LocShareFlg").prop("disabled", true);
                            //2025/10/14 ADD Kudo RFIDタグ拠点
                            $("#LocRfidFlg").prop("disabled", true);
                            $("#OwnInvFlg").prop("disabled", true);
                            $("#MAIL_SEND_ADDRESS").attr("readonly", true);
                            $("#MAIL_SEND_ADDRESS").addClass("disable");
                            $("#MAINTE_LEASE_NUM").attr("readonly", true);
                            $("#MAINTE_LEASE_NUM").addClass("disable");
                            $("#MAINTE_LEASE_DAY").attr("readonly", true);
                            $("#MAINTE_LEASE_DAY").addClass("disable");
                            $("#MainteLeaseDayWorkFlg").prop("disabled", true);
                            $("#MAINTE_DAY").attr("readonly", true);
                            $("#MAINTE_DAY").addClass("disable");
                            $("#MainteDayWorkFlg").prop("disabled", true);
                            @* 2023/02/08 ADD Start Shirakawa*@
                            $("#SPECIFIED_QTY").attr("readonly", true);
                            $("#SPECIFIED_QTY").addClass("disable");
                            @* 2023/02/08 ADD End Shirakawa*@
                            break;
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }

        //変更ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_UPDATE_CLICK";

        //--2020/02/12 nv miyaji add
        //チェックボックスの状態からキーを設定する
        setting_checklist();

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //登録ボタンクリック処理（確認メッセージ）
    function btnCreateModal_onclick(){
        // メッセージ表示
        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
            showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/Location")/Create");
        } else {
            showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/Location")/Create");
        }
    }

    //削除ボタンクリック処理
    function btnDelete_onclick() {
        //確認メッセージ
        showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/Location")/Delete");

        //--2020/02/12 nv miyaji add
        //チェックボックスの状態からキーを設定する
        setting_checklist();
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        //画面ブロックを表示
        showLoading();

        //--2020/02/12 nv miyaji add
        //チェックボックスの状態からキーを設定する
        setting_checklist();

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //大区分選択
    function ChangeLocationL(locL, initialM, initialS, doAutoSearch) {
        //--2019/11/21 nv miyaji upd start
        if(locL.name == "SearchLocL"){
            // 中区分をクリア
            $("#SearchLocM").val("").trigger("change");
            $("#SearchLocM").prop("disabled", true);
            // 小区分をクリア
            $("#SearchLocS").val("").trigger("change");
            $("#SearchLocS").prop("disabled", true);
        }else{
            // コンボボックスで選択した値をエディットボックスにも反映する。
            document.getElementById("LOC_L_CD").value = $("#ModalLocL").val();
            GetLocLName(document.getElementById("LOC_L_CD").value)
            // 中区分をクリア
            document.getElementById("LOC_M_CD").value = "";
            $("#ModalLocM").val("").trigger("change");
            $("#ModalLocM").attr("disabled", "disabled");
            // 小区分をクリア
            document.getElementById("LOC_S_CD").value = "";
        }
        //// 中区分をクリア
        //document.getElementById("SearchLocM").value = "";
        //$("#SearchLocM").attr("disabled", "disabled");
        //// 小区分をクリア
        //document.getElementById("SearchLocS").value = "";
        //$("#SearchLocS").attr("disabled", "disabled");
        //--2019/11/21 nv miyaji upd end

        // 選択されていない場合はここで終了
        if (!locL.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Location")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationM", selectLCode: locL.value, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    //--2019/11/18 nv miyaji upd start
                    var SearchLocM = ""
                    if(locL.name == "SearchLocL"){
                        SearchLocM = document.getElementById("SearchLocM");
                    }else{
                        SearchLocM =document.getElementById("ModalLocM");
                    }
                    //var SearchLocM = document.getElementById("SearchLocM");
                    SearchLocM.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchLocM.appendChild(opBlank);
                    //document.getElementById("SearchLocM").appendChild(opBlank);

                    for (var i = 0; i < data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data[i].LOC_CD;
                        op.text = data[i].LOC_NAME;
                        SearchLocM.appendChild(op);
                        //document.getElementById("SearchLocM").appendChild(op);
                    }
                }
                // 中区分データ取得
                if(locL.name == "SearchLocL"){
                    $("#SearchLocM").removeAttr("disabled");
                }else{
                    $("#ModalLocM").removeAttr("disabled");
                }
                //$("#SearchLocM").removeAttr("disabled");
                //document.getElementById("SearchLocM").value = data.locMDropdownData;
                SearchLocM.value = data.locMDropdownData;
                // 選択肢の入れ替えをSelect2に反映（検索条件のみ。登録・変更画面は通常のコンボボックス）
                if(locL.name == "SearchLocL"){
                    refreshSelect2(SearchLocM);
                }
                //検索時にロケ大区分の検索条件を保存2025/10/15 Kudo
                if (initialM) {
                    // ※trigger("change") だと onchange（ChangeLocationM）も呼ばれ二重に実行されるため refreshSelect2 を使用
                    $("#SearchLocM").val(initialM);
                    refreshSelect2("#SearchLocM");
                    ChangeLocationM(SearchLocM, initialS,  doAutoSearch);
                } else if (doAutoSearch) {
                    // 検索処理実行
                    if (document.getElementById('FirstSearch').value == "True") {
                        document.getElementById('Search_Button').click();
                    }
                }

                //--2019/11/21 nv miyaji upd end
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //中区分選択
    function ChangeLocationM(locM, initialS, doAutoSearch) {
        var lCode = document.getElementById("SearchLocL").value;
        //--2019/11/18 nv miyaji upd start
        if(locM.name == "SearchLocM"){
            // 小分類をクリア
            $("#SearchLocS").val("").trigger("change");
            $("#SearchLocS").prop("disabled", true);
        }else{
            // コンボボックスで選択した値をエディットボックスにも反映する。
            document.getElementById("LOC_M_CD").value = $("#ModalLocM").val();
            GetLocMName(document.getElementById("LOC_M_CD").value)
            // 小分類をクリア
            document.getElementById("LOC_S_CD").value = "";
            // モーダル画面の場合はここで終了
            return;
        }
        //document.getElementById("SearchLocS").value = "";
        //$("#SearchLocS").attr("disabled", "disabled");
        //--2019/11/21 nv miyaji upd end

        // 選択されていない場合はここで終了
        if (!locM.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Location")/ChangeLocation',
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
                    document.getElementById("SearchLocS").appendChild(opBlank);

                    for (var i = 0; i < data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data[i].LOC_CD;
                        op.text = data[i].LOC_NAME;
                        document.getElementById("SearchLocS").appendChild(op);
                    }
                }
                // 小区分データ取得
                $("#SearchLocS").removeAttr("disabled");
                document.getElementById("SearchLocS").value = data.locSDropdownData;
                // 選択肢の入れ替えをSelect2に反映
                refreshSelect2("#SearchLocS");

                //--2021/10/13 sv suzuki add start 2025/10/23ADD
                if (initialS) {
                    $("#SearchLocS").val(initialS).trigger("change");
                }

                  //検索時にロケ大区分の検索条件を保存2025/10/15 Kudo
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

    //ロケーション区分変更
    function ChangeLocDiv(value) {
        switch (value) {
            case "@LOC_DIV.LocationL":
                //--2019/11/21 nv miyaji add start
                // 大区分選択時、通常表示
                $("#td-LSentaku").show();
                $("#td-NotLSentaku").hide();
                $("#td-SSentaku").hide();
                $("#td-NotSSentaku").show();
                // 大区分コード・名称をクリア
                $("#LOC_L_CD").val("");
                $("#DISP_LOC_L_NAME").val("");
                // 大区分コンボボックスの選択をクリア
                $("#ModalLocL").val("").trigger("change");
                // 中区分コンボボックスをクリア
                $("#ModalLocM").val("").trigger("change");
                $("#ModalLocM").attr("disabled", "disabled");
                //--2019/11/21 nv miyaji add end
                //中区分：書き込み不可
                document.getElementById("LOC_M_CD").readOnly = true;
                $("#LOC_M_CD").addClass("disable");
                $("#LOC_M_CD").val("");
                $("#DISP_LOC_M_NAME").val("");
                //小区分：書き込み不可
                document.getElementById("LOC_S_CD").readOnly = true;
                $("#LOC_S_CD").addClass("disable");
                $("#LOC_S_CD").val("");
                $("#DISP_LOC_S_NAME").val("");

                //共通ロケーション：使用可
              //  $("#LocShareFlg").prop("disabled", false);
                //2025/10/14 ADD Kudo RFIDタグ拠点
                $("#LocRfidFlg").prop("disabled", false);
                $("#OwnInvFlg").prop("disabled", false);
                //メール送信先：書き込み可
                $("#MAIL_SEND_ADDRESS").attr("readonly", false);
                $("#MAIL_SEND_ADDRESS").removeClass("disable");
                //貸出回数：書き込み可
                $("#MAINTE_LEASE_NUM").attr("readonly", false);
                $("#MAINTE_LEASE_NUM").removeClass("disable");
                //貸出日数：書き込み可
                $("#MAINTE_LEASE_DAY").attr("readonly", false);
                $("#MAINTE_LEASE_DAY").removeClass("disable");
                //稼働日考慮(貸出日数)：使用可
                $("#MainteLeaseDayWorkFlg").prop("disabled", false);
                //メンテナンス日数：書き込み可
                $("#MAINTE_DAY").attr("readonly", false);
                $("#MAINTE_DAY").removeClass("disable");
                //稼働日考慮(メンテナンス日数)：使用可
                $("#MainteDayWorkFlg").prop("disabled", false);
                //2023/02/08 ADD Start Shirakawa
                //指定数量：書き込み可
                $("#SPECIFIED_QTY").attr("readonly", false);
                $("#SPECIFIED_QTY").removeClass("disable");
                //2023/02/08 ADD End Shirakawa
                break;

            case "@LOC_DIV.LocationM":
                //--2019/11/21 nv miyaji add start
                // 中区分選択時、大区分をコンボボックス表示にする
                $("#td-LSentaku").hide();
                $("#td-NotLSentaku").show();
                $("#td-SSentaku").hide();
                $("#td-NotSSentaku").show();
                // 大区分コード・名称をクリア
                $("#LOC_L_CD").val("");
                $("#DISP_LOC_L_NAME").val("");
                // 中区分コード・名称をクリア
                $("#LOC_M_CD").val("");
                $("#DISP_LOC_M_NAME").val("");
                // 大区分コンボボックスの選択をクリア
                $("#ModalLocL").val("").trigger("change");
                // 中区分コンボボックスをクリア
                $("#ModalLocM").val("").trigger("change");
                $("#ModalLocM").attr("disabled", "disabled");
                //--2019/11/21 nv miyaji add end
                //中区分：書き込み可
                document.getElementById("LOC_M_CD").readOnly = false;
                $("#LOC_M_CD").removeClass("disable");
                //小区分：書き込み不可
                document.getElementById("LOC_S_CD").readOnly = true;
                $("#LOC_S_CD").addClass("disable");
                $("#LOC_S_CD").val("");
                $("#DISP_LOC_S_NAME").val("");
                //共通ロケーション：使用不可
               //$("#LocShareFlg").prop("disabled", true);
               //$("#LocShareFlg").prop("checked", false);
                //'2025/10/14 ADD Kudo RFIDタグ拠点
                $("#LocRfidFlg").prop("disabled", true);
                $("#LocRfidFlg").prop("checked", false);
                $("#OwnInvFlg").prop("disabled", true);
                $("#OwnInvFlg").prop("checked", false);
                //メール送信先：書き込み不可
                $("#MAIL_SEND_ADDRESS").attr("readonly", true);
                $("#MAIL_SEND_ADDRESS").addClass("disable");
                $("#MAIL_SEND_ADDRESS").val("");
                //貸出回数：書き込み不可
                $("#MAINTE_LEASE_NUM").attr("readonly", true);
                $("#MAINTE_LEASE_NUM").addClass("disable");
                $("#MAINTE_LEASE_NUM").val("");
                //貸出日数：書き込み不可
                $("#MAINTE_LEASE_DAY").attr("readonly", true);
                $("#MAINTE_LEASE_DAY").addClass("disable");
                $("#MAINTE_LEASE_DAY").val("");
                //稼働日考慮(貸出日数)：使用不可
                $("#MainteLeaseDayWorkFlg").prop("disabled", true);
                $("#MainteLeaseDayWorkFlg").prop("checked", false);
                //メンテナンス日数：書き込み不可
                $("#MAINTE_DAY").attr("readonly", true);
                $("#MAINTE_DAY").addClass("disable");
                $("#MAINTE_DAY").val("");
                //稼働日考慮(メンテナンス日数)：使用不可
                $("#MainteDayWorkFlg").prop("disabled", true);
                $("#MainteDayWorkFlg").prop("checked", false);
                //2023/02/08 ADD Start Shirakawa
                //指定数量：書き込み不可
                $("#SPECIFIED_QTY").attr("readonly", true);
                $("#SPECIFIED_QTY").addClass("disable");
                $("#SPECIFIED_QTY").val("");
                //2023/02/08 ADD End Shirakawa
                break;

            case "@LOC_DIV.LocationS":
                //--2019/11/21 nv miyaji add start
                // 小区分選択時、大区分・中区分をコンボボックス表示にする
                $("#td-LSentaku").hide();
                $("#td-NotLSentaku").show();
                $("#td-SSentaku").show();
                $("#td-NotSSentaku").hide();
                // 大区分コード・名称をクリア
                $("#LOC_L_CD").val("");
                $("#DISP_LOC_L_NAME").val("");
                // 中区分コード・名称をクリア
                $("#LOC_M_CD").val("");
                $("#DISP_LOC_M_NAME").val("");
                // 大区分コンボボックスの選択をクリア
                $("#ModalLocL").val("").trigger("change");
                // 中区分コンボボックスをクリア
                $("#ModalLocM").val("").trigger("change");
                $("#ModalLocM").attr("disabled", "disabled");
                //--2019/11/21 nv miyaji add end
                //中区分：書き込み可
                document.getElementById("LOC_M_CD").readOnly = false;
                $("#LOC_M_CD").removeClass("disable");
                //小区分：書き込み可
                document.getElementById("LOC_S_CD").readOnly = false;
                $("#LOC_S_CD").removeClass("disable");
                //共通ロケーション：使用不可
                //$("#LocShareFlg").prop("disabled", true);
                //$("#LocShareFlg").prop("checked", false);
                //'2025/10/14 ADD Kudo RFIDタグ拠点
                $("#LocRfidFlg").prop("disabled", true);
                $("#LocRfidFlg").prop("checked", false);
                $("#OwnInvFlg").prop("disabled", true);
                $("#OwnInvFlg").prop("checked", false);
                //メール送信先：書き込み不可
                $("#MAIL_SEND_ADDRESS").attr("readonly", true);
                $("#MAIL_SEND_ADDRESS").addClass("disable");
                $("#MAIL_SEND_ADDRESS").val("");
                //貸出回数：書き込み不可
                $("#MAINTE_LEASE_NUM").attr("readonly", true);
                $("#MAINTE_LEASE_NUM").addClass("disable");
                $("#MAINTE_LEASE_NUM").val("");
                //貸出日数：書き込み不可
                $("#MAINTE_LEASE_DAY").attr("readonly", true);
                $("#MAINTE_LEASE_DAY").addClass("disable");
                $("#MAINTE_LEASE_DAY").val("");
                //稼働日考慮(貸出日数)：使用不可
                $("#MainteLeaseDayWorkFlg").prop("disabled", true);
                $("#MainteLeaseDayWorkFlg").prop("checked", false);
                //メンテナンス日数：書き込み不可
                $("#MAINTE_DAY").attr("readonly", true);
                $("#MAINTE_DAY").addClass("disable");
                $("#MAINTE_DAY").val("");
                //稼働日考慮(メンテナンス日数)：使用不可
                $("#MainteDayWorkFlg").prop("disabled", true);
                $("#MainteDayWorkFlg").prop("checked", false);
                //2023/02/08 ADD Start Shirakawa
                //指定数量：書き込み不可
                $("#SPECIFIED_QTY").attr("readonly", true);
                $("#SPECIFIED_QTY").addClass("disable");
                $("#SPECIFIED_QTY").val("");
                //2023/02/08 ADD End Shirakawa
                break;
        }
        // 設定M値による中区分の非活性
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes") {
            $("#LOC_M_CD").attr("disabled", "disabled");
            $("#LOC_M_CD").addClass("disable");
        }
        // 設定M値による小区分の非活性
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes") {
            $("#LOC_S_CD").attr("disabled", "disabled");
            $("#LOC_S_CD").addClass("disable");
        }
    }

    //大区分名称取得
    function GetLocLName(value) {
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Location")/GetLocName',
            data: { inputLCode: value, inputMCode: '', inputSCode: '' },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    document.getElementById("DISP_LOC_L_NAME").value = "";
                    return;
                }
                else {
                    //取得したデータを表示する
                    document.getElementById("DISP_LOC_L_NAME").value = "";
                    if (data.length != 0) {
                        document.getElementById("DISP_LOC_L_NAME").value = data[0].LOC_L_NAME;
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }
    //中区分名称取得
    function GetLocMName(value) {
        var locLCd;
        locLCd = document.getElementById("LOC_L_CD").value;

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Location")/GetLocName',
            data: { inputLCode: locLCd, inputMCode: value, inputSCode: '' },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    document.getElementById("DISP_LOC_M_NAME").value = "";
                    return;
                }
                else {
                    //取得したデータを表示する
                    document.getElementById("DISP_LOC_M_NAME").value = "";
                    if (data.length != 0) {
                        document.getElementById("DISP_LOC_M_NAME").value = data[0].LOC_M_NAME;
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }
    //小区分名称取得
    function GetLocSName(value) {
        var locLCd;
        locLCd = document.getElementById("LOC_L_CD").value;
        var locMCd;
        locMCd = document.getElementById("LOC_M_CD").value;

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Location")/GetLocName',
            data: { inputLCode: locLCd, inputMCode: locMCd, inputSCode: value },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    document.getElementById("DISP_LOC_S_NAME").value = "";
                    return;
                }
                else {
                    //取得したデータを表示する
                    document.getElementById("DISP_LOC_S_NAME").value = "";
                    if (data.length != 0) {
                        document.getElementById("DISP_LOC_S_NAME").value = data[0].LOC_S_NAME;
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //チェックボックスクリック処理    // --2020/02/12 nv miyaji add
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

    //チェックボックスの選択状態を設定する   // --2020/02/12 nv miyaji add
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

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }
</script>

<div id="location-master">
    @Using (Html.BeginForm("Index", "Location", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    <div style="margin-bottom: 5px;">
                        @*<input class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                            <input class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">*@
                        @If ViewBag.AuthProcFlg <> AuthProcFlg.ShokaiNomi Then
                            @<input id="create-btn" Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                            @<input id="update-btn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">
                        Else
                            @<input id="show-btn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DISP)" onclick="btnEdit_onclick()">
                        End If
                    </div>
                    <div>
                        @* 2020/02/12 nv miyaji　全選択/全解除ボタンの表記文字設定 *@
                        @Code
                            Dim AllSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG)
                            Dim sDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                            If AllSelectFlg Then
                                sDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                            End If
                        End Code
                        <input class="defButton" type="button" id="btnSelectAll" value="@sDispStr" onclick="btnSelectAll_onclick()">
                        @*<input class="defButton" type="button" id="btnSelectAll" value="@Message.getMessage(CommonLabel.BUTTON_ALL_ON)" onclick="btnSelectAll_onclick()">*@
                        <input class="defButton size-x-large" type="button" value="@Message.getMessage(CommonLabel.BUTTON_LOC_LABEL)" onclick="btnLocLabel_onclick()">
                    </div>
                </div>
                <div class="view-area-button-right">
                    @* 2021/09/27 nv iwamoto update start *@
                    @*<input class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">*@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @<input id="csv-import-btn" Class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">
                    End If
                    @* 2021/09/27 nv iwamoto update end *@
                    <input class="defButton csv-Export-btn  submit-button" type="button" name="CsvExport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL)" onclick="btnCsvExport_onclick()">
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
                            <div class="col-sm-2 filter-combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("LOC_DIV").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("LOC_CD").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("LOC_NAME").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L)<font Class="label-require">※</font></div>
                                <div class="grid-condition-col">
                                    @* select2-clearable クラスで入力して絞り込めるSelect2（単一選択・×ボタン付き）になる（Scripts\select2-common.js）。×での選択解除用に空の選択肢を追加 *@
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocL, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control select2-clearable", .onchange = "ChangeLocationL(this)"})  @*--2019/11/19 nv miyaji 引数をthis.valueからthisに変更*@
                                </div>
                            </div>

                            <div class="col-sm-2 filter-combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("LOC_DIV").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("LOC_CD").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("LOC_NAME").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_M)</div>
                                <div class="grid-condition-col">
                                    @* select2-clearable クラスで入力して絞り込めるSelect2（単一選択・×ボタン付き）になる（Scripts\select2-common.js） *@
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocM, DirectCast(ViewBag.modelLocM, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control select2-clearable", .onchange = "ChangeLocationM(this)"})  @*--2019/11/19 nv miyaji 引数をthis.valueからthisに変更*@
                                </div>
                            </div>

                            <div class="col-sm-2 filter-combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("LOC_DIV").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("LOC_CD").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("LOC_NAME").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_S)</div>
                                <div class="grid-condition-col">
                                    @* select2-clearable クラスで入力して絞り込めるSelect2（単一選択・×ボタン付き）になる（Scripts\select2-common.js） *@
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocS, DirectCast(ViewBag.modelLocS, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control select2-clearable"})  @*--2019/11/19 nv miyaji 引数をthis.valueからthisに変更*@
                                </div>
                            </div>


                            @* 処理分類 *@
                            <div class="col-sm-5" style="max-width:750px;">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_DISP_SELECT)</div>
                                <div class="grid-condition-col">
                                    <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.L, New With {.id = "SearchDispSelect_L"})@Message.getMessage(CommonLabel.VALUE_LOC_L)</div>
                                    <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.LM, New With {.id = "SearchDispSelect_LM"})@Message.getMessage(CommonLabel.LABEL_LOC_LM)</div>
                                    <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.LMS, New With {.id = "SearchDispSelect_LMS"})@Message.getMessage(CommonLabel.LABEL_LOC_LMS)</div>
                                    <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.M, New With {.id = "SearchDispSelect_M"})@Message.getMessage(CommonLabel.LABEL_LOC_M_ONLY)</div>
                                    <div class="input-select-item-last">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.S, New With {.id = "SearchDispSelect_S"})@Message.getMessage(CommonLabel.LABEL_LOC_S_ONLY)</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="view-area-label" style="height: 25px"></div>
                    @*検索ボタンにidをつける 2025/10/15 Kudo*@
                    <input class="float-right defButton submit-button" id="Search_Button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>
        @*直近登録者情報*@
        @<div class="search-wrap">
            <div class="float-left mr50">@Message.getMessage(CommonLabel.LABEL_INS_DATETIME):@ViewBag.LastInsDateTime</div>
            <div class="float-left">@Message.getMessage(CommonLabel.LABEL_INS_EMPLOYEE_NAME):@ViewBag.LastInsEmployeeNm</div>
            <div style="clear:both;"></div>
        </div>

        @*検索時にロケ大区分の検索条件を保存2025/ 10 / 15 Kudo*@
        @<input type="hidden" id="IsRestoreOwnSearchConditions" name="IsRestoreOwnSearchConditions" value="@Model.FirstOrDefault().IsRestoreOwnSearchConditions.ToString()" />
        @<input type="hidden" id="FirstSearch" name="FirstSearch" value="@Model.FirstOrDefault().FirstSearch.ToString()" />
        @<input type="hidden" id="EditLocLCd" name="EditLocLCd" value="@Model.FirstOrDefault().EditLocLCd.ToString()" />

        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @* --2020/02/12 nv miyaji add start *@
        @*@Html.Label("選択件数：", New With {.style = "font-size:12px;"}) yamada*@
        @Html.Label(Message.getMessage(CommonLabel.LABEL_SELECT_COUNT), New With {.style = "font-size:12px;"})
        @Code
            Dim lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
            Dim CheckedCount As String = "0"
            If Not IsNothing(lstCheckedKey) Then
                CheckedCount = lstCheckedKey.Count.ToString
            End If
            @Html.Label(CheckedCount, New With {.id = "selectcount", .style = "margin-right:15px;font-size:12px;"})
        End Code
        @* --2020/02/12 nv miyaji add end *@

        @<div class="list-view">

        @*<div id="ListGrids" style="height:auto;padding-bottom: 20px;" />*@
            <div id="ListGrids" style="height: auto; margin-bottom: 20px;" />
    
        </div>

        @* チェックボックス選択値の引き渡し用隠し項目 *@
        @<input id="SelectKeys" name="SelectKeys" type="hidden">
        @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden" /> @* 2020/02/12 nv miyaji add *@

        @* プリンタ選択モーダル画面 *@
        @<div id="modal-prt-content">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title-prt"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
            @CODE
                'モデルの値を設定
                Dim prtSessionModel As LocationViewModel = Session(CommonSessionKey.CREAGE_LOCATION)
                If Not IsNothing(prtSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(prtSessionModel)
                Else
                    Model.Add(New LocationViewModel())
                End If
            END CODE
            <br />
            <div class="mb10">
                @Html.DropDownListFor(Function(model) model.FirstOrDefault.PrinterKey, DirectCast(ViewBag.modelPrinter, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 300px;"})
            </div>
            <br />
            <div Class="PopupButton float-left">
                <input id="modal-ok" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_OK)" onclick="btnPrinterOK_onclick()">
            </div>
            <div Class="PopupButton float-right">
                <input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CANCEL)">
            </div>
        </div>

        @* CSV取込のモーダル画面 *@
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
                Dim csvSessionModel As LocationViewModel = Session(CommonSessionKey.CREAGE_LOCATION)
                If Not IsNothing(csvSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(csvSessionModel)
                Else
                    Model.Add(New LocationViewModel())
                End If
            END CODE
            @*@Html.AntiForgeryToken()*@
            <div Class="PopupButton clearfix mb10">
                <div class="float-left">
                    <input type="file" name="CsvFile" id="CsvFile" />
                    <div style="color:red;">@Message.getMessage(CommonLabel.MSG_INFO_CSV_IMPORT_ALL_DATA_DELETE)</div>
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

        @* 登録・更新のモーダル画面 *@
        @<div id="modal-content" class="modal-location">
            <div Class="popup-title-wrap">
                <p Class="Line3"></p>
                <p Class="Line4"></p>
                <h2 id="popup-title"></h2>
                <p Class="Line3"></p>
                <p Class="Line4"></p>
            </div>
            @CODE
                'モデルの値を設定
                Dim SessionModel As LocationViewModel = Session(CommonSessionKey.CREAGE_LOCATION)
                If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(SessionModel)
                Else
                    Model.Add(New LocationViewModel())
                End If
            END CODE
            @Html.AntiForgeryToken()

         <Table>
             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_LOC_DIV)</td>
                 <td width="20" class="require">※</td>
                 <td class="radio-group">
                     <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LOC_DIV, LOC_DIV.LocationL, New With {.id = "LOC_DIV_L", .onchange = "ChangeLocDiv(this.value)"})@Message.getMessage(CommonLabel.VALUE_LOC_L)</div>
                     <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LOC_DIV, LOC_DIV.LocationM, New With {.id = "LOC_DIV_M", .onchange = "ChangeLocDiv(this.value)"})@Message.getMessage(CommonLabel.VALUE_LOC_M)</div>
                     <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LOC_DIV, LOC_DIV.LocationS, New With {.id = "LOC_DIV_S", .onchange = "ChangeLocDiv(this.value)"})@Message.getMessage(CommonLabel.VALUE_LOC_S)</div>
                 </td>
             </tr>
             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L_CD)</td>
                 <td width="20" class="require">※</td>
                 <td id="td-LSentaku">
                     @Html.EditorFor(Function(model) model.Last.LOC_L_CD, New With {.htmlAttributes = New With {.class = "text-box-cd-small", .maxlength = 10, .oninput = "editHanEi(this); GetLocLName(this.value)", .style = "width:85px;"}})
                     @Html.EditorFor(Function(model) model.Last.DispLocLName, New With {.htmlAttributes = New With {.id = "DISP_LOC_L_NAME", .class = "disable", .readonly = "readonly", .style = "width:701px;"}})
                 </td>
                 <td id="td-NotLSentaku">
                     @* 分類区分で中区分・小区分選択時はコンボボックス表示にする  *@
                     @Html.DropDownListFor(Function(model) model.Last.ModalLocL, DirectCast(ViewBag.modalmodelLocL, IEnumerable(Of SelectListItem)), Nothing, New With {.class = "filter-combo", .onchange = "ChangeLocationL(this)", .style = "width: 420px;"})
                 </td>
             </tr>
             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_LOC_M_CD)</td>
                 <td width="20"></td>
                 <td id="td-NotSSentaku">
                     @Html.EditorFor(Function(model) model.Last.LOC_M_CD, New With {.htmlAttributes = New With {.class = "text-box-cd-small", .maxlength = 10, .oninput = "editHanEi(this); GetLocMName(this.value)", .style = "width:85px;"}})
                     @Html.EditorFor(Function(model) model.Last.DispLocMName, New With {.htmlAttributes = New With {.id = "DISP_LOC_M_NAME", .class = "disable", .readonly = "readonly", .style = "width:701px;"}})
                 </td>
                 <td id="td-SSentaku">
                     @* 分類区分で小区分選択時はコンボボックス表示にする  *@
                     @Html.DropDownListFor(Function(model) model.Last.ModalLocM, DirectCast(ViewBag.modalmodelLocM, IEnumerable(Of SelectListItem)), "", New With {.class = "filter-combo", .onchange = "ChangeLocationM(this)", .style = "width: 420px;"})
                 </td>
             </tr>
             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_LOC_S_CD)</td>
                 <td width="20"></td>
                 <td>
                     @Html.EditorFor(Function(model) model.Last.LOC_S_CD, New With {.htmlAttributes = New With {.class = "text-box-cd-small", .maxlength = 10, .oninput = "editHanEi(this); GetLocSName(this.value)", .style = "width:85px;"}})
                     @Html.EditorFor(Function(model) model.Last.DispLocSName, New With {.htmlAttributes = New With {.id = "DISP_LOC_S_NAME", .class = "disable", .readonly = "readonly", .style = "width:701px;"}})
                 </td>
             </tr>
             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_LOC_NAME)</td>
                 <td width="20"></td>
                 <td>@Html.EditorFor(Function(model) model.Last.LOC_NAME, New With {.htmlAttributes = New With {.maxlength = 60, .style = "width:790px;"}})</td>
             </tr>

             @* Add Start 2021.08.23 Iwamoto *@
             @*<tr Class="popup-row">
            <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_LOC_SHARE_FLG)</td>
            <td width="20"></td>
            <td>@Html.CheckBoxFor(Function(model) model.Last.LocShareFlg) @Message.getMessage(CommonLabel.LABEL_LOC_SHARE)</td>
        </tr>*@

             @*2025/10/14 ADD Kudo RFIDタグ拠点 *@
             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_LOC_RFID_FLG)</td>
                 <td width="20"></td>
                 <td>@Html.CheckBoxFor(Function(model) model.Last.LocRfidFlg) @Message.getMessage(CommonLabel.VALUE_LOC_RFID_FLG)</td>
                 @Html.HiddenFor(Function(model) model.Last.LOC_RFID_FLG)
             </tr>

             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabelLocation.LABEL_OWN_INV_FLG)</td>
                 <td width="20"></td>
                 <td>@Html.CheckBoxFor(Function(model) model.Last.OwnInvFlg) @Message.getMessage(CommonLabelLocation.LABEL_OWN_INV_ONLY)</td>
                 @Html.HiddenFor(Function(model) model.Last.OWN_INV_FLG)
             </tr>

             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_MAIL_SEND_ADDRESS)</td>
                 <td width="20"></td>
                 <td>@Html.EditorFor(Function(model) model.Last.MAIL_SEND_ADDRESS, New With {.htmlAttributes = New With {.maxlength = 1000, .style = "width:790px;", .oninput = "editPassword(this)", .autocomplete = "off"}})</td>
             </tr>
             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_MAINTENANCE)</td>
                 <td width="20"></td>
                 <td class="head-col" style="width:100px;">@Message.getMessage(CommonLabel.LABEL_LEASE_COUNT)</td>
                 <td class="condition-col" style="width:120px;">
                     @Html.TextBoxFor(Function(model) model.Last.MAINTE_LEASE_NUM, New With {.maxlength = 4, .style = "width:100px; text-align:right;", .oninput = "editNumber(this);"})
                 </td>
                 <td class="head-col" style="width:100px;">@Message.getMessage(CommonLabel.LABEL_LEASE_DAY)</td>
                 <td class="condition-col" style="width:120px;">@Html.TextBoxFor(Function(model) model.Last.MAINTE_LEASE_DAY, New With {.maxlength = 4, .style = "width:100px; text-align:right;", .oninput = "editNumber(this);"})</td>
                 <td class="condition-col" style="width:auto;">@Html.CheckBoxFor(Function(model) model.Last.MainteLeaseDayWorkFlg) @Message.getMessage(CommonLabel.LABEL_WORK_FLAG)</td>
             </tr>
             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_MAINTENANCE_DAY)</td>
                 <td width="20"></td>
                 <td class="condition-col" style="width:120px;">@Html.TextBoxFor(Function(model) model.Last.MAINTE_DAY, New With {.maxlength = 4, .style = "width:100px; text-align:right;", .oninput = "editNumber(this);"})</td>
                 <td class="condition-col" style="width:auto;">@Html.CheckBoxFor(Function(model) model.Last.MainteDayWorkFlg) @Message.getMessage(CommonLabel.LABEL_WORK_FLAG)</td>
             </tr>
             @* Add End 2021.08.23 Iwamoto *@

             @* 2023/02/08 ADD Start Shirakawa*@
             <tr Class="popup-row">
                 <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_SPECIFIED_QTY)</td>
                 <td width="20"></td>
                 <td class="condition-col" style="width:120px;">@Html.TextBoxFor(Function(model) model.Last.SPECIFIED_QTY, New With {.maxlength = 9, .style = "width:100px; text-align:right;", .oninput = "editNumberDicimal(this);"})</td>
             </tr>
             @* 2023/02/08 ADD End Shirakawa*@
         </Table>

            @*<div Class="PopupButton" style="margin:0 auto; width:500px;"> yamada*@
            <div Class="PopupButton" style="margin:0 auto; width:550px;">
                <span style="float:left;margin:15px 20px 0 60px;">
                    @*<input id="modal-create-btn" class="defButton modal-close modal-create-btn submit-button" formaction="~/Location/Create" type="submit" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)">*@
                    <input id="modal-create-btn" class="defButton  modal-create-btn submit-button" formaction="~/Location/Create" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnCreateModal_onclick()">
                </span>
                <span style="float:left;margin:15px 15% 0 0;">
                    <input id="modal-delete-btn" class="defButton submit-button" type="button" formaction="~/Location/Delete" name="Delete" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="return btnDelete_onclick()">
                </span>
                <span style="float:left;margin:15px 0%;">
                    @* 戻るボタン押下時、更新しないと次のモーダル表示がおかしくなるのでsubmitに変更*@
                    @*<input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">*@
                    <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
                </span>
            </div>
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
        @Html.HiddenFor(Function(model) model.Last.INS_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_TASK)
                End Using
</div>

