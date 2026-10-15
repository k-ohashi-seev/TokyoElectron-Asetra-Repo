@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of InventorySummaryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
End Code
<script>
    //
    //  棚卸進捗状況
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

        }
        //20230904 ADD End Shirakawa

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessCreate") {
            //登録
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_INV_START)")

        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessEdit") {
            //変更
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_INV_START)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.ErrorInvKakutei") {
            //棚卸確定エラー
            var err = "@Message.getMessage(TempData("ErrCode"))";
            if (err) {
                //エラーメッセージ表示
                showErrMessage(err, "#@Model.FirstOrDefault.ErrTarget")
            }
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessInvKakutei") {
            @*//棚卸確定
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_INV_KAKUTEI)")*@

            // ダイアログのメッセージを設定
            strMessage = "@Message.getMessage(CommonLabel.MSG_INVENTRY_RESULT_OUTPUT)".replace("@@@@CR@@@@", "<BR>");   //　2020/02/27 nv miyaji add
            $("#show-dialog").html(strMessage);

            // ダイアログを作成
            $("#show-dialog").dialog({
                modal: true,
                title: strConfirmTitle,
                dialogClass: "show-confirm-wrap",
                buttons: {
                    "OK": function () {
                        $(this).dialog("close");

                        showLoading();

                        // フォームを取得
                        var $form = $('#form');

                        // 変更前に退避
                        var befAction = $form.attr("action");
                        var befTarget = $form.attr("target");

                        // ダウンロード用に target を iframe に変更
                        $form.attr("target", "downloadFrame");

                        // ダウンロード完了待ち処理
                        waitDownloadComplete($form, befTarget);

                        var gridData = grid.getData();

                        //一覧の全項目データ
                        var item = grid.getDataItem(0)

                        $form.attr("action", "@Url.Content("~/InventorySummary")/InvResultExport");
                        $form.submit();
                        $form.attr("action", befAction);

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

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
            //変更画面を表示
            document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_NAME_TITLE)";
            ShowModalWindow()
        }


        //列定義
        let columns = [
            { id: "@ColKey.InventorySummary.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.CHECK_SELECT", 40), sortOrder: -2, formatter: InventorySummaryCheckboxFormatter, cssClass: "first-column",visible: false },
            { id: "@ColKey.InventorySummary.INV_LOC_L_KUBUN", name: '@Message.getMessage(CommonLabel.LABEL_INV_LOC_L_KUBUN)', field: "LOC_L_CD", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.INV_LOC_L_KUBUN", 300), sortOrder: "@Model.FirstOrDefault.GridInfo("INV_LOC_L_KUBUN").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INV_LOC_L_KUBUN").DispFlag", formatter: LocCdNameFormatter },
            { id: "@ColKey.InventorySummary.INVENTORY_STATUIS", name: '@Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STATUIS)', field: "INVENTORY_STATUIS", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.INVENTORY_STATUIS", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("INVENTORY_STATUIS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INVENTORY_STATUIS").DispFlag", formatter: InventoryStatusFormatter },
            { id: "@ColKey.InventorySummary.INVENTORY_STARTDAY", name: '@Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTDAY)', field: "INVENTORY_STARTDAY", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.INVENTORY_STARTDAY", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("INVENTORY_STARTDAY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INVENTORY_STARTDAY").DispFlag", formatter: InventoryStartDayFormatter },
            { id: "@ColKey.InventorySummary.BOOK_QUANTITY", name: '@Message.getMessage(CommonLabelInventorySummary.LABEL_BOOK_QUANTITY)', field: "BOOK_QUANTITY", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.BOOK_QUANTITY", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_QUANTITY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_QUANTITY").DispFlag", formatter: BookInventoryNumberFormatter },
            { id: "@ColKey.InventorySummary.NUMBER_OF_LOANS_WITHIN", name: '@Message.getMessage(CommonLabelInventorySummary.LABEL_NUMBER_OF_LOANS_WITHIN)', field: "NUMBER_OF_LOANS_WITHIN", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.NUMBER_OF_LOANS_WITHIN", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("NUMBER_OF_LOANS_WITHIN").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("NUMBER_OF_LOANS_WITHIN").DispFlag", formatter: NumberOfLoansWithinFormatter },
            { id: "@ColKey.InventorySummary.ACTUAL_NUMBER_OF_SHELVES", name: '@Message.getMessage(CommonLabelInventorySummary.LABEL_ACTUAL_NUMBER_OF_SHELVES)', field: "ACTUAL_NUMBER_OF_SHELVES", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.ACTUAL_NUMBER_OF_SHELVES", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("ACTUAL_NUMBER_OF_SHELVES").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ACTUAL_NUMBER_OF_SHELVES").DispFlag", formatter: ActualNumberOfShelvesFormatter },
            { id: "@ColKey.InventorySummary.NORMAL_NUMBER", name: '@Message.getMessage(CommonLabelInventorySummary.LABEL_NORMAL_NUMBER)', field: "NORMAL_NUMBER", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.NORMAL_NUMBER", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("NORMAL_NUMBER").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("NORMAL_NUMBER").DispFlag", formatter: NormalNumberFormatter },
            { id: "@ColKey.InventorySummary.LOCATION_DIFFERENCE_NUMBER", name: '@Message.getMessage(CommonLabelInventorySummary.LABEL_LOCATION_DIFFERENCE_NUMBER)', field: "LOCATION_DIFFERENCE_NUMBER", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.LOCATION_DIFFERENCE_NUMBER", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("LOCATION_DIFFERENCE_NUMBER").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOCATION_DIFFERENCE_NUMBER").DispFlag", formatter: LocationDifferenceNumberFormatter },
            { id: "@ColKey.InventorySummary.OUT_OF_STOCK", name: '@Message.getMessage(CommonLabelInventorySummary.LABEL_OUT_OF_STOCK)', field: "OUT_OF_STOCK", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.OUT_OF_STOCK", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("OUT_OF_STOCK").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("OUT_OF_STOCK").DispFlag", formatter: OutOfStockFormatter },
            { id: "@ColKey.InventorySummary.NEAR_INVENTORY_DATE", name: '@Message.getMessage(CommonLabel.LABEL_NEAR_INVENTORY_DATE)', field: "NEAR_INVENTORY_DATE", width: getColumnWidth("@DispId.INVENTORY_SUMMARY", "@ColKey.InventorySummary.NEAR_INVENTORY_DATE", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("NEAR_INVENTORY_DATE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("NEAR_INVENTORY_DATE").DispFlag", formatter: NearInventoryDateFormatter },
            { id: "UPD_FLG", name: 'UPD_FLG', field: "UPD_FLG", width: 50, sortOrder: -1, visible: false },
            { id: "INV_NAME_LIST", name: 'INV_NAME_LIST', field: "INV_NAME_LIST", width: 50, sortOrder: -1, visible: false }
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
            fixCol = fixCol + 1      //選択列を加算
        }

        checkedList = []

        @*// 選択状態を復元
        let lstCheckedKey = @Html.Raw(jsonCheckKeys);
        if (lstCheckedKey) {
            let keys = []

            for (let i = 0; i < lstCheckedKey.length; i++) {
                // Key1:棚卸ID Key2:ロケーション大区分コード
                var splitKeys = lstCheckedKey[i].split("/");
                keys.push(lstCheckedKey[i])

                for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                    if (splitKeys[0] === String(jsonData[grid_i].INV_ID) && splitKeys[1] === String(jsonData[grid_i].LOC_L_CD)) {
                        jsonData[grid_i].DISP_FLAG = lstCheckedKey[i]
                        break;
                    }
                }
            }

            //チェックしたキー
            document.getElementById("SelectKeys").value = keys;
        }*@


        let data = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.INVENTORY_SUMMARY", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, "@DispId.INVENTORY_SUMMARY", fixCol);
        }

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

        //ローカルストレージから復元
        const saved = localStorage.getItem("InventorySummarySearchConditions");
        if ("@Model.FirstOrDefault.IsRestoreOwnSearchConditions" == "True") {
            var lcd = document.getElementById("SearchLocL");
            //最初の要素（×での選択解除用に追加した空の選択肢は除く）
            var firstLoc = $(lcd).find("option").filter(function () { return this.value !== ""; })[0];
            if (saved) {
                const cond = JSON.parse(saved);
                //ローカルストレージが空の場合は最初の要素を選択
                if (!cond.SearchLocL || cond.SearchLocL.trim() === "") {
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
            //※trigger("change.select2") だと onchange（ChangeLocationL）も呼ばれるため refreshSelect2 を使用
            refreshSelect2(lcd);

            //ロケ大の値が変わった時のメソッド実行
            ChangeLocationL(lcd, true);
        }


    });

    //ローカルストレージ保存
    function localStorageSave() {
        let cond = { SearchLocL: document.getElementById("SearchLocL").value };

        //ローカルストレージを保存
        localStorage.setItem("InventorySummarySearchConditions", JSON.stringify(cond));

    }

    //選択用formatter
    function InventorySummaryCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.INV_ID) + '/' + convertToEmptyIfNull(dataContext.LOC_L_CD)
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;">' +
               '<input type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" ' +
               (isKeyInclude ? 'checked="checked"' : '') + ' />' +
               '</div>';
    }

    //ロケ大区分用formatter
    function LocCdNameFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(dataContext.LOC_L_CD) != '') {
            return dataContext.LOC_L_CD + ' ' + dataContext.LOC_L_NAME
        }
    }

    //棚卸開始日用formatter
    function InventoryStartDayFormatter(row, cell, value, columnDef, dataContext) {
        let inventoryStartday = dataContext.INVENTORY_STARTDAY

        if (inventoryStartday != 0) {
            let status = convertToEmptyIfNull(inventoryStartday).toString()
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            let hhmm = status.substr(8, 2) + ':' + status.substr(10, 2)
            inventoryStartday = yyyy + '/' + mm + '/' + dd + ' ' + hhmm
        } else if (dataContext.INVENTORY_STATUS != '@Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTED)') {
            inventoryStartday = '-'
        }
        return '<div style="text-align:center; width:100%;">' + inventoryStartday + '</div>'
    }

    //棚卸状態用formatter
    function InventoryStatusFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:center; width:100%;">' + dataContext.INVENTORY_STATUS + '</div>'
    }

    //帳簿在庫数formatter
    function BookInventoryNumberFormatter(row, cell, value, columnDef, dataContext) {
        let status = dataContext.BOOK_INVENTORY_NUMBER
        if (dataContext.INVENTORY_STATUS != '@Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTED)') {
            status = '0'
        }
        return '<div style="text-align:right; width:100%;">' + status + '</div>'
    }

    //内貸出数用formatter
    function NumberOfLoansWithinFormatter(row, cell, value, columnDef, dataContext) {
        let status = dataContext.NUMBER_OF_LOANS_WITHIN
        if (dataContext.INVENTORY_STATUS != '@Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTED)') {
            status = '0'
        }
        return '<div style="text-align:right; width:100%;">' + status + '</div>'
    }

    //実棚数用formatter
    function ActualNumberOfShelvesFormatter(row, cell, value, columnDef, dataContext) {
        let status = dataContext.ACTUAL_NUMBER_OF_SHELVES
        if (dataContext.INVENTORY_STATUS != '@Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTED)') {
            status = '0'
        }
        return '<div style="text-align:right; width:100%;">' + status + '</div>'
    }

    //正常数用formatter
    function NormalNumberFormatter(row, cell, value, columnDef, dataContext) {
        let status = dataContext.NORMAL_NUMBER
        if (dataContext.INVENTORY_STATUS != '@Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTED)') {
            status = '0'
        }
        return '<div style="text-align:right; width:100%;">' + status + '</div>'
    }

    //ロケ違い数用formatter
    function LocationDifferenceNumberFormatter(row, cell, value, columnDef, dataContext) {
        let status = dataContext.LOCATION_DIFFERENCE_NUMBER
        if (dataContext.INVENTORY_STATUS != '@Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTED)') {
            status = '0'
        }
        return '<div style="text-align:right; width:100%;">' + status + '</div>'
    }

    //在庫なし数用formatter
    function OutOfStockFormatter(row, cell, value, columnDef, dataContext) {
        let status = dataContext.OUT_OF_STOCK
        if (dataContext.INVENTORY_STATUS != '@Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTED)') {
            status = '0'
        }
        return '<div style="text-align:right; width:100%;">' + status + '</div>'
    }

    //直近棚卸日用formatter
    function NearInventoryDateFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value)
        if (status != '') {
            status = status.replaceAll('-', '/')
            return status.substr(0, 10)
        }
    }

    // 棚卸開始宣言ボタンクリック
    function btnStartTanaoroshi_onclick() {
        //20230929 ADD Start Shirakawa
        //テーブルの選択行を取得する
        var keys = [];
        var invStatusIs = true;
        var invAuth = true;
        var invLocCdName;
        // SlickGridのチェックボックスを取得
        //var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        var gridData = grid.getData();

        //for (var i = 0; i < checkBoxes.length; i++) {
        //    if (checkBoxes[i].checked) {
        //        var item = grid.getDataItem(i)
        //        keys.push(checkBoxes[i].value);
        //        if (item.INVENTORY_STATUS != "-") {
        //            invStatusIs = false
        //        }
        //        invLocCdName = item.LOC_L_CD

        //        if (item.UPD_FLG == "N") {
        //            invAuth = false
        //        }
        //    }
        //}

        @*if (keys.length == 0) {         //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }*@

        var item = grid.getDataItem(0)

        //棚卸開始宣言済みチェック
        if (item.INVENTORY_STATUS != "-") {
            showErrMessage("@Message.getMessage(CommonLabelInventorySummary.MSG_TANAOROSHI_KAISHI_ALREADY)")
            return;
        }

        //棚卸開始権限チェック
        if (item.UPD_FLG == "N") {
            showErrMessage("@Message.getMessage(CommonLabelInventorySummary.MSG_TANAOROSHI_AUTH)")
            return;
        }

        //20230929 ADD End Shirakawa

        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_NAME_TITLE)";
        document.getElementById("INV_NAME").value = ""
        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        //チェックしたキー
        //document.getElementById("SelectKeys").value = item.INV_ID + '/' + item.LOC_L_CD;
        //登録ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_CREATE_CLICK";
        // ダイアログを表示
        ShowModalWindow();
        return true;
    }

    // 棚卸確定ボタンクリック
    function btnConfirmInventory_onclick() {

        //20231002 ADD Start Shirakawa
        //テーブルの選択行を取得する
        var keys = [];
        var invids = []; //20231027 ADD Shirakawa
        var invStatusIs = true;
        var invAuth = true;
        var invLocCdName = [];
        // SlickGridのチェックボックスを取得
        //var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        var gridData = grid.getData();
        //for (var i = 0; i < checkBoxes.length; i++) {
        //    if (checkBoxes[i].checked) {
        //        var item = grid.getDataItem(i)
        //        keys.push(checkBoxes[i].value);
        //        if (item.INVENTORY_STATUS == "-") {
        //            invStatusIs = false
        //        }
        //        invLocCdName = item.LOC_L_CD

        //        if (item.UPD_FLG == "N") {
        //            invAuth = false
        //        }
        //    }
        //}

        //一覧の全項目データ
        var item = grid.getDataItem(0)

        @*if (keys.length == 0) {         //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }*@

        //棚卸開始宣言済みチェック
        if (item.INVENTORY_STATUS == "-") {
            showErrMessage("@Message.getMessage(CommonLabelInventorySummary.MSG_NOT_INVENTORY)")
            return;
        }

        //棚卸確定権限チェック
        if (item.UPD_FLG == "N") {
            showErrMessage("@Message.getMessage(CommonLabelInventorySummary.MSG_TANAOROSHI_AUTH)")
            return;
        }
        //20231002 ADD End Shirakawa

        document.getElementById("SelectKeys").value = item.INV_ID + '/' + item.LOC_L_CD;

        //確認メッセージ
        var req = showConfirmShowLoading(item.INV_NAME_LIST + "@Message.getMessage(CommonInfoMsg.INV_KAKUTEI_CONFIRM)", "@Url.Content("~/InventorySummary")/UpdateInventory");

    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/InventorySummary")/CsvExport");
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

        //ページ番号のリセット
        ResetPageNum();

        //20230904 ADD Start Shirakawa
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventorySummary")/SearchClick',
        });
        //20230904 ADD End Shirakawa


        //ローカルストレージに検索条件保存
        localStorageSave();

        return true;
    }

    //登録ボタンクリック処理（確認メッセージ）
    function btnCreateModal_onclick() {
        var keys = [];
        var invStatusIs = true;
        var invAuth = true;
        var invLocCdName;
        var gridData = grid.getData();


        var item = grid.getDataItem(0)

        //チェックしたキー
        document.getElementById("SelectKeys").value = item.INV_ID + '/' + item.LOC_L_CD;

        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
            showConfirm("@Message.getMessage(CommonInfoMsg.INV_START_CONFIRM)", "@Url.Content("~/InventorySummary")/Create")
        } else {
            showConfirm("@Message.getMessage(CommonInfoMsg.INV_START_CONFIRM)", "@Url.Content("~/InventorySummary")/Create")
        }
    }

    //20230929 ADD Start Shirakawa
    function btnSelectAll_onclick() {
        var chkFlag = false;
        var btn = document.getElementById("btnSelectAll");
        if (btn.value == "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)") {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_OFF)";
            $('input[name="checkEdit"]').prop('checked', true)
            chkFlag = true;
        } else {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
            $('input[name="checkEdit"]').prop('checked', false)
            chkFlag = false;
        }

        //非同期で現在の検索条件での全件を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventorySummary")/SelectAll',
            data: { blnSelectAll: chkFlag },
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
    }
    //20230929 ADD End Shirakawa

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        //画面ブロックを表示
        showLoading();

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //20231003 ADD Start Shirakawa
    //詳細ボタン
    function btnDetail_onclick(element) {
        //テーブルの選択行を取得する
        var keys = [];
        var invStatus;
        var invLocCdName = [];
        // SlickGridのチェックボックスを取得
        //var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        var gridData = grid.getData();
        //for (var i = 0; i < checkBoxes.length; i++) {
        //    if (checkBoxes[i].checked) {
        //        var item = grid.getDataItem(i)
        //        keys.push(checkBoxes[i].value);
        //        invStatus = item.INVENTORY_STATUS
        //        invLocCdName = item.LOC_L_CD
        //    }
        //}

        //一覧の全項目データ
        var item = grid.getDataItem(0)

        @*if (keys.length == 0) {         //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }
        if (keys.length > 1) {         //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }*@

        //実棚照会へ遷移
        @*location.href = '@Url.Action("Index", "InventoryInquiry", New With {.loc = .loc})'*@

        @*//非同期で現在の検索条件での全件を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/Index',
            data: { loc: loc },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                //件数を設定する
                document.getElementById("selectcount").innerHTML = data;

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });*@

        //実棚照会画面に引き渡すキー(開始宣言日時/棚卸ロケ大区分コード)を設定
        document.getElementById("SelectKeys").value = item.INV_ID + '/' + item.LOC_L_CD;

        $('#form').attr("action", "@Url.Content("~/InventorySummary")/SetKeys");
        $('#form').submit();

        return true;

    }
    //20231003 ADD End Shirakawa

    //大区分選択
    function ChangeLocationL(locL,doAutoSearch) {
        // 自動検索は画面表示時の復元（doAutoSearch）のみ行う
        // ※×で選択解除した場合に検索が走り、必須エラーにならないようにする
        if (doAutoSearch) {
            // 検索処理実行
            if (document.getElementById('FirstSearch').value == "True") {
                document.getElementById('Search_Button').click();
            }
        }
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

<div id="inventory-summary">
    @Using (Html.BeginForm("Index", "InventorySummary", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @*@Html.AntiForgeryToken()*@

    Dim buttonSettingKaishi As String = ""
    Dim buttonSettingKakutei As String = ""
    Dim buttonSettingJitsuTana As String = ""
    Dim buttonSettingCsv As String = ""
    Dim buttonSettingSearch As String = ""
    Dim buttonStyleKaishi As String = "width:130px;margin-right:100px;"
    Dim buttonStyleKakutei As String = "margin-left:100px;"
    Dim buttonStyleJitsuTana As String = ""
    Dim buttonStyleCsv As String = ""
    Dim buttonStyleSearch As String = ""

    '棚卸開始宣言済
    '--棚卸開始宣言
    '20230929 UPD Shirakawa
    buttonSettingKaishi = ""
    '--棚卸確定
    buttonSettingKakutei = ""
    '--実棚照会
    buttonStyleJitsuTana = ""
    '--CSV出力
    buttonSettingCsv = ""
    '--検索
    buttonSettingSearch = ""

    '権限が「部署担当者」の場合は使用不可
    If Model.FirstOrDefault.LOGIN_KENGEN = SHOSAI_AUTH_DIV.Kyoten_Nomal Then
        buttonSettingKaishi = "disabled"
        buttonSettingKakutei = "disabled"
        buttonStyleKaishi &= "background-color:#cecece;"
        buttonStyleKakutei &= "background-color:#cecece;"
    End If
    '権限が「本部担当者」の場合は使用不可
    If Model.FirstOrDefault.LOGIN_KENGEN = SHOSAI_AUTH_DIV.Honbu_Nomal Then
        buttonSettingKaishi = "disabled"
        buttonStyleKaishi &= "background-color:#cecece;"
    End If

        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    @* 棚卸開始宣言 *@
                    @*<input Class="defButton" type="button" value="@Message.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_KAISHI)" onclick="return btnStartTanaoroshi_onclick()" @buttonSettingKaishi>*@
                    @*<input Class="defButton" type="button" value="@Model.FirstOrDefault.INVENTORY_MESSAGE" onclick="return btnStartTanaoroshi_onclick()" @buttonSettingKaishi style="@buttonStyleKaishi">*@
                    <input Class="defButton" type="button" value="@Model.FirstOrDefault.INVENTORY_MESSAGE" onclick="return btnStartTanaoroshi_onclick()" @buttonSettingKaishi style="@buttonStyleKaishi">


                    @*20230929 ADD Start Shirakawa*@
                    @* 全選択 *@
                    @* 全選択/全解除ボタンの表記文字設定 *@
                    @Code
                        Dim AllSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG)
                        Dim sDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                        If AllSelectFlg Then
                            sDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                        End If
                    End Code
                    <input id="btnSelectAll" Class="defButton submit-button" type="button" style="display:none;" value="@sDispStr" onclick="return btnSelectAll_onclick()" />
                    @*20230929 ADD End Shirakawa*@

                    @* 実棚照会 *@
                    <input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventoryHistory.LABEL_SHOW_INVENTORY_INQUIRY)" onclick="return btnDetail_onclick()" @buttonSettingJitsuTana style="@buttonStyleJitsuTana">
                    @* 棚卸確定 *@
                    @*<input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_KAKUTEI)" onclick="return btnConfirmInventory_onclick()" @buttonSettingKakutei style="@buttonStyleKakutei">*@
                    <input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_KAKUTEI)" onclick="return btnConfirmInventory_onclick()" @buttonSettingKakutei style="@buttonStyleKakutei">

                </div>
                <div class="view-area-button-right">
                    @* CSV出力 *@
                    <input class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL)" onclick="return btnCsvExport_onclick()" @buttonSettingCsv style="@buttonStyleCsv">
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
                            @* ロケーション 大区分 *@
                            <div class="col-sm-2 filter-combo-area" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L)<font Class="label-require">※</font></div>
                                <div Class="grid-condition-col">
                                    @* select2-clearable クラスで入力して絞り込めるSelect2になる（Scripts\select2-common.js）。選択肢・選択後とも「コード 名称」を表示 *@
                                    @* ×で選択解除するため、空の選択肢を追加している *@
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocL, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeLocationL(this)", .class = "cond-control select2-clearable"})
                                </div>
                            </div>
                            @* 状態 *@
                            <div class="col-sm-1" style="display:none;max-width:150px;">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_STATUS)</div>
                                <div Class="grid-condition-col">
                                    @Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchNoStockTaking) @Message.getMessage(CommonLabel.LABEL_INVENTORY_MI)
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="view-area-label" style="height: 25px"></div>
                    <input class="float-right defButton submit-button" id="Search_Button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>

        @*20230929 DEL Start Shirakawa*@

        @*20230929 DEL End Shirakawa*@

                        'チェックボックス選択値の引き渡し用隠し項目
        @<input id="SelectKeys" name="SelectKeys" type="hidden">
        @<input type="hidden" id="IsRestoreOwnSearchConditions" name="IsRestoreOwnSearchConditions" value="@Model.FirstOrDefault().IsRestoreOwnSearchConditions.ToString()" />
        @<input type="hidden" id="FirstSearch" name="FirstSearch" value="@Model.FirstOrDefault().FirstSearch.ToString()" />

        @*ページャー表示*@
        @*@PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)*@
        @<div class="list-view">
            <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
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
                Dim SessionModel As InventorySummaryViewModel = Session(CommonSessionKey.CREAGE_INVENTORY_SUMMARY)
                If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(SessionModel)
                Else
                    Model.Add(New InventorySummaryViewModel())
                End If
            END CODE
            @Html.AntiForgeryToken()
            <Table>
                <tr Class="popup-row">
                    <td width="200px" Class="head-col large">@Message.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_NAME)</td>
                    <td width="20"><font Class="require">※</font></td>
                    <td>@Html.EditorFor(Function(model) model.Last.INV_NAME, New With {.htmlAttributes = New With {.maxlength = 60, .style = "width:790px;"}})</td>
                </tr>
            </Table>

            @Html.HiddenFor(Function(model) model.FirstOrDefault.START_DEC_TS)

            @*<div Class="PopupButton" style="margin:0 auto; width:220px;"> yamada*@
            <div Class="PopupButton" style="margin:0 auto; width:260px;">
                <span style="float:left;margin:15px 20px 0 0;">
                    @*<input id="modal-create-btn" class="defButton modal-close modal-create-btn submit-button" formaction="~/InventorySummary/Create" type="submit" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)">*@
                    <input id="modal-create-btn" class="defButton  modal-create-btn submit-button" formaction="~/InventorySummary/Create" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnCreateModal_onclick()">
                </span>
                <span style="float:left;margin:15px 0%;">
                    <input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
                </span>
                <div style="clear:both;"></div>
            </div>
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)

                End Using
</div>
