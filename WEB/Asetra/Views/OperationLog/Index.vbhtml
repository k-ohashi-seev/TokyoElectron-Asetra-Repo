@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@ModelType List(Of OperationLogViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabel.TITLE_LOG)
End Code
<script>
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

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        // 大区分非選択時は中区分を非活性
        //if (document.getElementById("SearchLocL").value == "") {   // 2020/03/17 nv miyaji upd
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocL").value == "") {   // 2020/03/17 nv miyaji upd
            document.getElementById("SearchLocM").value = ""
            $("#SearchLocM").attr("disabled", "disabled");
        }
        // 中区分非選択時は小区分を非活性
        //if (document.getElementById("SearchLocM").value == "") {   // 2020/03/17 nv miyaji upd
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocM").value == "") {   // 2020/03/17 nv miyaji upd
            document.getElementById("SearchLocS").value = "";
            $("#SearchLocS").attr("disabled", "disabled");
        }
        // 期間(From)のkeyDown
        document.getElementById("SearchDateFrom").addEventListener('keydown', function () {
            // 0-9 / バックスペース以外は無効
            if (event.keyCode >= 48 && event.keyCode <= 57 || event.keyCode == 8 || event.keyCode == 191) {
                //
            } else {
                event.preventDefault();
            }
        });
        // 期間(To)のkeyDown
        document.getElementById("SearchDateTo").addEventListener('keydown', function () {
            // 0-9 / バックスペース以外は無効
            if (event.keyCode >= 48 && event.keyCode <= 57 || event.keyCode == 8 || event.keyCode == 191) {
                //
            } else {
                event.preventDefault();
            }
        });

        // 列定義
        let columns = [
            { id: "@ColKey.OperationLog.PROCESS_DATETIME", name: "@Message.getMessage(CommonLabelOperationLog.LABEL_PROCESS_DATE)", field: "PROCESS_DATETIME", width: getColumnWidth("@DispId.OPERATION_LOG", "@ColKey.OperationLog.PROCESS_DATETIME", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("PROCESS_DATETIME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PROCESS_DATETIME").DispFlag", formatter: ProcDateTimeFormatter, },
            { id: "@ColKey.OperationLog.PROCESS_CAT", name: "@Message.getMessage(CommonLabelOperationLog.LABEL_PROCESS_CAT)", field: "PROCESS_CAT", width: getColumnWidth("@DispId.OPERATION_LOG", "@ColKey.OperationLog.PROCESS_CAT", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("PROCESS_CAT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PROCESS_CAT").DispFlag", formatter: ProcCatFormatter },
            { id: "@ColKey.OperationLog.PROCESS_TOOL_DIV", name: "@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL)", field: "PROCESS_TOOL_DIV", width: getColumnWidth("@DispId.OPERATION_LOG", "@ColKey.OperationLog.PROCESS_TOOL_DIV", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("PROCESS_TOOL_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PROCESS_TOOL_DIV").DispFlag", formatter: ProcToolDivFormatter },
            { id: "@ColKey.OperationLog.OPE_EMPLOYEE_CD", name: "@Message.getMessage(CommonLabelOperationLog.LABEL_EMPLOYEE_CODE_NAME)", field: "OPE_EMPLOYEE_CD", width: getColumnWidth("@DispId.OPERATION_LOG", "@ColKey.OperationLog.OPE_EMPLOYEE_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("OPE_EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("OPE_EMPLOYEE_CD").DispFlag", formatter: OpeEmpCdFormatter },
            { id: "@ColKey.OperationLog.PROCESS_CONTENTS", name: "@Message.getMessage(CommonLabelOperationLog.LABEL_PROCESS_CONTENTS)", field: "PROCESS_CONTENTS", width: getColumnWidth("@DispId.OPERATION_LOG", "@ColKey.OperationLog.PROCESS_CONTENTS", 460), sortOrder: "@Model.FirstOrDefault.GridInfo("PROCESS_CONTENTS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PROCESS_CONTENTS").DispFlag" },
            { id: "@ColKey.OperationLog.BOOK_LOCATION_L_CODE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_BOOK_LOCATION_L_CODE_NAME)", field: "LOC_L_CD", width: getColumnWidth("@DispId.OPERATION_LOG", "@ColKey.OperationLog.BOOK_LOCATION_L_CODE_NAME", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_L_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_L_CODE_NAME").DispFlag", formatter: BookLocLCdFormatter },
            { id: "@ColKey.OperationLog.BOOK_LOCATION_M_CODE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_BOOK_LOCATION_M_CODE_NAME)", field: "LOC_M_CD", width: getColumnWidth("@DispId.OPERATION_LOG", "@ColKey.OperationLog.BOOK_LOCATION_M_CODE_NAME", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_M_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_M_CODE_NAME").DispFlag", formatter: BookLocMCdFormatter },
            { id: "@ColKey.OperationLog.BOOK_LOCATION_S_CODE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_BOOK_LOCATION_S_CODE_NAME)", field: "LOC_S_CD", width: getColumnWidth("@DispId.OPERATION_LOG", "@ColKey.OperationLog.BOOK_LOCATION_S_CODE_NAME", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_S_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_S_CODE_NAME").DispFlag", formatter: BookLocSCdFormatter },
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

        let jsonData = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            // 安全に使用できるようにJSON文字列をエンコード
            jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");

        grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.OPERATION_LOG", fixCol);

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

    });

    // 処理日時用formatter
    function ProcDateTimeFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:center; width:100%;">' + toStringYMDHM(dataContext.PROCESS_DATETIME) + '</div>';
    }

    // 処理種別用formatter
    function ProcCatFormatter(row, cell, value, columnDef, dataContext) {
        let procCat = "";

        switch (dataContext.PROCESS_CAT) {
            case "@ProcessCat.Event":
                procCat = "@Message.getMessage(CommonLabel.LABEL_PROCESS_CAT_EV)";
                break;
            case "@ProcessCat.Operation":
                procCat = "@Message.getMessage(CommonLabel.LABEL_PROCESS_CAT_OP)";
                break;
            case "@ProcessCat.Error":
                procCat = "@Message.getMessage(CommonLabel.LABEL_PROCESS_CAT_ER)";
                break;
        }

        return procCat;
    }

    // 処理分類用formatter
    function ProcToolDivFormatter(row, cell, value, columnDef, dataContext) {
        let procToolDiv = "";

        switch (dataContext.PROCESS_TOOL_DIV) {
            case "@ProcessToolDiv.Web":
                procToolDiv = "@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)";
                break;
            case "@ProcessToolDiv.CS":
                procToolDiv = "@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS)";
                break;
            case "@ProcessToolDiv.HT":
                procToolDiv = "@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)";
                break;
        }

        return procToolDiv;
    }

    // 操作社員コード/名用formatter
    function OpeEmpCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.OPE_EMPLOYEE_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.OPE_EMPLOYEE_NAME);
    }

    // 帳簿ロケーション大区分コード/名用formatter
    function BookLocLCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LOC_L_NAME);
    }

    // 帳簿ロケーション中区分コード/名用formatter
    function BookLocMCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_M_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LOC_M_NAME);
    }

    // 帳簿ロケーション小区分コード/名用formatter
    function BookLocSCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_S_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LOC_S_NAME);
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        // 期間(From)未入力チェック
        if (!document.getElementById("SearchDateFrom").value) {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_NOT_ENTERED_DATE_FROM)")
            return false;
        }
        // 期間(To)未入力チェック
        if (!document.getElementById("SearchDateTo").value) {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_NOT_ENTERED_DATE_TO)")
            return false;
        }
        // 期間の日付形式チェック
        // From
        //if (!isDate(document.getElementById("SearchDateFrom").value)) {
        //    alert("期間(From不正)");
        //    return false;
        //}
        // To
        //if (!isDate(document.getElementById("SearchDateTo").value)) {
        //    alert("期間(To不正)");
        //    return false;
        //}
        // From <= To
        if (!isDateComparison()) {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")
            return false;
        }
        // ログ種別の選択状態をチェック　'2020/03/14 nv miyaji
        if (!document.getElementById("SearchProcessCategoryEV").checked
            && !document.getElementById("SearchProcessCategoryER").checked) {
            showErrMessage("@Message.getMessage(CommonLabelOperationLog.MSG_ERR_PROCESS_CAT_UNSELECTED)")
            return false;
        }

        // 処理種別の選択状態をチェック
        //if (!document.getElementById("SearchProcessCategoryOP").checked
        //    && !document.getElementById("SearchProcessCategoryEV").checked
        //    && !document.getElementById("SearchProcessCategoryER").checked) {
        //    showErrMessage("@Message.getMessage(CommonLabelOperationLog.MSG_ERR_PROCESS_CAT_UNSELECTED)")
        //    return false;
        //}

        //画面ブロックを表示
        showLoading();

        //ページ番号のリセット
        ResetPageNum();

        //20230904 ADD Start Shirakawa
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/OperationLog")/SearchClick',
        });
        //20230904 ADD End Shirakawa

        return true;
    }

    // 日付チェック
    function isDate(tmpDate) {
        var arrDate = tmpDate.split("/");
        if (arrDate.length != 3) {
            return false;
        }
        var chkDate = new Date(arrDate[0], arrDate[1] - 1, arrDate[2]);
        if (arrDate[0] !== String(chkDate.getFullYear())
            || ('0' + arrDate[1]).slice(-2) !== ('0' + (chkDate.getMonth() + 1)).slice(-2)
            || ('0' + arrDate[2]).slice(-2) !== ('0' + chkDate.getDate()).slice(-2)) {
            return false;
        }
        return true;
    }
    // 期間の大小比較
    function isDateComparison() {
        var arrDateFrom = document.getElementById("SearchDateFrom").value.split("/");
        var arrDateTo = document.getElementById("SearchDateTo").value.split("/");
        if (arrDateFrom > arrDateTo) {
            return false;
        }
        return true;
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        //画面ブロックを表示
        showLoading();

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //検索条件の権限ラジオボタンクリック処理
    function SearchProcessTool_onclick(value) {
        //クリックされたラジオボタンの値を隠し項目に格納する
        document.getElementById("SearchProcessTool").value = value;
    }

    //大区分選択
    function ChangeLocationL(locL) {
        // 中区分をクリア
        $("#SearchLocM").val("").trigger("change");
        $("#SearchLocM").prop("disabled", true);
        // 小区分をクリア
        $("#SearchLocS").val("").trigger("change");
        $("#SearchLocS").prop("disabled", true);

        // 選択されていない場合はここで終了
        if (!locL) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/OperationLog")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationM", selectLCode: locL, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocM = document.getElementById("SearchLocM");
                    SearchLocM.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    document.getElementById("SearchLocM").appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        document.getElementById("SearchLocM").appendChild(op);
                    }
                }
                // 中区分データ取得
                $("#SearchLocM").removeAttr("disabled");
                document.getElementById("SearchLocM").value = data.locMDropdownData;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //中区分選択
    function ChangeLocationM(locM) {
        var lCode = document.getElementById("SearchLocL").value;
        // 小区分をクリア
        $("#SearchLocS").val("").trigger("change");
        $("#SearchLocS").prop("disabled", true);

        // 選択されていない場合はここで終了
        if (!locM) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/OperationLog")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationS", selectLCode: lCode, selectMCode: locM },
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

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        document.getElementById("SearchLocS").appendChild(op);
                    }
                }
                // 小区分データ取得
                $("#SearchLocS").removeAttr("disabled");
                document.getElementById("SearchLocS").value = data.locSDropdownData;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
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

