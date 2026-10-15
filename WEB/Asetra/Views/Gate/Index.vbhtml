@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of GateViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelGate.TITLE)
End Code
<script>
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessCreate") {
            //登録
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessEdit") {
            //変更
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT_GATE)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessDelete") {
            //削除
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            //  ゲートの有効/無効変更時処理　コントロールの状態設定
            SetControlCondition()

            if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

                //ゲートCDを書き込み可
                document.getElementById("GATE_CD").readOnly = false;
                $("#GATE_CD").removeClass("disable");

                //削除ボタンを非表示
                $("#modal-delete-btn").css("display", "none");

                //登録画面を表示
                ShowModalWindow()
                //$('.create-btn').trigger("click")
            } else {
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

                //ゲートコードを書き込み不可
                document.getElementById("GATE_CD").readOnly = true;
                $("#GATE_CD").removeClass("input-error");
                $("#GATE_CD").addClass("disable");

                //削除ボタンを表示
                $("#modal-delete-btn").css("display", "block");

                //変更画面を表示
                ShowModalWindow()
                //$('.update-btn').trigger("click")
            }

            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")

            //機種変更時処理
            ChangeDeviceType();            //20220511 ADD Kawamoto
        }

        let dispId;
        let columns;

        if ("@Model.FirstOrDefault.GATE_DIV" == "@GateDiv.Cd.WG") {
            //表示画面区分設定
            dispId = "@DispId.GATE_WAVE_GATE"

            //列定義
            columns = [
                { id: "@ColKey.GATE.CHECK_SELECT_WG", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.GATE_WAVE_GATE", "@ColKey.GATE.CHECK_SELECT_WG", 40), sortOrder: -2, formatter: GateCheckboxFormatter, cssClass: "first-column" },
                { id: "@ColKey.GATE.GATE_CD_WG", name: '@Message.getMessage(CommonLabel.LABEL_GATE_CD)', field: "GATE_CD", width: getColumnWidth("@DispId.GATE_WAVE_GATE", "@ColKey.GATE.GATE_CD_WG", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_GATE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_GATE_CD").DispFlag" },
                { id: "@ColKey.GATE.GATE_NAME_WG", name: '@Message.getMessage(CommonLabel.LABEL_GATE_NAME)', field: "GATE_NAME", width: getColumnWidth("@DispId.GATE_WAVE_GATE", "@ColKey.GATE.GATE_NAME_WG", 300), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_GATE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_GATE_NAME").DispFlag" },
                { id: "@ColKey.GATE.GATE_IPADDRESS_WG", name: '@Message.getMessage(CommonLabel.LABEL_GATE_IP)', field: "GATE_IPADDRESS", width: getColumnWidth("@DispId.GATE_WAVE_GATE", "@ColKey.GATE.GATE_IPADDRESS_WG", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_GATE_IPADDRESS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_GATE_IPADDRESS").DispFlag" },
                { id: "@ColKey.GATE.GATE_VALID_FLAG_WG", name: '@Message.getMessage(CommonLabel.LABEL_GATE) @Message.getMessage(CommonLabel.LABEL_GATE_VALID)', field: "GATE_VALID_FLAG", width: getColumnWidth("@DispId.GATE_WAVE_GATE", "@ColKey.GATE.GATE_VALID_FLAG_WG", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_GATE_VALID_FLAG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_GATE_VALID_FLAG").DispFlag", formatter: GateValidFlagFomatter },
                { id: "@ColKey.GATE.PATLITE_VALID_FLAG", name: '@Message.getMessage(CommonLabel.LABEL_PATLITE) @Message.getMessage(CommonLabel.LABEL_GATE_VALID)', field: "PATLITE_VALID_FLAG", width: getColumnWidth("@DispId.GATE_WAVE_GATE", "@ColKey.GATE.PATLITE_VALID_FLAG", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_PATLITE_VALID_FLAG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_PATLITE_VALID_FLAG").DispFlag", formatter: PatliteValidFlagFomatter },
                { id: "@ColKey.GATE.THERMO1_VALID_FLAG", name: '@Message.getMessage(CommonLabel.LABEL_THERMO1) @Message.getMessage(CommonLabel.LABEL_GATE_VALID)', field: "THERMO1_VALID_FLAG", width: getColumnWidth("@DispId.GATE_WAVE_GATE", "@ColKey.GATE.THERMO1_VALID_FLAG", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_THERMO1_VALID_FLAG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_THERMO1_VALID_FLAG").DispFlag", formatter: Thermo01ValidFlagFomatter },
                { id: "@ColKey.GATE.THERMO2_VALID_FLAG", name: '@Message.getMessage(CommonLabel.LABEL_THERMO2) @Message.getMessage(CommonLabel.LABEL_GATE_VALID)', field: "THERMO2_VALID_FLAG", width: getColumnWidth("@DispId.GATE_WAVE_GATE", "@ColKey.GATE.THERMO2_VALID_FLAG", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_THERMO2_VALID_FLAG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_THERMO2_VALID_FLAG").DispFlag", formatter: Thermo02ValidFlagFomatter },
                { id: "@ColKey.GATE.GATE_CHANNEL", name: '@Message.getMessage(CommonLabel.LABEL_GATE_CHANNEL)', field: "GATE_CHANNEL", width: getColumnWidth("@DispId.GATE_WAVE_GATE", "@ColKey.GATE.GATE_CHANNEL", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("WG_GATE_CHANNEL").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("WG_GATE_CHANNEL").DispFlag", formatter: RightAlignFormatter },
            ]
        } else {
            //表示画面区分設定
            dispId = "@DispId.GATE_SPEED_WAY"

            //列定義
            columns = [
                { id: "@ColKey.GATE.CHECK_SELECT_SW", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.CHECK_SELECT_SW", 40), sortOrder: -2, formatter: GateCheckboxFormatter, cssClass: "first-column", },
                { id: "@ColKey.GATE.GATE_CD_SW", name: '@Message.getMessage(CommonLabel.LABEL_GATE_CD)', field: "GATE_CD", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.GATE_CD_SW", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_GATE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_GATE_CD").DispFlag" },
                { id: "@ColKey.GATE.GATE_NAME_SW", name: '@Message.getMessage(CommonLabel.LABEL_GATE_NAME)', field: "GATE_NAME", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.GATE_NAME_SW", 300), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_GATE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_GATE_NAME").DispFlag" },
                { id: "@ColKey.GATE.GATE_IPADDRESS_SW", name: '@Message.getMessage(CommonLabel.LABEL_GATE_IP)', field: "GATE_IPADDRESS", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.GATE_IPADDRESS_SW", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_GATE_IPADDRESS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_GATE_IPADDRESS").DispFlag" },
                { id: "@ColKey.GATE.FREQUENCY", name: '@Message.getMessage(CommonLabel.FREQUENCY)', field: "DISP_FREQUENCY", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.FREQUENCY", 300), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_FREQUENCY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_FREQUENCY").DispFlag", formatter: RightAlignFormatter },
                { id: "@ColKey.GATE.ANTENNA_POW", name: '@Message.getMessage(CommonLabel.LABEL_ANTENNA_POW)', field: "ANTENNA_RF_POWER", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.ANTENNA_POW", 130), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_ANTENNA_POW").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_ANTENNA_POW").DispFlag", formatter: AntenaPowRightAlignFormatter },
                { id: "@ColKey.GATE.GATE_VALID_FLAG_SW", name: '@Message.getMessage(CommonLabel.LABEL_GATE) @Message.getMessage(CommonLabel.LABEL_GATE_VALID)', field: "GATE_VALID_FLAG", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.GATE_VALID_FLAG_SW", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_GATE_VALID_FLAG").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_GATE_VALID_FLAG").DispFlag", formatter: GateValidFlagFomatter },
                { id: "@ColKey.GATE.DEVICE_TYPE", name: '@Message.getMessage(CommonLabel.DEVICE_TYPE)', field: "DEVICE_TYPE", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.DEVICE_TYPE", 110), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_DEVICE_TYPE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_DEVICE_TYPE").DispFlag", formatter: DeviceTypeFormatter },
                { id: "@ColKey.GATE.GPIO", name: '@Message.getMessage(CommonLabel.GPIO)', field: "GPIO", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.GPIO", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_GPIO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_GPIO").DispFlag", formatter: GpioFormatter },
                { id: "@ColKey.GATE.IO_No1", name: '@Message.getMessage(CommonLabel.IO_No1) @Message.getMessage(CommonLabel.LABEL_GATE_VALID)', field: "IO_No1", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.IO_No1", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_IO_No1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_IO_No1").DispFlag", formatter: IoNo1Formatter },
                { id: "@ColKey.GATE.IO_No2", name: '@Message.getMessage(CommonLabel.IO_No2) @Message.getMessage(CommonLabel.LABEL_GATE_VALID)', field: "IO_No2", width: getColumnWidth("@DispId.GATE_SPEED_WAY", "@ColKey.GATE.IO_No2", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("SW_IO_No2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SW_IO_No2").DispFlag", formatter: IoNo2Formatter },
            ]
        }

        // Gridのオプション機能の設定
        let options = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: true,                       //Gridの高さの自動調整可否
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列を加算
        }


        // Grid初期化
        let data = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, dispId, fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, dispId, fixCol);
        }
    });

    //選択用formatter
    function GateCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.GATE_CD)
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;">' +
               '<input type="checkbox" class="slick-checkbox" ' +
               'value="' + key + '" ' +
               (value == 1 ? 'checked="checked"' : '') + ' />' +
               '</div>';
    }

    //ゲート 有効/無効用formatter
    function GateValidFlagFomatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (dataContext.GATE_VALID_FLAG == '@ValidFlg.Cd.Valid') {
            status = '@Message.getMessage(CommonLabel.VALUE_VALID)'
        }
        else {
            status = '@Message.getMessage(CommonLabel.VALUE_INVALID)'
        }
        return status
    }

    //パトライト 有効/無効用formatter
    function PatliteValidFlagFomatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (dataContext.PATLITE_VALID_FLAG == '@ValidFlg.Cd.Valid') {
            status = '@Message.getMessage(CommonLabel.VALUE_VALID)'
        }
        else {
            status = '@Message.getMessage(CommonLabel.VALUE_INVALID)'
        }
        return status
    }

    //サーモセンサ１ 有効/無効	用formatter
    function Thermo01ValidFlagFomatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (dataContext.THERMO1_VALID_FLAG == '@ValidFlg.Cd.Valid') {
            status = '@Message.getMessage(CommonLabel.VALUE_VALID)'
        }
        else {
            status = '@Message.getMessage(CommonLabel.VALUE_INVALID)'
        }
        return status
    }

    //サーモセンサ2 有効/無効用formatter
    function Thermo02ValidFlagFomatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (dataContext.THERMO2_VALID_FLAG == '@ValidFlg.Cd.Valid') {
            status = '@Message.getMessage(CommonLabel.VALUE_VALID)'
        }
        else {
            status = '@Message.getMessage(CommonLabel.VALUE_INVALID)'
        }
        return status
    }

    // テキスト表示(右寄せ)
    function RightAlignFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(value) != '') {
            return '<div style="text-align:right; width:100%;">' + value + '</div>';
        }
    }

    // 電波強度用テキスト表示(右寄せ)　
    function AntenaPowRightAlignFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(dataContext.DISP_ANTENNA_POWER) != '') {
            return '<div style="text-align:right; width:100%;">' + dataContext.DISP_ANTENNA_POWER + '</div>';
        }
    }


    //機種用formatter
    function DeviceTypeFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (dataContext.DEVICE_TYPE == '@DeviceType.Cd.R420') {
            status = '@Message.getMessage(CommonLabel.R420)'
        } else if (dataContext.DEVICE_TYPE == '@DeviceType.Cd.R700') {
            status = '@Message.getMessage(CommonLabel.R700)'
        }
        return status
    }

    //GPIO用formatter
    function GpioFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (dataContext.GPIO_DEVICE == '@GpioDevice.Cd.Speed_Way') {
            status = '@Message.getMessage(CommonLabel.SPEED_WAY)'
        } else if (dataContext.GPIO_DEVICE == '@GpioDevice.Cd.Dio_Board') {
            status = '@Message.getMessage(CommonLabel.CONTEC_DIO_BOARD)'
        }
        return status
    }

    //IO No1用formatter
    function IoNo1Formatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (dataContext.CAMERA_VALID_FLAG == '@ValidFlg.Cd.Valid') {
            status = '@Message.getMessage(CommonLabel.VALUE_VALID)'
        } else {
            status = '@Message.getMessage(CommonLabel.VALUE_INVALID)'
        }
        return status
    }

    //IO No2用formatter
    function IoNo2Formatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (dataContext.PATLITE_VALID_FLAG == '@ValidFlg.Cd.Valid') {
            status = '@Message.getMessage(CommonLabel.VALUE_VALID)'
        }
        else {
            status = '@Message.getMessage(CommonLabel.VALUE_INVALID)'
        }
        return status
    }

    //登録ボタンクリック処理
    function btnCreate_onclick() {
        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

        //ゲートCDを書き込み可
        document.getElementById("GATE_CD").readOnly = false;
        $("#GATE_CD").removeClass("disable");

        //削除ボタンを非表示
        $("#modal-delete-btn").css("display", "none");

        if ("@Model.FirstOrDefault.GATE_DIV" == "@GateDiv.Cd.WG") {
            //WaveGate使用
            //画面を初期化する
            document.getElementById("GATE_CD").value = "";
            document.getElementById("GATE_NAME").value = "";
            document.getElementById("GATE_IPADDRESS").value = "";
            document.getElementById("GATE_CHANNEL").value = 1;
            document.getElementById("PATLITE_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("GATE_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA1_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA2_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA3_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA4_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA1_RF_POWER").value = "48";
            document.getElementById("ANTENNA2_RF_POWER").value = "48";
            document.getElementById("ANTENNA3_RF_POWER").value = "48";
            document.getElementById("ANTENNA4_RF_POWER").value = "48";
            document.getElementById("THERMO1_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("THERMO2_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
        } else {
            //20220328 ADD Start Kawamoto v2.1
            //SpeedWay使用
            document.getElementById("GATE_CD").value = "";
            document.getElementById("GATE_NAME").value = "";
            document.getElementById("GATE_IPADDRESS").value = "";
            document.getElementById("PATLITE_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("GATE_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("CAMERA_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA1_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA2_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA3_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA4_VALID_FLAG").value = "@ValidFlg.Cd.Valid";
            document.getElementById("ANTENNA1_RF_POWER").value = "30";
            document.getElementById("SENSITIVITY").value = "-80";
            document.getElementById("SESSION").value = "0";
            document.getElementById("FREQUENCY1").value = "";
            document.getElementById("FREQUENCY2").value = "";
            document.getElementById("FREQUENCY3").value = "";
            document.getElementById("FREQUENCY4").value = "";
            document.getElementById("SEARCH_MODE").value = "@SearchMode.Cd.DualTarget";
            //20220511 UPD Start Kawamoto
            //document.getElementById("READER_MODE").value = "@ReaderMode.Cd.AutoSetDenseReader";
            document.getElementById("READER_MODE").value = "@ReaderMode.Cd.AutoSetDenseReaderDeepScan";
            //20220511 UPD End Kawamoto
            document.getElementById("TAG_POPULATION").value = "32";
            document.getElementById("DEVICE_NAME_C").value = "";
            document.getElementById("BIT_POS_C").value = "";
            document.getElementById("DEVICE_NAME_P").value = "";
            document.getElementById("BIT_POS_P").value = "";
            document.getElementById("UNCONDITIONAL_P").value = "@ValidFlg.Cd.Invalid";
            document.getElementById("UNCONDITIONAL_C").value = "@ValidFlg.Cd.Invalid";
            //20220328 ADD End Kawamoto v2.1
            //20220511 ADD Start Kawamoto
            document.getElementById("DEVICE_TYPE").value = "@DeviceType.Cd.R700";
            document.getElementById("GPIO_DEVICE").value = "@GpioDevice.Cd.Speed_Way";
            document.getElementById("GPO_PORT_C").value = "1";
            document.getElementById("GPO_PORT_P").value = "2";

            //機種変更時処理
            ChangeDeviceType();
            //20220511 ADD End Kawamoto
        }

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        //登録ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_CREATE_CLICK";

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //変更ボタンクリック処理
    function btnEdit_onclick() {
        //テーブルの選択行を取得する。
        var text = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
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

        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

        //ゲートコードを書き込み不可
        document.getElementById("GATE_CD").readOnly = true;
        $("#GATE_CD").removeClass("input-error");
        $("#GATE_CD").addClass("disable");

        //削除ボタンを表示
        $("#modal-delete-btn").css("display", "block");

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/Gate")/GetEditData',
                cache: false,
                data: { ID: text[0], DIV: "@Model.FirstOrDefault.GATE_DIV" },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }
                    if ("@Model.FirstOrDefault.GATE_DIV" == "@GateDiv.Cd.WG") {
                        //WaveGate使用
                        //取得したデータを表示する
                        document.getElementById("GATE_CD").value = data.GATE_CD;
                        document.getElementById("GATE_NAME").value = data.GATE_NAME;
                        document.getElementById("GATE_IPADDRESS").value = data.GATE_IPADDRESS;
                        document.getElementById("GATE_CHANNEL").value = data.GATE_CHANNEL;
                        document.getElementById("PATLITE_VALID_FLAG").value = data.PATLITE_VALID_FLAG;
                        document.getElementById("GATE_VALID_FLAG").value = data.GATE_VALID_FLAG;
                        document.getElementById("ANTENNA1_VALID_FLAG").value = data.ANTENNA1_VALID_FLAG;
                        document.getElementById("ANTENNA1_RF_POWER").value = data.ANTENNA1_RF_POWER;
                        document.getElementById("ANTENNA2_VALID_FLAG").value = data.ANTENNA2_VALID_FLAG;
                        document.getElementById("ANTENNA2_RF_POWER").value = data.ANTENNA2_RF_POWER;
                        document.getElementById("ANTENNA3_VALID_FLAG").value = data.ANTENNA3_VALID_FLAG;
                        document.getElementById("ANTENNA3_RF_POWER").value = data.ANTENNA3_RF_POWER;
                        document.getElementById("ANTENNA4_VALID_FLAG").value = data.ANTENNA4_VALID_FLAG;
                        document.getElementById("ANTENNA4_RF_POWER").value = data.ANTENNA4_RF_POWER;
                        document.getElementById("THERMO1_VALID_FLAG").value = data.THERMO1_VALID_FLAG;
                        document.getElementById("THERMO2_VALID_FLAG").value = data.THERMO2_VALID_FLAG;
                        document.getElementById("INS_DATETIME").value = formatDate(new Date(data.InsDatetimeString), "yyyy/MM/dd HH:mm:ss");
                        document.getElementById("INS_EMPLOYEE_CD").value = data.INS_EMPLOYEE_CD;
                        document.getElementById("INS_EMPLOYEE_NAME").value = data.INS_EMPLOYEE_NAME;
                        document.getElementById("UPD_DATETIME").value = formatDate(new Date(data.UpdDatetimeString), "yyyy/MM/dd HH:mm:ss");
                        document.getElementById("UPD_EMPLOYEE_CD").value = data.UPD_EMPLOYEE_CD;
                        document.getElementById("UPD_EMPLOYEE_NAME").value = data.UPD_EMPLOYEE_NAME;
                        document.getElementById("UPD_TASK").value = data.RenewProcess;

                        // ゲートの有効/無効変更時処理　コントロールの状態設定
                        SetControlCondition()

                    } else {
                        //20220328 ADD Start Kawamoto v2.1
                        //SpeedWay使用
                        //取得したデータを表示する
                        document.getElementById("GATE_CD").value = data.GATE_CD;
                        document.getElementById("GATE_NAME").value = data.GATE_NAME;
                        document.getElementById("GATE_IPADDRESS").value = data.GATE_IPADDRESS;
                        document.getElementById("PATLITE_VALID_FLAG").value = data.PATLITE_VALID_FLAG;
                        document.getElementById("GATE_VALID_FLAG").value = data.GATE_VALID_FLAG;
                        document.getElementById("CAMERA_VALID_FLAG").value = data.CAMERA_VALID_FLAG;
                        document.getElementById("ANTENNA1_VALID_FLAG").value = data.ANTENNA1_VALID_FLAG;
                        document.getElementById("ANTENNA1_RF_POWER").value = data.ANTENNA1_RF_POWER;
                        document.getElementById("ANTENNA2_VALID_FLAG").value = data.ANTENNA2_VALID_FLAG;
                        document.getElementById("ANTENNA3_VALID_FLAG").value = data.ANTENNA3_VALID_FLAG;
                        document.getElementById("ANTENNA4_VALID_FLAG").value = data.ANTENNA4_VALID_FLAG;
                        document.getElementById("UNCONDITIONAL_P").value = data.UNCONDITIONAL_P;
                        document.getElementById("UNCONDITIONAL_C").value = data.UNCONDITIONAL_C;
                        document.getElementById("INS_DATETIME").value = formatDate(new Date(data.InsDatetimeString), "yyyy/MM/dd HH:mm:ss");
                        document.getElementById("INS_EMPLOYEE_CD").value = data.INS_EMPLOYEE_CD;
                        document.getElementById("INS_EMPLOYEE_NAME").value = data.INS_EMPLOYEE_NAME;
                        document.getElementById("UPD_DATETIME").value = formatDate(new Date(data.UpdDatetimeString), "yyyy/MM/dd HH:mm:ss");
                        document.getElementById("UPD_EMPLOYEE_CD").value = data.UPD_EMPLOYEE_CD;
                        document.getElementById("UPD_EMPLOYEE_NAME").value = data.UPD_EMPLOYEE_NAME;
                        document.getElementById("UPD_TASK").value = data.RenewProcess;

                        document.getElementById("SENSITIVITY").value = data.SENSITIVITY;
                        document.getElementById("SESSION").value = data.SESSION;
                        document.getElementById("FREQUENCY1").value = data.FREQUENCY1;
                        document.getElementById("FREQUENCY2").value = data.FREQUENCY2;
                        document.getElementById("FREQUENCY3").value = data.FREQUENCY3;
                        document.getElementById("FREQUENCY4").value = data.FREQUENCY4;
                        document.getElementById("SEARCH_MODE").value = data.SEARCH_MODE;
                        document.getElementById("READER_MODE").value = data.READER_MODE;
                        document.getElementById("TAG_POPULATION").value = data.TAG_POPULATION;
                        document.getElementById("DEVICE_NAME_C").value = data.DEVICE_NAME_C;
                        document.getElementById("BIT_POS_C").value = data.BIT_POS_C;
                        document.getElementById("DEVICE_NAME_P").value = data.DEVICE_NAME_P;
                        document.getElementById("BIT_POS_P").value = data.BIT_POS_P;

                        //20220511 ADD Start Kawamoto
                        document.getElementById("DEVICE_TYPE").value = data.DEVICE_TYPE;
                        document.getElementById("GPIO_DEVICE").value = data.GPIO_DEVICE;
                        document.getElementById("GPO_PORT_C").value = data.GPO_PORT_C;
                        document.getElementById("GPO_PORT_P").value = data.GPO_PORT_P;
                        //20220511 ADD End Kawamoto

                        // ゲートの有効/無効変更時処理　コントロールの状態設定
                        SetControlCondition()
                        //20220328 ADD End Kawamoto v2.1
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }

        //変更ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_UPDATE_CLICK";

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //登録ボタンクリック処理（確認メッセージ）
    function btnCreateModal_onclick() {
        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
            //確認メッセージを出し、ＯＫなら登録処理
            // ダイアログのメッセージを設定
            $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)");
            @*showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/Gate")/Create")*@
        } else {
            //確認メッセージを出し、ＯＫなら変更処理
            // ダイアログのメッセージを設定
            $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)");
            @*showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/Gate")/Create")*@
        }

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    if ("@Model.FirstOrDefault.GATE_DIV" == "@GateDiv.Cd.WG") {
                        //WaveGate使用
                        // チャンネル
                        $("#GATE_CHANNEL").removeAttr("disabled");
                        // パトライト有効/無効フラグ
                        $("#PATLITE_VALID_FLAG").removeAttr("disabled");
                        // アンテナ有効/無効フラグ
                        $("#ANTENNA1_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA2_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA3_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA4_VALID_FLAG").removeAttr("disabled");
                        // サーモセンサ有効/無効フラグ
                        $("#THERMO1_VALID_FLAG").removeAttr("disabled");
                        $("#THERMO2_VALID_FLAG").removeAttr("disabled");
                    } else {
                        //20220328 ADD Start Kawamoto v2.1
                        //SpeedWay使用
                        // チャンネル
                        $("#GATE_CHANNEL").removeAttr("disabled");
                        // アンテナ有効/無効フラグ
                        $("#ANTENNA1_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA2_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA3_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA4_VALID_FLAG").removeAttr("disabled");
                        // パトライト有効/無効フラグ
                        $("#PATLITE_VALID_FLAG").removeAttr("disabled");
                        // カメラ有効/無効フラグ
                        $("#CAMERA_VALID_FLAG").removeAttr("disabled");
                        // 無条件で発報(パトライト) 有効/無効フラグ
                        $("#UNCONDITIONAL_P").removeAttr("disabled");
                        // 無条件で発報(カメラ) 有効/無効フラグ
                        $("#UNCONDITIONAL_C").removeAttr("disabled");
                        // Searchモード
                        $("#SEARCH_MODE").removeAttr("disabled");
                        // Readerモード
                        $("#READER_MODE").removeAttr("disabled");
                        // 周波数
                        $("#FREQUENCY1").removeAttr("disabled");
                        $("#FREQUENCY2").removeAttr("disabled");
                        $("#FREQUENCY3").removeAttr("disabled");
                        $("#FREQUENCY4").removeAttr("disabled");
                        //20220328 ADD End Kawamoto v2.1
                        //20220511 ADD Start Kawamoto
                        // 機種
                        $("#DEVICE_TYPE").removeAttr("disabled");
                        // GPIO機器
                        $("#GPIO_DEVICE").removeAttr("disabled");
                        // GPOポート(カメラ)
                        $("#GPO_PORT_C").removeAttr("disabled");
                        // GPOポート(パトライト)
                        $("#GPO_PORT_P").removeAttr("disabled");
                        //20220511 ADD End Kawamoto
                    }

                    var befAction = $('#form').attr("action");
                    $(this).dialog("close");
                    $('#form').attr("action", "@Url.Content("~/Gate")/Create");
                    $('#form').submit();
                    $('#form').attr("action", befAction);
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

    //削除ボタンクリック処理
    function btnDelete_onclick() {
        @*showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/Gate")/Delete");*@
        //確認メッセージを出し、ＯＫなら削除処理
        // ダイアログのメッセージを設定
        $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)");
        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    if ("@Model.FirstOrDefault.GATE_DIV" == "@GateDiv.Cd.WG") {
                        //WaveGate使用
                        // チャンネル
                        $("#GATE_CHANNEL").removeAttr("disabled");
                        // パトライト有効/無効フラグ
                        $("#PATLITE_VALID_FLAG").removeAttr("disabled");
                        // アンテナ有効/無効フラグ
                        $("#ANTENNA1_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA2_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA3_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA4_VALID_FLAG").removeAttr("disabled");
                        // サーモセンサ有効/無効フラグ
                        $("#THERMO1_VALID_FLAG").removeAttr("disabled");
                        $("#THERMO2_VALID_FLAG").removeAttr("disabled");
                    } else {
                        //20220328 ADD Start Kawamoto v2.1
                        //SpeedWay使用
                        // チャンネル
                        $("#GATE_CHANNEL").removeAttr("disabled");
                        // アンテナ有効/無効フラグ
                        $("#ANTENNA1_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA2_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA3_VALID_FLAG").removeAttr("disabled");
                        $("#ANTENNA4_VALID_FLAG").removeAttr("disabled");
                        // パトライト有効/無効フラグ
                        $("#PATLITE_VALID_FLAG").removeAttr("disabled");
                        // カメラ有効/無効フラグ
                        $("#CAMERA_VALID_FLAG").removeAttr("disabled");
                        // 無条件で発報(パトライト) 有効/無効フラグ
                        $("#UNCONDITIONAL_P").removeAttr("disabled");
                        // 無条件で発報(カメラ) 有効/無効フラグ
                        $("#UNCONDITIONAL_C").removeAttr("disabled");
                        // Searchモード
                        $("#SEARCH_MODE").removeAttr("disabled");
                        // Readerモード
                        $("#READER_MODE").removeAttr("disabled");
                        // 周波数
                        $("#FREQUENCY1").removeAttr("disabled");
                        $("#FREQUENCY2").removeAttr("disabled");
                        $("#FREQUENCY3").removeAttr("disabled");
                        $("#FREQUENCY4").removeAttr("disabled");
                        //20220328 ADD End Kawamoto v2.1

                        //20220511 ADD Start Kawamoto
                        // 機種
                        $("#DEVICE_TYPE").removeAttr("disabled");
                        // GPIO機器
                        $("#GPIO_DEVICE").removeAttr("disabled");
                        // GPOポート(カメラ)
                        $("#GPO_PORT_C").removeAttr("disabled");
                        // GPOポート(パトライト)
                        $("#GPO_PORT_P").removeAttr("disabled");
                        //20220511 ADD End Kawamoto
                    }
                    var befAction = $('#form').attr("action");
                    $(this).dialog("close");
                    $('#form').attr("action", "@Url.Content("~/Gate")/Delete");
                    $('#form').submit();
                    $('#form').attr("action", befAction);
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

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    // ゲートの有効/無効変更時処理　コントロールの状態設定
    function SetControlCondition() {
        if ("@Model.FirstOrDefault.GATE_DIV" == "@GateDiv.Cd.WG") {
            //WaveGate使用
            if (document.getElementById("GATE_VALID_FLAG").value == "@ValidFlg.Cd.Valid") {
                // IPアドレスを書き込み可
                document.getElementById("GATE_IPADDRESS").readOnly = false;
                $("#GATE_IPADDRESS").removeClass("disable");

                // チャンネル
                $("#GATE_CHANNEL").removeAttr("disabled");

                // パトライト有効/無効フラグ
                $("#PATLITE_VALID_FLAG").removeAttr("disabled");

                // アンテナ有効/無効フラグ
                $("#ANTENNA1_VALID_FLAG").removeAttr("disabled");
                $("#ANTENNA2_VALID_FLAG").removeAttr("disabled");
                $("#ANTENNA3_VALID_FLAG").removeAttr("disabled");
                $("#ANTENNA4_VALID_FLAG").removeAttr("disabled");

                // 電波強度の状態設定
                ChangeAntenna1()
                ChangeAntenna2()
                ChangeAntenna3()
                ChangeAntenna4()

                // サーモセンサ有効/無効フラグ選択不可
                $("#THERMO1_VALID_FLAG").removeAttr("disabled");
                $("#THERMO2_VALID_FLAG").removeAttr("disabled");

            } else {
                // IPアドレスを書き込み不可
                document.getElementById("GATE_IPADDRESS").readOnly = true;
                $("#GATE_IPADDRESS").addClass("disable");

                // チャンネル選択不可
                $("#GATE_CHANNEL").attr("disabled", "disabled");

                // パトライト有効/無効フラグ選択不可
                $("#PATLITE_VALID_FLAG").attr("disabled", "disabled");

                // アンテナ有効/無効フラグ選択不可
                $("#ANTENNA1_VALID_FLAG").attr("disabled", "disabled");
                $("#ANTENNA2_VALID_FLAG").attr("disabled", "disabled");
                $("#ANTENNA3_VALID_FLAG").attr("disabled", "disabled");
                $("#ANTENNA4_VALID_FLAG").attr("disabled", "disabled");

                // 電波強度を書き込み不可
                document.getElementById("ANTENNA1_RF_POWER").readOnly = true;
                $("#ANTENNA1_RF_POWER").addClass("disable");
                document.getElementById("ANTENNA2_RF_POWER").readOnly = true;
                $("#ANTENNA2_RF_POWER").addClass("disable");
                document.getElementById("ANTENNA3_RF_POWER").readOnly = true;
                $("#ANTENNA3_RF_POWER").addClass("disable");
                document.getElementById("ANTENNA4_RF_POWER").readOnly = true;
                $("#ANTENNA4_RF_POWER").addClass("disable");

                // サーモセンサ有効/無効フラグ選択不可
                $("#THERMO1_VALID_FLAG").attr("disabled", "disabled");
                $("#THERMO2_VALID_FLAG").attr("disabled", "disabled");
            }
        } else {
            //20220328 ADD Start Kawamoto v2.1
            //SpeedWay使用
            if (document.getElementById("GATE_VALID_FLAG").value == "@ValidFlg.Cd.Valid") {
                // IPアドレスを書き込み可
                document.getElementById("GATE_IPADDRESS").readOnly = false;
                $("#GATE_IPADDRESS").removeClass("disable");

                // アンテナ有効/無効フラグ
                $("#ANTENNA1_VALID_FLAG").removeAttr("disabled");
                $("#ANTENNA2_VALID_FLAG").removeAttr("disabled");
                $("#ANTENNA3_VALID_FLAG").removeAttr("disabled");
                $("#ANTENNA4_VALID_FLAG").removeAttr("disabled");

                // カメラ有効/無効フラグ
                $("#CAMERA_VALID_FLAG").removeAttr("disabled");

                // パトライト有効/無効フラグ
                $("#PATLITE_VALID_FLAG").removeAttr("disabled");

                // 無条件で発報(パトライト) 有効/無効フラグ
                $("#UNCONDITIONAL_P").removeAttr("disabled");

                // 無条件で発報(カメラ) 有効/無効フラグ
                $("#UNCONDITIONAL_C").removeAttr("disabled");

                // SpeedWay関連項目の状態設定
                ChangeAntennaSW()

                // DIOボード カメラ用設定の状態
                ChangeCamera()

                // DIOボード パトライト用設定の状態
                ChangePatlite()

                //20220511 ADD Start Kawamoto
                // 機種
                $("#DEVICE_TYPE").removeClass("disable");

                //機種変更時処理
                ChangeDeviceType();
                //20220511 ADD End Kawamoto

            } else {
                // IPアドレスを書き込み不可
                document.getElementById("GATE_IPADDRESS").readOnly = true;
                $("#GATE_IPADDRESS").addClass("disable");

                // アンテナ有効/無効フラグ選択不可
                $("#ANTENNA1_VALID_FLAG").attr("disabled", "disabled");
                $("#ANTENNA2_VALID_FLAG").attr("disabled", "disabled");
                $("#ANTENNA3_VALID_FLAG").attr("disabled", "disabled");
                $("#ANTENNA4_VALID_FLAG").attr("disabled", "disabled");

                //電波強度を書き込み不可
                document.getElementById("ANTENNA1_RF_POWER").readOnly = true;
                $("#ANTENNA1_RF_POWER").addClass("disable");

                // 感度を書き込み不可
                document.getElementById("SENSITIVITY").readOnly = true;
                $("#SENSITIVITY").addClass("disable");

                // セッションを書き込み不可
                document.getElementById("SESSION").readOnly = true;
                $("#SESSION").addClass("disable");

                // Searchモード、Readerモード選択不可
                $("#SEARCH_MODE").attr("disabled", "disabled");
                $("#READER_MODE").attr("disabled", "disabled");

                // Tag Populationを書き込み不可
                document.getElementById("TAG_POPULATION").readOnly = true;
                $("#TAG_POPULATION").addClass("disable");

                // 周波数選択不可
                $("#FREQUENCY1").attr("disabled", "disabled");
                $("#FREQUENCY2").attr("disabled", "disabled");
                $("#FREQUENCY3").attr("disabled", "disabled");
                $("#FREQUENCY4").attr("disabled", "disabled");

                // カメラ有効/無効フラグ選択不可
                $("#CAMERA_VALID_FLAG").attr("disabled", "disabled");

                // 機器名(カメラ用)を書き込み不可
                document.getElementById("DEVICE_NAME_C").readOnly = true;
                $("#DEVICE_NAME_C").addClass("disable");

                // ポート番号(カメラ用)を書き込み不可
                document.getElementById("BIT_POS_C").readOnly = true;
                $("#BIT_POS_C").addClass("disable");

                // パトライト有効/無効フラグ選択不可
                $("#PATLITE_VALID_FLAG").attr("disabled", "disabled");

                // 機器名(パトライト用)を書き込み不可
                document.getElementById("DEVICE_NAME_P").readOnly = true;
                $("#DEVICE_NAME_P").addClass("disable");

                // ポート番号(パトライト用)を書き込み不可
                document.getElementById("BIT_POS_P").readOnly = true;
                $("#BIT_POS_P").addClass("disable");

                // 無条件で発報(パトライト) 有効/無効フラグ選択不可
                $("#UNCONDITIONAL_P").attr("disabled", "disabled");

                // 無条件で発報(カメラ) 有効/無効フラグ選択不可
                $("#UNCONDITIONAL_C").attr("disabled", "disabled");

                //20220511 ADD Start Kawamoto
                // 機種選択不可
                $("#DEVICE_TYPE").addClass("disable");

                // GPIO機器選択不可
                $("#GPIO_DEVICE").attr("disabled", "disabled");

                // GPOポート(カメラ)選択不可
                $("#GPO_PORT_C").attr("disabled", "disabled");

                // GPOポート(パトライト)選択不可
                $("#GPO_PORT_P").attr("disabled", "disabled");
                //20220511 ADD End Kawamoto
            }
            //20220328 ADD End Kawamoto v2.1
        }
    }

    //アンテナ１の有効/無効変更時処理
    function ChangeAntenna1() {
        if (document.getElementById("ANTENNA1_VALID_FLAG").value == "@ValidFlg.Cd.Valid") {
            //電波強度を書き込み可
            document.getElementById("ANTENNA1_RF_POWER").readOnly = false;
            $("#ANTENNA1_RF_POWER").removeClass("disable");

        } else {
            //電波強度を書き込み不可
            document.getElementById("ANTENNA1_RF_POWER").readOnly = true;
            $("#ANTENNA1_RF_POWER").addClass("disable");
        }
    }

    //アンテナ２の有効/無効変更時処理
    function ChangeAntenna2() {
        if (document.getElementById("ANTENNA2_VALID_FLAG").value == "@ValidFlg.Cd.Valid") {
            //電波強度を書き込み可
            document.getElementById("ANTENNA2_RF_POWER").readOnly = false;
            $("#ANTENNA2_RF_POWER").removeClass("disable");

        } else {
            //電波強度を書き込み不可
            document.getElementById("ANTENNA2_RF_POWER").readOnly = true;
            $("#ANTENNA2_RF_POWER").addClass("disable");
        }
    }

    //アンテナ３の有効/無効変更時処理
    function ChangeAntenna3() {
        if (document.getElementById("ANTENNA3_VALID_FLAG").value == "@ValidFlg.Cd.Valid") {
            //電波強度を書き込み可
            document.getElementById("ANTENNA3_RF_POWER").readOnly = false;
            $("#ANTENNA3_RF_POWER").removeClass("disable");

        } else {
            //電波強度を書き込み不可
            document.getElementById("ANTENNA3_RF_POWER").readOnly = true;
            $("#ANTENNA3_RF_POWER").addClass("disable");
        }
    }

    //アンテナ４の有効/無効変更時処理
    function ChangeAntenna4() {
        if (document.getElementById("ANTENNA4_VALID_FLAG").value == "@ValidFlg.Cd.Valid") {
            //電波強度を書き込み可
            document.getElementById("ANTENNA4_RF_POWER").readOnly = false;
            $("#ANTENNA4_RF_POWER").removeClass("disable");

        } else {
            //電波強度を書き込み不可
            document.getElementById("ANTENNA4_RF_POWER").readOnly = true;
            $("#ANTENNA4_RF_POWER").addClass("disable");
        }
    }

    //20220328 ADD Start Kawamoto v2.1
    //アンテナの有効/無効変更時処理(SpeedWay用)
    function ChangeAntennaSW() {
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            document.getElementById("FREQUENCY1").value = "@Model.FirstOrDefault.FREQUENCY1";
            document.getElementById("FREQUENCY2").value = "@Model.FirstOrDefault.FREQUENCY2";
            document.getElementById("FREQUENCY3").value = "@Model.FirstOrDefault.FREQUENCY3";
            document.getElementById("FREQUENCY4").value = "@Model.FirstOrDefault.FREQUENCY4";
        }
        if (document.getElementById("ANTENNA1_VALID_FLAG").value == "@ValidFlg.Cd.Invalid" &&
            document.getElementById("ANTENNA2_VALID_FLAG").value == "@ValidFlg.Cd.Invalid" &&
            document.getElementById("ANTENNA3_VALID_FLAG").value == "@ValidFlg.Cd.Invalid" &&
            document.getElementById("ANTENNA4_VALID_FLAG").value == "@ValidFlg.Cd.Invalid") {

            //電波強度を書き込み不可
            document.getElementById("ANTENNA1_RF_POWER").readOnly = true;
            $("#ANTENNA1_RF_POWER").addClass("disable");

            // 感度を書き込み不可
            document.getElementById("SENSITIVITY").readOnly = true;
            $("#SENSITIVITY").addClass("disable");

            // セッションを書き込み不可
            document.getElementById("SESSION").readOnly = true;
            $("#SESSION").addClass("disable");

            // Searchモード、Readerモード選択不可
            $("#SEARCH_MODE").attr("disabled", "disabled");
            $("#READER_MODE").attr("disabled", "disabled");

            // Tag Populationを書き込み不可
            document.getElementById("TAG_POPULATION").readOnly = true;
            $("#TAG_POPULATION").addClass("disable");

            // 周波数選択不可
            $("#FREQUENCY1").attr("disabled", "disabled");
            $("#FREQUENCY2").attr("disabled", "disabled");
            $("#FREQUENCY3").attr("disabled", "disabled");
            $("#FREQUENCY4").attr("disabled", "disabled");

        } else {
            //電波強度を書き込み可
            document.getElementById("ANTENNA1_RF_POWER").readOnly = false;
            $("#ANTENNA1_RF_POWER").removeClass("disable");

            // 感度を書き込み可
            document.getElementById("SENSITIVITY").readOnly = false;
            $("#SENSITIVITY").removeClass("disable");

            // セッションを書き込み可
            document.getElementById("SESSION").readOnly = false;
            $("#SESSION").removeClass("disable");

            // Searchモード、Readerモード選択可
            $("#SEARCH_MODE").removeAttr("disabled");
            $("#READER_MODE").removeAttr("disabled");

            // Tag Populationを書き込み可
            document.getElementById("TAG_POPULATION").readOnly = false;
            $("#TAG_POPULATION").removeClass("disable");

            // 周波数選択可
            $("#FREQUENCY1").removeAttr("disabled");
            $("#FREQUENCY2").removeAttr("disabled");
            $("#FREQUENCY3").removeAttr("disabled");
            $("#FREQUENCY4").removeAttr("disabled");
        }
    }

    // カメラの有効/無効変更時処理
    function ChangeCamera() {
        if (document.getElementById("CAMERA_VALID_FLAG").value == "@ValidFlg.Cd.Valid") {

            //GPIO機器を選択可
            $("#GPIO_DEVICE").removeAttr("disabled");

            // 無条件で発報(カメラ)(カメラ)を選択可
            $("#UNCONDITIONAL_C").removeAttr("disabled");

            // GPIO機器が「DIOボード」の場合
            if (document.getElementById("GPIO_DEVICE").value == "@GpioDevice.Cd.Dio_Board") {

                // 機器名、ポート番号を書き込み可
                document.getElementById("DEVICE_NAME_C").readOnly = false;
                $("#DEVICE_NAME_C").removeClass("disable");

                document.getElementById("BIT_POS_C").readOnly = false;
                $("#BIT_POS_C").removeClass("disable");

                // GPIO機器が「SpeedWay」の場合
            } else {

                // GPOポート(カメラ)を選択可
                $("#GPO_PORT_C").removeAttr("disabled");
            }
        } else {

            //パトライトが無効の場合
            if (document.getElementById("PATLITE_VALID_FLAG").value == "@ValidFlg.Cd.Invalid") {

                // GPIO機器を選択不可
                $("#GPIO_DEVICE").attr("disabled", "disabled");
            }

            // 無条件で発報(カメラ)を選択不可
            $("#UNCONDITIONAL_C").attr("disabled", "disabled");

            // GPIO機器が「DIOボード」の場合
            if (document.getElementById("GPIO_DEVICE").value == "@GpioDevice.Cd.Dio_Board") {

                // 機器名、ポート番号を書き込み不可
                document.getElementById("DEVICE_NAME_C").readOnly = true;
                $("#DEVICE_NAME_C").addClass("disable");

                document.getElementById("BIT_POS_C").readOnly = true;
                $("#BIT_POS_C").addClass("disable");

                // GPIO機器が「SpeedWay」の場合
            } else {

                // GPOポート(カメラ)を選択不可
                $("#GPO_PORT_C").attr("disabled", "disabled");
            }
        }
    }

    // パトライトの有効/無効変更時処理
    function ChangePatlite() {
        if (document.getElementById("PATLITE_VALID_FLAG").value == "@ValidFlg.Cd.Valid") {

            //GPIO機器を選択可
            $("#GPIO_DEVICE").removeAttr("disabled");

            // 無条件で発報(パトライト)を選択可
            $("#UNCONDITIONAL_P").removeAttr("disabled");

            // GPIO機器が「DIOボード」の場合
            if (document.getElementById("GPIO_DEVICE").value == "@GpioDevice.Cd.Dio_Board") {

                // 機器名、ポート番号を書き込み可
                document.getElementById("DEVICE_NAME_P").readOnly = false;
                $("#DEVICE_NAME_P").removeClass("disable");

                document.getElementById("BIT_POS_P").readOnly = false;
                $("#BIT_POS_P").removeClass("disable");

                // GPIO機器が「SpeedWay」の場合
            } else {

                // GPOポート(パトライト)を選択可
                $("#GPO_PORT_P").removeAttr("disabled");
            }
        } else {

            // カメラが無効の場合
            if (document.getElementById("CAMERA_VALID_FLAG").value == "@ValidFlg.Cd.Invalid") {

                // GPIO機器を選択不可
                $("#GPIO_DEVICE").attr("disabled", "disabled");
            }

            // 無条件で発報(パトライト)を選択不可
            $("#UNCONDITIONAL_P").attr("disabled", "disabled");

            // GPIO機器が「DIOボード」の場合
            if (document.getElementById("GPIO_DEVICE").value == "@GpioDevice.Cd.Dio_Board") {

                // 機器名、ポート番号を書き込み不可
                document.getElementById("DEVICE_NAME_P").readOnly = true;
                $("#DEVICE_NAME_P").addClass("disable");

                document.getElementById("BIT_POS_P").readOnly = true;
                $("#BIT_POS_P").addClass("disable");

                // GPIO機器が「SpeedWay」の場合
            } else {

                // GPOポート(パトライト)を選択不可
                $("#GPO_PORT_P").attr("disabled", "disabled");
            }
        }
    }
    //20220328 ADD End Kawamoto v2.1

    //20220511 ADD Start Kawamoto
    //機種変更時処理
    function ChangeDeviceType() {

        // 「ReaderMode」のselect要素の中の全てのspan要素を削除
        var spans = $("#READER_MODE span");
        for (var i = 0; i < spans.length; i++) {
            spans.eq(i).children().unwrap();
        }
        // 対象のoptionをspan要素で囲む
        var options = $("#READER_MODE option");
        if (document.getElementById("DEVICE_TYPE").value == "@DeviceType.Cd.R700") {

            // 機種が「R700」の場合、ReaderModeの「1000:AutoSetDenseReader」は対象外
            for (var i = 0; i < options.length; i++) {
                if (options.eq(i).val() == "1000") {
                    options.eq(i).wrap("<span>");
                }
                options.eq(i).prop("selected", false);
            }

            // Readerモードの初期値「AutoSetDenseReaderDeepScan」を設定
            document.getElementById("READER_MODE").value = "@ReaderMode.Cd.AutoSetDenseReaderDeepScan";
        }

        // 「GPIO機器」のselect要素の中の全てのspan要素を削除
        var spans = $("#GPIO_DEVICE span");
        for (var i = 0; i < spans.length; i++) {
            spans.eq(i).children().unwrap();
        }
        // 対象のoptionをspan要素で囲む
        var options = $("#GPIO_DEVICE option");
        if (document.getElementById("DEVICE_TYPE").value == "@DeviceType.Cd.R420") {

            // 機種が「R420」の場合、GPIO機器の「0:SpeedWay」は対象外
            for (var i = 0; i < options.length; i++) {
                if (options.eq(i).val() == "0") {
                    options.eq(i).wrap("<span>");
                }
                options.eq(i).prop("selected", false);
            }
            options.eq(0).prop("selected", true);
        }

        //GPIO機器変更時処理
        ChangeGpioDevice();
    }

    //GPIO機器変更時処理
    function ChangeGpioDevice() {

        if (document.getElementById("GPIO_DEVICE").value == "@GpioDevice.Cd.Dio_Board") {
            // GPIO機器が「DIOボード」の場合
            document.getElementById("DIO_BOARD_GPO").innerHTML = "@Message.getMessage(CommonLabel.IO_SETTING)";
            $('#DIO_BOARD_C').show();
            $('#DIO_BOARD_P').show();

            $('#GPO_C').hide();
            $('#GPO_P').hide();

        } else {
            // GPIO機器が「SpeedWay」の場合
            document.getElementById("DIO_BOARD_GPO").innerHTML = "@Message.getMessage(CommonLabel.GPO)";
            $('#GPO_C').show();
            $('#GPO_P').show();

            $('#DIO_BOARD_C').hide();
            $('#DIO_BOARD_P').hide();
        }

        // カメラの有効/無効変更時処理
        ChangeCamera();

        // パトライトの有効/無効変更時処理
        ChangePatlite();
    }
    //20220511 ADD End Kawamoto
