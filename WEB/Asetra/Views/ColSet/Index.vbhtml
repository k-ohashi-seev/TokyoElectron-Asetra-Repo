@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of ColSetDetailViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelColSet.TITLE)
End Code

<script>
    let grid;
    let dataView;

    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            // エラーメッセージ表示後、ライセンスコントローラーを呼び出す
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
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

        // 更新有無フラグ取得
        let isUpd = "@ViewBag.AuthProcFlg" === "@AuthProcFlg.All";
        // 他拠点更新有無フラグ取得
        let isUpdAll = "@ViewBag.AuthProcFlgAll" === "@AuthProcFlg.All";

        if (isUpd) {
            // 活性
            isUpd = true;
            $("#btnUpdate").attr("disabled", false)
            $("#txtFilColCnt").attr("disabled", false)
        } else {
            // 非活性
            isUpd = false;
            $("#btnUpdate").attr("disabled", true)
            $("#btnUpdate").attr('style', 'background-color:#cecece;');
            $("#txtFilColCnt").attr("readonly", "readonly");
            $("#txtFilColCnt").attr('style', 'width: 200px;text-align: right;background-color:#cecece;');
        }

        if (isUpdAll) {
            // 活性
            isUpdAll = true;
            if ("@ViewBag.SearchLocCd" != "ALL") {
                $("#btnUpdateAll").attr("disabled", false)
            } else {
                // 非活性
                $("#btnUpdateAll").attr("disabled", true)
                $("#btnUpdateAll").attr('style', 'background-color:#cecece;');
            }
        } else {
            // 非活性
            $("#btnUpdateAll").attr("disabled", true)
            $("#btnUpdateAll").attr('style', 'background-color:#cecece;');
        }

        document.getElementById("txtFilColCnt").value = "@Model.FirstOrDefault.FixColCnt";

        // 固定列フラグがNの場合は固定列数を編集できないようにする
        if ("@Model.FirstOrDefault.FIX_COL_FLG" == "N") {
            $("#txtFilColCnt").attr("readonly", "readonly");
            $("#txtFilColCnt").attr('style', 'width: 200px;text-align: right;color:#cecece;background-color:#cecece;');
        }

        // 列定義
        let columns = [
            { id: "@ColKey.ColSet.DB_COLUMN_NAME", name: "", behavior: 'selectAndMove', field: "DB_COLUMN_NAME", width: 0, visible: false },
            { id: "@ColKey.ColSet.DISP_ORDER", name: "@Message.getMessage(CommonLabelColSet.LABEL_DISP_ORDER)", behavior: 'selectAndMove', field: "DISP_ORDER", width: getColumnWidth("@DispDiv.COL_SET", "@ColKey.ColSet.DISP_ORDER", 100), visible: false },
            { id: "@ColKey.ColSet.DISP_FLAG", name: "@Message.getMessage(CommonLabelColSet.LABEL_DISP)", behavior: 'selectAndMove', field: "DISP_FLAG", width: getColumnWidth("@DispDiv.COL_SET", "@ColKey.ColSet.DISP_FLAG", 40), formatter: CheckboxFormatter, visible: true, cssClass: "first-column", enable: isUpd },
            { id: "@ColKey.ColSet.COL_NAME", name: "@Message.getMessage(CommonLabelColSet.LABEL_COL_NAME)", behavior: 'selectAndMove', field: "COL_NAME", width: getColumnWidth("@DispDiv.COL_SET", "@ColKey.ColSet.COL_NAME", 300), visible: true },
            { id: "@ColKey.ColSet.FIX_COL_CNT", name: "", behavior: 'selectAndMove', field: "FIX_COL_CNT", width: 0, visible: false },
        ];

        // Gridのオプション機能の設定
        let options = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: false,                      //Gridの高さの自動調整可否（ヘッダー固定のためfalse）
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
            multiSelect: false                      //行の複数選択可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // SQL順を保証するためにフロント側でソート
        jsonData.sort((a, b) => a.DISP_ORDER - b.DISP_ORDER);

        // DataView にセットするためのユニークID列を追加
        jsonData.forEach((item, index) => item.__id = index);

        // Gridのデータ管理オブジェクトにセット
        dataView = new Slick.Data.DataView();

        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "__id");
            dataView.endUpdate();
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispDiv.COL_SET", 0);

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

        // 行選択を可能にする
        if (isUpd) {
            grid.setSelectionModel(new Slick.RowSelectionModel());
        }

        // ドラッグ&ドロップを可能にする
        let rowMoveManager = new Slick.RowMoveManager({
            cancelEditOnDrag: true
        });
        grid.registerPlugin(rowMoveManager);

        // Grid セルClickイベント
        grid.onClick.subscribe(function (e, args) {
            let cell = args.cell;
            let row = args.row;
            let column = grid.getColumns()[cell];
            let item = grid.getDataItem(row);

            // チェックボックスが配置されている列かどうかを確認
            if (column.id === "@ColKey.ColSet.DISP_FLAG") {
                // 更新権限なしの場合は変更しない
                if (!isUpd) { return; }

                // クリックされた要素がチェックボックスであるか確認
                var target = e.target;
                if (target && target.type === 'checkbox') {
                    // チェックボックスがクリックされた場合、チェック状態を変更
                    item[column.field] = target.checked ? 1 : 0;
                } else {
                    // チェックボックス以外がクリックされた場合、行選択のみ
                    e.preventDefault();
                }

                // グリッドのデータを更新
                grid.updateRow(row);
            }
        });

        // 行ドラッグ&ドロップイベント
        rowMoveManager.onMoveRows.subscribe(function (e, args) {
            // 以前の選択されていた行のインデックスを保持
            var selectedRowIndeces = grid.getSelectedRows();

            let extractedRows = [];                                 // 移動した行を格納するための配列
            let moveRows = args.rows;                               // 移動する行のインデックスの配列
            let destIndex = args.insertBefore;                      // 移動先のインデックス
            let gridData = dataView.getItems();                     // Gridに表示中のデータを取得

            // Grid表示中のデータを、移動行が挿入される前後で分ける
            let gridLeft = gridData.slice(0, destIndex);
            let girdRight = gridData.slice(destIndex, gridData.length);

            // 移動する行を昇順にソート ※複数選択なしの場合は不要
            //rows.sort(function (a, b) { return a - b; });

            // 移動する行を抽出して配列に追加
            for (let i = 0; i < moveRows.length; i++) {
                extractedRows.push(gridData[moveRows[i]]);
            }

            // 移動する行を除去
            moveRows.reverse();
            for (let i = 0; i < moveRows.length; i++) {
                let row = moveRows[i];
                if (row < destIndex) {
                    gridLeft.splice(row, 1);
                } else {
                    girdRight.splice(row - destIndex, 1);
                }
            }

            // Gridに表示するデータを移動後の状態になるように連結
            dataView.beginUpdate();
            dataView.setItems(gridLeft.concat(extractedRows.concat(girdRight)), "__id");
            dataView.endUpdate();

            // 選択行の背景色を保持するために選択状態を更新
            var movedRows = [];
            for (let i = 0; i < selectedRowIndeces.length; i++) {
                var originalRowIndex = selectedRowIndeces[i];
                var newRow = gridLeft.length + extractedRows.findIndex(item => item.id === gridData[originalRowIndex].id);
                movedRows.push(newRow);
            }

            grid.setSelectedRows(movedRows);

            // Gridを更新
            grid.invalidate();
            grid.render();
        });
    });

    // 更新ボタンクリック処理
    function btnUpdateConfirm_onclick() {
        //確認メッセージ
        showConfirmOkEvt("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)").then(function () {
            // OKクリック後
            let gridData = dataView.getItems();

            //画面ブロックを表示
            showLoading();

            // 固定列数と画面名を取得
            let filColCnt = document.getElementById("txtFilColCnt").value;
            let dispDiv = document.getElementById("DropDownDispDiv").value;
            let homeBaseLoc = document.getElementById("DropDownHomeBase").value;
            let btnUpdateAllDisabled = document.getElementById("btnUpdateAll").disabled;

            // 画面表示順で列の並び順を設定
            for (let i = 0; i < gridData.length; i++) {
                gridData[i].DISP_ORDER = (i + 1)
            }

            // 送信用データ
            let dataToSend = {
                viewModels: gridData,
                fixColCnt: filColCnt,
                dispDiv: dispDiv,
                homeBaseLoc: homeBaseLoc
            };

            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/ColSet")/Update',
                dataType: 'json',
                data: JSON.stringify(dataToSend),
                contentType: "application/Json",
                cache: false,
                success: function (data, dataType) {
                    $('#form').submit();
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $('#form').submit();
                }
            });
        });
    }

    // 全拠点一括更新ボタンクリック処理
    function btnUpdateAllLocConfirm_onclick() {
        //確認メッセージ
        showConfirmOkEvt("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM_ALL)").then(function () {
            // OKクリック後
            let gridData = dataView.getItems();

            //画面ブロックを表示
            showLoading();

            // 固定列数と画面名を取得
            let filColCnt = document.getElementById("txtFilColCnt").value;
            let dispDiv = document.getElementById("DropDownDispDiv").value;
            let homeBaseLoc = document.getElementById("DropDownHomeBase").value;

            // 画面表示順で列の並び順を設定
            for (let i = 0; i < gridData.length; i++) {
                gridData[i].DISP_ORDER = (i + 1)
            }

            // 送信用データ
            let dataToSend = {
                viewModels: gridData,
                fixColCnt: filColCnt,
                dispDiv: dispDiv,
                homeBaseLoc: homeBaseLoc
            };

            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/ColSet")/UpdateAllLoc',
                dataType: 'json',
                data: JSON.stringify(dataToSend),
                contentType: "application/Json",
                cache: false,
                success: function (data, dataType) {
                    $('#form').submit();
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $('#form').submit();
                }
            });
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

    // 検索ボタンクリック処理
    function btnSearch_onclick() {

        //画面ブロックを表示
        showLoading();

        let dispDiv = document.getElementById("DropDownDispDiv").value;
        let homeBaseLoc = document.getElementById("DropDownHomeBase").value;
        let isDisabled = document.getElementById("btnUpdateAll").disabled;

        let dataToSend = {
            dispDiv: dispDiv,
            homeBaseLoc: homeBaseLoc,
            isDisabled: isDisabled
        };

        $.ajax({
            url: '@Url.Content("~/ColSet")/Search',
            type: 'POST',
            dataType: 'json',
            data: JSON.stringify(dataToSend),
            contentType: "application/Json",
            success: function (response) {
                $('#form').submit();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("通信エラー:", textStatus);
                $('#form').submit();
            }
        });
    }

    // 画面名選択肢設定変更時処理
    async function DropDownDispDiv_onchange() {
        const selected = $("#DropDownDispDiv").val();
        const $ddlHome = $("#DropDownHomeBase");
        $ddlHome.empty();

        try {
            const data = await $.ajax({
                url: '@Url.Content("~/ColSet")/GetHomeBaseList',
                type: 'POST',
                data: { searchDispDiv: selected },
                dataType: 'json'
            });

            $ddlHome.empty();
            data.forEach(item => {
                $ddlHome.append($('<option>', {
                    value: item.Value,
                    text: item.Text
                }));
            });

        } catch (error) {
            console.error("通信エラー:", error);
        }
    }

</script>

<div id="colset-master">
    @Using (Html.BeginForm("Index", "ColSet", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))

        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    <input id="btnUpdate" class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_UPDATE)" onclick="return btnUpdateConfirm_onclick()">
                </div>
                <div class="view-area-button-left">
                    <input id="btnUpdateAll" class="defButton" type="button" value="@Message.getMessage(CommonLabelColSet.BUTTON_ALL_BASE_UPDATA)" onclick="return btnUpdateAllLocConfirm_onclick()" >
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
                                 <div class="grid-head-col">@Message.getMessage(CommonLabelColSet.LABEL_DISP_NAME)</div>
                                 <div class="grid-condition-col">
                                     @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchDispDiv, DirectCast(ViewBag.DispNameList, IEnumerable(Of SelectListItem)), New With {.id = "DropDownDispDiv", .class = "cond-control", .onchange = "DropDownDispDiv_onchange()"})
                                 </div>
                             </div>
                             <div class="col-sm-2">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabelColSet.LABEL_HOME_BASE)</div>
                                 <div class="grid-condition-col">
                                     @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocCd, DirectCast(ViewBag.HomeBaseList, IEnumerable(Of SelectListItem)), New With {.id = "DropDownHomeBase", .class = "cond-control"})
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
                             <td width="70px" Class="head-col">@Message.getMessage(CommonLabelColSet.LABEL_FIX_COL_CNT)</td>
                             <td width="20"></td>
                             <td>@Html.TextBoxFor(Function(model) model.FirstOrDefault.FixColCnt, New With {.oninput = "editNumber(this)", .id = "txtFilColCnt", .maxlength = 3, .style = "width: 200px;text-align: right;"})</td>
                             <td>@Html.Label(Message.getMessage(CommonLabel.MSG_ERR_NOT_FIX_COL), New With {.style = "margin-left:10px;padding-top:3px"})</td>
                         </tr>
                     </Table>
                     @Html.Label(Message.getMessage(CommonLabel.MSG_ERR_ORDER_CHANGE_DRAG_AND_DROP), New With {.style = "margin-top:10px;"})


                     @<div class="list-view">
                         @* 高さはヘッダー固定の処理（enableFixedHeader）で画面の残り領域に合わせる *@
                         <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
                     </div>
                     End Using




                 </div>
