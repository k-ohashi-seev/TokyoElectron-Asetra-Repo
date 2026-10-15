@Imports Asetra.Models
@Imports Asetra.Misc
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonLabel.KEY_PAGER_LABEL_ALL) = Message.getMessage(CommonLabel.KEY_PAGER_LABEL_ALL)
End Code

<!DOCTYPE html>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8" />
    <title>@ViewBag.TITLE | @Message.getMessage(CommonLabel.TITLE_SYSTEM)</title>
    @Styles.Render("~/Content/css")
    @Scripts.Render("~/bundles/modernizr")
    @Scripts.Render("~/bundles/jquery")
    @Scripts.Render("~/bundles/common")
    @Scripts.Render("~/bundles/myslickgrid")
    @Scripts.Render("~/bundles/select2")

    <script>
        //ダイアログのタイトルを取得(common.jsで表示する)
        var strConfirmTitle = "@Message.getMessage(CommonLabel.LABEL_CONFIRM)";
        var strErrMessageTitle = "@Message.getMessage(CommonLabel.LABEL_ERROR)";
        //Select2の×ボタンのツールチップ(select2-common.jsで表示する。複数選択＝全解除、単一選択＝クリア)
        var strSelect2AllClearTooltip = "@Message.getMessage(CommonLabel.LABEL_TOOLTIP_ALL_CLEAR)";
        var strSelect2ClearTooltip = "@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)";

    </script>
