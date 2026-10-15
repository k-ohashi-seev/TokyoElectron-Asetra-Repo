@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = ""
    'Layout = "~/Views/Shared/_Layout.vbhtml"
End Code

<script>
    $(function () {
        //Controllerでエラーの場合、エラーメッセージを表示する
        @*if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")

        }*@

    });

</script>

<h2></h2>

