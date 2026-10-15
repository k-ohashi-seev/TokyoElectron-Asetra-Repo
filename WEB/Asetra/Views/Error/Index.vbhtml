@Imports Asetra.Misc
@Code
    ViewData(CommonConst.KEY_PAGE_TITLE) = "システムエラーが発生しました。"
    Layout = "~/Views/Shared/_LayoutLogin.vbhtml"
End Code

<div class="error-content">
    @Using (Html.BeginForm("Index", "NotFound", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
    @<H1 Class="logo" style="text-align:center"><img src="~/Images/ASETRA.jpg" alt="ASTERA" width="200" /></H1>
        @<div align="center" style="margin-top:50px">
            <h2 style="font-size:30px">システムエラー</h2>
            <table>
                <tr>
                    <td>
                        <div Class="error-title" style="margin-top:10px">
                            <p>システムエラーが発生しました。</p>
                            <p>システム管理者に連絡してください。</p>
                        </div>
                        <div class="error-detail">
                            <p>エラー発生時刻：@CommonRfid.ToStringNvl(Session(CommonSessionKey.SYSTEM_ERR_DATE)) </p>
                            <p>エラー内容：@CommonRfid.ToStringNvl(Session(CommonSessionKey.SYSTEM_ERR_MESSAGE))</p>
                        </div>
                    </td>

                </tr>
            </table>
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

