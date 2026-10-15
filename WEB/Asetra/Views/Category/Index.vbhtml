@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of CategoryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelCategory.TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)

End Code
<script>
    //
    //カテゴリーマスタメンテ画面
    //
    $(function () {
        //初期表示で値・入力可否を設定した後に×の表示を更新する
        refreshCategoryClearableLater();

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            // 2019/12/24 miyaji upd start エラーメッセージ表示後、ライセンスコントローラーを呼び出すように修正
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            //エラーメッセージ表示後、ログイン画面に遷移
            @*showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/Login")/Index");*@
            // 2019/12/24 miyaji upd end
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
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessDelete") {
            //削除
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

                //貸出上限エラー区分のラジオボタンが全て未選択の場合は「しない」を選択する  Add 2020.03.04 Iwamoto
                if (!$('#LEASE_UPPER_ERR_DIV_NONE').prop('checked') && !$('#LEASE_UPPER_ERR_DIV_Y').prop('checked') && !$('#LEASE_UPPER_ERR_DIV_N').prop('checked')) {
                    $("#LEASE_UPPER_ERR_DIV_NONE").prop("checked", true);
                }

                if ($('#CAT_DIV_L').prop('checked')) {
                    //--2019/11/18 nv miyaji add start
                    // 大分類選択時、通常表示
                    $("#td-LSentaku").show();
                    $("#td-NotLSentaku").hide();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    //--2019/11/18 nv miyaji add end
                    //中分類：書き込み不可
                    document.getElementById("CAT_M_CD").readOnly = true;
                    $("#CAT_M_CD").addClass("disable");
                    //小分類：書き込み不可
                    document.getElementById("CAT_S_CD").readOnly = true;
                    $("#CAT_S_CD").addClass("disable");
                    //アラート表示「上位に準じる」使用不可
                    $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", true);
                    $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", true);
                    $("#LEASE_ALERT_DIV_F").prop("disabled", true);     // Add 2020.02.06 Iwamoto

                    //資産ID(EPC)登録方法使用可
                    $("#ASSET_ID_REG_DIV").prop("disabled", false);
                    //Upd Start 2020.02.14 Iwamoto
                    ////EPC領域サイズ使用可
                    //$("#EPC_SIZE").prop("disabled", false);
                    var assetIdRegDiv = document.getElementById("ASSET_ID_REG_DIV");
                    switch (assetIdRegDiv.value) {
                        case "@AssetIDGenerationMethod.GIAI" :
                            // GIAI
                            // --タグ領域サイズ：使用不可、096固定
                            $('#EPC_SIZE').prop('disabled', true);
                            $('#EPC_SIZE').val(@TagSize.Size096);
                            // --タグEPC事業所：使用可
                            $('#EPC_COMPANY_KEY').prop('disabled', false);
                            // --タグEPC資産タイプ：使用不可
                            $('#EPC_ASSET_TYPE').prop('disabled', true);
                            break;
                        case "@AssetIDGenerationMethod.GRAI" :
                            // GRAI
                            // --タグ領域サイズ：使用不可、096固定
                            $('#EPC_SIZE').prop('disabled', true);
                            $('#EPC_SIZE').val(@TagSize.Size096);
                            // --タグEPC事業所：使用可
                            $('#EPC_COMPANY_KEY').prop('disabled', false);
                            // --タグEPC資産タイプ：使用可
                            $('#EPC_ASSET_TYPE').prop('disabled', false);
                            break;
                        default:
                            // 以外
                            // --タグ領域サイズ：使用可
                            $('#EPC_SIZE').prop('disabled', false);
                            // --タグEPC事業所：使用不可
                            $('#EPC_COMPANY_KEY').prop('disabled', true);
                            // --タグEPC資産タイプ：使用不可
                            $('#EPC_ASSET_TYPE').prop('disabled', true);
                            break;
                    }
                    //Upd End 2020.02.14 Iwamoto

                } else if ($('#CAT_DIV_M').prop('checked')) {
                    //--2019/11/18 nv miyaji add start
                    // 中分類選択時、大分類をコンボボックス表示にする
                    $("#td-LSentaku").hide();
                    $("#td-NotLSentaku").show();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    //--2019/11/18 nv miyaji add end
                    //中分類：書き込み可
                    document.getElementById("CAT_M_CD").readOnly = false;
                    $("#CAT_M_CD").removeClass("disable");
                    //小分類：書き込み不可
                    document.getElementById("CAT_S_CD").readOnly = true;
                    $("#CAT_S_CD").addClass("disable");
                    $("#CAT_S_CD").value = "test";
                    //アラート表示「上位に準じる」使用可
                    $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", false);
                    $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", false);
                    $("#LEASE_ALERT_DIV_F").prop("disabled", false);    // Add 2020.02.06 Iwamoto
                    //資産ID(EPC)登録方法使用不可
                    $("#ASSET_ID_REG_DIV").prop("disabled", true);
                    //EPC領域サイズ使用不可
                    $("#EPC_SIZE").prop("disabled", true);
                    //タグEPC事業所：使用不可
                    $('#EPC_COMPANY_KEY').prop('disabled', true);       // Add 2020.02.14 Iwamoto
                    //タグEPC資産タイプ：使用不可
                    $('#EPC_ASSET_TYPE').prop('disabled', true);        // Add 2020.02.14 Iwamoto

                } else if ($('#CAT_DIV_S').prop('checked')) {
                    //--2019/11/18 nv miyaji add start
                    // 小分類選択時、大分類・中分類をコンボボックス表示にする
                    $("#td-LSentaku").hide();
                    $("#td-NotLSentaku").show();
                    $("#td-SSentaku").show();
                    $("#td-NotSSentaku").hide();
                    //--2019/11/18 nv miyaji add end
                    //中分類：書き込み可
                    document.getElementById("CAT_M_CD").readOnly = false;
                    $("#CAT_M_CD").removeClass("disable");
                    //小分類：書き込み可
                    document.getElementById("CAT_S_CD").readOnly = false;
                    $("#CAT_S_CD").removeClass("disable");
                    //アラート表示「上位に準じる」使用可
                    $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", false);
                    $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", false);
                    $("#LEASE_ALERT_DIV_F").prop("disabled", false);    // Add 2020.02.06 Iwamoto
                    //資産ID(EPC)登録方法使用不可
                    $("#ASSET_ID_REG_DIV").prop("disabled", true);
                    //EPC領域サイズ使用不可
                    $("#EPC_SIZE").prop("disabled", true);
                    //タグEPC事業所：使用不可
                    $('#EPC_COMPANY_KEY').prop('disabled', true);       // Add 2020.02.14 Iwamoto
                    //タグEPC資産タイプ：使用不可
                    $('#EPC_ASSET_TYPE').prop('disabled', true);        // Add 2020.02.14 Iwamoto
                }

                //削除ボタンを非表示
                $("#modal-delete-btn").css("display", "none");

                //登録画面を表示
                ShowModalWindow()
                //$('.create-btn').trigger("click")
            } else {
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

                //各区分コード書き込み不可
                document.getElementById("CAT_L_CD").readOnly = true;
                $("#CAT_L_CD").addClass("disable");
                document.getElementById("CAT_M_CD").readOnly = true;
                $("#CAT_M_CD").addClass("disable");
                document.getElementById("CAT_S_CD").readOnly = true;
                $("#CAT_S_CD").addClass("disable");

                //貸出上限エラー区分のラジオボタンが全て未選択の場合は「しない」を選択する  Add 2020.03.04 Iwamoto
                if (!$('#LEASE_UPPER_ERR_DIV_NONE').prop('checked') && !$('#LEASE_UPPER_ERR_DIV_Y').prop('checked') && !$('#LEASE_UPPER_ERR_DIV_N').prop('checked')) {
                    $("#LEASE_UPPER_ERR_DIV_NONE").prop("checked", true);
                }

                if ($('#CAT_DIV_L').prop('checked')) {
                    //--2019/11/18 nv miyaji add start
                    // 大分類選択時、通常表示
                    $("#td-LSentaku").show();
                    $("#td-NotLSentaku").hide();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    //--2019/11/18 nv miyaji add end
                    //活性・非活性
                    $('#CAT_DIV_L').prop('disabled', false);
                    $('#CAT_DIV_M').prop('disabled', true);
                    $('#CAT_DIV_S').prop('disabled', true);
                    //アラート表示「上位に準じる」使用不可
                    $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", true);
                    $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", true);
                    $("#LEASE_ALERT_DIV_F").prop("disabled", true);     // Add 2020.02.06 Iwamoto

                    // 2019/09/26 nv miyaji update start
                    //資産ID(EPC)登録方法の書き込み可不可設定
                    if("@ViewBag.AssetIdDivReg" == "False"){
                        $("#ASSET_ID_REG_DIV").prop("disabled", true);
                        $("#EPC_SIZE").prop("disabled", true);
                        $('#EPC_COMPANY_KEY').prop('disabled', true);   // Add 2020.02.13 Iwamoto
                        $('#EPC_ASSET_TYPE').prop('disabled', true);    // Add 2020.02.13 Iwamoto
                    } else {
                        //資産ID(EPC)登録方法使用可
                        $("#ASSET_ID_REG_DIV").prop("disabled", false);
                        var assetIdRegDiv = document.getElementById("ASSET_ID_REG_DIV");
                        switch (assetIdRegDiv.value) {
                            case "@AssetIDGenerationMethod.GIAI" :
                                // GIAI
                                // --タグ領域サイズ：使用不可、096固定
                                $('#EPC_SIZE').prop('disabled', true);
                                $('#EPC_SIZE').val(@TagSize.Size096);
                                // --タグEPC事業所：使用可
                                $('#EPC_COMPANY_KEY').prop('disabled', false);
                                // --タグEPC資産タイプ：使用不可
                                $('#EPC_ASSET_TYPE').prop('disabled', true);
                                break;
                            case "@AssetIDGenerationMethod.GRAI" :
                                // GRAI
                                // --タグ領域サイズ：使用不可、096固定
                                $('#EPC_SIZE').prop('disabled', true);
                                $('#EPC_SIZE').val(@TagSize.Size096);
                                // --タグEPC事業所：使用可
                                $('#EPC_COMPANY_KEY').prop('disabled', false);
                                // --タグEPC資産タイプ：使用可
                                $('#EPC_ASSET_TYPE').prop('disabled', false);
                                break;
                            default:
                                // 以外
                                // --タグ領域サイズ：使用可
                                $('#EPC_SIZE').prop('disabled', false);
                                // --タグEPC事業所：使用不可
                                $('#EPC_COMPANY_KEY').prop('disabled', true);
                                // --タグEPC資産タイプ：使用不可
                                $('#EPC_ASSET_TYPE').prop('disabled', true);
                                break;
                        }
                    }
                    ////資産ID(EPC)登録方法使用可
                    //$("#ASSET_ID_REG_DIV").prop("disabled", false);
                    ////EPC領域サイズ使用可
                    //$("#EPC_SIZE").prop("disabled", false);
                    // 2019/09/26 nv miyaji update end

                } else if ($('#CAT_DIV_M').prop('checked')) {
                    // Upd Start 2020.03.09 Iwamoto
                    ////--2019/11/18 nv miyaji add start
                    //// 中分類選択時、大分類をコンボボックス表示にする
                    //$("#td-LSentaku").hide();
                    //$("#td-NotLSentaku").show();
                    //$("#td-SSentaku").hide();
                    //$("#td-NotSSentaku").show();
                    ////--2019/11/18 nv miyaji add end
                    // 各分類コンボボックスを非表示にする
                    $("#td-LSentaku").show();
                    $("#td-NotLSentaku").hide();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    // Upd End 2020.03.09 Iwamoto
                    //活性・非活性
                    $('#CAT_DIV_L').prop('disabled', true);
                    $('#CAT_DIV_M').prop('disabled', false);
                    $('#CAT_DIV_S').prop('disabled', true);
                    //アラート表示「上位に準じる」使用可
                    $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", false);
                    $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", false);
                    $("#LEASE_ALERT_DIV_F").prop("disabled", false);    // Add 2020.02.06 Iwamoto
                    //資産ID(EPC)登録方法使用不可
                    $("#ASSET_ID_REG_DIV").prop("disabled", true);
                    //EPC領域サイズ使用不可
                    $("#EPC_SIZE").prop("disabled", true);
                    //タグEPC事業所：使用不可
                    $('#EPC_COMPANY_KEY').prop('disabled', true);
                    //タグEPC資産タイプ：使用不可
                    $('#EPC_ASSET_TYPE').prop('disabled', true);

                } else if ($('#CAT_DIV_S').prop('checked')) {
                    // Upd Start 2020.03.09 Iwamoto
                    ////--2019/11/18 nv miyaji add start
                    //// 小分類選択時、大分類・中分類をコンボボックス表示にする
                    //$("#td-LSentaku").hide();
                    //$("#td-NotLSentaku").show();
                    //$("#td-SSentaku").show();
                    //$("#td-NotSSentaku").hide();
                    ////--2019/11/18 nv miyaji add end
                    // 各分類コンボボックスを非表示にする
                    $("#td-LSentaku").show();
                    $("#td-NotLSentaku").hide();
                    $("#td-SSentaku").hide();
                    $("#td-NotSSentaku").show();
                    // Upd End 2020.03.09 Iwamoto
                    //活性・非活性
                    $('#CAT_DIV_L').prop('disabled', true);
                    $('#CAT_DIV_M').prop('disabled', true);
                    $('#CAT_DIV_S').prop('disabled', false);
                    //アラート表示「上位に準じる」使用可
                    $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", false);
                    $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", false);
                    $("#LEASE_ALERT_DIV_F").prop("disabled", false);    // Add 2020.02.06 Iwamoto
                    //資産ID(EPC)登録方法使用不可
                    $("#ASSET_ID_REG_DIV").prop("disabled", true);
                    //EPC領域サイズ使用不可
                    $("#EPC_SIZE").prop("disabled", true);
                    //タグEPC事業所：使用不可
                    $('#EPC_COMPANY_KEY').prop('disabled', true);
                    //タグEPC資産タイプ：使用不可
                    $('#EPC_ASSET_TYPE').prop('disabled', true);
                }

                //削除ボタンを表示
                $("#modal-delete-btn").css("display", "block");

                //変更画面を表示
                ShowModalWindow()
                //$('.update-btn').trigger("click")
            }
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        if ("@Model.FirstOrDefault.GridInfo("CAT_DIV").DispFlag" === "1") {
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
        }

        btnCsvImportEnabled(true)       // 2022/7/25 sv sato add

        // 列定義
        let columns = [
            { id: "@ColKey.Category.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.CHECK_SELECT", 40), sortOrder: -2, formatter: DepCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.Category.CAT_DIV", name: "@Message.getMessage(CommonLabel.LABEL_CAT_DIV)", field: "CAT_DIV", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.CAT_DIV", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_DIV").DispFlag", formatter: CatDivFormatter},
            { id: "@ColKey.Category.CAT_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_CD)", field: "CAT_L_CD", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.CAT_CD", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("CAT_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_CD").DispFlag", formatter: CatCdFormatter},
            { id: "@ColKey.Category.CAT_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CAT_NAME)", field: "CAT_NAME", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.CAT_NAME", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("CAT_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_NAME").DispFlag", formatter: CatNameFormatter},
            { id: "@ColKey.Category.LEASE_LIMIT_ALERT_DIV", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_ALERT)", field: "LEASE_LIMIT_ALERT", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.LEASE_LIMIT_ALERT_DIV", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_LIMIT_ALERT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_LIMIT_ALERT").DispFlag", formatter: LeaseLimitAlertDivFormatter},
            { id: "@ColKey.Category.LEASE_LIMIT_BEF_DAYS", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)@Message.getMessage(CommonLabel.LABEL_ALERT)?@Message.getMessage(CommonLabel.LABEL_BEF_DAYS)", field: "BEF_DAYS", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.LEASE_LIMIT_BEF_DAYS", 240), sortOrder:  "@Model.FirstOrDefault.GridInfo("BEF_DAYS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BEF_DAYS").DispFlag", formatter: LeaseLimitBefDaysFormatter},
            { id: "@ColKey.Category.GENERIC_LIMIT_ALERT_DIV", name: "@Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT_ALERT)", field: "GENERIC_LIMIT_ALERT", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.GENERIC_LIMIT_ALERT_DIV", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("GENERIC_LIMIT_ALERT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("GENERIC_LIMIT_ALERT").DispFlag", formatter: GenericLimitAlertDivFormatter},
            { id: "@ColKey.Category.GENERIC_LIMIT_BEF_DAYS", name: "@Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)@Message.getMessage(CommonLabel.LABEL_ALERT)?@Message.getMessage(CommonLabel.LABEL_BEF_DAYS)", field: "GENERIC_LIMIT_BEF_DAYS", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.GENERIC_LIMIT_BEF_DAYS", 240), sortOrder:  "@Model.FirstOrDefault.GridInfo("GENERIC_LIMIT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("GENERIC_LIMIT").DispFlag", formatter: GenericLimitBefDaysFormatter},
            { id: "@ColKey.Category.LEASE_ALERT_DIV", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_ALERT)", field: "LEASE_ALERT", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.LEASE_ALERT_DIV", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_ALERT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_ALERT").DispFlag", formatter: LeaseAlertDivFormatter},
            { id: "@ColKey.Category.LEASE_ALERT_CNT", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_ALERT_CNT)", field: "LEASE_ALERT_CNT", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.LEASE_ALERT_CNT", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_ALERT_CNT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_ALERT_CNT").DispFlag", formatter: LeaseAlertCntFormatter},
            { id: "@ColKey.Category.LEASE_UPPER_LIMIT", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT)", field: "LEASE_UPPER_LIMIT", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.LEASE_UPPER_LIMIT", 100), sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_UPPER_LIMIT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_UPPER_LIMIT").DispFlag", formatter: LeaseUpperErrDivFormatter},
            { id: "@ColKey.Category.LEASE_UPPER_ERR_DIV", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT_ALERT)", field: "LEASE_UPPER_LIMIT_ALERT", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.LEASE_UPPER_ERR_DIV", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("LEASE_UPPER_LIMIT_ALERT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_UPPER_LIMIT_ALERT").DispFlag", formatter: LeaseUpperLimitFormatter},
            { id: "@ColKey.Category.ASSET_ID_REG_DIV", name: "@Message.getMessage(CommonLabelCategory.LABEL_ASSET_ID_REG_DIV)", field: "ASSET_ID_REG_DIV", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.ASSET_ID_REG_DIV", 240), sortOrder:  "@Model.FirstOrDefault.GridInfo("ASSET_ID_REG_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_ID_REG_DIV").DispFlag", formatter: AssetIdRegDivFormatter},
            { id: "@ColKey.Category.EPC_SIZE", name: "@Message.getMessage(CommonLabelCategory.LABEL_EPC_SIZE)", field: "EPC_SIZE", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.EPC_SIZE", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("EPC_SIZE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("EPC_SIZE").DispFlag", formatter: EpcSizeFormatter},
            { id: "@ColKey.Category.EPC_COMPANY_KEY", name: "@Message.getMessage(CommonLabel.LABEL_EPC_COMPANY)", field: "EPC_COMPANY", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.EPC_COMPANY_KEY", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("EPC_COMPANY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("EPC_COMPANY").DispFlag", formatter: EpcCompanyKetNameFormatter},
            { id: "@ColKey.Category.EPC_ASSET_TYPE", name: "@Message.getMessage(CommonLabel.LABEL_EPC_ASSET_TYPE)", field: "EPC_ASSET_TYPE", width: getColumnWidth("@DispId.CATEGORY", "@ColKey.Category.EPC_ASSET_TYPE", 200), sortOrder:  "@Model.FirstOrDefault.GridInfo("EPC_ASSET_TYPE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("EPC_ASSET_TYPE").DispFlag", formatter: EpcAssetTypeFormatter},
        ]

        // Gridのオプション機能の設定
        let options = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: false,                      //Gridの高さの自動調整可否（ヘッダー固定のためfalse）
            editable: false,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列と変更列を加算
        }

        let data = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.CATEGORY", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, "@DispId.CATEGORY", fixCol);
        }

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

    });

    // 分類区分用formatter
    function CatDivFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";


        if (String(dataContext.CAT_DIV) === "@CAT_DIV.CategoryL") {
            status = "@Message.getMessage(CommonLabel.VALUE_CAT_L)"
        } else if (String(dataContext.CAT_DIV) === "@CAT_DIV.CategoryM") {
            status = "@Message.getMessage(CommonLabel.VALUE_CAT_M)"
        } else if (String(dataContext.CAT_DIV) === "@CAT_DIV.CategoryS") {
            status = "@Message.getMessage(CommonLabel.VALUE_CAT_S)"
        }

        return status;
    }

    // 分類コード用formatter
    function CatCdFormatter(row, cell, value, columnDef, dataContext) {
        let catCd = dataContext.CAT_L_CD.trim();

        let catM = convertToEmptyIfNull(dataContext.CAT_M_CD.trim())
        if (catM != '') {
            catCd = catCd + '-' + catM
        }

        let catS = convertToEmptyIfNull(dataContext.CAT_S_CD.trim())
        if (catS != '') {
            catCd = catCd + '-' + catS
        }
        return catCd
    }

    // 分類名用formatter
    function CatNameFormatter(row, cell, value, columnDef, dataContext) {
        let catLName = convertToEmptyIfNull(dataContext.CAT_L_NAME).trim()
        let catMName = convertToEmptyIfNull(dataContext.CAT_M_NAME).trim()
        let catSName = convertToEmptyIfNull(dataContext.CAT_S_NAME).trim()
        let catName = catLName

        switch(dataContext.CAT_DIV) {
            case "@CAT_DIV.CategoryM":
                if (catName.length != 0) {
                    catName += ' - '
                }
                catName += catMName
                break;

            case "@CAT_DIV.CategoryS":
                if (catName.length != 0) {
                    catName += ' - '
                }
                catName += catMName
                if (catName.length != 0) {
                    catName += ' - '
                }
                catName += catSName
                break;

        }

        return catName
    }

    // リース期限アラート表示用formatter
    function LeaseLimitAlertDivFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";
        switch(dataContext.LEASE_LIMIT_ALERT_DIV) {
            case "@LeaseLimitAlertDiv.Front":
                status = "@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_F)"
                break;

            case "@LeaseLimitAlertDiv.No":
                status = "@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_N)"
                break;

            case "@LeaseLimitAlertDiv.Yes":
                status = "@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_Y)"
                break;
        }
        return status
    }

    //リース期限アラート？日前用formatter
    function LeaseLimitBefDaysFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(dataContext.LEASE_LIMIT_BEF_DAYS) != '') {
            return '<div style="text-align:right; width:100%;">' + dataContext.LEASE_LIMIT_BEF_DAYS +'</div>'
        }
    }

    //貸出アラート表示用formatter
    function GenericLimitAlertDivFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";
        switch (dataContext.GENERIC_LIMIT_ALERT_DIV) {
            case "@GenericLimitAlertDiv.Front":
                status = "@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_F)"
                break;
            case "@GenericLimitAlertDiv.No":
                status = "@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_N)"
                break;
            case "@GenericLimitAlertDiv.Yes":
                status = "@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_Y)"
                break;
        }
        return status
    }

    //○○期限何日前用formatter
    function GenericLimitBefDaysFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(dataContext.GENERIC_LIMIT_BEF_DAYS) != '') {
            return '<div style="text-align:right; width:100%;">' + dataContext.GENERIC_LIMIT_BEF_DAYS +'</div>'
        }
    }

    //貸出アラート区分用formatter
    function LeaseAlertDivFormatter(row, cell, value, columnDef, dataContext) {
        let leaseAlertDivNm = ''
        switch(dataContext.LEASE_ALERT_DIV) {
            case "@LeaseAlertDiv.Front":
                leaseAlertDivNm = "@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_F)"
                break;
            case "@LeaseAlertDiv.Yes":
                leaseAlertDivNm = "@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_Y)"
                break;
            case "@LeaseAlertDiv.No":
                leaseAlertDivNm = "@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_N)"
                break;
        }
        return leaseAlertDivNm
    }

    //貸出アラート回数用formatter
    function LeaseAlertCntFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(dataContext.LEASE_ALERT_CNT) != '') {
            return '<div style="text-align:right; width:100%;">' + dataContext.LEASE_ALERT_CNT +'</div>'
        }
    }

    //貸出上限数用formatter
    function LeaseUpperErrDivFormatter(row, cell, value, columnDef, dataContext) {
        let leaseUpperErrDivNm = ''
        switch(dataContext.LEASE_UPPER_ERR_DIV) {
            case "@LeaseUpperErrDiv.Yes":
                leaseUpperErrDivNm = "@Message.getMessage(CommonLabel.VALUE_LEASE_UPPER_ERR_DIV_Y)"
                break;
            case "@LeaseUpperErrDiv.No":
                leaseUpperErrDivNm = "@Message.getMessage(CommonLabel.VALUE_LEASE_UPPER_ERR_DIV_N)"
                break;
            default:
                leaseUpperErrDivNm = "@Message.getMessage(CommonLabel.VALUE_LEASE_UPPER_ERR_DIV_NONE)"
                break;
        }
        return leaseUpperErrDivNm
    }

    //貸出上限エラー区分用formatter
    function LeaseUpperLimitFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(dataContext.LEASE_UPPER_LIMIT) != '') {
            return '<div style="text-align:right; width:100%;">' + dataContext.LEASE_UPPER_LIMIT +'</div>'
        }
    }

    //資産ID登録方法用formatter
    function AssetIdRegDivFormatter(row, cell, value, columnDef, dataContext) {
        let assetIdRegDivNm = ''
        switch(String(value)) {
            case "@AssetIDGenerationMethod.None":
                assetIdRegDivNm = "@Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_NONE)"
                break;
            case "@AssetIDGenerationMethod.AssetNo":
                assetIdRegDivNm = "@Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_ASSET_NO)"
                break;
            case "@AssetIDGenerationMethod.Auto":
                assetIdRegDivNm = "@Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_AUTO)"
                break;
            case "@AssetIDGenerationMethod.Inp":
                assetIdRegDivNm = "@Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_INP)"
                break;
            case "@AssetIDGenerationMethod.GIAI":
                assetIdRegDivNm = "@Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_GIAI)"
                break;
            case "@AssetIDGenerationMethod.GRAI":
                assetIdRegDivNm = "@Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_GRAI)"
                break;
        }
        return assetIdRegDivNm
    }

    //EPC領域サイズ用formatter
    function EpcSizeFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        switch(String(dataContext.EPC_SIZE)) {
            case "@TagSize.Size064":
                status = "@Message.getMessage(CommonLabel.VALUE_EPC_SIZE_064)"
                break;
            case "@TagSize.Size096":
                status = "@Message.getMessage(CommonLabel.VALUE_EPC_SIZE_096)"
                break;
            case "@TagSize.Size128":
                status = "@Message.getMessage(CommonLabel.VALUE_EPC_SIZE_128)"
                break;
            case "@TagSize.Size256":
                status = "@Message.getMessage(CommonLabel.VALUE_EPC_SIZE_256)"
                break;
            case "@TagSize.Size512":
                status = "@Message.getMessage(CommonLabel.VALUE_EPC_SIZE_512)"
                break;
        }
        if (convertToEmptyIfNull(status) != '') {
            return '<div style="text-align:right; width:100%;">' + status +'</div>'
        }
    }

    //EPC事業所キー用formatter
    function EpcCompanyKetNameFormatter(row, cell, value, columnDef, dataContext) {
        return dataContext.EPC_COMPANY_KEY_NAME
    }

    //EPC資産タイプ用formatter
    function EpcAssetTypeFormatter(row, cell, value, columnDef, dataContext) {
        return dataContext.EPC_ASSET_TYPE
    }

    //選択用formatter
    function DepCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.CAT_DIV) + ',' + convertToEmptyIfNull(dataContext.CAT_L_CD) + ',' + convertToEmptyIfNull(dataContext.CAT_M_CD) + ',' + convertToEmptyIfNull(dataContext.CAT_S_CD)
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;">' +
               '<input type="checkbox" class="slick-checkbox" ' +
               'value="' + key + '" '+
               (value == 1 ? 'checked="checked"' : '') + ' />' +
               '</div>';
    }

    //CSVデータ取込画面　取込開始ボタンクリック処理
    function btnCsvImportStart_onclick() {
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
                //i行目のj列目のセルを削除
                tblTbody.rows["@DataInport.DISP_ROW_COUNT"].remove();
            }

        }

        //CSVデータを取得し表示する(Ajax非同期通信)
        var $form = $(this).closest('form')
        var formData = new FormData($form.get(0));
        formData.append("CsvFile", $('#CsvFile')[0].files[0]);

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Category")/ImportCsvFile',
            data: formData,
            dataType: 'json',
            processData: false,
            contentType: false,
            cache: false,
            success: function (data, dataType) {
                this;

                switch (data[0].ResultCd) {
                    case @DataInport.DataInportResultCd.OK:
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTOK)";
                        break;
                    case @DataInport.DataInportResultCd.NoSelect:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SELECT)";
                        break;
                    case @DataInport.DataInportResultCd.Empty:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_EMPTY)";
                        break;
                    case @DataInport.DataInportResultCd.NotCsv:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_NOTCSV)";
                        break;
                    case @DataInport.DataInportResultCd.KoumokuSuuErr:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU)";
                        break;
                    case @DataInport.DataInportResultCd.DataNG:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTNG)";

                        for (var icnt = 0; icnt < data.length; icnt++) {
                            if( icnt < "@DataInport.DISP_ROW_COUNT"){
                                //icnt行目の各項目に値設定
                                var dispErrRowIdx = "";
                                if (data[icnt].ErrRowIdx != null) {
                                    dispErrRowIdx = data[icnt].ErrRowIdx;
                                }
                                //tblTbody.rows[icnt].cells[0].innerHTML =data[icnt].ErrRowIdx;
                                tblTbody.rows[icnt].cells[0].innerHTML =dispErrRowIdx;
                                tblTbody.rows[icnt].cells[0].title = dispErrRowIdx;
                                tblTbody.rows[icnt].cells[1].innerHTML =data[icnt].ErrKoumoku;
                                tblTbody.rows[icnt].cells[1].title = data[icnt].ErrKoumoku;
                                tblTbody.rows[icnt].cells[2].innerHTML =data[icnt].ErrGaiyou;
                                tblTbody.rows[icnt].cells[2].title =data[icnt].ErrGaiyou;
                            }else{
                                var dispErrRowIdx = "";
                                if (data[icnt].ErrRowIdx != null) {
                                    dispErrRowIdx = data[icnt].ErrRowIdx;
                                }
                                //$("#csv-err-ichiran").append('<tr><td class="col1">'+ data[icnt].ErrRowIdx +'</td><td class="col2" >'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" >'+ data[icnt].ErrGaiyou +'</td></tr>');
                                //$("#csv-err-ichiran").append('<tr><td class="col1">'+ dispErrRowIdx +'</td><td class="col2" >'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" >'+ data[icnt].ErrGaiyou +'</td></tr>');
                                $("#csv-err-ichiran").append('<tr><td class="col1" title=' + dispErrRowIdx + '>'+ dispErrRowIdx +'</td><td class="col2" title=' + data[icnt].ErrKoumoku + '>'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" title=' + data[icnt].ErrGaiyou + '>'+ data[icnt].ErrGaiyou +'</td></tr>');
                            }

                        }

                        break;
                    case @DataInport.DataInportResultCd.LoginSession:
                        // ログインセッション切れの場合
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_LOGININFO_EMPTY)";
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
                this;
                document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SOUTEIGAI)";
                btnCsvImportEnabled(false)           // ADD sv sato 2022/7/25
            }
        });
    }

    //2022/7/25 sv sato add start
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

    //CSVデータ取込ボタンクリック処理
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
            if( i < "@DataInport.DISP_ROW_COUNT"){
                // tr内のtdをループ。cellsコレクションで行内セル位置取得。
                for (var j=0, colLen=tblTbody.rows[i].cells.length ; j<colLen; j++) {
                    //i行目のj列目のセルをクリア
                    tblTbody.rows[i].cells[j].innerHTML ="　";
                    tblTbody.rows[i].cells[j].title ="　";
                }
            }else{
                //i行目のj列目のセルを削除
                tblTbody.rows["@DataInport.DISP_ROW_COUNT"].remove();
            }

        }

        //CSV取込画面を表示
        ShowCsvModalWindow()
    }

    //全データCSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.CSVEXPORT_ALL_CONFIRM)", "@Url.Content("~/Category")/CsvExport");

    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        //画面ブロックを表示
        showLoading();
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

    //登録ボタンクリック処理
    function btnCreate_onclick() {
        //値・入力可否を設定した後に×の表示を更新する
        refreshCategoryClearableLater();
        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

        // EPC事業所キードロップダウンリスト作成                     // Add 2020.02.13 Iwamoto
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Category")/ChangeAssetIdRegDiv',
            data: { selectAssetIdRegDiv: @ViewBag.AssetIDRegDivDef },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var epcCompanyKey = document.getElementById("EPC_COMPANY_KEY");
                    epcCompanyKey.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    document.getElementById("EPC_COMPANY_KEY").appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].Cd;
                        op.text = data.Data[i].Name;
                        document.getElementById("EPC_COMPANY_KEY").appendChild(op);
                    }
                }
                // EPC事業所キーデータ取得
                document.getElementById("EPC_COMPANY_KEY").value = data.epcCompanyKeyDropdownData;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });

        //カテゴリ大分類コードを書き込み可
        document.getElementById("CAT_L_CD").readOnly = false;
        $("#CAT_L_CD").removeClass("disable");

        //カテゴリ中分類、小分類コードを書き込み不可
        document.getElementById("CAT_M_CD").readOnly = true;
        $("#CAT_M_CD").addClass("disable");
        document.getElementById("CAT_S_CD").readOnly = true;
        $("#CAT_S_CD").addClass("disable");

        //アラート表示「上位に準じる」使用不可、しないを選択
        $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", true);
        $("#LEASE_LIMIT_ALERT_DIV_N").prop("checked", true);
        $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", true);
        $("#GENERIC_LIMIT_ALERT_DIV_N").prop("checked", true);
        $("#LEASE_ALERT_DIV_F").prop("disabled", true);             // Add 2020.02.06 Iwamoto
        $("#LEASE_ALERT_DIV_N").prop("checked", true);              // Add 2020.02.06 Iwamoto

        // 初期値設定
        $('[id=CAT_DIV_L]').prop('checked', true);
        $('[id=CAT_DIV_M]').prop('checked', false);
        $('[id=CAT_DIV_S]').prop('checked', false);
        document.getElementById("CAT_L_CD").value = "";
        document.getElementById("CAT_M_CD").value = "";
        document.getElementById("CAT_S_CD").value = "";
        document.getElementById("CAT_NAME").value = "";
        document.getElementById("LEASE_LIMIT_BEF_DAYS").value = "";
        document.getElementById("GENERIC_LIMIT_BEF_DAYS").value = "";
        document.getElementById("LEASE_ALERT_CNT").value = "";      // Add 2020.02.06 Iwamoto
        $("#LEASE_UPPER_ERR_DIV_NONE").prop("checked", true);       // Add 2020.03.04 Iwamoto
        $("#LEASE_UPPER_ERR_DIV_Y").prop("checked", false);         // Add 2020.03.04 Iwamoto
        $("#LEASE_UPPER_ERR_DIV_N").prop("checked", false);         // Add 2020.03.04 Iwamoto
        document.getElementById("LEASE_UPPER_LIMIT").value = "";    // Add 2020.02.06 Iwamoto
        document.getElementById("ASSET_ID_REG_DIV").value = @ViewBag.AssetIDRegDivDef;
        document.getElementById("EPC_ASSET_TYPE").value = "";       // Add 2020.02.06 Iwamoto
        document.getElementById("CAT_L_NAME").value = "";
        document.getElementById("CAT_M_NAME").value = "";
        document.getElementById("CAT_S_NAME").value = "";

        // 資産ID作成方法による設定
        var assetIdRegDiv = $('#ASSET_ID_REG_DIV').val();
        switch (assetIdRegDiv) {
            case "@AssetIDGenerationMethod.GIAI":
                // GIAI
                // --タグ領域サイズ：使用不可、096固定
                $('#EPC_SIZE').prop('disabled', true);
                $('#EPC_SIZE').val(@TagSize.Size096);
                // --タグEPC事業所：使用可
                $('#EPC_COMPANY_KEY').prop('disabled', false);
                // --タグEPC資産タイプ：使用不可
                $('#EPC_ASSET_TYPE').prop('disabled', true);
                break;
            case "@AssetIDGenerationMethod.GRAI":
                // GRAI
                // --タグ領域サイズ：使用不可、096固定
                $('#EPC_SIZE').prop('disabled', true);
                $('#EPC_SIZE').val(@TagSize.Size096);
                // --タグEPC事業所：使用可
                $('#EPC_COMPANY_KEY').prop('disabled', false);
                // --タグEPC資産タイプ：使用可
                $('#EPC_ASSET_TYPE').prop('disabled', false);
                break;
            default:
                // 以外
                // --タグ領域サイズ：使用可、DB初期値
                $('#EPC_SIZE').prop('disabled', false);
                $('#EPC_SIZE').val(@ViewBag.EpcSizeDef);
                // --タグEPC事業所：使用不可
                $('#EPC_COMPANY_KEY').prop('disabled', true);
                // --タグEPC資産タイプ：使用不可
                $('#EPC_ASSET_TYPE').prop('disabled', true);
                break;
        }

        //削除ボタンを非表示
        $("#modal-delete-btn").css("display", "none");


        //--2019/11/18 nv miyaji add start
        // 大分類選択時、通常表示
        $("#td-LSentaku").show();
        $("#td-NotLSentaku").hide();
        $("#td-SSentaku").hide();
        $("#td-NotSSentaku").show();
        //--2019/11/18 nv miyaji add end

        //画面を初期化する
        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_UPDATE_CLICK") {
            //エラー情報のクリア
            $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        }

        //登録ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_CREATE_CLICK";

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //変更ボタンクリック処理
    function btnEdit_onclick() {
        //値・入力可否を設定した後に×の表示を更新する
        refreshCategoryClearableLater();
        //テーブルの選択行を取得する。
        var text = [];
        //--2019/11/21 nv miyaji upd start
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                text.push(checkBoxes[i].value);
            }
        }
        //$("[type=checkbox]:checked").each(function () {
        //    text.push($(this).val());
        //});
        //--2019/11/21 nv miyaji upd end
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

        //各区分コード書き込み不可
        document.getElementById("CAT_L_CD").readOnly = true;
        $("#CAT_L_CD").addClass("disable");
        document.getElementById("CAT_M_CD").readOnly = true;
        $("#CAT_M_CD").addClass("disable");
        document.getElementById("CAT_S_CD").readOnly = true;
        $("#CAT_S_CD").addClass("disable");

        // 2019/11/19 nv miyaji 各区分コンボボックスを非表示にする
        $("#td-LSentaku").show();
        $("#td-NotLSentaku").hide();
        $("#td-SSentaku").hide();
        $("#td-NotSSentaku").show();

        @* 2021/09/27 nv iwamoto update start *@
        ////削除ボタンを表示
        //$("#modal-delete-btn").css("display", "block");
        // 権限による処理区分が「編集・照会可能」でない場合
        if("@ViewBag.AuthProcFlg" != "@AuthProcFlg.All"){
            // 変更ボタンを非表示
            $("#modal-create-btn").css("display", "none");
            // 削除ボタンを非表示
            $("#modal-delete-btn").css("display", "none");
        }else{
            //削除ボタンを表示
            $("#modal-delete-btn").css("display", "block");
        }
        @* 2021/09/27 nv iwamoto update end *@

        //画面を初期化する
        document.getElementById("CAT_L_CD").value = ""
        document.getElementById("CAT_M_CD").value = ""
        document.getElementById("CAT_S_CD").value = ""
        document.getElementById("CAT_NAME").value = ""
        document.getElementById("ASSET_ID_REG_DIV").value = ""
        document.getElementById("EPC_SIZE").value = ""
        document.getElementById("CAT_L_NAME").value = ""
        document.getElementById("CAT_M_NAME").value = ""
        document.getElementById("CAT_S_NAME").value = ""
        // 2019/11/19 nv miyaji add start
        document.getElementById("CAT_M_NAME").value = ""
        document.getElementById("CAT_S_NAME").value = ""
        // 2019/11/19 nv miyaji add end

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/Category")/GetEditData',
                cache: false,
                data: { ID: text[0] },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }

                    //EPC事業所キードロップダウン作成
                    var epcCompanyKey = document.getElementById("EPC_COMPANY_KEY");
                    epcCompanyKey.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    document.getElementById("EPC_COMPANY_KEY").appendChild(opBlank);

                    for (var i = 0; i < data.epcCompanyKeyDropdownData.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.epcCompanyKeyDropdownData[i].Cd;
                        op.text = data.epcCompanyKeyDropdownData[i].Name;
                        document.getElementById("EPC_COMPANY_KEY").appendChild(op);
                    }

                    //取得したデータを表示する
                    //document.getElementById("CAT_DIV").value = data.CAT_DIV;
                    document.getElementById("CAT_L_CD").value = data.CAT_L_CD;
                    document.getElementById("CAT_M_CD").value = data.CAT_M_CD;
                    document.getElementById("CAT_S_CD").value = data.CAT_S_CD;
                    document.getElementById("CAT_NAME").value = data.CAT_NAME;
                    //document.getElementById("LEASE_LIMIT_ALERT_DIV").value = data.LEASE_LIMIT_ALERT_DIV;
                    document.getElementById("LEASE_LIMIT_BEF_DAYS").value = data.LEASE_LIMIT_BEF_DAYS;
                    //document.getElementById("GENERIC_LIMIT_ALERT_DIV").value = data.GENERIC_LIMIT_ALERT_DIV;
                    document.getElementById("GENERIC_LIMIT_BEF_DAYS").value = data.GENERIC_LIMIT_BEF_DAYS;
                    // Add Start 2020.02.06 Iwamoto
                    document.getElementById("LEASE_ALERT_CNT").value = data.LEASE_ALERT_CNT;
                    document.getElementById("LEASE_UPPER_LIMIT").value = data.LEASE_UPPER_LIMIT;
                    // Add End 2020.02.06 Iwamoto
                    document.getElementById("ASSET_ID_REG_DIV").value = data.ASSET_ID_REG_DIV;
                    document.getElementById("EPC_SIZE").value = data.EPC_SIZE;
                    document.getElementById("EPC_COMPANY_KEY").value = data.EPC_COMPANY_KEY;    // Add 2020.02.13 Iwamoto
                    document.getElementById("EPC_ASSET_TYPE").value = data.EPC_ASSET_TYPE;      // Add 2020.02.06 Iwamoto
                    document.getElementById("INS_DATETIME").value = formatDate(new Date(data.InsDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("INS_EMPLOYEE_CD").value = data.INS_EMPLOYEE_CD;
                    document.getElementById("INS_EMPLOYEE_NAME").value = data.INS_EMPLOYEE_NAME;
                    document.getElementById("UPD_DATETIME").value = formatDate(new Date(data.UpdDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("UPD_EMPLOYEE_CD").value = data.UPD_EMPLOYEE_CD;
                    document.getElementById("UPD_EMPLOYEE_NAME").value = data.UPD_EMPLOYEE_NAME;
                    document.getElementById("UPD_TASK").value = data.RenewProcess;

                    document.getElementById("CAT_L_NAME").value = data.CatLName;
                    document.getElementById("CAT_M_NAME").value = data.CatMName;
                    document.getElementById("CAT_S_NAME").value = data.CatSName;

                    //隠し項目　資産ID登録方法
                    document.getElementById("AssetIdDivReg").value = data.AssetIdDivReg;

                    //分類区分による設定
                    switch (data.CAT_DIV) {
                        case "@CAT_DIV.CategoryL":
                            $("#CAT_DIV_L").prop("checked", true);
                            $("#CAT_DIV_M").prop("checked", false);
                            $("#CAT_DIV_S").prop("checked", false);
                            //活性・非活性
                            $('#CAT_DIV_L').prop('disabled', false);
                            $('#CAT_DIV_M').prop('disabled', true);
                            $('#CAT_DIV_S').prop('disabled', true);
                            //アラート表示「上位に準じる」使用不可
                            $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", true);
                            $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", true);
                            $("#LEASE_ALERT_DIV_F").prop("disabled", true);     // Add 2020.02.06 Iwamoto

                            // 2019/09/26 nv miyaji upd start
                            if (data.AssetIdDivReg == false){
                                $("#ASSET_ID_REG_DIV").prop("disabled", true);
                                $("#EPC_SIZE").prop("disabled", true);
                                $('#EPC_COMPANY_KEY').prop('disabled', true);   // Add 2020.02.12 Iwamoto
                                $('#EPC_ASSET_TYPE').prop('disabled', true);    // Add 2020.02.12 Iwamoto
                            }else{
                                $("#ASSET_ID_REG_DIV").prop("disabled", false);
                                // Upd Start 2020.02.12 Iwamoto
                                //$("#EPC_SIZE").prop("disabled", false);
                                switch (data.ASSET_ID_REG_DIV) {
                                    case @AssetIDGenerationMethod.GIAI:
                                        // GIAI
                                        // --タグ領域サイズ：使用不可
                                        $('#EPC_SIZE').prop('disabled', true);
                                        // --タグEPC事業所：使用可
                                        $('#EPC_COMPANY_KEY').prop('disabled', false);
                                        // --タグEPC資産タイプ：使用不可
                                        $('#EPC_ASSET_TYPE').prop('disabled', true);
                                        break;
                                    case @AssetIDGenerationMethod.GRAI:
                                        // GRAI
                                        // --タグ領域サイズ：使用不可
                                        $('#EPC_SIZE').prop('disabled', true);
                                        // --タグEPC事業所：使用可
                                        $('#EPC_COMPANY_KEY').prop('disabled', false);
                                        // --タグEPC資産タイプ：使用可
                                        $('#EPC_ASSET_TYPE').prop('disabled', false);
                                        break;
                                    default:
                                        // 以外
                                        // --タグ領域サイズ：使用可
                                        $('#EPC_SIZE').prop('disabled', false);
                                        // --タグEPC事業所：使用不可
                                        $('#EPC_COMPANY_KEY').prop('disabled', true);
                                        // --タグEPC資産タイプ：使用不可
                                        $('#EPC_ASSET_TYPE').prop('disabled', true);
                                        break;
                                }
                            }
                            // Upd End 2020.02.12 Iwamoto
                            ////資産ID(EPC)登録方法使用可
                            //$("#ASSET_ID_REG_DIV").prop("disabled", false);
                            ////EPC領域サイズ使用可
                            //$("#EPC_SIZE").prop("disabled", false);
                            // 2019/09/26 nv miyaji upd end
                            break;

                        case "@CAT_DIV.CategoryM":
                            $("#CAT_DIV_L").prop("checked", false);
                            $("#CAT_DIV_M").prop("checked", true);
                            $("#CAT_DIV_S").prop("checked", false);
                            //活性・非活性
                            $('#CAT_DIV_L').prop('disabled', true);
                            $('#CAT_DIV_M').prop('disabled', false);
                            $('#CAT_DIV_S').prop('disabled', true);
                            //アラート表示「上位に準じる」使用可
                            $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", false);
                            $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", false);
                            $("#LEASE_ALERT_DIV_F").prop("disabled", false);    // Add 2020.02.06 Iwamoto
                            //資産ID(EPC)登録方法使用不可
                            $("#ASSET_ID_REG_DIV").prop("disabled", true);
                            //EPC領域サイズ使用不可
                            $("#EPC_SIZE").prop("disabled", true);
                            //タグEPC事業所：使用不可
                            $('#EPC_COMPANY_KEY').prop('disabled', true);
                            //タグEPC資産タイプ：使用不可
                            $('#EPC_ASSET_TYPE').prop('disabled', true);
                            break;

                        case "@CAT_DIV.CategoryS":
                            $("#CAT_DIV_L").prop("checked", false);
                            $("#CAT_DIV_M").prop("checked", false);
                            $("#CAT_DIV_S").prop("checked", true);
                            //活性・非活性
                            $('#CAT_DIV_L').prop('disabled', true);
                            $('#CAT_DIV_M').prop('disabled', true);
                            $('#CAT_DIV_S').prop('disabled', false);
                            //アラート表示「上位に準じる」使用可
                            $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", false);
                            $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", false);
                            $("#LEASE_ALERT_DIV_F").prop("disabled", false);    // Add 2020.02.06 Iwamoto
                            //資産ID(EPC)登録方法使用不可
                            $("#ASSET_ID_REG_DIV").prop("disabled", true);
                            //EPC領域サイズ使用不可
                            $("#EPC_SIZE").prop("disabled", true);
                            //タグEPC事業所：使用不可
                            $('#EPC_COMPANY_KEY').prop('disabled', true);
                            //タグEPC資産タイプ：使用不可
                            $('#EPC_ASSET_TYPE').prop('disabled', true);
                            break;
                    }

                    //リース期限アラート区分
                    switch (data.LEASE_LIMIT_ALERT_DIV) {
                        case "@LeaseLimitAlertDiv.Front":
                            $("#LEASE_LIMIT_ALERT_DIV_F").prop("checked", true);
                            $("#LEASE_LIMIT_ALERT_DIV_N").prop("checked", false);
                            $("#LEASE_LIMIT_ALERT_DIV_Y").prop("checked", false);
                            break;

                        case "@LeaseLimitAlertDiv.No":
                            $("#LEASE_LIMIT_ALERT_DIV_F").prop("checked", false);
                            $("#LEASE_LIMIT_ALERT_DIV_N").prop("checked", true);
                            $("#LEASE_LIMIT_ALERT_DIV_Y").prop("checked", false);
                            break;

                        case "@LeaseLimitAlertDiv.Yes":
                            $("#LEASE_LIMIT_ALERT_DIV_F").prop("checked", false);
                            $("#LEASE_LIMIT_ALERT_DIV_N").prop("checked", false);
                            $("#LEASE_LIMIT_ALERT_DIV_Y").prop("checked", true);
                            break;
                    }

                    //○○期限アラート区分
                    switch (data.GENERIC_LIMIT_ALERT_DIV) {
                        case "@GenericLimitAlertDiv.Front":
                            $("#GENERIC_LIMIT_ALERT_DIV_F").prop("checked", true);
                            $("#GENERIC_LIMIT_ALERT_DIV_N").prop("checked", false);
                            $("#GENERIC_LIMIT_ALERT_DIV_Y").prop("checked", false);
                            break;

                        case "@GenericLimitAlertDiv.No":
                            $("#GENERIC_LIMIT_ALERT_DIV_F").prop("checked", false);
                            $("#GENERIC_LIMIT_ALERT_DIV_N").prop("checked", true);
                            $("#GENERIC_LIMIT_ALERT_DIV_Y").prop("checked", false);
                            break;

                        case "@GenericLimitAlertDiv.Yes":
                            $("#GENERIC_LIMIT_ALERT_DIV_F").prop("checked", false);
                            $("#GENERIC_LIMIT_ALERT_DIV_N").prop("checked", false);
                            $("#GENERIC_LIMIT_ALERT_DIV_Y").prop("checked", true);
                            break;
                    }

                    //貸出アラート区分      Add 2020.02.06 Iwamoto
                    switch (data.LEASE_ALERT_DIV) {
                        case "@LeaseAlertDiv.Front":
                            $("#LEASE_ALERT_DIV_F").prop("checked", true);
                            $("#LEASE_ALERT_DIV_N").prop("checked", false);
                            $("#LEASE_ALERT_DIV_Y").prop("checked", false);
                            break;

                        case "@LeaseAlertDiv.No":
                            $("#LEASE_ALERT_DIV_F").prop("checked", false);
                            $("#LEASE_ALERT_DIV_N").prop("checked", true);
                            $("#LEASE_ALERT_DIV_Y").prop("checked", false);
                            break;

                        case "@LeaseAlertDiv.Yes":
                            $("#LEASE_ALERT_DIV_F").prop("checked", false);
                            $("#LEASE_ALERT_DIV_N").prop("checked", false);
                            $("#LEASE_ALERT_DIV_Y").prop("checked", true);
                            break;
                    }

                    //貸出上限エラー区分
                    switch (data.LEASE_UPPER_ERR_DIV) {
                        case "@LeaseAlertDiv.No":
                            $("#LEASE_UPPER_ERR_DIV_N").prop("checked", true);
                            $("#LEASE_UPPER_ERR_DIV_Y").prop("checked", false);
                            break;

                        case "@LeaseAlertDiv.Yes":
                            $("#LEASE_UPPER_ERR_DIV_N").prop("checked", false);
                            $("#LEASE_UPPER_ERR_DIV_Y").prop("checked", true);
                            break;

                        default:
                            $("#LEASE_UPPER_ERR_DIV_N").prop("checked", false);
                            $("#LEASE_UPPER_ERR_DIV_Y").prop("checked", false);
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
    function btnCreateModal_onclick(){
        if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
            showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/Category")/Create")
        } else {
            //確認メッセージを出し、ＯＫなら除却処理
            // ダイアログのメッセージを設定
            $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)");

            // ダイアログを作成
            $("#show-dialog").dialog({
                modal: true,
                title: strConfirmTitle,
                dialogClass: "show-confirm-wrap",
                buttons: {
                    "OK": function () {
                        $("#ASSET_ID_REG_DIV").removeAttr("disabled");
                        $("#EPC_SIZE").removeAttr("disabled");
                        $("#EPC_COMPANY_KEY").removeAttr("disabled");   // Add 2020.02.12 Iwamoto
                        $("#EPC_ASSET_TYPE").removeAttr("disabled");    // Add 2020.02.12 Iwamoto
                        var befAction = $('#form').attr("action");
                        $(this).dialog("close");
                        $('#form').attr("action", "@Url.Content("~/Category")/Create");
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
            @*showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/Category")/Create")*@
        }
    }

    //削除ボタンクリック処理
    function btnDelete_onclick() {
        //確認メッセージを出し、ＯＫなら除却処理
        // ダイアログのメッセージを設定
        $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)");

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $("#ASSET_ID_REG_DIV").removeAttr("disabled");
                    $("#EPC_SIZE").removeAttr("disabled");
                    $("#EPC_COMPANY_KEY").removeAttr("disabled");   // Add 2020.02.12 Iwamoto
                    $("#EPC_ASSET_TYPE").removeAttr("disabled");    // Add 2020.02.12 Iwamoto
                    var befAction = $('#form').attr("action");
                    $(this).dialog("close");
                    $('#form').attr("action", "@Url.Content("~/Category")/Delete");
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
        @*//確認メッセージ
        showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/Category")/Delete");*@
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji
        //画面ブロックを表示
        showLoading();
        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //大分類選択
    function ChangeCategoryL(catL) {
        //中分類・小分類をクリアした後に×の表示を更新する
        refreshCategoryClearableLater();
        //--2019/11/18 nv miyaji upd start
        if(catL.name == "SearchCatL"){
            // 中分類をクリア
            document.getElementById("SearchCatM").value = "";
            $("#SearchCatM").attr("disabled", "disabled");
            // 小分類をクリア
            document.getElementById("SearchCatS").value = "";
            $("#SearchCatS").attr("disabled", "disabled");
        }else{
            // コンボボックスで選択した値をエディットボックスにも反映する。
            document.getElementById("CAT_L_CD").value = document.getElementById("ModalCatL").value;
            GetCatLName(document.getElementById("CAT_L_CD").value)
            // 中区分をクリア
            document.getElementById("CAT_M_CD").value = "";
            document.getElementById("ModalCatM").value = "";
            $("#ModalCatM").attr("disabled", "disabled");
            // 小区分をクリア
            document.getElementById("CAT_S_CD").value = "";
        }
        //// 中分類をクリア
        //document.getElementById("SearchCatM").value = "";
        //$("#SearchCatM").attr("disabled", "disabled");
        //// 小分類をクリア
        //document.getElementById("SearchCatS").value = "";
        //$("#SearchCatS").attr("disabled", "disabled");
        //--2019/11/18 nv miyaji upd end

        // 選択されていない場合はここで終了
        if (!catL.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Category")/ChangeCategory',
            data: { categoryLevel: "@CAT_DIV.CategoryM", selectLCode: catL.value, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    //--2019/11/18 nv miyaji upd start
                    var SearchCatM = ""
                    if(catL.name == "SearchCatL"){
                        SearchCatM = document.getElementById("SearchCatM");
                    }else{
                        SearchCatM =document.getElementById("ModalCatM");
                    }
                    //var SearchCatM = document.getElementById("SearchCatM");
                    SearchCatM.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchCatM.appendChild(opBlank);
                    //document.getElementById("SearchCatM").appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].CAT_CD;
                        op.text = data.Data[i].CAT_NAME;
                        SearchCatM.appendChild(op);
                        //document.getElementById("SearchCatM").appendChild(op);
                    }
                }
                // 中分類データ取得
                if(catL.name == "SearchCatL"){
                    $("#SearchCatM").removeAttr("disabled");
                }else{
                    $("#ModalCatM").removeAttr("disabled");
                }

                //document.getElementById("SearchCatM").value = data.catMDropdownData;
                SearchCatM.value = data.catMDropdownData;
                //--2019/11/18 nv miyaji upd end
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //中分類選択
    function ChangeCategoryM(catM) {
        //小分類をクリアした後に×の表示を更新する
        refreshCategoryClearableLater();
        var lCode  = document.getElementById("SearchCatL").value;
        //--2019/11/18 nv miyaji upd start
        if(catM.name == "SearchCatM"){
            // 小分類をクリア
            document.getElementById("SearchCatS").value = "";
            $("#SearchCatS").attr("disabled", "disabled");
        }else{
            // コンボボックスで選択した値をエディットボックスにも反映する。
            document.getElementById("CAT_M_CD").value = document.getElementById("ModalCatM").value;
            GetCatMName(document.getElementById("CAT_M_CD").value)
            // 小分類をクリア
            document.getElementById("CAT_S_CD").value = "";
            // モーダル画面の場合はここで終了
            return;
        }
        //--2019/11/18 nv miyaji upd end


        // 選択されていない場合はここで終了
        if (!catM.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Category")/ChangeCategory',
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

    //分類区分変更
    function ChangeCatDiv(value) {
        //値・入力可否を設定した後に×の表示を更新する
        refreshCategoryClearableLater();
        switch (value) {
            case "@CAT_DIV.CategoryL":
                //--2019/11/18 nv miyaji add start
                // 大分類選択時、通常表示
                $("#td-LSentaku").show();
                $("#td-NotLSentaku").hide();
                $("#td-SSentaku").hide();
                $("#td-NotSSentaku").show();
                // 大分類コード・名称をクリア
                $("#CAT_L_CD").val("");
                $("#CAT_L_NAME").val("");
                // 大分類コンボボックスの選択をクリア
                document.getElementById("ModalCatL").value = "";
                // 中分類コンボボックスをクリア
                document.getElementById("ModalCatM").value = "";
                $("#ModalCatM").attr("disabled", "disabled");
                //--2019/11/18 nv miyaji add end
                //中分類：書き込み不可
                document.getElementById("CAT_M_CD").readOnly = true;
                $("#CAT_M_CD").addClass("disable");
                $("#CAT_M_CD").val("");
                $("#CAT_M_NAME").val("");
                //小分類：書き込み不可
                document.getElementById("CAT_S_CD").readOnly = true;
                $("#CAT_S_CD").addClass("disable");
                $("#CAT_S_CD").val("");
                $("#CAT_S_NAME").val("");
                //アラート表示「上位に準じる」使用不可、しないを選択
                $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", true);
                $("#LEASE_LIMIT_ALERT_DIV_N").prop("checked", true);
                $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", true);
                $("#GENERIC_LIMIT_ALERT_DIV_N").prop("checked", true);
                $("#LEASE_ALERT_DIV_F").prop("disabled", true);     // Add 2020.02.06 Iwamoto
                $("#LEASE_ALERT_DIV_N").prop("checked", true);      // Add 2020.02.06 Iwamoto
                //資産ID(EPC)登録方法使用可
                $("#ASSET_ID_REG_DIV").prop("disabled", false);
                //Upd Start 2020.02.14 Iwamoto
                ////EPC領域サイズ使用可
                //$("#EPC_SIZE").prop("disabled", false);
                var assetIdRegDiv = document.getElementById("ASSET_ID_REG_DIV");
                switch (assetIdRegDiv.value) {
                    case "@AssetIDGenerationMethod.GIAI":
                        // GIAI
                        // --タグ領域サイズ：使用不可
                        $('#EPC_SIZE').prop('disabled', true);
                        // --タグEPC事業所：使用可
                        $('#EPC_COMPANY_KEY').prop('disabled', false);
                        // --タグEPC資産タイプ：使用不可
                        $('#EPC_ASSET_TYPE').prop('disabled', true);
                        break;
                    case "@AssetIDGenerationMethod.GRAI":
                        // GRAI
                        // --タグ領域サイズ：使用不可
                        $('#EPC_SIZE').prop('disabled', true);
                        // --タグEPC事業所：使用可
                        $('#EPC_COMPANY_KEY').prop('disabled', false);
                        // --タグEPC資産タイプ：使用可
                        $('#EPC_ASSET_TYPE').prop('disabled', false);
                        break;
                    default:
                        // 以外
                        // --タグ領域サイズ：使用可
                        $('#EPC_SIZE').prop('disabled', false);
                        // --タグEPC事業所：使用不可
                        $('#EPC_COMPANY_KEY').prop('disabled', true);
                        // --タグEPC資産タイプ：使用不可
                        $('#EPC_ASSET_TYPE').prop('disabled', true);
                        break;
                }
                //Upd End 2020.02.14 Iwamoto
                break;

            case "@CAT_DIV.CategoryM":
                //--2019/11/18 nv miyaji add start
                // 中分類選択時、大分類をコンボボックス表示にする
                $("#td-LSentaku").hide();
                $("#td-NotLSentaku").show();
                $("#td-SSentaku").hide();
                $("#td-NotSSentaku").show();
                // 大分類コード・名称をクリア
                $("#CAT_L_CD").val("");
                $("#CAT_L_NAME").val("");
                // 中分類コード・名称をクリア
                $("#CAT_M_CD").val("");
                $("#CAT_M_NAME").val("");
                // 大分類コンボボックスの選択をクリア
                document.getElementById("ModalCatL").value = "";
                // 中分類コンボボックスをクリア
                document.getElementById("ModalCatM").value = "";
                $("#ModalCatM").attr("disabled", "disabled");
                //--2019/11/18 nv miyaji add end
                //中分類：書き込み可
                document.getElementById("CAT_M_CD").readOnly = false;
                $("#CAT_M_CD").removeClass("disable");
                //小分類：書き込み不可
                document.getElementById("CAT_S_CD").readOnly = true;
                $("#CAT_S_CD").addClass("disable");
                $("#CAT_S_CD").val("");
                $("#CAT_S_NAME").val("");
                //アラート表示「上位に準じる」使用可、選択
                $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", false);
                $("#LEASE_LIMIT_ALERT_DIV_F").prop("checked", true);
                $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", false);
                $("#GENERIC_LIMIT_ALERT_DIV_F").prop("checked", true);
                $("#LEASE_ALERT_DIV_F").prop("disabled", false);    // Add 2020.02.06 Iwamoto
                $("#LEASE_ALERT_DIV_F").prop("checked", true);      // Add 2020.02.06 Iwamoto
                //資産ID(EPC)登録方法使用不可
                $("#ASSET_ID_REG_DIV").prop("disabled", true);
                //EPC領域サイズ使用不可
                $("#EPC_SIZE").prop("disabled", true);
                //タグEPC事業所：使用不可
                $('#EPC_COMPANY_KEY').prop('disabled', true);       // Add 2020.02.14 Iwamoto
                //タグEPC資産タイプ：使用不可
                $('#EPC_ASSET_TYPE').prop('disabled', true);        // Add 2020.02.14 Iwamoto
                break;

            case "@CAT_DIV.CategoryS":
                //--2019/11/18 nv miyaji add start
                // 小分類選択時、大分類・中分類をコンボボックス表示にする
                $("#td-LSentaku").hide();
                $("#td-NotLSentaku").show();
                $("#td-SSentaku").show();
                $("#td-NotSSentaku").hide();
                // 大分類コード・名称をクリア
                $("#CAT_L_CD").val("");
                $("#CAT_L_NAME").val("");
                // 中分類コード・名称をクリア
                $("#CAT_M_CD").val("");
                $("#CAT_M_NAME").val("");
                // 大分類コンボボックスの選択をクリア
                document.getElementById("ModalCatL").value = "";
                // 中分類コンボボックスをクリア
                document.getElementById("ModalCatM").value = "";
                $("#ModalCatM").attr("disabled", "disabled");
                //--2019/11/18 nv miyaji add end
                //中分類：書き込み可
                document.getElementById("CAT_M_CD").readOnly = false;
                $("#CAT_M_CD").removeClass("disable");
                //小分類：書き込み可
                document.getElementById("CAT_S_CD").readOnly = false;
                $("#CAT_S_CD").removeClass("disable");
                //アラート表示「上位に準じる」使用可、選択
                $("#LEASE_LIMIT_ALERT_DIV_F").prop("disabled", false);
                $("#LEASE_LIMIT_ALERT_DIV_F").prop("checked", true);
                $("#GENERIC_LIMIT_ALERT_DIV_F").prop("disabled", false);
                $("#GENERIC_LIMIT_ALERT_DIV_F").prop("checked", true);
                $("#LEASE_ALERT_DIV_F").prop("disabled", false);    // Add 2020.02.06 Iwamoto
                $("#LEASE_ALERT_DIV_F").prop("checked", true);      // Add 2020.02.06 Iwamoto
                //資産ID(EPC)登録方法使用不可
                $("#ASSET_ID_REG_DIV").prop("disabled", true);
                //EPC領域サイズ使用不可
                $("#EPC_SIZE").prop("disabled", true);
                //タグEPC事業所：使用不可
                $('#EPC_COMPANY_KEY').prop('disabled', true);       // Add 2020.02.14 Iwamoto
                //タグEPC資産タイプ：使用不可
                $('#EPC_ASSET_TYPE').prop('disabled', true);        // Add 2020.02.14 Iwamoto
                break;
        }
    }

    //大分類名称取得
    function GetCatLName(value) {
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Category")/GetCatName',
            data: { inputLCode: value, inputMCode: '', inputSCode: '' },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    document.getElementById("CAT_L_NAME").value = "";
                    return;
                }
                else {
                    //取得したデータを表示する
                    document.getElementById("CAT_L_NAME").value = "";
                    if (data.length != 0) {
                        document.getElementById("CAT_L_NAME").value = data[0].CAT_L_NAME;
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }
    //中分類名称取得
    function GetCatMName(value) {
        var catLCd;
        catLCd = document.getElementById("CAT_L_CD").value;

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Category")/GetCatName',
            data: { inputLCode: catLCd, inputMCode: value, inputSCode: '' },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    document.getElementById("CAT_M_NAME").value = "";
                    return;
                }
                else {
                    //取得したデータを表示する
                    document.getElementById("CAT_M_NAME").value = "";
                    if (data.length != 0) {
                        document.getElementById("CAT_M_NAME").value = data[0].CAT_M_NAME;
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }
    //小分類名称取得
    function GetCatSName(value) {
        var catLCd;
        catLCd = document.getElementById("CAT_L_CD").value;
        var locMCd;
        catMCd = document.getElementById("CAT_M_CD").value;

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Category")/GetCatName',
            data: { inputLCode: catLCd, inputMCode: catMCd, inputSCode: value },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    document.getElementById("CAT_S_NAME").value = "";
                    return;
                }
                else {
                    //取得したデータを表示する
                    document.getElementById("CAT_S_NAME").value = "";
                    if (data.length != 0) {
                        document.getElementById("CAT_S_NAME").value = data[0].CAT_S_NAME;
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //資産ID登録方法ドロップダウン選択
    function ChangeAssetIdRegDiv(assetIdRegDiv) {
        //値・選択肢・入力可否を設定した後に×の表示を更新する
        refreshCategoryClearableLater();
        // EPC事業所キーをクリア
        document.getElementById("EPC_COMPANY_KEY").options.length = 0;
        $('#EPC_COMPANY_KEY').prop('disabled', true);

        switch (assetIdRegDiv.value) {
            case "@AssetIDGenerationMethod.GIAI":
                // GIAI
                // --タグ領域サイズ：使用不可、096固定
                $('#EPC_SIZE').prop('disabled', true);
                $('#EPC_SIZE').val(@TagSize.Size096);
                // --タグEPC資産タイプ：使用不可、空文字列設定
                $('#EPC_ASSET_TYPE').prop('disabled', true);
                $('#EPC_ASSET_TYPE').val('');
                break;
            case "@AssetIDGenerationMethod.GRAI":
                // GRAI
                // --タグ領域サイズ：使用不可、096固定
                $('#EPC_SIZE').prop('disabled', true);
                $('#EPC_SIZE').val(@TagSize.Size096);
                // --タグEPC資産タイプ：使用可
                $('#EPC_ASSET_TYPE').prop('disabled', false);
                break;
            default:
                // 以外
                // --タグ領域サイズ：使用可
                $('#EPC_SIZE').prop('disabled', false);
                // --タグEPC資産タイプ：使用不可、空文字列設定
                $('#EPC_ASSET_TYPE').prop('disabled', true);
                $('#EPC_ASSET_TYPE').val('');
                // --以降の処理を行わない
                return;
                break;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/Category")/ChangeAssetIdRegDiv',
            data: { selectAssetIdRegDiv: assetIdRegDiv.value },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var epcCompanyKey = document.getElementById("EPC_COMPANY_KEY");
                    epcCompanyKey.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    document.getElementById("EPC_COMPANY_KEY").appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].Cd;
                        op.text = data.Data[i].Name;
                        document.getElementById("EPC_COMPANY_KEY").appendChild(op);
                    }
                }
                // EPC事業所キーデータ取得
                $("#EPC_COMPANY_KEY").removeAttr("disabled");
                document.getElementById("EPC_COMPANY_KEY").value = data.epcCompanyKeyDropdownData;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

    //×（クリアボタン）付きプルダウン（検索条件・モーダル画面）の×の表示を更新する
    //※入力・選択時は自動で更新される（Scripts\multi-input-dialog.js）。JS で値・選択肢・入力可否（disabled）を変更した後に呼び出す
    function refreshCategoryClearable() {
        refreshClearable("#SearchCatL, #SearchCatM, #SearchCatS, #ModalCatL, #ModalCatM, #ASSET_ID_REG_DIV, #EPC_SIZE, #EPC_COMPANY_KEY");
    }

    //処理の途中で return する関数が多いため、呼び出し元の処理が終わった後に×の表示を更新する
    function refreshCategoryClearableLater() {
        setTimeout(refreshCategoryClearable, 0);
    }

    //Ajax の結果で値・選択肢・入力可否を変更する処理が多いため、Ajax 完了時にも×の表示を更新する
    $(document).ajaxComplete(refreshCategoryClearable);

</script>

<style type="text/css">
    /* モーダル画面の×付きプルダウンは、プルダウンの幅に合わせて×の位置を決める（×がプルダウンの枠外に出ないようにする） */
    #modal-content .clearable {
        display: inline-block;
        max-width: 100%;
    }
</style>

<div id="category-master">
    @Using (Html.BeginForm("Index", "Category", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    @* 2021/09/27 nv iwamoto update start *@
                    @*<input class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                    <input class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">*@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @<input id="create-btn" Class="defButton create-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                        @<input id="update-btn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="btnEdit_onclick()">
                    Else
                        @<input id="show-btn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DISP)" onclick="btnEdit_onclick()">
                    End If
                    @* 2021/09/27 nv iwamoto update end *@
                </div>
                <div class="view-area-button-right">
                    @* 2021/09/27 nv iwamoto update start *@
                    @*<input class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">*@
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @<input id="csv-import-btn" Class="defButton csv-import-btn" type="button" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">
                    End If
                    @* 2021/09/27 nv iwamoto update end *@
                    <input class="defButton csv-Export-btn  submit-button" type="button" name="CsvExport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL)" onclick="btnCsvExport_onclick()">
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
                             <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CAT_DIV").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("CAT_CD").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("CAT_NAME").DispFlag = "1", "display", "None"))">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                                 <div class="grid-condition-col-multi">
                                     @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                                     <span class="clearable">
                                         @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control", .onchange = "ChangeCategoryL(this)"})  @*--2019/11/19 nv miyaji 引数をthis.valueからthisに変更*@
                                         <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                     </span>
                                 </div>
                             </div>

                             <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CAT_DIV").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("CAT_CD").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("CAT_NAME").DispFlag = "1", "display", "None"))">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_M)</div>
                                 <div class="grid-condition-col-multi">
                                     @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                                     <span class="clearable">
                                         @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatM, DirectCast(ViewBag.modelCatM, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control", .onchange = "ChangeCategoryM(this)"})  @*--2019/11/19 nv miyaji 引数をthis.valueからthisに変更*@
                                         <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                     </span>
                                 </div>
                             </div>

                             <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("CAT_DIV").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("CAT_CD").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("CAT_NAME").DispFlag = "1", "display", "None"))">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_S)</div>
                                 <div class="grid-condition-col-multi">
                                     @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                                     @* ※onchange="ChangeCategoryS(this)" は関数が存在せずエラーになり、×での選択解除時に×の表示が更新されないため削除 *@
                                     <span class="clearable">
                                         @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatS, DirectCast(ViewBag.modelCatS, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                         <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                     </span>
                                 </div>
                             </div>

                             @* 表示指定 *@
                             <div class="col-sm-4"  style="max-width:600px;">
                                 <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_DISP_SELECT)</div>
                                 <div class="grid-condition-col">
                                     <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.L)@Message.getMessage(CommonLabel.VALUE_CAT_L)</div>
                                     <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.LM)@Message.getMessage(CommonLabel.LABEL_CATEGORY_LM)</div>
                                     <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.LMS)@Message.getMessage(CommonLabel.LABEL_CATEGORY_LMS)</div>
                                     <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.M)@Message.getMessage(CommonLabel.LABEL_CATEGORY_M_ONLY)</div>
                                     <div class="input-select-item-last">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, DISP_SELECT.S)@Message.getMessage(CommonLabel.LABEL_CATEGORY_S_ONLY)</div>
                                 </div>
                             </div>
                         </div>
                     </div>

                     <div class="view-area-label" style="height: 25px"></div>
                     <input class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />

                 </div>
                </div>
                     </div>
                     @*直近登録者情報*@
                     @<div class="search-wrap">
                         <div class="float-left mr50">@Message.getMessage(CommonLabel.LABEL_INS_DATETIME):@ViewBag.LastInsDateTime</div>
                         <div class="float-left">@Message.getMessage(CommonLabel.LABEL_INS_EMPLOYEE_NAME):@ViewBag.LastInsEmployeeNm</div>
                         <div style="clear:both;"></div>
                     </div>
                     @*ページャー表示*@
                     @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
                     @<div Class="list-view">
                         @* 高さはヘッダー固定の処理（enableFixedHeader）で画面の残り領域に合わせる *@
                         <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
                     </div>

                     'CSV取込のモーダル画面
                     @<div id="modal-csv-content">
                         <div class="popup-title-wrap">
                             <p class="Line3"></p>
                             <p class="Line4"></p>
                             <h2 id="popup-title-csv"></h2>
                             <p class="Line3"></p>
                             <p class="Line4"></p>
                         </div>
                         @CODE
                             'モデルの値を設定
                             Dim csvSessionModel As CategoryViewModel = Session(CommonSessionKey.CREAGE_CATEGORY)
                             If Not IsNothing(csvSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                                 Model.Add(csvSessionModel)
                             Else
                                 Model.Add(New CategoryViewModel())
                             End If
                         END CODE
                         @*@Html.AntiForgeryToken()*@
                         <div Class="PopupButton clearfix mb10">
                             <div class="float-left">
                                 <input type="file" name="CsvFile" id="CsvFile" />
                                 <div style="color:red;">@Message.getMessage(CommonLabel.MSG_INFO_CSV_IMPORT_ALL_DATA_DELETE)</div>
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
                                             @for icnt As Integer = 1 To DataInport.DISP_ROW_COUNT
                             @<tr><td Class="col1">　</td><td class="col2">　</td><td class="col3">　</td></tr>
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


                             '登録・更新のモーダル画面
                     @<div id="modal-content" class="modal-category">
                         <div class="popup-title-wrap">
                             <p class="Line3"></p>
                             <p class="Line4"></p>
                             <h2 id="popup-title"></h2>
                             <p class="Line3"></p>
                             <p class="Line4"></p>
                         </div>
                         @CODE
                             'モデルの値を設定
                             Dim SessionModel As CategoryViewModel = Session(CommonSessionKey.CREAGE_CATEGORY)
                             If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                                 Model.Add(SessionModel)
                             Else
                                 Model.Add(New CategoryViewModel())
                             End If
                         END CODE
                         @Html.AntiForgeryToken()


                         <Table>
                             <tr Class="popup-row">
                                 <td class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_CAT_DIV)</td>
                                 <td width="20" class="require">※</td>
                                 <td class="radio-group">
                                     <div class="col">@Html.RadioButtonFor(Function(model) model.Last.CAT_DIV, CAT_DIV.CategoryL, New With {.id = "CAT_DIV_L", .onchange = "ChangeCatDiv(this.value)"})@Message.getMessage(CommonLabel.VALUE_CAT_L)</div>
                                     <div class="col">@Html.RadioButtonFor(Function(model) model.Last.CAT_DIV, CAT_DIV.CategoryM, New With {.id = "CAT_DIV_M", .onchange = "ChangeCatDiv(this.value)"})@Message.getMessage(CommonLabel.VALUE_CAT_M)</div>
                                     <div class="col">@Html.RadioButtonFor(Function(model) model.Last.CAT_DIV, CAT_DIV.CategoryS, New With {.id = "CAT_DIV_S", .onchange = "ChangeCatDiv(this.value)"})@Message.getMessage(CommonLabel.VALUE_CAT_S)</div>
                                 </td>
                             </tr>
                             <tr Class="popup-row">
                                 <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_CAT_L_CD)</td>
                                 <td width="20" class="require">※</td>
                                 <td id="td-LSentaku">
                                     @Html.EditorFor(Function(model) model.Last.CAT_L_CD, New With {.htmlAttributes = New With {.class = "text-box-cd-small", .maxlength = 8, .style = "width:85px;", .oninput = "editHanEi(this); GetCatLName(this.value)"}})
                                     @Html.EditorFor(Function(model) model.Last.CatLName, New With {.htmlAttributes = New With {.id = "CAT_L_NAME", .class = "disable", .readonly = "readonly", .style = "width:790px;"}})
                                 </td>
                                 <td id="td-NotLSentaku">@* 分類区分で中分類・小分類選択時はコンボボックス表示にする  *@
                                 @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                                 <span class="clearable">
                                     @Html.DropDownListFor(Function(model) model.Last.ModalCatL, DirectCast(ViewBag.modalmodelCatL, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeCategoryL(this)", .style = "width: 420px;"})
                                     <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                 </span>
                             </td>
                         </tr>
                         <tr Class="popup-row">
                             <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_CAT_M_CD)</td>
                             <td width="20"></td>
                             <td id="td-NotSSentaku">
                                 @Html.EditorFor(Function(model) model.Last.CAT_M_CD, New With {.htmlAttributes = New With {.class = "text-box-cd-small", .maxlength = 8, .style = "width:85px;", .oninput = "editHanEi(this); GetCatMName(this.value)"}})
                                 @Html.EditorFor(Function(model) model.Last.CatMName, New With {.htmlAttributes = New With {.id = "CAT_M_NAME", .class = "disable", .readonly = "readonly", .style = "width:790px;"}})
                             </td>
                             <td id="td-SSentaku">@* 分類区分で小分類選択時はコンボボックス表示にする  *@
                             @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                             <span class="clearable">
                                 @Html.DropDownListFor(Function(model) model.Last.ModalCatM, DirectCast(ViewBag.modalmodelCatM, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeCategoryM(this)", .style = "width: 420px;"})
                                 <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                             </span>
                         </td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_CAT_S_CD)</td>
                         <td width="20"></td>
                         <td>
                             @Html.EditorFor(Function(model) model.Last.CAT_S_CD, New With {.htmlAttributes = New With {.class = "text-box-cd-small", .maxlength = 8, .style = "width:85px;", .oninput = "editHanEi(this); GetCatSName(this.value)"}})
                             @Html.EditorFor(Function(model) model.Last.CatSName, New With {.htmlAttributes = New With {.id = "CAT_S_NAME", .class = "disable", .readonly = "readonly", .style = "width:790px;"}})
                         </td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_CAT_NAME)</td>
                         <td width="20"></td>
                         <td>@Html.EditorFor(Function(model) model.Last.CAT_NAME, New With {.htmlAttributes = New With {.maxlength = 60, .style = "width:790px;"}})</td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_ALERT)</td>
                         <td width="20"></td>
                         <td class="radio-group">
                             <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LEASE_LIMIT_ALERT_DIV, LeaseLimitAlertDiv.Front, New With {.id = "LEASE_LIMIT_ALERT_DIV_F"})@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_F)</div>
                             <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LEASE_LIMIT_ALERT_DIV, LeaseLimitAlertDiv.No, New With {.id = "LEASE_LIMIT_ALERT_DIV_N"})@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_N)</div>
                             <div class="col">
                                 @Html.RadioButtonFor(Function(model) model.Last.LEASE_LIMIT_ALERT_DIV, LeaseLimitAlertDiv.Yes, New With {.id = "LEASE_LIMIT_ALERT_DIV_Y"})@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_Y)
                                 @Html.TextBoxFor(Function(model) model.Last.LEASE_LIMIT_BEF_DAYS, New With {.class = "text-box-cd-small", .maxlength = 3, .oninput = "editNumber(this);", .style = "text-align: right;"})@Message.getMessage(CommonLabel.LABEL_BEF_DAYS)
                             </div>
                         </td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT_ALERT)</td>
                         <td width="20"></td>
                         <td class="radio-group">
                             <div class="col">@Html.RadioButtonFor(Function(model) model.Last.GENERIC_LIMIT_ALERT_DIV, GenericLimitAlertDiv.Front, New With {.id = "GENERIC_LIMIT_ALERT_DIV_F"})@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_F)</div>
                             <div class="col">@Html.RadioButtonFor(Function(model) model.Last.GENERIC_LIMIT_ALERT_DIV, GenericLimitAlertDiv.No, New With {.id = "GENERIC_LIMIT_ALERT_DIV_N"})@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_N)</div>
                             <div class="col">
                                 @Html.RadioButtonFor(Function(model) model.Last.GENERIC_LIMIT_ALERT_DIV, GenericLimitAlertDiv.Yes, New With {.id = "GENERIC_LIMIT_ALERT_DIV_Y"})@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_Y)
                                 @Html.TextBoxFor(Function(model) model.Last.GENERIC_LIMIT_BEF_DAYS, New With {.class = "text-box-cd-small", .maxlength = 3, .oninput = "editNumber(this);", .style = "text-align: right;"})@Message.getMessage(CommonLabel.LABEL_BEF_DAYS)
                             </div>
                         </td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_LEASE_ALERT)</td>
                         <td width="20"></td>
                         <td class="radio-group">
                             <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LEASE_ALERT_DIV, LeaseAlertDiv.Front, New With {.id = "LEASE_ALERT_DIV_F"})@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_F)</div>
                             <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LEASE_ALERT_DIV, LeaseAlertDiv.No, New With {.id = "LEASE_ALERT_DIV_N"})@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_N)</div>
                             <div class="col">
                                 @Html.RadioButtonFor(Function(model) model.Last.LEASE_ALERT_DIV, LeaseAlertDiv.Yes, New With {.id = "LEASE_ALERT_DIV_Y"})@Message.getMessage(CommonLabel.VALUE_ALERT_DIV_Y)
                             </div>
                         </td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_LEASE_ALERT_CNT)</td>
                         <td width="20"></td>
                         @*<td style="display:inline-block;width:150px;"> yamada*@
                         <td style="display:inline-block;width:100px;">
                             @Html.TextBoxFor(Function(model) model.Last.LEASE_ALERT_CNT, New With {.class = "text-box-cd-small", .maxlength = 5, .oninput = "editNumber(this);", .style = "text-align: right;"})
                         </td>
                         <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT_ALERT)</td>
                         @*<td class="condition-col radio-group" style="width:250px;"> yamada*@
                         <td class="condition-col radio-group" style="width:310px;">
                             <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LEASE_UPPER_ERR_DIV, LeaseUpperErrDiv.None, New With {.id = "LEASE_UPPER_ERR_DIV_NONE"})@Message.getMessage(CommonLabel.VALUE_LEASE_UPPER_ERR_DIV_NONE)</div>
                             <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LEASE_UPPER_ERR_DIV, LeaseUpperErrDiv.Yes, New With {.id = "LEASE_UPPER_ERR_DIV_Y"})@Message.getMessage(CommonLabel.VALUE_LEASE_UPPER_ERR_DIV_Y)</div>
                             <div class="col">@Html.RadioButtonFor(Function(model) model.Last.LEASE_UPPER_ERR_DIV, LeaseUpperErrDiv.No, New With {.id = "LEASE_UPPER_ERR_DIV_N"})@Message.getMessage(CommonLabel.VALUE_LEASE_UPPER_ERR_DIV_N)</div>
                         </td>
                         <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT)</td>
                         <td class="condition-col" style="width:100px;">
                             @Html.TextBoxFor(Function(model) model.Last.LEASE_UPPER_LIMIT, New With {.class = "text-box-cd-small", .maxlength = 5, .oninput = "editNumber(this);", .style = "text-align: right;"})
                         </td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabelCategory.LABEL_ASSET_ID_REG_DIV)</td>
                         <td width="20"></td>
                         <td>
                             @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                             <span class="clearable">
                                 @Html.DropDownListFor(Function(model) model.Last.ASSET_ID_REG_DIV, DirectCast(ViewBag.AssetIdRegDiv, IEnumerable(Of SelectListItem)), "", New With {.id = "ASSET_ID_REG_DIV", .onchange = "ChangeAssetIdRegDiv(this)"})
                                 <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                             </span>
                         </td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabelCategory.LABEL_EPC_SIZE)</td>
                         <td width="20"></td>
                         <td>
                             @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                             <span class="clearable">
                                 @Html.DropDownListFor(Function(model) model.Last.EPC_SIZE, DirectCast(ViewBag.EpcSize, IEnumerable(Of SelectListItem)), "", New With {.id = "EPC_SIZE", .style = "width: 100px;"})
                                 <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                             </span>
                         </td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_EPC_COMPANY)</td>
                         <td width="20"></td>
                         <td>
                             @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                             <span class="clearable">
                                 @Html.DropDownListFor(Function(model) model.Last.EPC_COMPANY_KEY, DirectCast(ViewBag.EpcCompanyKey, IEnumerable(Of SelectListItem)), "", New With {.id = "EPC_COMPANY_KEY"})
                                 <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                             </span>
                         </td>
                     </tr>
                     <tr Class="popup-row">
                         <td Class="head-col category-large">@Message.getMessage(CommonLabel.LABEL_EPC_ASSET_TYPE)</td>
                         <td width="20"></td>
                         <td>@Html.EditorFor(Function(model) model.Last.EPC_ASSET_TYPE, New With {.htmlAttributes = New With {.maxlength = 6, .style = "width:60px;", .oninput = "editNumber(this); "}})</td>
                     </tr>
                 </Table>

                 @*<div Class="PopupButton" style="margin:0 auto; width:500px;"> yamada*@
                 <div Class="PopupButton" style="margin:0 auto; width:560px;">
                     <span style="float:left;margin:15px 20px 0 60px;">
                         @*<input id="modal-create-btn" class="defButton modal-close modal-create-btn submit-button" formaction="~/Category/Create" type="submit" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)">*@
                         <input id="modal-create-btn" class="defButton  modal-create-btn submit-button" formaction="~/Category/Create" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnCreateModal_onclick()">
                     </span>
                     <span style="float:left;margin:15px 15% 0 0;">
                         <input id="modal-delete-btn" class="defButton submit-button" type="button" formaction="~/Category/Delete" name="Delete" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="return btnDelete_onclick()">
                     </span>
                     <span style="float:left;margin:15px 0;">
                         @* 戻るボタン押下時、更新しないと次のモーダル表示がおかしくなるのでsubmitに変更*@
                         @*<input id="modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">*@
                         <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
                     </span>
                     <div style="clear:both;"></div>
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
             @Html.HiddenFor(Function(model) model.Last.AssetIdDivReg)
             End Using
         </div>
