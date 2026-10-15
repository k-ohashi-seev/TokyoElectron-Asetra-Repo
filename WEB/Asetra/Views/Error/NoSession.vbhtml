@Code
    ViewData("Title") = ""
End Code

@Using (Html.BeginForm("Index", "Login", FormMethod.Post))
@<div Class="error-content">
    <h2> セッションタイムアウト</h2>
    <div Class="error-title">
        <p> セッションがタイムアウトになりました。</p>
        <p> 再度ログインしてください。</p>
    </div>
    <input type = "submit" class="defButton" name="login" value="ログイン" />
</div>
End Using