<div id="operationlog">
    @Using (Html.BeginForm("Index", "OperationLog", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
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
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PERIOD)</div>
                                 <div class="grid-condition-col-common">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchDateFrom, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                    @Html.Raw("～")
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchDateTo, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                </div>
                             </div>

                             @* ロケーション大区分 *@
                             <div class="col-sm-2 filter-combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("BOOK_LOCATION_L_CODE_NAME").DispFlag = "1", "display", "None"))">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L)</div>
                                 <div class="grid-condition-col">
                                     @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocL, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control", .onchange = "ChangeLocationL(this.value)"})
                                 </div>
                             </div>
                             @* ロケーション中区分 *@
                             <div class="col-sm-2 filter-combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("BOOK_LOCATION_L_CODE_NAME").DispFlag = "1" And Model.FirstOrDefault.GridInfo("BOOK_LOCATION_M_CODE_NAME").DispFlag = "1", "display", "None"))">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_M)</div>
                                 <div class="grid-condition-col">
                                     @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocM, DirectCast(ViewBag.modelLocM, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control", .onchange = "ChangeLocationM(this.value)"})
                                 </div>
                             </div>
                             @* ロケーション小区分 *@
                             <div class="col-sm-2 filter-combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("BOOK_LOCATION_L_CODE_NAME").DispFlag = "1" And Model.FirstOrDefault.GridInfo("BOOK_LOCATION_M_CODE_NAME").DispFlag = "1" And Model.FirstOrDefault.GridInfo("BOOK_LOCATION_S_CODE_NAME").DispFlag = "1", "display", "None"))">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_S)</div>
                                 <div class="grid-condition-col">
                                     @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocS, DirectCast(ViewBag.modelLocS, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                 </div>
                             </div>

                             @* 処理分類 *@
                             <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("PROCESS_TOOL_DIV").DispFlag = "1", "display", "None"));max-width:300px;">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL)</div>
                                 <div class="grid-condition-col">
                                     <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchProcessTool, "ALL", New With {.onclick = "SearchProcessTool_onclick(this.value);"})@Message.getMessage(CommonLabel.LABEL_SELECT_ALL)</div>
                                     <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchProcessTool, "W", New With {.onclick = "SearchProcessTool_onclick(this.value);"})@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)</div>
                                     <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchProcessTool, "C", New With {.onclick = "SearchProcessTool_onclick(this.value);"})@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS)</div>
                                     <div class="input-select-item-last">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchProcessTool, "H", New With {.onclick = "SearchProcessTool_onclick(this.value);"})@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)</div>
                                 </div>
                             </div>

                             @* ログイン者名 *@
                             <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("OPE_EMPLOYEE_CD").DispFlag = "1", "display", "None"))">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabelOperationLog.LABEL_LOGIN_EMPLOYEE_NAME)</div>
                                 <div class="grid-condition-col">
                                     @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchEmployeeName, New With {.class = "cond-control", .maxlength = 60})
                                 </div>
                             </div>

                             <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("PROCESS_CAT").DispFlag = "1", "display", "None"));max-width:300px;">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOG_CAT)</div>
                                 <div class="grid-condition-col">
                                     <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessCategoryEV)@Message.getMessage(CommonLabel.LABEL_PROCESS_CAT_EV)</div>
                                     <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchProcessCategoryER)@Message.getMessage(CommonLabel.LABEL_PROCESS_CAT_ER)</div>
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
        @<div class="list-view">
            @* 高さはヘッダー固定の処理（enableFixedHeader）で画面の残り領域に合わせる *@
            <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)

                                             End Using
</div>
