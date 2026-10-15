@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of CalendarViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelCalendar.TITLE)
End Code
<script>
    //
    //カレンダーマスタメンテ画面
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

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        btnCsvImportEnabled(true)       // 2022/7/25 sv sato add

        // 列定義
        let columns = [
             { id: "@ColKey.CALENDAR.DT", name: '@Message.getMessage(CommonLabel.LABEL_YEAR_MONTH_DAY)', field: "DT", width: getColumnWidth("@DispId.CALENDAR", "@ColKey.CALENDAR.DT", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DT").DispFlag", formatter: DtFomatter},
             { id: "@ColKey.CALENDAR.DAY_OF_WEEK", name: '@Message.getMessage(CommonLabel.LABEL_YOBI)', field: "DT", width: getColumnWidth("@DispId.CALENDAR", "@ColKey.CALENDAR.DAY_OF_WEEK", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("DAY_OF_WEEK").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DAY_OF_WEEK").DispFlag", formatter: DayOfWeekFomatter},
             { id: "@ColKey.CALENDAR.NOT_WORK_FLG", name: '@Message.getMessage(CommonLabel.LABEL_NOT_WORK_FLG_Y)', field: "NOT_WORK_FLG", width: getColumnWidth("@DispId.CALENDAR", "@ColKey.CALENDAR.NOT_WORK_FLG", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("NOT_WORK_FLG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("NOT_WORK_FLG").DispFlag", formatter: NotWorkFlagY},

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

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol       //選択列を加算
        }

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "DT");
            dataView.endUpdate();
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.CALENDAR", fixCol);

    });

    //年月日用fomatter
    function DtFomatter(row, cell, value, columnDef, dataContext) {
        let status = String(value)
        if (convertToEmptyIfNull(status) != '0') {
            let yyyy = status.substr(0, 4) + '/'
            let mm = status.substr(4, 2) + '/'
            let dd = status.substr(6, 2)
            if (mm[0] == '0') {
                mm = mm[1] + '/'
            }
            if (dd[0] == '0') {
                dd = dd[1]
            }
            return '<div style="text-align:center; width:100%;">' + yyyy + mm + dd + '</div>'
        }
    }

    //曜日用fomatter　
    function DayOfWeekFomatter(row, cell, value, columnDef, dataContext) {
        let dayOfWeekStr = [ "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" ];
        let status = String(value)

        if (convertToEmptyIfNull(status) != '0') {
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            yyyy = Number(yyyy)
            if (mm[0] == '0') {
                mm = Number(mm[1]) - 1
            }
            if (dd[0] == '0') {
                dd = Number(dd[1])
            }
            var date = new Date( yyyy, mm , dd);
            return '<div style="text-align:center; width:100%;">' + dayOfWeekStr[date.getDay()] + '</div>'
        }
    }

    //非稼働日用formatter
    function NotWorkFlagY(row, cell, value, columnDef, dataContext) {
        if (dataContext.NOT_WORK_FLG == '@NotWorkFlg.Yes') {
            return '<div style="text-align:center; width:100%;">' + '@Message.getMessage(CommonLabel.VALUE_MARK_CIRCLE)' + '</div>'
        }
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
            url: '@Url.Content("~/Calendar")/ImportCsvFile',
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
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_ALL_CONFIRM)", "@Url.Content("~/Calendar")/CsvExport");
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        //年月日
        let searchDtFrom = $("#SearchDtFrom").val();
        let searchDtTo = $("#SearchDtTo").val();
        //前後チェック
        if (searchDtFrom != "" && searchDtTo != "") {
            searchDtFrom = parseFloat(searchDtFrom.replace(/-/g, ""));
            searchDtTo = parseFloat(searchDtTo.replace(/-/g, ""));
            if (searchDtFrom > searchDtTo) {
                showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")
                return false;
            }
        }

        //ページ番号のリセット
        ResetPageNum();
        return true;
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;
        $('#form').submit();
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

<div id="calendar-master">
    @Using (Html.BeginForm("Index", "Calendar", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    @* 2021/09/27 nv iwamoto update start *@
                    @*<input class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">*@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @<input class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">
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
                            <div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_YEAR_MONTH_DAY)</div>
                                <div class="grid-condition-col-common">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchDtFrom, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                    @Html.Raw("～")
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchDtTo, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                </div>
                            </div>

                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("NOT_WORK_FLG").DispFlag = "1", "display", "None"));max-width:300px;">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_WORK_OR_NOT_WORK)</div>
                                <div class="grid-condition-col">
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchNotWorkFlgNo)@Message.getMessage(CommonLabel.LABEL_NOT_WORK_FLG_N)</div>
                                    <div class="input-select-item-last">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchNotWorkFlgYes)@Message.getMessage(CommonLabel.LABEL_NOT_WORK_FLG_Y)</div>
                                </div>
                            </div>

                        </div>
                    </div>

                    <div class="view-area-label" style="height: 25px"></div>
                    <input class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />

                </div>
            </div>
        </div>
                    @* 登録日時、登録者表示 *@
                    @<div class="search-wrap">
                        <div class="float-left mr50">@Message.getMessage(CommonLabel.LABEL_INS_DATETIME):@CommonRfid.ToStringDateYMDHM(Model.FirstOrDefault.LAST_INS_DATETIME)</div>
                        <div class="float-left">@Message.getMessage(CommonLabel.LABEL_INS_EMPLOYEE_NAME):@Model.FirstOrDefault.LAST_INS_EMPLOYEE_NAME</div>
                        <div style="clear:both;"></div>
                    </div>
                    @*ページャー表示*@
                    @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
                    @<div class="list-view">
                        <div id="ListGrids" style="height:auto;padding-bottom: 20px;" />
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
                Dim csvSessionModel As CalendarViewModel = Session(CommonSessionKey.CREAGE_CALENDAR)
                If Not IsNothing(csvSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(csvSessionModel)
                Else
                    Model.Add(New CalendarViewModel())
                End If
            END CODE
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

                    @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
                    @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
                    @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
                End Using
</div>
