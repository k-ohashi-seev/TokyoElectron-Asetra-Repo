@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of CsvMapDetailViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelCsvMap.TITLE)
End Code

<script>
    let grid;

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

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessEdit") {
            //登録
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")

        }

        // 設定ファイルに文字コードの定義がない場合、エラー
        if ("@ViewBag.ErrorMessage" != "") {
            showErrMessage("@ViewBag.ErrorMessage")
        }

        // CSV項目名を取得
        let csvKoumokuName = "";
        if ("@Model.FirstOrDefault.CSV_DIV" === "@CsvDiv.Asset") {
            csvKoumokuName = "@Message.getMessage(CommonLabel.LABEL_CSV_KOUMOKU_ASSET)";
        } else {
            csvKoumokuName = "@Message.getMessage(CommonLabel.LABEL_CSV_KOUMOKU_EMPLOYEE)";
        }

        // 列定義
        let columns = [
            { id: "@ColKey.CsvMapping.NO", name: "@Message.getMessage(CommonLabel.LABEL_NO)", field: "CSV_NO", width: getColumnWidth("@DispId.CSV_MAPPING", "@ColKey.CsvMapping.NO", 40), sortOrder: "@Model.FirstOrDefault.GridInfo("CSV_NO").DispOrder", formatter: RightAlignFormatter, cssClass: "first-column" },
            { id: "@ColKey.CsvMapping.CSV_KOUMOKU", name: csvKoumokuName, field: "CSV_KOUMOKU", width: getColumnWidth("@DispId.CSV_MAPPING", "@ColKey.CsvMapping.CSV_KOUMOKU", 250), sortOrder: "@Model.FirstOrDefault.GridInfo("CSV_ITEM").DispOrder" },
            { id: "@ColKey.CsvMapping.REQUIRED", name: "@Message.getMessage(CommonLabel.LABEL_REQUIRED)", field: "REQUIRED_ITEM_FLAG", width: getColumnWidth("@DispId.CSV_MAPPING", "@ColKey.CsvMapping.REQUIRED", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("REQUIRED_ITEM_FLAG").DispOrder", formatter: RequiredFormatter },
            { id: "@ColKey.CsvMapping.SPECIFIED", name: "@Message.getMessage(CommonLabel.LABEL_SPECIFIED)", field: "SPECIFIED_DIV", width: getColumnWidth("@DispId.CSV_MAPPING", "@ColKey.CsvMapping.SPECIFIED", 350), sortOrder: "@Model.FirstOrDefault.GridInfo("SPECIFIED_DIV").DispOrder", formatter: SpecifiedFormatter },
            { id: "@ColKey.CsvMapping.COL_NO", name: "@Message.getMessage(CommonLabel.LABEL_COL_NO)", field: "CSV_COL_NO", width: getColumnWidth("@DispId.CSV_MAPPING", "@ColKey.CsvMapping.COL_NO", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("CSV_COL_NO").DispOrder", editor: Slick.Editors.CustomTextEditor, formatter: ColNoFormatter },
            { id: "@ColKey.CsvMapping.FIX_VAL", name: "@Message.getMessage(CommonLabel.LABEL_FIX_VAL)", field: "CSV_FIX_VAL", width: getColumnWidth("@DispId.CSV_MAPPING", "@ColKey.CsvMapping.FIX_VAL", 230), sortOrder: "@Model.FirstOrDefault.GridInfo("CSV_FIX_VAL").DispOrder", editor: Slick.Editors.CustomTextEditor, formatter: FixValFormatter },
        ];

        // Gridのオプション機能の設定
        let options = {
            enableCellNavigation: false,            //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: true,                       //Gridの高さの自動調整可否
            editable: true,                         //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
            multiSelect: false                      //行の複数選択可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // Grid表示判定
        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "CSV_NO");
            dataView.endUpdate();
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.CSV_MAPPING", 0);

        // エラー項目があればフォーカス
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            let rowIndex;
            let colIndex;

            if ("@Model.FirstOrDefault.ErrTarget".includes("CSV_COL_NO")) {
                // 列番号
                rowIndex = Number("@Model.FirstOrDefault.ErrTarget".replace("CSV_COL_NO", ""));
                colIndex = Number("@Model.FirstOrDefault.GridInfo("CSV_COL_NO").DispOrder");
            } else if ("@Model.FirstOrDefault.ErrTarget".includes("CSV_FIX_VAL")) {
                // 固定値
                rowIndex = Number("@Model.FirstOrDefault.ErrTarget".replace("CSV_FIX_VAL", ""));
                colIndex = Number("@Model.FirstOrDefault.GridInfo("CSV_FIX_VAL").DispOrder");
            }

            if (rowIndex === null || rowIndex === undefined) {
                return;
            }
            if (colIndex === null || colIndex === undefined) {
                return;
            }

            let cellNode = grid.getCellNode(rowIndex - 1, colIndex - 1);

            //エラー項目にセット
            $(cellNode).find('input').addClass("slick-input-error");
            $(cellNode).find('input').focus();
        }
    });

    // 必須用Formatter
    function RequiredFormatter(row, cell, value, columnDef, dataContext) {
        let symbol = "";
        if (dataContext.REQUIRED_ITEM_FLAG === "@CsvMapping_ReqItemFlag.Yes") {
            symbol = "○";
        }

        return '<div style="display: flex; justify-content: center; align-items: center; height: 100%; text-align:center; font-size:32px;">' + symbol + '</div>';
    }

    // 指定方法用Formatter
    function SpecifiedFormatter(row, cell, value, columnDef, dataContext) {
        let csvNo = dataContext.CSV_NO;
        let specifiedDiv = dataContext.SPECIFIED_DIV;
        let specCanDiv = dataContext.SPEC_CAN_DIV;
        let isRequired = dataContext.REQUIRED_ITEM_FLAG === '@CsvMapping_ReqItemFlag.Yes'
        let html = '';

        // 列番指定
        if (specCanDiv.includes('C')) {
            let checked = specifiedDiv === '@CsvMapping_SpecifiedDiv.Index' ? ' checked' : '';
            html += '<input type="radio" id="SPECIFIED_DIV' + csvNo + '" name="SPECIFIED_DIV' + csvNo + '" value="' + '@CsvMapping_SpecifiedDiv.Index' + '"' + checked + ' onclick="OnRdoClick(this.parentNode.parentNode);">';
            html += '@Message.getMessage(CommonLabel.LABEL_COL_SELECTED)';
        }

        // 固定値
        if (specCanDiv.includes('F')) {
            let checked = specifiedDiv === '@CsvMapping_SpecifiedDiv.Kotei' ? ' checked' : '';
            html += '<input type="radio" id="SPECIFIED_DIV' + csvNo + '" name="SPECIFIED_DIV' + csvNo + '" value="' + '@CsvMapping_SpecifiedDiv.Kotei' + '"' + checked + ' onclick="OnRdoClick(this.parentNode.parentNode);">';
            html += '@Message.getMessage(CommonLabel.LABEL_FIX_VAL)';
        }

        // 未指定
        if (!isRequired) {
            let checked = specifiedDiv === '@CsvMapping_SpecifiedDiv.MiShitei' ? ' checked' : '';
            html += '<input type="radio" id="SPECIFIED_DIV' + csvNo + '" name="SPECIFIED_DIV' + csvNo + '" value="' + '@CsvMapping_SpecifiedDiv.MiShitei' + '"' + checked + ' onclick="OnRdoClick(this.parentNode.parentNode);">';
            html += '@Message.getMessage(CommonLabel.LABEL_NO_SELECTED)';
        }

        return html;
    }

    // 列番号用Formatter
    function ColNoFormatter(row, cell, value, columnDef, dataContext) {
        let csvNo = dataContext.CSV_NO;
        let csvColNo = dataContext.CSV_COL_NO;
        let isRetsuEnable = (dataContext.SPECIFIED_DIV === '@CsvMapping_SpecifiedDiv.Index');
        let html = '';

        if (isRetsuEnable) {
            // テキストボックスを有効にする
            let inputValue = csvColNo > 0 ? csvColNo : "";
            html += '<input type="text" id="CSV_COL_NO' + csvNo + '" name="CSV_COL_NO' + csvNo + '" class="editable-cell" value="' + inputValue +
                    '" maxlength="3" style="width:100%;text-align:right;padding-right:2px;" oninput="editNumber(this)" />';
        } else {
            // テキストボックスを無効にする
            let inputValue = csvColNo > 0 ? csvColNo : "";
            html += '<input type="text" id="CSV_COL_NO' + csvNo + '" name="CSV_COL_NO' + csvNo + '" class="editable-cell" value="' + inputValue +
                    '" maxlength="3" disabled="disabled" style="width:100%;text-align:right;padding-right:2px;" />';
        }

        return html;
    }

    // 固定値用Formatter
    function FixValFormatter(row, cell, value, columnDef, dataContext) {
        var csvNo = dataContext.CSV_NO;
        var csvFixVal = convertToEmptyIfNull(dataContext.CSV_FIX_VAL);
        var isKoteiEnable = (dataContext.SPECIFIED_DIV === '@CsvMapping_SpecifiedDiv.Kotei');
        var html = '';

        if (isKoteiEnable) {
            // テキストボックスを有効にする
            html += '<input type="text" id="CSV_FIX_VAL' + csvNo + '" name="CSV_FIX_VAL' + csvNo + '" class="editable-cell" value="' + csvFixVal +
                    '" maxlength="255" style="width:100%;" />';
        } else {
            // テキストボックスを無効にする
            html += '<input type="text" id="CSV_FIX_VAL' + csvNo + '" name="CSV_FIX_VAL' + csvNo + '" class="editable-cell" value="' + csvFixVal +
                    '" maxlength="255" disabled="disabled" style="width:100%;" />';
        }

        return html;
    }

    // 更新ボタンクリック処理
    function btnUpdateConfirm_onclick() {

        // 設定ファイルに文字コードの定義がない場合、エラー
        if ("@ViewBag.ErrorMessage" != "") {
            showErrMessage("@ViewBag.ErrorMessage")
            return false;
        }

        let data = grid.getData().getItems();
        let form = $('#form');

        // 既存の隠し要素を削除
        form.find('input[name^="LIST_CSV_NO"], input[name^="LIST_CSV_DIV"], input[name^="LIST_KOUMOKU"]').remove();

        // 新しい隠し要素を追加
        data.forEach(function (row, index) {
            form.append('<input type="hidden" name="LIST_CSV_NO" value="' + row.CSV_NO + '">');
            form.append('<input type="hidden" name="LIST_CSV_DIV" value="' + row.CSV_DIV + '">');
            form.append('<input type="hidden" name="LIST_KOUMOKU" value="' + row.CSV_KOUMOKU + '">');
        });

        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/CsvMapping")/Buttom");
    }

    // グリッドのラジオボタンクリック処理
    function OnRdoClick(element) {
        if (element.children["@CsvMapping_Col.ColIndex"].children["@CsvMapping_Radio.Retsu"].checked) {
            document.getElementById("CSV_COL_NO" + element.children["@CsvMapping_Col.No"].innerText).disabled = false;
            document.getElementById("CSV_FIX_VAL" + element.children["@CsvMapping_Col.No"].innerText).disabled = true;
        } else if (element.children["@CsvMapping_Col.ColIndex"].children["@CsvMapping_Radio.Kotei"].checked) {
            document.getElementById("CSV_COL_NO" + element.children["@CsvMapping_Col.No"].innerText).disabled = true;
            document.getElementById("CSV_FIX_VAL" + element.children["@CsvMapping_Col.No"].innerText).disabled = false;
        } else if (element.children["@CsvMapping_Col.ColIndex"].children["@CsvMapping_Radio.Mishitei"].checked) {
            document.getElementById("CSV_COL_NO" + element.children["@CsvMapping_Col.No"].innerText).disabled = true;
            document.getElementById("CSV_FIX_VAL" + element.children["@CsvMapping_Col.No"].innerText).disabled = true;
        }
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        document.getElementById("CHAR_CD").disabled = true;
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

<div id="csvmapping-master">
    @Using (Html.BeginForm("Index", "CsvMapping", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))

        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    <input class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_UPDATE)" onclick="return btnUpdateConfirm_onclick()">
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
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_SHURUI)</div>
                                 <div class="grid-condition-col">
                                     @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCsvDiv, DirectCast(ViewBag.CsvDivMapList, IEnumerable(Of SelectListItem)), New With {.class = "cond-control"})
                                 </div>
                             </div>
                        </div>
                    </div>

                     <div class="view-area-label" style="height: 25px"></div>
                     <input class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>


                         </div>
                     </div>

                     @<Table Class="detail-table" style="margin-bottom:5px;">
                         <tr Class="detail-row">
                             <td width="70px" Class="head-col">@Message.getMessage(CommonLabel.LABEL_CHAR_CD)</td>
                             <td width="20"></td>
                             <td>@Html.DropDownListFor(Function(model) model.FirstOrDefault.CHAR_CD, DirectCast(ViewBag.CsvMapList, IEnumerable(Of SelectListItem)), New With {.style = "width: 200px;"})</td>
                         </tr>
                     </Table>

                     @<div class="list-view">
                         <div id="ListGrids" style="height:auto;padding-bottom: 20px;" />
                     </div>
                     End Using




                 </div>
