@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of LendingListViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingList.TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
End Code
<script>
    //
    //予約一覧画面
    //
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            //エラーメッセージ表示後、ライセンスコントローラーを呼び出す
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
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
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        //並べ替えの日付使用可否設定
        let lst = document.getElementById("search-sort");
        let checkBoxes = lst.getElementsByTagName("INPUT");
        let checkValue = '';
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                checkValue = checkBoxes[i].value;
            }
        }
        switch (checkValue) {
            case "@LendingListDateExtract.RealLendingPeriod":
                //実質貸出期間
                $("#SearchRealLendingPeriodFrom").prop('disabled', false);
                $("#SearchRealLendingPeriodTo").prop('disabled', false);
                $("#SearchRealLendingStartDate").prop('disabled', true);
                break;

            case "@LendingListDateExtract.SameDayShipping":
                //当日出荷
                $("#SearchRealLendingPeriodFrom").prop('disabled', true);
                $("#SearchRealLendingPeriodTo").prop('disabled', true);
                $("#SearchRealLendingStartDate").prop('disabled', true);
                break;

            case "@LendingListDateExtract.NextDayShipping":
                //翌日出荷
                $("#SearchRealLendingPeriodFrom").prop('disabled', true);
                $("#SearchRealLendingPeriodTo").prop('disabled', true);
                $("#SearchRealLendingStartDate").prop('disabled', true);
                break;

            case "@LendingListDateExtract.RealLendingStartDate":
                //実質貸出開始日
                $("#SearchRealLendingPeriodFrom").prop('disabled', true);
                $("#SearchRealLendingPeriodTo").prop('disabled', true);
                $("#SearchRealLendingStartDate").prop('disabled', false);
                break;
        }

        // 列定義
        let columns = [
            { id: "@ColKey.LendingList.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.CHECK_SELECT", 40), sortOrder: -1, formatter: LendingListCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.LendingList.RESERVE_ID", name: "@Message.getMessage(CommonLabel.LABEL_RESERVE_NO)", field: "RESERVE_ID", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.RESERVE_ID", 110), sortOrder: "@Model.FirstOrDefault.GridInfo("RESERVE_ID").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("RESERVE_ID").DispFlag", formatter: ReserveIdFormatter },
            { id: "@ColKey.LendingList.RESERVE_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)", field: "RESERVE_STATUS", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.RESERVE_STATUS", 110), sortOrder: "@Model.FirstOrDefault.GridInfo("RESERVE_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("RESERVE_STATUS").DispFlag", formatter: LeaseStatusFormatter },
            { id: "@ColKey.LendingList.CHARGE_EMP_CD", name: "@Message.getMessage(CommonLabel.LABEL_EMP)", field: "CHARGE_EMP_CD", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.CHARGE_EMP_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("CHARGE_EMP_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHARGE_EMP_CD").DispFlag", formatter: ChargeEmpCdFormatter },
            { id: "@ColKey.LendingList.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_MNG_LOC)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.MNG_LOC_L_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag", formatter: MngLocCdFormatter },
            { id: "@ColKey.LendingList.LEASE_PLACE_CD", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_NAME)", field: "LEASE_PLACE_CD", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.LEASE_PLACE_CD", 220), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispFlag", formatter: LeasePlaceCdFormatter },
            { id: "@ColKey.LendingList.DEST_NAME", name: "@Message.getMessage(CommonLabel.LABEL_SEND_NAME)", field: "DEST_NAME", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.DEST_NAME", 220), sortOrder: "@Model.FirstOrDefault.GridInfo("DEST_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DEST_NAME").DispFlag" },
            { id: "@ColKey.LendingList.REAL_START_DT", name: "@Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE)", field: "REAL_START_DT", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.REAL_START_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("REAL_START_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REAL_START_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingList.REAL_END_DT", name: "@Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE)", field: "REAL_END_DT", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.REAL_END_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("REAL_END_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REAL_END_DT").DispFlag", formatter: SlashYMDFormatter },
            { id: "@ColKey.LendingList.DEST_NO", name: "@Message.getMessage(CommonLabel.LABEL_SEND_DEST_NUMBER)", field: "DEST_NO", width: getColumnWidth("@DispId.LENDING_LIST", "@ColKey.LendingList.DEST_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("DEST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DEST_NO").DispFlag" },
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

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // 選択状態を復元
        let lstCheckedKey = @Html.Raw(jsonCheckKeys);
        for (let i = 0; i < lstCheckedKey.length; i++) {
            // Key1:予約番号
            var keys = lstCheckedKey[i].split(",");

            for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                if (keys[0] === String(jsonData[grid_i].RESERVE_ID)) {
                    jsonData[grid_i].DISP_FLAG = lstCheckedKey[i]
                    break;
                }
            }
        }

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // Grid表示判定
        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "RESERVE_ID");
            dataView.endUpdate();
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");

        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列と変更列を加算
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.LENDING_LIST", fixCol);

    });

    // チェックボックス(中央寄せ)
    function LendingListCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let isKeyInclude = (dataContext[columnDef.field] === String(dataContext.RESERVE_ID));

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheck" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + convertToEmptyIfNull(dataContext.RESERVE_ID) + '" '+
               (isKeyInclude ? 'checked="checked" ' : '') +
               'onchange="check_onclick(this)" ' +
               '/>' +
               '</div>';
    }

    // 予約番号formatter
    function ReserveIdFormatter(row, cell, value, columnDef, dataContext) {
        // 引き渡し用隠し項目のid, name
        let sSelectMngLocCd = "SelectMngLocCd" + dataContext.RESERVE_ID
        let sSelectReqEmpCd = "SelectReqEmpCd" + dataContext.RESERVE_ID

        return '<a href="#" onclick="lnkReserveId_onclick(this.text)" class="blue-link">' + value + '</a>' +
               '<input id="' + sSelectMngLocCd + '" name="' + sSelectMngLocCd + '" type="hidden" value="' + dataContext.MNG_LOC_L_CD + '">' +
               '<input id="' + sSelectReqEmpCd + '" name="' + sSelectReqEmpCd + '" type="hidden" value="' + dataContext.REQ_EMP_CD + '">';
    }

    // 貸出状態formatter
    function LeaseStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch (String(dataContext.RESERVE_STATUS)) {
            case "@ReserveStatus.Mi":
                status = "@Message.getMessage(CommonLabel.VALUE_NOT_RENTED)"
                break;
            case "@ReserveStatus.Lending":
                status = "@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)"
                break;
            case "@ReserveStatus.HenkyakuSumi":
                status = "@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)"
                break;
            default:
                status = dataContext.RESERVE_STATUS;
                break;
        }

        return status;
    }

    // 担当者formatter
    function ChargeEmpCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CHARGE_EMP_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CHARGE_EMP_NAME)
    }

    // 管理ロケーションformatter
    function MngLocCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.MNG_LOC_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.MNG_LOC_L_NAME)
    }

    // 貸出先名formatter
    function LeasePlaceCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LEASE_PLACE_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LEASE_PLACE_NAME)
    }

    // yyyyMMdd→yyyy/MM/dd表示用formatter
    function SlashYMDFormatter(row, cell, value, columnDef, dataContext) {
        return toStringDateYMD(value);
    }

    //送り状作成ボタンクリック処理
    function btnLendingInvoice_onclick() {
        document.getElementById("popup-title-invoice").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SELECT_INVOICE)";

        //チェックボックスの状態からキーを設定する
        setting_checklist();
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        // ダイアログのメッセージを設定
        $("#show-dialog").html("@Message.getMessage(CommonLabel.MSG_INFO_INVOICE_CONFIRM)");

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $(this).dialog("close");

                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/LendingList")/ShowInvoiceList',
                        dataType: 'json',
                        data: { SelectKeys: document.getElementById("SelectKeys").value, NoneSelectKeys: document.getElementById("NoneSelectKeys").value },
                        cache: false,
                        success: function (data, dataType) {
                            if (data == null) {
                                return;
                            } else {
                                var InvoiceKey = document.getElementById("InvoiceKey");
                                InvoiceKey.options.length = 0;
                                for (var i = 0; i < data.length; i++) {
                                    let op = document.createElement("option");
                                    op.value = data[i].KEY1;
                                    op.text = data[i].DATA1;
                                    document.getElementById("InvoiceKey").appendChild(op);
                                }
                                if (data.length == 1) {
                                    document.getElementById("InvoiceKey").value = data[0].KEY1;
                                    btnInvoiceOK_onclick();
                                } else {
                                    //モーダルウインドウを開く
                                    ShowInvoiceModalWindow();
                                }
                            }
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

    // 貸出確認書作成ボタンクリック処理
    function btnOutputLendingConfirm_onclick() {
        //チェックボックスの状態からキーを設定する
        setting_checklist();
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        // 100件以上選択されていた場合、警告メッセージを出す
        if (document.getElementById("selectcount").innerHTML >= 100) {
            // 100件以上選択されていた場合
            //確認メッセージ
            return showConfirm("@Message.getMessage(CommonLabel.MSG_INFO_LENDING_CONFIRM_TAKE_TIME)", "@Url.Content("~/LendingList")/OutputLendingConfirm");
        } else {
            //確認メッセージ
            return showConfirm("@Message.getMessage(CommonLabel.MSG_INFO_LENDING_CONFIRM)", "@Url.Content("~/LendingList")/OutputLendingConfirm");
        }
    }

    //貸出ラベル発行ボタンクリック処理
    function btnLendingLabel_onclick() {
        document.getElementById("popup-title-prt").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SELECT_PRINTER)";

        //チェックボックスの状態からキーを設定する
        setting_checklist();
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        // ダイアログのメッセージを設定
        $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.HAKKO_CONFIRM)");

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $(this).dialog("close");

                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/LendingList")/ShowPrinterList',
                        dataType: 'json',
                        data: { SelectKeys: document.getElementById("SelectKeys").value, NoneSelectKeys: document.getElementById("NoneSelectKeys").value },
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
                                    op.text = data[i].DATA2;
                                    document.getElementById("PrinterKey").appendChild(op);
                                }
                                if (data.length == 1) {
                                    document.getElementById("PrinterKey").value = data[0].KEY1;
                                    btnPrinterOK_onclick();
                                } else {
                                    //モーダルウインドウを開く
                                    ShowPRTModalWindow();
                                }
                            }
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

    // 送り状I/Fの種類選択選択
    function btnInvoiceOK_onclick() {
        var prtKey = document.getElementById("InvoiceKey").value;
        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/LendingList")/OutputLendingInvoice");
        $('#form').submit();
        $('#form').attr("action", befAction);
    }

    // プリンタ選択
    function btnPrinterOK_onclick() {
        var prtKey = document.getElementById("PrinterKey").value;
        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/LendingList")/OutputLendingLabel");
        $('#form').submit();
        $('#form').attr("action", befAction);
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //管理ロケーションのチェック状態
        var keys = [];
        var lst = document.getElementById("search-mng-loc");
        var checkBoxes = lst.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                keys.push(checkBoxes[i].value);
            }
        }
        //管理ロケーションが未選択の場合はエラー
        if (keys.length == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_MNG_LOC)")
            return false;
        }
        // カンマ区切りの文字列を設定
        document.getElementById("SelectMngLocKeys").value = keys.join(",");

        //貸出状態のチェック状態
        var keysLeaseStatus = [];
        var lstLeaseStatus = document.getElementById("search-lease-status");
        var checkLeaseStatus = lstLeaseStatus.getElementsByTagName("INPUT");
        for (var i = 0; i < checkLeaseStatus.length; i++) {
            if (checkLeaseStatus[i].checked) {
                keysLeaseStatus.push(checkLeaseStatus[i].value);
            }
        }
        //貸出状態が未選択の場合はエラー
        if (keysLeaseStatus.length == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_LEASE_STATUS)")
            return false;
        }

        //並べ替え
        let lstSort = document.getElementById("search-sort");
        let checkBoxesSort = lstSort.getElementsByTagName("INPUT");
        let checkSort = '';
        for (var i = 0; i < checkBoxesSort.length; i++) {
            if (checkBoxesSort[i].checked) {
                checkSort = checkBoxesSort[i].value;
            }
        }
        switch (checkSort) {
            case "@LendingListDateExtract.RealLendingPeriod":
                //実質貸出期間
                //必須入力チェック
                let realLendingPeriodFrom = $("#SearchRealLendingPeriodFrom").val();
                let realLendingPeriodTo = $("#SearchRealLendingPeriodTo").val();
                if (realLendingPeriodFrom == "" && realLendingPeriodTo == "") {
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_ENTERED_PERIOD_FROM_OR_TO)")
                    return false;
                }
                //前後チェック
                if (realLendingPeriodFrom != "" && realLendingPeriodTo != "") {
                    realLendingPeriodFrom = parseFloat(realLendingPeriodFrom.replace(/-/g, ""));
                    realLendingPeriodTo = parseFloat(realLendingPeriodTo.replace(/-/g, ""));
                    if (realLendingPeriodFrom > realLendingPeriodTo) {
                        @* '--2021/12/14 nv miyaji update start *@
                        showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_REAL_END_DT_KAKOBI)")
                        @*showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")*@
                        @* '--2021/12/14 nv miyaji update end *@
                        return false;
                    }
                }
                break;

            case "@LendingListDateExtract.RealLendingStartDate":
                //実質貸出開始日
                let realLendingStartDate = $("#SearchRealLendingStartDate").val();
                //必須入力チェック
                if (realLendingStartDate == "") {
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_REAL_START_DT_EMPTY)")
                    return false;
                }
                break;
        }

        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/LendingList")/CsvExport");
    }

    //全選択ボタンクリック時処理
    function btnSelectListAll_onclick() {
        var chkFlag = false;
        var btn = document.getElementById("btnSelectListAll");
        if (btn.value == "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)") {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_OFF)";
            $('input[name="checkEdit"]').prop('checked', true)
            chkFlag = true;
        } else {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
            $('input[name="checkEdit"]').prop('checked', false)
        }

        //非同期で現在の検索条件での全件を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingList")/SelectListAll',
            data: { blnSelectListAll: chkFlag },
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

    //検索ボタンクリック処理
    function btnSearch_onclick(element) {
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        //管理ロケーションのチェック状態
        var keys = [];
        var lst = document.getElementById("search-mng-loc");
        var checkBoxes = lst.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                keys.push(checkBoxes[i].value);
            }
        }
        //管理ロケーションが未選択の場合はエラー
        if (keys.length == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_MNG_LOC)")
            return false;
        }
        // カンマ区切りの文字列を設定
        document.getElementById("SelectMngLocKeys").value = keys.join(",");

        //貸出状態のチェック状態
        var keysLeaseStatus = [];
        var lstLeaseStatus = document.getElementById("search-lease-status");
        var checkLeaseStatus = lstLeaseStatus.getElementsByTagName("INPUT");
        for (var i = 0; i < checkLeaseStatus.length; i++) {
            if (checkLeaseStatus[i].checked) {
                keysLeaseStatus.push(checkLeaseStatus[i].value);
            }
        }
        //貸出状態が未選択の場合はエラー
        if (keysLeaseStatus.length == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_LEASE_STATUS)")
            return false;
        }

        //並べ替え
        let lstSort = document.getElementById("search-sort");
        let checkBoxesSort = lstSort.getElementsByTagName("INPUT");
        let checkSort = '';
        for (var i = 0; i < checkBoxesSort.length; i++) {
            if (checkBoxesSort[i].checked) {
                checkSort = checkBoxesSort[i].value;
            }
        }
        switch (checkSort) {
            case "@LendingListDateExtract.RealLendingPeriod":
                //実質貸出期間
                //必須入力チェック
                let realLendingPeriodFrom = $("#SearchRealLendingPeriodFrom").val();
                let realLendingPeriodTo = $("#SearchRealLendingPeriodTo").val();
                if (realLendingPeriodFrom == "" && realLendingPeriodTo == "") {
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_ENTERED_PERIOD_FROM_OR_TO)")
                    return false;
                }
                //前後チェック
                if (realLendingPeriodFrom != "" && realLendingPeriodTo != "") {
                    realLendingPeriodFrom = parseFloat(realLendingPeriodFrom.replace(/-/g, ""));
                    realLendingPeriodTo = parseFloat(realLendingPeriodTo.replace(/-/g, ""));
                    if (realLendingPeriodFrom > realLendingPeriodTo) {
                        @* '--2021/12/14 nv miyaji update start *@
                        showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_REAL_END_DT_KAKOBI)")
                        @*showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")*@
                        @* '--2021/12/14 nv miyaji update end *@
                        return false;
                    }
                }
                break;

            case "@LendingListDateExtract.RealLendingStartDate":
                //実質貸出開始日
                let realLendingStartDate = $("#SearchRealLendingStartDate").val();
                //必須入力チェック
                if (realLendingStartDate == "") {
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_REAL_START_DT_EMPTY)")
                    return false;
                }
                break;
        }

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

    //全選択ボタンクリック処理
    function btnSelectAll_onclick() {
        var chkFlag = false;
        var btn = document.getElementById("btnSelectAll");
        if (btn.value == "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)") {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_OFF)";
            chkFlag = true;
        } else {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
            chkFlag = false;
        }

        var lst = document.getElementById("search-mng-loc");
        var checkBoxes = lst.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            checkBoxes[i].checked = chkFlag;
        }

        //非同期で現在のチェック状態をセッションに保存する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingList")/SelectAll',
            data: { isSelectAll: chkFlag },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //予約番号リンククリック処理
    function lnkReserveId_onclick(value) {
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        //予約番号関連情報をカンマ区切りで作成します
        var mngLocCd = document.getElementById("SelectMngLocCd" + value).value;
        var reqEmpCd = document.getElementById("SelectReqEmpCd" + value).value;
        document.getElementById("SelectReserveInfo").value = value + "," + mngLocCd + "," + reqEmpCd;

        $('#form').attr("action", "@Url.Content("~/LendingList")/SetReserveId");
        $('#form').submit();
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        document.getElementById("PageNum").value = element.name;
        setting_checklist();

        $('#form').submit();
    }

    //並べ替えラジオボタンクリック処理
    function selectSearchSort_onclick(element) {
        switch (element.value) {
            case "@LendingListDateExtract.RealLendingPeriod":
                //実質貸出期間
                $("#SearchRealLendingPeriodFrom").prop('disabled', false);
                $("#SearchRealLendingPeriodTo").prop('disabled', false);
                $("#SearchRealLendingStartDate").prop('disabled', true);
                break;

            case "@LendingListDateExtract.SameDayShipping":
                //当日出荷
                $("#SearchRealLendingPeriodFrom").prop('disabled', true);
                $("#SearchRealLendingPeriodTo").prop('disabled', true);
                $("#SearchRealLendingStartDate").prop('disabled', true);
                break;

            case "@LendingListDateExtract.NextDayShipping":
                //翌日出荷
                $("#SearchRealLendingPeriodFrom").prop('disabled', true);
                $("#SearchRealLendingPeriodTo").prop('disabled', true);
                $("#SearchRealLendingStartDate").prop('disabled', true);
                break;

            case "@LendingListDateExtract.RealLendingStartDate":
                //実質貸出開始日
                $("#SearchRealLendingPeriodFrom").prop('disabled', true);
                $("#SearchRealLendingPeriodTo").prop('disabled', true);
                $("#SearchRealLendingStartDate").prop('disabled', false);
                break;
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

    //管理ロケーションチェックボックスの選択状態を設定する
    function setting_checkMngLocList() {
        //管理ロケーションのチェック状態
        var keys = [];
        var lst = document.getElementById("search-mng-loc");
        var checkBoxes = lst.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                keys.push(checkBoxes[i].value);
            }
        }

        // カンマ区切りの文字列を設定
        document.getElementById("SelectMngLocKeys").value = keys.join(",");

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

<div id="lending-list">
    @Using (Html.BeginForm("Index", "LendingList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
    @Html.AntiForgeryToken()
    @<div class="view-area mb20">
        <div id="view-area-button-parent">
            <div class="view-area-button-left">
                @* 2021/12/06 nv miyaji update start *@
                @if ViewBag.LendingInvoiceFunctionFlg = True Then
                @<input class="defButton" type="button" style="width:200px;" value="@Message.getMessage(CommonLabel.BUTTON_OUTPUT_INVOICE)" onclick="btnLendingInvoice_onclick()">
                     End If
                @if ViewBag.LendingConfirmOutputFlg = True Then
                @<input Class="defButton" type="button" style="width:200px;" value="@Message.getMessage(CommonLabel.BUTTON_OUTPUT_LENDING_CONFIRM)" onclick="btnOutputLendingConfirm_onclick()">
                     End If
                @if ViewBag.LendingLabelFunctionFlg = True Then
                @<input class="defButton" type="button" style="width:200px;" value="@Message.getMessage(CommonLabel.BUTTON_OUTPUT_LENDING_LABEL)" onclick="btnLendingLabel_onclick()">
                     End If
                @*<input class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_OUTPUT_INVOICE)" onclick="btnLendingInvoice_onclick()">*@
                @*<input class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_OUTPUT_LENDING_CONFIRM)" onclick="btnOutputLendingConfirm_onclick()">*@
                @*<input class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_OUTPUT_LENDING_LABEL)" onclick="btnLendingLabel_onclick()">*@
                @* 2021/12/06 nv miyaji update end *@
                @* 全選択/全解除ボタンの表記文字設定 *@
                @Code
                         Dim AllListSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG_SECOND)
                         Dim sListDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                         If AllListSelectFlg Then
                             sListDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                         End If
                End Code
                <input class="defButton" type="button" id="btnSelectListAll" value="@sListDispStr" onclick="btnSelectListAll_onclick()">
            </div>
            <div class="view-area-button-right">
                <input class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
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
                        @* 担当者 *@
                        <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CHARGE_EMP_CD").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_EMP)</div>
                            <div class="grid-condition-col">
                                @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchEmpName, New With {.class = "cond-control", .maxlength = 60})
                            </div>
                        </div>
                        @* 申請者 *@
                        <div class="col-sm-2">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_REQ_EMP)</div>
                            <div class="grid-condition-col">
                                @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchReqEmpName, New With {.class = "cond-control", .maxlength = 60})
                            </div>
                        </div>
                        @* 予約番号 *@
                        <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("RESERVE_ID").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_RESERVE_NO)</div>
                            <div class="grid-condition-col">
                                @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchReserveId, New With {.class = "cond-control", .maxlength = 18})
                            </div>
                        </div>
                        @* 貸出状態 *@
                        <div class="col-sm-3" style="max-width:450px;">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)</div>
                            <div id="search-lease-status" class="grid-condition-col">
                                <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_NotRented) @Message.getMessage(CommonLabel.VALUE_NOT_RENTED)</div>
                                <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_Lending) @Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)</div>
                                <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_Returned) @Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)</div>
                                <div class="input-select-item-last">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_ReturnDelay) @Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETUEN_DELAY)</div>
                            </div>
                        </div>
                    </div>
                </div>
                @* 日付抽出 *@
                <div class="row" id="item-conditions">
                    <div class="col-md-12">
                        <div class="col-sm-5" style="max-width:750px;">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_DATE_EXTRACT)</div>
                            <div id="search-sort" class="grid-condition-col">
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDateExtract, LendingListDateExtract.RealLendingPeriod, New With {.onclick = "selectSearchSort_onclick(this)"}) @Message.getMessage(CommonLabel.LABEL_LEASE_KIKAN_JISSHITSU)</div>
                                <div class="input-select-item" style="margin-bottom:5px;">

                                    <div class="grid-condition-col-common">
                                        @Html.EditorFor(Function(model) model.FirstOrDefault.SearchRealLendingPeriodFrom, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                        ～
                                        @Html.EditorFor(Function(model) model.FirstOrDefault.SearchRealLendingPeriodTo, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                    </div>

                                </div>
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDateExtract, LendingListDateExtract.SameDayShipping, New With {.onclick = "selectSearchSort_onclick(this)"}) @Message.getMessage(CommonLabel.LABEL_SAME_DAY_SHIPPING)</div>
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDateExtract, LendingListDateExtract.NextDayShipping, New With {.onclick = "selectSearchSort_onclick(this)"}) @Message.getMessage(CommonLabel.LABEL_NEXT_DAY_SHIPPING)</div>
                                @If (Model.FirstOrDefault.GridInfo("REAL_START_DT").DispFlag = "1") Then
                                    @<div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDateExtract, LendingListDateExtract.RealLendingStartDate, New With {.onclick = "selectSearchSort_onclick(this)"}) @Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE)</div>
                                    @<div class="input-select-item-last">
                                <div class="grid-condition-col-common">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchRealLendingStartDate, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                </div>
                              
                                </div>
                                End If
                            </div>
                        </div>
                    </div>
                </div>
                @* 管理ロケーション *@
                <div class="row" id="item-conditions">
                    <div class="col-md-12">
                        <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_MNG_LOC)</div>
                            <td class="condition-col" style="width:300px;">
                                <div id="search-mng-loc" class="box-srcollbar">
                                    @Code
                                        If Model.FirstOrDefault.mngLocCheckBoxListData IsNot Nothing Then
                                            Dim selectMngLocKeys As String() = New String() {}
                                            If Model.FirstOrDefault.SelectMngLocKeys IsNot Nothing Then
                                                selectMngLocKeys = Model.FirstOrDefault.SelectMngLocKeys.Split(",")
                                            End If
                                            For Each item In Model.FirstOrDefault.mngLocCheckBoxListData
                                                Dim isChecked As Boolean = False
                                                For Each key As String In selectMngLocKeys
                                                    If item.LOC_CD = key Then
                                                        isChecked = True
                                                        Exit For
                                                    End If
                                                Next
                                                If isChecked Then
                                                    @<input type="checkbox" name="checkMngLoc" value="@item.LOC_CD" checked />@item.LOC_NAME@<br />
                                                Else
                                                    @<input type="checkbox" name="checkMngLoc" value="@item.LOC_CD" />@item.LOC_NAME@<br />
                                                End If
                                            Next
                                        End If
                                    End Code
                                </div>
                            </td>
                            <div class="condition-col" style="width:150px;">
                                @* 全選択/全解除ボタンの表記文字設定 *@
                                @Code
                                    Dim AllSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG)
                                    Dim sDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                                    If AllSelectFlg Then
                                        sDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                                    End If
                                End Code
                                <input id="btnSelectAll" Class="defButton submit-button" type="button" value="@sDispStr" onclick="return btnSelectAll_onclick()" />
                            </div>
                        </div>
                    </div>
                </div>
                @* 検索ボタン *@
                <div Class="search-btn">
                    <input id="btn-search" class="defButton submit-button" name="actionname" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick(this)" />
                </div>
            </div>
        </div>
    </div>

    @*管理ロケーションチェックボックス選択値の引き渡し用隠し項目*@
    @<input id="SelectMngLocKeys" name="SelectMngLocKeys" type="hidden">
    @*明細データチェックボックス選択値の引き渡し用隠し項目*@
    @<input id="SelectKeys" name="SelectKeys" type="hidden">
    @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden">
    @*予約番号関連情報引き渡し用隠し項目*@
    @<input id="SelectReserveInfo" name="SelectReserveInfo" type="hidden">


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
    @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden" />

    @*送り状I/F情報選択画面*@
    @<div id="modal-invoice-content">
        <div class="popup-title-wrap">
            <p class="Line3"></p>
            <p class="Line4"></p>
            <h2 id="popup-title-invoice"></h2>
            <p class="Line3"></p>
            <p class="Line4"></p>
        </div>
        @CODE
            'モデルの値を設定
            Dim invoiceSessionModel As LendingListViewModel = Session(CommonSessionKey.CREAGE_LENDING_LIST)
            If Not IsNothing(invoiceSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                Model.Add(invoiceSessionModel)
            Else
                Model.Add(New LendingListViewModel())
            End If
        END CODE
        <br />
        <div class="mb10">
            @Html.DropDownListFor(Function(model) model.FirstOrDefault.InvoiceKey, DirectCast(ViewBag.modelInvoice, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 300px;"})
        </div>
        <br />
        <div Class="PopupButton float-left">
            <input id="modal-ok" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_OK)" onclick="btnInvoiceOK_onclick()">
        </div>
        <div Class="PopupButton float-right">
            <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_CANCEL)">
        </div>
    </div>


    @*プリンタ画面*@
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
                Dim prtSessionModel As LendingListViewModel = Session(CommonSessionKey.CREAGE_LENDING_LIST)
                If Not IsNothing(prtSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(prtSessionModel)
                Else
                    Model.Add(New LendingListViewModel())
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
            <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_CANCEL)">
        </div>
    </div>



    @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
    @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
    @*@Html.HiddenFor(Function(model) model.Last.INS_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_TASK)*@
    End Using
</div>
