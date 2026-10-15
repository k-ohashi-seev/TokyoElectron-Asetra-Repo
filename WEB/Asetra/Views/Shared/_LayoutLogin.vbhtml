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

    @Styles.Render("~/Content/styles.css")

    @Scripts.Render("~/Scripts/jquery.collapse.js")
    @Scripts.Render("~/bundles/select2")

    <script>
        //ダイアログのタイトルを取得(common.jsで表示する)
        var strConfirmTitle = "@Message.getMessage(CommonLabel.LABEL_CONFIRM)";
        var strErrMessageTitle = "@Message.getMessage(CommonLabel.LABEL_ERROR)";

    </script>

</head>
<body>
    <header>
        @*
        <span class="logo"><img src="~/Images/SATO-logo.png" alt="SATO" width="200" /></span>
        <div class="content-title">
            <p class="Line3" />
            <p class="Line4" />
            <h1 id="site-title">@ViewBag.Title</h1>
            <p class="Line3" />
            <p class="Line4" />
        </div>
        *@
    </header>
    <div class="container body-content">
        @RenderBody()
    </div>
    <footer></footer>

    <!-- 確認メッセージ用 -->
    <div id="show-dialog"></div>
    <!-- エラーメッセージ用 -->
    <div id="show-message"></div>

    @Scripts.Render("~/bundles/bootstrap")
    @RenderSection("scripts", required:=False)
</body>
</html>
