@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of InventoryHistoryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE)
    Dim ComRfid As CommonRfid = New CommonRfid() ' = App(CommonConst.KEY_APP_LANGUAGE)

End Code
<script>
    //
    //  棚卸実績一覧
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
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
            //変更画面を表示
            document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabelInventoryHistory.LABEL_CHANGE_INVENTORY_NAME)";
            ShowModalWindow()
        }

        // 権限が一般の場合は棚卸名変更ボタンを非活性
        if ("@ViewBag.LoginKengenKbn" == "@AUTH_DIV.Nomal") {
            $("#btnChangeInvName").prop("disabled", true);
            $("#btnChangeInvName").css("background-color", "#cecece");
        }

        //列定義
        let columns = [
            { id: "@ColKey.InventoryHistory.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.CHECK_SELECT", 40), sortOrder: -2, formatter: InventoryHistoryCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.InventoryHistory.START_DEC_TS", name: '@Message.getMessage(CommonLabelInventoryHistory.LABEL_START_DEC_TS)', field: "START_DEC_TS", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.START_DEC_TS", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("START_DEC_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("START_DEC_TS").DispFlag", formatter: StartDecTsFormatter},
            { id: "@ColKey.InventoryHistory.START_DEC_NAME", name: '@Message.getMessage(CommonLabelInventoryHistory.LABEL_START_DEC_NAME)', field: "START_DEC_NAME", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.START_DEC_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("START_DEC_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("START_DEC_NAME").DispFlag" },
            { id: "@ColKey.InventoryHistory.INV_NAME", name: '@Message.getMessage(CommonLabelInventoryHistory.LABEL_INV_NAME)', field: "INV_NAME", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.INV_NAME", 300), sortOrder: "@Model.FirstOrDefault.GridInfo("INV_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INV_NAME").DispFlag" },
            { id: "@ColKey.InventoryHistory.INV_CONFIRM_TS", name: '@Message.getMessage(CommonLabelInventoryHistory.LABEL_INV_CONFIRM_TS)', field: "INV_CONFIRM_TS", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.INV_CONFIRM_TS", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("INV_CONFIRM_TS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INV_CONFIRM_TS").DispFlag", formatter: InvConfirmTsFormatter },
            { id: "@ColKey.InventoryHistory.INV_CONFIRM_NAME", name: '@Message.getMessage(CommonLabelInventoryHistory.LABEL_INV_CONFIRM_NAME)', field: "INV_CONFIRM_NAME", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.INV_CONFIRM_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("INV_CONFIRM_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INV_CONFIRM_NAME").DispFlag" },
            { id: "@ColKey.InventoryHistory.INV_LOC_L_KUBUN", name: '@Message.getMessage(CommonLabel.LABEL_INV_LOC_L_KUBUN)', field: "INV_LOC_L_KUBUN", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.INV_LOC_L_KUBUN", 300), sortOrder: "@Model.FirstOrDefault.GridInfo("INV_LOC_L_KUBUN").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INV_LOC_L_KUBUN").DispFlag", formatter: LocCdNameFormatter },
            { id: "@ColKey.InventoryHistory.LOGIC_INV_NUM", name: '@Message.getMessage(CommonLabelInventoryHistory.LABEL_LOGIC_INV_NUM)', field: "LOGIC_INV_NUM", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.LOGIC_INV_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("LOGIC_INV_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOGIC_INV_NUM").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.InventoryHistory.PHYINV_NUM", name: '@Message.getMessage(CommonLabelInventoryHistory.LABEL_PHYINV_NUM)', field: "PHYINV_NUM", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.PHYINV_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_NUM").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.InventoryHistory.LOC_DIFF_NUM", name: '@Message.getMessage(CommonLabelInventoryHistory.LABEL_LOC_DIFF_NUM)', field: "LOC_DIFF_NUM", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.LOC_DIFF_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_DIFF_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_DIFF_NUM").DispFlag", formatter: RightAlignFormatter },
            { id: "@ColKey.InventoryHistory.NO_ITEM_NUM", name: '@Message.getMessage(CommonLabelInventoryHistory.LABEL_NO_ITEM_NUM)', field: "NO_ITEM_NUM", width: getColumnWidth("@DispId.INVENTORY_HISTORY", "@ColKey.InventoryHistory.NO_ITEM_NUM", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("NO_ITEM_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("NO_ITEM_NUM").DispFlag", formatter: RightAlignFormatter },

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

        let data = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.INVENTORY_HISTORY", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, "@DispId.INVENTORY_HISTORY", fixCol);
        }

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

        //ローカルストレージから復元
        const saved = localStorage.getItem("InventoryHistorySearchConditions");
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
        localStorage.setItem("InventoryHistorySearchConditions", JSON.stringify(cond));

    }

    //選択用formatter
    function InventoryHistoryCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.START_DEC_TS) + '/' + convertToEmptyIfNull(dataContext.INV_ID) + '/' + convertToEmptyIfNull(dataContext.LOC_L_CD) + '/' + convertToEmptyIfNull(dataContext.LOC_CD_NAME)
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;">' +
               '<input type="checkbox" class="slick-checkbox" ' +
               'value="' + key + '" ' +
               (value == 1 ? 'checked="checked"' : '') + ' />' +
               '</div>';
    }

    //棚卸開始宣言日時用formatter
    function StartDecTsFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value).toString()
        if (status != '') {
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            let hhmm = status.substr(8, 2) + ':' + status.substr(10, 2)
            return yyyy + '/' + mm + '/' + dd + ' ' + hhmm
        }
    }

    //棚卸確定日時用formatter
    function InvConfirmTsFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value).toString()
        if (status != '') {
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            let hhmm = status.substr(8, 2) + ':' + status.substr(10, 2)
            return yyyy + '/' + mm + '/' + dd + ' ' + hhmm
        }
    }

    //ロケ大区分用formatter
    function LocCdNameFormatter(row, cell, value, columnDef, dataContext) {
        return dataContext.LOC_CD_NAME
    }

    // テキスト表示(右寄せ)
    function RightAlignFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return '<div style="text-align:right; width:100%;">' + value + '</div>';
        }
    }


    // 棚卸名変更ボタンクリック
    function btnStartTanaoroshi_onclick() {
        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabelInventoryHistory.LABEL_CHANGE_INVENTORY_NAME)";
        // 選択行の開始宣言日時を取得
        var startDecTs = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                var row = checkBoxes[i].parentNode.parentNode;
                startDecTs.push(checkBoxes[i].value);
            }
        }
        if (startDecTs.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        } else if (startDecTs.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }

        //エラーの場合検索しない
        //if ("@Model.FirstOrDefault.ErrCode" == "") {
        //選択行のデータを取得し表示する(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryHistory")/GetEditData',
            cache: false,
            data: { ID: startDecTs[0] },
            success: function (data, dataType) {
                this;
                //データが存在しない場合エラー
                if (data == null) {
                    return;
                }

                //取得したデータを表示する
                document.getElementById("INV_ID").value = startDecTs[0].split('/')[1];  //棚卸IDを補完

                document.getElementById("START_DEC_TS").value = data.START_DEC_TS;
                document.getElementById("START_DEC_EMPLOYEE_CD").value = data.START_DEC_EMPLOYEE_CD;
                document.getElementById("START_DEC_NAME").value = data.START_DEC_NAME;
                document.getElementById("INV_NAME").value = data.INV_NAME;

                document.getElementById("INV_CONFIRM_TS").value = data.INV_CONFIRM_TS;
                document.getElementById("INV_CONFIRM_EMPLOYEE_CD").value = data.INV_CONFIRM_EMPLOYEE_CD;
                document.getElementById("INV_CONFIRM_NAME").value = data.INV_CONFIRM_NAME;

                document.getElementById("LOGIC_INV_NUM").value = data.LOGIC_INV_NUM;
                document.getElementById("PHYINV_NUM").value = data.PHYINV_NUM;
                document.getElementById("LOC_DIFF_NUM").value = data.LOC_DIFF_NUM;
                document.getElementById("NO_ITEM_NUM").value = data.NO_ITEM_NUM;
                document.getElementById("INV_ALL_CONFIRM_TS").value = data.INV_ALL_CONFIRM_TS;

                document.getElementById("UPD_DATETIME").value = formatDate(new Date(data.UpdDatetimeString), "yyyy/MM/dd HH:mm:ss");
                document.getElementById("UPD_EMPLOYEE_CD").value = data.UPD_EMPLOYEE_CD;
                document.getElementById("UPD_EMPLOYEE_NAME").value = data.UPD_EMPLOYEE_NAME;
                document.getElementById("UPD_TASK").value = data.RenewProcess;

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
        //}

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        // ダイアログを表示
        ShowModalWindow();
        return true;
    }

    // 実棚照会ボタンクリック処理
    function btnShowInventoryInquiry_onclick() {
        // 選択行の開始宣言日時を取得
        var keys = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                var row = checkBoxes[i].parentNode.parentNode;
                keys.push(checkBoxes[i].value);
            }
        }
        if (keys.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        } else if (keys.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }
        //--2019/10/15 nv miyaji upd start
        //location.href = './InventoryAssetHistory?startDecTs=' + startDecTs[0];
        // 実棚実績照会画面に引き渡す開始宣言日時を設定
        document.getElementById("SelectKeys").value = keys[0];

        $('#form').attr("action", "@Url.Content("~/InventoryHistory")/SetKeys");
        $('#form').submit();

        //--2019/10/15 nv miyaji upd end

        return true;
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {

        var keys = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                var row = checkBoxes[i].parentNode.parentNode;
                keys.push(checkBoxes[i].value.split('/')[1]);
            }
        }
        if (keys.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        } else if (keys.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }

        // カンマ区切りの文字列を設定
        document.getElementById("SelectKeys").value = keys.join(",");

        // ローディング表示
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
        
        $form.attr("action", "@Url.Content("~/InventoryHistory")/CsvExport");
        $form.submit();
        $form.attr("action", befAction);
        //--2019/10/15 nv miyaji upd end

        return true;
    }

    //登録ボタンクリック処理（確認メッセージ）
    function btnCreateModal_onclick() {
        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
            showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/InventoryHistory")/Create")
        } else {
            showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/InventoryHistory")/Create")
        }
    }

    //削除ボタンクリック処理
    function btnDelete_onclick() {
        var startDecTs = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                var row = checkBoxes[i].parentNode.parentNode;
                startDecTs.push(checkBoxes[i].value.split('/')[1]);
            }
        }
        if (startDecTs.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        // カンマ区切りの文字列を設定
        document.getElementById("SelectKeys").value = startDecTs.join(",");

        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/InventoryHistory")/Delete");

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

    //ローカルストレージに検索条件保存
    localStorageSave();

    return true;
    }

    //[CC431] ADD Start 20261015
    //検索条件リンククリック時処理（他の画面に合わせ、リンクの文言を入れ替える）
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }
    //[CC431] ADD End 20261015

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        //画面ブロックを表示
        showLoading();

        $('#form').submit();   //2020/02/20 nv miyaji
    }

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

