@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of DepartmentViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelDepartment.TITLE)
End Code
<script>
    //
    //部署マスタメンテ画面
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
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

                //部署コードを書き込み可
                document.getElementById("DEPARTMENT_CD").readOnly = false;
                $("#DEPARTMENT_CD").removeClass("disable");

                //削除ボタンを非表示
                $("#modal-delete-btn").css("display", "none");

                //登録画面を表示
                ShowModalWindow()
                //$('.create-btn').trigger("click")
            } else {
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

                //部署コードを書き込み不可
                document.getElementById("DEPARTMENT_CD").readOnly = true;
                $("#DEPARTMENT_CD").removeClass("input-error");
                $("#DEPARTMENT_CD").addClass("disable");

                //削除ボタンを表示
                $("#modal-delete-btn").css("display", "block");

                //変更画面を表示
                ShowModalWindow()
                //$('.update-btn').trigger("click")
            }

            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")

        }

        btnCsvImportEnabled(true)       // 2022/7/25 sv sato add

        // 列定義
        let columns = [
            { id: "@ColKey.DepartmentList.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.DEPARTMENT", "@ColKey.DepartmentList.CHECK_SELECT", 40), sortOrder: -2, formatter: DepCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.DepartmentList.DEPARTMENT_CD", name: "@Message.getMessage(CommonLabel.LABEL_BUSHOCODE)", field: "DEPARTMENT_CD", width: getColumnWidth("@DispId.DEPARTMENT", "@ColKey.DepartmentList.DEPARTMENT_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("DEPARTMENT_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DEPARTMENT_CD").DispFlag"},
            { id: "@ColKey.DepartmentList.DEPARTMENT_NAME", name: "@Message.getMessage(CommonLabel.LABEL_BUSHONAME)", field: "DEPARTMENT_NAME", width: getColumnWidth("@DispId.DEPARTMENT", "@ColKey.DepartmentList.DEPARTMENT_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("DEPARTMENT_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DEPARTMENT_NAME").DispFlag"},
            { id: "@ColKey.DepartmentList.HEAD_OFFICE_FLG", name: "@Message.getMessage(CommonLabel.LABEL_HEAD_OFFICE_FLG)", field: "HEAD_OFFICE_FLG", width: getColumnWidth("@DispId.DEPARTMENT", "@ColKey.DepartmentList.HEAD_OFFICE_FLG", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("HEAD_OFFICE_FLG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("HEAD_OFFICE_FLG").DispFlag", formatter: HeadOfficeFlagFormatter},
            { id: "@ColKey.DepartmentList.ZIP_CODE", name: "@Message.getMessage(CommonLabel.LABEL_ZIP_CODE)", field: "ZIP_CODE", width: getColumnWidth("@DispId.DEPARTMENT", "@ColKey.DepartmentList.ZIP_CODE", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("ZIP_CODE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ZIP_CODE").DispFlag"},
            { id: "@ColKey.DepartmentList.ADDRESS", name: "@Message.getMessage(CommonLabel.LABEL_ADDRESS)", field: "ADDRESS", width: getColumnWidth("@DispId.DEPARTMENT", "@ColKey.DepartmentList.ADDRESS", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("ADDRESS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ADDRESS").DispFlag"},
            { id: "@ColKey.DepartmentList.REQ_DEP_NAME", name: "@Message.getMessage(CommonLabel.LABEL_REQ_DEP_NAME)", field: "REQ_DEP_NAME", width: getColumnWidth("@DispId.DEPARTMENT", "@ColKey.DepartmentList.REQ_DEP_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("REQ_DEP_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REQ_DEP_NAME").DispFlag"},
            { id: "@ColKey.DepartmentList.PHONE_NUMBER", name: "@Message.getMessage(CommonLabel.LABEL_PHONE_NUMBER)", field: "PHONE_NUMBER", width: getColumnWidth("@DispId.DEPARTMENT", "@ColKey.DepartmentList.PHONE_NUMBER", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("PHONE_NUMBER").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHONE_NUMBER").DispFlag"},
            { id: "@ColKey.DepartmentList.FAX", name: "@Message.getMessage(CommonLabel.LABEL_FAX)", field: "FAX", width: getColumnWidth("@DispId.DEPARTMENT", "@ColKey.DepartmentList.FAX", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("FAX").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FAX").DispFlag"},

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

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "DEPARTMENT_CD");
            dataView.endUpdate();
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列と変更列を加算
        }

        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.DEPARTMENT", fixCol);

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

    });

    // 本部/拠点用formatter 
    function HeadOfficeFlagFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (String(dataContext.HEAD_OFFICE_FLG) === "@HeadOfficeFlg.Cd.Honbu") {
            status = "@Message.getMessage(CommonLabel.VALUE_HEAD_OFFICE_FLG_Y)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_HEAD_OFFICE_FLG_N)"
        }

        return status;
    }

    function DepCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:center; height:100%;">' +
               '<input type="checkbox" class="slick-checkbox" ' +
               'value="' + dataContext.DEPARTMENT_CD + '" '+
               (value == 1 ? 'checked="checked"' : '') + ' />' +
               '</div>';
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
            url: '@Url.Content("~/Department")/ImportCsvFile',
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
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_ALL_CONFIRM)", "@Url.Content("~/Department")/CsvExport");

    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
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

        //部署コードを書き込み可
        document.getElementById("DEPARTMENT_CD").readOnly = false;
        $("#DEPARTMENT_CD").removeClass("disable");

        //削除ボタンを非表示
        $("#modal-delete-btn").css("display", "none");

        //画面を初期化する
        document.getElementById("DEPARTMENT_CD").value = "";
        document.getElementById("DEPARTMENT_NAME").value = "";
        // 2021/08/16 nv miyaji add start
        $("#HonbuFlg").prop("checked", false);
        document.getElementById("ZIP_CODE").value = "";
        document.getElementById("ADDRESS").value = "";
        document.getElementById("REQ_DEP_NAME").value = "";
        document.getElementById("PHONE_NUMBER").value = "";
        document.getElementById("FAX").value = "";
        // 2021/08/16 nv miyaji add end

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

        //部署コードを書き込み不可
        document.getElementById("DEPARTMENT_CD").readOnly = true;
        $("#DEPARTMENT_CD").removeClass("input-error");
        $("#DEPARTMENT_CD").addClass("disable");

        @* '--2021/09/27 nv miyaji update start *@
        // 権限による処理区分が「編集・照会可能」でない場合
        if("@ViewBag.AuthProcFlg" != "@AuthProcFlg.All"){
            // 変更ボタンを非表示
            $("#modal-create-btn").css("display", "none");
            // 削除ボタンを非表示
            $("#modal-delete-btn").css("display", "none");
        }else{
            //削除ボタンを表示
            $("#modal-delete-btn").css("display", "block");
        };
        ////削除ボタンを表示
        //$("#modal-delete-btn").css("display", "block");
        @* '--2021/09/27 nv miyaji update end *@

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/Department")/GetEditData',
                cache: false,
                data: { ID: text[0] },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }

                    //取得したデータを表示する
                    document.getElementById("DEPARTMENT_CD").value = data.DEPARTMENT_CD;
                    document.getElementById("DEPARTMENT_NAME").value = data.DEPARTMENT_NAME;
                    // 2021/08/16 nv miyaji add start
                    if (data.HEAD_OFFICE_FLG == "@HeadOfficeFlg.Cd.Honbu") {
                        $("#HonbuFlg").prop("checked", true);
                    }else{
                        $("#HonbuFlg").prop("checked", false);
                    }
                    document.getElementById("ZIP_CODE").value = data.ZIP_CODE;
                    document.getElementById("ADDRESS").value = data.ADDRESS;
                    document.getElementById("REQ_DEP_NAME").value = data.REQ_DEP_NAME;
                    document.getElementById("PHONE_NUMBER").value = data.PHONE_NUMBER;
                    document.getElementById("FAX").value = data.FAX;
                    // 2021/08/16 nv miyaji add end
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

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //登録ボタンクリック処理（確認メッセージ）
    function btnCreateModal_onclick(){
        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
            showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/Department")/Create")
        } else {
            showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/Department")/Create")
        }
    }

    //削除ボタンクリック処理
    function btnDelete_onclick() {
        //確認メッセージ
        showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/Department")/Delete");
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    ////検索条件の権限ラジオボタンクリック処理
    //function rdoSearchkengen_onclick(element) {
    //    //クリックされたラジオボタンの値を隠し項目に格納する
    //    document.getElementById("SearchKengen").value = element.value;

    //}

    ////モーダルの登録ボタンクリック処理
    //function modal_btnCreate_onclick(){
    //    //クリックされたラジオボタンの値を隠し項目に格納する
    //    document.getElementById("AUTH_DIV").value = $('input[name="AUTH_DIV"]:checked').val();
    //}

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

</script>

<div id="department-master">
    @Using (Html.BeginForm("Index", "Department", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    @* --2021/09/27 nv miyaji update start *@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @<input id="create-btn" Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                        @<input id="update-btn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">
                    Else
                        @<input id="show-btn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DISP)" onclick="btnEdit_onclick()">
                    End If
                    @*<input class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                    <input class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">*@
                    @* --2021/09/27 nv miyaji update end *@
                </div>
                <div class="view-area-button-right">
                    @* --2021/09/27 nv miyaji update start *@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                          @<input id = "csv-import-btn" Class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">
                    End If
                    @*<input class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">*@
                    @* --2021/09/27 nv miyaji update end *@
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

                             <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("DEPARTMENT_CD").DispFlag = "1", "display", "None"))">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_BUSHOCODE)</div>
                                 <div class="grid-condition-col">
                                     @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchCode, New With {.class = "cond-control", .maxlength = 15, .oninput = "editHanEi(this)"})
                                 </div>
                             </div>

                             <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("DEPARTMENT_NAME").DispFlag = "1", "display", "None"))">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_BUSHONAME)</div>
                                 <div class="grid-condition-col">
                                     @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchName, New With {.class = "cond-control", .maxlength = 60})
                                 </div>
                             </div>
                         </div>
                     </div>
                     <div class="view-area-label" style="height: 25px"></div>
                     <input class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                 </div>
             </div>
        </div>
        @*直近登録者情報*@
        @<div class="search-wrap">
            <div class="float-left mr50">@Message.getMessage(CommonLabel.LABEL_INS_DATETIME):@ViewBag.LastInsDateTime</div>
            <div class="float-left">@Message.getMessage(CommonLabel.LABEL_INS_EMPLOYEE_NAME):@ViewBag.LastInsEmployeeNm</div>
            <div style="clear:both;"></div>
        </div>
        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
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
                Dim csvSessionModel As DepartmentViewModel = Session(CommonSessionKey.CREAGE_DEPARTMENT)
                If Not IsNothing(csvSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(csvSessionModel)
                Else
                    Model.Add(New DepartmentViewModel())
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


                '登録・更新のモーダル画面
                @<div id="modal-content" class="modal-department">
                    <div class="popup-title-wrap">
                        <p class="Line3"></p>
                        <p class="Line4"></p>
                        <h2 id="popup-title"></h2>
                        <p class="Line3"></p>
                        <p class="Line4"></p>
                    </div>
                    @CODE
                        'モデルの値を設定
                        Dim SessionModel As DepartmentViewModel = Session(CommonSessionKey.CREAGE_DEPARTMENT)
                        If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                            Model.Add(SessionModel)
                        Else
                            Model.Add(New DepartmentViewModel())
                        End If
                    END CODE
                    @Html.AntiForgeryToken()


                    <Table>
                        <tr Class="popup-row">
                            <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_BUSHOCODE)</td>
                            <td width="20" class="require">※</td>
                            <td>@Html.EditorFor(Function(model) model.Last.DEPARTMENT_CD, New With {.htmlAttributes = New With {.maxlength = itemlength.DepartmentCd, .style = "width:200px;", .oninput = "editHanEi(this)"}})</td>
                        </tr>
                        <tr Class="popup-row">
                            <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_BUSHONAME)</td>
                            <td width="20" class="require">※</td>
                            <td>@Html.EditorFor(Function(model) model.Last.DEPARTMENT_NAME, New With {.htmlAttributes = New With {.maxlength = itemlength.DepartmentNm, .style = "width:790px;"}})</td>
                        </tr>
                        @* 2021/08/16 nv miyaji add start *@
                        <tr Class="popup-row">
                            <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_HEAD_OFFICE_FLG)</td>
                            <td width="20" class="require"></td>
                            <td>@Html.CheckBoxFor(Function(model) model.Last.HonbuFlg) @Message.getMessage(CommonLabel.LABEL_TO_HEAD_OFFICE)</td>
                        </tr>
                        <tr Class="popup-row">
                            <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_ZIP_CODE)</td>
                            <td width="20" class="require"></td>
                            <td>@Html.EditorFor(Function(model) model.Last.ZIP_CODE, New With {.htmlAttributes = New With {.maxlength = itemlength.ZipCode, .style = "width:200px;", .oninput = "editNumberKigou(this)"}})</td>
                        </tr>
                        <tr Class="popup-row">
                            <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_ADDRESS)</td>
                            <td width="20" class="require"></td>
                            <td>@Html.EditorFor(Function(model) model.Last.ADDRESS, New With {.htmlAttributes = New With {.maxlength = itemlength.Address, .style = "width:790px;"}})</td>
                        </tr>
                        <tr Class="popup-row">
                            <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_REQ_DEP_NAME)</td>
                            <td width="20" class="require"></td>
                            <td>@Html.EditorFor(Function(model) model.Last.REQ_DEP_NAME, New With {.htmlAttributes = New With {.maxlength = itemlength.ReqDepName, .style = "width:790px;"}})</td>
                        </tr>
                        <tr Class="popup-row">
                            <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_PHONE_NUMBER)</td>
                            <td width="20" class="require"></td>
                            <td>@Html.EditorFor(Function(model) model.Last.PHONE_NUMBER, New With {.htmlAttributes = New With {.maxlength = itemlength.PhoneNumber, .style = "width:200px;", .oninput = "editNumberKigou(this)"}})</td>
                        </tr>
                        <tr Class="popup-row">
                            <td width="200px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_FAX)</td>
                            <td width="20" class="require"></td>
                            <td>@Html.EditorFor(Function(model) model.Last.FAX, New With {.htmlAttributes = New With {.maxlength = itemlength.Fax, .style = "width:200px;", .oninput = "editNumberKigou(this)"}})</td>
                        </tr>
                        @* 2021/08/16 nv miyaji add end *@
                    </Table>

                    @*<div Class="PopupButton" style="margin:0 auto; width:500px;"> yamad*@
                    <div Class="PopupButton" style="margin:0 auto; width:560px;">
                        <span style="float:left;margin:15px 20px 0 60px;">
                            @*<input id="modal-create-btn" class="defButton modal-close modal-create-btn submit-button" formaction="~/Department/Create" type="submit" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)">*@
                            <input id="modal-create-btn" class="defButton  modal-create-btn submit-button" formaction="~/Department/Create" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnCreateModal_onclick()">
                        </span>
                        <span style="float:left;margin:15px 15% 0 0;">
                            <input id="modal-delete-btn" class="defButton submit-button" type="button" formaction="~/Department/Delete" name="Delete" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="return btnDelete_onclick()">
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
