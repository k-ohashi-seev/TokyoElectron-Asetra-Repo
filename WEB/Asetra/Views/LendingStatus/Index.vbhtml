@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of LendingStatusViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS)
End Code
<style>
    button.fold {
        width: auto;
        background: none;
        border: 0;
    }
</style>
<script>
    //
    //予約状況照会画面
    //

    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            //エラーメッセージ表示後、ライセンスコントローラーを呼び出す
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        //20230904 ADD Start Shirakawa
        //上限を超えている場合エラーメッセージ
        if ("@ViewBag.isLimitFlg" == "True") {
            showErrMessage("@String.Format(Message.getMessage(CommonLabel.MSG_ERR_LIMIT_DATA), ViewBag.cntList, ViewBag.cntLimit)")

        }
        //20230904 ADD End Shirakawa

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        // オプション項目表示非表示設定
        if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "inline-block");
            document.getElementById("DetailAccordion").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }

        // 一覧画面の表示幅を設定
        var arrayObj = @Html.Raw(Json.Encode(Model.FirstOrDefault.isClosedColumns)) ;


        @*// 列設定の箇所が2列目からなので初期値は2に設定
        for (var i = 2; i < arrayObj.length; i++) {
            if (i <= 10) {                              //2022/08/31 Upd Kawamoto
                if (arrayObj[i]['isClosed'] == true) {
                    document.getElementById("col" + i).className = "col" + i + "_2";
                    for (var j = 0; j < document.getElementsByClassName("col" + i).length - 1; j++) {
                        document.getElementsByClassName("col" + i)[j].className = "col" + i + "_2";
                    }
                    document.getElementById("img" + i).srcset = "@Url.Content("~/Images")/plus.png";

                } else {
                    document.getElementById("col" + i).className = "col" + i;
                    for (var j = 0; j < document.getElementsByClassName("col" + i + "_2").length - 1; j++) {
                        document.getElementsByClassName("col" + i + "_2")[j].className = "col" + i;
                    }
                    document.getElementById("img" + i).srcset = "@Url.Content("~/Images")/minus.png";
                }
            }
            else {
                // 開始日とiから年月を割り出して、年月の一致する要素を探す。
                var dspStartDate = new Date(document.getElementById("SearchDspStartDate").value);
                dspStartDate.setDate(1);
                dspStartDate.setMonth(dspStartDate.getMonth() + (i - 11));  //2022/08/31 Upd Kawamoto
                var strDate = dspStartDate.getFullYear() + ("00" + (dspStartDate.getMonth() + 1)).slice(-2);

                for (var j = 0; j < arrayObj.length; j++) {
                    if (arrayObj[j]['columnName'] == strDate) {
                        if (arrayObj[j]['isClosed'] == true) {
                            // widthをcolspan数*10pxで設定
                            var strWidth = 0;
                            strWidth = document.getElementById("col" + i).colSpan * 10 + 'px';
                            document.getElementById("col" + i).style.width = strWidth;
                            $(".col" + i).css({ 'width': '10px' });
                            document.getElementById("img" + i).srcset = "@Url.Content("~/Images")/plus.png";
                        }
                        else {
                            // widthをcolspan数*25pxで設定
                            var strWidth = document.getElementById("col" + i).colSpan * 25 + 'px';
                            document.getElementById("col" + i).style.width = strWidth;
                            $(".col" + i).css({ 'width': '25px' });
                            document.getElementById("img" + i).srcset = "@Url.Content("~/Images")/minus.png";
                        }
                        break;
                    }
                }
            }
        }*@

        //選択数表示
        if (document.getElementById("cart-select-number") != null){
            document.getElementById("cart-select-number").innerHTML = @ViewBag.CartSelectNumber;
        }
        //カゴに申請データがありません
        if (document.getElementById("cart-no-data") != null){
            if (@ViewBag.CartSelectNumber == 0) {
                document.getElementById("cart-no-data").innerHTML = "@Message.getMessage(CommonLabelLendingStatus.MSG_ERR_CART_NO_DATA)";
            } else {
                document.getElementById("cart-no-data").innerHTML = "";
            }
        }

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessCreate") {
            //登録
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessEdit") {
            //変更
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessDelete") {
            //削除
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        // 大分類非選択時は中分類を非活性
        if (document.getElementById("SearchCatL").value == "") {
            document.getElementById("SearchCatM").value = ""
            $("#SearchCatM").attr("disabled", "disabled");
        }
        // 中分類非選択時は小分類を非活性
        if (document.getElementById("SearchCatM").value == "") {
            document.getElementById("SearchCatS").value = "";
            $("#SearchCatS").attr("disabled", "disabled");
        }

        btnCsvImportEnabled(true)       // 2022/7/25 sv sato add
    });

    //検索項目欄　詳細検索クリック時処理
    function DetailAccordion_onclick() {
        if (document.getElementById("DetailAccordion").innerHTML == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "none");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)"
        } else {
            $(".detail-accordion").css("display", "inline-block");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)"
        }
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        let lst = document.getElementById("search-sort");
        let checkBoxes = lst.getElementsByTagName("INPUT");
        let checkValue = '';
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                checkValue = checkBoxes[i].value;
            }
        }

        //確認メッセージ
        if (checkValue == '@LendingStatusSort.Reserve') {
            return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/LendingStatus")/CsvExportLendingRefList");
        } else {
            return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/LendingStatus")/CsvExportLendingRefMainte");
        }
    }

    //2022/7/25 sv sato add start
    //CSVデータ取込画面　取込開始ボタンクリック処理
    function btnCsvImportStart_onclick() {
        // ダイアログのメッセージを設定
        strMessage = "@Message.getMessage(CommonLabel.MSG_ASSET_CSVINPUT)".replace("@@@@CR@@@@", "<BR>");
        $("#show-dialog").html(strMessage);

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $(this).dialog("close");

                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Ok";
                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORT)";

                    // td内のtrをループ。rowsコレクションで,行位置取得。
                    var tblTbody = document.getElementById('csv-err-ichiran');
                    var rowCount = tblTbody.rows.length
                    for (var i=0; i<rowCount; i++) {
                        if( i < "@DataInport.DISP_ROW_COUNT"){
                            // tr内のtdをループ。cellsコレクションで行内セル位置取得。
                            for (var j=0, colLen=tblTbody.rows[i].cells.length ; j<colLen; j++) {
                                //i行目のj列目のセルをクリア
                                tblTbody.rows[i].cells[j].innerHTML ="　";
                                tblTbody.rows[i].cells[j].title ="　";
                            }
                        }else{
                            //i行目のセルを削除
                            tblTbody.rows["@DataInport.DISP_ROW_COUNT"].remove();
                        }
                    }

                    //CSVデータを取得し表示する(Ajax非同期通信)
                    var $form = $(this).closest('form')
                    var formData = new FormData($form.get(0));
                    formData.append("CsvFile", $('#CsvFile')[0].files[0]);

                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/LendingStatus")/ImportCsvFile',
                        data: formData,
                        dataType: 'json',
                        processData: false,
                        contentType: false,
                        cache: false,
                        success: function (data, dataType) {
                            this;

                            switch (data[0].ResultCd) {
                                case @DataInport.DataInportResultCd.OK:
                                    // 正常終了
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTOK)";
                                    for (var icnt = 0; icnt < data.length; icnt++) {
                                        if( icnt < "@DataInport.DISP_ROW_COUNT"){
                                            //icnt行目の各項目に値設定
                                            if (data[icnt].ErrRowIdx > 0) {
                                                tblTbody.rows[icnt].cells[0].innerHTML =data[icnt].ErrRowIdx;
                                                tblTbody.rows[icnt].cells[0].title = data[icnt].ErrRowIdx;
                                                tblTbody.rows[icnt].cells[1].innerHTML =data[icnt].ErrKoumoku;
                                                tblTbody.rows[icnt].cells[1].title = data[icnt].ErrKoumoku;
                                                tblTbody.rows[icnt].cells[2].innerHTML =data[icnt].ErrGaiyou;
                                                tblTbody.rows[icnt].cells[2].title =data[icnt].ErrGaiyou;
                                            }
                                        }else{
                                            $("#csv-err-ichiran").append('<tr><td class="csv_col1" title=' + data[icnt].ErrRowIdx + '>'+ data[icnt].ErrRowIdx +'</td><td class="csv_col2" title=' + data[icnt].ErrKoumoku + '>'+ data[icnt].ErrKoumoku +'</td>' + '<td class="csv_col3" title=' + data[icnt].ErrGaiyou + '>'+ data[icnt].ErrGaiyou +'</td></tr>');
                                        }
                                    }
                                    break;
                                case @DataInport.DataInportResultCd.NoSelect:
                                    // CSVファイル未選択
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SELECT)";
                                    break;
                                case @DataInport.DataInportResultCd.Empty:
                                    // CSVファイルが空
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_EMPTY)";
                                    break;
                                case @DataInport.DataInportResultCd.NotCsv:
                                    // CSVファイル以外
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_NOTCSV)";
                                    break;
                                case @DataInport.DataInportResultCd.KoumokuSuuErr:
                                    // 項目数エラー
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU)";
                                    break;
                                case @DataInport.DataInportResultCd.DataNG:
                                    // エラー有
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTNG)";

                                    for (var icnt = 0; icnt < data.length; icnt++) {
                                        if( icnt < "@DataInport.DISP_ROW_COUNT"){
                                            //icnt行目の各項目に値設定
                                            tblTbody.rows[icnt].cells[0].innerHTML =data[icnt].ErrRowIdx;
                                            tblTbody.rows[icnt].cells[0].title = data[icnt].ErrRowIdx;
                                            tblTbody.rows[icnt].cells[1].innerHTML =data[icnt].ErrKoumoku;
                                            tblTbody.rows[icnt].cells[1].title = data[icnt].ErrKoumoku;
                                            tblTbody.rows[icnt].cells[2].innerHTML =data[icnt].ErrGaiyou;
                                            tblTbody.rows[icnt].cells[2].title =data[icnt].ErrGaiyou;
                                        }else{
                                            $("#csv-err-ichiran").append('<tr><td class="csv_col1" title=' + data[icnt].ErrRowIdx + '>'+ data[icnt].ErrRowIdx +'</td><td class="csv_col2" title=' + data[icnt].ErrKoumoku + '>'+ data[icnt].ErrKoumoku +'</td>' + '<td class="csv_col3" title=' + data[icnt].ErrGaiyou + '>'+ data[icnt].ErrGaiyou +'</td></tr>');
                                        }

                                    }
                                    break;
                                case @DataInport.DataInportResultCd.LoginSession:
                                    // ログインセッション切れ
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_LOGININFO_EMPTY)";
                                    break;
                                case @DataInport.DataInportResultCd.CsvMapMei: @* 2019/12/19 nv miyaji add *@
                                    // CSVマッピング明細マスタエラー
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_CSVMAPMEI)";
                                    break;
                                case @DataInport.DataInportResultCd.CsvMapCharCd: @* 2019/12/19 nv miyaji add *@
                                    // CSVマッピングマスタ文字コードエラー
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_CHARCD)";
                                    break;
                                case @DataInport.DataInportResultCd.SettingInfo: @* 2019/12/19 nv miyaji add *@
                                    // 設定マスタの文字コード情報取得エラー
                                    document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                                    document.getElementById("modal-msg").innerHTML = data[0].ErrGaiyou;
                                    break;
                            }
                            // 2022/7/25 sv sato add start
                            if (data[0].ResultCd != "@DataInport.DataInportResultCd.OK" &&
                                data[0].ResultCd != "@DataInport.DataInportResultCd.NoSelect") {
                                //正常終了以外の場合、CSVデータ取込ボタンを非活性にする
                                btnCsvImportEnabled(false)
                            }
                            // 2022/7/25 sv sato add end
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            console.error("XMLHttpRequest : " + XMLHttpRequest.status);
                            console.error("textStatus     : " + textStatus);
                            console.error("errorThrown    : " + errorThrown.message);
                            this;
                            document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                            document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SOUTEIGAI)";
                            btnCsvImportEnabled(false)           // ADD sv sato 2022/7/25
                        }
                    });
                    return true;
                },
                "Cancel": function () {
                    $(this).dialog("close");
                    return false;
                }
            },
            open: function () {      // キャンセルボタンにフォーカスをあてる
                $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').focus();
            }
        });
    }

    //CSVモーダル CSVデータ取込ボタン押下可否設定処理
    function btnCsvImportEnabled(enable) {
        if (enable) {
            //活性
            $("#modal-import-btn").attr("disabled", false)
        } else {
            //非活性
            $("#modal-import-btn").attr("disabled", "disabled")
            $("#modal-import-btn").css("background-color","#cecece");
        }
    }
    //2022/7/25 sv sato add end

    //CSV取込ボタンクリック処理
    function btnCsvImport_onclick() {
        document.getElementById("popup-title-csv").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)";
        document.getElementById("modal-import-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)";
        document.getElementById("CsvFile").value = "";          //ファイル選択をクリア
        document.getElementById("modal-msg").innerHTML = "";    //メッセージをクリア
        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Nomal";   //メッセージの背景色を白にする

        // td内のtrをループ。rowsコレクションで,行位置取得。
        var tblTbody = document.getElementById('csv-err-ichiran');
        var rowCount = tblTbody.rows.length
        for (var i=0; i<rowCount; i++) {
            if (i < "@DataInport.DISP_ROW_COUNT") {
                // tr内のtdをループ。cellsコレクションで行内セル位置取得。
                for (var j = 0, colLen = tblTbody.rows[i].cells.length ; j < colLen; j++) {
                    //i行目のj列目のセルをクリア
                    tblTbody.rows[i].cells[j].innerHTML = "　";
                    tblTbody.rows[i].cells[j].title ="　";
                }
            } else {
                //i行目のj列目のセルを削除
                tblTbody.rows["@DataInport.DISP_ROW_COUNT"].remove();
            }

        }

        //CSV取込画面を表示
        ShowCsvModalWindow()
    }
    //2022/7/25 sv sato add end

    //選択件数クリック処理
    function lnkShowCartInfo_onclick() {
        //モーダルの明細クリア
        var tblTbody = document.getElementById('cart-info-list');
        var rowCount = tblTbody.rows.length;
        for (var i = 0; i < rowCount; i++) {
            tblTbody.rows[0].remove();
        }

        //カゴ情報表示処理(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingStatus")/ShowCartInfo',
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                } else if(data.length == 0) {
                    showErrMessage("@Message.getMessage(CommonLabelLendingStatus.MSG_ERR_CART_NO_DATA)")
                    return;
                }

                //管理ロケーション
                document.getElementById("popup-title-cart").innerHTML = data[0].MngLocLName;
                //明細
                for (var icnt = 0; icnt < data.length; icnt++) {
                    $("#cart-info-body").append('<tr><td class="col1" style="padding-right:10px;" title=' + data[icnt].AssetNo + '>'+ data[icnt].AssetNo +'</td><td class="col2" title=' + data[icnt].AssetName + '>'+ data[icnt].AssetName +'</td></tr>');
                }

                //モーダルウインドウを開く
                ShowCartModalWindow();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //検索ボタンクリック処理
    //※先月・当月・次月ボタンクリック時も以下の処理を行う
    function btnSearch_onclick(element) {
        if (element == null) {
            return;
        }

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        //管理ロケーションのチェック状態
        var keys = [];
        var lst = document.getElementById("search-mng-loc");
        var checkBoxes = lst.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                keys.push(checkBoxes[i].value);
            }
        }
        //管理ロケーションが未選択の場合はエラー
        if (keys.length == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_MNG_LOC)")
            return false;
        }
        // カンマ区切りの文字列を設定
        document.getElementById("SelectMngLocKeys").value = keys.join(",");

        //表示開始日が未入力の場合はエラー
        var dspStartDate = document.getElementById("SearchDspStartDate").value;
        if (dspStartDate == "") {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_DSP_START_DT_EMPTY)")
            return false;
        }

        //貸出状態のチェック状態
        var keysLeaseStatus = [];
        var lstLeaseStatus = document.getElementById("search-lease-status");
        var checkLeaseStatus = lstLeaseStatus.getElementsByTagName("INPUT");
        for (var i = 0; i < checkLeaseStatus.length; i++) {
            if (checkLeaseStatus[i].checked) {
                keysLeaseStatus.push(checkLeaseStatus[i].value);
            }
        }
        //貸出状態が未選択の場合はエラー
        if (keysLeaseStatus.length == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_LEASE_STATUS)")
            return false;
        }

        //出荷通知のチェック状態
        var keysShippingNotification = [];
        var lstShippingNotification = document.getElementById("search-shipping-notification");
        var checkShippingNotification = lstShippingNotification.getElementsByTagName("INPUT");
        for (var i = 0; i < checkShippingNotification.length; i++) {
            if (checkShippingNotification[i].checked) {
                keysShippingNotification.push(checkShippingNotification[i].value);
            }
        }
        //出荷通知が未選択の場合はエラー
        if (keysShippingNotification.length == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SHIPPING_NOTIFICATION)")
            return false;
        }

        //呼び出し元が先月・当月・次月ボタンの場合は表示開始日を変更する
        switch (element.id) {
            case "btn-last-month":
                //先月
                //表示開始日を1か月前に戻して検索する
                var dt = new Date(dspStartDate);
                dt.setMonth(dt.getMonth() - 1);
                //フォーマット
                var y = dt.getFullYear();
                var m = ('00' + (dt.getMonth()+1)).slice(-2);
                var d = ('00' + dt.getDate()).slice(-2);
                //値設定
                document.getElementById("SearchDspStartDate").value = y + "-" + m + "-" + d;
                break;

            case "btn-this-month":
                //当月
                //表示開始日の月をシステム日付の月にセットして検索する
                var splits = dspStartDate.split("-");
                var today = new Date();
                var newday = today.getFullYear() + "-" + ('00' + (today.getMonth()+1)).slice(-2) + "-" + splits[2];
                document.getElementById("SearchDspStartDate").value = newday;
                break;

            case "btn-next-month":
                //次月
                //表示開始日を1か月先に進めて検索する
                var dt = new Date(dspStartDate);
                dt.setMonth(dt.getMonth() + 1);
                //フォーマット
                var y = dt.getFullYear();
                var m = ('00' + (dt.getMonth()+1)).slice(-2);
                var d = ('00' + dt.getDate()).slice(-2);
                //値設定
                document.getElementById("SearchDspStartDate").value = y + "-" + m + "-" + d;
                break;
        }

        //ページ番号のリセット
        ResetPageNum();

        return true;
    }

    ////クリアボタンクリック処理
    //function btnClear_onclick() {
    //    //検索条件を初期化
    //    document.getElementById("SearchCode").value = "";
    //    document.getElementById("SearchName").value = "";
    //    document.getElementById("SearchBusho").value = "";
    //    document.getElementById("SearchKengen").value = "";
    //    document.getElementById("CsvPath").value = "";
    //    document.getElementById("PageNum").value = 1;

    //    //ページ番号のリセット
    //    ResetPageNum();

    //    return true;
    //}

    //予約申請ボタンクリック処理
    function btnCreateLending_onclick() {
        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        //カゴの選択件数が0件の場合はエラー
        var selectNumber = document.getElementById("cart-select-number").innerHTML;
        if (selectNumber == 0) {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_ASSET_NOT_SELECTED)")
            return;
        }

        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingEntry_Create");
        $('#form').submit();
    }

    //メンテナンス登録ボタンクリック処理
    function btnCreateMainte_onclick() {
        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        //テーブルの選択行を取得する。
        var text = [];
        var grid = document.getElementById("LendingStatusListGrids");
        var checkBoxes = grid.querySelectorAll('input[type="checkbox"]');
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (text.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        //キー値設定
        document.getElementById("SelectKeys").value = text[0];
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingMainteEntry_Create");
        $('#form').submit();
    }

    //〇〇登録ボタンクリック処理
    function btnBleRegist_onclick() {
        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        //テーブルの選択行を取得する。
        var text = [];
        var grid = document.getElementById("LendingStatusListGrids");
        var checkBoxes = grid.querySelectorAll('input[type="checkbox"]');
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (text.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        //キー値設定
        document.getElementById("SelectKeys").value = text[0];
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingBlaRegist_Create");
        $('#form').submit();
    }

    //予備登録１ボタンクリック処理
    function btnPreRegist1_onclick() {
        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        //テーブルの選択行を取得する。
        var text = [];
        var grid = document.getElementById("LendingStatusListGrids");
        var checkBoxes = grid.querySelectorAll('input[type="checkbox"]');
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (text.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        //キー値設定
        document.getElementById("SelectKeys").value = text[0];
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingPreRegist1_Create");
        $('#form').submit();
    }

    //予備登録２ボタンクリック処理
    function btnPreRegist2_onclick() {
        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        //テーブルの選択行を取得する。
        var text = [];
        var grid = document.getElementById("LendingStatusListGrids");
        var checkBoxes = grid.querySelectorAll('input[type="checkbox"]');
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (text.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        //キー値設定
        document.getElementById("SelectKeys").value = text[0];
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingPreRegist2_Create");
        $('#form').submit();
    }


    //全選択ボタンクリック処理
    function btnSelectAll_onclick() {
        var chkFlag = false;
        var btn = document.getElementById("btnSelectAll");
        if (btn.value == "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)") {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_OFF)";
            chkFlag = true;
        } else {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
            chkFlag = false;
        }

        var lst = document.getElementById("search-mng-loc");
        var checkBoxes = lst.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            checkBoxes[i].checked = chkFlag;
        }

        //非同期で現在のチェック状態をセッションに保存する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingStatus")/SelectAll',
            data: { blnSelectAll: chkFlag },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //カゴ追加ボタンクリック処理
    function btnCartAdd_onclick() {
        //テーブルの選択行を取得する。
        var text = [];
        var grid = document.getElementById("LendingStatusListGrids");
        var checkBoxes = grid.querySelectorAll('input[type="checkbox"]');
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        }

        //スラッシュ区切りの文字列を設定
        document.getElementById("SelectKeys").value = text.join("/");

        //カゴに追加処理(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingStatus")/AddCartInfo',
            dataType: 'json',
            cache: false,
            data: { SelectKeys: document.getElementById("SelectKeys").value },
            success: function (data, dataType) {
                if (data == null) {
                    return;
                } else {
                    //カゴに追加済の資産が選択された場合はエラー
                    if (data == "error-asset-no-already-added") {
                        showErrMessage("@Message.getMessage(CommonLabelLendingStatus.MSG_ERR_ASSET_NO_ALREADY_ADDED)")
                        return;
                    }
                    //異なる管理ロケーションが選択された場合はエラー
                    if (data == "error-mngloc") {
                        showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_DIFFERENT_MNG_LOC)")
                        return;
                    }
                    //カゴに追加できる最大件数を超える場合はエラー
                    if (data == "error-cart-over-number") {
                        showErrMessage("@String.Format(Message.getMessage(CommonLabel.MSG_ERR_CART_OVER_NUMBER), ViewBag.LendingNumber)")
                        return;
                    }
                }
                //表示
                document.getElementById("cart-select-number").innerHTML = data;
                document.getElementById("cart-no-data").innerHTML = "";

                //一覧のチェック状態をクリアする
                for (var i = 0; i < checkBoxes.length; i++) {
                    checkBoxes[i].checked = false;
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //カゴクリアボタンクリック処理
    function btnCartClear_onclick() {
        //カゴクリア処理(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingStatus")/ClearCartInfo',
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                //表示
                document.getElementById("cart-select-number").innerHTML = data;
                document.getElementById("cart-no-data").innerHTML = "@Message.getMessage(CommonLabelLendingStatus.MSG_ERR_CART_NO_DATA)";
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        document.getElementById("PageNum").value = element.name;
        $('#form').submit();
    }

    //変更ボタンクリック処理(予約モーダル)
    function btnEditModal_onclick(){
        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        let mainteId = $('#MainteIdKey').val();
        if (mainteId == '') {
            $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingEntry_Edit");
            $('#form').submit();
        } else {
            let mainteStatus = $('#MainteStatus').val();
            switch (mainteStatus){
                case "@MainteKbn.Mainte":
                    $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingMainteEntry_Edit");
                    $('#form').submit();
                    break;
                case "@MainteKbn.BlaRegist":
                    $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingBlaRegist_Edit");
                    $('#form').submit();
                    break;
                case "@MainteKbn.PreRegist1":
                    $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingPreRegist1_Edit");
                    $('#form').submit();
                    break;
                case "@MainteKbn.PreRegist2":
                    $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingPreRegist2_Edit");
                    $('#form').submit();
                    break;
            }
        }
    }

    //照会ボタンクリック処理(予約モーダル)
    function btnDispModal_onclick(){
        //管理ロケーションチェックボックスの選択状態を設定する
        setting_checkMngLocList();

        let mainteId = $('#MainteIdKey').val();
        if (mainteId == '') {
            $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingEntry_Disp");
            $('#form').submit();
        } else {
            let mainteStatus = $('#MainteStatus').val();
            switch (mainteStatus){
                case "@MainteKbn.Mainte":
                    $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingMainteEntry_Disp");
                    $('#form').submit();
                    break;
                case "@MainteKbn.BlaRegist":
                    $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingBlaRegist_Disp");
                    $('#form').submit();
                    break;
                case "@MainteKbn.PreRegist1":
                    $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingPreRegist1_Disp");
                    $('#form').submit();
                    break;
                case "@MainteKbn.PreRegist2":
                    $('#form').attr("action", "@Url.Content("~/LendingStatus")/ShowLendingPreRegist2_Disp");
                    $('#form').submit();
                    break;
            }

        }
    }

    //大分類選択
    function ChangeCategoryL(catL) {
        // 中分類をクリア
        document.getElementById("SearchCatM").value = "";
        $("#SearchCatM").attr("disabled", "disabled");
        // 小分類をクリア
        document.getElementById("SearchCatS").value = "";
        $("#SearchCatS").attr("disabled", "disabled");
        // 選択されていない場合はここで終了
        if (!catL.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingStatus")/ChangeCategory',
            data: { categoryLevel: "@CAT_DIV.CategoryM", selectLCode: catL.value, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchCatM = ""
                    SearchCatM = document.getElementById("SearchCatM");
                    SearchCatM.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchCatM.appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].CAT_CD;
                        op.text = data.Data[i].CAT_NAME;
                        SearchCatM.appendChild(op);
                    }
                }
                // 中分類データ取得
                $("#SearchCatM").removeAttr("disabled");
                SearchCatM.value = data.catMDropdownData;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //中分類選択
    function ChangeCategoryM(catM) {
        var lCode = document.getElementById("SearchCatL").value;
        // 小分類をクリア
        document.getElementById("SearchCatS").value = "";
        $("#SearchCatS").attr("disabled", "disabled");
        // 選択されていない場合はここで終了
        if (!catM.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/LendingStatus")/ChangeCategory',
            data: { categoryLevel: "@CAT_DIV.CategoryS", selectLCode: lCode, selectMCode: catM.value },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchCatS = document.getElementById("SearchCatS");
                    SearchCatS.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    document.getElementById("SearchCatS").appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].CAT_CD;
                        op.text = data.Data[i].CAT_NAME;
                        document.getElementById("SearchCatS").appendChild(op);
                    }
                }
                // 小分類データ取得
                $("#SearchCatS").removeAttr("disabled");
                document.getElementById("SearchCatS").value = data.catSDropdownData;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //管理ロケーションチェックボックスの選択状態を設定する
    function setting_checkMngLocList() {
        //管理ロケーションのチェック状態
        var keys = [];
        var lst = document.getElementById("search-mng-loc");
        var checkBoxes = lst.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                keys.push(checkBoxes[i].value);
            }
        }

        // カンマ区切りの文字列を設定
        document.getElementById("SelectMngLocKeys").value = keys.join(",");

        return keys;
    }


    $.ajax({
        type: "POST",
        url: '@Url.Content("~/LendingStatus")/GetGridData',
        dataType: "json",
        contentType: "application/Json",
        success: function (ret) {
            console.log("success");
            console.log(ret);

            var columns = [
                { id: "@ColKey.LendingStatus.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.CHECK_SELECT", 40), sortOrder: -2, formatter: CheckboxFormatter, cssClass: "first-column" },
                { id: "@ColKey.LendingStatus.CAT_L", name: "@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)", field: "CAT_L", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.CAT_L", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag" },
                { id: "@ColKey.LendingStatus.CAT_M", name: "@Message.getMessage(CommonLabel.LABEL_CAT_M_CD_NAME)", field: "CAT_M", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.CAT_M", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispFlag" },
                { id: "@ColKey.LendingStatus.CAT_S", name: "@Message.getMessage(CommonLabel.LABEL_CAT_S_CD_NAME)", field: "CAT_S", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.CAT_S", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispFlag" },

                { id: "@ColKey.LendingStatus.MANAGE_LOC", name: "@Message.getMessage(CommonLabel.LABEL_MNG_LOC)", field: "MANAGE_LOC", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.MANAGE_LOC", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag"},
                { id: "@ColKey.LendingStatus.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.ASSET_NO", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag" },
                { id: "@ColKey.LendingStatus.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)", field: "ASSET_NAME", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.ASSET_NAME", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag" },
                { id: "@ColKey.LendingStatus.LOT_NO", name: "@Message.getMessage(CommonLabel.LABEL_MODEL_NO)", field: "LOT_NO", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.LOT_NO", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispFlag" },
                { id: "@ColKey.LendingStatus.ASSET_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_STATUS)", field: "ASSET_STATUS", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.ASSET_STATUS", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_STATUS").DispFlag", formatter: AssetStatusFormatter},
            ];

            leftPaneHeaderNum = 1;
            leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag ? 1 : 0;
            leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("CAT_M_CD").DispFlag ? 1 : 0;
            leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("CAT_S_CD").DispFlag ? 1 : 0;
            leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag ? 1 : 0;
            leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag ? 1 : 0;
            leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag ? 1 : 0;
            leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("MODEL_NO").DispFlag ? 1 : 0;
            leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("ASSET_STATUS").DispFlag ? 1 : 0;

            //照合機能OFFの場合は表示
            if ("@ViewBag.MatchingFuncFlg" != "Y") {
                columns.push({ id: "@ColKey.LendingStatus.SHIPPING_NOTIFY", name: "@Message.getMessage(CommonLabel.LABEL_SHIPPING_NOTIFICATION)", field: "SHIPPING_NOTIFY", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.SHIPPING_NOTIFY", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("SHIPPING_NOTIFICATION").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SHIPPING_NOTIFICATION").DispFlag", formatter: ShippingNotifyFormatter });
                leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("SHIPPING_NOTIFICATION").DispFlag ? 1 : 0;
            }

            //予約機能OFFの場合は表示
            if ("@ViewBag.ReserveFuncFlg" != "Y") {
                columns.push({ id: "@ColKey.LendingStatus.PRODUCT_NO", name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)", field: "PRODUCT_NO", width: getColumnWidth("@DispId.LENDING_STATUS", "@ColKey.LendingStatus.PRODUCT_NO", 190), sortOrder: "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispFlag" });
                leftPaneHeaderNum += @Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispFlag ? 1 : 0;
            }

            let startDate = new Date('@Model.FirstOrDefault.SearchDspStartDate');
            let endDate = new Date('@Model.FirstOrDefault.SearchDspStartDate');

            endDate.setMonth(startDate.getMonth() + Number('@Model.FirstOrDefault.DspMonthCount'));
            endDate.setDate(endDate.getDate() - 1);
            var headerIdList = [];
            var monthDayNum = [];

            let exitStartDate = startDate.getDate();
            startDate.setDate(1);
            for (var i = 0; i <= '@Model.FirstOrDefault.DspMonthCount'; i++) {
                let date = startDate;

                var fromDay = 1;
                var toDay = getDaysInMonth(date.getFullYear(), date.getMonth());

                if (i == 0) {
                    fromDay = exitStartDate;
                }

                var month = date.getMonth();
                if (endDate.getFullYear() == date.getFullYear() && endDate.getMonth() == date.getMonth()) {
                    toDay = endDate.getDate();
                }

                monthDayNum.push(Number(month + 1) + ":" + Number(toDay - fromDay + 1));

                var header = makeMonthHeader(date.getFullYear() ,month + 1, fromDay, toDay);
                headerIdList.push(date.getFullYear() + "year" + Number(month + 1) + "month");
                columns.push(header);


                date.setMonth(date.getMonth() + 1);
            }


            var options = {
                enableCellNavigation: true,
                enableColumnReorder: false,
                multiColumnSort: true,
                asyncEditorLoading: false,
                enableAsyncPostRender: true,
                asyncPostRenderDelay: 0,
                enableAsyncPostRenderCleanup: false,
                asyncPostRenderCleanupDelay: 40,
            };

            gridData = [];
            $.each(ret, function (index, value) {
                console.log(value);

                var dispData = {
                    CAT_L: value.CAT_L_CD + " " + value.CAT_L_NAME == null ? "" : value.CAT_L_NAME,
                    CAT_M: value.CAT_M_CD + " " + value.CAT_M_NAME == null ? "" : value.CAT_M_NAME,
                    CAT_S: value.CAT_S_CD + " " + value.CAT_S_NAME == null ? "" : value.CAT_S_NAME,
                    MANAGE_LOC: value.MNG_LOC_L_CD + " " + value.MNG_LOC_L_NAME,
                    MNG_LOC_L_CD: value.MNG_LOC_L_CD,
                    ASSET_NO: value.ASSET_NO,
                    ASSET_NAME: value.ASSET_NAME,
                    LOT_NO: value.MODEL_NO,
                    ASSET_STATUS: value.ASSET_STATUS,
                    SHIPPING_NOTIFY: value.DEST_NO,
                    PRODUCT_NO: value.PRODUCT_NO,
                    CALENDER: value.CAL_STR,
                    UPD_FLG: value.UPD_FLG
            }

                gridData.push(dispData);
            })

            // 固定列を設定
            let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
            if (fixCol > 0) {
                fixCol = fixCol + 1       //選択列と変更列を加算
            }

            // Grid初期化
            //grid = new Slick.Grid('#testGrids', gridData, columns, options);
            grid = initializeGrid("LendingStatusListGrids", gridData, columns, options, "@DispId.LENDING_STATUS", fixCol);

            grid.registerPlugin(new Slick.Plugins.ColGroup());
            grid.registerPlugin(new CalenderColumn());
            grid.setSelectionModel(new Slick.RowSelectionModel({ selectActiveRow:  false }));

            var headerGroup = document.getElementsByClassName('slick-header-columns-group');
            for (var header of headerGroup) {
                for (var headerId of headerIdList) {
                    if (header.id.indexOf(headerId) != -1) {

                        var str = headerId.substring(0, headerId.length - 5);
                        var [year, month] = str.split('year');
                        strDate = year + String(month).padStart(2, '0');
                        var src = "@Url.Content("~/Images")/minus.png";
                        var isPlus = false;
                        var arrayObj = JSON.parse('@Html.Raw(Json.Encode(Model.FirstOrDefault.isClosedColumns))');
                        for (var j = 0; j < arrayObj.length; j++) {
                            if (arrayObj[j]['columnName'] == strDate) {
                                if (toBoolean(document.getElementById("isClosedColumns_" + j + "__isClosed").value)) {
                                    src = "@Url.Content("~/Images")/plus.png";
                                    isPlus = true;
                                }
                                break;
                            }
                        }

                        var newImage = $('<img>', {
                            src: src,
                            width: '14px',
                            'data-value': headerId,
                            click: plusMinusBtn
                        });

                        $("#" + header.id + " span").append(newImage);

                        var dayNum = 0;
                        for (var str of monthDayNum) {
                            var [a, b] = str.split(":");
                            if (a == month) {
                                dayNum = b;
                                break;
                            }
                        }

                        console.log(month + "," + dayNum + "," + isPlus);

                        var width = isPlus ? dayNum * 15 - 10: dayNum * 25 - 10;
                        header.style.width = width + "px";
                    }
                }
            }

            $('.slick-cell').click(function () {

                var inputTag = this.querySelector("input");
                if (inputTag == null || inputTag == undefined) {
                    return;
                }
                if (inputTag.type != 'hidden') {
                    return;
                }

                var cellId = inputTag.id;
                var [row, col] = cellId.split('A');
                cellId = "#" + cellId;

                //予約情報表示処理(Ajax非同期通信)
                $.ajax({
                    type: 'POST',
                    url: '@Url.Content("~/LendingStatus")/ShowReserveInfo',
                    dataType: 'json',
                    cache: false,
                    data: { SelectKeys: $(cellId).val() },
                    success: function (data, dataType) {
                        if (data == null) {
                            return;
                        } else if (data.length == 0) {
                            return;
                        }

                        //画面に値を表示
                        switch (data[0].RSVFLG) {
                            case '@AssetReserveInfo.None':
                                $('#modal-reserve-status').text('@Message.getMessage(CommonLabel.VALUE_NOT_RENTED)');
                                break;
                            case '@AssetReserveInfo.Reserve':
                                $('#modal-reserve-status').text('@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RESERVE)');
                                break;
                            case '@AssetReserveInfo.Lease':
                                $('#modal-reserve-status').text('@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)');
                                break;
                            case '@AssetReserveInfo.Henkyaku':
                                $('#modal-reserve-status').text('@Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)');
                                break;
                            case '@AssetReserveInfo.Mainte':
                                $('#modal-reserve-status').text('@Message.getMessage(CommonLabel.VALUE_MAINTE)');
                                break;
                            case '@AssetReserveInfo.blaRegist':
                                $('#modal-reserve-status').text('@Message.getMessage(CommonLabel.VALUE_BLAREGIST)');
                                break;
                            case '@AssetReserveInfo.preRegist1':
                                $('#modal-reserve-status').text('@Message.getMessage(CommonLabel.VALUE_PREREGIST1)');
                                break;
                            case '@AssetReserveInfo.preRegist2':
                                $('#modal-reserve-status').text('@Message.getMessage(CommonLabel.VALUE_PREREGIST2)');
                                break;
                        }

                        $('#modal-reserve-charge-emp').text(data[0].CHARGE_EMP_NAME);
                        $('#modal-reserve-req-emp').text(data[0].REQ_EMP_NAME);
                        if (data[0].RESERVE_ID == null) {
                            $('#modal-reserve-no').text("");
                        } else {
                            $('#modal-reserve-no').text(data[0].RESERVE_ID);
                        }
                        $('#modal-const-no').text(data[0].CONST_NO);
                        $('#modal-const-dt').text(data[0].CONST_DT);
                        $('#modal-const-contents').text(data[0].CONST_CONTENTS);
                        $('#modal-const-employee').text(data[0].CONST_EMPLOYEE_NAME);

                        //予約情報引き渡し用隠し項目に値を設定
                        if (data[0].RESERVE_ID == null) {
                            $('#ReserveIdKey').val("");
                        } else {
                            $('#ReserveIdKey').val(data[0].RESERVE_ID);
                        }
                        if (data[0].MAINTE_ID == null) {
                            $('#MainteIdKey').val("");
                        } else {
                            $('#MainteIdKey').val(data[0].MAINTE_ID);
                            $('#MainteStatus').val(data[0].RSVFLG);
                        }

                        //ボタン表示設定
                        //更新権限があり、かつ資産が未貸出または貸出中の場合は変更ボタンのみ表示、以外の場合は照会ボタンのみ表示
                        //照合機能ONの時はどちらも非表示
                        let cellValue = $(cellId).val().split(",");
                        // 2021/11/30 upd nv Inoue
                        //if (cellValue[2] == '@UpdFlg.Yes' && (data[0].RESERVE_STATUS == '@ReserveStatus.Mi' || data[0].RESERVE_STATUS == '@ReserveStatus.Lending')) {
                        if ("@ViewBag.MatchingFuncFlg" == "Y") {
                            if (data[0].MAINTE_ID == null || data[0].MAINTE_ID == "") {
                                $('#modal-reserve-edit').hide();
                            } else {
                                $('#modal-reserve-edit').show();
                            }
                            $('#modal-reserve-show').hide();
                        } else if (data[0].IS_UPD == true && (data[0].RESERVE_STATUS == '@ReserveStatus.Mi' || data[0].RESERVE_STATUS == '@ReserveStatus.Lending')) {
                            $('#modal-reserve-edit').show();
                            $('#modal-reserve-show').hide();
                        } else {
                            $('#modal-reserve-edit').hide();
                            $('#modal-reserve-show').show();
                        }

                        //モーダルウインドウを開く
                        ShowReserveModalWindow();
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        this;
                    }
                });
            });

            if (@Model.FirstOrDefault.TotalCount == 0) {
                document.getElementsByClassName('slick-pane-top')[0].remove()
            }
        },
        error: function (e) {
            console.log("error")
            console.log(e);
        }
    });

    function plusMinusBtn() {

        var src = this.getAttribute('src');
        var isPlus = src.indexOf('plus') != -1;

        var str = this.getAttribute('data-value');
        str = str.substring(0, str.length - 5);
        var [year, month] = str.split('year');
        strDate = year + String(month).padStart(2, '0');

        var arrayObj = JSON.parse('@Html.Raw(Json.Encode(Model.FirstOrDefault.isClosedColumns))');
        for (var j = 0; j < arrayObj.length; j++) {
            if (arrayObj[j]['columnName'] == strDate) {
                document.getElementById("isClosedColumns_" + j + "__isClosed").value = !isPlus;
                break;
            }
        }

        btnSearch_onclick(this);

        $('#form').submit();
    }

    function getDaysInMonth(year, month) {
        // monthは0から始まるため、実際の月から1を引く
        var lastDay = new Date(year, month + 1, 0).getDate();
        return lastDay;
    }

    //資産状態　formatter
    function AssetStatusFormatter(row, cell, value, columnDef, dataContext) {
        if (dataContext.ASSET_STATUS == null) {
            return "";
        }
        switch (String(dataContext.ASSET_STATUS)) {
            case '@ASSET_STATUS.Preparation': return '@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_JUNBI)';
            case '@ASSET_STATUS.Storage': return '@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_HOKAN)';
            case '@ASSET_STATUS.Lending': return '@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_LEASE)';
            case '@ASSET_STATUS.Removed': return '@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_RETIRE)';
        }

        return "";
    }

    //出荷通知formatter
    function ShippingNotifyFormatter(row, cell, value, columnDef, dataContext) {

        let ret = "";
        if (dataContext.SHIPPING_NOTIFY == null || dataContext == undefined) {
            ret = "-";
        }
        else if (dataContext.SHIPPING_NOTIFY == "") {
            ret = '@Message.getMessage(CommonLabel.VALUE_MI)';
        }
        else {
            ret = '@Message.getMessage(CommonLabel.VALUE_SUMI)';
        }

        return ret;
    }

    //選択用formatter
    function CheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let isUpd = dataContext.UPD_FLG == 'Y';
        let disableStr = isUpd ? "" : " disabled ";
        let valueStr = dataContext.ASSET_NO + "," + dataContext.MNG_LOC_L_CD;

        return '<div style="text-align:center; height:100%;">' +
            '<input type="checkbox" class="slick-checkbox" name="checkEdit" ' + disableStr + ' value="' + valueStr + '" />' +
            '</div >';
    }


    function makeMonthHeader(year, month, fromDay, toDay) {
        var monthCell = {
            id: year + "year" + month + "month",
            name: month + "月",
            field: month + "month",
            children:  [],
            onclick: btnSearch_onclick,
        };

        let arrayObj = JSON.parse('@Html.Raw(Json.Encode(Model.FirstOrDefault.isClosedColumns))');

        var isClose = false;
        let monthStr = String(month).padStart(2, '0');
        for (var i = 0; i < arrayObj.length; i++) {
            if (arrayObj[i]['columnName'] == year + monthStr) {
                isClose = Boolean(arrayObj[i]['isClosed']);
                break;
            }
        }

        let notWorkDayStr = '@Model.FirstOrDefault.NotWorkDay';

        for (var i = fromDay; i <= toDay; i++) {
            var dateStr = year + monthStr + String(i).padStart(2, '0');;
            var headerCss = "dateHeader";
            if (notWorkDayStr != null && notWorkDayStr.indexOf(dateStr) != -1){
                headerCss += " holidayHeader";
            }
            var width = isClose ? 15 : 25;
            var calenderColumn = new CalenderColumn({
                name: i,
                headerCssClass: headerCss,
                width: width
            });
            monthCell.children.push(calenderColumn.getColumnDefinition(i));
        }
        return monthCell;
    }

    function CalenderColumn(options) {
        var _grid;
        var _self = this;
        var _handler = new Slick.EventHandler();
        //var _selectedRowsLookup = {};
        var _defaults = {
            columnId:   "_calender_cell",
            name: "name",
            minWidth: 15,
        };

        var _options = $.extend(true, {}, _defaults, options);

        function init(grid) {
            _grid = grid;
        }

        function destroy() {
            _handler.unsubscribeAll();
        }

        function getColumnDefinition(status) {
            return {
                id: _options.columnId,
                toolTip: _options.toolTip,
                field:  "cell",
                minWidth: 15,
                width: _options.width,
                name: _options.name,
                resizable:   false,
                sortable: false,
                cssClass: _options.cssClass,
                headerCssClass: _options.headerCssClass,
                formatter: calenderCellSelectionFormatter,
                asyncPostRender: paintColor
            };
        }

        function paintColor(cellNode, row, dataContext, colDef) {
            var parentCell = cellNode[0];
            if (parentCell == null || parentCell == undefined) {
                return;
            }
            var child = parentCell.firstChild;
            if (child == null) { return; }
            var dataClassValue = child.getAttribute('data-class');
            $(cellNode).css('background-color',dataClassValue);
        }

        function calenderCellSelectionFormatter(row, cell, value, columnDef, dataContext) {
            if (dataContext.CALENDER == null) {
                return;
            }
            var str = dataContext.CALENDER.substring(cell - leftPaneHeaderNum, cell - (leftPaneHeaderNum - 1));
            var style = "";
            switch (str) {
                case '@AssetReserveInfo.None': style = '@Misc.CalendarColor.None'; break;
                case '@AssetReserveInfo.Holiday': style = '@Misc.CalendarColor.Holiday'; break;
                case '@AssetReserveInfo.Reserve': style = '@Misc.CalendarColor.Reserve'; break;
                case '@AssetReserveInfo.Lease': style = '@Misc.CalendarColor.Lease'; break;
                case '@AssetReserveInfo.Henkyaku': style = '@Misc.CalendarColor.Henkyaku'; break;

                @*case '@AssetReserveInfo.Mainte': style = '@Misc.CalendarColor.Mainte'; break;*@
                case '@AssetReserveInfo.Mainte': style = '@ViewBag.mainteStatusMColor'; break;
                case '@AssetReserveInfo.blaRegist': style = '@ViewBag.mainteStatusOColor'; break;
                case '@AssetReserveInfo.preRegist1': style = '@ViewBag.mainteStatusPColor'; break;
                case '@AssetReserveInfo.preRegist2': style = '@ViewBag.mainteStatusQColor'; break;
            }

            var id = row + "A" + cell;
            let date = new Date('@Model.FirstOrDefault.SearchDspStartDate');
            date.setDate(date.getDate() + Number(cell) - leftPaneHeaderNum);

            var yyyy = date.getFullYear().toString();
            var MM = (date.getMonth() + 1).toString().padStart(2, '0');
            var dd = date.getDate().toString().padStart(2, '0');

            var value = dataContext.ASSET_NO + "," + yyyy + MM + dd + "," + dataContext.UPD_FLG

            return '<div data-class="' + style + '"><input type="hidden" id="' + id + '" value="' + value +'"></div>';

        }

        $.extend(this, {
            "init": init,
            "destroy": destroy,
            "getColumnDefinition": getColumnDefinition
        });

    }

    function toBoolean(booleanStr) {
        if (booleanStr == null) {
            return false;
        }
        return booleanStr.toLowerCase() == "true";
    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

    var test;
    var leftPaneHeaderNum; //カレンダーより左のヘッダー行数


</script>

<style>
    .slick-header-column {
        background-color: #6ab3b3;
    }

        .slick-header-column: hover {
            background-color: #6ab3b3;
        }

        .slick-header-column span {
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100%;
            color: #fff;
            font-weight: 600;
        }

    .cssClass {
        background-color: #4cff00;
    }

    .holidayHeader {
        background-color: #ffe3e3;
    }

        .holidayHeader : hover {
            background-color: #ffe3e3;
        }

        .holidayHeader span {
            color: red;
        }

    .dateHeader {
        height: 21px !important;
    }

    .closeCell {
        width: 5px !important;
    }
</style>

<div id="lending-status">
    @Using (Html.BeginForm("Index", "LendingStatus", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20" style="width:1790px">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    @*2022/7/25 sv sato upd start*@
                    @*@If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                            @<input class="defButton size-x-large" type="button" style="margin-right:30px;" value="@Message.getMessage(CommonLabel.BUTTON_RESERVE)" onclick="btnCreateLending_onclick()">
                        End If*@
                    @*<div id="cart-no-data" class="view-area-label" style="color:red;"></div>*@
                    @If ViewBag.CanDspOptionData2 AndAlso Not ViewBag.MatchingFuncFlg = "Y" Then
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                    @<input class="defButton size-x-large" type="button" style="margin-right:30px;" value="@Message.getMessage(CommonLabel.BUTTON_RESERVE)" onclick="btnCreateLending_onclick()">
                    End If
                    @<div id="cart-no-data" Class="view-area-label" style="color:red;"></div>
                    End If
                    @*2022/7/25 sv sato upd end*@
                </div>
                <div Class="view-area-button-right">
                    @If ViewBag.CanDspMainteEntry IsNot Nothing AndAlso Boolean.Parse(ViewBag.CanDspMainteEntry) Then
                        '2022/7/25 sv sato upd start
                    @*@<input class="defButton size-x-large" style="margin-right:220px;" type="button" value="@Message.getMessage(CommonLabel.BUTTON_MAINTE_ENTRY)" onclick="btnCreateMainte_onclick()">*@
                    @If ViewBag.CanDspOptionData2 Then
                    @If ViewBag.mainteStatusMDisp = "Y" Then
                    @<input class="defButton size-x-large" style="margin-right:5px;" type="button" value="@Message.getMessage(CommonLabel.BUTTON_MAINTE_ENTRY)" onclick="btnCreateMainte_onclick()">
                    End If
                    End If
                        '2022/7/25 sv sato upd end
                    End If
                    @If ViewBag.mainteStatusODisp = "Y" Then
                    @<input class="defButton size-x-large" style="margin-right:5px;" type="button" value="@Message.getMessage(CommonLabel.BUTTON_BLA_REGIST)" onclick="btnBleRegist_onclick()">
                    End If
                    @If ViewBag.mainteStatusPDisp = "Y" Then
                    @<input class="defButton size-x-large" style="margin-right:5px;" type="button" value="@Message.getMessage(CommonLabel.BUTTON_PRE_REGIST1)" onclick="btnPreRegist1_onclick()">
                    End If
                    @If ViewBag.mainteStatusQDisp = "Y" Then
                    @<input class="defButton size-x-large" style="margin-right:5px;" type="button" value="@Message.getMessage(CommonLabel.BUTTON_PRE_REGIST2)" onclick="btnPreRegist2_onclick()">
                    End If


                    <input Class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" style="margin-left: 220px; width: 120px;" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">

                    @*2022/7/25 sv sato add start*@
                    @If ViewBag.CanDspCsvImport = True AndAlso Not ViewBag.MatchingFuncFlg = "Y" Then
                    @<input Class="defButton csv-LendImport-btn submit-button" type="button" name="CsvLendImport" style="width:120px;" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">
                    Else
                    @<input Class="defButton csv-LendImport-btn submit-button" type="button" name="CsvLendImport" style="width:120px; visibility:hidden" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">
                    End If
                    @*2022/7/25 sv sato add end*@
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
                            @* カテゴリー 大区分*@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeCategoryL(this)", .class = "cond-control"})
                                </div>
                            </div>
                            @* カテゴリー中分類 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag = "1" AndAlso Model.FirstOrDefault.GridInfo("CAT_M_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_M)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatM, DirectCast(ViewBag.modelCatM, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeCategoryM(this)", .class = "cond-control"})
                                </div>
                            </div>
                            @* カテゴリー小分類 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag = "1" AndAlso Model.FirstOrDefault.GridInfo("CAT_M_CD").DispFlag = "1" AndAlso Model.FirstOrDefault.GridInfo("CAT_S_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_S)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatS, DirectCast(ViewBag.modelCatS, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeCategoryS(this)", .class = "cond-control"})
                                </div>
                            </div>
                            @* 資産番号 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetNo, New With {.class = "cond-control", .maxlength = 24, .oninput = "editAssetNo(this)"})
                                </div>
                            </div>
                            @* 資産名称 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>
                            @* 担当者 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_EMP)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchEmpName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>
                            @If Not ViewBag.MatchingFuncFlg! = "Y" Then
                            @* 申請者 *@
                            @<div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_REQ_EMP)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchReqEmpName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>
                            End If
                            @If Not ViewBag.ReserveFuncFlg = "Y" Then
                            @* 工事番号 *@
                            @<div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CONST_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchConstNo, New With {.class = "cond-control", .maxlength = 20})
                                </div>
                            </div>
                            @* 品番 *@
                            @<div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchProductNo, New With {.class = "cond-control", .maxlength = 30})
                                </div>
                            </div>
                            End If
                            @* 表示開始日 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_DSP_START_DT)</div>
                                <div class="grid-condition-col">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchDspStartDate, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                </div>
                            </div>
                            @* 貸出状態 *@
                            <div style="display:@(If(Model.FirstOrDefault.GridInfo("ASSET_STATUS").DispFlag = "1", "display", "None"))">
                                <div class="col-sm-7" style="max-width:950px;">
                                    <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)</div>
                                    <div id="search-lease-status" class="grid-condition-col">
                                        <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_NoReserve) @Message.getMessage(CommonLabel.VALUE_NO_RESERVE)</div>
                                        <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_NotRented) @Message.getMessage(CommonLabel.VALUE_NOT_RENTED)</div>
                                        <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_Lending) @Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)</div>
                                        <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_LendingDelay) @Message.getMessage(CommonLabel.VALUE_LENDING_DELAY)</div>
                                        <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_Returned) @Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)</div>
                                        @If ViewBag.mainteStatusMDisp = "Y" Then
                                        @<div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_Mainte) @Message.getMessage(CommonLabel.VALUE_MAINTE)</div>
                                        End If
                                        @If ViewBag.mainteStatusODisp = "Y" Then
                                        @<div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_BlaRegist) @Message.getMessage(CommonLabel.VALUE_BLAREGIST)</div>
                                        End If
                                        @If ViewBag.mainteStatusPDisp = "Y" Then
                                        @<div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_PreRegist1) @Message.getMessage(CommonLabel.VALUE_PREREGIST1)</div>
                                        End If
                                        @If ViewBag.mainteStatusQDisp = "Y" Then
                                        @<div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLeaseStatus_PreRegist2) @Message.getMessage(CommonLabel.VALUE_PREREGIST2)</div>
                                        End If
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="row" id="item-conditions">
                        @* 管理ロケーション *@
                        <div class="col-md-12">
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_MNG_LOC)</div>
                                <td class="condition-col" style="width:300px;">
                                    <div id="search-mng-loc" class="box-srcollbar">
                                        @Code
                                            If Model.FirstOrDefault.mngLocCheckBoxListData IsNot Nothing Then
                                                Dim selectMngLocKeys As String() = New String() {}
                                                If Model.FirstOrDefault.SelectMngLocKeys IsNot Nothing Then
                                                    selectMngLocKeys = Model.FirstOrDefault.SelectMngLocKeys.Split(",")
                                                End If
                                                For Each item In Model.FirstOrDefault.mngLocCheckBoxListData
                                                    Dim isChecked As Boolean = False
                                                    For Each key As String In selectMngLocKeys
                                                        If item.LOC_CD = key Then
                                                            isChecked = True
                                                            Exit For
                                                        End If
                                                    Next
                                                    If isChecked Then
                                        @<input type="checkbox" name="checkMngLoc" value="@item.LOC_CD" checked />@item.LOC_NAME@<br />
                                                    Else
                                        @<input type="checkbox" name="checkMngLoc" value="@item.LOC_CD" />@item.LOC_NAME@<br />
                                                    End If
                                                Next
                                            End If
                                        End Code
                                    </div>
                                </td>
                                <div class="condition-col" style="width:150px;">
                                    @* 全選択/全解除ボタンの表記文字設定 *@
                                    @Code
                                        Dim AllSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG)
                                        Dim sDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                                        If AllSelectFlg Then
                                            sDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                                        End If
                                    End Code
                                    <input id="btnSelectAll" Class="defButton submit-button" type="button" value="@sDispStr" onclick="return btnSelectAll_onclick()" />
                                </div>
                            </div>
                        </div>
                    </div>
                    @* 詳細検索 *@
                    <div Class="detail-row"><div height="40px">@* 空白調整行 *@</div></div>
                    <div class="detail-row">
                        <div class="condition-col" style="width:150px">
                            <a href="#" id="DetailAccordion" onclick="DetailAccordion_onclick();return false;">@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)</a>
                        </div>
                    </div>
                    <table Class="detail-table">
                        <tbody class="detail-accordion" style="display:none;">

                            <tr class="row" id="item-conditions">
                                <td class="col-md-12">
                                    @* 出荷通知 *@
                                    <div class="col-sm-5" style="display:@(If(Not ViewBag.MatchingFuncFlg = "Y" AndAlso Model.FirstOrDefault.GridInfo("SHIPPING_NOTIFICATION").DispFlag = "1", "display", "None"))">
                                        <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_SHIPPING_NOTIFICATION)</div>
                                        <div id="search-shipping-notification" class="grid-condition-col">
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchShippingNotification_NotNotified) @Message.getMessage(CommonLabel.VALUE_NOT_NOTIFIED)</div>
                                            <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchShippingNotification_Notified) @Message.getMessage(CommonLabel.VALUE_NOTIFIED)</div>
                                        </div>
                                    </div>
                                    @* 並び替え *@
                                    <div class="col-sm-5" style="width:300px;">
                                        <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_SORT)</div>
                                        <div id="search-sort" class="grid-condition-col">
                                            <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchSort, LendingStatusSort.Reserve) @Message.getMessage(CommonLabel.VALUE_SORT_RESERVE)</div>
                                            <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchSort, LendingStatusSort.Mainte) @Message.getMessage(CommonLabel.VALUE_SORT_MAINTE)</div>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    @* 検索ボタン *@
                    <div Class="search-btn">
                        <input id="btn-search" class="defButton submit-button" name="actionname" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick(this)" />
                        @*<input id="search-clear" class="defButton submit-button" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_CLEAR)" onclick="return btnClear_onclick()" />*@
                    </div>
                </div>
            </div>
        </div>

        @*管理ロケーションチェックボックス選択値の引き渡し用隠し項目*@
                                        @<input id="SelectMngLocKeys" name="SelectMngLocKeys" type="hidden">
                                        @*明細データチェックボックス選択値の引き渡し用隠し項目*@
                                        @<input id="SelectKeys" name="SelectKeys" type="hidden">


                                        @<div class="lending-view-area mb10">
                                            <div id="view-area-button-parent">
                                                <div class="view-area-button-left">
                                                    @*2022/7/25 sv sato upd start*@
                                                    @*@If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                                                            @<input class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CART_ADD)" onclick="btnCartAdd_onclick()">
                                                            @<input class="defButton" type="button" style="margin-right:20px;" value="@Message.getMessage(CommonLabel.BUTTON_CART_CLEAR)" onclick="btnCartClear_onclick()">
                                                        End If*@
                                                    @*<div class="view-area-label">@Message.getMessage(CommonLabel.LABEL_SELECT_NUMBER)<a href="#" onclick="lnkShowCartInfo_onclick();return false;"><span id="cart-select-number"></span>@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)</a></div>*@
                                                    @If ViewBag.CanDspOptionData2 AndAlso Not ViewBag.MatchingFuncFlg = "Y" Then
                                                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                                                    @<input class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CART_ADD)" onclick="btnCartAdd_onclick()">
                                                    @<input class="defButton" type="button" style="margin-right:20px;" value="@Message.getMessage(CommonLabel.BUTTON_CART_CLEAR)" onclick="btnCartClear_onclick()">
                                                    End If
                                                    @<div Class="view-area-label">@Message.getMessage(CommonLabel.LABEL_SELECT_NUMBER)<a href="#" onclick="lnkShowCartInfo_onclick();return false;"><span id="cart-select-number"></span>@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)</a></div>
                                                    End If
                                                    @*2022/7/25 sv sato upd end*@
                                                </div>
                                                @**凡例表示*@
                                                @If ViewBag.LegendDispWidth <> "0px" Then
                                                @<div Class="guide-area" style="width: @ViewBag.LegendDispWidth">
                                                    @*<div class="guide-area">*@
                                                    @If ViewBag.legendReserve = "Y" Then
                                                @<div Class="guide-unit-area">
                                                    <div Class="guide-color-area" style="@CalendarColor.Reserve"></div>
                                                    @Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RESERVE)
                                                </div>
                                                    End If
                                                    @If ViewBag.legendLease = "Y" Then
                                                @<div class="guide-unit-area">
                                                    <div Class="guide-color-area" style="@CalendarColor.Lease"></div>
                                                    @Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)
                                                </div>
                                                    End If
                                                    @If ViewBag.legendMainte = "Y" Then
                                                @<div class="guide-unit-area">
                                                    @*<div class="guide-color-area" style="@CalendarColor.Mainte"></div>*@
                                                    <div Class="guide-color-area" style="@ViewBag.mainteStatusMColor"></div>
                                                    @Message.getMessage(CommonLabel.VALUE_MAINTE)
                                                </div>
                                                    End If
                                                    @If ViewBag.legendHenkyaku = "Y" Then
                                                @<div class="guide-unit-area">
                                                    <div Class="guide-color-area" style="@CalendarColor.Henkyaku"></div>
                                                    @Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)
                                                </div>
                                                    End If
                                                    @If ViewBag.legendBlaRegist = "Y" Then
                                                @<div class="guide-unit-area">
                                                    <div Class="guide-color-area" style="@ViewBag.mainteStatusOColor"></div>
                                                    @Message.getMessage(CommonLabel.VALUE_BLAREGIST)
                                                </div>
                                                    End If
                                                    @If ViewBag.legendPreRegist1 = "Y" Then
                                                @<div class="guide-unit-area">
                                                    <div Class="guide-color-area" style="@ViewBag.mainteStatusPColor"></div>
                                                    @Message.getMessage(CommonLabel.VALUE_PREREGIST1)
                                                </div>
                                                    End If
                                                    @If ViewBag.legendPreRegist2 = "Y" Then
                                                @<div class="guide-unit-area">
                                                    <div Class="guide-color-area" style="@ViewBag.mainteStatusQColor"></div>
                                                    @Message.getMessage(CommonLabel.VALUE_PREREGIST2)
                                                </div>
                                                    End If
                                                </div>
                                                End If
                                                <div class="view-area-button-right">
                                                    <input id="btn-last-month" class="defButton submit-button" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_LAST_MONTH)" onclick="return btnSearch_onclick(this)">
                                                    <input id="btn-this-month" class="defButton submit-button" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_THIS_MONTH)" onclick="return btnSearch_onclick(this)">
                                                    <input id="btn-next-month" class="defButton submit-button" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_NEXT_MONTH)" onclick="return btnSearch_onclick(this)">
                                                </div>
                                            </div>
                                        </div>
                                        @*ページャー表示*@
                                        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
                                        @<div Class="list-view">
                                            <div id="LendingStatusListGrids" style="height: @Model.FirstOrDefault.Pager.GetCurrentPageRecord(); ">
                                                <Script>

                                                </Script>
                                            </div>
                                        </div>

                                        @*カゴモーダル画面*@
                                        @<div id="modal-cart-content">
                                            <div class="popup-title-wrap">
                                                <h2 id="popup-title-cart"></h2>
                                            </div>
                                            <table id="cart-info-list" class="mb10">
                                                <tbody id="cart-info-body"></tbody>
                                            </table>
                                            <div Class="PopupButton float-right">
                                                <input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CLOSE)">
                                            </div>
                                        </div>


                                        @*予約モーダル画面*@
                                        @<div id="modal-reserve-content">
                                            <div class="mb10">
                                                <div>@Message.getMessage(CommonLabel.LABEL_STATUS) : <span id="modal-reserve-status"></span></div>
                                                <div style="display:@(If(ViewBag.MatchingFuncFlg = "Y" AndAlso Not IsNothing(ViewBag.MatchingReserveDetail) AndAlso ViewBag.MatchingReserveDetail = "1", "None", "display"))">@Message.getMessage(CommonLabel.LABEL_EMP) : <span id="modal-reserve-charge-emp"></span></div>
                                                <div>@(If((ViewBag.MatchingFuncFlg = "Y" AndAlso Not IsNothing(ViewBag.MatchingReserveDetail) AndAlso ViewBag.MatchingReserveDetail = "2") OrElse Not ViewBag.MatchingFuncFlg = "Y", Message.getMessage(CommonLabel.LABEL_REQ_EMP), Message.getMessage(CommonLabel.LABEL_RESERVE_EMPLOYEE))) : <span id="modal-reserve-req-emp"></span></div>
                                                <div>@Message.getMessage(CommonLabel.LABEL_RESERVE_NO) : <span id="modal-reserve-no"></span></div>
                                                <div style="display:@(If(ViewBag.MatchingFuncFlg = "Y" AndAlso Not IsNothing(ViewBag.MatchingReserveDetail), "display", "None"))">@Message.getMessage(CommonLabel.LABEL_CONST_NO) : <span id="modal-const-no"></span></div>
                                                <div style="display:@(If(ViewBag.MatchingFuncFlg = "Y" AndAlso Not IsNothing(ViewBag.MatchingReserveDetail) AndAlso ViewBag.MatchingReserveDetail = "2", "display", "None"))">@Message.getMessage(CommonLabel.LABEL_CONST_DT) : <span id="modal-const-dt"></span></div>
                                                <div style="display:@(If(ViewBag.MatchingFuncFlg = "Y" AndAlso Not IsNothing(ViewBag.MatchingReserveDetail) AndAlso ViewBag.MatchingReserveDetail = "2", "display", "None"))">@Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS) : <span id="modal-const-contents"></span></div>
                                                <div style="display:@(If(ViewBag.MatchingFuncFlg = "Y" AndAlso Not IsNothing(ViewBag.MatchingReserveDetail) AndAlso ViewBag.MatchingReserveDetail = "2", "display", "None"))">@Message.getMessage(CommonLabel.LABEL_CONST_EMPLOYEE) : <span id="modal-const-employee"></span></div>

                                            </div>
                                            <div Class="PopupButton float-left mr10">
                                                <input id="modal-reserve-edit" Class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEditModal_onclick()">
                                                <input id="modal-reserve-show" Class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_SANSHO)" onclick="btnDispModal_onclick()">
                                            </div>
                                            <div Class="PopupButton float-right">
                                                <input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CLOSE)">
                                            </div>
                                        </div>

                                        @*2022/7/25 sv sato add start*@
                                        @*CSV取込モーダル画面*@
                                        @<div id="modal-csv-content" class="modal-lending-status">
                                            <div class="popup-title-wrap">
                                                <p class="Line3"></p>
                                                <p class="Line4"></p>
                                                <h2 id="popup-title-csv"></h2>
                                                <p class="Line3"></p>
                                                <p class="Line4"></p>
                                            </div>
                                            @*@Html.AntiForgeryToken()*@
                                            <div Class="PopupButton clearfix mb10">
                                                <div class="float-left">
                                                    <input type="file" name="CsvFile" id="CsvFile" />
                                                </div>
                                                <div class="float-right">
                                                    <input id="modal-import-btn" class="defButton CsvImport" style="width:120px" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImportStart_onclick()">
                                                </div>
                                            </div>

                                            <div class="ichiran clearfix mb10">
                                                <div id="div_koumoku_h">
                                                    <table class="ich_table" id="csv_tbl_koumoku_h">
                                                        <tbody>
                                                            <tr>
                                                                <th class="col1">@Message.getMessage(CommonLabel.LABEL_ROW)</th>
                                                                <th class="col2">@Message.getMessage(CommonLabel.LABEL_DATA)</th>
                                                                <th class="col3">@Message.getMessage(CommonLabel.LABEL_ERRORGAIYOU)</th>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </div>
                                                <div style="height:133px; overflow-y:scroll;">
                                                    <div id="div_koumoku_d">
                                                        <table class="ich_table" id="csv_tbl_koumoku_d">
                                                            <tbody id="csv-err-ichiran">
                                                                @*エラーグリッド一覧表示*@
                                                                @For icnt As Integer = 1 To DataInport.DISP_ROW_COUNT
                                                                @<tr><td Class="csv_col1">　</td><td class="csv_col2">　</td><td class="csv_col3">　</td></tr>
                                                                Next
                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </div>
                                            </div>

                                            <div id="modal-msg" class="mb10">

                                            </div>

                                            <div Class="PopupButton float-right">
                                                <input id="csvimport-modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="fileNmClear()">
                                            </div>
                                        </div>
                                        @*2022/7/25 sv sato add end*@

                                        @*予約情報引き渡し用隠し項目*@
                                        @<input id="ReserveIdKey" name="ReserveIdKey" type="hidden">
                                        @<input id="MainteIdKey" name="MainteIdKey" type="hidden">
                                        @<input id="MainteStatus" name="MainteStatus" type="hidden">


                                        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
                                        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
                                        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
                                        @*@Html.HiddenFor(Function(model) model.Last.INS_DATETIME)
                                            @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_CD)
                                            @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_NAME)
                                            @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
                                            @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
                                            @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
                                            @Html.HiddenFor(Function(model) model.Last.UPD_TASK)*@
                                        @Html.HiddenFor(Function(model) model.FirstOrDefault.OpenFlg)
                                        @<br>
                                        For i = 0 To Model.FirstOrDefault.isClosedColumns.Count - 1
                                            @Html.HiddenFor(Function(model) model.FirstOrDefault.isClosedColumns(i).isClosed)
                                            @Html.HiddenFor(Function(model) model.FirstOrDefault.isClosedColumns(i).columnName)
                                            Next

                                        End Using
</div>
