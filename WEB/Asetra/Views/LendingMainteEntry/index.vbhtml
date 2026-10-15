@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of LendingMainteEntryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    If Model.FirstOrDefault.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK Then
        Select Case Model.FirstOrDefault.MAINTE_KBN
            Case MainteKbn.Mainte
                ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_HENKOU)
            Case MainteKbn.BlaRegist
                ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_BLE_HENKOU)
            Case MainteKbn.PreRegist1
                ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_PRE_HENKOU1)
            Case MainteKbn.PreRegist2
                ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_PRE_HENKOU2)
        End Select
    Else
        Select Case Model.FirstOrDefault.MAINTE_KBN
            Case MainteKbn.Mainte
                ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_TOUROKU)
            Case MainteKbn.BlaRegist
                ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_BLE_TOUROKU)
            Case MainteKbn.PreRegist1
                ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_PRE_TOUROKU1)
            Case MainteKbn.PreRegist2
                ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_PRE_TOUROKU2)
        End Select
    End If
End Code
<script>
    //
    //  メンテナンス登録/変更
    //
    $(function () {

        // 資産番号・シリアルNo・管理ロケーション・資産名書き込み不可
        document.getElementById("ASSET_NO").readOnly = true;
        $("#ASSET_NO").addClass("disable");
        document.getElementById("SerialNo").readOnly = true;
        $("#SerialNo").addClass("disable");
        document.getElementById("MngLocLName").readOnly = true;
        $("#MngLocLName").addClass("disable");
        document.getElementById("AssetName").readOnly = true;
        $("#AssetName").addClass("disable");

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            // エラーメッセージ表示後、ライセンスコントローラーを呼び出す
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessCreate") {
            //登録
            // メッセージ表示後、予約状況照会画面を呼び出す
            showMessageAndSeni("@Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE)", "@Url.Content("~/LendingStatus")/Index");
            return;
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessEdit") {
            //変更
            // メッセージ表示後、予約状況照会画面を呼び出す
            showMessageAndSeni("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)", "@Url.Content("~/LendingStatus")/Index");
            return;
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessDelete") {
            //削除
            // メッセージ表示後、予約状況照会画面を呼び出す
            showMessageAndSeni("@Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE)", "@Url.Content("~/LendingStatus")/Index");
            return;
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {

            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }


    });

    // 登録ボタンクリック処理
    function btnCreate_onclick() {

       showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/LendingMainteEntry")/Create")
        return true;
    }

    // 変更ボタンクリック処理
    function btnEdit_onclick() {
            showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/LendingMainteEntry")/Create")
            return true;
    }

    // 削除ボタンクリック処理
    function btnDelete_onclick() {
        //確認メッセージ
        showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/LendingMainteEntry")/Delete");
    }

    // 戻るボタンクリック処理
    function btnReturn_onclick() {

        return true;
    }
</script>

<div id="lending-entry">
    @Using (Html.BeginForm("Index", "LendingEntry", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    @If model.FirstOrDefault.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK Then
                        @If Model.FirstOrDefault.AuthProcFlg <> AuthProcFlg.ShokaiNomi Then
                            @<input Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">
                            @<input Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="btnDelete_onclick()">
                        End If
                    Else
                        @<input Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_TOUROKU)" onclick="btnCreate_onclick()">
                    End If
                </div>
                <div Class="view-area-button-right">
                    <input Class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "LendingStatus")'">
                    @*<input class="defButton return-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="btnReturn_onclick()">*@
                </div>
            </div>
        </div>
        @<div Class="search-wrap">
            <div class="search-border">
                <Table Class="detail-table">
                    <tr Class="detail-row">
                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_MNG_LOC)</td>
                        <td width="20" style="display:inline-block;"></td>
                        <td class="condition-col" style="display:inline-block; width:420px;">
                            @Html.EditorFor(Function(model) model.FirstOrDefault.MngLocLName, New With {.htmlAttributes = New With {.style = "width: 190px;"}})
                        </td>
                    </tr>
                    <tr Class="detail-row">
                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_NO)</td>
                        <td width="20" style="display:inline-block;"></td>
                        <td class="condition-col" style="display:inline-block; width:420px;">
                            @Html.EditorFor(Function(model) model.FirstOrDefault.ASSET_NO, New With {.htmlAttributes = New With {.style = "width: 190px;"}})
                        </td>
                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_SERIAL_NO)</td>
                        <td width="20" style="display:inline-block;"></td>
                        <td class="condition-col" style="display:inline-block; width:420px;">
                            @Html.EditorFor(Function(model) model.FirstOrDefault.SerialNo, New With {.htmlAttributes = New With {.style = "width: 190px;"}})
                        </td>
                    </tr>
                    <tr Class="detail-row">
                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</td>
                        <td width="20" class="require" style="display:inline-block;"></td>
                        <td class="condition-col" style="display:inline-block; width:420px;">
                            @Html.EditorFor(Function(model) model.FirstOrDefault.AssetName, New With {.htmlAttributes = New With {.style = "width: 190px;"}})
                        </td>
                    </tr>
                    <tr Class="detail-row">
                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_MAINTE_KIKAN)</td>
                        <td width="20" class="require" style="display:inline-block;">※</td>
                        <td class="condition-col" style="display:inline-block; width:420px;">
                            @Html.EditorFor(Function(model) model.FirstOrDefault.MainteStartDtString, New With {.htmlAttributes = New With {.type = "date"}})
                            ～
                            @Html.EditorFor(Function(model) model.FirstOrDefault.MainteEndDtString, New With {.htmlAttributes = New With {.type = "date"}})
                        </td>
                    </tr>
                    <tr Class="detail-row" style="height:75px">
                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK)</td>
                        <td width="20" class="require" style="display:inline-block;"></td>
                        <td class="condition-col" style="display:inline-block; width:420px;">
                            @Html.TextAreaFor(Function(model) model.FirstOrDefault.REMARK, New With {.maxlength = 255, .rows = "3", .style = "width: 400px;resize: none"})
                        </td>
                    </tr>
                </Table>
            </div>
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.MAINTE_ID)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.AuthProcFlg)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.MAINTE_START_DT)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.MAINTE_END_DT)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.INS_DATETIME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.INS_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.INS_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_DATETIME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_CD)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_EMPLOYEE_NAME)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.UPD_TASK)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.MAINTE_KBN)
    End Using
</div>