</head>
<body>
    <header>
        <span class="logo" style="margin-bottom:50px;"><img src="~/Images/ASETRA.jpg" alt="ASTERA" width="200" /></span>
        <ul id="navi" class="nav navbar-nav">
            @* 資産メニュー *@
            @If Message.IsDspMenu(Misc.Auth.MenuId.Asset, ViewBag.LoginInfo) Then
                @<li Class="dropdown">
                    <a href="#" Class="dropdown-toggle" data-toggle="dropdown" role="button">@Message.getMessage(CommonLabel.TITLE_ASSET)</a>
                    <ul Class="dropdown-menu" role="menu">
                        @* 資産ラベル発行 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.AssetLabelPrint, ViewBag.LoginInfo) Then
                            @<li><a href="~/AssetLabelPrint/Init">@Message.getMessage(CommonLabelAssetLabelPrint.TITLE)</a></li>
                        End If
                        @* 資産一覧 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.AssetList, ViewBag.LoginInfo) Then
                            @<li><a href="~/AssetList/Init">@Message.getMessage(CommonLabelAssetList.TITLE)</a></li>
                        End If
                        @* 資産在庫照会 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.StockList, ViewBag.LoginInfo) Then
                            @<li><a href="~/StockList/Init">@Message.getMessage(CommonLabelStockList.TITLE)</a></li>
                        End If
                        @* 資産履歴照会 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.AssetHistory, ViewBag.LoginInfo) Then
                            @<li><a href="~/AssetHistory/Init">@Message.getMessage(CommonLabelAssetHistory.TITLE)</a></li>
                        End If
                        <!-- 2023/03/22 Kawamoto ADD START -->
                        @* 指定数量管理表出力 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.DrugSpecList, ViewBag.LoginInfo) Then
                            @<li><a href="~/DrugSpecList/Init">@Message.getMessage(CommonLabelDrugSpecList.TITLE)</a></li>
                        End If
                        @* 管理簿出力 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.DrugManageList, ViewBag.LoginInfo) Then
                            @<li><a href="~/DrugManageList/Init">@Message.getMessage(CommonLabelDrugManageList.TITLE)</a></li>
                        End If
                        <!-- 2023/03/22 Kawamoto ADD END -->
                    </ul>
                </li>
            End If
            @* 工事メニュー *@
            @If Message.IsDspMenu(Misc.Auth.MenuId.Construction, ViewBag.LoginInfo) Then
                @<li Class="dropdown">
                    <a href="#" Class="dropdown-toggle" data-toggle="dropdown" role="button">@Message.getMessage(CommonLabel.TITLE_CONSTRUCTION)</a>
                    <ul Class="dropdown-menu" role="menu">
                        @* 工事一覧 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.ConstructionList, ViewBag.LoginInfo) Then
                            @<li><a href="~/ConstructionList/Init">@Message.getMessage(CommonLabelConstructionList.TITLE)</a></li>
                        End If
                        @* 工事資産一覧 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.ConstAssetList, ViewBag.LoginInfo) Then
                            @<li><a href="~/ConstAssetList/Init">@Message.getMessage(CommonLabelConstAssetList.TITLE)</a></li>
                        End If
                        @* 工事履歴一覧 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.ConstructionHist, ViewBag.LoginInfo) Then
                            @<li><a href="~/ConstructionHist/Init">@Message.getMessage(CommonLabelConstructionHist.TITLE)</a></li>
                        End If
                        @* 仮置一覧 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.TemporaryList, ViewBag.LoginInfo) Then
                            @<li><a href="~/TemporaryList/Init">@Message.getMessage(CommonLabelTemporaryList.TITLE)</a></li>
                        End If
                    </ul>
                </li>
            End If
            @* 貸出メニュー *@
            @If Message.IsDspMenu(Misc.Auth.MenuId.Lending, ViewBag.LoginInfo) Then
                @<li Class="dropdown">
                    <a href="#" Class="dropdown-toggle" data-toggle="dropdown" role="button">@Message.getMessage(CommonLabel.TITLE_LEASE)</a>
                    <ul Class="dropdown-menu" role="menu">
                        @* 予約状況照会 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.LendingStatus, ViewBag.LoginInfo) Then
                            @<li><a href="~/LendingStatus/Init">@Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS)</a></li>
                        End If
                        @* 予約一覧 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.LendingList, ViewBag.LoginInfo) Then
                            @<li><a href="~/LendingList/Init">@Message.getMessage(CommonLabelLendingList.TITLE)</a></li>
                        End If
                        @* 貸出先一覧 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.AssetLeasePlaceList, ViewBag.LoginInfo) Then
                            @<li><a href="~/AssetLeasePlaceList/Init">@Message.getMessage(CommonLabelAssetLeasePlaceList.TITLE)</a></li>
                        End If
                        @* 貸出先資産照会 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.AssetLeasePlaceDetail, ViewBag.LoginInfo) Then
                            @<li><a href="~/AssetLeasePlaceDetail/Init">@Message.getMessage(CommonLabelAssetLeasePlaceDetail.TITLE)</a></li>
                        End If
                        @* 貸出履歴照会 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.AssetLeaseHitory, ViewBag.LoginInfo) Then
                            @<li><a href="~/AssetLeaseHitory/Init">@Message.getMessage(CommonLabelAssetLeaseHitory.TITLE)</a></li>
                        End If
                    </ul>
                </li>
            End If
            @* 棚卸メニュー *@
            @If Message.IsDspMenu(Misc.Auth.MenuId.Inventory, ViewBag.LoginInfo) Then
                @<li Class="dropdown">
                    <a href="#" Class="dropdown-toggle" data-toggle="dropdown" role="button">@Message.getMessage(CommonLabel.TITLE_INVENTORY)</a>
                    <ul Class="dropdown-menu" role="menu">
                        @* 棚卸進捗状況 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.InventorySummary, ViewBag.LoginInfo) Then
                            @<li><a href="~/InventorySummary/Init">@Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE)</a></li>
                        End If
                        @* 棚卸実績一覧 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.InventoryInquiry, ViewBag.LoginInfo) Then
                            @<li><a href="~/InventoryHistory/Init">@Message.getMessage(CommonLabelInventoryInquiry.TITLE_INV_RESULT)</a></li>
                        End If
                    </ul>
                </li>
            End If
            @* マスタメンテナンスメニュー *@
            @If Message.IsDspMenu(Misc.Auth.MenuId.Master, ViewBag.LoginInfo) Then
                @<li Class="dropdown">
                    <a href="#" Class="dropdown-toggle" data-toggle="dropdown" role="button">@Message.getMessage(CommonLabel.TITLE_MST)</a>
                    <ul Class="dropdown-menu" role="menu">
                        @* カテゴリーマスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.Category, ViewBag.LoginInfo) Then
                            @<li><a href="~/Category">@Message.getMessage(CommonLabelCategory.TITLE)</a></li>
                        End If
                        @* ロケーションマスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.Location, ViewBag.LoginInfo) Then
                            @<li><a href="~/Location/Init">@Message.getMessage(CommonLabelLocation.TITLE)</a></li>
                        End If
                        @* 部署マスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.Department, ViewBag.LoginInfo) Then
                            @<li><a href="~/Department">@Message.getMessage(CommonLabelDepartment.TITLE)</a></li>
                        End If
                        @* 社員マスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.Employee, ViewBag.LoginInfo) Then
                            @<li><a href="~/Employee/Init">@Message.getMessage(CommonLabelEmployee.LABEL_TITLE)</a></li>
                        End If
                        @* 貸出先マスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.LeasePlace, ViewBag.LoginInfo) Then
                            @<li><a href="~/LeasePlace">@Message.getMessage(CommonLabelLeasePlace.TITLE)</a></li>
                        End If
                        @* 設定マスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.Setting, ViewBag.LoginInfo) Then
                            @<li><a href="~/Setting">@Message.getMessage(CommonLabelSetting.TITLE)</a></li>
                        End If
                        @* 文言マスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.Word, ViewBag.LoginInfo) Then
                            @<li><a href="~/Word">@Message.getMessage(CommonLabelWord.TITLE)</a></li>
                        End If
                        @* CSVマッピング *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.CsvMapping, ViewBag.LoginInfo) Then
                            @<li><a href="~/CsvMapping">@Message.getMessage(CommonLabelCsvMap.TITLE)</a></li>
                        End If
                        @* カレンダーマスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.Calender, ViewBag.LoginInfo) Then
                            @<li><a href="~/Calendar">@Message.getMessage(CommonLabelCalendar.TITLE)</a></li>
                        End If
                        @* 商品マスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.Goods, ViewBag.LoginInfo) Then
                            @<li><a href="~/Goods">@Message.getMessage(CommonLabelGoods.TITLE)</a></li>
                        End If
                        @* 項目配置マスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.ColSet, ViewBag.LoginInfo) Then
                            @<li><a href="~/ColSet">@Message.getMessage(CommonLabelColSet.TITLE)</a></li>
                        End If
                    </ul>
                </li>
            End If
            @* ゲートメニュー *@
            @If Message.IsDspMenu(Misc.Auth.MenuId.Gate, ViewBag.LoginInfo) Then
                @<li Class="dropdown">
                    <a href="#" Class="dropdown-toggle" data-toggle="dropdown" role="button">@Message.getMessage(CommonLabel.TITLE_GATE)</a>
                    <ul Class="dropdown-menu" role="menu">
                        @* ゲートマスタメンテ *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.Gate, ViewBag.LoginInfo) Then
                            @<li><a href="~/Gate">@Message.getMessage(CommonLabelGate.TITLE)</a></li>
                        End If
                        @* ゲート持ち出し履歴 *@
                        @If Message.IsDspFunction(Misc.Auth.FunctionId.GatePassLog, ViewBag.LoginInfo) Then
                            @<li><a href="~/GatePassLog">@Message.getMessage(CommonLabelGatePassLog.TITLE)</a></li>
                        End If
                    </ul>
                </li>
            End If
            @* ログ照会メニュー *@
            @If Message.IsDspFunction(Misc.Auth.FunctionId.OperationLog, ViewBag.LoginInfo) Then
                @<li Class="dropdown">
                    <a href="~/OperationLog/Init">@Message.getMessage(CommonLabel.TITLE_LOG)</a>
                </li>
            End If
        </ul>
        <div class="content-title">
            <p class="Line3" />
            <p class="Line4" />
            <h1 id="site-title">@ViewBag.Title</h1>
            <div class="user-area">
                <span Class="username">
                    @If Not String.IsNullOrEmpty(ViewBag.LoginUserNm) Then
                        @* @Message.getMessage(CommonLabel.LABEL_USER) 2021/09/25 DEL nv Inoue*@
                        @Message.GetUserDspNm(ViewBag.LoginInfo)
                    End If
                </span>
                <span class="logout">
                    @If Not String.IsNullOrEmpty(ViewBag.LoginUserNm) Then
                        @Html.ActionLink(Message.getMessage(CommonLabel.BUTTON_LOGOUT), "Index", New With {.actionname = CommonConst.KEY_FROM_LOGOUT, .controller = "Login"})
                    End If
                </span>
                @*<span class="password"><a href="~/Login">@Message.getMessage(CommonLabel.LABEL_EDITPASSWORD)</a></span>|*@
                @*<span class="logout"><a href="~/Login">@Message.getMessage(CommonLabel.BUTTON_LOGOUT)</a></span>*@
            </div>
            <p class="Line3" />
            <p class="Line4" />
        </div>
    </header>
    <div class="container body-content">
        @RenderBody()

        <!-- 全画面ローディング用（非表示） -->
        <div id="overlay" style="display:none;"></div>
    </div>
    <footer></footer>

    <!-- 確認メッセージ用 -->
    <div id="show-dialog"></div>
    <!-- エラーメッセージ用 -->
    <div id="show-message"></div>

    @* 複数入力ダイアログ（共通） *@
    @Html.Partial("_MultiInputDialog")

    @Scripts.Render("~/bundles/bootstrap")
    @* 複数入力ダイアログ・×クリアボタン（共通） *@
    @Scripts.Render("~/bundles/multiinput")
    @* select 表示（選択肢＝「コード 名称」／選択後＝コードのみ）（共通） *@
    @Scripts.Render("~/bundles/selectcodename")
    @RenderSection("scripts", required:=False)

    <!-- ダウンロード用iframe(非表示) -->
    <iframe name="downloadFrame" style="display:none;"></iframe>

    <script type="text/javascript">
        // 画面ロード完了時に overlay を非表示
        $(window).on("load", function () {
            hideLoading();
        });
    </script>
</body>
    </html>
