@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of TemporaryListViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelTemporaryList.TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
    Dim inputItemList = DirectCast(ViewBag.inputItemList, IEnumerable(Of SearchInputItemModel))
End Code
<script>
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
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessAddTemp") {
            //追加
            showResultMessage("@Message.getMessage(CommonLabelTemporaryList.MSG_SUCCESS_ADD_REL_COMPLETE)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            //変更画面を表示
            //タイトルの設定
            document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_UPDATE)";
            document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_UPDATE)";

            // 権限による処理区分
            if ("@ViewBag.AuthProcFlg" == "@AuthProcFlg.All" || "@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiAll") {
                // 権限による処理区分が「編集・照会可能」の場合
            } else {
                // 権限による処理区分が「照会のみ」の場合（本来ここに入ることはないはずだが、念のため）
                // 変更ボタンを非表示
                $("#modal-create-btn").css("display", "none");
            };

            ShowModalWindow()

            if ("@Model.FirstOrDefault.ErrTarget".search("Target_") >= 0) {
                showErrMessage("@Model.FirstOrDefault.ErrCode", "#@Model.FirstOrDefault.ErrTarget".replace("Target_", ""))
            } else {
                showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
            }

        } else {
            //ロード前のボタン名を設定する
            document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"
        }

        // キー値が存在する場合の項目非表示設定
        if ("@ViewBag.ConstructionListKey") {
            $("#btnReturn").show();
            $("#btnAdd").show();
        }else{
            $("#update-btn").show();
        }

        //隠し項目に格納
        document.getElementById("LoadingImg").value = '@Url.Content("~/Images/")' + "Loading.gif";

        // 列定義
        let columns = [
            { id: "@ColKey.TemporaryList.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.CHECK_SELECT", 40), sortOrder: -2, formatter: TemporaryListCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.TemporaryList.REL_CONST_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_REL_CONST_STATUS)", field: "REL_CONST_NO", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.REL_CONST_STATUS", 190), sortOrder: "@Model.FirstOrDefault.GridInfo("REL_CONST_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REL_CONST_STATUS").DispFlag", formatter: RelConstStatusFormatter },
            { id: "@ColKey.TemporaryList.REL_CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_REL_CONST_NO)", field: "REL_CONST_NO", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.REL_CONST_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("REL_CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REL_CONST_NO").DispFlag"},
            { id: "@ColKey.TemporaryList.REG_CONST_NO", name: "@Message.getMessage(CommonLabel.LABEL_REG_CONST_NO)", field: "REG_CONST_NO", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.REG_CONST_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("REG_CONST_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REG_CONST_NO").DispFlag"},
            { id: "@ColKey.TemporaryList.TEMP_DT", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_DT)", field: "TEMP_DT", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_DT", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_DT").DispFlag", formatter: SlashYMDFormatter},
            { id: "@ColKey.TemporaryList.CONST_CONTENTS", name: "@Message.getMessage(CommonLabel.LABEL_WORK_CONTENTS)", field: "CONST_CONTENTS", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.CONST_CONTENTS", 300), sortOrder: "@Model.FirstOrDefault.GridInfo("CONST_CONTENTS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CONST_CONTENTS").DispFlag" },
            { id: "@ColKey.TemporaryList.TEMP_INFO1", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO1)", field: "TEMP_INFO1", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_INFO1", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_INFO1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_INFO1").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TemporaryList.TEMP_INFO2", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO2)", field: "TEMP_INFO2", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_INFO2", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_INFO2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_INFO2").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TemporaryList.TEMP_INFO3", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO3)", field: "TEMP_INFO3", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_INFO3", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_INFO3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_INFO3").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TemporaryList.TEMP_INFO4", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO4)", field: "TEMP_INFO4", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_INFO4", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_INFO4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_INFO4").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TemporaryList.TEMP_INFO5", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO5)", field: "TEMP_INFO5", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_INFO5", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_INFO5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_INFO5").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TemporaryList.TEMP_INFO6", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO6)", field: "TEMP_INFO6", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_INFO6", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_INFO6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_INFO6").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TemporaryList.TEMP_INFO7", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_INFO7)", field: "TEMP_INFO7", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_INFO7", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_INFO7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_INFO7").DispFlag", formatter: TempInfoFormatter },
            { id: "@ColKey.TemporaryList.TEMP_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_REMARK)", field: "TEMP_REMARK", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_REMARK", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_REMARK").DispFlag" },
            { id: "@ColKey.TemporaryList.TEMP_NUM", name: "@Message.getMessage(CommonLabel.LABEL_TEMP_NUM)", field: "TEMP_NUM", width: getColumnWidth("@DispId.TEMPORARY_LIST", "@ColKey.TemporaryList.TEMP_NUM", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("TEMP_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TEMP_NUM").DispFlag", formatter: TempNumFormatter },
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
            // Key1:仮置きID
            var keys = lstCheckedKey[i].split(",");

            for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                if (keys[0] === jsonData[grid_i].TEMP_ID.toString()) {
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
            dataView.setItems(jsonData, "TEMP_ID");
            dataView.endUpdate();
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列を加算
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.TEMPORARY_LIST", fixCol);

    });

    // チェックボックス(中央寄せ)
    function TemporaryListCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.TEMP_ID).toString();
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheck" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" ' +
               (isKeyInclude ? 'checked="checked" ' : '') +
               'onchange="check_onclick(this)" ' +
               '/>' +
               '</div>';
    }

    // 仮置き紐付状態用formatter
    function RelConstStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (dataContext.REL_CONST_NO) {
            status = "@Message.getMessage(CommonLabel.VALUE_REL_CONSTRUCTION_STATUS_LINKED)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_REL_CONSTRUCTION_STATUS_UNLINKED)"
        }

        return status;
    }

    // 仮置き情報用formatter
    function TempInfoFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(value).replaceAll("$$$", "");
    }

    // yyyyMMdd→yyyy/MM/dd表示用formatter
    function SlashYMDFormatter(row, cell, value, columnDef, dataContext) {
        let ret = "";
        if (value) {
            ret = toStringDateYMD(value);
        }
        return ret

    }

    // 仮置き数用formatter
    function TempNumFormatter( row, cell, value, columnDef, dataContext )  {
        return '<a href="#" onclick="SetTempId(\'' + dataContext.TEMP_ID + '\')" class="blue-link">' + dataContext.TEMP_NUM + '</a>';
    }

    //選択した仮置きIDをセッションに保存します
    function SetTempId(tmpId) {
        // 仮置資産一覧画面に引き渡す資産番号を設定
        document.getElementById("SelectedTempId").value = tmpId;

        $('#form').attr("action", "@Url.Content("~/TemporaryList")/SetTempId");
        $('#form').submit();

    }

    //CSV出力ボタンクリック処理
    function btnIchiranCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/TemporaryList")/IchiranCsvExport");

    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
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

    //追加ボタンクリック処理
    function btnAdd_onclick() {

        var keys = [];
        var nonekeys = [];
        var invLinkage = false;
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        var gridData = grid.getData();
        for (var i = 0; i < checkBoxes.length; i++) {
            //チェックボックスの場合
            if (checkBoxes[i].type == "checkbox") {
                //キーを取得する
                var result = checkBoxes[i].value

                if (checkBoxes[i].checked) {
                    //選択されている場合
                    keys.push(result);
                    var item = grid.getDataItem(i)
                    if (item.REL_CONST_NO) {
                        invLinkage = true
                    }
                } else {
                    //選択されていない場合
                    nonekeys.push(result);
                }
            }
        }

        if (keys.length == 0) {         //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (keys.length > 1) {   //テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        //紐付ありチェック
        if (invLinkage) {
            showErrMessage("@Message.getMessage(CommonLabelTemporaryList.MSG_ADD_REL_TEMP)")
            return;
        }

        // スラッシュ区切りの文字列を設定
        document.getElementById("SelectKeys").value = keys.join("/");
        document.getElementById("NoneSelectKeys").value = nonekeys.join("/");

        //確認メッセージ
        var req = showConfirm("@Message.getMessage(CommonLabelTemporaryList.MSG_ADD_CONFIRM)", "@Url.Content("~/TemporaryList")/UpdRelConstNo");
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
        document.getElementById("popup-title").innerHTML = "@Message.getMessage(CommonLabelTemporaryList.TITLE_TEMPORARY_UPDATE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_UPDATE)";

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        //削除ボタンを表示
        // 権限による処理区分
        if(("@ViewBag.AuthProcFlg" == "@AuthProcFlg.All"|| "@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiAll") && isEditMode == true){
            // 権限による処理区分が「編集・照会可能」の場合
        }else{
            // 権限による処理区分が「照会のみ」の場合
            // 変更ボタンを非表示
            $("#modal-create-btn").css("display", "none");
        };

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        $("#@(Model.FirstOrDefault.ErrTarget)_FROM").removeClass("input-error");
        $("#@(Model.FirstOrDefault.ErrTarget)_TO").removeClass("input-error");

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/TemporaryList")/GetEditData',
                cache: false,
                data: { ID: constNo[0] },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }

                    //取得したデータを表示する
                    document.getElementById("TEMP_INFO1").value = data.TEMP_INFO1;
                    document.getElementById("TEMP_INFO2").value = data.TEMP_INFO2;
                    document.getElementById("TEMP_INFO3").value = data.TEMP_INFO3;
                    document.getElementById("TEMP_INFO4").value = data.TEMP_INFO4;
                    document.getElementById("TEMP_INFO5").value = data.TEMP_INFO5;
                    document.getElementById("TEMP_INFO6").value = data.TEMP_INFO6;
                    document.getElementById("TEMP_INFO7").value = data.TEMP_INFO7;

                    document.getElementById("TEMP_ID").value = data.TEMP_ID;
                    document.getElementById("REG_CONST_NO").value = data.REG_CONST_NO;
                    document.getElementById("REL_CONST_NO").value = data.REL_CONST_NO;
                    document.getElementById("TEMP_DT").value = data.TEMP_DT;
                    document.getElementById("WORK_EMPLOYEE_CD").value = data.WORK_EMPLOYEE_CD;
                    document.getElementById("WORK_EMPLOYEE_NAME").value = data.WORK_EMPLOYEE_NAME;
                    document.getElementById("TEMP_REMARK").value = data.TEMP_REMARK;
                    document.getElementById("TASK_TS").value = data.TASK_TS;
                    document.getElementById("INS_DATETIME").value = formatDate(new Date(data.InsDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("INS_EMPLOYEE_CD").value = data.INS_EMPLOYEE_CD;
                    document.getElementById("INS_EMPLOYEE_NAME").value = data.INS_EMPLOYEE_NAME;
                    document.getElementById("UPD_DATETIME").value = formatDate(new Date(data.UpdDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("UPD_EMPLOYEE_CD").value = data.UPD_EMPLOYEE_CD;
                    document.getElementById("UPD_EMPLOYEE_NAME").value = data.UPD_EMPLOYEE_NAME;
                    document.getElementById("UPD_TASK").value = data.RenewProcess;

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

    // 詳細ボタンクリック処理
    function btnShowTempAssetList_onclick() {
        // 選択行のキーを取得
        var keyValue = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
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
        SetTempId(keyValue[0]);

        return true;
    }

    //モーダル画面　変更ボタンクリック処理
    function btnUpdate_onclick() {
        if(document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_UPDATE_CLICK"){
            //確認メッセージを出し、ＯＫなら変更処理
            // ダイアログのメッセージを設定
            $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)");

            // ダイアログを作成
            $("#show-dialog").dialog({
                modal: true,
                title: strConfirmTitle,
                dialogClass: "show-confirm-wrap",
                buttons: {
                    "OK": function () {
                        var befAction = $('#form').attr("action");
                        $(this).dialog("close");
                        $('#form').attr("action", "@Url.Content("~/TemporaryList")/Create");
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
        }else{
            showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/TemporaryList")/Create")
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

</script>

<div id="construction-list">
    @Using (Html.BeginForm("Index", "TemporaryList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20" style="width:100%">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    <div>
                        @If ViewBag.AuthProcFlg = AuthProcFlg.All Or ViewBag.AuthProcFlg = AuthProcFlg.JiAll Then
                            @<input Class="defButton add-btn" type="button" id="btnAdd" style="display:none;" value="@Message.getMessage(CommonLabel.BUTTON_ADD)" onclick="btnAdd_onclick()">
                            @<input Class="defButton update-btn" type="button" id="update-btn" style="display:none;" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick(true)">
                        End If
                        <input class="defButton detail-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DETAIL)" onclick="btnEdit_onclick(false)">
                        <input class="defButton detail-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_ASSET_LINK)" onclick="btnShowTempAssetList_onclick()">
                    </div>
                </div>
                <div class="view-area-button-right" style="margin-right:20px">
                    <div>
                        <input id="csvBtn" class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnIchiranCsvExport_onclick()">
                        @*戻るボタンの初期表示を非表示にする*@
                        <input class="defButton" type="button" id="btnReturn" name="Return" style="display:none;" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "ConstructionList", New With {.actionname = "Create"})'">
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
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("REL_CONST_STATUS").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_REL_CONST_STATUS)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchRelConstStatus, DirectCast(ViewBag.RelConstStatusList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("REL_CONST_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_REL_CONST_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchRelConstNo, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("REG_CONST_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_REG_CONST_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchRegConstNo, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("TEMP_DT").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchTempDt, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CONST_CONTENTS").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_WORK_CONTENTS)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstContents, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("TEMP_INFO1").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO1)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("TEMP_INFO1").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchTempInfo1, DirectCast(ViewBag.inputItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "1").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("TEMP_INFO2").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO2)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("TEMP_INFO2").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchTempInfo2, DirectCast(ViewBag.inputItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "2").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("TEMP_INFO3").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO3)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("TEMP_INFO3").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchTempInfo3, DirectCast(ViewBag.inputItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "3").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("TEMP_INFO4").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO4)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("TEMP_INFO4").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchTempInfo4, DirectCast(ViewBag.inputItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "4").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("TEMP_INFO5").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO5)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("TEMP_INFO5").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchTempInfo5, DirectCast(ViewBag.inputItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "5").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>

                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("TEMP_INFO6").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO6)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("TEMP_INFO6").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchTempInfo6, DirectCast(ViewBag.inputItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "5").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("TEMP_INFO7").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO7)</div>
                                <div class="grid-condition-col">
                                    @(If(Model.FirstOrDefault.GridInfo("TEMP_INFO7").DispFlag = "1", SearchInputItemHelper.SearchItemFor(Me.Html, Function(model) model.FirstOrDefault.SearchTempInfo7, DirectCast(ViewBag.inputItemList, IEnumerable(Of SearchInputItemModel)).Where(Function(x) x.KEY1 = "5").FirstOrDefault, New With {.Class = "cond-control", .maxlength = 255}), ""))
                                </div>
                            </div>

                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("TEMP_REMARK").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TEMP_REMARK)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchTempRemark, New With {.class = "cond-control", .maxlength = 255})
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
            '詳細画面遷移時、仮置きID引き渡し用隠し項目
        @<input id="SelectedTempId" name="SelectedTempId" type="hidden">

            '登録・更新のモーダル画面
        @<div id="modal-content" class="modal-constructionlist" style="width: 550px;">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
            @CODE
                'モデルの値を設定
                Dim SessionModel As TemporaryListViewModel = Session(CommonSessionKey.CREAGE_TEMPORARY_LIST)
                If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(SessionModel)
                Else
                    Model.Add(New TemporaryListViewModel())
                End If
            END CODE
            @Html.AntiForgeryToken()

            <Table>
                <tr Class="popup-row">
                    <td class="head-col label-control" style="width:90px;">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO1)</td>
                    <td width="20" style="display:inline-block;"></td>
                    <td style="display:inline-block; width:355px;">
                        @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.TEMP_INFO1, inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault, New With {.htmlAttributes = New With {.class = "text-control", .maxlength = 255, .style = "width: 350px;"}})
                    </td>
                    <td>@inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault.DATA4</td>
                </tr>
                <tr Class="popup-row">
                    <td class="head-col label-control" style="width:90px;">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO2)</td>
                    <td width="20" style="display:inline-block;"></td>
                    <td style="display:inline-block; width:355px;">
                        @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.TEMP_INFO2, inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault, New With {.htmlAttributes = New With {.class = "text-control", .maxlength = 255, .style = "width: 350px;"}})
                    </td>
                    <td>@inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault.DATA4</td>
                </tr>
                <tr Class="popup-row">
                    <td class="head-col label-control" style="width:90px;">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO3)</td>
                    <td width="20" style="display:inline-block;"></td>
                    <td style="display:inline-block; width:355px;">
                        @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.TEMP_INFO3, inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault, New With {.htmlAttributes = New With {.class = "text-control", .maxlength = 255, .style = "width: 350px;"}})
                    </td>
                    <td>@inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault.DATA4</td>
                </tr>
                <tr Class="popup-row">
                    <td class="head-col label-control" style="width:90px;">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO4)</td>
                    <td width="20" style="display:inline-block;"></td>
                    <td style="display:inline-block; width:355px;">
                        @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.TEMP_INFO4, inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault, New With {.htmlAttributes = New With {.class = "text-control", .maxlength = 255, .style = "width: 350px;"}})
                    </td>
                    <td>@inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault.DATA4</td>
                </tr>
                <tr Class="popup-row">
                    <td class="head-col label-control" style="width:90px;">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO5)</td>
                    <td width="20" style="display:inline-block;"></td>
                    <td style="display:inline-block; width:355px;">
                        @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.TEMP_INFO5, inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault, New With {.htmlAttributes = New With {.class = "text-control", .maxlength = 255, .style = "width: 350px;"}})
                    </td>
                    <td>@inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault.DATA4</td>
                </tr>
                <tr Class="popup-row">
                    <td class="head-col label-control" style="width:90px;">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO6)</td>
                    <td width="20" style="display:inline-block;"></td>
                    <td style="display:inline-block; width:355px;">
                        @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.TEMP_INFO6, inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault, New With {.htmlAttributes = New With {.class = "text-control", .maxlength = 255, .style = "width: 350px;"}})
                    </td>
                    <td>@inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault.DATA4</td>
                </tr>
                <tr Class="popup-row">
                    <td class="head-col label-control" style="width:90px;">@Message.getMessage(CommonLabel.LABEL_TEMP_INFO7)</td>
                    <td width="20" style="display:inline-block;"></td>
                    <td style="display:inline-block; width:355px;">
                        @SearchInputItemHelper.InputItemFor(Me.Html, Function(model) model.Last.TEMP_INFO7, inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault, New With {.htmlAttributes = New With {.class = "text-control", .maxlength = 255, .style = "width: 350px;"}})
                    </td>
                    <td>@inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault.DATA4</td>
                </tr>
            </Table>

             <Table>
                 <tr><td>　</td></tr> @* 新規登録時、見目をよくするためのダミー行 *@
             </Table>
            <div Class="PopupButton" style="clear:both;">
                <span style="float:left;margin:15px 0 0 10%;">
                    <input id="modal-create-btn" class="defButton modal-create-btn submit-button" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnUpdate_onclick()">
                </span>
                <span style="float:right;margin:15px 10% 0 0;">
                    <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
                </span>
            </div>
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OpenFlg)

        @Html.HiddenFor(Function(model) model.Last.TEMP_ID)
        @Html.HiddenFor(Function(model) model.Last.REG_CONST_NO)
        @Html.HiddenFor(Function(model) model.Last.REL_CONST_NO)
        @Html.HiddenFor(Function(model) model.Last.TEMP_DT)
        @Html.HiddenFor(Function(model) model.Last.WORK_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.WORK_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.TEMP_REMARK)
        @Html.HiddenFor(Function(model) model.Last.TASK_TS)
        @Html.HiddenFor(Function(model) model.Last.INS_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_TASK)

    End Using
</div>
