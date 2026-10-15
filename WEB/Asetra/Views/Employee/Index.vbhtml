@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of EmployeeViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelEmployee.LABEL_TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
End Code
<script>
    //
    //社員マスタメンテ画面
    //

    //変更画面に値をセット中かどうか
    var isSettingValues = false;

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

        @*//ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"*@

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
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessHakko") {
            //発行
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_HAKKO)")

        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            if ("@ViewBag.AuthNoEditDep" == "@AUTH_FLG.Yes") {
                //部署変更権限がない場合、部署を非活性化
                $("#DEPARTMENT_CD").prop("disabled", true);
            }
            if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
                //登録画面を表示
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

                //IDを書き込み可
                document.getElementById("EMPLOYEE_CD").readOnly = false;
                $("#EMPLOYEE_CD").removeClass("disable");

                //削除ボタンを非表示
                $("#modal-delete-btn").css("display", "none");

                ShowModalWindow()
                //$('.create-btn').trigger("click")
            } else {
                //変更画面を表示
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

                //IDを書き込み不可
                document.getElementById("EMPLOYEE_CD").readOnly = true;
                $("#EMPLOYEE_CD").removeClass("input-error");
                $("#EMPLOYEE_CD").addClass("disable");

                // --2021/09/27 nv miyaji add start 照会のみの場合、権限以外変更できなくする。
                if("@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiShokaiNomi"){
                    document.getElementById("EMPLOYEE_NAME").readOnly = true;
                    $("#EMPLOYEE_NAME").addClass("disable");
                    $("#DispRetireFlg").attr("disabled", "disabled");
                    $("#DEPARTMENT_CD").attr("disabled", "disabled");
                    document.getElementById("MAIL_ADDRESS").readOnly = true;
                    $("#MAIL_ADDRESS").addClass("disable");
                    document.getElementById("PASSWORD").readOnly = true;
                    $("#PASSWORD").addClass("disable");
                    $("#AUTH_DIV").attr("disabled", "disabled");
                    for (iCnt = 0; iCnt < @CommonConst.MAX_HUBLOC_CNT; iCnt++){
                        $("select[name='HUB_LOC_L[" + iCnt + "]']").prop("disabled", true);
                        $("select[name='HUB_LOC_M[" + iCnt + "]']").prop("disabled", true);
                    }
                    //--2021/09/27 nv miyaji add end
                }else{
                    //削除ボタンを表示
                    $("#modal-delete-btn").css("display", "block");
                }

                ShowModalWindow()
                //$('.update-btn').trigger("click")
            }
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }else{
            //ロード前のボタン名を設定する
            document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"
        }

        btnCsvImportEnabled(true)       // 2022/7/25 sv sato add

        // ロケーション大区分非選択時は中区分を非活性
        if (document.getElementById("SearchHubLocLCd").value == "") {
            document.getElementById("SearchHubLocMCd").value = ""
            refreshSelect2("#SearchHubLocMCd");     //Select2の表示に反映
            $("#SearchHubLocMCd").attr("disabled", "disabled");
        }

        for (iCnt = 0; iCnt < @CommonConst.MAX_HUBLOC_CNT; iCnt++){
            if (document.getElementsByName("HUB_LOC_L[" + iCnt + "]")[0].value == "") {
                $("select[name='HUB_LOC_M[" + iCnt + "]']").prop("disabled", true);
            }
        }

        // 列定義
        let columns = [
            { id: "@ColKey.Employee.CHECK_SELECT", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_SELECT)</div>', field: "DISP_FLAG", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.CHECK_SELECT", 40), sortOrder: -2, formatter: EmployeeCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.Employee.EMPLOYEE_CD", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_EMPLOYEEID)</div>', field: "EMPLOYEE_CD", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.EMPLOYEE_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("EMPLOYEE_CD").DispFlag"},
            { id: "@ColKey.Employee.EMPLOYEE_NAME", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_EMPLOYEENM)</div>', field: "EMPLOYEE_NAME", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.EMPLOYEE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("EMPLOYEE_NAME").DispFlag"},
            { id: "@ColKey.Employee.RETIRE_FLG", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_RETIRE)</div>', field: "RETIRE_FLG", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.RETIRE_FLG", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("RETIRE_FLG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("RETIRE_FLG").DispFlag", formatter: RetireFlagFormatter},
            { id: "@ColKey.Employee.DEPARTMENT_CD", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_BUSHOCODE)</div>', field: "DEPARTMENT_CD", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.EMPLOYEE_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("DEPARTMENT_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DEPARTMENT_CD").DispFlag"},
            { id: "@ColKey.Employee.DEPARTMENT_NAME", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_BUSHONAME)</div>', field: "DEPARTMENT_NAME", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.DEPARTMENT_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("DEPARTMENT_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DEPARTMENT_NAME").DispFlag"},
            { id: "@ColKey.Employee.PASSWORD", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_PASSWORD)</div>', field: "PASSWORD", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.PASSWORD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("PASSWORD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PASSWORD").DispFlag", formatter: PasswordFormatter},
            { id: "@ColKey.Employee.AUTH_DIV", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_KENGEN)</div>', field: "AUTH_DIV", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.AUTH_DIV", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("AUTH_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("AUTH_DIV").DispFlag", formatter: AuthDivFormatter},
            { id: "@ColKey.Employee.HUB_LOC_L_NAME", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_SHOZOKU_KYOTEN)</div>', field: "HUB_LOC_L_NAME", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.HUB_LOC_L_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("HUB_LOC_L_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("HUB_LOC_L_NAME").DispFlag"},
            { id: "@ColKey.Employee.INS_DATETIME", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_INS_DATETIME)</div>', field: "INS_DATETIME", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.INS_DATETIME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("INS_DATETIME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INS_DATETIME").DispFlag", formatter: InsDatetimeFormatter},
            { id: "@ColKey.Employee.INS_EMPLOYEE_NAME", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_INS_EMPLOYEE_NAME)</div>', field: "INS_EMPLOYEE_NAME", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.INS_EMPLOYEE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("INS_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INS_EMPLOYEE_NAME").DispFlag"},
            { id: "@ColKey.Employee.UPD_DATETIME", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_UPD_DATETIME)</div>', field: "UPD_DATETIME", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.UPD_DATETIME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("UPD_DATETIME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("UPD_DATETIME").DispFlag", formatter: UpdDatetimeFormatter},
            { id: "@ColKey.Employee.UPD_EMPLOYEE_NAME", name: '<div style="text-align:center; width:100%;">@Message.getMessage(CommonLabel.LABEL_UPD_EMPLOYEE_NAME)</div>', field: "UPD_EMPLOYEE_NAME", width: getColumnWidth("@DispId.EMPLOYEE", "@ColKey.EMPLOYEE.UPD_EMPLOYEE_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("UPD_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("UPD_EMPLOYEE_NAME").DispFlag"},
        ];

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

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列を加算
        }

        // 選択状態を復元
        let lstCheckedKey = @Html.Raw(jsonCheckKeys);
        for (let i = 0; i < lstCheckedKey.length; i++) {
            // Key:社員コード
            var key = lstCheckedKey[i];

            for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                if (key === jsonData[grid_i].EMPLOYEE_CD) {
                    jsonData[grid_i].DISP_FLAG = lstCheckedKey[i]
                    break;
                }
            }
        }

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "EMPLOYEE_CD");
            dataView.endUpdate();
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.EMPLOYEE", fixCol);

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

        //ローカルストレージから復元
        const saved = localStorage.getItem("EmployeeConditions");
        if ("@Model.FirstOrDefault.IsRestoreOwnSearchConditions" == "True") {
            var lcd = document.getElementById("SearchHubLocLCd");
            //最初の要素（×での選択解除用に追加した空の選択肢は除く）
            var firstLoc = $(lcd).find("option").filter(function () { return this.value !== ""; })[0];
            if (saved) {
                const cond = JSON.parse(saved);
                if (!cond.SearchHubLocLCd || cond.SearchHubLocLCd.trim() === "") {
                    //ローカルストレージが空の場合は最初の要素を選択
                    if (firstLoc) {
                        lcd.value = firstLoc.value;
                    }
                } else {
                    //ローカルストレージが空でない場合
                    var found = false;
                    for (var i = 0; i < lcd.options.length; i++) {
                        //ローカルストレージに該当する値があれば選択
                        if (lcd.options[i].value == cond.SearchHubLocLCd) {
                            lcd.value = cond.SearchHubLocLCd;
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
            ChangeLocationL(lcd, document.getElementById("SearchHubLocMCd").value, true);
        }

    });

    //ローカルストレージ保存
    function localStorageSave() {
        let cond = { SearchHubLocLCd: document.getElementById("SearchHubLocLCd").value };

        //ローカルストレージを保存
        localStorage.setItem("EmployeeConditions", JSON.stringify(cond));
    }

    // チェックボックス(中央寄せ)
    function EmployeeCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = dataContext.EMPLOYEE_CD;
        let isKeyInclude = (dataContext[columnDef.field] === key);
        let upd = (dataContext.UPD_FLG === "@AUTH_FLG.Yes");

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheck" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" '+
            (isKeyInclude ? 'checked="checked" ' : '') +
            (upd ? '' : 'disabled="disabled" ') +
               'onchange="check_onclick(this)" ' +
               '/>' +
               '</div>';
    }

    //退職フラグ用formatter
    function RetireFlagFormatter(row, cell, value, columnDef, dataContext) {
        var val = null;
        if (dataContext.RETIRE_FLG == "@RetireFlg.Cd.Yes") {
            val="@Message.getMessage(CommonLabel.VALUE_SUMI)"
        }
        //テキスト（中央寄せ）
        if (val != null) {
            return '<div style="text-align:center; width:100%;">' + val + '</div>';
        }
    }

    //パスワード用formatter
    function PasswordFormatter(row, cell, value, columnDef, dataContext) {
        let password = convertToEmptyIfNull(dataContext.PASSWORD)
        if (password != '') {
            return '*'.repeat(password.length)
        }
    }

    //権限区分用formatter AUTH_DIV
    function AuthDivFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        switch(dataContext.AUTH_DIV) {
            case "@AUTH_DIV.Nomal":
                status = '@Message.getMessage(CommonLabel.LABEL_KENGEN_NOMAL)'
                break;
            case "@AUTH_DIV.Admin":
                status = '@Message.getMessage(CommonLabel.LABEL_KENGEN_ADMIN)'
                break;
        }
        return status
    }

    //登録日時用formatter　INS_DATETIME
    function InsDatetimeFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value)
        if (status != '') {
            status = status.replace('T', ' ')
            status = status.replaceAll('-', '/')
            return status.substr(0, 19)
        }
    }

    //最終登録更新日時用formatter  UPD_DATETIME
    function UpdDatetimeFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value)
        if (status != '') {
            status = status.replace('T', ' ')
            status = status.replaceAll('-', '/')
            return status.substr(0, 19)
        }
    }

    //大区分選択
    function ChangeLocationL(locL, initialM, doAutoSearch) {
        // 値セット中は処理しない
        if (isSettingValues) return;

        var indexNo = 0
        // ※中区分は Select2 のため、値を変更したら refreshSelect2 で表示に反映する
        if (locL.name == "SearchHubLocLCd"){
            // 中区分をクリア、編集不可
            $("#SearchHubLocMCd").val("");
            refreshSelect2("#SearchHubLocMCd");
            $("#SearchHubLocMCd").attr("disabled", "disabled");

        }else{
            // 中区分をクリア、編集不可
            indexNo = locL.name.replace("HUB_LOC_L[", "").replace("]", "")
            $("select[name='HUB_LOC_M[" + indexNo + "]']").val("");
            refreshSelect2("select[name='HUB_LOC_M[" + indexNo + "]']");
            $("select[name='HUB_LOC_M[" + indexNo + "]']").prop("disabled", true);
        }

         //選択されていない場合はここで終了
        if (!locL.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Employee")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationM", selectLCode: locL.value, controlName: locL.name },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocM = ""
                    var indexNo=0
                    if (locL.name == "SearchHubLocLCd") {
                        //検索条件
                        SearchLocM = document.getElementById("SearchHubLocMCd");
                    } else {
                        //所属拠点　入力欄
                        indexNo = locL.name.replace("HUB_LOC_L[", "").replace("]", "")
                        SearchLocM = document.getElementsByName("HUB_LOC_M[" + indexNo + "]")[0];
                    }

                    //選択肢をセット
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
                //　編集可にする
                if (locL.name == "SearchHubLocLCd") {
                    $("#SearchHubLocMCd").removeAttr("disabled");
                } else {
                    $("select[name='HUB_LOC_M[" + indexNo + "]']").prop("disabled", false);
                }

                if (initialM) {
                    $(SearchLocM).val(initialM);
                }
                // 選択肢の入れ替え・値をSelect2の表示に反映
                refreshSelect2(SearchLocM);

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
            url: '@Url.Content("~/Employee")/ImportCsvFile',
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
                    case @DataInport.DataInportResultCd.DataNG:
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
                                //$("#csv-err-ichiran").append('<tr><td class="col1">'+ data[icnt].ErrRowIdx +'</td><td class="col2" >'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" >'+ data[icnt].ErrGaiyou +'</td></tr>');
                                $("#csv-err-ichiran").append('<tr><td class="col1" title=' + data[icnt].ErrRowIdx + '>'+ data[icnt].ErrRowIdx +'</td><td class="col2" title=' + data[icnt].ErrKoumoku + '>'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" title=' + data[icnt].ErrGaiyou + '>'+ data[icnt].ErrGaiyou +'</td></tr>');
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
        showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.CSVEXPORT_ALL_CONFIRM)", "@Url.Content("~/Employee")/CsvExport");

    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        // ロケーション(会社)必須チェック
        var locL = document.getElementById("SearchHubLocLCd").value;
        if (locL.trim() === "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERRNO_SELECT_HUB_LOC_L)")
            return false;
        }

        //画面ブロックを表示
        showLoading();

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
    //    document.getElementById("SearchKengen_KyotenN").value = "";
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

        isSettingValues = true;     //値セット中

        //IDを書き込み可
        document.getElementById("EMPLOYEE_CD").readOnly = false;
        $("#EMPLOYEE_CD").removeClass("disable");

        //削除ボタンを非表示
        $("#modal-delete-btn").css("display", "none");

        document.getElementById("EMPLOYEE_CD").value = ""
        document.getElementById("EMPLOYEE_NAME").value = ""
        document.getElementById("DEPARTMENT_CD").value = ""
        document.getElementById("PASSWORD").value = ""
        $('[id=AUTH_DIV_NOMAL]').prop('checked', true);
        $('[id=AUTH_DIV_ADMIN]').prop('checked', false);
        // 2021/09/02 nv miyaji add start
        $("#DispRetireFlg").prop("checked", false);
        document.getElementById("MAIL_ADDRESS").value = "";

        if ("@ViewBag.AuthNoEditDep" == "@AUTH_FLG.Yes") {
            //部署変更権限がない場合
            $("#DEPARTMENT_CD").attr("disabled", "disabled");

            var dep = document.getElementById("DEPARTMENT_CD");
            for (var i = 0; i < dep.options.length; i++) {
                //固定値「拠点」があれば選択
                if (dep.options[i].value == "@DepartmentCD.DEP_HUB") {
                    dep.value =  "@DepartmentCD.DEP_HUB";
                    break;
                }
            }
        } else {
            $("#DEPARTMENT_CD").removeAttr("disabled");
        }
        //部署の×（クリアボタン）の表示を更新する（値・入力可否を JS で変更したため）
        refreshClearable("#DEPARTMENT_CD");

        for (iCnt = 0; iCnt < @CommonConst.MAX_HUBLOC_CNT; iCnt++) {
            // ※会社・原価センタは Select2 のため、値を変更したら refreshSelect2 で表示に反映する
            var elemL = document.getElementsByName("HUB_LOC_L[" + iCnt + "]")[0];
            $(elemL).val("");
            refreshSelect2(elemL);
            var elemM = document.getElementsByName("HUB_LOC_M[" + iCnt + "]")[0];
            $(elemM).val("");
            refreshSelect2(elemM);
            $("select[name='HUB_LOC_M[" + iCnt + "]']").prop("disabled", true);
        }
        // 2021/09/02 nv miyaji add end

        isSettingValues = false;        //値セット中を解除

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        //登録ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_CREATE_CLICK";

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

        //IDを書き込み不可
        document.getElementById("EMPLOYEE_CD").readOnly = true;
        $("#EMPLOYEE_CD").removeClass("input-error");
        $("#EMPLOYEE_CD").addClass("disable");

        @* '--2021/09/27 nv miyaji update start *@
        // 権限による処理区分が「編集・照会可能」でない場合
        if ("@ViewBag.AuthProcFlg" == "@AuthProcFlg.All" || "@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiAll"){
            //削除ボタンを表示
            $("#modal-delete-btn").css("display", "block");
        }else{
            // 変更ボタンを非表示
            $("#modal-create-btn").css("display", "none");
            // 削除ボタンを非表示
            $("#modal-delete-btn").css("display", "none");
        };
        ////削除ボタンを表示
        //$("#modal-delete-btn").css("display", "block");
        @* '--2021/09/27 nv miyaji update end *@

        if ("@ViewBag.AuthNoEditDep" == "@AUTH_FLG.Yes") {
            $("#DEPARTMENT_CD").attr("disabled", "disabled");
        } else {
            $("#DEPARTMENT_CD").removeAttr("disabled");
        }

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/Employee")/GetEditData',
                cache: false,
                data: { ID: text[0] },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }

                    //取得したデータを表示する
                    isSettingValues = true;     //値セット中
                    document.getElementById("EMPLOYEE_CD").value = data.EMPLOYEE_CD;
                    document.getElementById("EMPLOYEE_NAME").value = data.EMPLOYEE_NAME;
                    document.getElementById("DEPARTMENT_CD").value = data.DEPARTMENT_CD;
                    document.getElementById("PASSWORD").value = data.PASSWORD;
                    // 2021/09/02 nv miyaji add start
                    if (data.RETIRE_FLG == "@RetireFlg.Cd.Yes") {
                        $("#DispRetireFlg").prop("checked", true);
                    }else{
                        $("#DispRetireFlg").prop("checked", false);
                    }
                    document.getElementById("MAIL_ADDRESS").value = data.MAIL_ADDRESS;

                    //所属拠点
                    for (var iCnt = 0; iCnt < @CommonConst.MAX_HUBLOC_CNT; iCnt++) {
                        //所属拠点大区分
                        var LocL = document.getElementsByName("HUB_LOC_L[" + iCnt + "]")[0];
                        if (LocL) {
                            //選択肢をセット
                            if (data.hubLocLDropdownDatas[iCnt] != null) {
                                LocL.options.length = 0;
                                let opBlank = document.createElement("option");
                                opBlank.value = "";
                                opBlank.text = "";
                                LocL.appendChild(opBlank);

                                for (var i = 0; i < data.hubLocLDropdownDatas[iCnt].length; i++) {
                                    let op = document.createElement("option");
                                    op.value = data.hubLocLDropdownDatas[iCnt][i].LOC_CD;
                                    op.text = data.hubLocLDropdownDatas[iCnt][i].LOC_NAME;
                                    LocL.appendChild(op);
                                }
                            }
                            //値をセット（Select2の表示に反映）
                            $(LocL).val(data.HUB_LOC_L[iCnt]);
                            refreshSelect2(LocL);
                        }

                        //所属拠点中区分
                        var LocM = document.getElementsByName("HUB_LOC_M[" + iCnt + "]")[0];
                        if (LocM) {
                            if (data.HUB_LOC_L[iCnt] && data.HUB_LOC_L[iCnt].trim() !== "") {
                                //所属拠点大区分が入っている場合
                                //選択肢をセット
                                if (data.hubLocMDropdownDatas[iCnt] != null) {
                                    LocM.options.length = 0;
                                    let opBlank = document.createElement("option");
                                    opBlank.value = "";
                                    opBlank.text = "";
                                    LocM.appendChild(opBlank);

                                    for (var j = 0; j < data.hubLocMDropdownDatas[iCnt].length; j++) {
                                        let op = document.createElement("option");
                                        op.value = data.hubLocMDropdownDatas[iCnt][j].LOC_CD;
                                        op.text = data.hubLocMDropdownDatas[iCnt][j].LOC_NAME;
                                        LocM.appendChild(op);
                                    }
                                }
                                //値をセット（Select2の表示に反映）
                                $(LocM).val(data.HUB_LOC_M[iCnt]);
                                refreshSelect2(LocM);
                                //編集可にする
                                $("select[name='HUB_LOC_M[" + iCnt + "]']").prop("disabled", false);
                            } else {
                                //値を空白にする（Select2の表示に反映）
                                $(LocM).val("");
                                refreshSelect2(LocM);
                                //編集不可とする
                                $("select[name='HUB_LOC_M[" + iCnt + "]']").prop("disabled", true);
                            }
                        }
                    }

                    //document.getElementById("AUTH_DIV").value = data.AUTH_DIV;
                    document.getElementById("INS_DATETIME").value = formatDate(new Date(data.InsDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("INS_EMPLOYEE_CD").value = data.INS_EMPLOYEE_CD;
                    document.getElementById("INS_EMPLOYEE_NAME").value = data.INS_EMPLOYEE_NAME;
                    document.getElementById("UPD_DATETIME").value = formatDate(new Date(data.UpdDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("UPD_EMPLOYEE_CD").value = data.UPD_EMPLOYEE_CD;
                    document.getElementById("UPD_EMPLOYEE_NAME").value = data.UPD_EMPLOYEE_NAME;
                    document.getElementById("UPD_TASK").value = data.RenewProcess;

                    //権限ラジオボタンのチェック状態設定
                    if (data.AUTH_DIV == "@AUTH_DIV.Admin") {
                        $('[id=AUTH_DIV_ADMIN]').prop('checked', true);
                    } else {
                        $('[id=AUTH_DIV_NOMAL]').prop('checked', true);
                    }

                    // --2021/09/27 nv miyaji add start 拠点責任者の場合、権限以外変更できなくする。
                    if ("@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiShokaiNomi") {
                        document.getElementById("EMPLOYEE_NAME").readOnly = true;
                        $("#EMPLOYEE_NAME").addClass("disable");
                        $("#DispRetireFlg").attr("disabled", "disabled");
                        $("#DEPARTMENT_CD").attr("disabled", "disabled");
                        document.getElementById("MAIL_ADDRESS").readOnly = true;
                        $("#MAIL_ADDRESS").addClass("disable");
                        document.getElementById("PASSWORD").readOnly = true;
                        $("#PASSWORD").addClass("disable");
                        $("#AUTH_DIV").attr("disabled", "disabled");
                        for (iCnt = 0; iCnt < @CommonConst.MAX_HUBLOC_CNT; iCnt++) {
                            $("select[name='HUB_LOC_L[" + i + "]']").prop("disabled", true);
                            $("select[name='HUB_LOC_M[" + i + "]']").prop("disabled", true);
                        }
                        //--2021/09/27 nv miyaji add end
                    }
                    //部署の×（クリアボタン）の表示を更新する（値・入力可否を JS で変更したため）
                    refreshClearable("#DEPARTMENT_CD");
                    isSettingValues = false;    //値セット中解除

                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }

        //変更ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_UPDATE_CLICK";

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //登録ボタンクリック処理（確認メッセージ）
    function btnCreateModal_onclick() {
        // 部署の編集権限がない場合、非活性項目を活性化させてから保存する。
        if ("@ViewBag.AuthNoEditDep" == "@AUTH_FLG.Yes") {
            var msg=""
            if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
                msg="@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)"
            } else {
                msg ="@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)"
            }
            //確認メッセージを出し、ＯＫなら登録処理
            // ダイアログのメッセージを設定
            $("#show-dialog").html(msg);

            // ダイアログを作成
            $("#show-dialog").dialog({
                modal: true,
                title: strConfirmTitle,
                dialogClass: "show-confirm-wrap",
                buttons: {
                    "OK": function () {
                        //部署を活性化
                        $("#DEPARTMENT_CD").prop("disabled", false);

                        var befAction = $('#form').attr("action");
                        $(this).dialog("close");
                        $('#form').attr("action", "@Url.Content("~/Employee")/Create");
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
        } else {
            if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
                showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/Employee")/Create")
            } else {
               showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/Employee")/Create")
            }
        }
    }

    //削除ボタンクリック処理
    function btnDelete_onclick() {
        // 部署の編集権限がない場合、非活性項目を活性化させてから削除する。
        if ("@ViewBag.AuthNoEditDep" == "@AUTH_FLG.Yes") {
            var msg="@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)"
            //確認メッセージを出し、ＯＫなら削除処理
            // ダイアログのメッセージを設定
            $("#show-dialog").html(msg);

            // ダイアログを作成
            $("#show-dialog").dialog({
                modal: true,
                title: strConfirmTitle,
                dialogClass: "show-confirm-wrap",
                buttons: {
                    "OK": function () {
                        //部署を活性化
                        $("#DEPARTMENT_CD").prop("disabled", false);

                        var befAction = $('#form').attr("action");
                        $(this).dialog("close");
                        $('#form').attr("action", "@Url.Content("~/Employee")/Delete");
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
        } else {
            showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/Employee")/Delete");
        }
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        //画面ブロックを表示
        showLoading();

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //モーダルダイアログの戻るボタンクリック処理
    function btnModalClose_Click() {
        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
    }

    //タグラベル発行
    function btnTagLabel_onclick() {
        document.getElementById("popup-title-prt").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SELECT_PRINTER)";


        setting_checklist();

        if (document.getElementById("SelectKeys").value == "") { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Employee")/ShowPrinterList',
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

        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/Employee")/CsvExportForLabel");
        $('#form').submit();
        $('#form').attr("action", befAction);
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
<style type="text/css">
    /* モーダルの×付きプルダウン（部署）
       プルダウンの幅に合わせて×の位置を決める（×がプルダウンの枠外に出ないようにする） */
    #modal-content .clearable {
        display: inline-block;
        max-width: 100%;
    }
</style>

<div id="employee-master">
    @Using (Html.BeginForm("Index", "Employee", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    @* --2021/09/27 nv miyaji update start *@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @<input id="create-btn" Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                        @<input id="update-btn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">
                    ElseIf ViewBag.AuthProcFlg = AuthProcFlg.JiAll Then
                        @<input id="create-btn" Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                        @<input id="update-btn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">
                    Else
                        @<input id="show-btn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DISP)" onclick="btnEdit_onclick()">
                    End If
                    @*<input class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                        <input class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">*@
                    @* --2021/09/27 nv miyaji update end *@
                </div>
                @* タグ発行不要 *@
                @*<input Class="defButton update-btn" type="button" id="btnPrintTag" style="float: left; margin-left: 20px;" value="@Message.getMessage(CommonLabel.BUTTON_PRINT_TAG)" onclick="btnTagLabel_onclick()">*@
                <div class="view-area-button-right">
                    @* --2021/09/27 nv miyaji update start *@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @<input id="csv-import-btn" Class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">
                    ElseIf ViewBag.AuthProcFlg = AuthProcFlg.JiAll Then
                        @<input id="csv-import-btn" Class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">
                    End If
                    @*<input class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">*@
                    @* --2021/09/27 nv miyaji update end *@
                    <input id="double_submit_ok" class="defButton csv-export-btn" type="button" name="CsvExport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL)" onclick="btnCsvExport_onclick()">
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
                            <div Class="col-sm-2 filter-combo-area" style="display: block">
                                <div Class="grid-head-col">@Message.getMessage(CommonLabel.VALUE_LOC_L_COM)<font Class="label-require">※</font></div>
                                <div Class="grid-condition-col">
                                    @* select2-clearable クラスで入力して絞り込めるSelect2になる（Scripts\select2-common.js）。選択肢・選択後とも「コード 名称」を表示 *@
                                    @* ×で選択解除するため、空の選択肢を追加している（未選択での検索は btnSearch_onclick の必須チェックでエラー） *@
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchHubLocLCd, DirectCast(ViewBag.lstLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control select2-clearable", .onchange = "ChangeLocationL(this)"})
                                </div>
                            </div>

                            <div Class="col-sm-2 filter-combo-area" style="display: block">
                                <div Class="grid-head-col">@Message.getMessage(CommonLabel.VALUE_LOC_M_DEP)</div>
                                <div Class="grid-condition-col">
                                    @* 絞り込み機能付きの単一選択（select2-clearable）。選択肢は会社の選択時に JS（ChangeLocationL）で入れ替える *@
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchHubLocMCd, DirectCast(ViewBag.lstLocM, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control select2-clearable"})
                                </div>
                            </div>

                            <div Class="col-sm-2" style="display:block">
                                <div Class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_EMPLOYEEID)</div>
                                <div Class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchCode, New With {.class = "cond-control", .maxlength = 15, .oninput = "editHanEi(this)"})
                                </div>
                            </div>

                            <div Class="col-sm-2" style="display:block">
                                <div Class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_EMPLOYEENM)</div>
                                <div Class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>

                            <div Class="col-sm-2" style="height: 56px;">
                                <div Class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_RETIREE)</div>
                                <div Class="grid-condition-col">
                                    <div Class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchRetire)@Message.getMessage(CommonLabel.LABEL_FUKUMERU)</div>
                                </div>
                            </div>

                            <div Class="col-sm-2" style="display: block">
                                <div Class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_BUSHO)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchBusho, DirectCast(ViewBag.DepartmentList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>                                                       
                            <div Class="col-sm-2" style="display:block;max-width:300px;">
                                <div Class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_KENGEN)</div>
                                <div class="grid-condition-col">
                                    <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchKengen_KyotenN, "")@Message.getMessage(CommonLabel.LABEL_SELECT_ALL)</div>
                                    <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchKengen_KyotenN, SHOSAI_AUTH_DIV.Tsujo_Admin)@Message.getMessage(CommonLabel.LABEL_KENGEN_ADMIN)</div>
                                    <div class="input-select-item-last">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchKengen_KyotenN, SHOSAI_AUTH_DIV.Tsujo_Nomal)@Message.getMessage(CommonLabel.LABEL_KENGEN_NOMAL)</div>
                                </div>
                            </div>                                                        
                        </div>
                    </div>
                    <div class="view-area-label" style="height: 25px"></div>
                    <input class="float-right defButton submit-button" id="Search_Button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>

            </div>
        </div>

        @<input type="hidden" id="IsRestoreOwnSearchConditions" name="IsRestoreOwnSearchConditions" value="@Model.FirstOrDefault().IsRestoreOwnSearchConditions.ToString()" />
        @<input type="hidden" id="FirstSearch" name="FirstSearch" value="@Model.FirstOrDefault().FirstSearch.ToString()" />

        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div class="list-view">
            @* 高さはヘッダー固定の処理（enableFixedHeader）で画面の残り領域に合わせる *@
            <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
        </div>

        'チェックボックス選択値の引き渡し用隠し項目
        @<input id="SelectKeys" name="SelectKeys" type="hidden">
        @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden" />
        'プリンタ選択モーダル画面
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
                Dim prtSessionModel As EmployeeViewModel = Session(CommonSessionKey.CREAGE_EMPLOYEE)
                If Not IsNothing(prtSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(prtSessionModel)
                Else
                    Model.Add(New EmployeeViewModel())
                End If
                Dim lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
                Dim CheckedCount As String = "0"
                If Not IsNothing(lstCheckedKey) Then
                    CheckedCount = lstCheckedKey.Count.ToString
                End If
                @*@Html.Label(CheckedCount, New With {.id = "selectcount", .style = "margin-right:15px;font-size:12px;"})*@
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


                'CSV取込のモーダル画面
        @<div id="modal-csv-content">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title-csv"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
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


                '登録・更新のモーダル画面
        @<div id="modal-content" class="modal-Employee">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
            @CODE
                'モデルの値を設定
                Dim SessionModel As EmployeeViewModel = Session(CommonSessionKey.CREAGE_EMPLOYEE)
                If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(SessionModel)
                Else
                    Model.Add(New EmployeeViewModel())
                End If
            END CODE
            @Html.AntiForgeryToken()


            <Table>
                <tr Class="popup-row">
                    <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_EMPLOYEEID)</td>
                    <td width="20"><font Class="require">※</font></td>
                    <td>@Html.EditorFor(Function(model) model.Last.EMPLOYEE_CD, New With {.htmlAttributes = New With {.maxlength = 15, .style = "width:200px;", .oninput = "editHanEi(this)"}})</td>
                </tr>
                <tr Class="popup-row">
                    <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_EMPLOYEENM)</td>
                    <td width="20"><font Class="require">※</font></td>
                    <td>@Html.EditorFor(Function(model) model.Last.EMPLOYEE_NAME, New With {.htmlAttributes = New With {.maxlength = 60, .style = "width:790px;"}})</td>
                </tr>
                <tr Class="popup-row">
                    @* 2021/09/02 nv miyaji add *@
                    <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_RETIRE)</td>
                    <td width="20" class="require"></td>
                    <td>@Html.CheckBoxFor(Function(model) model.Last.DispRetireFlg) @Message.getMessage(CommonLabel.LABEL_RETIRED)</td>
                </tr>
                <tr Class="popup-row">
                    <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_BUSHO)</td>
                    <td width="20"><font Class="require">※</font></td>
                    <td>
                        @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加。未選択は登録時に必須エラー） *@
                        <span class="clearable">
                            @Html.DropDownListFor(Function(model) model.Last.DEPARTMENT_CD, DirectCast(ViewBag.modalDepartmentList, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 810px;"})
                            <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                        </span>
                    </td>
                </tr>
                <tr Class="popup-row">
                    @* 2021/09/02 nv miyaji add *@
                    <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_MAIL_ADDRESS)</td>
                    <td width="20"></td>
                    <td>@Html.EditorFor(Function(model) model.Last.MAIL_ADDRESS, New With {.htmlAttributes = New With {.maxlength = itemlength.MailAddress, .style = "width:350px;", .oninput = "editPassword(this)", .autocomplete = "off"}})</td>
                </tr>
                <tr Class="popup-row">
                    <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_PASSWORD)</td>
                    <td width="20"></td>
                    <td>@Html.EditorFor(Function(model) model.Last.PASSWORD, New With {.htmlAttributes = New With {.maxlength = 60, .style = "width:790px;", .oninput = "editPassword(this)", .autocomplete = "off"}})</td>
                </tr>
                <tr Class="popup-row">
                    <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_KENGEN)</td>
                    <td width="20"><font Class="require">※</font></td>
                    <td width="120px" style="display:inline-block;">
                        @Html.RadioButtonFor(Function(model) model.Last.AUTH_DIV, AUTH_DIV.Admin, New With {.id = "AUTH_DIV_ADMIN"})@Message.getMessage(CommonLabel.LABEL_KENGEN_ADMIN)
                    </td>
                    <td width="120px" style="display:inline-block;">
                        @Html.RadioButtonFor(Function(model) model.Last.AUTH_DIV, AUTH_DIV.Nomal, New With {.id = "AUTH_DIV_NOMAL"})@Message.getMessage(CommonLabel.LABEL_KENGEN_NOMAL)
                    </td>
                </tr>
            </Table>

            @* 2021/09/06 nv miyaji add start *@
            <div style="display:inline-block;height:125px; overflow-y:scroll;margin-left:-3px;margin-top:3px">

                <Table style="margin-left:155px;">
                    <tr class="grid-head-row">
                        <td width="20"></td>
                        <td width="270px">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.VALUE_LOC_L_COM)</div>
                        </td>
                        <td width="20"></td>
                        <td width="270px">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.VALUE_LOC_M_DEP)</div>
                        </td>
                    </tr>

                    @for icnt As Integer = 0 To CommonConst.MAX_HUBLOC_CNT - 1
                        @<tr Class="popup-row">
                            @If icnt = 0 Then
                                @<td width="20"><font Class="require">※</font></td>
                            Else
                                @<td width="20"></td>
                            End If

                             <td width="270px">
                                 @* 絞り込み機能付きの単一選択（select2-clearable）。選択肢・選択後とも「コード 名称」を表示 *@
                                 @Html.DropDownListFor(Function(model) model.Last.HUB_LOC_L(icnt), DirectCast(ViewBag.LocLOptions(icnt), IEnumerable(Of SelectListItem)), "", New With {.class = "select2-clearable", .style = "width: 270px;", .onchange = "ChangeLocationL(this)"})
                                 @* 所属拠点大区分コードリストに確実にデータが作成されるようにするため、非表示で代入する *@
                                 @Html.HiddenFor(Function(model) model.Last.HUB_LOC_L(icnt))
                             </td>

                            @If icnt = 0 Then
                                @<td width="20"><font Class="require">※</font></td>
                            Else
                                @<td width="20"></td>
                            End If

                            <td width="270px">
                                @* 絞り込み機能付きの単一選択（select2-clearable）。選択肢は会社の選択時に JS（ChangeLocationL）で入れ替える *@
                                @Html.DropDownListFor(Function(model) model.Last.HUB_LOC_M(icnt), DirectCast(ViewBag.LocMOptions(icnt), IEnumerable(Of SelectListItem)), "", New With {.class = "select2-clearable", .style = "width: 270px;"})
                                @* 所属拠点中区分コードリストに確実にデータが作成されるようにするため、非表示で代入する *@
                                @Html.HiddenFor(Function(model) model.Last.HUB_LOC_M(icnt))
                            </td>
                        </tr>
                    Next
                </Table>
            </div>
            @* 2021/09/06 nv miyaji add end *@

            @*<div Class="PopupButton" style="margin:0 auto; width:500px;"> yamada*@
            <div Class="PopupButton" style="margin:0 auto; width:560px;">
                <span style="float:left;margin:15px 20px 0 60px;">
                    @*<input id="modal-create-btn" class="defButton modal-close modal-create-btn submit-button" formaction="~/Employee/Create" type="submit" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" >*@
                    <input id="modal-create-btn" class="defButton  modal-create-btn submit-button" formaction="~/Employee/Create" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnCreateModal_onclick()">
                </span>
                <span style="float:left;margin:15px 15% 0 0;">
                    <input id="modal-delete-btn" class="defButton submit-button" type="button" formaction="~/Employee/Delete" name="Delete" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="return btnDelete_onclick()">
                </span>
                <span style="float:left;margin:15px 0%;">
                    <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)"> @* 戻るボタン押下時、更新しないと次のモーダル表示がおかしくなるのでsubmitに変更*@
                    @*<input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">*@
                </span>
            </div>
        </div>
        @Html.HiddenFor(Function(model) model.Last.INS_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.Last.UPD_TASK)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.SearchKengen_KyotenN)
        @*@Html.HiddenFor(Function(model) model.FirstOrDefault.SearchKengen_KyotenY)*@
    End Using
</div>