</script>

<div id="inventory-history">
    @Using (Html.BeginForm("Index", "InventoryHistory", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @*@Html.AntiForgeryToken()*@

        @<div class="view-area">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    @* 棚卸名変更 *@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @<input id="btnChangeInvName" Class="defButton Inventory-large" type="button" value="@Message.getMessage(CommonLabelInventoryHistory.LABEL_CHANGE_INVENTORY_NAME)" onclick="return btnStartTanaoroshi_onclick()">
                    End If
                    @* 削除 --2019/11/08 nv miyaji add*@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @<input Class="defButton Inventory-large" type="button" name="Delete"  style="display:none" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="btnDelete_onclick()">
                    End If
                </div>
                <div class="view-area-button-right">
                    @* 実棚照会 *@
                    @*<input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventoryHistory.LABEL_SHOW_INVENTORY_INQUIRY)" onclick="return btnShowInventoryInquiry_onclick()">*@
                    @*<input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventoryHistory.LABEL_SHOW_INVENTORY_INQUIRY)" onclick="btnShowInventoryInquiry_onclick();location.href='@Url.Action("Index", "InventoryAssetHistory", New With {.startDecTs = Model.Last.START_DEC_TS})'">*@
                    <input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventoryHistory.LABEL_SHOW_INVENTORY_INQUIRY)" onclick="btnShowInventoryInquiry_onclick()">

                    @* 棚卸実績出力 *@
                    <input class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_INVENTRY_RESULT_OUTPUT)" onclick="return btnCsvExport_onclick()">
                </div>
            </div>
        </div>

        @* 検索条件 *@
        @<div Class="search-wrap">
            <a id="SearchCondition" data-toggle="collapse" href="#cond" aria-expanded="true" onclick="SearchCondition_onclick()">
                @*[CC431] UPD Start 20261015 他の画面に合わせ、開閉で入れ替わる文言（▼検索条件／▲検索条件）にする*@
                @Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)
                @*[CC431] UPD End 20261015*@
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
                        </div>
                    </div>
                    <div class="view-area-label" style="height: 25px"></div>
                    <input class="float-right defButton submit-button" id="Search_Button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>

        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div class="list-view">
            <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
        </div>


        '履歴画面遷移時、資産番号引き渡し用各四項目

        @<input id="SelectedStartDecTs" name="SelectedStartDecTs" type="hidden">
        'チェックボックス選択値の引き渡し用隠し項目
        @<input id="SelectKeys" name="SelectKeys" type="hidden">

        @<input type="hidden" id="IsRestoreOwnSearchConditions" name="IsRestoreOwnSearchConditions" value="@Model.FirstOrDefault().IsRestoreOwnSearchConditions.ToString()" />
        @<input type="hidden" id="FirstSearch" name="FirstSearch" value="@Model.FirstOrDefault().FirstSearch.ToString()" />

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
                Dim SessionModel As InventoryHistoryViewModel = Session(CommonSessionKey.CREAGE_INVENTORY_HISTORY)
                If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(SessionModel)
                Else
                    Model.Add(New InventoryHistoryViewModel())
                End If
            END CODE
            @Html.AntiForgeryToken()
            <Table>
                <tr Class="popup-row">
                    <td width="200px" Class="head-col large">@Message.getMessage(CommonLabelInventoryHistory.LABEL_INV_NAME)</td>
                    <td width="20"><font Class="require">※</font></td>
                    <td>@Html.EditorFor(Function(model) model.Last.INV_NAME, New With {.htmlAttributes = New With {.maxlength = 60, .style = "width:790px;"}})</td>
                </tr>
            </Table>

            @*<div Class="PopupButton" style="margin:0 auto; width:220px;"> yamada*@
            <div Class="PopupButton" style="margin:0 auto; width:260px;">
                <span style="float:left;margin:15px 20px 0 0;">
                    @*<input id="modal-create-btn" class="defButton modal-close modal-create-btn submit-button" formaction="~/InventoryHistory/Create" type="submit" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)">*@
                    <input id="modal-create-btn" class="defButton  modal-create-btn submit-button" formaction="~/InventoryHistory/Create" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="return btnCreateModal_onclick()">
                </span>
                <span style="float:left;margin:15px 0%;">
                    <input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
                </span>
            </div>
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)

        @Html.HiddenFor(Function(model) model.FirstOrDefault.INV_ID)    @*20231031 ADD Start Shirakawa*@

        @Html.HiddenFor(Function(model) model.FirstOrDefault.START_DEC_TS)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.START_DEC_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.START_DEC_NAME)

        @Html.HiddenFor(Function(model) model.FirstOrDefault.INV_CONFIRM_TS)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.INV_CONFIRM_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.INV_CONFIRM_NAME)

        @Html.HiddenFor(Function(model) model.FirstOrDefault.LOGIC_INV_NUM)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.PHYINV_NUM)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.LOC_DIFF_NUM)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.NO_ITEM_NUM)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.INV_ALL_CONFIRM_TS)

        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_TASK)

                End Using
</div>
