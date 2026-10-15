@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of AssetLeasePlaceDetailViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelAssetLeasePlaceDetail.TITLE)
End Code
<script>
    //
    // 貸出先資産照会
    //
    $(function () {
        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }
        // 貸出先一覧画面からの遷移の場合は戻るボタン表示
        if ("@ViewBag.FromListKey") {
            $("#btnReturn").show();
        }
        // 列定義
        let columns = [
						{ id: "@ColKey.AssetLeasePlaceDetailList.LEASE_PLACE_CD_NAME"
						, name: "@Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD_NAME)"
						, field: "LEASE_PLACE_CD"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.LEASE_PLACE_CD_NAME", 300)
						, formatter: LeasePlaceCdNameFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceDetailList.LEASE_STATUS"
						, name: "@Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)"
						, field: "LEASE_STATUS"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_STATUS").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("LEASE_STATUS").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.LEASE_STATUS", 80)
						, formatter: LeaseStatusFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceDetailList.LEASE_KIKAN"
						, name: "@Message.getMessage(CommonLabel.LABEL_LEASE_KIKAN)"
						, field: "LEASE_KIKAN"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_DT").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("LEASE_DT").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.LEASE_KIKAN", 240)
						, formatter: LeaseKikanFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceDetailList.LEASE_REMARK"
						, name: "@Message.getMessage(CommonLabel.LABEL_LEASE_REMARK)"
						, field: "LEASE_REMARK"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("REMARK").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("REMARK").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.LEASE_REMARK", 100)
						, formatter: LeaseRemarkFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceDetailList.LEASE_EMPLOYEE_CD_NAME"
						, name: "@Message.getMessage(CommonLabel.LABEL_LEASE_EMPLOYEE_CD_NAME)"
						, field: "LEASE_EMPLOYEE_CD_NAME"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_EMPLOYEE_CD").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("LEASE_EMPLOYEE_CD").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.LEASE_EMPLOYEE_CD_NAME", 200)
						, formatter: LeaseEmployeecdnameFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceDetailList.ASSET_NO"
						, name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)"
						, field: "ASSET_NO"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.ASSET_NO", 100)
						, formatter: AssetNoFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceDetailList.ASSET_NAME"
						, name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)"
						, field: "ASSET_NAME"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.ASSET_NAME", 380)
						, formatter: AssetNameFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceDetailList.MAKER"
						, name: "@Message.getMessage(CommonLabel.LABEL_MAKER)"
						, field: "MAKER"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("MAKER").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("MAKER").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.MAKER", 300)
						, formatter: MakerFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceDetailList.PRODUCT_NO"
						, name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)"
						, field: "PRODUCT_NO"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.PRODUCT_NO", 130)
						, formatter: ProductNoFormatter
						},

						{ id: "@ColKey.AssetLeasePlaceDetailList.MODEL_NO"
						, name: "@Message.getMessage(CommonLabel.LABEL_MODEL_NO)"
						, field: "MODEL_NO"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispFlag"
						, width: getColumnWidth("@DispId.ASSET_LEASE_PLACE_DETAIL", "@ColKey.AssetLeasePlaceDetailList.MODEL_NO", 300)
						, formatter: ModelNoFormatter
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
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.ASSET_LEASE_PLACE_DETAIL", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, "@DispId.ASSET_LEASE_PLACE_DETAIL", fixCol);
        }

    });

    //データCSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/AssetLeasePlaceDetail")/CsvExport");

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

    // 貸出先コード/名formatter
    function LeasePlaceCdNameFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LEASE_PLACE_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LEASE_PLACE_NAME) ;
    }
    // 貸出状態formatter
    function LeaseStatusFormatter(row, cell, value, columnDef, dataContext) {
    let s = dataContext.DispLeaseStatus;
    switch (s){
      case Number("@LeaseStatus.Cd.Returned"):
        return"@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)";
        break;
      case Number("@LeaseStatus.Cd.Lending"):
        return"@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)";
        break;
      case Number("@LeaseStatus.Cd.LimitExpired"):
        return"@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LIMIT_EXPIRED)";
        break;
      default:
        return'';
      }
    }
    // 貸出期間formatter
    function LeaseKikanFormatter(row, cell, value, columnDef, dataContext) {
        let sLeaseDt = "　　　　　"; // 空白を埋めるためのスペース
        if (dataContext.DispLeaseKikanFrom) {
            sLeaseDt = toStringDateYMD(String(dataContext.DispLeaseKikanFrom));
        }

        let sLeaseDueDt = "";
        if (dataContext.DispLeaseKikanTo) {
            sLeaseDueDt = toStringDateYMD(String(dataContext.DispLeaseKikanTo));
        }

        let sLeaseComment = sLeaseDt;
        if (!(sLeaseDt.trim() === "" && sLeaseDueDt === "")) {
            sLeaseComment = sLeaseDt + "～" + sLeaseDueDt;
        }

        return sLeaseComment;
    }
    // 貸出先備考formatter
    function LeaseRemarkFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.DispLeaseBiko) ;
    }
    // 貸出担当者コード/名formatter
    function LeaseEmployeecdnameFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.DispLeaseEmployeeCd) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.DispLeaseEmployeeNm) ;
    }
    // 資産番号formatter
    function AssetNoFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.ASSET_NO) ;
    }
    // 資産名称formatter
    function AssetNameFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.ASSET_NAME) ;
    }
    // 資産製造元formatter
    function MakerFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.MAKER) ;
    }
    // 品番formatter
    function ProductNoFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.PRODUCT_NO) ;
    }
    // 型番formatter
    function ModelNoFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.MODEL_NO) ;
    }

</script>

<div id="leaseplace-detail">
    @Using (Html.BeginForm("Index", "AssetLeasePlaceDetail", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    <input class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
                    <input class="defButton" type="button"  id="btnReturn" name="Return" style="display:none;" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "AssetLeasePlaceList")'">
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
                            <div class="col-sm-1" style="display:@(If(Model.FirstOrDefault.GridInfo("LEASE_DT").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_LEASE_LIMIT)</div>
                                <td class="condition-col" style="width:80px">
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseLimitExpired) @Message.getMessage(CommonLabel.LABEL_EXPIRED)</div>
                                </td>
                            </div>
                            @* 過去算出 *@
                            <div class="col-sm-1">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_PAST)</div>
                                <td class="condition-col" style="width:80px">
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeasePast) @Message.getMessage(CommonLabel.LABEL_FUKUMU)</div>
                                </td>
                            </div>
                            @* 貸出備考 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("REMARK").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_BIKO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchLeaseBiko, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 貸出担当者 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("LEASE_EMPLOYEE_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_EMPLOYEE)</div>
                                <div Class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLeaseEmployeeCd, DirectCast(ViewBag.LeasePlaceList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                </div>
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

                                         End Using
</div>