</script>

<div id="gate-master">
    @Using (Html.BeginForm("Index", "Gate", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    <input class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                    <input class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">
                </div>
            </div>
        </div>
        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div class="list-view">
            <div id="ListGrids" style="height:auto;padding-bottom: 20px;" />
         </div>


        '登録・更新のモーダル画面
                @<div id="modal-content" class="modal-gate">
                    <div class="popup-title-wrap">
                        <p class="Line3"></p>
                        <p class="Line4"></p>
                        <h2 id="popup-title"></h2>
                        <p class="Line3"></p>
                        <p class="Line4"></p>
                    </div>
                    @CODE
                        'モデルの値を設定
                        Dim SessionModel As GateViewModel = Session(CommonSessionKey.CREAGE_GATE)
                        If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                            Model.Add(SessionModel)
                        Else
                            Model.Add(New GateViewModel())
                        End If
                    END CODE
                    @Html.AntiForgeryToken()

                     @If Model.FirstOrDefault.GATE_DIV = GateDiv.Cd.WG Then
                         'WaveGate用
                         @<Table>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_CD)</td>
                                 <td width="20" Class="require">※</td>
                                 <td style="display:inline-block;width:340px;">@Html.EditorFor(Function(model) model.Last.GATE_CD, New With {.htmlAttributes = New With {.maxlength = 10, .style = "width:320px;", .oninput = "editHanEi(this)"}})</td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE)</td>
                                 <td width="20" Class="require" style="display:inline-block;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.GATE_VALID_FLAG, DirectCast(ViewBag.modalValidGate, IEnumerable(Of SelectListItem)), New With {.onchange = "SetControlCondition()", .style = "width: 100px;"})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_NAME)</td>
                                 <td width="20" Class="require">※</td>
                                 <td style="display:inline-block;width:340px;">@Html.EditorFor(Function(model) model.Last.GATE_NAME, New With {.htmlAttributes = New With {.maxlength = 100, .style = "width:320px;"}})</td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_PATLITE)</td>
                                 <td width="20" Class="require" style="display:inline-block;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID) </td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.PATLITE_VALID_FLAG, DirectCast(ViewBag.modalValidPat, IEnumerable(Of SelectListItem)), New With {.style = "width: 100px;"})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_IP)</td>
                                 <td width="20" Class="require"></td>
                                 <td style="display:inline-block;width:340px;">@Html.EditorFor(Function(model) model.Last.GATE_IPADDRESS, New With {.htmlAttributes = New With {.maxlength = 15, .style = "width:150px;", .oninput = "editPassword(this)"}})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_CHANNEL)</td>
                                 <td width="20" Class="require"></td>
                                 <td>@Html.DropDownListFor(Function(model) model.Last.GATE_CHANNEL, DirectCast(ViewBag.modalChannel, IEnumerable(Of SelectListItem)), New With {.style = "width: 150px;"})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_ANTENNA1)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.ANTENNA1_VALID_FLAG, DirectCast(ViewBag.modalValidAntenna1, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeAntenna1()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ANTENNA_POW)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.Last.ANTENNA1_RF_POWER, New With {.htmlAttributes = New With {.maxlength = 2, .style = "width:100px;text-align:right;", .oninput = "editNumber(this)"}}) (1～81)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_ANTENNA2)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.ANTENNA2_VALID_FLAG, DirectCast(ViewBag.modalValidAntenna2, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeAntenna2()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ANTENNA_POW)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.Last.ANTENNA2_RF_POWER, New With {.htmlAttributes = New With {.maxlength = 2, .style = "width:100px;text-align:right;", .oninput = "editNumber(this)"}}) (1～81)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_ANTENNA3)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.ANTENNA3_VALID_FLAG, DirectCast(ViewBag.modalValidAntenna3, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeAntenna3()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ANTENNA_POW)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.Last.ANTENNA3_RF_POWER, New With {.htmlAttributes = New With {.maxlength = 2, .style = "width:100px;text-align:right;", .oninput = "editNumber(this)"}}) (1～81)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_ANTENNA4)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.ANTENNA4_VALID_FLAG, DirectCast(ViewBag.modalValidAntenna4, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeAntenna4()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ANTENNA_POW)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.Last.ANTENNA4_RF_POWER, New With {.htmlAttributes = New With {.maxlength = 2, .style = "width:100px;text-align:right;", .oninput = "editNumber(this)"}}) (1～81)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_THERMO1)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.THERMO1_VALID_FLAG, DirectCast(ViewBag.modalValidThermo1, IEnumerable(Of SelectListItem)), New With {.style = "width: 100px;"})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_THERMO2)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.THERMO2_VALID_FLAG, DirectCast(ViewBag.modalValidThermo2, IEnumerable(Of SelectListItem)), New With {.style = "width: 100px;"})</td>
                             </tr>
                         </Table>
                     Else
                         '20220328 ADD Start Kawamoto v2.1
                         'SpeedWay用
                         @<Table>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_CD)</td>
                                 <td width="20" Class="require">※</td>
                                 <td style="display:inline-block;width:340px;">@Html.EditorFor(Function(model) model.Last.GATE_CD, New With {.htmlAttributes = New With {.maxlength = 10, .style = "width:320px;", .oninput = "editHanEi(this)"}})</td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE)</td>
                                 <td width="20" Class="require" style="display:inline-block;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.GATE_VALID_FLAG, DirectCast(ViewBag.modalValidGate, IEnumerable(Of SelectListItem)), New With {.onchange = "SetControlCondition()", .style = "width: 100px;"})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_NAME)</td>
                                 <td width="20" Class="require">※</td>
                                 <td style="display:inline-block;width:340px;">@Html.EditorFor(Function(model) model.Last.GATE_NAME, New With {.htmlAttributes = New With {.maxlength = 100, .style = "width:320px;"}})</td>
                                 @*20220511 ADD Start Kawamoto *@
                                 <td Class="head-col">@Message.getMessage(CommonLabel.DEVICE_TYPE)</td>
                                 <td width="20" Class="require" style="display:inline-block;"></td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.DEVICE_TYPE, DirectCast(ViewBag.modalDeviceType, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeDeviceType()", .style = "width: 100px;"})</td>
                                 @*20220511 ADD End Kawamoto *@
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_IP)</td>
                                 <td width="20" Class="require"></td>
                                 <td style="display:inline-block;width:340px;">@Html.EditorFor(Function(model) model.Last.GATE_IPADDRESS, New With {.htmlAttributes = New With {.maxlength = 15, .style = "width:150px;", .oninput = "editPassword(this)"}})</td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ANTENNA_POW)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.Last.ANTENNA1_RF_POWER, New With {.htmlAttributes = New With {.maxlength = 2, .style = "width:100px;text-align:right;", .oninput = "editNumber(this)"}}) (10～30)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_ANTENNA1)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.ANTENNA1_VALID_FLAG, DirectCast(ViewBag.modalValidAntenna1, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeAntennaSW()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.SENSITIVITY)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.Last.SENSITIVITY, New With {.htmlAttributes = New With {.maxlength = 5, .style = "width:100px;text-align:right;", .oninput = "editGateSensitivity(this)"}}) (-30～-80)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_ANTENNA2)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.ANTENNA2_VALID_FLAG, DirectCast(ViewBag.modalValidAntenna2, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeAntennaSW()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.SESSION)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.Last.SESSION, New With {.htmlAttributes = New With {.maxlength = 1, .style = "width:100px;text-align:right;", .oninput = "editNumber(this)"}}) (0～3)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_ANTENNA3)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.ANTENNA3_VALID_FLAG, DirectCast(ViewBag.modalValidAntenna3, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeAntennaSW()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.FREQUENCY)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.FirstOrDefault.FREQUENCY1, New With {.htmlAttributes = New With {.maxlength = 8, .style = "width:100px;text-align:right;", .oninput = "editNumberDicimal(this)"}}) Mhz</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_ANTENNA4)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_GATE_VALID)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:150px;">@Html.DropDownListFor(Function(model) model.Last.ANTENNA4_VALID_FLAG, DirectCast(ViewBag.modalValidAntenna4, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeAntennaSW()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:190px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.FirstOrDefault.FREQUENCY2, New With {.htmlAttributes = New With {.maxlength = 8, .style = "width:100px;text-align:right;", .oninput = "editNumberDicimal(this)"}}) Mhz</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.SEARCH_MODE)</td>
                                 <td width="20" Class="require"></td>
                                 <td style="display:inline-block;width:340px;">@Html.DropDownListFor(Function(model) model.Last.SEARCH_MODE, DirectCast(ViewBag.modalSearch, IEnumerable(Of SelectListItem)), New With {.style = "width: 250px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:170px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.FirstOrDefault.FREQUENCY3, New With {.htmlAttributes = New With {.maxlength = 8, .style = "width:100px;text-align:right;", .oninput = "editNumberDicimal(this)"}}) Mhz</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.READER_MODE)</td>
                                 <td width="20" Class="require"></td>
                                 <td style="display:inline-block;width:340px;">@Html.DropDownListFor(Function(model) model.Last.READER_MODE, DirectCast(ViewBag.modalReader, IEnumerable(Of SelectListItem)), New With {.style = "width: 250px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:170px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.FirstOrDefault.FREQUENCY4, New With {.htmlAttributes = New With {.maxlength = 8, .style = "width:100px;text-align:right;", .oninput = "editNumberDicimal(this)"}}) Mhz</td>
                             </tr>
                             @*20220511 ADD Start Kawamoto *@
                             <tr Class="popup-row">
                                 <td Class="head-col">@Message.getMessage(CommonLabel.GPIO_DEVICE)</td>
                                 <td width="20" Class="require"></td>
                                 <td style="display:inline-block;width:340px;">@Html.DropDownListFor(Function(model) model.Last.GPIO_DEVICE, DirectCast(ViewBag.modalGpioDevice, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeGpioDevice()", .style = "width: 250px;"})</td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.TAG_POPULATION)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:200px;">@Html.EditorFor(Function(model) model.Last.TAG_POPULATION, New With {.htmlAttributes = New With {.maxlength = 5, .style = "width:100px;text-align:right;", .oninput = "editNumber(this)"}}) (1～65535)</td>                         
                            @*20220511 ADD End Kawamoto *@
                             </tr>
                             <tr Class="popup-row">
                                 @*20220511 UPD Start Kawamoto *@
                                 <td Class="head-col" id="DIO_BOARD_GPO">@Message.getMessage(CommonLabel.IO_SETTING)</td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.IO_No1)</td>
                                 @*20220511 UPD End Kawamoto *@
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.CAMERA_VALID_FLAG, DirectCast(ViewBag.modalValidCamera, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangeCamera()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:70px;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.UNCONDITIONAL_C)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.UNCONDITIONAL_C, DirectCast(ViewBag.modalUnconditionalCamera, IEnumerable(Of SelectListItem)), New With {.style = "width: 100px;"})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col" style="background-color: white;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:70px;"></td>
                                 <td style="display:inline-block;width:400px;font-size:11px;">@Message.getMessage(CommonLabel.CONDITIONS_C)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col" style="background-color: white;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:70px;"></td>
                                 <td style="display:inline-block;width:50px;font-size:11px;padding-left:10px;">@Message.getMessage(CommonLabel.VALUE_INVALID)</td>
                                 <td style="display:inline-block;width:400px;font-size:11px;">@Message.getMessage(CommonLabel.CONDITIONS_INVALID_C)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col" style="background-color: white;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:70px;"></td>
                                 <td style="display:inline-block;width:50px;font-size:11px;padding-left:10px;">@Message.getMessage(CommonLabel.VALUE_VALID)</td>
                                 <td style="display:inline-block;width:400px;font-size:11px;">@Message.getMessage(CommonLabel.CONDITIONS_VALID_C)</td>
                             </tr>
                             <tr style="height:4px;"></tr>
                             <tr Class="popup-row" id="DIO_BOARD_C">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.DEVICE_NAME)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:170px;">@Html.EditorFor(Function(model) model.Last.DEVICE_NAME_C, New With {.htmlAttributes = New With {.maxlength = 60, .style = "width:150px;"}})</td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.BIT_POS)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;">@Html.EditorFor(Function(model) model.Last.BIT_POS_C, New With {.htmlAttributes = New With {.maxlength = 1, .style = "width:50px;text-align:right;", .oninput = "editNumber(this)"}}) (0～7)</td>
                             </tr>
                             <tr Class="popup-row" id="GPO_C">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.GPO_PORT)</td>
                                 <td width = "20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.GPO_PORT_C, DirectCast(ViewBag.modalGpoPortCamera, IEnumerable(Of SelectListItem)), New With {.style = "width: 100px;"})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.IO_No2)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.PATLITE_VALID_FLAG, DirectCast(ViewBag.modalValidPat, IEnumerable(Of SelectListItem)), New With {.onchange = "ChangePatlite()", .style = "width: 100px;"})</td>
                                 <td width="20" Class="require" style="display:inline-block;width:70px;"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.UNCONDITIONAL_P)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;">※</td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.UNCONDITIONAL_P, DirectCast(ViewBag.modalUnconditionalPatlite, IEnumerable(Of SelectListItem)), New With {.style = "width: 100px;"})</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col" style="background-color: white;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:70px;"></td>
                                 <td style="display:inline-block;width:400px;font-size:11px;">@Message.getMessage(CommonLabel.CONDITIONS_P)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col" style="background-color: white;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:70px;"></td>
                                 <td style="display:inline-block;width:50px;font-size:11px;padding-left:10px;">@Message.getMessage(CommonLabel.VALUE_INVALID)</td>
                                 <td style="display:inline-block;width:400px;font-size:11px;">@Message.getMessage(CommonLabel.CONDITIONS_INVALID_P)</td>
                             </tr>
                             <tr Class="popup-row">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col" style="background-color: white;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;"></td>
                                 <td width="20" Class="require" style="display:inline-block;width:70px;"></td>
                                 <td style="display:inline-block;width:50px;font-size:11px;padding-left:10px;">@Message.getMessage(CommonLabel.VALUE_VALID)</td>
                                 <td style="display:inline-block;width:400px;font-size:11px;">@Message.getMessage(CommonLabel.CONDITIONS_VALID_P)</td>
                             </tr>
                             <tr style="height:4px;"></tr>
                             <tr Class="popup-row" id="DIO_BOARD_P">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.DEVICE_NAME)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:170px;">@Html.EditorFor(Function(model) model.Last.DEVICE_NAME_P, New With {.htmlAttributes = New With {.maxlength = 60, .style = "width:150px;"}})</td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.BIT_POS)</td>
                                 <td width="20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;">@Html.EditorFor(Function(model) model.Last.BIT_POS_P, New With {.htmlAttributes = New With {.maxlength = 1, .style = "width:50px;text-align:right;", .oninput = "editNumber(this)"}}) (0～7)</td>
                             </tr>
                             <tr Class="popup-row" id="GPO_P">
                                 <td></td>
                                 <td width="20" Class="require"></td>
                                 <td Class="head-col">@Message.getMessage(CommonLabel.GPO_PORT)</td>
                                 <td width = "20" Class="require" style="display:inline-block;width:20px;"></td>
                                 <td style="display:inline-block;width:100px;">@Html.DropDownListFor(Function(model) model.Last.GPO_PORT_P, DirectCast(ViewBag.modalGpoPortPatlite, IEnumerable(Of SelectListItem)), New With {.style = "width: 100px;"})</td>
                             </tr>
                         </Table>
                         '20220328 ADD End Kawamoto v2.1
                     End If

                    @*<div Class="PopupButton" style="margin:0 auto; width:500px;"> yamada*@
                    <div Class="PopupButton" style="margin:0 auto; width:560px;">
                        <span style="float:left;margin:15px 20px 0 60px;">
                            <input id="modal-create-btn" class="defButton  modal-create-btn submit-button" formaction="~/Gate/Create" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnCreateModal_onclick()">
                        </span>
                        <span style="float:left;margin:15px 15% 0 0;">
                            <input id="modal-delete-btn" class="defButton submit-button" type="button" formaction="~/Gate/Delete" name="Delete" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="return btnDelete_onclick()">
                        </span>
                        <span style="float:left;margin:15px 0%;">
                            <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
                        </span>
                    </div>
                </div>

                @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
                        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
                        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
                        @Html.HiddenFor(Function(model) model.Last.INS_DATETIME)
                        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_CD)
                        @Html.HiddenFor(Function(model) model.Last.INS_EMPLOYEE_NAME)
                        @Html.HiddenFor(Function(model) model.Last.UPD_DATETIME)
                        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_CD)
                        @Html.HiddenFor(Function(model) model.Last.UPD_EMPLOYEE_NAME)
                        @Html.HiddenFor(Function(model) model.Last.UPD_TASK)
                        @Html.HiddenFor(Function(model) model.FirstOrDefault.GATE_DIV)
                        End Using
</div>
