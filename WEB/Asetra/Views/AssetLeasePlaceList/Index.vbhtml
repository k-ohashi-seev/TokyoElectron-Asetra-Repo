@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of AssetLeasePlaceListViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelAssetLeasePlaceList.TITLE)
End Code
<script>
    $(function () {
        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        // 列定義
        let columns = [
						{ id: "@ColKey.AssetLeasePlaceList.LEASE_PLACE_CD"
						, name: "@Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD_NAME)"
						, field: "LEASE_PLACE_CD"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_LIST", "@ColKey.AssetLeasePlaceList.LEASE_PLACE_CD", 300)
						, formatter: LeasePlaceCdFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceList.LEASE_COUNT"
						, name: "@Message.getMessage(CommonLabelAssetLeasePlaceList.LABEL_LEASE_COUNT)"
						, field: "LEASE_COUNT"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_COUNT").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("LEASE_COUNT").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_LIST", "@ColKey.AssetLeasePlaceList.LEASE_COUNT", 100)
						, formatter: LeaseCountFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceList.LEASE_LIMIT_EXPIRE_COUNT"
						, name: "@Message.getMessage(CommonLabelAssetLeasePlaceList.LABEL_LEASE_LIMIT_EXPIRED)"
						, field: "LEASE_LIMIT_EXPIRE_COUNT"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_LIMIT_EXPIRE_COUNT").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("LEASE_LIMIT_EXPIRE_COUNT").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_LIST", "@ColKey.AssetLeasePlaceList.LEASE_LIMIT_EXPIRE_COUNT", 100)
						, formatter: LeaseLimitExpireCountFormatter
						},
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

        // Grid表示判定
        let data = [];
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.ASSET_LEASE_PLACE_LIST", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, "@DispId.ASSET_LEASE_PLACE_LIST", fixCol);
        }

    });

    //データCSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/AssetLeasePlaceList")/CsvExport");

    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        //ページ番号のリセット
        ResetPageNum();
        return true;
    }

  
    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //選択した貸出先コードをセッションに保存します
    function SetLeasePlaceCd(leasePlaceCd, isLimitExpired) {
        // 貸出先資産照会画面に引き渡す資産番号を設定
        document.getElementById("SelectedLeasePlaceCd").value = leasePlaceCd + "," + isLimitExpired;

        $('#form').attr("action", "@Url.Content("~/AssetLeasePlaceList")/SetLeasePlaceCd");
        $('#form').submit();

    }

    // 貸出先コード用formatter
    function LeasePlaceCdFormatter( row, cell, value, columnDef, dataContext )  {
        return '<a href="#" onclick="SetLeasePlaceCd(\'' + dataContext.LEASE_PLACE_CD + '\',\'False\')" class="blue-link">' + dataContext.LEASE_PLACE_CD + " " + dataContext.LEASE_PLACE_NAME + '</a>';
    }

    // 貸出数用formatter
    function LeaseCountFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.DispLeaseCount) ;
    }

    // 期限切れ用
    function LeaseLimitExpireCountFormatter( row, cell, value, columnDef, dataContext )  {
        return '<a href="#" onclick="SetLeasePlaceCd(\'' + dataContext.LEASE_PLACE_CD + '\',\'True\')" class="blue-link">' + dataContext.DispLeaseLimitExpiredCount + '</a>';
    }

</script>

<div id="leaseplace-ichiran">
    @Using (Html.BeginForm("Index", "AssetLeasePlaceList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    <input class="defButton csv-Export-btn  submit-button" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
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
                            @* 貸出先 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_PLACE)</div>
                                <div Class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCode, DirectCast(ViewBag.LeasePlaceList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
                            </div>
                            @* 貸出期限 *@
                            <div class="col-sm-1" style="display:@(If(Model.FirstOrDefault.GridInfo("LEASE_LIMIT_EXPIRE_COUNT").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_LEASE_LIMIT)</div>
                                <td class="condition-col" style="width:80px">
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseLimitExpired) @Message.getMessage(CommonLabel.LABEL_EXPIRED)</div>
                                </td>
                            </div>
                            @* 現在算出 *@
                            <div class="col-sm-1" style="display:@(If(Model.FirstOrDefault.GridInfo("LEASE_COUNT").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_NOW_LENDING)</div>
                                <td class="condition-col" style="width:80px">
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseNow) @Message.getMessage(CommonLabel.LABEL_ARI)</div>
                                </td>
                            </div>
                        </div>
                    </div>
                    <div Class="view-area-label" style="height: 25px"></div>
                    <input Class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>
        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div Class="list-view">
            <div id="ListGrids" style="width:100%;height:auto;" />
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)

        '貸出先資産照会画面遷移時、貸出先コード引き渡し用各四項目
        @<input id="SelectedLeasePlaceCd" name="SelectedLeasePlaceCd" type="hidden">
    End Using
</div>
