@Imports Asetra.Misc
@Code
    ViewData(CommonConst.KEY_PAGE_TITLE) = "404 Page Not Found."
    Layout = "~/Views/Shared/_LayoutLogin.vbhtml"
End Code

<div class="error-content">
    @Using (Html.BeginForm("Index", "NotFound", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<H1 Class="logo" style="text-align:center"><img src="~/Images/ASETRA.jpg" alt="ASTERA" width="200" /></H1>
        @<div align="center" style="margin-top:50px" >
            <h2 style="font-size:30px">ページが見つかりません。</h2>
            <div Class="error-title" style="margin-top:10px">
                <p>お探しのページは移動もしくは削除された可能性があります。</p>
                <p>ページ(URL)が正しいかご確認ください。</p>
            </div>
            <table style="margin-top:30px;">
                <tr style="text-align:center">
                    <td>
                        <input type="submit" class="defButton" formaction="~/Login" value="ログイン画面" />
                    <td>
                </tr>
            </table>
        </div>
    End Using
</div>