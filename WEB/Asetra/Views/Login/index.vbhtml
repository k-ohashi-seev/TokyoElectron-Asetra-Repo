@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of LoginViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabel.BUTTON_LOGIN)
    Layout = "~/Views/Shared/_LayoutLogin.vbhtml"
End Code

<script>
    $(function () {

        // ライセンスセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        // オプション項目表示非表示設定
        if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_LOGINOPTION_CLOSE)") {
            $(".detail-accordion").css("display", "inline-block");
            document.getElementById("DetailAccordion").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        $("#uid").focus();

        //$("#divActionsMain").collapse({
        //    head: "h4",
        //    group: "div, ul, ol",
        //    show: function () {
        //        this.animate({
        //            opacity: 'toggle',
        //            height: 'toggle'
        //        }, 300);
        //        $("#triangle").html($("#triangle").html().replace("▼", "▲"));
        //    },
        //    hide: function () {
        //        this.animate({
        //            opacity: 'toggle',
        //            height: 'toggle'
        //        }, 300);
        //        $("#triangle").html($("#triangle").html().replace("▲", "▼"));
        //    }
        //});

    });

    //ログインボタンクリック処理
    function btnLogin_onclick() {
        @*//確認メッセージ
        showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/Login")/Login");*@
        document.getElementById("openFlg").value = document.getElementById("DetailAccordion").innerHTML;
    }

    //ログインオプションクリック時処理
    function DetailAccordion_onclick() {
        if (document.getElementById("DetailAccordion").innerHTML == "@Message.getMessage(CommonLabel.LABEL_LOGINOPTION_CLOSE)") {
            $(".detail-accordion").css("display", "none");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_LOGINOPTION_OPEN)"
        } else {
            $(".detail-accordion").css("display", "inline-block");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_LOGINOPTION_CLOSE)"
        }
    }

    function enter() {
        if (window.event.keyCode == 13) {
            document.form1.submit();
        }
    }


</script>

@*<STYLE type="text/css">
    #divActionsMain .hide {
        background: #eee;
        -moz-border-radius: 3px;
        -webkit-border-radius: 3px;
        border-radius: 3px;
        position: relative;
        margin-top: 0.5em;
        margin-right: 25px;
        margin-bottom: 0.5em;
        margin-left: 25px;
        padding: 3px;
    }
</STYLE>*@


<div id="login">
    @Using (Html.BeginForm("Index", "Login", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<H1 Class="logo" style="text-align:center;margin-bottom:20px;"><IMG alt="ASETRA" src="~/Images/ASETRA.jpg" width="400" height="60"></H1>
        @<div class="formbox">
            <div class="formboxheader3">
                <H2>@Message.getMessage(CommonLabelLogin.TITLE)</H2>
                <div style="clear: both;"></div>
            </div>
            <div Class="form1">
                <div class="login">
                    <table style="margin:0 auto">
                        <tr>
                            <td class="lab" scope="col"><LABEL for="uid">@Message.getMessage(CommonLabel.LABEL_EMPLOYEEID)</LABEL></td>
                            <td class="lab" scope="col"></td>
                            <td scope="col">@Html.EditorFor(Function(model) model.FirstOrDefault.EMPLOYEE_CD, New With {.htmlAttributes = New With {.maxlength = 15, .oninput = "editHanEi(this)"}})</td>
                        </tr>
                    </table>
                </div>
                <div class="login">
                    <table style="margin:0 auto">
                        <tr>
                            <td class="lab" scope="col" width="120px" style="padding-left: 20px;">@Message.getMessage(CommonLabel.LABEL_PASSWORD)</td>
                            <td class="lab" scope="col" width="20"></td>
                            <td scope="col">@Html.EditorFor(Function(model) model.FirstOrDefault.PASSWORD, New With {.htmlAttributes = New With {.maxlength = 60}})</td>
                        </tr>
                    </table>
                </div>

                <div style="height: 15px; clear: both;"></div>

                @*<H4 class="inactive" id="triangle">ログインオプション ▼			 </H4>*@
                <div style="text-align:center"><b><a href="#" id="DetailAccordion" onclick="DetailAccordion_onclick();return false;">@Message.getMessage(CommonLabel.LABEL_LOGINOPTION_OPEN)</a></b></div>
                @*<div class="hide" aria-hidden="true" style="display: none;">*@
                <div class="detail-accordion" style="width:495px; display: none;">
                    <P>
                        <div class="login">
                            <table style="margin:0 auto">
                                <tr>
                                    <td class="lab" scope="col">@Message.getMessage(CommonLabel.LABEL_NEWPASSWORD)</td>
                                    <td class="lab" scope="col"></td>
                                    <td scope="col">@Html.EditorFor(Function(model) model.FirstOrDefault.NewPassword, New With {.htmlAttributes = New With {.maxlength = 60}})</td>
                                </tr>
                            </table>
                        </div>
                        <div class="login">
                            <table style="margin:0 auto">
                                <tr>
                                    <td class="lab" scope="col">@Message.getMessage(CommonLabel.LABEL_KAKUNINPASSWORD)</td>
                                    <td class="lab" scope="col"></td>
                                    <td scope="col">@Html.EditorFor(Function(model) model.FirstOrDefault.KakuninPassword, New With {.htmlAttributes = New With {.maxlength = 60}})</td>
                                </tr>
                            </table>
                        </div>
                    </P>
                </div>
                <table style="margin:30px auto">
                    <tr style="text-align:center">
                        <td>
                            <input type="submit" class="defButton" formaction="~/Login/Login" value="@Message.getMessage(CommonLabel.BUTTON_LOGIN)" onclick="return btnLogin_onclick()" />
                            @Html.EditorFor(Function(model) model.FirstOrDefault.OpenFlg, New With {.htmlAttributes = New With {.id = "openFlg", .type = "hidden"}})
                        <td>
                            @*<td>
                                    <input type="button" style="margin-top: 20px;margin-left: 50px;" class="defButton" value="@Message.getMessage(CommonLabel.BUTTON_EXIT)" />
                                <td>*@
                    </tr>
                    @*<tr>
                            <td width="400px" style="padding-left: 120px;padding-top: 30px;"><b>@Message.getMessage(CommonLabel.TITLE_COPYRIGHT)</b></td>
                        </tr>*@
                </table>
            </div>
        </div>
        @<div class="text-center" style="margin-top: 30px; margin-bottom: 30px;">@Message.getMessage(CommonLabel.TITLE_COPYRIGHT) @ViewBag.Version</div>

        '2023/03/22 Kawamoto ADD START
        @If Model.FirstOrDefault.NEWS_CD IsNot Nothing Then

            @<div id="news_list">
                 <table class="news-table" rules="none" border="1" style="margin: auto; width:500px">
                    @*お知らせ一覧表示*@
                    @Code
                    Dim cnt As Integer = Model.Count
                    If cnt > NewsDownload.DISP_ROW_COUNT Then
                        cnt = NewsDownload.DISP_ROW_COUNT
                    End If
                    End Code

                    @For i As Integer = 0 To cnt - 1
                        @<tr>
                             <td>
                                @If String.IsNullOrEmpty(Model(i).DOWNLOAD_URL) Then
                                    @Html.DisplayFor(Function(modelItem) Model(i).DISPLAY_NAME)
                                Else
                                    @<a href="@Model(i).DOWNLOAD_URL" target="_blank" rel="noopener noreferrer" >@Model(i).DISPLAY_NAME</a>
                                End If
                             </td>
                         </tr>
                    Next
                 </table>
             </div>
        End If
        '2023/03/22 Kawamoto ADD END

    End Using
</div>
