@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of EntryProductKeyViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLicense.TITLE)
    Layout = "~/Views/Shared/_LayoutLogin.vbhtml"
End Code

<script>
    $(function () {

        // ライセンスセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessCreate") {
            //登録メッセージ表示後、ログイン画面に遷移
            showMessageAndSeni("@Message.getMessage(CommonLabelLicense.MSG_OK_PRODUCT_KEY)", "@Url.Content("~/Login")/Index");
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        $("#PRODUCT_KEY1").focus();

    });

    // キー表示ボタン押下処理
    function windowOpen() {
        var url = "@Message.getSettingInfo(CommonSettingKey.GetkeyURL.KeyDiv, CommonSettingKey.GetkeyURL.Key1, CommonSettingKey.GetkeyURL.Key2, CommonSettingKey.GetkeyURL.Key3).DATA1";
        //window.open(url, "新規"); //タブで表示
        window.open(url, "新規", "top=100,left=100,width=900,height=400"); //別ウィンドウで表示

    }

</script>

<div id="entry-productkey">
    @Using (Html.BeginForm("Index", "EntryProductKey", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()

       @<H1 Class="logo" style="text-align:center;margin-bottom:20px;"><IMG alt="ASETRA" src="~/Images/ASETRA.jpg" width="400" height="60"></H1>
        @<div class="formbox2" style="padding-bottom:5px;">
            <div class="formboxheader4">
                <H2>@Message.getMessage(CommonLabelLicense.TITLE)</H2>
                <div style="clear: both;"></div>
            </div>
             <div Class="form1">
                 <div class="productKey" style="margin-Top:10px;font-size: 14px;">
                     @*<table style="margin:0px auto;margin-bottom:10px;">
                <tr>
                    <td class="lab" style="text-align:left; padding: 4px;"><LABEL for="uid">@Message.getMessage(CommonLabelLicense.LABEL_NO):</LABEL></td>
                    <td colspan="4" style="text-align:left;">@Html.DisplayFor(Function(model) model.FirstOrDefault.PRODUCT_CD)</td>
                    <td>
                        <input type="button" class="defButton productkey-large" value="@Message.getMessage(CommonLabelLicense.BUTTON_GET_KEY)" onclick="windowOpen()" />
                    <td>
                </tr>
                <tr>
                    <td class="lab" style="text-align:left; padding: 4px;">@Message.getMessage(CommonLabelLicense.TITLE)</td>
                </tr>
                <tr style="margin-top:5px;">
                    <td>@Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY1, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})</td>
                    <td>@Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY2, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})</td>
                    <td>@Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY3, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})</td>
                    <td>@Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY4, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})</td>
                    <td>@Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY5, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})</td>
                    <td><input type="submit" class="defButton productkey-large" formaction="~/EntryProductKey/Entry" value="@Message.getMessage(CommonLabelLicense.BUTTON_REGIST)" /></td>
                </tr>
            </table> yamada*@
          
                     <div style="text-align:left; width:400px; padding-top:8px; padding-bottom:8px; display:inline-block;">
                         <LABEL id="lab">@Message.getMessage(CommonLabelLicense.LABEL_NO):</LABEL>
                         <div style="text-align:left; padding: 4px; display:inline-block;">@Html.DisplayFor(Function(model) model.FirstOrDefault.PRODUCT_CD)</div>
                     </div>
                     <input type="button" class="defButton productkey-large" value="@Message.getMessage(CommonLabelLicense.BUTTON_GET_KEY)" onclick="windowOpen()" />

                     <div style="text-align:left; width:400px; padding-left: 10px;">
                         <LABEL id="lab">@Message.getMessage(CommonLabelLicense.TITLE)</LABEL>
                     </div>
                     
                     <div style="text-align:left; width:400px; padding-bottom:10px;  display:inline-block; font-size: 14px;">
                         <div style="text-align:left; width:18px; display:inline-block;"></div>
                         @Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY1, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})
                         @Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY2, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})
                         @Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY3, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})
                         @Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY4, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})
                         @Html.TextBoxFor(Function(model) model.FirstOrDefault.PRODUCT_KEY5, New With {.maxlength = 5, .style = "width:70px;text-align:center", .oninput = "editProductKey(this)"})
                     </div>
                     <input type="submit" class="defButton productkey-large" formaction="~/EntryProductKey/Entry" value="@Message.getMessage(CommonLabelLicense.BUTTON_REGIST)" />
                 </div>
          
             <div style="height: 10px; clear: both;"></div>
                 <table style="margin:auto auto;">
                     <tr style="text-align:center">
                         <td>
                             @If Model.FirstOrDefault.TRIAL_LIMIT > 0 Then
                                 @<input type="submit" Class="defButton" formaction="~/EntryProductKey/ToLogin" value="@Message.getMessage(CommonLabelLicense.BUTTON_TRIAL)" />
                             End If
                         <td>
                     </tr>
                 </table>
             </div>
            @If Model.FirstOrDefault.TRIAL_LIMIT > 0 Then
                 @<div Class="form1" style="margin-top:15px;">
                     <div Class="trialLabel">
                         <Table style = "margin:0 auto;font-size: 12px;font-weight: bold" >
                             <tr>
                                <td style="text-align:left;"><LABEL>@Message.getMessage(CommonLabelLicense.LABEL_TRIAL1)</LABEL></td>
                                <td style="padding-bottom:5px;">@Html.DisplayFor(Function(model) model.FirstOrDefault.TRIAL_LIMIT)</td>
                                 <td style="text-align:left;"><LABEL>@Message.getMessage(CommonLabelLicense.LABEL_TRIAL2)</LABEL></td>
                             </tr>
                         </table>
                     </div>
                 </div>
            End If

        </div>
    End Using
         </div>
