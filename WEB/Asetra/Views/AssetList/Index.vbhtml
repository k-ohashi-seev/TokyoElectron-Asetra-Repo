@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of AssetListViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelAssetList.TITLE)
    Dim AttachFileMaxSize As Long = Long.Parse(ConfigurationManager.AppSettings("AttachFileMaxSizeMB"))
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
End Code
<script>
    //
    //資産一覧画面
    //

    //変更画面に値をセット中かどうか
    var isSettingValues = false;

    $(function () {
        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            // 2019/12/24 miyaji upd start エラーメッセージ表示後、ライセンスコントローラーを呼び出すように修正
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            //エラーメッセージ表示後、ログイン画面に遷移
            @*showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/Login")/Index");*@
            // 2019/12/24 miyaji upd end
            return;
        }

        // オプション項目表示非表示設定
        if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            $("#cond").removeClass("in");
            $("#cond").addClass("collapse");
            document.getElementById("SearchCondition").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }
        //[CC431] ADD Start 20261015
        // 検索の再表示で、エラーなし・1 件以上のときは検索条件を折りたたむ（仕様変更1）。
        // エラー・0 件のときは追加しない（検索前の状態＝OpenFlg のまま）。ページ変更・モーダル戻りは検索ボタンの値を送らないため対象外
        if ("@Request.Form("actionname")" == "@Message.getMessage(CommonLabel.BUTTON_SEARCH)"
            && "@Model.FirstOrDefault.ErrCode" == "" && Number("@Model.FirstOrDefault.TotalCount") > 0) {
            $("#cond").removeClass("in");
            $("#cond").addClass("collapse");
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)";
            document.getElementById("OpenFlg").value = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)";
        }
        //[CC431] ADD End 20261015


        //20230904 ADD Start Shirakawa
        //上限を超えている場合エラーメッセージ
        if ("@ViewBag.isLimitFlg" == "True") {
            showErrMessage("@String.Format(Message.getMessage(CommonLabel.MSG_ERR_LIMIT_DATA), ViewBag.cntList, ViewBag.cntLimit)")

            //上限を超えている場合はCSV出力できないようにする。
            $("#csvBtn").prop("disabled", true);
            $("#csvBtn").attr('style', 'background-color:#cecece;');
            $("#csvBtn").css("cursor", "default");
        }
        //20230904 ADD End Shirakawa

        // オプション項目表示非表示設定
        @*if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "inline-block");
            document.getElementById("DetailAccordion").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }*@
        if ("@Model.FirstOrDefault.DetailOpenFlg" == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "flex");
            document.getElementById("DetailAccordion").innerHTML = "@Model.FirstOrDefault.DetailOpenFlg";
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
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessRetire") {
            //除却
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_RETIRE)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessHakko") {
            //タグ発行
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_HAKKO)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessRetireOff") {
            //除却解除
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_RETIRE_OFF)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_CREATE_CLICK") {
                //登録画面を表示
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CREATE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

                //除却日、貸出回数、資産状態、資産ＩＤ状態、タグ紐付け状態、削除ボタンを非表示
                $("#modal-delete-btn").css("display", "none");
                $("#modal-retire-btn").css("display", "none");
                $(".DisplayOnOff").css("display", "none");

                // 2023/03/22 Kawamoto ADD START
                // 添付ファイルボタンを使用不可
                $("#btnAttachment").prop("disabled", true);
                $("#btnAttachment").attr('style', 'background-color:#cecece;');
                $("#btnAttachment").css("cursor", "default");
                // 2023/03/22 Kawamoto ADD END

                // 貸出状態変更不可  // 2021/10/05 nv miyaji add
                $("#LEASE_STATUS").attr("disabled", "disabled");

                //有形/無形区分変更不可
                $('[id=IS_TANGIBLE_TANGIBLE]').prop("disabled", true);
                $('[id=IS_TANGIBLE_NOT_TANGIBLE]').prop("disabled", true);

                //ShowModalWindow() '2019/10/11 nv miyaji 登録か変更かのif文の外に出す
                ////$('.create-btn').trigger("click")
            } else {
                //変更画面を表示
                //タイトルの設定
                document.getElementById("popup-title").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";
                document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

                // 2023/03/22 Kawamoto ADD
                // 添付ファイルボタン名の設定
                document.getElementById("btnAttachment").value = "@Message.getMessage(CommonLabel.BUTTON_ATTACHMENT)".replace("[0]", "[" + "@Model.FirstOrDefault.ATTACHMENT_NUM" + "]");

                //有形/無形区分変更不可
                $('[id=IS_TANGIBLE_TANGIBLE]').prop("disabled", true);
                $('[id=IS_TANGIBLE_NOT_TANGIBLE]').prop("disabled", true);

                //除却日、貸出回数、資産状態、資産ＩＤ状態、タグ紐付け状態、削除ボタンを表示
                @* '--2021/09/27 nv miyaji update start *@
                // 権限による処理区分
                if ("@ViewBag.AuthProcFlg" == "@AuthProcFlg.All" || "@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiAll") {
                    // 権限による処理区分が「編集・照会可能」の場合
                    //削除・除却ボタンを表示
                    $("#modal-delete-btn").css("display", "block");
                    $("#modal-retire-btn").css("display", "block");
                } else {
                    // 権限による処理区分が「照会のみ」の場合（本来ここに入ることはないはずだが、念のため）
                    // 変更ボタンを非表示
                    $("#modal-create-btn").css("display", "none");
                    // 削除ボタンを非表示
                    $("#modal-delete-btn").css("display", "none");
                    // 除却ボタンを非表示
                    $("#modal-retire-btn").css("display", "none");
                };
                //$("#modal-delete-btn").css("display", "block");
                //$("#modal-retire-btn").css("display", "block");
                @* '--2021/09/27 nv miyaji update end *@
                $(".DisplayOnOff").css("display", "inline-block");

                //資産番号コード、除却日、貸出回数、資産状態、資産ＩＤ状態、タグ紐付け状態、カテゴリ大区分書き込み不可
                document.getElementById("ASSET_NO").readOnly = true;
                $("#ASSET_NO").addClass("disable");
                document.getElementById("AssetStatusNm").readOnly = true;
                $("#AssetStatusNm").addClass("disable");
                document.getElementById("EpcEncStatusNm").readOnly = true;
                $("#EpcEncStatusNm").addClass("disable");
                document.getElementById("TagLinkStatusNm").readOnly = true;
                $("#TagLinkStatusNm").addClass("disable");
                document.getElementById("RetireTsString").readOnly = true;
                $("#RetireTsString").addClass("disable");
                document.getElementById("LEASE_CNT").readOnly = true;
                $("#LEASE_CNT").addClass("disable");
                $("#CAT_L_CD").attr("disabled", "disabled");

                //資産IDの書き込み可不可設定
                if ("@ViewBag.AssetIdDivReg" == "False") {
                    $("#ASSET_ID").addClass("disable");
                    document.getElementById("ASSET_ID").readOnly = true;
                }

                // 除却/除却解除ボタン設定
                document.getElementById("modal-retire-btn").value = "@ViewBag.RetireBtnStr"

                // 2021/10/05 nv miyaji add start
                if ("@ViewBag.NotEnableLeaseStatus" == "True") {
                    // 貸出状態変更不可
                    $("#LEASE_STATUS").attr("disabled", "disabled");
                }
                // 2021/10/05 nv miyaji add end

                // 2023/05/10 Kawamoto ADD START
                // 薬品機能ONの場合
                if ("@ViewBag.DrugsFunc" == "True") {

                    // 計測器連携ありの計測器の場合
                    if ("@ViewBag.WeighingMachineLinkList" != "" && "@ViewBag.WeighingMachine" != "") {
                        if ("@ViewBag.WeighingMachineLinkList.Contains(ViewBag.WeighingMachine)" == "True") {
                            // 使用不可
                            document.getElementById("FREE_ITEM16").readOnly = true;
                            $("#FREE_ITEM16").addClass("disable");
                        }
                    }
                    document.getElementById("RemainingAmount").value = "@ViewBag.RemainingAmount";
                }
                // 2023/05/10 Kawamoto ADD END

                // 除却状態の時、変更ボタン非表示
                if ("@ViewBag.RetireBtnStr" == "@Message.getMessage(CommonLabel.BUTTON_RETIRE_OFF)") {
                    $("#modal-create-btn").css("display", "none");

                    // 2023/03/22 Kawamoto ADD START
                    // 添付ファイルボタンを使用不可
                    $("#btnAttachment").prop("disabled", true);
                    $("#btnAttachment").attr('style', 'background-color:#cecece;');
                    $("#btnAttachment").css("cursor", "default");
                    // 2023/03/22 Kawamoto ADD END

                    //--2019/10/11 nv miyaji add start 除却時、全ての項目を変更できなくする
                    document.getElementById("ASSET_ID").readOnly = true;
                    $("#ASSET_ID").addClass("disable");
                    $("#LOC_L_CD").attr("disabled", "disabled");
                    $("#LOC_M_CD").attr("disabled", "disabled");
                    $("#LOC_S_CD").attr("disabled", "disabled");
                    document.getElementById("ASSET_DIV").readOnly = true;
                    $("#ASSET_DIV").addClass("disable");
                    document.getElementById("ASSET_NAME").readOnly = true;
                    $("#ASSET_NAME").addClass("disable");
                    $("#CAT_M_CD").attr("disabled", "disabled");
                    $("#CAT_S_CD").attr("disabled", "disabled");
                    document.getElementById("MAKER").readOnly = true;
                    $("#MAKER").addClass("disable");
                    document.getElementById("PRODUCT_NO").readOnly = true;
                    $("#PRODUCT_NO").addClass("disable");
                    document.getElementById("MODEL_NO").readOnly = true;
                    $("#MODEL_NO").addClass("disable");
                    document.getElementById("LEASE_NO").readOnly = true;
                    $("#LEASE_NO").addClass("disable");
                    document.getElementById("LeaseLimitString").readOnly = true;
                    $("#LeaseLimitString").addClass("disable");
                    document.getElementById("GenLimitString").readOnly = true;
                    $("#GenLimitString").addClass("disable");

                    document.getElementById("FREE_ITEM2").readOnly = true;
                    $("#FREE_ITEM2").addClass("disable");
                    // 2023/02/08 Kawamoto UPD START
                    if ("@ViewBag.DrugsFunc" == "False") {
                        // 薬品機能OFFの場合
                        document.getElementById("FREE_ITEM3").readOnly = true;
                        $("#FREE_ITEM3").addClass("disable");
                        document.getElementById("FREE_ITEM4").readOnly = true;
                        $("#FREE_ITEM4").addClass("disable");
                        document.getElementById("FREE_ITEM5").readOnly = true;
                        $("#FREE_ITEM5").addClass("disable");
                        document.getElementById("FREE_ITEM6").readOnly = true;
                        $("#FREE_ITEM6").addClass("disable");
                        document.getElementById("FREE_ITEM7").readOnly = true;
                        $("#FREE_ITEM7").addClass("disable");
                        document.getElementById("FREE_ITEM8").readOnly = true;
                        $("#FREE_ITEM8").addClass("disable");
                        document.getElementById("FREE_ITEM9").readOnly = true;
                        $("#FREE_ITEM9").addClass("disable");
                    } else {
                        // 薬品機能ONの場合
                        $("#FREE_ITEM3").attr("disabled", "disabled");
                        $("#FREE_ITEM4").attr("disabled", "disabled");
                        $("#FREE_ITEM5").attr("disabled", "disabled");
                        $("#FREE_ITEM6").attr("disabled", "disabled");
                        $("#FREE_ITEM7").attr("disabled", "disabled");
                        $("#FREE_ITEM8").attr("disabled", "disabled");
                        $("#FREE_ITEM9").attr("disabled", "disabled");
                    }
                    // 2023/02/08 Kawamoto UPD END
                    // 2023/02/08 Kawamoto ADD START
                    document.getElementById("FREE_ITEM10").readOnly = true;
                    $("#FREE_ITEM10").addClass("disable");
                    document.getElementById("FREE_ITEM11").readOnly = true;
                    $("#FREE_ITEM11").addClass("disable");
                    document.getElementById("FREE_ITEM12").readOnly = true;
                    $("#FREE_ITEM12").addClass("disable");
                    document.getElementById("FREE_ITEM13").readOnly = true;
                    $("#FREE_ITEM13").addClass("disable");
                    document.getElementById("FREE_ITEM14").readOnly = true;
                    $("#FREE_ITEM14").addClass("disable");
                    if ("@ViewBag.DrugsFunc" == "False") {
                        // 薬品機能OFFの場合
                        document.getElementById("FREE_ITEM15").readOnly = true;
                        $("#FREE_ITEM15").addClass("disable");
                    } else {
                        // 薬品機能ONの場合
                        $("#FREE_ITEM15").attr("disabled", "disabled");
                    }
                    document.getElementById("FREE_ITEM16").readOnly = true;
                    $("#FREE_ITEM16").addClass("disable");
                    document.getElementById("FREE_ITEM17").readOnly = true;
                    $("#FREE_ITEM17").addClass("disable");
                    document.getElementById("FREE_ITEM18").readOnly = true;
                    $("#FREE_ITEM18").addClass("disable");
                    document.getElementById("FREE_ITEM19").readOnly = true;
                    $("#FREE_ITEM19").addClass("disable");
                    // 2023/02/08 Kawamoto ADD END
                    $("#ASSET_PIC_FILENAME").addClass("disable");
                    $("#ASSET_PIC_FILENAME").removeAttr("onclick");
                    $("#select_file_button").prop("disabled", true);
                    $("#select_file_button").attr('style', 'background-color:#cecece;');
                    $("#select_file_clear_button").prop("disabled", true);
                    $("#select_file_clear_button").attr('style', 'background-color:#cecece;margin-left:300px;');
                    //--2021/09/17 nv miyaji add start
                    $("#MNG_LOC_L_CD").attr("disabled", "disabled");
                    $("#LEASE_STATUS").attr("disabled", "disabled");
                    document.getElementById("MAINTE_LEASE_NUM").readOnly = true;
                    $("#MAINTE_LEASE_NUM").addClass("disable");
                    document.getElementById("MAINTE_LEASE_DAY").readOnly = true;
                    $("#MAINTE_LEASE_DAY").addClass("disable");
                    $('#DispLeaseDayWorkFlg').prop('disabled', true);
                    document.getElementById("MAINTE_DAY").readOnly = true;
                    $("#MAINTE_DAY").addClass("disable");
                    $('#DispMainteDayWorkFlg').prop('disabled', true);
                    //--2021/09/17 nv miyaji add end

                    //--2019/10/11 nv miyaji add end
                    //--2021/10/13 sv suzuki add start
                    $("#reflect_button").prop("disabled", true);
                    $("#reflect_button").attr('style', 'background-color:#cecece;');
                    //--2021/10/13 sv suzuki add end
                }
            }

            //--2019/10/11 nv miyaji add start
            var filenm = $('#ASSET_PIC_FILENAME').val();
            if (filenm != "") {
                var imageUrl = document.getElementById("NowImageUrl").value;
                document.getElementById("preview").src = imageUrl;
            }
            //--2019/10/11 nv miyaji add end

            ShowModalWindow()
            //$('.update-btn').trigger("click")

            // 2023/02/08 Kawamoto ADD START
            if ("@Model.FirstOrDefault.ErrTarget".search("Target_") >= 0) {
                showErrMessage("@Model.FirstOrDefault.ErrCode", "#@Model.FirstOrDefault.ErrTarget".replace("Target_",""))
            } else {
                showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
            }
            // 2023/02/08 Kawamoto ADD END

        }else{
            //ロード前のボタン名を設定する
            document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"
        }

        // ロケーション大区分非選択時は中区分を非活性   // 2020/03/17 nv miyaji upd
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocL").value == "") {
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        // ロケーション中区分非選択時は小区分を非活性   // 2020/03/17 nv miyaji upd
        //[CC431] UPD Start 20261007
        // ※場所から選ぶ ON の場合は、小区分非選択時に中区分を非活性
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocL").value == "" || (!isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocM").length == 0)) {
            clearMultiSelect("#SearchLocS");
            $("#SearchLocS").attr("disabled", "disabled");
        }
        if (isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocS").length == 0) {
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        //[CC431] UPD End 20261007

        // 登録変更画面　ロケーション大区分非選択時は中区分を非活性    // 2020/03/17 nv miyaji add
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes" || document.getElementById("LOC_L_CD").value == "") {
            document.getElementById("LOC_M_CD").value = ""
            $("#LOC_M_CD").attr("disabled", "disabled");
        }
        // 登録変更画面　ロケーション中区分非選択時は小区分を非活性    // 2020/03/17 nv miyaji add
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes" || document.getElementById("LOC_M_CD").value == "") {
            document.getElementById("LOC_S_CD").value = "";
            $("#LOC_S_CD").attr("disabled", "disabled");
        }

        // 登録変更画面　カテゴリー大分類非選択時は中区分を非活性    // 2020/03/17 nv miyaji add
        if (document.getElementById("CAT_L_CD").value == "") {
            document.getElementById("CAT_M_CD").value = ""
            $("#CAT_M_CD").attr("disabled", "disabled");
        }
        // 登録変更画面　カテゴリー中分類非選択時は小区分を非活性    // 2020/03/17 nv miyaji add
        if (document.getElementById("CAT_M_CD").value == "") {
            document.getElementById("CAT_S_CD").value = "";
            $("#CAT_S_CD").attr("disabled", "disabled");
        }

        // モーダルダイアログの期限エディットボックス日付フォーマット
        $.datepicker.setDefaults({
            showOn: "focus",
            //dateFormat: 'yymmdd'
        });
        $("#LEASE_LIMIT").datepicker("option", "dateFormat", 'yymmdd');
        $("#GENERIC_LIMIT").datepicker("option", "dateFormat", 'yymmdd');

        // 返却期限日付範囲を非活性
        if ($('#SearchHenkyakuLimitKikan').prop('checked') == false){
            $("#SearchHenkyakuLimitFrom").prop("disabled", true);
            $("#SearchHenkyakuLimitTo").prop("disabled", true);
        }

        // リース期限日付範囲を非活性
        if ($('#SearchLeaseLimitKikan').prop('checked') == false){
            $("#SearchLeaseLimitFrom").prop("disabled", true);
            $("#SearchLeaseLimitTo").prop("disabled", true);
        }

        // ○○期限日付範囲を非活性
        if ($('#SearchGenericLimitKikan').prop('checked') == false) {
            $("#SearchGenericLimitFrom").prop("disabled", true);
            $("#SearchGenericLimitTo").prop("disabled", true);
        }

        // キー値が存在する場合の項目非表示設定
        if ("@ViewBag.AssetListKey") {
            $(".search-wrap").hide();
            $("#btnPrnQRDaicho").hide();
            $("#btnShowQR").hide();
            $("#btnSelectAll").hide();
            $("#btnCreate").hide();
            $("#btnPrintTag").hide();
            $("#btnExcelImport").hide();
            $("#btnCsvExport").hide();
            $("#btnReturn").show(); //2019/11/11 nv miyaji add
        }else {
            //$("#btnReturn").hide(); //2019/11/11 nv miyaji コメントアウト
        }

        //隠し項目に格納
        @*document.getElementById("AssetListKey").value = "@ViewBag.AssetListKey";*@
        document.getElementById("LoadingImg").value = '@Url.Content("~/Images/")' + "Loading.gif" ;

        btnExcelImportEnabled(true)

        // 列定義
        let columns = [
            { id: "@ColKey.AssetList.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.CHECK_SELECT", 40), sortOrder: -2, formatter: AssetListCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.AssetList.CHANGE_OR_DISP", name: "", field: "", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.CHANGE_OR_DISP", 76), sortOrder: -1, formatter: changeOrDispBtnFormatter},
            { id: "@ColKey.AssetList.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.ASSET_NO", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag", formatter: AssetNoFormatter},
            { id: "@ColKey.AssetList.SUB_ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_SUB_ASSET_NO)", field: "SUB_ASSET_NO", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.SUB_ASSET_NO", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispFlag"},
            { id: "@ColKey.AssetList.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)", field: "ASSET_NAME", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.ASSET_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag" },
            { id: "@ColKey.AssetList.MNG_LOC_L_CD_NAMED", name: "@Message.getMessage(CommonLabel.LABEL_MNG_LOC_L_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.MNG_LOC_L_CD_NAMED", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag", formatter: MngLocLFormatter},
            { id: "@ColKey.AssetList.ASSET_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_STATUS)", field: "ASSET_STATUS", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.ASSET_STATUS", 70), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_STATUS").DispFlag", formatter: AssetStatusFormatter},
            { id: "@ColKey.AssetList.LEASE_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)", field: "LEASE_STATUS", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LEASE_STATUS", 70), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_STATUS").DispFlag", formatter: LeaseStatusFormatter},
            { id: "@ColKey.AssetList.LEASE_COUNT", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_COUNT)", field: "LEASE_CNT", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LEASE_COUNT", 70), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_CNT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_CNT").DispFlag", formatter: RightAlignFormatter},
            { id: "@ColKey.AssetList.LEASE_KIKAN", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_KIKAN)", field: "LEASE_DT", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LEASE_KIKAN", 190), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_DT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_DT").DispFlag", formatter: LeaseKikanFormatter},
            { id: "@ColKey.AssetList.LEASE_PLACE_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD_NAME)", field: "LEASE_PLACE_CD", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LEASE_PLACE_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_PLACE_CD").DispFlag", formatter: LeasePlaceFormatter},
            { id: "@ColKey.AssetList.LEASE_REMARK", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_REMARK)", field: "REMARK", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LEASE_REMARK", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK").DispFlag"},
            { id: "@ColKey.AssetList.LEASE_EMPLOYEE_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_EMPLOYEE_CD_NAME)", field: "LEASE_EMPLOYEE_CD", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LEASE_EMPLOYEE_CD_NAME", 230), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_EMPLOYEE_CD").DispFlag", formatter: LeaseEmpFormatter},
            { id: "@ColKey.AssetList.MAINTE_LEASE_NUM", name: "@Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_NUM)", field: "MAINTE_LEASE_NUM", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.MAINTE_LEASE_NUM", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("MAINTE_LEASE_NUM").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAINTE_LEASE_NUM").DispFlag", formatter: RightFormatter },
            { id: "@ColKey.AssetList.MAINTE_LEASE_DAY", name: "@Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY)", field: "MAINTE_LEASE_DAY", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.MAINTE_LEASE_DAY", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("MAINTE_LEASE_DAY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAINTE_LEASE_DAY").DispFlag", formatter:  CenterFormatter },
            { id: "@ColKey.AssetList.MAINTE_DAY", name: "@Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)", field: "MAINTE_DAY", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.MAINTE_DAY", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("MAINTE_DAY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAINTE_DAY").DispFlag", formatter: CenterFormatter},
            { id: "@ColKey.AssetList.LOC_L_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_LOC_L_COM_NAME)", field: "LOC_L_CD", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LOC_L_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag", formatter: LocLFormatter },
            { id: "@ColKey.AssetList.LOC_M_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_NAME)", field: "LOC_M_CD", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LOC_M_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispFlag", formatter: LocMFormatter},
            { id: "@ColKey.AssetList.LOC_S_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_NAME)", field: "LOC_S_CD", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LOC_S_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispFlag", formatter: LocSFormatter},
            { id: "@ColKey.AssetList.CAT_L_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)", field: "CAT_L_CD", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.CAT_L_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag", formatter: CatLFormatter},
            { id: "@ColKey.AssetList.CAT_M_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CAT_M_CD_NAME)", field: "CAT_M_CD", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.CAT_M_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispFlag", formatter: CatMFormatter},
            { id: "@ColKey.AssetList.CAT_S_CD_NAME", name: "@Message.getMessage(CommonLabel.LABEL_CAT_S_CD_NAME)", field: "CAT_S_CD", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.CAT_S_CD_NAME", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispFlag", formatter: CatSFormatter},
            { id: "@ColKey.AssetList.GENERIC_LIMIT", name: "@Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)", field: "GENERIC_LIMIT", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.GENERIC_LIMIT", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("GENERIC_LIMIT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("GENERIC_LIMIT").DispFlag", formatter: SlashYMDFormatter},
            { id: "@ColKey.AssetList.MAKER", name: "@Message.getMessage(CommonLabel.LABEL_MAKER)", field: "MAKER", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.MAKER", 230), sortOrder: "@Model.FirstOrDefault.GridInfo("MAKER").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAKER").DispFlag"},
            { id: "@ColKey.AssetList.PRODUCT_NO", name: "@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)", field: "PRODUCT_NO", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.PRODUCT_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispFlag"},
            { id: "@ColKey.AssetList.MODEL_NO", name: "@Message.getMessage(CommonLabel.LABEL_MODEL_NO)", field: "MODEL_NO", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.MODEL_NO", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispFlag"},
            { id: "@ColKey.AssetList.ASSET_DIV", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_DIV)", field: "ASSET_DIV", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.ASSET_DIV", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_DIV").DispFlag"},
            { id: "@ColKey.AssetList.LEASE_NO", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_NO)", field: "LEASE_NO", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LEASE_NO", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_NO").DispFlag"},
            { id: "@ColKey.AssetList.LEASE_LIMIT", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)", field: "LEASE_LIMIT", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.LEASE_LIMIT", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_LIMIT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_LIMIT").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM1", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM1)", field: "FREE_ITEM1", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM1", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispFlag" },
            { id: "@ColKey.AssetList.FREE_ITEM2", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)", field: "FREE_ITEM2", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM2", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM3", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM3)", field: "FREE_ITEM3", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM3", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM4", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM4)", field: "FREE_ITEM4", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM4", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM5", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM5)", field: "FREE_ITEM5", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM5", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM6", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6)", field: "FREE_ITEM6", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM6", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispFlag", formatter: MngEmpFormatter},
            { id: "@ColKey.AssetList.FREE_ITEM7", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM7)", field: "FREE_ITEM7", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM7", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM8", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM8)", field: "FREE_ITEM8", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM8", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM9", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM9)", field: "FREE_ITEM9", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM9", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM10", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM10)", field: "FREE_ITEM10", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM10", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM11", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM11)", field: "FREE_ITEM11", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM11", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispFlag", formatter: CommaFormatter, cssClass: "cell-right"},
            { id: "@ColKey.AssetList.FREE_ITEM12", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM12)", field: "FREE_ITEM12", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM12", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispFlag", formatter: CommaFormatter, cssClass: "cell-right"},
            { id: "@ColKey.AssetList.FREE_ITEM13", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM13)", field: "FREE_ITEM13", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM13", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM14", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM14)", field: "FREE_ITEM14", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM14", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM15", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)", field: "FREE_ITEM15", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM15", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM16", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM16)", field: "FREE_ITEM16", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM16", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM17", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM17)", field: "FREE_ITEM17", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM17", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM18", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)", field: "FREE_ITEM18", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM18", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM19", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)", field: "FREE_ITEM19", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM19", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM20", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM20)", field: "FREE_ITEM20", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM20", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM20").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM20").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM21", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM21)", field: "FREE_ITEM21", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM21", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM21").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM21").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM22", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM22)", field: "FREE_ITEM22", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM22", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM22").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM22").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM23", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM23)", field: "FREE_ITEM23", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM23", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM23").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM23").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM24", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM24)", field: "FREE_ITEM24", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM24", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM24").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM24").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM25", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM25)", field: "FREE_ITEM25", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM25", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM25").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM25").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM26", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM26)", field: "FREE_ITEM26", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM26", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM26").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM26").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM27", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM27)", field: "FREE_ITEM27", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM27", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM27").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM27").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM28", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM28)", field: "FREE_ITEM28", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM28", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM28").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM28").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM29", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM29)", field: "FREE_ITEM29", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM29", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM29").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM29").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM30", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM30)", field: "FREE_ITEM30", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM30", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM30").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM30").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM31", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM31)", field: "FREE_ITEM31", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM31", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM31").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM31").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM32", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM32)", field: "FREE_ITEM32", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM32", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM32").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM32").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM33", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM33)", field: "FREE_ITEM33", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM33", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM33").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM33").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM34", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM34)", field: "FREE_ITEM34", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM34", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM34").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM34").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM35", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM35)", field: "FREE_ITEM35", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM35", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM35").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM35").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM36", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM36)", field: "FREE_ITEM36", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM36", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM36").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM36").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM37", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM37)", field: "FREE_ITEM37", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM37", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM37").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM37").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM38", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM38)", field: "FREE_ITEM38", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM38", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM38").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM38").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM39", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM39)", field: "FREE_ITEM39", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM39", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM39").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM39").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM40", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM40)", field: "FREE_ITEM40", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM40", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM40").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM40").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM41", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM41)", field: "FREE_ITEM41", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM41", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM41").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM41").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM42", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM42)", field: "FREE_ITEM42", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM42", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM42").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM42").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM43", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM43)", field: "FREE_ITEM43", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM43", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM43").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM43").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM44", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM44)", field: "FREE_ITEM44", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM44", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM44").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM44").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM45", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM45)", field: "FREE_ITEM45", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM45", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM45").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM45").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM46", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM46)", field: "FREE_ITEM46", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM46", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM46").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM46").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM47", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM47)", field: "FREE_ITEM47", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM47", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM47").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM47").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM48", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM48)", field: "FREE_ITEM48", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM48", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM48").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM48").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM49", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM49)", field: "FREE_ITEM49", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM49", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM49").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM49").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM50", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM50)", field: "FREE_ITEM50", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM50", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM50").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM50").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM51", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM51)", field: "FREE_ITEM51", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM51", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM51").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM51").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM52", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM52)", field: "FREE_ITEM52", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM52", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM52").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM52").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM53", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM53)", field: "FREE_ITEM53", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM53", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM53").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM53").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM54", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM54)", field: "FREE_ITEM54", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM54", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM54").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM54").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM55", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM55)", field: "FREE_ITEM55", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM55", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM55").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM55").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM56", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM56)", field: "FREE_ITEM56", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM56", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM56").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM56").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM57", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM57)", field: "FREE_ITEM57", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM57", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM57").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM57").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM58", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM58)", field: "FREE_ITEM58", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM58", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM58").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM58").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM59", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM59)", field: "FREE_ITEM59", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM59", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM59").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM59").DispFlag"},
            { id: "@ColKey.AssetList.FREE_ITEM60", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM60)", field: "FREE_ITEM60", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.FREE_ITEM60", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM60").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM60").DispFlag"},
            { id: "@ColKey.AssetList.ASSET_ID", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_ID)", field: "ASSET_ID", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.ASSET_ID", 300), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_ID").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_ID").DispFlag" },
            { id: "@ColKey.AssetList.ASSET_ID_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_ID_STATUS)", field: "EPC_ENC_STATUS", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.ASSET_ID_STATUS", 90), sortOrder: "@Model.FirstOrDefault.GridInfo("EPC_ENC_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("EPC_ENC_STATUS").DispFlag", formatter: EpcEncStatusFormatter},
            { id: "@ColKey.AssetList.TAG_LINK_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_TAG_LINK_STATUS)", field: "TAG_LINK_STATUS", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.TAG_LINK_STATUS", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("TAG_LINK_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TAG_LINK_STATUS").DispFlag", formatter: TagLinkStatusFormatter},
            { id: "@ColKey.AssetList.IS_TANGIBLE", name: "@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)", field: "IS_TANGIBLE", width: getColumnWidth("@DispId.ASSET_LIST", "@ColKey.AssetList.IS_TANGIBLE", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("IS_TANGIBLE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("IS_TANGIBLE").DispFlag", formatter: IsTangibleFormatter},
            { id: "@ColKey.AssetList.REMARK1", name: "@Message.getMessage(CommonLabel.LABEL_REMARK1)", field: "REMARK1", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK1", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK1").DispFlag"},
            { id: "@ColKey.AssetList.REMARK2", name: "@Message.getMessage(CommonLabel.LABEL_REMARK2)", field: "REMARK2", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK2", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK2").DispFlag"},
            { id: "@ColKey.AssetList.REMARK3", name: "@Message.getMessage(CommonLabel.LABEL_REMARK3)", field: "REMARK3", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK3", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK3").DispFlag"},
            { id: "@ColKey.AssetList.REMARK4", name: "@Message.getMessage(CommonLabel.LABEL_REMARK4)", field: "REMARK4", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK4", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK4").DispFlag"},
            { id: "@ColKey.AssetList.REMARK5", name: "@Message.getMessage(CommonLabel.LABEL_REMARK5)", field: "REMARK5", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK5", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK5").DispFlag"},
            { id: "@ColKey.AssetList.REMARK6", name: "@Message.getMessage(CommonLabel.LABEL_REMARK6)", field: "REMARK6", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK6", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK6").DispFlag"},
            { id: "@ColKey.AssetList.REMARK7", name: "@Message.getMessage(CommonLabel.LABEL_REMARK7)", field: "REMARK7", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK7", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK7").DispFlag"},
            { id: "@ColKey.AssetList.REMARK8", name: "@Message.getMessage(CommonLabel.LABEL_REMARK8)", field: "REMARK8", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK8", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK8").DispFlag"},
            { id: "@ColKey.AssetList.REMARK9", name: "@Message.getMessage(CommonLabel.LABEL_REMARK9)", field: "REMARK9", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK9", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK9").DispFlag"},
            { id: "@ColKey.AssetList.REMARK10", name: "@Message.getMessage(CommonLabel.LABEL_REMARK10)", field: "REMARK10", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.REMARK10", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK10").DispFlag"},
            { id: "@ColKey.AssetList.INS_DATETIME", name: "@Message.getMessage(CommonLabel.LABEL_INS_DATETIME)", field: "INS_DATETIME", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.INS_DATETIME", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("INS_DATETIME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INS_DATETIME").DispFlag", formatter: SlashYMDHMFormatter },
            { id: "@ColKey.AssetList.INS_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_INS_EMPLOYEE_NAME)", field: "INS_EMPLOYEE_NAME", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.INS_EMPLOYEE_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("INS_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("INS_EMPLOYEE_NAME").DispFlag"},
            { id: "@ColKey.AssetList.UPD_DATETIME", name: "@Message.getMessage(CommonLabel.LABEL_UPD_DATETIME)", field: "UPD_DATETIME", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.UPD_DATETIME", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("UPD_DATETIME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("UPD_DATETIME").DispFlag", formatter: SlashYMDHMFormatter},
            { id: "@ColKey.AssetList.UPD_EMPLOYEE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_UPD_EMPLOYEE_NAME)", field: "UPD_EMPLOYEE_NAME", width: getColumnWidth("@DispDiv.ASSET_LIST", "@ColKey.AssetList.UPD_EMPLOYEE_NAME", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("UPD_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("UPD_EMPLOYEE_NAME").DispFlag"},
        ];

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

        // 選択状態を復元
        let lstCheckedKey = @Html.Raw(jsonCheckKeys);
        for (let i = 0; i < lstCheckedKey.length; i++) {
            // Key1:資産番号 Key2:資産ID key3:カテゴリ大区分コード
            var keys = lstCheckedKey[i].split(",");

            for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                if (keys[0] === jsonData[grid_i].ASSET_NO) {
                    jsonData[grid_i].DISP_FLAG = lstCheckedKey[i]
                    break;
                }
            }
        }

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "ASSET_NO");
            dataView.endUpdate();
        }

        // 行やセルにメタデータをセットするためのイベント
        dataView.getItemMetadata = function(row) {
            let item = this.getItem(row);
            let isExpired = false;
            let isAlert = false;
            let leaseDueDt = String(item.LEASE_DUE_DT)
            let leaseLimitBefDays = String(item.LEASE_LIMIT_BEF_DAYS)
            let leaseLimit = String(item.LEASE_LIMIT)
            let genericLimitBefDays = String(item.GENERIC_LIMIT_BEF_DAYS)
            let genericLimit = String(item.GENERIC_LIMIT)

            // 返却期限
            if (leaseDueDt != null) {
                let dtHenkyakuLimit = parseDateyyyyMMdd(leaseDueDt);
                if (dtHenkyakuLimit != null) {
                    if (dtHenkyakuLimit < new Date()) {
                        // システム日が返却予定日よりも大きい場合は期限切れ
                        isExpired = true;
                    }
                }
            }

            // リース期限
            if (leaseLimitBefDays && leaseLimit != null && leaseLimitBefDays != -1) {
                let dtLeaseLimit = parseDateyyyyMMdd(leaseLimit);
                if (dtLeaseLimit != null) {
                    if (dtLeaseLimit < new Date()) {
                        // システム日がリース期限日よりも大きい場合は期限切れ
                        isExpired = true;
                    } else {
                        // 差分の日数が何日前の値よりも小さい場合はアラート
                        let ts = dtLeaseLimit - new Date();
                        let daysDiff = Math.ceil(ts / (1000 * 60 * 60 * 24)); // ミリ秒を日数に変換

                        if (daysDiff <= item.LEASE_LIMIT_BEF_DAYS) {
                            isAlert = true;
                        }
                    }
                }
            }

            // ○○期限
            if (genericLimitBefDays && genericLimit != null && genericLimitBefDays != -1) {
                let dtGenericLimit = parseDateyyyyMMdd(genericLimit);
                if (dtGenericLimit != null) {
                    if (dtGenericLimit < new Date()) {
                        // システム日がリース期限日よりも大きい場合は期限切れ
                        isExpired = true;
                    } else {
                        // 差分の日数が何日前の値よりも小さい場合はアラート
                        let ts = dtGenericLimit - new Date();
                        let daysDiff = Math.ceil(ts / (1000 * 60 * 60 * 24)); // ミリ秒を日数に変換

                        if (daysDiff <= item.LEASE_LIMIT_BEF_DAYS) {
                            isAlert = true;
                        }
                    }
                }
            }

            // 貸出回数
            if (item.LEASE_ALERT_CNT > -1) {
                if (item.LEASE_ALERT_CNT <= item.LEASE_CNT) {
                    isAlert = true;
                }
            }

            // 適用するCSSクラスを設定。期限切れ優先。
            if (isExpired) {
                return {cssClasses: 'slick-expired'};
            } else if (isAlert) {
                return {cssClasses: 'slick-alert'};
            }

            return null;
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 2       //選択列と変更列を加算
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispDiv.ASSET_LIST", fixCol);

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

        //ローカルストレージから復元
        const saved = localStorage.getItem("AssetListConditions");
        if ("@Model.FirstOrDefault.IsRestoreOwnSearchConditions" == "True") {
            var lcd = document.getElementById("SearchLocL");
            //最初の要素（×での選択解除用に追加した空の選択肢は除く）
            var firstLoc = $(lcd).find("option").filter(function () { return this.value !== ""; })[0];
            if (saved) {
                const cond = JSON.parse(saved);
                if (!cond.SearchLocL || cond.SearchLocL.trim() === "") {
                     //ローカルストレージが空の場合は最初の要素を選択
                    if (firstLoc) {
                        lcd.value = firstLoc.value;
                    }
                } else {
                    //ローカルストレージが空でない場合
                    var found = false;
                    for (var i = 0; i < lcd.options.length; i++) {
                        //ローカルストレージに該当する値があれば選択
                        if (lcd.options[i].value == cond.SearchLocL) {
                            lcd.value = cond.SearchLocL;
                            found = true;
                            break;
                        }
                    }
                    if (!found && firstLoc) {
                        //見つからなかった場合は最初の要素を選択
                        lcd.value = firstLoc.value;
                    }
                }
            } else {
                 //ローカルストレージが空の場合は最初の要素を選択
                if (firstLoc) {
                    lcd.value = firstLoc.value;
                }
            }

            //選択した内容をSelect2の表示に反映
            //※trigger("change.select2") だと onchange（ChangeLocationL）も呼ばれ、中区分・小区分がクリアされるため refreshSelect2 を使用
            refreshSelect2(lcd);

            //ロケ大の値が変わった時のメソッド実行（中区分・小区分は複数選択のため配列で渡す）
            ChangeLocationL(lcd, getMultiSelectValues("#SearchLocM"), getMultiSelectValues("#SearchLocS"), true);
        }
    });
        
    //ローカルストレージ保存
    function localStorageSave() {
        let cond = { SearchLocL: document.getElementById("SearchLocL").value };

        //ローカルストレージを保存
        localStorage.setItem("AssetListConditions", JSON.stringify(cond));
    }

    // 変更 or 照会ボタン
    function changeOrDispBtnFormatter(row, cell, value, columnDef, dataContext) {
        let assetNo = convertToEmptyIfNull(dataContext.ASSET_NO);
        if (dataContext.UPD_FLG === "@UpdFlg.Yes") {
            return '<input class="change-btn" type="button" value="' + "@Message.getMessage(CommonLabel.BUTTON_CHANGE)" + '" onclick="btnEdit_onclick(this.parentNode.parentNode, true, \'' + assetNo + '\')" />';
        } else {
            return '<input class="change-btn" type="button" value="' + "@Message.getMessage(CommonLabel.BUTTON_DISP)" + '" onclick="btnEdit_onclick(this.parentNode.parentNode, false, \'' + assetNo + '\')" />';
        }
    }

    // チェックボックス(中央寄せ)
    function AssetListCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.ASSET_NO) + ',' + convertToEmptyIfNull(dataContext.ASSET_ID) + ',' + convertToEmptyIfNull(dataContext.CAT_L_CD);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input id="listcheck" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" '+
               (isKeyInclude ? 'checked="checked" ' : '') +
               'onchange="check_onclick(this)" ' +
               '/>' +
               '</div>';
    }

    // 資産番号用formatter
    function AssetNoFormatter(row, cell, value, columnDef, dataContext) {
        return '<a href="#" onclick="SetAssetNo(this.text)" class="blue-link">' + value + '</a>';
    }

    // 管理ロケ-ションコード用formatter
    function MngLocLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.MNG_LOC_L_CD) + ' ' + convertToEmptyIfNull(dataContext.MngLocLName);
    }

    // ロケ-ション大区分コード用formatter
    function LocLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_L_CD) + ' ' + convertToEmptyIfNull(dataContext.LocLName);
    }

    // ロケ-ション中区分コード用formatter
    function LocMFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_M_CD) + ' ' + convertToEmptyIfNull(dataContext.LocMName);
    }

    // ロケ-ション小区分コード用formatter
    function LocSFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_S_CD) + ' ' + convertToEmptyIfNull(dataContext.LocSName);
    }

    // 貸出先コード用formatter
    function LeasePlaceFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LEASE_PLACE_CD) + ' ' + convertToEmptyIfNull(dataContext.LEASE_PLACE_NAME);
    }

    // 貸出先担当社員コード用formatter
    function LeaseEmpFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LEASE_EMPLOYEE_CD) + ' ' + convertToEmptyIfNull(dataContext.LEASE_EMPLOYEE_NAME);
    }

    function MngEmpFormatter(row, cell, value, columnDef, dataContext) {
        var sName = convertToEmptyIfNull(dataContext.FREE_ITEM6_NAME);
        if (sName == '') {
            return convertToEmptyIfNull(dataContext.FREE_ITEM6);
        }
        return convertToEmptyIfNull(dataContext.FREE_ITEM6) + ' ' + sName;
    }

    function CommaFormatter(row, cell, value, columnDef, dataContext) {
        var sValue = convertToEmptyIfNull(value);
        if (sValue == '') {
            return '';
        }
        var nValue = Number(sValue);
        if (isNaN(nValue)) {
            return sValue;
        }
        var aParts = sValue.split('.');
        var nDecimals = (aParts.length > 1) ? aParts[1].length : 0;
        return nValue.toLocaleString('ja-JP', { minimumFractionDigits: nDecimals, maximumFractionDigits: nDecimals });
    }

    // カテゴリ大分類コード用formatter
    function CatLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_L_CD) + ' ' + convertToEmptyIfNull(dataContext.CatLName);
    }

    // カテゴリ中分類コード用formatter
    function CatMFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_M_CD) + ' ' + convertToEmptyIfNull(dataContext.CatMName);
    }

    // カテゴリ小分類コード用formatter
    function CatSFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_S_CD) + ' ' + convertToEmptyIfNull(dataContext.CatSName);
    }

    // 貸出期間用formatter
    function LeaseKikanFormatter(row, cell, value, columnDef, dataContext) {
        let sLeaseDt = "　　　　　"; // 空白を埋めるためのスペース
        if (dataContext.LEASE_DT) {
            sLeaseDt = toStringDateYMD(String(dataContext.LEASE_DT));
        }

        let sLeaseDueDt = "";
        if (dataContext.LEASE_DUE_DT) {
            sLeaseDueDt = toStringDateYMD(String(dataContext.LEASE_DUE_DT));
        }

        let sLeaseComment = sLeaseDt;
        if (!(sLeaseDt.trim() === "" && sLeaseDueDt === "")) {
            sLeaseComment = sLeaseDt + "～" + sLeaseDueDt;
        }

        return sLeaseComment;
    }

    // 資産状態用formatter
    function AssetStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch(String(dataContext.ASSET_STATUS)) {
            case "@ASSET_STATUS.Preparation":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_JUNBI)"
                break;
            case "@ASSET_STATUS.Storage":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_HOKAN)"
                break;
            case "@ASSET_STATUS.Lending":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_LEASE)"
                break;
            case "@ASSET_STATUS.Removed":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_RETIRE)"
                break;
            default :
                status = dataContext.ASSET_STATUS;
                break;
        }

        return status;
    }

    // 貸出状態用formatter
    function LeaseStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (String(dataContext.LEASE_STATUS) === "@AssetLeaseStatus.Ok") {
            status = "@Message.getMessage(CommonLabel.VALUE_KANOU)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_FUKA)"
        }

        return status;
    }

    // 資産ID状態用formatter
    function EpcEncStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (String(dataContext.EPC_ENC_STATUS) === "0") {
            status = "@Message.getMessage(CommonLabel.VALUE_MI)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_SUMI)"
        }

        return status;
    }

    // タグ紐付け状態用formatter
    function TagLinkStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (String(dataContext.TAG_LINK_STATUS) === "0") {
            status = "@Message.getMessage(CommonLabel.VALUE_MI)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_SUMI)"
        }

        return status;
    }

    //有形・無形区分用formatter
    function IsTangibleFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";
        switch (String(dataContext.IS_TANGIBLE)) {
            case "@TANGIBLE.TANGIBLE":
                status =  "@Message.getMessage(CommonLabel.LABEL_TANGIBLE_ASSET_NAME)"
                break;
            case "@TANGIBLE.NOT_TANGIBLE":
                status = "@Message.getMessage(CommonLabel.LABEL_INTANGIBLE_ASSET_NAME)"
                break;
        }
        return status;
    }

    // テキスト表示(右寄せ)
    function RightFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(value) != '') {
            return '<div style="text-align:right; width:100%;">' + value + '</div>';
        }
    }

    // テキスト表示(中央)
    function CenterFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(value) != '') {
            return '<div style="text-align:center; width:100%;">' + value + '</div>';
        }
    }

    // yyyyMMdd→yyyy/MM/dd表示用formatter
    function SlashYMDFormatter(row, cell, value, columnDef, dataContext) {
        return toStringDateYMD(value);
    }

    // yyyy/MM/dd HH:mm表示用formatter
    function SlashYMDHMFormatter(row, cell, value, columnDef, dataContext) {
        return toFormatDate(value, "yyyy/MM/dd HH:mm");
    }

    // 資産台帳取込画面　取込開始ボタンクリック処理
    function btnExcellmportStart_onclick() {

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

        //Excelデータを取得し取込する(Ajax非同期通信)
        var $form = $(this).closest('form')

        var execAssetLedgerImport = function (importKey) {
        var formData;
        var sImportUrl;
        if (importKey) {
            formData = new FormData();
            formData.append("ImportKey", importKey);
            formData.append("LocLCd", document.getElementById("SearchLocL").value);
            sImportUrl = '@Url.Content("~/AssetList")/ImportExcelFileConfirmed';
        } else {
            formData = new FormData($form.get(0));
            formData.append("ExcelFile", $('#ExcelFile')[0].files[0]);
            formData.append("LocLCd", document.getElementById("SearchLocL").value);
            sImportUrl = '@Url.Content("~/AssetList")/ImportExcelFile';
        }
        btnExcelImportEnabled(false)
        $("#csvimport-modal-close").attr("disabled", "disabled").css("background-color", "#cecece");

        $.ajax({
            type: 'POST',
url: sImportUrl,
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
                        break;
                    case @DataInport.DataInportResultCd.NoSelect:
                        // Excelファイル未選択
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SELECT)";
                        break;
                    case @DataInport.DataInportResultCd.Empty:
                        // Excelファイルが空
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_EMPTY)";
                        break;
                    case @DataInport.DataInportResultCd.NotExcel:
                        // Excelファイル以外
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_NOTEXCEL)";
                        break;
                    case @DataInport.DataInportResultCd.KoumokuSuuErr:
                        // 項目数エラー
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU)";
                        break;
                    case @DataInport.DataInportResultCd.ImportExcelErr:
                        // Excel取込エラー（その他の予期しないエラー）
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_IMPORT_ERR)";
                        break;
                    case @DataInport.DataInportResultCd.ImportSpSystemErr:
                        // Excel取込エラ（ストアドでその他予期しないエラー）
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_UNEXPECTED_ERR)";
                        break;
                    case @DataInport.DataInportResultCd.ImportSpParamErr:
                        // Excel取込エラ（ストアドでパラメータエラー）
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_PARAM_ERR)";
                        break;
                    case @DataInport.DataInportResultCd.ImportSpLocMaterErr:
                        // Excel取込エラ（ストアドでロケマスタなしエラー）
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_LOC_ERR)";
                        break;
                    case @DataInport.DataInportResultCd.ImportSpEmployeeErr:
                        // Excel取込エラ（ストアドで登録担当者なしエラー）
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_EMPLOYEE_ERR)";
                        break;
                    case @DataInport.DataInportResultCd.ImportNotUpdatableComErr:
                        // Excel取込エラー（更新可能拠点以外を含む）
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_EXIST_ASSET_COMPANY_NOT_UPDATABLE)";
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
                                $("#csv-err-ichiran").append('<tr><td class="col1" title=' + data[icnt].ErrRowIdx + '>'+ data[icnt].ErrRowIdx +'</td><td class="col2" title=' + data[icnt].ErrKoumoku + '>'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" title=' + data[icnt].ErrGaiyou + '>'+ data[icnt].ErrGaiyou +'</td></tr>');
                            }

                        }
                        break;
                    case @DataInport.DataInportResultCd.LoginSession:
                        // ログインセッション切れ
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_LOGININFO_EMPTY)";
                        break;
                    case @DataInport.DataInportResultCd.ImportDeleteConfirm:
                        var sImportKey = data[0].ImportKey;
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Nomal";
                        document.getElementById("modal-msg").innerHTML = "";
                        showConfirmOkEvt("@Message.getMessage(CommonLabel.MSG_CONF_ASSET_IMPORT_DELETE)".replace("{0}", data[0].DeleteCount))
                            .then(function () {
                                document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Ok";
                                document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORT)";
                                execAssetLedgerImport(sImportKey);
                            })
                            .catch(function () {
                                document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Nomal";
                                document.getElementById("modal-msg").innerHTML = "";
                                btnExcelImportEnabled(true)
                                $("#csvimport-modal-close").attr("disabled", false).css("background-color", "");
                                $.ajax({
                                    type: 'POST',
                                    url: '@Url.Content("~/AssetList")/ImportExcelFileCancel',
                                    data: { ImportKey: sImportKey },
                                    dataType: 'json',
                                    cache: false
                                });
                            });
                        return;
                }
                if (data[0].ResultCd != "@DataInport.DataInportResultCd.OK" &&
                    data[0].ResultCd != "@DataInport.DataInportResultCd.NoSelect") {
                    //正常終了以外の場合、資産台帳取込ボタンを非活性にする
                    btnExcelImportEnabled(false)
                } else if (data[0].ResultCd == "@DataInport.DataInportResultCd.NoSelect") {
                    btnExcelImportEnabled(true)
                } else {
                    btnExcelImportEnabled(false)
                }
                $("#csvimport-modal-close").attr("disabled", false).css("background-color", "");
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
                $("#csvimport-modal-close").attr("disabled", false).css("background-color", "");
                document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SOUTEIGAI)";
                btnExcelImportEnabled(false)
            }
        });
        };
        execAssetLedgerImport(null);
        return true;

    }

    //資産台帳取込モーダル 取込ボタン押下可否設定処理
    function btnExcelImportEnabled(enable) {
        if (enable) {
            //活性
            $("#modal-import-btn").attr("disabled", false)
            $("#modal-import-btn").css("background-color", "");
        } else {
            //非活性
            $("#modal-import-btn").attr("disabled", "disabled")
            $("#modal-import-btn").css("background-color","#cecece");
        }
    }

    //資産台帳取込ボタンクリック処理
    function btnExcelImport_onclick() {

        // ロケーション(会社)必須チェック
        var locL = document.getElementById("SearchLocL").value;
        if (locL.trim() === "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERRNO_SELECTED_LOC_L)")
            return false;
        }
        document.getElementById("popup-title-csv").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_LEDGER_DATA_IMPORT)";
        document.getElementById("modal-import-btn").value = "@Message.getMessage(CommonLabel.BUTTON_ASSET_LEDGER_IMPORT)";
        document.getElementById("ExcelFile").value = "";          //ファイル選択をクリア
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

        //ローカルストレージに検索条件保存
        localStorageSave();

        //資産台帳取込画面を表示
        ShowCsvModalWindow()
    }

    //CSV出力ボタンクリック処理
    function btnIchiranCsvExport_onclick() {
        //確認メッセージ
        @* --2019/12/26 miyaji update start *@
        return showConfirmFileDownload("@Message.getMessage(CommonLabel.MSG_ASSET_CSVOUTPUT)", "@Url.Content("~/AssetList")/IchiranCsvExport");
        @*return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/AssetList")/IchiranCsvExport");*@
        @* --2019/12/26 miyaji update end *@

    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
        //[CC431] ADD Start 20261015
        // 検索条件の開閉状態を hidden に退避し、検索時に検索前の状態を判定できるようにする
        document.getElementById("OpenFlg").value = $.trim(document.getElementById("SearchCondition").innerText);
        //[CC431] ADD End 20261015
    }


    //全データCSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        @* --2019/12/26 miyaji update start *@
        return showConfirmFileDownload("@Message.getMessage(CommonLabel.MSG_ASSET_CSVOUTPUT)", "@Url.Content("~/AssetList")/CsvExport");
        @*return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_ALL_CONFIRM)", "@Url.Content("~/AssetList")/CsvExport");*@
        @* --2019/12/26 miyaji update end *@

    }

    // QR台帳出力ボタンクリック処理
    function btnQRDaichoExport_onclick() {
        //var keys = [];
        //var grid = document.getElementById("asset-list-detail");
        //var checkBoxes = grid.getElementsByTagName("INPUT");
        //var catLs = [];
        //for (var i = 0; i < checkBoxes.length; i++) {
        //    if (checkBoxes[i].checked) {
        //        //「, (カンマ)」で区切って分割する
        //        var result = checkBoxes[i].value.split(',');
        //        keys.push(result[0]);
        //    }
        //}

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        //if (keys.length == 0) { //テーブルの選択行がない場合
        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        // １００件以上選択されていた場合、警告メッセージを出す　2019/12/20 nv miyaji
        if (document.getElementById("selectcount").innerHTML >= 100 ) {
            // １００件以上選択されていた場合
            //確認メッセージ
            return showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.EXC_QRDAICHO_CONFIRM)", "@Url.Content("~/AssetList")/QRDaicho");
        }else{
            //確認メッセージ
            return showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.QRDAICHO_CONFIRM)", "@Url.Content("~/AssetList")/QRDaicho");
        }

        // カンマ区切りの文字列を設定
        //document.getElementById("SelectKeys").value = keys.join(",");



    }

    //タグラベル発行
    function btnTagLabel_onclick() {
        document.getElementById("popup-title-prt").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SELECT_PRINTER)";

        //var keys = [];
        //var grid = document.getElementById("asset-list-detail");
        //var checkBoxes = grid.getElementsByTagName("INPUT");
        //var catLs = [];
        //for (var i = 0; i < checkBoxes.length; i++) {
        //    if (checkBoxes[i].checked) {
        //        //「, (カンマ)」で区切って分割する
        //        var result = checkBoxes[i].value.split(',');
        //        keys.push(result[0]);
        //        catLs.push(result[2]);
        //        if(result[1] == ""){
        //            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_ID_MITOUROKU)")
        //            return false;
        //        }
        //    }
        //}

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        //if (keys.length == 0) { //テーブルの選択行がない場合
        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        // ダイアログのメッセージを設定
        $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.HAKKO_CONFIRM)");

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $(this).dialog("close");

                    // カンマ区切りの文字列を設定
                    //document.getElementById("SelectKeys").value = keys.join(",");
                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/AssetList")/ShowPrinterList',
                        dataType: 'json',
                        //data: { Cals: catLs },
                        data: { SelectKeys: document.getElementById("SelectKeys").value, NoneSelectKeys: document.getElementById("NoneSelectKeys").value},
                        cache: false,
                        success: function (data, dataType) {
                            if (data == null) {
                                return;
                            } else {
                                // 照会のみのデータがあればエラーメッセージ '2021/10/05 nv miyaji add
                                if(data == "auth-error"){
                                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SELECTED_NOT_UPD_ASSET)")
                                    return false;
                                }
                                // 資産生成方法①の資産があればエラーメッセージ
                                if(data == "error"){
                                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_ID_HIMODUKE)")
                                    return false;
                                }

                                var PrinterKey = document.getElementById("PrinterKey");
                                PrinterKey.options.length = 0;
                                for (var i = 0; i < data.length; i++) {
                                    let op = document.createElement("option");
                                    op.value = data[i].KEY1;
                                    op.text = data[i].DATA1;
                                    document.getElementById("PrinterKey").appendChild(op);
                                }
                                if (data.length == 1) {
                                    document.getElementById("PrinterKey").value = data[0].KEY1;
                                    btnPrinterOK_onclick();
                                } else {
                                    // 前回その端末で選択したプリンタを選択する
                                    const savedKey = localStorage.getItem("AssetListPrintKey");
                                    if (savedKey) {
                                        document.getElementById("PrinterKey").value = savedKey;
                                    }
                                    //モーダルウインドウを開く
                                    ShowPRTModalWindow();
                                }
                            }
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            this;
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

    // プリンタ選択
    function btnPrinterOK_onclick() {
        var prtKey = document.getElementById("PrinterKey").value;
        @*var keys = [];
        var grid = document.getElementById("asset-list-detail");
        var checkBoxes = grid.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                keys.push(checkBoxes[i].value);
            }
        }
        if (keys.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }
        // カンマ区切りの文字列を設定
        document.getElementById("SelectKeys").value = keys.join(",");*@

        //画面ブロックを表示
        showLoading();

        // プリンタ選択状態保存
        localStorage.setItem("AssetListPrintKey", prtKey);
        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/AssetList")/CsvExportForLabel");
        $('#form').submit();
        $('#form').attr("action", befAction);
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        // ロケーション(会社)必須チェック
        var locL = document.getElementById("SearchLocL").value;
        if (locL.trim() === "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERRNO_SELECT_HUB_LOC_L)")
            return false;
        }

        var dateRangeChecks = [
            { from: "SearchFreeItem2From", to: "SearchFreeItem2To", label: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)" },
            { from: "SearchFreeItem18From", to: "SearchFreeItem18To", label: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)" },
            { from: "SearchFreeItem19From", to: "SearchFreeItem19To", label: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)" }
        ];
        for (var i = 0; i < dateRangeChecks.length; i++) {
            var chk = dateRangeChecks[i];
            var dateFrom = document.getElementById(chk.from).value;
            var dateTo = document.getElementById(chk.to).value;
            if (dateFrom != "" && dateTo != "" && dateFrom > dateTo) {
                //[CC431] ADD Start 20261019
                // 詳細検索欄の項目（開始日(貸与)・終了日(貸与)）のエラーのときは、詳細検索欄を閉じていれば開いてからメッセージを表示する（仕様変更6）
                if ($("#" + chk.from).closest(".detail-accordion").length > 0 && $("#item-conditions-detail").css("display") == "none") {
                    DetailAccordion_onclick();
                }
                //[CC431] ADD End 20261019
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_DATE_ORDER_ITEM)".format(chk.label), "#" + chk.from);
                return false;
            }
        }

        // 詳細項目の開閉状態を取得
        /*document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;*/
        //document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        //画面ブロックを表示
        showLoading();

        //ローカルストレージに検索条件保存
        localStorageSave();

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

        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_CONTENT)" + " " + "@Message.getMessage(CommonLabel.LABEL_ASSET_INSERT)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CREATE)";

        // 詳細項目の開閉状態を取得
        //document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        //除却日、貸出回数、資産状態、資産ＩＤ状態、タグ紐付け状態、削除ボタン・除却を非表示
        $("#modal-delete-btn").css("display", "none");
        $("#modal-retire-btn").css("display", "none");
        $(".DisplayOnOff").css("display", "none");

        // 2023/03/22 Kawamoto ADD START
        // 添付ファイルボタンを使用不可
        $("#btnAttachment").prop("disabled", true);
        $("#btnAttachment").attr('style', 'background-color:#cecece;');
        $("#btnAttachment").css("cursor", "default");
        // 2023/03/22 Kawamoto ADD END

        // 貸出状態変更不可  // 2021/10/05 nv miyaji add
        $("#LEASE_STATUS").attr("disabled", "disabled");

        // 生成要求前に資産番号を初期化
        document.getElementById("ASSET_NO").value = "";

        // 資産番号の生成要求を非同期で実施
        let promise = $.ajax({
            url: '@Url.Content("~/AssetList")/RequestAssetNo',
            type: 'POST',
        });

        promise.always(function(assetNo) {
            // 自動生成上限超過ならエラー
            if (assetNo.length > '@itemlength.AssetNoAuto') {
                document.getElementById("ASSET_NO").value = "";
                showErrMessageOkEvt("@Message.getMessage(CommonLabel.MSG_ERR_AUTO_GENERATE_MAX_SIZE)").then(function () {
                    $('#form').submit();
                });
                return;
            }

            // 受け取った資産番号は一旦使用しないので必須チェックにならないようダミーの文字列をセット
            if(assetNo != ""){
                document.getElementById("ASSET_NO").value = "_";

                // 資産番号を灰色のフォント表示
                document.getElementById("ASSET_NO").style.color = "#cecece";
            }
        });


        //画面を初期化する
        isSettingValues = true;     //値セット中
        @*if (document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_UPDATE_CLICK") {*@
        //$('[id=LOC_DIV_L]').prop('checked', true);
        //$('[id=LOC_DIV_M]').prop('checked', false);
        //$('[id=LOC_DIV_S]').prop('checked', false);
        document.getElementById("SUB_ASSET_NO").value = "";
        document.getElementById("ASSET_ID").value = "";
        $("#LOC_L_CD").val("").trigger("change");
        $("#LOC_M_CD").val("").trigger("change");
        $("#LOC_M_CD").attr("disabled", "disabled");
        $("#LOC_S_CD").val("").trigger("change");
        $("#LOC_S_CD").attr("disabled", "disabled");
        document.getElementById("ASSET_DIV").value = "";
        document.getElementById("ASSET_NAME").value = "";
        $("#MNG_LOC_L_CD").val("").trigger("change");
        $("#CAT_L_CD").val("").trigger("change");
        $("#CAT_M_CD").val("").trigger("change");
        $("#CAT_M_CD").attr("disabled", "disabled");
        $("#CAT_S_CD").val("").trigger("change");
        $("#CAT_S_CD").attr("disabled", "disabled");
        document.getElementById("MAKER").value = "";
        document.getElementById("PRODUCT_NO").value = "";
        document.getElementById("MODEL_NO").value = "";
        document.getElementById("LEASE_NO").value = "";
        document.getElementById("LEASE_LIMIT").value = "";
        document.getElementById("GENERIC_LIMIT").value = "";
        document.getElementById("LeaseLimitString").value = "";
        document.getElementById("GenLimitString").value = "";
        document.getElementById("ASSET_PIC_FILENAME").value = "";

        document.getElementById("FREE_ITEM1").value = "";
        document.getElementById("FREE_ITEM2").value = "";
        document.getElementById("FREE_ITEM3").value = "";
        document.getElementById("FREE_ITEM4").value = "";
        document.getElementById("FREE_ITEM5").value = "";
        document.getElementById("FREE_ITEM6").value = "";
        document.getElementById("FREE_ITEM7").value = "";
        document.getElementById("FREE_ITEM8").value = "";
        document.getElementById("FREE_ITEM9").value = "";
        document.getElementById("FREE_ITEM10").value = "";
        document.getElementById("FREE_ITEM11").value = "";
        document.getElementById("FREE_ITEM12").value = "";
        document.getElementById("FREE_ITEM13").value = "";
        document.getElementById("FREE_ITEM14").value = "";
        document.getElementById("FREE_ITEM15").value = "";
        document.getElementById("FREE_ITEM16").value = "";
        document.getElementById("FREE_ITEM17").value = "";
        document.getElementById("FREE_ITEM18").value = "";
        document.getElementById("FREE_ITEM19").value = "";
        document.getElementById("FREE_ITEM20").value = "";
        document.getElementById("FREE_ITEM21").value = "";
        document.getElementById("FREE_ITEM22").value = "";
        document.getElementById("FREE_ITEM23").value = "";
        document.getElementById("FREE_ITEM24").value = "";
        document.getElementById("FREE_ITEM25").value = "";
        document.getElementById("FREE_ITEM26").value = "";
        document.getElementById("FREE_ITEM27").value = "";
        document.getElementById("FREE_ITEM28").value = "";
        document.getElementById("FREE_ITEM29").value = "";
        document.getElementById("FREE_ITEM30").value = "";
        document.getElementById("FREE_ITEM31").value = "";
        document.getElementById("FREE_ITEM32").value = "";
        document.getElementById("FREE_ITEM33").value = "";
        document.getElementById("FREE_ITEM34").value = "";
        document.getElementById("FREE_ITEM35").value = "";
        document.getElementById("FREE_ITEM36").value = "";
        document.getElementById("FREE_ITEM37").value = "";
        document.getElementById("FREE_ITEM38").value = "";
        document.getElementById("FREE_ITEM39").value = "";
        document.getElementById("FREE_ITEM40").value = "";
        document.getElementById("FREE_ITEM41").value = "";
        document.getElementById("FREE_ITEM42").value = "";
        document.getElementById("FREE_ITEM43").value = "";
        document.getElementById("FREE_ITEM44").value = "";
        document.getElementById("FREE_ITEM45").value = "";
        document.getElementById("FREE_ITEM46").value = "";
        document.getElementById("FREE_ITEM47").value = "";
        document.getElementById("FREE_ITEM48").value = "";
        document.getElementById("FREE_ITEM49").value = "";
        document.getElementById("FREE_ITEM50").value = "";
        document.getElementById("FREE_ITEM51").value = "";
        document.getElementById("FREE_ITEM52").value = "";
        document.getElementById("FREE_ITEM53").value = "";
        document.getElementById("FREE_ITEM54").value = "";
        document.getElementById("FREE_ITEM55").value = "";
        document.getElementById("FREE_ITEM56").value = "";
        document.getElementById("FREE_ITEM57").value = "";
        document.getElementById("FREE_ITEM58").value = "";
        document.getElementById("FREE_ITEM59").value = "";
        document.getElementById("FREE_ITEM60").value = "";

        for (var nRemark = 1; nRemark <= 10; nRemark++) {
            document.getElementById("REMARK" + nRemark).value = "";
        }

        document.getElementById("ASSET_PIC_FILENAME").value = "";
        document.getElementById("LEASE_CNT").value = 0;
        document.getElementById("ASSET_STATUS").value = 0;
        document.getElementById("EPC_ENC_STATUS").value = 0;
        document.getElementById("TAG_LINK_STATUS").value = 0;
        document.getElementById("LEASE_STATUS").value = @AssetLeaseStatus.Ng; // 2021/09/17 nv miyaji add
        document.getElementById("REMARK").value = "";

        document.getElementById("LEASE_PLACE_CD").value = "";
        document.getElementById("LEASE_PLACE_NAME").value = "";
        document.getElementById("LEASE_EMPLOYEE_CD").value ="";
        document.getElementById("LEASE_EMPLOYEE_NAME").value = "";
        document.getElementById("LEASE_DT").value = "";
        document.getElementById("LEASE_DUE_DT").value = "";
        document.getElementById("MAINTE_LEASE_NUM").value = ""; // 2021/09/17 nv miyaji add
        document.getElementById("MAINTE_LEASE_DAY").value = ""; // 2021/09/17 nv miyaji add
        $("#DispLeaseDayWorkFlg").prop("checked", false); // 2021/09/17 nv miyaji add
        document.getElementById("MAINTE_DAY").value = ""; // 2021/09/17 nv miyaji add
        $("#DispMainteDayWorkFlg").prop("checked", false); // 2021/09/17 nv miyaji add
        document.getElementById("RETIRE_EMPLOYEE_CD").value = "";
        document.getElementById("RETIRE_EMPLOYEE_NAME").value = "";
        document.getElementById("RETIRE_ASSET_ID").value = "";
        document.getElementById("PHYINV_TS").value = "";
        document.getElementById("PHYINV_STATUS").value = "";
        document.getElementById("PHYINV_LOC_L_CD").value = "";
        document.getElementById("PHYINV_LOC_M_CD").value = "";
        document.getElementById("PHYINV_LOC_S_CD").value = "";
        document.getElementById("PHYINV_LOC_L_NAME").value = "";
        document.getElementById("PHYINV_LOC_M_NAME").value ="";
        document.getElementById("PHYINV_LOC_S_NAME").value = "";
        document.getElementById("PHYINV_EMPLOYEE_CD").value ="";
        document.getElementById("PHYINV_EMPLOYEE_NAME").value = "";
        document.getElementById("TASK_TS").value = "";

        $('[id=IS_TANGIBLE_TANGIBLE]').prop('checked', true);
        $('[id=IS_TANGIBLE_NOT_TANGIBLE]').prop('checked', false);
        $('[id=IS_TANGIBLE_TANGIBLE]').prop("disabled", true);
        $('[id=IS_TANGIBLE_NOT_TANGIBLE]').prop("disabled", true);
        document.getElementById("IS_TANGIBLE").value = "@TANGIBLE.TANGIBLE";
        isSettingValues = false;     //値セット中解除

        // 非表示項目
        document.getElementById("RemainingAmount").value = "";  // 2023/05/10 Kawamoto ADD

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        //$("#LOC_L_CD").removeClass("input-error");
        //$("#LOC_M_CD").removeClass("input-error");
        //$("#LOC_S_CD").removeClass("input-error");
        //}

        //登録ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_CREATE_CLICK";

        // --2020/01/07 miyaji update start
        //チェックボックスの状態からキーを設定する
        setting_checklist();
        // --2020/01/07 miyaji update end

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //変更ボタンクリック処理
    function btnEdit_onclick(element,isEditMode,assetNo) {

        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_CONTENT)" + " " + "@Message.getMessage(CommonLabel.LABEL_ASSET_UPDATE)";
        document.getElementById("modal-create-btn").value = "@Message.getMessage(CommonLabel.BUTTON_CHANGE)";

        // 詳細項目の開閉状態を取得
        //document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        //資産番号コード、除却日、貸出回数、資産状態、資産ＩＤ状態、タグ紐付け状態、カテゴリー大区分書き込み不可
        document.getElementById("ASSET_NO").readOnly = true;
        $("#ASSET_NO").addClass("disable");
        document.getElementById("AssetStatusNm").readOnly = true;
        $("#AssetStatusNm").addClass("disable");
        document.getElementById("EpcEncStatusNm").readOnly = true;
        $("#EpcEncStatusNm").addClass("disable");
        document.getElementById("TagLinkStatusNm").readOnly = true;
        $("#TagLinkStatusNm").addClass("disable");
        document.getElementById("RetireTsString").readOnly = true;
        $("#RetireTsString").addClass("disable");
        document.getElementById("LEASE_CNT").readOnly = true;
        $("#LEASE_CNT").addClass("disable");
        $("#CAT_L_CD").attr("disabled", "disabled");

        //除却日、貸出回数、資産状態、資産ＩＤ状態、タグ紐付け状態、削除ボタン・除却を表示
        @* '--2021/09/27 nv miyaji update start *@
        // 権限による処理区分
        if("@ViewBag.AuthProcFlg" == "@AuthProcFlg.All"|| "@ViewBag.AuthProcFlg" == "@AuthProcFlg.JiAll" && isEditMode == true){
            // 権限による処理区分が「編集・照会可能」の場合
            //削除・除却ボタンを表示
            $("#modal-delete-btn").css("display", "block");
            $("#modal-retire-btn").css("display", "block");
        }else{
            // 権限による処理区分が「照会のみ」の場合
            // 変更ボタンを非表示
            $("#modal-create-btn").css("display", "none");
            // 削除ボタンを非表示
            $("#modal-delete-btn").css("display", "none");
            // 除却ボタンを非表示
            $("#modal-retire-btn").css("display", "none");
            // 画像削除ボタンを非表示
            $("#select_file_clear_button").css("display", "none");
            // 2023/03/22 Kawamoto ADD START
            // 添付ファイルボタンを使用不可
            $("#btnAttachment").prop("disabled", true);
            $("#btnAttachment").attr('style', 'background-color:#cecece;');
            $("#btnAttachment").css("cursor", "default");
            // 2023/03/22 Kawamoto ADD END
        };
        //$("#modal-delete-btn").css("display", "block");
        //$("#modal-retire-btn").css("display", "block");
        @* '--2021/09/27 nv miyaji update end *@
        $(".DisplayOnOff").css("display", "inline-block");

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        // 資産番号を黒色のフォント表示（資産番号自動生成で別色に変更したことの復帰処理)
        document.getElementById("ASSET_NO").style.color = "black";

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/AssetList")/GetEditData',
                cache: false,
                data: { ID: assetNo },
                success: function (data, dataType) {
                    this;
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }
                    // 除却/除却解除ボタン
                    if(data.RETIRE_TS){
                        document.getElementById("modal-retire-btn").value = "@Message.getMessage(CommonLabel.BUTTON_RETIRE_OFF)"
                    }else{
                        document.getElementById("modal-retire-btn").value = "@Message.getMessage(CommonLabel.BUTTON_RETIRE)"
                    }

                    // 2023/03/22 Kawamoto ADD
                    // 添付ファイルボタン名の設定
                    document.getElementById("btnAttachment").value = "@Message.getMessage(CommonLabel.BUTTON_ATTACHMENT)".replace("[0]", "[" + data.ATTACHMENT_NUM + "]");

                    // 資産状態が保管中でない場合、貸出状態変更不可  // 2021/10/05 nv miyaji add
                    if(data.ASSET_STATUS != "@ASSET_STATUS.Storage"){
                        $("#LEASE_STATUS").attr("disabled", "disabled");
                    }

                    //取得したデータを表示する
                    isSettingValues = true;     //値セット中
                    document.getElementById("ASSET_NO").value = data.ASSET_NO;
                    document.getElementById("SUB_ASSET_NO").value = data.SUB_ASSET_NO;
                    document.getElementById("ASSET_ID").value = data.ASSET_ID;
                    document.getElementById("GEN_ASSET_ID").value = data.GEN_ASSET_ID;  // 隠し項目
                    document.getElementById("EPC_SIZE").value = data.EPC_SIZE;  // 隠し項目
                    // --2021/10/01 nv miyaji add start
                    // 照会モードで開く時、管理ロケーションドロップダウンリストの項目を再設定する。
                    var LocMngLocL = ""
                    LocMngLocL =document.getElementById("MNG_LOC_L_CD");
                    if (!isEditMode && data.MnglocLDropdownData != null){
                        LocMngLocL.options.length = 0;
                        let opBlank = document.createElement("option");
                        opBlank.value = "";
                        opBlank.text = "";
                        LocMngLocL.appendChild(opBlank);
                        for (var i = 0; i < data.MnglocLDropdownData.length; i++) {
                            let op = document.createElement("option");
                            op.value = data.MnglocLDropdownData[i].LOC_CD;
                            op.text = data.MnglocLDropdownData[i].LOC_NAME;
                            LocMngLocL.appendChild(op);
                        }
                    }                   
                    $(LocMngLocL).val(data.MNG_LOC_L_CD).trigger("change");
                    // --2021/10/01 nv miyaji add end
                    $("#LOC_L_CD").val(data.LOC_L_CD).trigger("change");                    
                    // 中区分データがあれば表示する
                    var LocM = ""
                    LocM =document.getElementById("LOC_M_CD");
                    if (data.LOC_L_CD && data.LOC_L_CD.trim() != ""){
                        LocM.options.length = 0;
                        let opBlank = document.createElement("option");
                        opBlank.value = "";
                        opBlank.text = "";
                        LocM.appendChild(opBlank);
                        for (var i = 0; i < data.locMDropdownData.length; i++) {
                            let op = document.createElement("option");
                            op.value = data.locMDropdownData[i].LOC_CD;
                            op.text = data.locMDropdownData[i].LOC_NAME;
                            LocM.appendChild(op);
                        }
                        $("#LOC_M_CD").removeAttr("disabled");
                        $(LocM).val(data.LOC_M_CD).trigger("change");
                    }else{
                        $(LocM).val("").trigger("change");
                        $("#LOC_M_CD").attr("disabled", "disabled");
                    }
                    // 小区分データがあれば表示する
                    var LocS = ""
                    LocS =document.getElementById("LOC_S_CD");
                    if (data.LOC_M_CD && data.LOC_M_CD.trim() != ""){
                        LocS.options.length = 0;
                        let opBlank = document.createElement("option");
                        opBlank.value = "";
                        opBlank.text = "";
                        LocS.appendChild(opBlank);
                        for (var i = 0; i < data.locSDropdownData.length; i++) {
                            let op = document.createElement("option");
                            op.value = data.locSDropdownData[i].LOC_CD;
                            op.text = data.locSDropdownData[i].LOC_NAME;
                            LocS.appendChild(op);
                        }
                        $("#LOC_S_CD").removeAttr("disabled");
                        $(LocS).val(data.LOC_S_CD).trigger("change");                        
                    } else {
                        $(LocS).val("").trigger("change");
                        $("#LOC_S_CD").attr("disabled", "disabled");
                    }
                    // --2020/03/17 nv miyaji add start
                    if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes"){
                        $("#LOC_M_CD").attr("disabled", "disabled");
                    }
                    if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes"){
                        $("#LOC_S_CD").attr("disabled", "disabled");
                    }
                    // --2020/03/17 nv miyaji add end
                    document.getElementById("ASSET_DIV").value = data.ASSET_DIV;
                    document.getElementById("ASSET_NAME").value = data.ASSET_NAME;
                    $("#MNG_LOC_L_CD").val(data.MNG_LOC_L_CD).trigger("change");
                    $("#CAT_L_CD").val(data.CAT_L_CD).trigger("change");                    
                    // 中分類データがあれば表示する
                    var CatM = ""
                    CatM =document.getElementById("CAT_M_CD");
                    if (data.CAT_L_CD && data.CAT_L_CD.trim() != ""){
                        CatM.options.length = 0;
                        let opBlank = document.createElement("option");
                        opBlank.value = "";
                        opBlank.text = "";
                        CatM.appendChild(opBlank);
                        for (var i = 0; i < data.catMDropdownData.length; i++){
                            let op = document.createElement("option");
                            op.value = data.catMDropdownData[i].CAT_CD;
                            op.text = data.catMDropdownData[i].CAT_NAME;
                            CatM.appendChild(op);
                        }
                        $("#CAT_M_CD").removeAttr("disabled");                        
                        $(CatM).val(data.CAT_M_CD).trigger("change");
                    } else {
                        $(CatM).val("").trigger("change");                       
                        $("#CAT_M_CD").attr("disabled", "disabled");
                    }

                    // 小分類データがあれば表示する
                    var CatS = ""
                    CatS =document.getElementById("CAT_S_CD");
                    if (data.CAT_M_CD && data.CAT_M_CD.trim() != ""){
                        CatS.options.length = 0;
                        let opBlank = document.createElement("option");
                        opBlank.value = "";
                        opBlank.text = "";
                        CatS.appendChild(opBlank);
                        for (var i = 0; i < data.catSDropdownData.length; i++){
                            let op = document.createElement("option");
                            op.value = data.catSDropdownData[i].CAT_CD;
                            op.text = data.catSDropdownData[i].CAT_NAME;
                            CatS.appendChild(op);
                        }
                        $("#CAT_S_CD").removeAttr("disabled");                        
                        $(CatS).val(data.CAT_S_CD).trigger("change");
                    }else{                        
                        $(CatS).val("").trigger("change");
                        $("#CAT_S_CD").attr("disabled", "disabled");
                    }
                    document.getElementById("MAKER").value = data.MAKER;
                    document.getElementById("PRODUCT_NO").value = data.PRODUCT_NO;
                    document.getElementById("MODEL_NO").value = data.MODEL_NO;
                    document.getElementById("LEASE_NO").value = data.LEASE_NO;
                    if(data.LEASE_LIMIT){
                        //document.getElementById("LeaseLimitString").value = formatDate(new Date(data.LeaseLimitString), "yyyy-MM-dd");
                        document.getElementById("LeaseLimitString").value = data.LeaseLimitString;
                        document.getElementById("LEASE_LIMIT").value = String(data.LEASE_LIMIT);
                    }else{
                        document.getElementById("LeaseLimitString").value = "";
                    }
                    if(data.GENERIC_LIMIT){
                        //document.getElementById("GenLimitString").value =formatDate(new Date(data.GenLimitString), "yyyy-MM-dd");
                        document.getElementById("GenLimitString").value =data.GenLimitString;
                        document.getElementById("GENERIC_LIMIT").value = String(data.GENERIC_LIMIT);
                    }else{
                        document.getElementById("GenLimitString").value = "";
                    }

                    document.getElementById("FREE_ITEM1").value = data.FREE_ITEM1;
                    document.getElementById("FREE_ITEM2").value = data.FREE_ITEM2;
                    document.getElementById("FREE_ITEM3").value = data.FREE_ITEM3;
                    document.getElementById("FREE_ITEM4").value = data.FREE_ITEM4;
                    document.getElementById("FREE_ITEM5").value = data.FREE_ITEM5;
                    document.getElementById("FREE_ITEM6").value = data.FREE_ITEM6;
                    document.getElementById("FREE_ITEM7").value = data.FREE_ITEM7;
                    document.getElementById("FREE_ITEM8").value = data.FREE_ITEM8;
                    document.getElementById("FREE_ITEM9").value = data.FREE_ITEM9;
                    document.getElementById("FREE_ITEM10").value = data.FREE_ITEM10;
                    document.getElementById("FREE_ITEM11").value = data.FREE_ITEM11;
                    document.getElementById("FREE_ITEM12").value = data.FREE_ITEM12;
                    document.getElementById("FREE_ITEM13").value = data.FREE_ITEM13;
                    document.getElementById("FREE_ITEM14").value = data.FREE_ITEM14;
                    document.getElementById("FREE_ITEM15").value = data.FREE_ITEM15;
                    document.getElementById("FREE_ITEM16").value = data.FREE_ITEM16;
                    document.getElementById("FREE_ITEM17").value = data.FREE_ITEM17;
                    document.getElementById("FREE_ITEM18").value = data.FREE_ITEM18;
                    document.getElementById("FREE_ITEM19").value = data.FREE_ITEM19;
                    document.getElementById("FREE_ITEM20").value = data.FREE_ITEM20;
                    document.getElementById("FREE_ITEM21").value = data.FREE_ITEM21;
                    document.getElementById("FREE_ITEM22").value = data.FREE_ITEM22;
                    document.getElementById("FREE_ITEM23").value = data.FREE_ITEM23;
                    document.getElementById("FREE_ITEM24").value = data.FREE_ITEM24;
                    document.getElementById("FREE_ITEM25").value = data.FREE_ITEM25;
                    document.getElementById("FREE_ITEM26").value = data.FREE_ITEM26;
                    document.getElementById("FREE_ITEM27").value = data.FREE_ITEM27;
                    document.getElementById("FREE_ITEM28").value = data.FREE_ITEM28;
                    document.getElementById("FREE_ITEM29").value = data.FREE_ITEM29;
                    document.getElementById("FREE_ITEM30").value = data.FREE_ITEM30;
                    document.getElementById("FREE_ITEM31").value = data.FREE_ITEM31;
                    document.getElementById("FREE_ITEM32").value = data.FREE_ITEM32;
                    document.getElementById("FREE_ITEM33").value = data.FREE_ITEM33;
                    document.getElementById("FREE_ITEM34").value = data.FREE_ITEM34;
                    document.getElementById("FREE_ITEM35").value = data.FREE_ITEM35;
                    document.getElementById("FREE_ITEM36").value = data.FREE_ITEM36;
                    document.getElementById("FREE_ITEM37").value = data.FREE_ITEM37;
                    document.getElementById("FREE_ITEM38").value = data.FREE_ITEM38;
                    document.getElementById("FREE_ITEM39").value = data.FREE_ITEM39;
                    document.getElementById("FREE_ITEM40").value = data.FREE_ITEM40;
                    document.getElementById("FREE_ITEM41").value = data.FREE_ITEM41;
                    document.getElementById("FREE_ITEM42").value = data.FREE_ITEM42;
                    document.getElementById("FREE_ITEM43").value = data.FREE_ITEM43;
                    document.getElementById("FREE_ITEM44").value = data.FREE_ITEM44;
                    document.getElementById("FREE_ITEM45").value = data.FREE_ITEM45;
                    document.getElementById("FREE_ITEM46").value = data.FREE_ITEM46;
                    document.getElementById("FREE_ITEM47").value = data.FREE_ITEM47;
                    document.getElementById("FREE_ITEM48").value = data.FREE_ITEM48;
                    document.getElementById("FREE_ITEM49").value = data.FREE_ITEM49;
                    document.getElementById("FREE_ITEM50").value = data.FREE_ITEM50;
                    document.getElementById("FREE_ITEM51").value = data.FREE_ITEM51;
                    document.getElementById("FREE_ITEM52").value = data.FREE_ITEM52;
                    document.getElementById("FREE_ITEM53").value = data.FREE_ITEM53;
                    document.getElementById("FREE_ITEM54").value = data.FREE_ITEM54;
                    document.getElementById("FREE_ITEM55").value = data.FREE_ITEM55;
                    document.getElementById("FREE_ITEM56").value = data.FREE_ITEM56;
                    document.getElementById("FREE_ITEM57").value = data.FREE_ITEM57;
                    document.getElementById("FREE_ITEM58").value = data.FREE_ITEM58;
                    document.getElementById("FREE_ITEM59").value = data.FREE_ITEM59;
                    document.getElementById("FREE_ITEM60").value = data.FREE_ITEM60;

                    for (var nRemark = 1; nRemark <= 10; nRemark++) {
                        document.getElementById("REMARK" + nRemark).value = data["REMARK" + nRemark];
                    }

                    //有形・無形区分ラジオボタンのチェック状態設定
                    if (data.IS_TANGIBLE == "@TANGIBLE.TANGIBLE") {
                        $('[id=IS_TANGIBLE_TANGIBLE]').prop('checked', true);
                    } else {
                        $('[id=IS_TANGIBLE_NOT_TANGIBLE]').prop('checked', true);
                    }
                    $('[id=IS_TANGIBLE_TANGIBLE]').prop("disabled", true);
                    $('[id=IS_TANGIBLE_NOT_TANGIBLE]').prop("disabled", true);
                    document.getElementById("IS_TANGIBLE").value = data.IS_TANGIBLE;

                    document.getElementById("ASSET_PIC_FILENAME").value = data.ASSET_PIC_FILENAME;
                    if( data.ASSET_PIC_FILENAME != "" && data.ASSET_PIC_FILENAME != null ){
                        var imageUrl = "@Message.getSettingInfo(CommonSettingKey.Image.KeyDiv, CommonSettingKey.Image.Key1, CommonSettingKey.Image.Key2, CommonSettingKey.Image.Key3).DATA1" + "/" + data.ASSET_PIC_FILENAME;
                        document.getElementById("preview").src = '@Url.Content("~/")' + imageUrl;
                        document.getElementById("NowImageUrl").value ='@Url.Content("~/")' + imageUrl;

                    }else{
                        var imageUrl = "Images/noimage.png";
                        document.getElementById("preview").src = '@Url.Content("~/")' + imageUrl;
                        document.getElementById("NowImageUrl").value = '@Url.Content("~/")' + imageUrl;
                    }
                    document.getElementById("ASSET_STATUS").value = data.ASSET_STATUS;
                    if(data.ASSET_STATUS == "@ASSET_STATUS.Preparation"){
                        document.getElementById("AssetStatusNm").value = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_JUNBI)";
                    }else if(data.ASSET_STATUS == "@ASSET_STATUS.Storage"){
                        document.getElementById("AssetStatusNm").value = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_HOKAN)";
                    }else if(data.ASSET_STATUS == "@ASSET_STATUS.Lending"){
                        document.getElementById("AssetStatusNm").value = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_LEASE)";
                    }else if(data.ASSET_STATUS == "@ASSET_STATUS.Removed"){
                        document.getElementById("AssetStatusNm").value = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_RETIRE)";
                    }
                    document.getElementById("EPC_ENC_STATUS").value = data.EPC_ENC_STATUS;
                    if(data.EPC_ENC_STATUS == 0){
                        document.getElementById("EpcEncStatusNm").value = "@Message.getMessage(CommonLabel.VALUE_SETTING_MI)";
                    }else{
                        document.getElementById("EpcEncStatusNm").value = "@Message.getMessage(CommonLabel.VALUE_SETTING_SUMI)";
                    }
                    document.getElementById("TAG_LINK_STATUS").value = data.TAG_LINK_STATUS;
                    if(data.TAG_LINK_STATUS == 0){
                        document.getElementById("TagLinkStatusNm").value = "@Message.getMessage(CommonLabel.VALUE_MI)";
                    }else{
                        document.getElementById("TagLinkStatusNm").value = "@Message.getMessage(CommonLabel.VALUE_SUMI)";
                    }
                    document.getElementById("LEASE_STATUS").value = data.LEASE_STATUS; // 2021/09/17 nv miyaji add
                    document.getElementById("LEASE_CNT").value = data.LEASE_CNT;


                    document.getElementById("LEASE_PLACE_CD").value = data.LEASE_PLACE_CD;
                    document.getElementById("LEASE_PLACE_NAME").value = data.LEASE_PLACE_NAME;
                    document.getElementById("LEASE_EMPLOYEE_CD").value = data.LEASE_EMPLOYEE_CD;
                    document.getElementById("LEASE_EMPLOYEE_NAME").value = data.LEASE_EMPLOYEE_NAME;
                    document.getElementById("LEASE_DT").value = data.LEASE_DT;
                    document.getElementById("LEASE_DUE_DT").value = data.LEASE_DUE_DT;
                    document.getElementById("MAINTE_LEASE_NUM").value = data.MAINTE_LEASE_NUM; // 2021/09/17 nv miyaji add
                    document.getElementById("MAINTE_LEASE_DAY").value = data.MAINTE_LEASE_DAY; // 2021/09/17 nv miyaji add
                    if (data.MAINTE_LEASE_DAY_WORK_FLAG == "@MainteWorkFlg.Yes") {   // 2021/09/17 nv miyaji add
                        $("#DispLeaseDayWorkFlg").prop("checked", true);
                    }else{
                        $("#DispLeaseDayWorkFlg").prop("checked", false);
                    }
                    document.getElementById("MAINTE_DAY").value = data.MAINTE_DAY; // 2021/09/17 nv miyaji add
                    if (data.MAINTE_DAY_WORK_FLAG == "@MainteWorkFlg.Yes") {   // 2021/09/17 nv miyaji add
                        $("#DispMainteDayWorkFlg").prop("checked", true);
                    }else{
                        $("#DispMainteDayWorkFlg").prop("checked", false);
                    }
                    document.getElementById("RETIRE_EMPLOYEE_CD").value = data.RETIRE_EMPLOYEE_CD;
                    document.getElementById("RETIRE_EMPLOYEE_NAME").value = data.RETIRE_EMPLOYEE_NAME;
                    document.getElementById("RETIRE_ASSET_ID").value = data.RETIRE_ASSET_ID;
                    document.getElementById("PHYINV_TS").value = data.PHYINV_TS;
                    document.getElementById("PHYINV_STATUS").value = data.PHYINV_STATUS;
                    document.getElementById("PHYINV_LOC_L_CD").value = data.PHYINV_LOC_L_CD;
                    document.getElementById("PHYINV_LOC_M_CD").value = data.PHYINV_LOC_M_CD;
                    document.getElementById("PHYINV_LOC_S_CD").value = data.PHYINV_LOC_S_CD;
                    document.getElementById("PHYINV_LOC_L_NAME").value = data.PHYINV_LOC_L_NAME;
                    document.getElementById("PHYINV_LOC_M_NAME").value = data.PHYINV_LOC_M_NAME;
                    document.getElementById("PHYINV_LOC_S_NAME").value = data.PHYINV_LOC_S_NAME;
                    document.getElementById("PHYINV_EMPLOYEE_CD").value = data.PHYINV_EMPLOYEE_CD;
                    document.getElementById("PHYINV_EMPLOYEE_NAME").value = data.PHYINV_EMPLOYEE_NAME;
                    document.getElementById("TASK_TS").value = data.TASK_TS;


                    document.getElementById("RetireTsString").value = data.RetireTsString;
                    document.getElementById("RETIRE_TS").value = data.RETIRE_TS;
                    document.getElementById("REMARK").value = data.REMARK;
                    document.getElementById("INS_DATETIME").value = formatDate(new Date(data.InsDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("INS_EMPLOYEE_CD").value = data.INS_EMPLOYEE_CD;
                    document.getElementById("INS_EMPLOYEE_NAME").value = data.INS_EMPLOYEE_NAME;
                    document.getElementById("UPD_DATETIME").value = formatDate(new Date(data.UpdDatetimeString), "yyyy/MM/dd HH:mm:ss");
                    document.getElementById("UPD_EMPLOYEE_CD").value = data.UPD_EMPLOYEE_CD;
                    document.getElementById("UPD_EMPLOYEE_NAME").value = data.UPD_EMPLOYEE_NAME;
                    document.getElementById("UPD_TASK").value = data.RenewProcess;

                    //隠し項目　資産ID登録方法
                    if (data.AssetIdDivReg == false){
                        $("#ASSET_ID").addClass("disable");
                        document.getElementById("ASSET_ID").readOnly = true;
                    }
                    document.getElementById("AssetIdDivReg").value = data.AssetIdDivReg;

                    // 除却済の資産の場合、変更ボタンを押せなくする
                    if(data.RETIRE_TS){
                        $("#modal-create-btn").css("display", "none");

                        // 2023/03/22 Kawamoto ADD START
                        // 添付ファイルボタンを使用不可
                        $("#btnAttachment").prop("disabled", true);
                        $("#btnAttachment").attr('style', 'background-color:#cecece;');
                        $("#btnAttachment").css("cursor", "default");
                        // 2023/03/22 Kawamoto ADD END

                        //--2019/10/11 nv miyaji add start 除却時、全ての項目を変更できなくする
                        document.getElementById("ASSET_ID").readOnly = true;
                        $("#ASSET_ID").addClass("disable");
                        $("#LOC_L_CD").attr("disabled", "disabled");
                        $("#LOC_M_CD").attr("disabled", "disabled");
                        $("#LOC_S_CD").attr("disabled", "disabled");
                        document.getElementById("ASSET_DIV").readOnly = true;
                        $("#ASSET_DIV").addClass("disable");
                        document.getElementById("ASSET_NAME").readOnly = true;
                        $("#ASSET_NAME").addClass("disable");
                        $("#CAT_M_CD").attr("disabled", "disabled");
                        $("#CAT_S_CD").attr("disabled", "disabled");
                        document.getElementById("MAKER").readOnly = true;
                        $("#MAKER").addClass("disable");
                        document.getElementById("PRODUCT_NO").readOnly = true;
                        $("#PRODUCT_NO").addClass("disable");
                        document.getElementById("MODEL_NO").readOnly = true;
                        $("#MODEL_NO").addClass("disable");
                        document.getElementById("LEASE_NO").readOnly = true;
                        $("#LEASE_NO").addClass("disable");
                        document.getElementById("LeaseLimitString").readOnly = true;
                        $("#LeaseLimitString").addClass("disable");
                        document.getElementById("GenLimitString").readOnly = true;
                        $("#GenLimitString").addClass("disable");

                        document.getElementById("FREE_ITEM1").readOnly = true;
                        $("#FREE_ITEM1").addClass("disable");
                        document.getElementById("FREE_ITEM2").readOnly = true;
                        $("#FREE_ITEM2").addClass("disable");
                        document.getElementById("FREE_ITEM3").readOnly = true;
                        $("#FREE_ITEM3").addClass("disable");
                        document.getElementById("FREE_ITEM4").readOnly = true;
                        $("#FREE_ITEM4").addClass("disable");
                        document.getElementById("FREE_ITEM5").readOnly = true;
                        $("#FREE_ITEM5").addClass("disable");
                        document.getElementById("FREE_ITEM6").readOnly = true;
                        $("#FREE_ITEM6").addClass("disable");
                        document.getElementById("FREE_ITEM7").readOnly = true;
                        $("#FREE_ITEM7").addClass("disable");
                        document.getElementById("FREE_ITEM8").readOnly = true;
                        $("#FREE_ITEM8").addClass("disable");
                        document.getElementById("FREE_ITEM9").readOnly = true;
                        $("#FREE_ITEM9").addClass("disable");
                        document.getElementById("FREE_ITEM10").readOnly = true;
                        $("#FREE_ITEM10").addClass("disable");
                        document.getElementById("FREE_ITEM11").readOnly = true;
                        $("#FREE_ITEM11").addClass("disable");
                        document.getElementById("FREE_ITEM12").readOnly = true;
                        $("#FREE_ITEM12").addClass("disable");
                        document.getElementById("FREE_ITEM13").readOnly = true;
                        $("#FREE_ITEM13").addClass("disable");
                        document.getElementById("FREE_ITEM14").readOnly = true;
                        $("#FREE_ITEM14").addClass("disable");
                        document.getElementById("FREE_ITEM15").readOnly = true;
                        $("#FREE_ITEM15").addClass("disable");
                        document.getElementById("FREE_ITEM16").readOnly = true;
                        $("#FREE_ITEM16").addClass("disable");
                        document.getElementById("FREE_ITEM17").readOnly = true;
                        $("#FREE_ITEM17").addClass("disable");
                        document.getElementById("FREE_ITEM18").readOnly = true;
                        $("#FREE_ITEM18").addClass("disable");
                        document.getElementById("FREE_ITEM19").readOnly = true;
                        $("#FREE_ITEM19").addClass("disable");
                        document.getElementById("FREE_ITEM20").readOnly = true;
                        $("#FREE_ITEM20").addClass("disable");
                        document.getElementById("FREE_ITEM21").readOnly = true;
                        $("#FREE_ITEM21").addClass("disable");
                        document.getElementById("FREE_ITEM22").readOnly = true;
                        $("#FREE_ITEM22").addClass("disable");
                        document.getElementById("FREE_ITEM23").readOnly = true;
                        $("#FREE_ITEM23").addClass("disable");
                        document.getElementById("FREE_ITEM24").readOnly = true;
                        $("#FREE_ITEM24").addClass("disable");
                        document.getElementById("FREE_ITEM25").readOnly = true;
                        $("#FREE_ITEM25").addClass("disable");
                        document.getElementById("FREE_ITEM26").readOnly = true;
                        $("#FREE_ITEM26").addClass("disable");
                        document.getElementById("FREE_ITEM27").readOnly = true;
                        $("#FREE_ITEM27").addClass("disable");
                        document.getElementById("FREE_ITEM28").readOnly = true;
                        $("#FREE_ITEM28").addClass("disable");
                        document.getElementById("FREE_ITEM29").readOnly = true;
                        $("#FREE_ITEM29").addClass("disable");
                        document.getElementById("FREE_ITEM30").readOnly = true;
                        $("#FREE_ITEM30").addClass("disable");
                        document.getElementById("FREE_ITEM31").readOnly = true;
                        $("#FREE_ITEM31").addClass("disable");
                        document.getElementById("FREE_ITEM32").readOnly = true;
                        $("#FREE_ITEM32").addClass("disable");
                        document.getElementById("FREE_ITEM33").readOnly = true;
                        $("#FREE_ITEM33").addClass("disable");
                        document.getElementById("FREE_ITEM34").readOnly = true;
                        $("#FREE_ITEM34").addClass("disable");
                        document.getElementById("FREE_ITEM35").readOnly = true;
                        $("#FREE_ITEM35").addClass("disable");
                        document.getElementById("FREE_ITEM36").readOnly = true;
                        $("#FREE_ITEM36").addClass("disable");
                        document.getElementById("FREE_ITEM37").readOnly = true;
                        $("#FREE_ITEM37").addClass("disable");
                        document.getElementById("FREE_ITEM38").readOnly = true;
                        $("#FREE_ITEM38").addClass("disable");
                        document.getElementById("FREE_ITEM39").readOnly = true;
                        $("#FREE_ITEM39").addClass("disable");
                        document.getElementById("FREE_ITEM40").readOnly = true;
                        $("#FREE_ITEM40").addClass("disable");
                        document.getElementById("FREE_ITEM41").readOnly = true;
                        $("#FREE_ITEM41").addClass("disable");
                        document.getElementById("FREE_ITEM42").readOnly = true;
                        $("#FREE_ITEM42").addClass("disable");
                        document.getElementById("FREE_ITEM43").readOnly = true;
                        $("#FREE_ITEM43").addClass("disable");
                        document.getElementById("FREE_ITEM44").readOnly = true;
                        $("#FREE_ITEM44").addClass("disable");
                        document.getElementById("FREE_ITEM45").readOnly = true;
                        $("#FREE_ITEM45").addClass("disable");
                        document.getElementById("FREE_ITEM46").readOnly = true;
                        $("#FREE_ITEM46").addClass("disable");
                        document.getElementById("FREE_ITEM47").readOnly = true;
                        $("#FREE_ITEM47").addClass("disable");
                        document.getElementById("FREE_ITEM48").readOnly = true;
                        $("#FREE_ITEM48").addClass("disable");
                        document.getElementById("FREE_ITEM49").readOnly = true;
                        $("#FREE_ITEM49").addClass("disable");
                        document.getElementById("FREE_ITEM50").readOnly = true;
                        $("#FREE_ITEM50").addClass("disable");
                        document.getElementById("FREE_ITEM51").readOnly = true;
                        $("#FREE_ITEM51").addClass("disable");
                        document.getElementById("FREE_ITEM52").readOnly = true;
                        $("#FREE_ITEM52").addClass("disable");
                        document.getElementById("FREE_ITEM53").readOnly = true;
                        $("#FREE_ITEM53").addClass("disable");
                        document.getElementById("FREE_ITEM54").readOnly = true;
                        $("#FREE_ITEM54").addClass("disable");
                        document.getElementById("FREE_ITEM55").readOnly = true;
                        $("#FREE_ITEM55").addClass("disable");
                        document.getElementById("FREE_ITEM56").readOnly = true;
                        $("#FREE_ITEM56").addClass("disable");
                        document.getElementById("FREE_ITEM57").readOnly = true;
                        $("#FREE_ITEM57").addClass("disable");
                        document.getElementById("FREE_ITEM58").readOnly = true;
                        $("#FREE_ITEM58").addClass("disable");
                        document.getElementById("FREE_ITEM59").readOnly = true;
                        $("#FREE_ITEM59").addClass("disable");
                        document.getElementById("FREE_ITEM60").readOnly = true;
                        $("#FREE_ITEM60").addClass("disable");
                        for (var nRemark = 1; nRemark <= 10; nRemark++) {
                            document.getElementById("REMARK" + nRemark).readOnly = true;
                            $("#REMARK" + nRemark).addClass("disable");
                        }

                        $("#ASSET_PIC_FILENAME").addClass("disable");
                        $("#ASSET_PIC_FILENAME").removeAttr("onclick");
                        $("#select_file_button").prop("disabled", true);
                        $("#select_file_button").attr('style', 'background-color:#cecece;');
                        $("#select_file_clear_button").prop("disabled", true);
                        $("#select_file_clear_button").attr('style', 'background-color:#cecece;margin-left:300px;');
                        //--2019/10/11 nv miyaji add end
                        //--2021/09/17 nv miyaji add start
                        $("#MNG_LOC_L_CD").attr("disabled", "disabled");
                        $("#LEASE_STATUS").attr("disabled", "disabled");
                        document.getElementById("MAINTE_LEASE_NUM").readOnly = true;
                        $("#MAINTE_LEASE_NUM").addClass("disable");
                        document.getElementById("MAINTE_LEASE_DAY").readOnly = true;
                        $("#MAINTE_LEASE_DAY").addClass("disable");
                        $('#DispLeaseDayWorkFlg').prop('disabled', true);
                        document.getElementById("MAINTE_DAY").readOnly = true;
                        $("#MAINTE_DAY").addClass("disable");
                        $('#DispMainteDayWorkFlg').prop('disabled', true);
                        //--2021/09/17 nv miyaji add end
                        //--2021/10/13 sv suzuki add start
                        $("#reflect_button").prop("disabled", true);
                        $("#reflect_button").attr('style', 'background-color:#cecece;');
                        //--2021/10/13 sv suzuki add end
                    }

                    isSettingValues = false;     //値セット中解除
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }

        //変更ボタンを設定
        document.getElementById("OnCreteButton").value = "@CommonConst.KEY_ON_UPDATE_CLICK";

        // --2020/01/07 miyaji update start
        //チェックボックスの状態からキーを設定する
        setting_checklist();
        // --2020/01/07 miyaji update end

        //モーダルウインドウを開く
        ShowModalWindow()

    }

    //削除ボタンクリック処理
    function btnDelete_onclick() {

        // 詳細項目の開閉状態を取得
        //document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

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
                    $("#CAT_L_CD").removeAttr("disabled");
                    //--2019/10/11 nv miyaji add start
                    $("#CAT_M_CD").removeAttr("disabled");
                    $("#CAT_S_CD").removeAttr("disabled");
                    $("#LOC_L_CD").removeAttr("disabled");
                    $("#LOC_M_CD").removeAttr("disabled");
                    $("#LOC_S_CD").removeAttr("disabled");
                    //--2019/10/11 nv miyaji add end
                    //--2021/09/30 nv miyaji add start
                    $("#MNG_LOC_L_CD").removeAttr("disabled");
                    $("#LEASE_STATUS").removeAttr("disabled");
                    //--2021/09/30 nv miyaji add end
                    var befAction = $('#form').attr("action");
                    $(this).dialog("close");
                    $('#form').attr("action", "@Url.Content("~/AssetList")/Delete");
                    $('#form').submit();
                    $('#form').attr("action", befAction);

                    // --2020/01/07 miyaji update start
                    //チェックボックスの状態からキーを設定する
                    setting_checklist();
                    // --2020/01/07 miyaji update end

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
        @*$("#CAT_L_CD").removeAttr("disabled");
        //確認メッセージ
        showConfirm("@Message.getMessage(CommonInfoMsg.DELETE_CONFIRM)", "@Url.Content("~/AssetList")/Delete");*@
    }

    //モーダル画面　変更ボタンクリック処理
    function btnUpdate_onclick() {
        if(document.getElementById("OnCreteButton").value == "@CommonConst.KEY_ON_UPDATE_CLICK"){
            //確認メッセージを出し、ＯＫなら変更処理
            // ダイアログのメッセージを設定
            $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)");

            // ダイアログを作成
            $("#show-dialog").dialog({
                modal: true,
                title: strConfirmTitle,
                dialogClass: "show-confirm-wrap",
                buttons: {
                    "OK": function () {
                        $("#CAT_L_CD").removeAttr("disabled");
                        $("#LEASE_STATUS").removeAttr("disabled");  // 2021/10/05 nv miyaji add
                        var befAction = $('#form').attr("action");
                        $(this).dialog("close");
                        $('#form').attr("action", "@Url.Content("~/AssetList")/Create");
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
            @*$("#CAT_L_CD").removeAttr("disabled");

            showConfirm("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)", "@Url.Content("~/AssetList")/Create")*@
            @*$('#form').attr("action", "@Url.Content("~/AssetList")/Create");*@
        }else{
            showConfirm("@Message.getMessage(CommonInfoMsg.CREATE_CONFIRM)", "@Url.Content("~/AssetList")/Create")
            @*$('#form').attr("action", "@Url.Content("~/AssetList")/Create");*@
        }
    }

    //モーダル画面　除却/除却解除ボタンクリック処理
    function btnRetire_onclick() {
        //$("#CAT_L_CD").removeAttr("disabled");

        if(document.getElementById("RETIRE_TS").value == ""){
            //確認メッセージを出し、ＯＫなら除却処理
            // ダイアログのメッセージを設定
            $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.RETIRE_CONFIRM)");

            // ダイアログを作成
            $("#show-dialog").dialog({
                modal: true,
                title: strConfirmTitle,
                dialogClass: "show-confirm-wrap",
                buttons: {
                    "OK": function () {
                        $("#CAT_L_CD").removeAttr("disabled");
                        $("#LEASE_STATUS").removeAttr("disabled");  // 2021/10/05 nv miyaji add
                        var befAction = $('#form').attr("action");
                        $(this).dialog("close");
                        $('#form').attr("action", "@Url.Content("~/AssetList")/Retire");
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
            @*showConfirm("@Message.getMessage(CommonInfoMsg.RETIRE_CONFIRM)", "@Url.Content("~/AssetList")/Retire")*@
        }else{
            //確認メッセージを出し、ＯＫなら除却解除処理
            // ダイアログのメッセージを設定
            $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.RETIRE_OFF_CONFIRM)");

            // ダイアログを作成
            $("#show-dialog").dialog({
                modal: true,
                title: strConfirmTitle,
                dialogClass: "show-confirm-wrap",
                buttons: {
                    "OK": function () {
                        $("#CAT_L_CD").removeAttr("disabled");
                        //--2019/10/11 nv miyaji add start
                        $("#CAT_M_CD").removeAttr("disabled");
                        $("#CAT_S_CD").removeAttr("disabled");
                        $("#LOC_L_CD").removeAttr("disabled");
                        $("#LOC_M_CD").removeAttr("disabled");
                        $("#LOC_S_CD").removeAttr("disabled");
                        //--2019/10/11 nv miyaji add end
                        //--2021/09/30 nv miyaji add start
                        $("#MNG_LOC_L_CD").removeAttr("disabled");
                        $("#LEASE_STATUS").removeAttr("disabled");
                        //--2021/09/30 nv miyaji add end
                        var befAction = $('#form').attr("action");
                        $(this).dialog("close");
                        $('#form').attr("action", "@Url.Content("~/AssetList")/Retire");
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
            @*showConfirm("@Message.getMessage(CommonInfoMsg.RETIRE_OFF_CONFIRM)", "@Url.Content("~/AssetList")/Retire")*@
        }

        @*$('#form').attr("action", "@Url.Content("~/AssetList")/Retire");*@
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        //画面ブロックを表示
        showLoading();

        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        // 詳細項目の開閉状態を取得　2023/04/30 sv sato
        //document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        $('#form').submit();   //2020/02/20 nv miyaji

    }

    //QR画面表示ボタンクリック処理
    function btnQR_onclick() {

        //テーブルの選択行を取得する。
        var text = [];
        //--2019/11/08 nv miyaji upd start
        //$("[type=checkbox]:checked").each(function () {
        //    //「, (カンマ)」で区切って分割する
        //    var result = $(this).val().split(',');
        //    text.push(result[0]);
        //    //text.push($(this).val());
        //});
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        var catLs = [];
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                //「, (カンマ)」で区切って分割する
                var result = checkBoxes[i].value.split(',');
                text.push(result[0]);
            }
        }

        //--2019/11/08 nv miyaji upd end
        if (text.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return;
        } else if (text.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return;
        }

        @*//タイトルの設定
        document.getElementById("popup-title-qr").innerHTML = "@ViewBag.Title" + "@Message.getMessage(CommonLabel.BUTTON_SHOW_QR)";*@

        // 以下では、１度しか、実行しないので、ダメ
        //document.getElementById("qrview").src = '@Url.Content("~/AssetList")/GetQRData'
        //document.getElementById("qrview").src = '@Url.Action("GetQRData", New With {.sAssetNo = Model.FirstOrDefault.ASSET_NO})'

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {


            //表示するＱＲの資産番号を表示
            document.getElementById("QRAssetNo").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)" + ":"  +　text[0]


            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/AssetList")/GetQRData',
                data: { sAssetNo: text[0] },
                cache: false,
                success: function (result, dataType) {
                    this;

                    //取得したデータを表示する　（以下では、正常に表示できない）
                    //document.getElementById("qrview").src = result;
                    //alert(result);
                    document.getElementById("qrview").src = '@Url.Content("~/QRimages/")' + result;
                    //alert('@Url.Content("~/QRimages/")' + result);

                    //データが存在しない場合エラー
                    if (result == null) {
                        return;
                    }


                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });

        }


        //モーダルウインドウを開く
        ShowQRModalWindow()

    }

    //画像選択ボタンクリック時処理
    function btnImageSelect_onclick() {
        $("#file_button").click();
    }

    //画像削除ボタンクリック時処理
    function btnImageClear_onclick() {
        $('#ASSET_PIC_FILENAME').val("");
        var imageUrl = "Images/noimage.png";
        document.getElementById("preview").src = '@Url.Content("~/")' + imageUrl;
        document.getElementById("NowImageUrl").value = '@Url.Content("~/")' + imageUrl;
        return;
    }

    //画像が選択された時の処理
    $(document).on('change', '#file_button', function(e) {
        var file = e.target.files[0];
        // キャンセルが押された場合は何もしない
        if(file === undefined){
            return;
        }

        // 画像ファイル以外の場合は何もしない
        if(file){
            if(file.type.indexOf("image") < 0){
                @*file.value="";
                $('#ASSET_PIC_FILENAME').val("");
                var imageUrl = "Images/noimage.png";
                document.getElementById("preview").src = '@Url.Content("~/")' + imageUrl;*@
                return;
            }
        }

        //画像をアップロードし表示する(Ajax非同期通信)
        var $form = $(this).closest('form')
        var formData = new FormData($form.get(0));
        formData.append("NewImage", $('#file_button')[0].files[0]);
        formData.append("ASSET_PIC_FILENAME", $('#ASSET_PIC_FILENAME').val);
        formData.append("ASSET_NO", $('#ASSET_NO').val);

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/UploadImage',
            data: formData,
            dataType: 'json',
            processData: false,
            contentType: false,
            cache: false,
            success: function (data, dataType) {
                this;

                $('#ASSET_PIC_FILENAME').val(data.replace(/^.*\\/, ""));
                var imageUrl = "@CommonConst.IMAGETMP_FOLDERNM" + "/" + data.replace(/^.*\\/, "");
                document.getElementById("preview").src = '@Url.Content("~/")' + imageUrl;
                document.getElementById("NowImageUrl").value = '@Url.Content("~/")' + imageUrl;

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;

                $('#ASSET_PIC_FILENAME').val("");
                var imageUrl = "@CommonConst.ROOT_FOLDERNM" + "Images/noimage.png";
                document.getElementById("preview").src = '@Url.Content("~/")' + imageUrl;
                document.getElementById("NowImageUrl").value = '@Url.Content("~/")' + imageUrl;
            }
        });

        @*$('#ASSET_PIC_FILENAME').val($(this).val().replace(/^.*\\/, ""));
        var imageUrl = "@Message.getSettingInfo(CommonSettingKey.Image.KeyDiv, CommonSettingKey.Image.Key1, CommonSettingKey.Image.Key2, CommonSettingKey.Image.Key3).DATA1" + "/" + $(this).val().replace(/^.*\\/, "");
        document.getElementById("preview").src = '@Url.Content("~/")' + imageUrl;*@


    })

    //全選択ボタンクリック時処理
    function btnSelectAll_onclick() {
        var chkFlag = false;
        var btn = document.getElementById("btnSelectAll");
        if( btn.value == "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)"){
            btn.value ="@Message.getMessage(CommonLabel.BUTTON_ALL_OFF)";
            $('input[name="checkEdit"]').prop('checked',true)
            chkFlag = true;
        }else{
            btn.value ="@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
            $('input[name="checkEdit"]').prop('checked',false)
            chkFlag = false;
        }
        // 実行
        //$('#form').attr("action", "@Url.Content("~/AssetList")/SelectAll");
        //$('#form').submit();

        //非同期で現在の検索条件での全件を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/SelectAll',
            data: {blnSelectAll:chkFlag},
            dataType: 'json',
            cache: false,
            beforeSend: function () {
                //画面ブロックを表示
                showLoading();
            },
            success: function (data, dataType) {
                //件数を設定する
                document.getElementById("selectcount").innerHTML = data;

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            },
            complete: function () {
                //画面ブロックを解除
                hideLoading();
            }
        });
    }

    ////モーダルの登録ボタンクリック処理
    //function modal_btnCreate_onclick(){
    //    //クリックされたラジオボタンの値を隠し項目に格納する
    //    document.getElementById("AUTH_DIV").value = $('input[name="AUTH_DIV"]:checked').val();
    //}

    //※複数選択（Select2）の getMultiSelectValues / setMultiSelectValues / clearMultiSelect は Scripts\select2-common.js
    //※資産番号（複数入力）の入力制限 editAssetNoMultiInput は Scripts\common.js

    //[CC431] ADD Start 20261007
    //検索条件 ロケーション（原価センタ・場所）の連動
    //  場所から選ぶ OFF：中区分（m）で小区分（s）の選択肢を絞り込む
    //  場所から選ぶ ON ：小区分（s）で中区分（m）の選択肢を絞り込む
    //  絞り込む側の選択値を変えたら、絞り込まれる側の選択値は使えるものを残し、使えなくなったものを消す
    //  xhr：選択肢を取得中の通信（続けて選び直したときに、古い応答で選択肢が上書きされないよう中止する）
    var searchLocPairs = {
        Loc: { l: "#SearchLocL", m: "#SearchLocM", s: "#SearchLocS", reverse: "#SearchLocReverse", xhr: null }
    };

    //場所から選ぶが ON か（チェックボックスがない場合は OFF）
    function isSearchLocReverse(key) {
        return $(searchLocPairs[key].reverse).prop("checked") === true;
    }

    //中区分・小区分の選択値を変えたとき
    //  変えた側が絞り込む側（OFF なら中区分、ON なら小区分）のときだけ、反対側の選択肢を作り直す
    function onSearchLocChange(key, level) {
        var fromLevel = isSearchLocReverse(key) ? "@LOC_DIV.LocationS" : "@LOC_DIV.LocationM";
        if (level !== fromLevel) {
            return;
        }
        refreshSearchLocOptions(key);
    }

    //絞り込まれる側の選択肢を、絞り込む側の選択値で作り直す
    //  keepValues：絞り込まれる側で選び直す値（省略時は今の選択値。使えるものだけ残る）
    //  callback：選択肢を作り終えたら呼ぶ関数（省略可）
    function refreshSearchLocOptions(key, keepValues, callback) {
        var pair = searchLocPairs[key];
        var reverse = isSearchLocReverse(key);
        var from = reverse ? pair.s : pair.m;
        var to = reverse ? pair.m : pair.s;
        var toLevel = reverse ? "@LOC_DIV.LocationM" : "@LOC_DIV.LocationS";
        if (!keepValues) {
            keepValues = getMultiSelectValues(to);
        }

        abortSearchLocRequest(key);

        // 絞り込む側が未選択の場合は、絞り込まれる側をクリアして非活性
        var codes = getMultiSelectValues(from);
        if (codes.length == 0) {
            clearMultiSelect(to);
            $(to).prop("disabled", true);
            if (callback) {
                callback();
            }
            return;
        }

        loadSearchLocOptions(key, toLevel, codes, to, keepValues, callback);
    }

    //場所から選ぶを変えたとき
    //  ON にしたら：場所の選択値は残し、原価センタはクリア。OFF にしたら：原価センタの選択値は残し、場所はクリア
    //  残した側の選択肢を全件にしてから、その選択値で反対側の選択肢を作る
    function onSearchLocReverseChange(key) {
        var pair = searchLocPairs[key];
        var reverse = isSearchLocReverse(key);
        var from = reverse ? pair.s : pair.m;
        var fromLevel = reverse ? "@LOC_DIV.LocationS" : "@LOC_DIV.LocationM";
        var to = reverse ? pair.m : pair.s;

        abortSearchLocRequest(key);

        clearMultiSelect(to);
        $(to).prop("disabled", true);

        // 会社が未選択の場合は取得しない
        if (!$(pair.l).val()) {
            return;
        }

        loadSearchLocOptions(key, fromLevel, [], from, getMultiSelectValues(from), function () {
            refreshSearchLocOptions(key);
        });
    }

    //会社を変えたとき（初期表示の復元を含む）
    //  原価センタ・場所をクリアし（場所から選ぶの ON／OFF は残す）、絞り込む側の選択肢を取得してから、反対側の選択肢を作る
    //  keepM／keepS：選び直す値（初期表示で、検索後の選択値を復元する場合。会社を変えた場合は省略）
    //  callback：選択肢を作り終えたら呼ぶ関数（省略可。初期表示の自動検索に使う）
    function resetSearchLocOptions(key, keepM, keepS, callback) {
        var pair = searchLocPairs[key];
        var reverse = isSearchLocReverse(key);
        var from = reverse ? pair.s : pair.m;
        var fromLevel = reverse ? "@LOC_DIV.LocationS" : "@LOC_DIV.LocationM";

        abortSearchLocRequest(key);

        clearMultiSelect(pair.m);
        $(pair.m).prop("disabled", true);
        clearMultiSelect(pair.s);
        $(pair.s).prop("disabled", true);

        // 会社が未選択の場合はここで終了
        if (!$(pair.l).val()) {
            if (callback) {
                callback();
            }
            return;
        }

        loadSearchLocOptions(key, fromLevel, [], from, (reverse ? keepS : keepM) || [], function () {
            refreshSearchLocOptions(key, (reverse ? keepM : keepS) || [], callback);
        });
    }

    //選択肢を取得中の通信を中止する
    function abortSearchLocRequest(key) {
        var pair = searchLocPairs[key];
        if (pair.xhr) {
            var xhr = pair.xhr;
            pair.xhr = null;
            xhr.abort();
        }
    }

    //選択肢を取得して入れ替え、keepValues のうち選択肢にあるものだけを選択し直す（onchange は発生させない）
    //  codes：絞り込みに使う反対側の選択値（絞り込む側の選択肢を取得する場合はサーバー側で使わない）
    //  callback：取得し終えたら呼ぶ関数（省略可。中止した通信では呼ばない）
    function loadSearchLocOptions(key, level, codes, target, keepValues, callback) {
        var pair = searchLocPairs[key];
        var xhr = $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/GetSearchLocationOptions',
            data: { selectLCode: $(pair.l).val(), level: level, reverse: isSearchLocReverse(key), codes: codes.join(",") },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (pair.xhr !== xhr) {
                    return;    // 中止した通信、または古い通信の応答
                }
                pair.xhr = null;
                if (data != null) {
                    var select = $(target)[0];
                    select.options.length = 0;
                    // 複数選択のため選択解除用の空の選択肢は不要
                    for (var i = 0; i < data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data[i].LOC_CD;
                        op.text = data[i].LOC_NAME;
                        select.appendChild(op);
                    }
                    $(target).prop("disabled", false);
                    // 選択肢にない値は jQuery が外すため、使える値だけが残る
                    setMultiSelectValues(target, keepValues);
                }

                if (callback) {
                    callback();
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                if (pair.xhr !== xhr) {
                    return;    // 中止した通信、または古い通信の応答
                }
                pair.xhr = null;
                // 中区分・小区分を使用しない設定（応答が空）の場合もここに来るため、続きの処理は行う
                if (callback) {
                    callback();
                }
            }
        });
        pair.xhr = xhr;
    }
    //[CC431] ADD End 20261007

    //大区分選択
    function ChangeLocationL(locL, initialM, initialS, doAutoSearch) {
        // 値セット中は処理しない
        if (isSettingValues) return;

        // 検索条件の中区分・小区分は複数選択
        let isCondition = (locL.name == "SearchLocL");

        //[CC431] UPD Start 20261007
        if(isCondition){
            // 検索条件：原価センタ・場所をクリアし、場所から選ぶの ON／OFF に合わせて選択肢を作り直す
            // （初期表示は initialM／initialS の選択値を復元し、自動検索する。会社が未選択の場合は検索しない）
            resetSearchLocOptions('Loc', initialM, initialS, function () {
                if (doAutoSearch && locL.value && document.getElementById('FirstSearch').value == "True") {
                    // 検索処理実行
                    document.getElementById('Search_Button').click();
                }
            });
            return;
        }else{
            // 中区分をクリア
            $("#LOC_M_CD").val("").trigger("change");
            $("#LOC_M_CD").prop("disabled", true);
            // 小区分をクリア
            $("#LOC_S_CD").val("").trigger("change");
            $("#LOC_S_CD").prop("disabled", true);
        }
        //[CC431] UPD End 20261007


        // 選択されていない場合はここで終了
        if (!locL.value) {
             return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationM", selectLCode: locL.value, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocM = ""
                    if(isCondition){
                        SearchLocM = document.getElementById("SearchLocM");
                    }else{
                        SearchLocM =document.getElementById("LOC_M_CD");
                    }

                    SearchLocM.options.length = 0;

                    // 複数選択（検索条件）では選択解除用の空の選択肢は不要
                    if(!isCondition){
                        let opBlank = document.createElement("option");
                        opBlank.value = "";
                        opBlank.text = "";
                        SearchLocM.appendChild(opBlank);
                    }

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocM.appendChild(op);
                        //document.getElementById("SearchLocM").appendChild(op);
                    }
                }
                // 中区分データ取得
                if(isCondition){
                    $("#SearchLocM").removeAttr("disabled");
                    // 選択肢の入れ替えをSelect2に反映
                    setMultiSelectValues(SearchLocM, initialM);
                }else{
                    $("#LOC_M_CD").removeAttr("disabled");
                    SearchLocM.value = data.locMDropdownData;
                    //document.getElementById("SearchLocM").value = data.locMDropdownData;
                }
                if (initialM && initialM.length > 0) {
                    if(!isCondition){
                        $(SearchLocM).val(initialM).trigger("change");
                    }
                    ChangeLocationM(SearchLocM, initialS, doAutoSearch);
                } else if (doAutoSearch) {
                    // 検索処理実行
                    if (document.getElementById('FirstSearch').value == "True") {
                        document.getElementById('Search_Button').click();
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //中区分選択
    function ChangeLocationM(locM, initialS, doAutoSearch) {
        // 値セット中は処理しない
        if (isSettingValues) return;

        // 検索条件の中区分・小区分は複数選択
        let isCondition = (locM.name == "SearchLocM");

        var lCode = ""
        if(isCondition){
            lCode = document.getElementById("SearchLocL").value;
        }else{
            lCode = document.getElementById("LOC_L_CD").value;
        }

        // 選択中の中区分コード（検索条件は複数選択のため「,」区切りで渡す）
        var mCode = ""
        if(isCondition){
            mCode = getMultiSelectValues(locM).join(",");
        }else{
            mCode = locM.value;
        }

        // 小区分をクリア
        if(isCondition){
            clearMultiSelect("#SearchLocS");
            $("#SearchLocS").prop("disabled", true);
        }else{
            $("#LOC_S_CD").val("").trigger("change");
            $("#LOC_S_CD").prop("disabled", true);
        }


        // 選択されていない場合はここで終了
        if (!mCode) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationS", selectLCode: lCode, selectMCode: mCode },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocS = ""
                    if(isCondition){
                        SearchLocS = document.getElementById("SearchLocS");
                    }else{
                        SearchLocS = document.getElementById("LOC_S_CD");
                    }
                    SearchLocS.options.length = 0;

                    // 複数選択（検索条件）では選択解除用の空の選択肢は不要
                    if(!isCondition){
                        let opBlank = document.createElement("option");
                        opBlank.value = "";
                        opBlank.text = "";
                        SearchLocS.appendChild(opBlank);
                    }

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocS.appendChild(op);
                    }
                }
                // 小区分データ取得
                if(isCondition){
                    $("#SearchLocS").removeAttr("disabled");
                    // 選択肢の入れ替えをSelect2に反映
                    setMultiSelectValues(SearchLocS, initialS);
                }else{
                    $("#LOC_S_CD").removeAttr("disabled");

                    SearchLocS.value = data.locSDropdownData;
                    //--2021/10/13 sv suzuki add start
                    if (initialS) {
                        $(SearchLocS).val(initialS).trigger("change");
                    }
                    //--2021/10/13 sv suzuki add end
                }

                if (doAutoSearch) {
                    // 検索処理実行
                    if (document.getElementById('FirstSearch').value == "True") {
                        document.getElementById('Search_Button').click();
                    }
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //資産番号フォーカス移動時処理
    function assetNoOnBlur(input) {
        var val = input.value;

        if (val.length >= "@CommonConst.IS_TANGIBLE_DIGIT") {
            //入力値が5文字以上ある場合
            //5桁目を取得
            var str = val.charAt("@CommonConst.IS_TANGIBLE_DIGIT"-1);
            if (str === "@CommonConst.NOT_TANGIBLE_CHAR") {
                //5桁目=2の場合、無形資産
                $('[id=IS_TANGIBLE_NOT_TANGIBLE]').prop('checked', true);
                document.getElementById("IS_TANGIBLE").value = "@TANGIBLE.NOT_TANGIBLE";
            } else {
                //5桁目<>2の場合、有形資産
                $('[id=IS_TANGIBLE_TANGIBLE]').prop('checked', true);
                document.getElementById("IS_TANGIBLE").value = "@TANGIBLE.TANGIBLE";
            }
        } else {
            //4文字以下の場合は有形資産
            $('[id=IS_TANGIBLE_TANGIBLE]').prop('checked', true);
            document.getElementById("IS_TANGIBLE").value = "@TANGIBLE.TANGIBLE";
        }
    }

    //カテゴリー大分類選択
    function ChangeCategoryL(catL, initialM, initialS) {
        // 値セット中は処理しない
        if (isSettingValues) return;

        if (catL.name == "SearchCatL") {
            //検索条件の場合は終了(中分類以下が検索条件にないため)
            return;
        }else{
            // 中区分をクリア
            $("#CAT_M_CD").val("").trigger("change");
            $("#CAT_M_CD").attr("disabled", "disabled");
            // 小区分をクリア
            $("#CAT_S_CD").val("").trigger("change");
            $("#CAT_S_CD").attr("disabled", "disabled");
        }

        // 選択されていない場合はここで終了
        if (!catL.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/ChangeCategory',
            data: { categoryLevel: "@CAT_DIV.CategoryM", selectLCode: catL.value, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchCatM = ""
                    SearchCatM = document.getElementById("CAT_M_CD");
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
                $("#CAT_M_CD").removeAttr("disabled");

                SearchCatM.value = data.catMDropdownData;
                //--2021/10/13 sv suzuki add start
                if (initialM) {
                    $(SearchCatM).val(initialM).trigger("change");
                    ChangeCategoryM(SearchCatM,initialS);
                }
                //--2021/10/13 sv suzuki add end

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //カテゴリー中分類選択
    function ChangeCategoryM(catM, initialS) {
        // 値セット中は処理しない
        if (isSettingValues) return;

        var lCode = ""
        lCode = document.getElementById("CAT_L_CD").value;

        // 小分類をクリア
        $("#CAT_S_CD").val("").trigger("change");
        $("#CAT_S_CD").attr("disabled", "disabled");

        // 選択されていない場合はここで終了
        if (!catM.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/ChangeCategory',
            data: { categoryLevel: "@CAT_DIV.CategoryS", selectLCode: lCode, selectMCode: catM.value },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchCatS = ""
                    SearchCatS = document.getElementById("CAT_S_CD");
                    SearchCatS.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchCatS.appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].CAT_CD;
                        op.text = data.Data[i].CAT_NAME;
                        SearchCatS.appendChild(op);
                    }
                }
                // 小分類データ取得
                $("#CAT_S_CD").removeAttr("disabled");

                SearchCatS.value = data.catSDropdownData;
                //--2021/10/13 sv suzuki add start
                if (initialS) {
                    $(SearchCatS).val(initialS).trigger("change");
                }
                //--2021/10/13 sv suzuki add end
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //期限の期間表示非表示ラジオボタンクリック処理
    function chkSearchKikan_onclick(element) {
        //if(element.name == "SearchLeaseLimitKikan"){
        //    var bret = element.vale;
        //}
        ////クリックされたラジオボタンの値を隠し項目に格納する
        //document.getElementById("SearchKengen").value = element.value;
        var id = element.id;
        if (id == "SearchLeaseLimitKikan") {
            if ($('#SearchLeaseLimitKikan').prop('checked')) {
                $("#SearchLeaseLimitFrom").prop("disabled", false);
                $("#SearchLeaseLimitTo").prop("disabled", false);
            } else {
                $("#SearchLeaseLimitFrom").prop("disabled", true);
                $("#SearchLeaseLimitTo").prop("disabled", true);
            }
        } else if (id == "SearchGenericLimitKikan") {
            if ($('#SearchGenericLimitKikan').prop('checked')) {
                $("#SearchGenericLimitFrom").prop("disabled", false);
                $("#SearchGenericLimitTo").prop("disabled", false);
            } else {
                $("#SearchGenericLimitFrom").prop("disabled", true);
                $("#SearchGenericLimitTo").prop("disabled", true);
            }
        } else if (id == "SearchHenkyakuLimitKikan") {
            if ($('#SearchHenkyakuLimitKikan').prop('checked')) {
                $("#SearchHenkyakuLimitFrom").prop("disabled", false);
                $("#SearchHenkyakuLimitTo").prop("disabled", false);
            } else {
                $("#SearchHenkyakuLimitFrom").prop("disabled", true);
                $("#SearchHenkyakuLimitTo").prop("disabled", true);
            }
        }
    }

    //選択した資産番号をセッションに保存します
    function SetAssetNo(value) {
        // 詳細項目の開閉状態を取得
        //document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        // 履歴詳細画面に引き渡す資産番号を設定
        document.getElementById("SelectedAssetNo").value = value;


        $('#form').attr("action", "@Url.Content("~/AssetList")/SetAssetNo");
        $('#form').submit();

        @*var formdata = new FormData($('form').get(0));
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/SetAssetNo',
            data: { assetNo: value },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });*@
    }

    //検索項目欄　詳細検索クリック時処理
    //'【詳細検索エリアの開閉切り替え】
    //'　リンクの文言（▼詳細検索／▲詳細検索）を現在の状態として扱い、クリックのたびに反転させる。
    //'　あわせて hidden 項目 DetailOpenFlg を更新し、ポストバック後も同じ状態で復元できるようにする。
    function DetailAccordion_onclick() {
        if (document.getElementById("DetailAccordion").innerHTML == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)"){
            //'　開いている状態 → 閉じる
            $(".detail-accordion").css("display", "none");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)"
        }else{
            //'　閉じている状態 → 開く
            $(".detail-accordion").css("display", "flex");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)"
        }
        document.getElementById("DetailOpenFlg").value = document.getElementById("DetailAccordion").innerText;
    }

    //チェックボックスクリック処理
    function check_onclick(element) {
        var checkconfirm = element.checked;
        var count = document.getElementById("selectcount").innerHTML;
        if (checkconfirm) {
            count++;
        } else {
            count--;
        }
        document.getElementById("selectcount").innerHTML = count;
    }

    //チェックボックスの選択状態を設定する
    function setting_checklist() {
        var keys = [];
        var nonekeys = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");

        for (var i = 0; i < checkBoxes.length; i++) {
            //チェックボックスの場合
            if (checkBoxes[i].type == "checkbox") {
                //キーを取得する
                var result = checkBoxes[i].value

                if (checkBoxes[i].checked) {
                    //選択されている場合
                    keys.push(result);
                } else {
                    //選択されていない場合
                    nonekeys.push(result);
                }
            }
        }

        // スラッシュ区切りの文字列を設定
        document.getElementById("SelectKeys").value = keys.join("/");
        document.getElementById("NoneSelectKeys").value = nonekeys.join("/");

        return keys;
    }
    //--2021/10/13 sv suzuki add start
    //反映ボタンクリック時処理
    function btnReflect_onclick() {
        var productNo = document.getElementById("PRODUCT_NO").value;
        var catLCd = document.getElementById("CAT_L_CD").value;
        //非同期で品番に紐づく情報を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/GetProductNoInfo',
            dataType: 'json',
            cache: false,
            data: { ProductNo: productNo ,CatLCd: catLCd},
            success: function (data, dataType) {
                if (data.ErrCode == "" || data.ErrCode == "@CommonLabel.MSG_ERR_ASSET_ID_REG") {
                    //取得したデータを表示する
                    isSettingValues = true;     //値セット中
                    if (data.ASSET_DIV != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("ASSET_DIV").value = data.ASSET_DIV;
                    }
                    if (data.ASSET_NAME != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("ASSET_NAME").value = data.ASSET_NAME;
                    }
                    if (data.MNG_LOC_L_CD != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("MNG_LOC_L_CD").value = data.MNG_LOC_L_CD;
                    }

                    //ロケのセットに関して3区分のセット有無、即ち2の3乗=8通り考慮する。ただし、大区分と小区分のみの変更パターンは考慮しない
                    //ロケ大区分を変更
                    if (data.LOC_L_CD != "@CommonConst.NOT_UPD_STR" && data.LOC_M_CD == "@CommonConst.NOT_UPD_STR") {
                        $("#LOC_L_CD").val(data.LOC_L_CD).trigger("change");
                        ChangeLocationL(document.getElementById("LOC_L_CD"));
                    }
                        //ロケ大区分、中区分を変更
                    else if (data.LOC_L_CD != "@CommonConst.NOT_UPD_STR" && data.LOC_M_CD != "@CommonConst.NOT_UPD_STR" && data.LOC_S_CD == "@CommonConst.NOT_UPD_STR") {
                        $("#LOC_L_CD").val(data.LOC_L_CD).trigger("change");
                        ChangeLocationL(document.getElementById("LOC_L_CD"),data.LOC_M_CD);
                    }
                        //ロケ大区分、中区分、小区分を変更
                    else if (data.LOC_L_CD != "@CommonConst.NOT_UPD_STR" && data.LOC_M_CD != "@CommonConst.NOT_UPD_STR" && data.LOC_S_CD != "@CommonConst.NOT_UPD_STR") {
                        $("#LOC_L_CD").val(data.LOC_L_CD).trigger("change");
                        ChangeLocationL(document.getElementById("LOC_L_CD"),data.LOC_M_CD,data.LOC_S_CD);
                    }
                        //ロケ大区分、中区分を変更せず、小区分を変更
                    else if (data.LOC_L_CD == "@CommonConst.NOT_UPD_STR" && data.LOC_M_CD == "@CommonConst.NOT_UPD_STR" && data.LOC_S_CD != "@CommonConst.NOT_UPD_STR") {
                        ChangeLocationM(document.getElementById("LOC_M_CD"),data.LOC_S_CD);
                    }
                        //ロケ大区分を変更せず、中区分を変更
                    else if (data.LOC_L_CD == "@CommonConst.NOT_UPD_STR" && data.LOC_M_CD != "@CommonConst.NOT_UPD_STR" && data.LOC_S_CD == "@CommonConst.NOT_UPD_STR") {
                        ChangeLocationL(document.getElementById("LOC_L_CD"),data.LOC_M_CD);
                    }
                        //ロケ大区分を変更せず、中区分、小区分を変更
                    else if (data.LOC_L_CD == "@CommonConst.NOT_UPD_STR" && data.LOC_M_CD != "@CommonConst.NOT_UPD_STR" && data.LOC_S_CD != "@CommonConst.NOT_UPD_STR") {
                        ChangeLocationL(document.getElementById("LOC_L_CD"),data.LOC_M_CD,data.LOC_S_CD);
                    }

                    //カテゴリの考慮はロケの考慮と同じ
                    //カテゴリ大分類を変更
                    if (data.CAT_L_CD != "@CommonConst.NOT_UPD_STR" && data.CAT_M_CD == "@CommonConst.NOT_UPD_STR") {
                        $("#CAT_L_CD").val(data.CAT_L_CD).trigger("change");
                        ChangeCategoryL(document.getElementById("CAT_L_CD"));
                    }
                        //カテゴリ大分類、カテゴリ中分類を変更
                    else if (data.CAT_L_CD != "@CommonConst.NOT_UPD_STR" && data.CAT_M_CD != "@CommonConst.NOT_UPD_STR" && data.CAT_S_CD == "@CommonConst.NOT_UPD_STR") {
                        $("#CAT_L_CD").val(data.CAT_L_CD).trigger("change");
                        ChangeCategoryL(document.getElementById("CAT_L_CD"), data.CAT_M_CD);
                    }
                        //カテゴリ大分類、カテゴリ中分類、カテゴリ小分類を変更
                    else if (data.CAT_L_CD != "@CommonConst.NOT_UPD_STR" && data.CAT_M_CD != "@CommonConst.NOT_UPD_STR" && data.CAT_S_CD != "@CommonConst.NOT_UPD_STR") {
                        $("#CAT_L_CD").val(data.CAT_L_CD).trigger("change");
                        ChangeCategoryL(document.getElementById("CAT_L_CD"), data.CAT_M_CD, data.CAT_S_CD);
                    }
                        //カテゴリ大分類、中分類を変更せず、小分類を変更
                    else if (data.CAT_L_CD == "@CommonConst.NOT_UPD_STR" && data.CAT_M_CD == "@CommonConst.NOT_UPD_STR" && data.CAT_S_CD != "@CommonConst.NOT_UPD_STR") {
                        ChangeCategoryM(document.getElementById("CAT_M_CD"),data.CAT_S_CD);
                    }
                        //カテゴリ大分類を変更せず、中分類を変更
                    else if (data.CAT_L_CD == "@CommonConst.NOT_UPD_STR" && data.CAT_M_CD != "@CommonConst.NOT_UPD_STR" && data.CAT_S_CD == "@CommonConst.NOT_UPD_STR") {
                        ChangeCategoryL(document.getElementById("CAT_L_CD"),data.CAT_M_CD);
                    }
                        //カテゴリ大分類を変更せず、中分類、小分類を変更
                    else if (data.CAT_L_CD == "@CommonConst.NOT_UPD_STR" && data.CAT_M_CD != "@CommonConst.NOT_UPD_STR" && data.CAT_S_CD != "@CommonConst.NOT_UPD_STR") {
                        ChangeCategoryL(document.getElementById("CAT_L_CD"),data.CAT_M_CD,data.CAT_S_CD);
                    }

                    if (data.MAKER != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("MAKER").value = data.MAKER;
                    }
                    if (data.MODEL_NO != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("MODEL_NO").value = data.MODEL_NO;
                    }
                    if (data.LEASE_NO != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("LEASE_NO").value = data.LEASE_NO;
                    }
                    if (data.LeaseLimitString != "@CommonConst.NOT_UPD_INT") {
                        document.getElementById("LeaseLimitString").value = data.LeaseLimitString;
                    }
                    if (data.GenLimitString != "@CommonConst.NOT_UPD_INT") {
                        document.getElementById("GenLimitString").value = data.GenLimitString;
                    }

                    if (data.FREE_ITEM1 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM1").value = data.FREE_ITEM1;
                    }
                    if (data.FREE_ITEM2 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM2").value = data.FREE_ITEM2;
                    }
                    if (data.FREE_ITEM3 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM3").value = data.FREE_ITEM3;
                    }
                    if (data.FREE_ITEM4 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM4").value = data.FREE_ITEM4;
                    }
                    if (data.FREE_ITEM5 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM5").value = data.FREE_ITEM5;
                    }
                    if (data.FREE_ITEM6 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM6").value = data.FREE_ITEM6;
                    }
                    if (data.FREE_ITEM7 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM7").value = data.FREE_ITEM7;
                    }
                    if (data.FREE_ITEM8 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM8").value = data.FREE_ITEM8;
                    }
                    if (data.FREE_ITEM9 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM9").value = data.FREE_ITEM9;
                    }
                    if (data.FREE_ITEM10 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM10").value = data.FREE_ITEM10;
                    }
                    if (data.FREE_ITEM11 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM11").value = data.FREE_ITEM11;
                    }
                    if (data.FREE_ITEM12 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM12").value = data.FREE_ITEM12;
                    }
                    if (data.FREE_ITEM13 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM13").value = data.FREE_ITEM13;
                    }
                    if (data.FREE_ITEM14 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM14").value = data.FREE_ITEM14;
                    }
                    if (data.FREE_ITEM15 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM15").value = data.FREE_ITEM15;
                    }
                    if (data.FREE_ITEM16 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM16").value = data.FREE_ITEM16;
                    }
                    if (data.FREE_ITEM17 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM17").value = data.FREE_ITEM17;
                    }
                    if (data.FREE_ITEM18 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM18").value = data.FREE_ITEM18;
                    }
                    if (data.FREE_ITEM19 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM19").value = data.FREE_ITEM19;
                    }
                    if (data.FREE_ITEM20 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM20").value = data.FREE_ITEM20;
                    }
                    if (data.FREE_ITEM21 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM21").value = data.FREE_ITEM21;
                    }
                    if (data.FREE_ITEM22 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM22").value = data.FREE_ITEM22;
                    }
                    if (data.FREE_ITEM23 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM23").value = data.FREE_ITEM23;
                    }
                    if (data.FREE_ITEM24 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM24").value = data.FREE_ITEM24;
                    }
                    if (data.FREE_ITEM25 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM25").value = data.FREE_ITEM25;
                    }
                    if (data.FREE_ITEM26 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM26").value = data.FREE_ITEM26;
                    }
                    if (data.FREE_ITEM27 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM27").value = data.FREE_ITEM27;
                    }
                    if (data.FREE_ITEM28 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM28").value = data.FREE_ITEM28;
                    }
                    if (data.FREE_ITEM29 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM29").value = data.FREE_ITEM29;
                    }
                    if (data.FREE_ITEM30 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM30").value = data.FREE_ITEM30;
                    }
                    if (data.FREE_ITEM31 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM31").value = data.FREE_ITEM31;
                    }
                    if (data.FREE_ITEM32 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM32").value = data.FREE_ITEM32;
                    }
                    if (data.FREE_ITEM33 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM33").value = data.FREE_ITEM33;
                    }
                    if (data.FREE_ITEM34 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM34").value = data.FREE_ITEM34;
                    }
                    if (data.FREE_ITEM35 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM35").value = data.FREE_ITEM35;
                    }
                    if (data.FREE_ITEM36 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM36").value = data.FREE_ITEM36;
                    }
                    if (data.FREE_ITEM37 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM37").value = data.FREE_ITEM37;
                    }
                    if (data.FREE_ITEM38 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM38").value = data.FREE_ITEM38;
                    }
                    if (data.FREE_ITEM39 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM39").value = data.FREE_ITEM39;
                    }
                    if (data.FREE_ITEM40 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM40").value = data.FREE_ITEM40;
                    }
                    if (data.FREE_ITEM41 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM41").value = data.FREE_ITEM41;
                    }
                    if (data.FREE_ITEM42 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM42").value = data.FREE_ITEM42;
                    }
                    if (data.FREE_ITEM43 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM43").value = data.FREE_ITEM43;
                    }
                    if (data.FREE_ITEM44 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM44").value = data.FREE_ITEM44;
                    }
                    if (data.FREE_ITEM45 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM45").value = data.FREE_ITEM45;
                    }
                    if (data.FREE_ITEM46 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM46").value = data.FREE_ITEM46;
                    }
                    if (data.FREE_ITEM47 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM47").value = data.FREE_ITEM47;
                    }
                    if (data.FREE_ITEM48 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM48").value = data.FREE_ITEM48;
                    }
                    if (data.FREE_ITEM49 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM49").value = data.FREE_ITEM49;
                    }
                    if (data.FREE_ITEM50 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM50").value = data.FREE_ITEM50;
                    }
                    if (data.FREE_ITEM51 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM51").value = data.FREE_ITEM51;
                    }
                    if (data.FREE_ITEM52 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM52").value = data.FREE_ITEM52;
                    }
                    if (data.FREE_ITEM53 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM53").value = data.FREE_ITEM53;
                    }
                    if (data.FREE_ITEM54 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM54").value = data.FREE_ITEM54;
                    }
                    if (data.FREE_ITEM55 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM55").value = data.FREE_ITEM55;
                    }
                    if (data.FREE_ITEM56 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM56").value = data.FREE_ITEM56;
                    }
                    if (data.FREE_ITEM57 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM57").value = data.FREE_ITEM57;
                    }
                    if (data.FREE_ITEM58 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM58").value = data.FREE_ITEM58;
                    }
                    if (data.FREE_ITEM59 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM59").value = data.FREE_ITEM59;
                    }
                    if (data.FREE_ITEM60 != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("FREE_ITEM60").value = data.FREE_ITEM60;
                    }

                    for (var nRemark = 1; nRemark <= 10; nRemark++) {
                        if (data["REMARK" + nRemark] != "@CommonConst.NOT_UPD_STR") {
                            document.getElementById("REMARK" + nRemark).value = data["REMARK" + nRemark];
                        }
                    }

                    if (data.ASSET_PIC_FILENAME != "@CommonConst.NOT_UPD_STR") {
                        document.getElementById("ASSET_PIC_FILENAME").value = data.ASSET_PIC_FILENAME;
                    }
                    if (data.ErrCode == "@CommonLabel.MSG_ERR_ASSET_ID_REG") {
                        showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_ASSET_ID_REG)");
                    }
                    isSettingValues = false;     //値セット中解除
                }
                else if (data.ErrCode == "@CommonLabel.MSG_ERR_NOT_ENTERD") {
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_NOT_ENTERD)【@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)】");
                }
                else if (data.ErrCode == "@CommonLabel.MSG_ERR_GOODS_NOT_EXISTS") {
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_GOODS_NOT_EXISTS)");
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
        //--2021/10/13 sv suzuki add end
    }

    // 2023/03/22 Kawamoto ADD START
    // 添付ファイルボタンクリック処理
    function btnAttachment_onclick() {

        document.getElementById("popup-title-attachment").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ATTACHMENT_MANAGEMENT)";
        var assetNo = document.getElementById('ASSET_NO').value

        //添付ファイル情報を取得する(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/SelectAttachment',
            data: { AssetNo: assetNo},
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {

                //添付ファイル一覧作成
                CreateAttachmentList(data);
                //件数
                document.getElementById("attachment-count").innerHTML = "@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_ALL)" + data.length + "@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)";
                //資産番号
                document.getElementById("AttachmentAssetNo").value = assetNo;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });

        //添付ファイル管理画面を表示
        ShowAttachmentModalWindow()
    }

    // 添付ファイル一覧作成処理
    function CreateAttachmentList(data) {

        var tblTbody = document.getElementById('attachment-list-detail');
        tblTbody.innerHTML = "";

        for (var i = 0; i < data.length; i++) {
            var detail = '<tr>';
            detail += '<td class="col1" ><input type="checkbox" name="checkEdit" value="' + data[i].FileNm + '"/></td>';
            detail += '<td class="col2" title="' + data[i].FileNm + '"><a href="' + data[i].FileDownloadLink + '" target="_blank" rel="noopener noreferrer">' + data[i].FileNm + '</a></td>';
            detail += '<td class="col3" title="Download"><a href="' + data[i].FileDownloadLink + '" download><img src="' + '@Url.Content("~/Images/download.png")' + '" alt="Download" /></a></td>';
            detail += '<td class="col4" title="' + data[i].ByteCount + '">' + data[i].ByteCount + '</td>';
            detail += '<td class="col5" title="' + data[i].FileUpdateTime + '">' + data[i].FileUpdateTime + '</td>';
            detail += '</tr>';
            $("#attachment-list-detail").append(detail);
        }
        for (var i = 0; i < @AttachmentManagement.DISP_ROW_COUNT - data.length; i++) {
            var detail = '<tr>';
            detail += '<td class="col1" ><input type="checkbox" name="checkEdit" disabled/></td>';
            detail += '<td class="col2"></td>';
            detail += '<td class="col3"></td>';
            detail += '<td class="col4"></td>';
            detail += '<td class="col5"></td>';
            detail += '</tr>';
            $("#attachment-list-detail").append(detail);
        }
    }

    // モーダル画面(添付ファイル管理)　追加ボタンクリック処理
    function btnAddAttachment_onclick() {

        document.getElementById("filedrop-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_FILE_SELECTION)".replace("@@@@CR@@@@", "<BR>");
        document.getElementById('SelectedFiles').value = "";

        //ファイル選択画面を表示
        ShowFiledropModalWindow();
    }

    //ロケ読取ボタンクリック処理
    function btnLocationRead_onclick() {
        //イベントの削除
        var locQr = document.getElementById("TxtLocationQr");
        locQr.removeEventListener('keydown',locQrTexthandleKeyDown);
        document.getElementById("lblOrderReadLocation").style.color = "black";
        //ラベルに初期文言を表示
        document.getElementById('lblOrderReadLocation').innerHTML = "@Message.getMessage(CommonLabelSetting.MSG_READ_LOCATION_QR)";
        //ロケQR読取画面を表示
        ShowReadLocationModalWindow();
        //テキストの入力イベントを追加
        txtLocationQr_Keydown();
        // ページが完全に読み込まれた後にフォーカスを当てる

    }

    function txtLocationQr_Keydown() {
        // 入力イベントのリスナーを追加
        var locQr = document.getElementById("TxtLocationQr");
        if (locQr) {
            locQr.addEventListener('keydown',locQrTexthandleKeyDown);
        }
    }

    var locQrTexthandleKeyDown = function(event) {

        if (!event) {
            console.error('Event is not passed correctly');
            return;
        }

        // Enterキーが押された場合
        if (event.key === 'Enter') {
            // 任意のメソッドを呼び出す
            locSelect();
        }
    }

    function locSelect () {
        // イベントが発生したときの処理

        var dropdownL = document.getElementById('LOC_L_CD');
        var dropdownM = document.getElementById('LOC_M_CD');
        var dropdownS = document.getElementById('LOC_S_CD');

        //ドロップダウンリストの変更イベントを削除
        dropdownL.onchange = "";
        dropdownM.onchange = "";

        lineLocQr = document.getElementById("TxtLocationQr").value;
        if (lineLocQr.length == @itemlength.LocQr) {
            //読み取ったのがロケQRの場合存在チェック
            var locLCd = lineLocQr.slice(0, @itemlength.LocLCd).trim();
            var locMCd = lineLocQr.slice(@itemlength.LocLCd, @itemlength.LocLCd + @itemlength.LocMCd).trim();
            var locSCd = lineLocQr.slice(@itemlength.LocLCd + @itemlength.LocMCd, @itemlength.LocLCd + @itemlength.LocMCd + @itemlength.LocSCd).trim();
            //入力されたロケQRをクリア
            document.getElementById("TxtLocationQr").value = "";
            //非同期でロケーション存在チェックをする
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/AssetList")/CheckLocation',
                dataType: 'json',
                cache: false,
                data: { LocLCd: locLCd ,LocMCd: locMCd ,LocSCd: locSCd},
                success: function (data, dataType) {
                    if (!data) {
                        document.getElementById("lblOrderReadLocation").style.color = "red";
                        document.getElementById("lblOrderReadLocation").innerHTML = "@Message.getMessage(CommonLabelSetting.MSG_LOCATION_NOT_REGISTERE)";
                        return;
                    }

                    dropdownL.value = locLCd;
                    dropdownM.disabled = false;

                    //非同期で中ロケのドロップダウンリストを取得する
                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/AssetList")/GetLocationMData',
                        dataType: 'json',
                        cache: false,
                        data: { selectLCode: locLCd},
                        success: function (data, dataType) {
                            dropdownM.innerHTML = ""
                            let opBlank = document.createElement("option");
                            opBlank.value = "";
                            opBlank.text = "";
                            dropdownM.appendChild(opBlank);

                            for (var i = 0; i < data.length; i++) {
                                let op = document.createElement("option");
                                op.value = data[i].LOC_CD;
                                op.text = data[i].LOC_NAME;
                                dropdownM.appendChild(op);
                            }
                            dropdownM.value = locMCd;

                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            // リクエスト失敗時の処理
                            console.error("AJAX request failed");
                            console.error("Status: " + textStatus);
                            console.error("Error: " + errorThrown);
                        }
                    })

                    //非同期で小ロケのドロップダウンリストを取得する
                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/AssetList")/GetLocationSData',
                        dataType: 'json',
                        cache: false,
                        data: { selectLCode: locLCd ,selectMCode: locMCd},
                        success: function (data, dataType) {
                            dropdownS.innerHTML = ""
                            let opBlank = document.createElement("option");
                            opBlank.value = "";
                            opBlank.text = "";
                            dropdownS.appendChild(opBlank);

                            dropdownS.disabled = true;
                            //中ロケがブランクを選択していなければドロップダウンリストを活性
                            if (dropdownM.value != "") {
                                dropdownS.disabled = false;
                            }

                            for (var i = 0; i < data.length; i++) {
                                let op = document.createElement("option");
                                op.value = data[i].LOC_CD;
                                op.text = data[i].LOC_NAME;
                                dropdownS.appendChild(op);
                            }

                            dropdownS.value = locSCd;

                            document.getElementById("location-modal-close").click();
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            // リクエスト失敗時の処理
                            console.error("AJAX request failed");
                            console.error("Status: " + textStatus);
                            console.error("Error: " + errorThrown);
                        }
                    })

                },
                error: function (jqXHR, textStatus, errorThrown) {
                    // リクエスト失敗時の処理
                    console.error("AJAX request failed");
                    console.error("Status: " + textStatus);
                    console.error("Error: " + errorThrown);
                }
            });
        }else{
            //読み取ったのがロケQRでない場合
            document.getElementById("lblOrderReadLocation").style.color = "red";
            document.getElementById("lblOrderReadLocation").innerHTML = "@Message.getMessage(CommonLabelSetting.MSG_NOT_LOCATION)";
            //入力されたロケQRをクリア
            document.getElementById("TxtLocationQr").value = "";
            return;
        }

    }

    // モーダル画面(添付ファイル管理)　削除ボタンクリック処理
    function btnDeleteAttachment_onclick() {

        var keys = [];
        var grid = document.getElementById("attachment-list-detail");
        var checkBoxes = grid.getElementsByTagName("INPUT");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                keys.push(checkBoxes[i].value);
            }
        }
        if (keys.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)");
            return false;
        }

        // ダイアログのメッセージを設定
        $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.SELECT_FILE_DELETE_CONFIRM)");

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    var assetNo = document.getElementById('AttachmentAssetNo').value

                    //添付ファイルを削除する(Ajax非同期通信)
                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/AssetList")/DeleteAttachment',
                        data: { AssetNo: assetNo, DelFiles:keys},
                        dataType: 'json',
                        cache: false,
                        success: function (data, dataType) {
                            this;

                            //添付ファイル一覧作成
                            CreateAttachmentList(data);
                            //件数
                            document.getElementById("attachment-count").innerHTML = "@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_ALL)" + data.length + "@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)";
                            document.getElementById("btnAttachment").value = "@Message.getMessage(CommonLabel.BUTTON_ATTACHMENT)".replace("[0]", "[" + data.length + "]")
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            this;
                        }
                    });
                    $(this).dialog("close");
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

    // モーダル画面(ファイル選択)　ファイルが選択された場合の処理
    function FileOnSelect() {

        var fileList = document.getElementById('SelectedFiles').files;
        var assetNo = document.getElementById('AttachmentAssetNo').value

        // 各ファイルのサイズ(KB)をチェック  20230430 Sato v2.4 ADD
        for (let i = 0; i < fileList.length; i++) {
            if (Math.floor(fileList[i].size / 1024) > (@AttachFileMaxSize * 1024)) {
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_FILE_MAX_SIZE_OVER)".format("@AttachFileMaxSize"))
                return;
            }
        }

        var $form = $(this).closest('form')
        var formData = new FormData($form.get(0));
        formData.append("AssetNo", assetNo);
        for (var i = 0; i < fileList.length; i++) {
            formData.append("SelectedFiles", fileList[i]);
        }

        //添付ファイルを追加する(Ajax非同期通信)
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetList")/AddAttachment',
            data: formData,
            dataType: 'json',
            processData: false,
            contentType: false,
            cache: false,
            success: function (data, dataType) {
                this;

                //添付ファイル一覧作成
                CreateAttachmentList(data);
                //件数
                document.getElementById("attachment-count").innerHTML = "@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_ALL)" + data.length + "@Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)";
                document.getElementById("btnAttachment").value = "@Message.getMessage(CommonLabel.BUTTON_ATTACHMENT)".replace("[0]", "[" + data.length + "]")

                // モーダルダイアログ(ファイル選択) 終了処理
                //[#modal-filedrop-content]をフェードアウトした後
                $("#modal-filedrop-content").fadeOut("slow", function () {

                    //[#modal-overlay]の位置指定を変更
                    $("#modal-overlay").css({ "z-index": 2 });
                });
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //ロケーションQR読取画面の閉じるボタンクリック処理
    function btnDeleteLocation_onclick() {
        //読取のイベントを削除
        var locQr = document.getElementById("TxtLocationQr");
        var dropdownL = document.getElementById('LOC_L_CD');
        var dropdownM = document.getElementById('LOC_M_CD');

        locQr.removeEventListener('keydown', locQrTexthandleKeyDown());

        //ドロップダウンリストのイベントを復活
        dropdownL.onchange = function () { ChangeLocationL(this) };
        dropdownM.onchange = function () { ChangeLocationM(this) };
    }

    // モーダル画面(ファイル選択)　ドラッグ&ドロップでファイルのアップロードを行う
    window.addEventListener("DOMContentLoaded", () => {

        const ddarea = document.getElementById("modal-filedrop-content");
        const fileInput = document.getElementById('SelectedFiles');

        // ドラッグされたデータが有効かどうかチェック
        const isValid = e => e.dataTransfer.types.indexOf("Files") >= 0;

        const ddEvent = {
            "dragover": e => {
                e.preventDefault(); // 既定の処理をさせない
                if (!e.currentTarget.isEqualNode(ddarea)) {
                    // ドロップエリア外ならドロップを無効にする
                    e.dataTransfer.dropEffect = "none"; return;
                }
                e.stopPropagation(); // イベント伝播を止める

                if (!isValid(e)) {
                    // 無効なデータがドラッグされたらドロップを無効にする
                    e.dataTransfer.dropEffect = "none"; return;
                }
                // ドロップのタイプを変更
                e.dataTransfer.dropEffect = "copy";
                ddarea.classList.add("ddefect");
            },
            "dragleave": e => {
                if (!e.currentTarget.isEqualNode(ddarea)) {
                    return;
                }
                e.stopPropagation();    // イベント伝播を止める
                ddarea.classList.remove("ddefect");
            },
            "drop": e => {
                e.preventDefault();     // 既定の処理をさせない
                e.stopPropagation();    // イベント伝播を止める

                const files = e.dataTransfer.files;

                // 各ファイルのサイズ(KB)をチェック  20230430 Sato v2.4 ADD
                for (let i = 0; i < files.length; i++) {
                    if (Math.floor(files[i].size / 1024) > (@AttachFileMaxSize * 1024)) {
                        showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_FILE_MAX_SIZE_OVER)".format("@AttachFileMaxSize"))
                        return;
                    }
                }

                // 取得したファイルをinput[type=file]へ
                fileInput.files = files;

                //ファイルが正常に受け取れた場合は以下の処理を行う
                if (typeof files[0] !== 'undefined') {
                    FileOnSelect()
                }

                ddarea.classList.remove("ddefect");
            }
        };

        Object.keys(ddEvent).forEach(e => {
            ddarea.addEventListener(e, ddEvent[e]);
            document.body.addEventListener(e, ddEvent[e])
        });

    });
    // 2023/03/22 Kawamoto ADD END

    // 2023/05/10 Kawamoto ADD START
    // FreeItem15(計測器)選択
    function ChangeFreeItem15List(WeighingMachine) {

        // 更新処理の場合のみ
        if (document.getElementById("modal-create-btn").value == "@Message.getMessage(CommonLabel.BUTTON_CHANGE)") {

            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/AssetList")/GetWeighingMachine',
                cache: false,
                data: {},
                success: function (data, dataType) {
                    this;

                    // 計測器連携ありの計測器の場合
                    if (data != "" && WeighingMachine.value != "" && data.includes(WeighingMachine.value)) {
                        // 使用不可
                        document.getElementById("FREE_ITEM16").readOnly = true;
                        $("#FREE_ITEM16").addClass("disable");
                        document.getElementById("FREE_ITEM16").value = document.getElementById("RemainingAmount").value;

                    } else {
                        // 使用可
                        document.getElementById("FREE_ITEM16").readOnly = false;
                        $("#FREE_ITEM16").removeClass("disable");
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }
    }
    // 2023/05/10 Kawamoto ADD END


</script>

<div id="asset-list">
    @Using (Html.BeginForm("Index", "AssetList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    <div>
                        <input class="defButton update-btn" type="button" id="btnPrnQRDaicho" value="@Message.getMessage(CommonLabel.BUTTON_PRINT_QR_DAICHO)" onclick="btnQRDaichoExport_onclick()">
                        <input class="defButton update-btn" type="button" id="btnShowQR" value="@Message.getMessage(CommonLabel.BUTTON_SHOW_QR)" onclick="btnQR_onclick()">
                        <input id="csvBtn" class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnIchiranCsvExport_onclick()">
                        @* 2020/02/12 nv miyaji　全選択/全解除ボタンの表記文字設定 *@
                        @Code
                            Dim AllSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG)
                            Dim sDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                            If AllSelectFlg Then
                                sDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                            End If
                        End Code
                        <input class="defButton" type="button" id="btnSelectAll" value="@sDispStr" onclick="btnSelectAll_onclick()">
                        @*<input class="defButton" type="button" id="btnSelectAll" value="@Message.getMessage(CommonLabel.BUTTON_ALL_ON)" onclick="btnSelectAll_onclick()">*@
                        @* --2021/10/01 nv miyaji update start *@
                        @If ViewBag.AuthProcFlg = AuthProcFlg.All Or ViewBag.AuthProcFlg = AuthProcFlg.JiAll Then
                        @<input Class="defButton create-btn" type="button" id="btnCreate" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">
                        End If
                        @*<input Class="defButton create-btn" type="button" id="btnCreate" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="btnCreate_onclick()">*@
                        @* --2021/10/01 nv miyaji update end *@
                        @If ViewBag.ExistRFIDLoc AndAlso ViewBag.AuthProcFlg <> AuthProcFlg.ShokaiNomi Then
                        @<input Class="defButton update-btn" type="button" id="btnPrintTag" value="@Message.getMessage(CommonLabel.BUTTON_LABEL_PRINT)" onclick="btnTagLabel_onclick()">
                        End If
                    </div>
                </div>
                <div Class="view-area-button-right">
                        @* --2021/10/01 nv miyaji update start *@
                        @If ViewBag.CsvImportDispFlg = True Then
                        @<input Class="defButton csv-import-btn" type="button" id="btnExcelImport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_ASSET_LEDGER_IMPORT)" onclick="btnExcelImport_onclick()">
                    End If
                        @*<input Class="defButton csv-import-btn" type="button" id="btnCsvImport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="btnCsvImport_onclick()">*@
                        @* --2021/10/01 nv miyaji update end *@
                    <input Class="defButton csv-Export-btn  submit-button" type="button" id="btnCsvExport" name="CsvExport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL)" onclick="btnCsvExport_onclick()">
                        @*戻るボタンの初期表示を非表示にする '2019/11/11 nv miyaji*@
                    <input class="defButton" type="button" id="btnReturn" name="Return" style="display:none;" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "StockList")'">
                </div>
                @*<div class="view-area-label-assetlist" id="lblMiTagHimoduke" style="color:red;font-size:14px;">
                        @ViewBag.MiTagHimoduke
                </div>*@

            </div>
        </div>
                        @* 検索条件 *@
                        @<div class="search-wrap">
                            <a id="SearchCondition" data-toggle="collapse" href="#cond" aria-expanded="true" onclick="SearchCondition_onclick()">
                                @Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)
                            </a>
                            <div class="search-border">
                                <div id="cond" class="container collapse in">
                                    <div class="row" id="item-conditions">
                                        @* condition-flex：複数選択で項目の高さが伸びても段が崩れないよう flex で並べる（Content\Site.css） *@
                                        <div class="col-md-12 condition-flex">
                                            @* ロケーション大区分 *@
                                            <div class="col-sm-2 filter-combo-area" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L)<font Class="label-require">※</font></div>
                                                <div class="grid-condition-col">
                                                    @* select2-clearable クラスで入力して絞り込めるSelect2になる（Scripts\select2-common.js）。選択肢・選択後とも「コード 名称」を表示 *@
                                                    @* ×で選択解除するため、空の選択肢を追加している *@
                                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocL, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control select2-clearable", .onchange = "ChangeLocationL(this)"})
                                                </div>
                                            </div>
                                            @* ロケーション中区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                                            <div class="col-sm-2 combo-area" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_M)</div>
                                                <div class="grid-condition-col">
                                                    @* ListBoxFor で複数選択（select multiple）。select2-clearable クラスで×ボタン付きSelect2になる。空の選択肢は不要 *@
                                                    @*[CC431] UPD Start 20261007*@
                                                    @* 選択値を変えたら、場所から選ぶ OFF のとき場所の選択肢を作り直す *@
                                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchLocM, DirectCast(ViewBag.modelLocM, IEnumerable(Of SelectListItem)), New With {.class = "cond-control select2-clearable", .onchange = "onSearchLocChange('Loc', '" & LOC_DIV.LocationM & "')"})
                                                    @*[CC431] UPD End 20261007*@
                                                </div>
                                            </div>
                                            @* ロケーション小区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                                            <div class="col-sm-2 combo-area" style="display: block">
                                                @*[CC431] UPD Start 20261007*@
                                                <div class="grid-head-col">
                                                    @Message.getMessage(CommonLabel.LABEL_LOC_S)
                                                    @* 「場所から選ぶ」チェックボックス（ON：場所で原価センタの選択肢を絞り込む）。中区分・小区分の両方を使用する場合だけ表示 *@
                                                    @* head-check：見出し右端の緑のチェックボックス（Content\Site.css） *@
                                                    @If ViewBag.LocUsage_M = Misc.ItemUse.Yes AndAlso ViewBag.LocUsage_S = Misc.ItemUse.Yes Then
                                                        @<label class="head-check">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLocReverse, New With {.onchange = "onSearchLocReverseChange('Loc')"})@Message.getMessage(CommonLabel.LABEL_SELECT_FROM_PLACE)</label>
                                                    End If
                                                </div>
                                                <div class="grid-condition-col">
                                                    @* ListBoxFor で複数選択（select multiple）。選択値を変えたら、場所から選ぶ ON のとき原価センタの選択肢を作り直す *@
                                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchLocS, DirectCast(ViewBag.modelLocS, IEnumerable(Of SelectListItem)), New With {.class = "cond-control select2-clearable", .onchange = "onSearchLocChange('Loc', '" & LOC_DIV.LocationS & "')"})
                                                </div>
                                                @*[CC431] UPD End 20261007*@
                                            </div>
                                            @* カテゴリー大分類（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                                            <div class="col-sm-2 combo-area" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                                                <div class="grid-condition-col">
                                                    @* ListBoxFor で複数選択（select multiple）。検索条件では中分類以下の絞り込みがないため onchange は不要 *@
                                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), New With {.class = "cond-control select2-clearable"})
                                                </div>
                                            </div>

                                            @* 資産番号 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NO)</div>
                                                <div class="grid-condition-col-multi">
                                                    @* 「|」区切りで複数入力可。×でクリア、複数ボタンで複数入力ダイアログを表示（Scripts\multi-input-dialog.js） *@
                                                    <span class="clearable">
                                                        @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetNo, New With {.class = "cond-control", .oninput = "editAssetNoMultiInput(this)"})
                                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                                    </span>
                                                    @*[CC431] UPD Start 20261013 複数入力ダイアログの文字数（24）・文字種（editAssetNo）の制限を追加*@
                                                    <button type="button" class="btn btn-default multi-input-button" data-multi-input="#SearchAssetNo" data-multi-title="@Message.getMessage(CommonLabel.LABEL_ASSET_NO)" data-multi-maxlength="24" data-multi-edit="editAssetNo">@Message.getMessage(CommonLabel.BUTTON_MULTI_INPUT)</button>
                                                    @*[CC431] UPD End 20261013*@
                                                </div>
                                            </div>
                                            @* 資産名称 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetName, New With {.class = "cond-control", .maxlength = 60})
                                                </div>
                                            </div>
                                            @* 有形・無形区分 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)</div>
                                                <div class="grid-condition-col-asset">
                                                    @Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchIsTangibleY, New With {.style = "margin-right:5px;"})
                                                    @Message.getMessage(CommonLabel.VALUE_TANGIBLE_ASSET)
                                                    @Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchIsTangibleN, New With {.style = "margin-left:15px; margin-right:5px;"})
                                                    @Message.getMessage(CommonLabel.VALUE_INTANGIBLE_ASSET)
                                                </div>
                                            </div>
                                            @*[CC431] DEL 20261019 詳細検索欄へ移動（1 項目。詳細検索欄の先頭に追加）*@
                                            @* 資本化日付 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)</div>
                                                <div class="grid-condition-col-asset">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem2From, New With {.class = "cond-control", .type = "date"})
                                                    ～
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem2To, New With {.class = "cond-control", .type = "date"})
                                                </div>
                                            </div>
                                            @* 稼働区分名 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM4)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem4, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* 型式 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM5)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem5, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* 管理社員番号 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6_CD)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem6, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* シリアルNo *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM7)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem7, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @*[CC431] DEL 20261019 詳細検索欄へ移動（1 項目。詳細検索欄の先頭に追加）*@
                                            @* 備考 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM9)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem9, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* 取得金額 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM11)</div>
                                                <div class="grid-condition-col-asset">
                                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem11From, New With {.htmlAttributes = New With {.max = "999999999999999999999.99", .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                                    @Html.Raw("～")
                                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem11To, New With {.htmlAttributes = New With {.max = "999999999999999999999.99", .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                                </div>
                                            </div>
                                            @* 当月末簿価 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM12)</div>
                                                <div class="grid-condition-col-asset">
                                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem12From, New With {.htmlAttributes = New With {.max = "999999999999999999999.99", .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                                    @Html.Raw("～")
                                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem12To, New With {.htmlAttributes = New With {.max = "999999999999999999999.99", .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                                </div>
                                            </div>
                                            @* シリアル番号 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM14)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem14, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* 資産グループコード *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem15, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @*[CC431] DEL 20261019 詳細検索欄へ移動（9 項目。詳細検索欄の先頭に追加）*@
                                            @* タグ紐付状態 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TAG_LINK_STATUS)</div>
                                                <div class="grid-condition-col-multi">
                                                    @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                                                    @* 選択肢の「全て」の値が空のため、選択解除用の空の選択肢は追加しない（×で「全て」に戻る。「全て」選択中は×を表示しない） *@
                                                    <span class="clearable">
                                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchTagLinkStatus, DirectCast(ViewBag.TagLinkStatusList, IEnumerable(Of SelectListItem)), New With {.class = "cond-control"})
                                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                                    </span>
                                                </div>
                                            </div>
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM27)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem27, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row" id="item-sort">
                                        <div class="col-md-12">
                                            <div class="grid-head-col sort-head-col">@Message.getMessage(CommonLabel.LABEL_SORT_ORDER)</div>
                                            <div id="search-sort" class="grid-condition-col sort-condition-col">
                                                @Code
                                                    Dim sSortAsc As String = Message.getMessage(CommonLabel.LABEL_SORT_ASC)
                                                    Dim lstSort As New List(Of KeyValuePair(Of Integer, String)) From {
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.CatLAsc, Message.getMessage(CommonLabel.LABEL_CAT_L)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.AssetNoAsc, Message.getMessage(CommonLabel.LABEL_ASSET_NO)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.LocMAsc, Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_CODE)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.LocSAsc, Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_CODE)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.FreeItem2Asc, Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.FreeItem6Asc, Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6_CD)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark1Asc, Message.getMessage(CommonLabel.LABEL_REMARK1)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark2Asc, Message.getMessage(CommonLabel.LABEL_REMARK2)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark3Asc, Message.getMessage(CommonLabel.LABEL_REMARK3)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark4Asc, Message.getMessage(CommonLabel.LABEL_REMARK4)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark5Asc, Message.getMessage(CommonLabel.LABEL_REMARK5)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark6Asc, Message.getMessage(CommonLabel.LABEL_REMARK6)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark7Asc, Message.getMessage(CommonLabel.LABEL_REMARK7)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark8Asc, Message.getMessage(CommonLabel.LABEL_REMARK8)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark9Asc, Message.getMessage(CommonLabel.LABEL_REMARK9)),
                                                        New KeyValuePair(Of Integer, String)(AssetDispSelect.Remark10Asc, Message.getMessage(CommonLabel.LABEL_REMARK10))
                                                    }
                                                End Code
                                                @For Each kvSort As KeyValuePair(Of Integer, String) In lstSort
                                                    @<div class="input-select-item">
                                                        @Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelect, kvSort.Key)@(kvSort.Value & sSortAsc)
                                                    </div>
                                                Next
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col-md-12">
                                            <a href="#" id="DetailAccordion" onclick="DetailAccordion_onclick();return false;">@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)</a>
                                        </div>
                                    </div>
                                    <div class="row detail-accordion" id="item-conditions-detail" style="display:none;">
                                        <div class="col-md-12 condition-flex">
                                            @*[CC431] ADD Start 20261019*@
                                            @* 検索条件欄から移動した項目（仕様変更6。★検索条件一覧.xlsx の詳細検索「〇」） *@
                                            @* 貸与資産状態区分 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM10)</div>
                                                <div class="grid-condition-col-multi">
                                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                                    <span class="clearable">
                                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchFreeItem10, DirectCast(ViewBag.FreeItem10List, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                                    </span>
                                                </div>
                                            </div>
                                            @* WBS番号 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM8)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem8, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* 貸与先 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM16)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem16, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* 貸与先名 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM17)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem17, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* 開始日(貸与) *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)</div>
                                                <div class="grid-condition-col-asset">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem18From, New With {.class = "cond-control", .type = "date"})
                                                    ～
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem18To, New With {.class = "cond-control", .type = "date"})
                                                </div>
                                            </div>
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)</div>
                                                <div class="grid-condition-col-asset">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem19From, New With {.class = "cond-control", .type = "date"})
                                                    ～
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem19To, New With {.class = "cond-control", .type = "date"})
                                                </div>
                                            </div>
                                            @* 貸与回数 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM20)</div>
                                                <div class="grid-condition-col-asset">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem20From, New With {.class = "cond-control", .oninput = "editNumber(this)", .style = "text-align: right;", .maxlength = 2})
                                                    ～
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem20To, New With {.class = "cond-control", .oninput = "editNumber(this)", .style = "text-align: right;", .maxlength = 2})
                                                </div>
                                            </div>
                                            @* 備考欄 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM21)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem21, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* 市区町村コード名称 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM39)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem39, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* CAPS番号 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM42)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem42, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @* 資産リース区分 *@
                                            <div class="col-sm-2" style="display: block">
                                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM37)</div>
                                                <div class="grid-condition-col">
                                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem37, New With {.class = "cond-control", .maxlength = 255})
                                                </div>
                                            </div>
                                            @*[CC431] ADD End 20261019*@
                                            @*[CC431] ADD Start 20261019*@
                                            @* 備考1～備考10を別の行にする（flex の折り返し。幅 100% の高さ 0 の要素） *@
                                            <div style="flex-basis:100%; height:0;"></div>
                                            @*[CC431] ADD End 20261019*@
                                            @Code
                                                Dim lstRemark As New List(Of KeyValuePair(Of String, String)) From {
                                                    New KeyValuePair(Of String, String)("SearchRemark1", Message.getMessage(CommonLabel.LABEL_REMARK1)),
                                                    New KeyValuePair(Of String, String)("SearchRemark2", Message.getMessage(CommonLabel.LABEL_REMARK2)),
                                                    New KeyValuePair(Of String, String)("SearchRemark3", Message.getMessage(CommonLabel.LABEL_REMARK3)),
                                                    New KeyValuePair(Of String, String)("SearchRemark4", Message.getMessage(CommonLabel.LABEL_REMARK4)),
                                                    New KeyValuePair(Of String, String)("SearchRemark5", Message.getMessage(CommonLabel.LABEL_REMARK5)),
                                                    New KeyValuePair(Of String, String)("SearchRemark6", Message.getMessage(CommonLabel.LABEL_REMARK6)),
                                                    New KeyValuePair(Of String, String)("SearchRemark7", Message.getMessage(CommonLabel.LABEL_REMARK7)),
                                                    New KeyValuePair(Of String, String)("SearchRemark8", Message.getMessage(CommonLabel.LABEL_REMARK8)),
                                                    New KeyValuePair(Of String, String)("SearchRemark9", Message.getMessage(CommonLabel.LABEL_REMARK9)),
                                                    New KeyValuePair(Of String, String)("SearchRemark10", Message.getMessage(CommonLabel.LABEL_REMARK10))
                                                }
                                                Dim dicRemarkValue As New Dictionary(Of String, String) From {
                                                    {"SearchRemark1", Model.FirstOrDefault.SearchRemark1},
                                                    {"SearchRemark2", Model.FirstOrDefault.SearchRemark2},
                                                    {"SearchRemark3", Model.FirstOrDefault.SearchRemark3},
                                                    {"SearchRemark4", Model.FirstOrDefault.SearchRemark4},
                                                    {"SearchRemark5", Model.FirstOrDefault.SearchRemark5},
                                                    {"SearchRemark6", Model.FirstOrDefault.SearchRemark6},
                                                    {"SearchRemark7", Model.FirstOrDefault.SearchRemark7},
                                                    {"SearchRemark8", Model.FirstOrDefault.SearchRemark8},
                                                    {"SearchRemark9", Model.FirstOrDefault.SearchRemark9},
                                                    {"SearchRemark10", Model.FirstOrDefault.SearchRemark10}
                                                }
                                            End Code
                                            @For Each kvRemark As KeyValuePair(Of String, String) In lstRemark
                                                @<div class="col-sm-2" style="display: block">
                                                    <div class="grid-head-col">@kvRemark.Value</div>
                                                    <div class="grid-condition-col-multi">
                                                        <span class="clearable">
                                                            @Html.TextBox(kvRemark.Key, dicRemarkValue(kvRemark.Key), New With {.class = "cond-control"})
                                                            <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                                        </span>
                                                        @*[CC431] UPD Start 20261013 複数入力ダイアログの文字数（255）の制限を追加*@
                                                        <button type="button" class="btn btn-default multi-input-button" data-multi-input="#@kvRemark.Key" data-multi-title="@kvRemark.Value" data-multi-maxlength="255">@Message.getMessage(CommonLabel.BUTTON_MULTI_INPUT)</button>
                                                        @*[CC431] UPD End 20261013*@
                                                    </div>
                                                </div>
                                            Next
                                        </div>
                                    </div>
                                    <div class="view-area-label" style="height: 25px"></div>
                                    <input class="float-right defButton submit-button" id="Search_Button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                                </div>
                            </div>
                        </div>

                        @<input type="hidden" id="IsRestoreOwnSearchConditions" name="IsRestoreOwnSearchConditions" value="@Model.FirstOrDefault().IsRestoreOwnSearchConditions.ToString()" />
                        @<input type="hidden" id="FirstSearch" name="FirstSearch" value="@Model.FirstOrDefault().FirstSearch.ToString()" />

                        @*ページャー表示*@
                        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
                        @*@Html.Label("選択件数：", New With {.style = "font-size:12px;"}) yamada*@
                        @Html.Label(Message.getMessage(CommonLabel.LABEL_SELECT_COUNT), New With {.style = "font-size:12px;"})
                        @Code
            Dim lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
            Dim CheckedCount As String = "0"
            If Not IsNothing(lstCheckedKey) Then
                CheckedCount = lstCheckedKey.Count.ToString
            End If
                            @Html.Label(CheckedCount, New With {.id = "selectcount", .style = "margin-right:15px;font-size:12px;"})
                        End Code

                        @<div class="list-view">
                            <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
                        </div>
        'チェックボックス選択値の引き渡し用隠し項目
                        @<input id="SelectKeys" name="SelectKeys" type="hidden">
                        @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden">
                        @*'クエリ文字列引き渡し用隠し項目
        @<input id="AssetListKey" name="AssetListKey" type="hidden">*@
        'ローディング画像引き渡し用隠し項目
                        @<input id="LoadingImg" name="LoadingImg" type="hidden">
        '履歴画面遷移時、資産番号引き渡し用各四項目
                        @<input id="SelectedAssetNo" name="SelectedAssetNo" type="hidden">


        'プリンタ選択モーダル画面
                        @<div id="modal-prt-content">
                            <div class="popup-title-wrap">
                                <p class="Line3"></p>
                                <p class="Line4"></p>
                                <h2 id="popup-title-prt"></h2>
                                <p class="Line3"></p>
                                <p class="Line4"></p>
                            </div>
                            @CODE
                'モデルの値を設定
                Dim prtSessionModel As AssetListViewModel = Session(CommonSessionKey.CREAGE_ASSET_LIST)
                If Not IsNothing(prtSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(prtSessionModel)
                Else
                    Model.Add(New AssetListViewModel())
                End If
                            END CODE
                            <br />
                            <div class="mb10">
                                @Html.DropDownListFor(Function(model) model.FirstOrDefault.PrinterKey, DirectCast(ViewBag.modelPrinter, IEnumerable(Of SelectListItem)), "", New With {.style = "width: 300px;"})
                            </div>
                            <br />
                            <div Class="PopupButton float-left">
                                <input id="modal-ok" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_OK)" onclick="btnPrinterOK_onclick()">
                            </div>
                            <div Class="PopupButton float-right">
                                <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_CANCEL)">
                            </div>
                        </div>


                '資産台帳取込のモーダル画面
                        @<div id="modal-csv-content" class="modal-assetlist">
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
                                    <input type="file" name="ExcelFile" id="ExcelFile" />
                                </div>
                                <div class="float-right">
                                    <input id="modal-import-btn" class="defButton ExcelImport" style="width:120px" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_ASSET_LEDGER_IMPORT)" onclick="btnExcellmportStart_onclick()">
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
                        @<div id="modal-content" class="modal-assetlist" style="width: 1550px;">
                            <div class="popup-title-wrap">
                                <p class="Line3"></p>
                                <p class="Line4"></p>
                                <h2 id="popup-title"></h2>
                                <p class="Line3"></p>
                                <p class="Line4"></p>
                            </div>
                            @CODE
                'モデルの値を設定
                Dim SessionModel As AssetListViewModel = Session(CommonSessionKey.CREAGE_ASSET_LIST)
                If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(SessionModel)
                Else
                    Model.Add(New AssetListViewModel())
                End If
                            END CODE
                            @Html.AntiForgeryToken()


                            <Table>
                                <tr Class="popup-row">
                                    <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_NO)</td>

                                    @If ViewBag.AssetNoSelfReg = "Y" Then
                                    @<td width="20" Class="require" style="display:inline-block;"></td>
                 Else
                                    @<td width="20" Class="require" style="display:inline-block;">※</td>
                 End If
                                    <td style="display:inline-block; width:420px;">
                                        @If ViewBag.AssetNoSelfReg = "Y" Then
                                        @If Model.FirstOrDefault.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                                        @Html.EditorFor(Function(model) model.Last.ASSET_NO, New With {.htmlAttributes = New With {.style = "width: 190px;color:#cecece;background-color:#cecece;", .readOnly = True, .maxlength = 24, .oninput = "editAssetNo(this)"}})
                         Else
                                        @Html.EditorFor(Function(model) model.Last.ASSET_NO, New With {.htmlAttributes = New With {.style = "width: 190px;background-color:#cecece;", .readOnly = True, .maxlength = 24, .oninput = "editAssetNo(this)"}})
                         End If
                     Else
                                        @Html.EditorFor(Function(model) model.Last.ASSET_NO, New With {.htmlAttributes = New With {.style = "width: 190px;", .maxlength = 24, .oninput = "editAssetNo(this)", .onblur = "assetNoOnBlur(this)"}})  @* 2022/8/31 Upd Kawamoto *@
                                        End If
                                    </td>
                                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:420px;">@Html.DropDownListFor(Function(model) model.Last.LEASE_STATUS, DirectCast(ViewBag.LeaseStatus, IEnumerable(Of SelectListItem)), New With {.style = "width: 100px;"})</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_SUB_ASSET_NO)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:420px;">
                                        @Html.EditorFor(Function(model) model.Last.SUB_ASSET_NO, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 15, .oninput = "editAssetNo(this)"}})
                                    </td>
                                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_ID)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:420px;">
                                        @Html.EditorFor(Function(model) model.Last.ASSET_ID, New With {.htmlAttributes = New With {.style = "width: 400px;", .maxlength = 128, .oninput = "editHanEi(this)"}})
                                    </td>
                                </tr>
                                <tr Class="popup-row">
                                    <td class="head-col DisplayOnOff" style="width: 230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_STATUS)</td>
                                    <td class="DisplayOnOff" width="20" style="display:inline-block;"></td>
                                    @*<td class="DisplayOnOff" style="display:inline-block; width:90px;"> yamada*@
                                    <td class="DisplayOnOff" style="display:inline-block; width:290px;">
                                        @*@Html.TextBox("ASSET_STATUS_NM", "", New With {.style = "width: 60px;"})*@
                                        @*@Html.TextBoxFor(Function(model) model.Last.AssetStatusNm, New With {.style = "width: 60px;"}) yamada*@
                                        @Html.TextBoxFor(Function(model) model.Last.AssetStatusNm, New With {.style = "width: 260px;"})
                                    </td>
                                    <td class="head-col DisplayOnOff" style="width:150px;">@Message.getMessage(CommonLabel.LABEL_ASSET_ID_STATUS)</td>
                                    <td class="DisplayOnOff" width="10" style="display:inline-block;"></td>
                                    @*<td class="DisplayOnOff" style="display:inline-block; width:90px;"> yamada*@
                                    <td class="DisplayOnOff" style="display:inline-block; width:290px;">
                                        @*@Html.TextBox("EPC_ENC_STATUS_NM", "", New With {.style = "width: 60px;"})*@
                                        @*@Html.TextBoxFor(Function(model) model.Last.EpcEncStatusNm, New With {.style = "width: 60px;"}) yamada*@
                                        @Html.TextBoxFor(Function(model) model.Last.EpcEncStatusNm, New With {.style = "width: 260px;"})
                                    </td>
                                    <td class="head-col DisplayOnOff" style="width:150px;">@Message.getMessage(CommonLabel.LABEL_TAG_LINK_STATUS)</td>
                                    <td class="DisplayOnOff" width="10" style="display:inline-block;"></td>
                                    @*<td class="DisplayOnOff" style="display:inline-block; width:90px;"> yamada*@
                                    <td class="DisplayOnOff" style="display:inline-block; width:290px;">
                                        @*@Html.TextBox("TAG_LINK_STATUS_NM", "", New With {.style = "width: 60px;"})*@
                                        @*@Html.TextBoxFor(Function(model) model.Last.TagLinkStatusNm, New With {.style = "width: 60px;"}) yamada*@
                                        @Html.TextBoxFor(Function(model) model.Last.TagLinkStatusNm, New With {.style = "width: 260px;"})
                                    </td>
                                </tr>
                            </Table>
                            <Table align="left">
                                <tr Class="popup-row">
                                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_MNG_LOC)</td>
                                    <td width="20" class="require" style="display:inline-block;">※</td>
                                    @* select2-clearable クラスで入力して絞り込めるSelect2になる（Scripts\select2-common.js）。選択肢・選択後とも「コード 名称」を表示 *@
                                    @* ×で選択解除するため、空の選択肢を追加している（必須チェックは登録時にサーバー側で行う） *@
                                    <td style="display:inline-block; width:420px;">@Html.DropDownListFor(Function(model) model.Last.MNG_LOC_L_CD, DirectCast(ViewBag.modalmodelMngLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "select2-clearable", .style = "width: 400px;"})</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_GEN_LOCATION)</td>
                                    <td class="head-col" style="width:80px;">@Message.getMessage(CommonLabel.VALUE_LOC_L_COM)</td>
                                    <td width="20" class="require" style="display:inline-block;">※</td>
                                    <td style="display:inline-block; width:420px;">@Html.DropDownListFor(Function(model) model.Last.LOC_L_CD, DirectCast(ViewBag.modalmodelLocL, IEnumerable(Of SelectListItem)), "", New With {.class = "select2-clearable", .onchange = "ChangeLocationL(this)", .style = "width: 400px;"})</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td Class="head-col">&nbsp;</td>
                                    <td class="head-col" style="width:80px;">@Message.getMessage(CommonLabel.VALUE_LOC_M_DEP)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:420px;">@Html.DropDownListFor(Function(model) model.Last.LOC_M_CD, DirectCast(ViewBag.modalmodelLocM, IEnumerable(Of SelectListItem)), "", New With {.class = "text-box-name select2-clearable", .onchange = "ChangeLocationM(this)", .style = "width: 400px;"})</td>
                                </tr>
                                <tr class="popup-row">
                                    <td class="head-col">&nbsp;</td>
                                    <td class="head-col" style="width:80px;">@Message.getMessage(CommonLabel.VALUE_LOC_S_PLA)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td width="420px" style="display:inline-block;">@Html.DropDownListFor(Function(model) model.Last.LOC_S_CD, DirectCast(ViewBag.modalmodelLocS, IEnumerable(Of SelectListItem)), "", New With {.class = "text-box-name select2-clearable", .style = "width: 400px;"})</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_DIV)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:420px;">
                                        @Html.EditorFor(Function(model) model.Last.ASSET_DIV, New With {.htmlAttributes = New With {.class = "text-box-name", .maxlength = 60, .style = "width: 400px;"}})
                                    </td>
                                </tr>
                                <tr Class="popup-row">
                                    <td Class="head-col" style="width:230px;">
                                        @Message.getMessage(CommonLabel.LABEL_ASSET_NAME)
                                        <font color="#ff0000">
                                            @If ViewBag.DrugsFunc = True Then
                                            @Message.getMessage(CommonLabel.LABEL_REQUIRED_INPUT)
                            End If
                                        </font>
                                    </td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:420px;">
                                        @Html.EditorFor(Function(model) model.Last.ASSET_NAME, New With {.htmlAttributes = New With {.class = "text-box-name", .maxlength = 60, .style = "width: 400px;"}})
                                    </td>
                                </tr>
                                <tr Class="popup-row">
                                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_CATEGORY)</td>
                                    <td class="head-col" style="width:80px;">@Message.getMessage(CommonLabel.VALUE_CAT_L)</td>
                                    <td width="20" class="require" style="display:inline-block;">※</td>
                                    <td style="display:inline-block; width:420px;">@Html.DropDownListFor(Function(model) model.Last.CAT_L_CD, DirectCast(ViewBag.modalmodelCatL, IEnumerable(Of SelectListItem)), "", New With {.class = "text-box-name select2-clearable", .onchange = "ChangeCategoryL(this)", .style = "width: 400px;"})</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td Class="head-col">&nbsp;</td>
                                    <td class="head-col" style="width:80px;">@Message.getMessage(CommonLabel.VALUE_CAT_M)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:420px;">@Html.DropDownListFor(Function(model) model.Last.CAT_M_CD, DirectCast(ViewBag.modalmodelCatM, IEnumerable(Of SelectListItem)), "", New With {.class = "text-box-name select2-clearable", .onchange = "ChangeCategoryM(this)", .style = "width: 400px;"})</td>
                                </tr>
                                <tr class="popup-row">
                                    <td class="head-col">&nbsp;</td>
                                    <td class="head-col" style="width:80px;">@Message.getMessage(CommonLabel.VALUE_CAT_S)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td width="420px" style="display:inline-block;">@Html.DropDownListFor(Function(model) model.Last.CAT_S_CD, DirectCast(ViewBag.modalmodelCatS, IEnumerable(Of SelectListItem)), "", New With {.class = "text-box-name select2-clearable", .style = "width:400px;"})</td>
                                </tr>
                            </Table>

                            <div style="height:290px; width:50%; overflow-y:scroll;">
                                <table overflow-y:scroll;>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM1)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM1, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM2, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM3)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM3, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM4)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM4, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM5)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM5, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6_CD)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM6, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM7)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM7, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM8)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM8, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM9)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM9, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM10)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM10, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM11)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM11, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM12)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM12, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM13)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM13, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM14)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM14, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM15, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM16)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM16, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM17)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM17, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM18, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM19, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM20)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM20, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM21)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM21, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM22)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM22, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM23)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM23, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM24)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM24, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM25)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM25, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM26)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM26, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM27)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM27, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM28)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM28, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM29)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM29, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM30)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM30, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM31)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM31, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM32)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM32, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM33)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM33, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM34)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM34, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM35)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM35, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM36)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM36, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM37)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM37, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM38)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM38, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM39)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM39, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM40)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM40, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM41)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM41, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM42)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM42, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM43)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM43, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM44)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM44, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM45)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM45, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM46)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM46, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM47)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM47, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM48)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM48, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM49)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM49, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM50)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM50, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM51)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM51, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM52)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM52, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM53)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM53, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM54)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM54, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM55)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM55, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM56)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM56, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM57)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM57, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM58)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM58, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM59)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM59, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM60)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.FREE_ITEM60, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK1)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK1, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK2)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK2, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK3)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK3, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK4)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK4, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK5)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK5, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK6)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK6, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK7)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK7, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK8)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK8, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK9)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK9, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                    <tr Class="popup-row">
                                        <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_REMARK10)</td>
                                        <td width="20" style="display:inline-block;"></td>
                                        <td style="display:inline-block; width:485px;">@Html.EditorFor(Function(model) model.Last.REMARK10, New With {.htmlAttributes = New With {.maxlength = 255, .style = "width: 480px;"}})</td>
                                    </tr>
                                </table>
                            </div>
                            <Table>
                                <tr Class="popup-row">
                                    <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_MAKER)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:420px;">@Html.EditorFor(Function(model) model.Last.MAKER, New With {.htmlAttributes = New With {.class = "text-box-name", .maxlength = 60, .style = "width: 400px;"}})</td>
                                    <td class="head-col" style="width:125px;">@Message.getMessage(CommonLabel.LABEL_MAINTENANCE)</td>
                                    <td class="head-col" style="width:105px;">@Message.getMessage(CommonLabel.LABEL_LEASE_COUNT)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:330px;">@Html.TextBoxFor(Function(model) model.Last.MAINTE_LEASE_NUM, New With {.maxlength = 4, .oninput = "editNumber(this);", .style = "text-align: right; width: 100px;"})</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td Class="head-col" style="width:230px;">
                                        @Message.getMessage(CommonLabel.LABEL_PRODUCT_NO_REGIST)
                                        <font color="#ff0000">
                                            @If ViewBag.DrugsFunc = True Then
                                            @Message.getMessage(CommonLabel.LABEL_REQUIRED_INPUT)
                            End If
                                        </font>
                                    </td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:280px;">@Html.EditorFor(Function(model) model.Last.PRODUCT_NO, New With {.htmlAttributes = New With {.class = "text-box-name", .maxlength = 30, .style = "width: 260px;", .oninput = "editPassword(this)"}})</td>
                                    @* --2021/10/13 sv suzuki add start *@
                                    <td style="display:inline-block; width:265px;">
                                        @If Message.getSettingInfo(CommonSettingKey.GoodsReflect.KeyDiv, CommonSettingKey.GoodsReflect.Key1, CommonSettingKey.GoodsReflect.Key2, CommonSettingKey.GoodsReflect.Key3).DATA1 = ReflectFlg.Yes Then
                                        @<input id="reflect_button" Class="defButton" type="button" name="ReflectBtn" value="@Message.getMessage(CommonLabel.BUTTON_REFLECT)" onclick="return btnReflect_onclick()">
                        Else
                                        @<input id="reflect_button_hidden" Class="defButton" type="button" name="ReflectBtn" style="visibility:hidden">
                        End If
                                    </td>
                                    @* --2021/10/13 sv suzuki add end *@
                                    <td class="head-col" style="width:105px;">@Message.getMessage(CommonLabel.LABEL_LEASE_DAY)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:100px;">@Html.TextBoxFor(Function(model) model.Last.MAINTE_LEASE_DAY, New With {.maxlength = 4, .oninput = "editNumber(this);", .style = "text-align: right; width: 100px;"})</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:100px;">@Html.CheckBoxFor(Function(model) model.Last.DispLeaseDayWorkFlg) @Message.getMessage(CommonLabel.LABEL_KADOUBI_KOURYO)</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_MODEL_NO)</td>
                                    <!-- 2023/02/08 Kawamoto UPD START -->
                                    @If ViewBag.DrugsFunc = False Then
                                    @* 薬品機能OFFの場合 *@
                                    @<td width="20" style="display:inline-block;"></td>
                    Else
                                    @* 薬品機能ONの場合 *@
                                    @<td width="20" Class="require" style="display:inline-block;">※</td>
                    End If
                                    <!-- 2023/02/08 Kawamoto UPD END -->
                                    @* --2021/10/13 sv suzuki upd start *@
                                    @*<td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.MODEL_NO, New With {.htmlAttributes = New With {.class = "text-box-name", .maxlength = 30, .style = "width: 400px;", .oninput = "editPassword(this)"}})</td>*@
                                    <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.MODEL_NO, New With {.htmlAttributes = New With {.class = "text-box-name", .maxlength = 30, .style = "width: 400px;"}})</td>
                                    @* --2021/10/13 sv suzuki upd end *@
                                    <td width="20" style="display:inline-block;"></td>
                                    <td class="head-col" style="width:228px;">@Message.getMessage(CommonLabel.LABEL_MAINTENANCE_DAY)</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:100px;">@Html.TextBoxFor(Function(model) model.Last.MAINTE_DAY, New With {.maxlength = 4, .oninput = "editNumber(this);", .style = "text-align: right; width: 100px;"})</td>
                                    <td width="20" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:100px;">@Html.CheckBoxFor(Function(model) model.Last.DispMainteDayWorkFlg) @Message.getMessage(CommonLabel.LABEL_KADOUBI_KOURYO)</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_LEASE_NO)</td>
                                    <td width="20px" style="display:inline-block;"></td>
                                    <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.LEASE_NO, New With {.htmlAttributes = New With {.class = "text-box-name", .maxlength = 30, .style = "width: 400px;"}})</td>
                                    <td rowspan="4" style="margin-left:-100px"><img id="preview" src="Images/noimage.png" style="margin-left:-280px;" width="130" height="140"></td>
                                </tr>
                                <tr Class="popup-row">
                                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)</td>
                                    <td width="20px" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:200px;">@Html.EditorFor(Function(model) model.Last.LeaseLimitString, New With {.style = "width: 70px;", .htmlAttributes = New With {.type = "date"}})</td>
                                    <td class="head-col large">@Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)</td>
                                    <td width="20px" style="display:inline-block;"></td>
                                    <td style="display:inline-block;">@Html.EditorFor(Function(model) model.Last.GenLimitString, New With {.style = "width: 70px;", .htmlAttributes = New With {.type = "date"}})</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_ASSET_PIC_FILENAME)</td>
                                    <td width="20px" style="display:inline-block;"></td>
                                    <td style="display:inline-block;">
                                        <input id="file_button" name="NewImage" type="file" style="display: none;" accept=".jpg,.gif,.png,image/gif,image/jpeg,image/png" />
                                        @Html.EditorFor(Function(model) model.Last.ASSET_PIC_FILENAME, New With {.htmlAttributes = New With {.style = "width: 530px;", .class = "text-box-name", .maxlength = 255, .onclick = "btnImageSelect_onclick()", .readOnly = True}})
                                        <input id="select_file_button" class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_PIC_SELECT)" onclick="btnImageSelect_onclick()">
                                    </td>
                                    <td style="display:inline-block;">
                                        <input id="select_file_clear_button" class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_PIC_CLEAR)" style="margin-left:300px;" onclick="btnImageClear_onclick()">
                                    </td>
                                </tr>
                                <tr Class="popup-row DisplayOnOff">
                                    <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_RETIRE_DT)</td>
                                    <td width="13px" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:195px;">
                                        @Html.EditorFor(Function(model) model.Last.RetireTsString, New With {.htmlAttributes = New With {.style = "width: 120px;"}})
                                    </td>
                                    <td class="head-col large">@Message.getMessage(CommonLabel.LABEL_LEASE_COUNT)</td>
                                    <td width="12px" style="display:inline-block;"></td>
                                    <td style="display:inline-block; width:50px;">@Html.EditorFor(Function(model) model.Last.LEASE_CNT, New With {.htmlAttributes = New With {.style = "width: 50px;text-align:right;"}})</td>
                                </tr>
                                <tr Class="popup-row">
                                    <td class="head-col" style="width:230px;">@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)</td>
                                    <td width="20px" style="display:inline-block;"></td>
                                    <td width="120px" style="display:inline-block;">
                                        @Html.RadioButtonFor(Function(model) model.Last.IS_TANGIBLE, TANGIBLE.TANGIBLE, New With {.id = "IS_TANGIBLE_TANGIBLE"})@Message.getMessage(CommonLabel.VALUE_TANGIBLE_ASSET)
                                    </td>
                                    <td width="120px" style="display:inline-block;">
                                        @Html.RadioButtonFor(Function(model) model.Last.IS_TANGIBLE, TANGIBLE.NOT_TANGIBLE, New With {.id = "IS_TANGIBLE_NOT_TANGIBLE"})@Message.getMessage(CommonLabel.VALUE_INTANGIBLE_ASSET)
                                    </td>
                                    @Html.HiddenFor(Function(model) model.Last.IS_TANGIBLE)
                                </tr>
                                @*<tr Class="popup-row">
                    <td Class="head-col" style="width:230px;">@Message.getMessage(CommonLabelSetting.LABEL_REMARK)</td>
                    <td width="20px" style="display:inline-block;"></td>
                    <td style="display:inline-block;">
                        @Html.EditorFor(Function(model) model.Last.REMARK, New With {.htmlAttributes = New With {.style = "width: 400px;"}})
                    </td>
                </tr>*@
                                <tr><td>　</td></tr> @* 新規登録時、見目をよくするためのダミー行 *@
                            </Table>

                            <div Class="PopupButton">
                                <span style="float:left;margin:15px 100px 0 100px;">
                                    <input id="modal-location-btn" class="defButton modal-location-btn submit-button" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_LOCATION_READ)" onclick="return btnLocationRead_onclick()">
                                </span>
                                <span style="float:left;margin:15px 20px 0 0;">
                                    <input id="modal-create-btn" class="defButton modal-create-btn submit-button" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnUpdate_onclick()">
                                    @*<input id="modal-create-btn" class="defButton modal-close modal-create-btn submit-button" formaction="~/AssetList/Create" type="submit" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CREATE)" onclick="return btnUpdate_onclick()">*@
                                </span>
                                <span style="float:left;margin:15px 10% 0 0;">
                                    <input id="modal-retire-btn" class="defButton modal-create-btn submit-button" type="button" name="RetireBtn" value="@Message.getMessage(CommonLabel.VALUE_RETIRE)" onclick="return btnRetire_onclick()">
                                    @*<input id="modal-retire-btn" class="defButton modal-close modal-create-btn submit-button" formaction="~/AssetList/Retire"  type="submit" name="RetireBtn" value="@Message.getMessage(CommonLabel.VALUE_RETIRE)"onclick="return btnRetire_onclick()">*@
                                </span>
                                <span style="float:left;margin:15px 10% 0 0;">
                                    <input id="modal-delete-btn" class="defButton submit-button" type="button" formaction="~/AssetList/Delete" name="Delete" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="return btnDelete_onclick()">
                                </span>
                                <span style="float: left; margin: 15px 5% 0 0; ">
                                    <input id="modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
                                </span>
                                <span style="float: right; margin: 15px 10% 0 0; ">
                                    <input Class="defButton attachment-btn" style="width: 130px;" type="button" id="btnAttachment" name="Attachment" value="@Message.getMessage(CommonLabel.BUTTON_ATTACHMENT)" onclick="btnAttachment_onclick()">
                                </span>
                            </div>
                        </div>

                'QR画面表示のモーダル画面
                        @<div id="modal-qr-content" style="text-align:center">
                            <div id="QRAssetNo" class="mb10">
                            </div>
                            <img id="qrview" class="mb15" src='' width="150" height="150">
                            <div class="text-center">
                                <input id="modal-close" class="defButton modal-close" style="margin:0 auto;" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
                            </div>
                        </div>


                '2023/03/22 Kawamoto ADD START
                '添付ファイル管理のモーダル画面
                        @<div id="modal-attachment-content" style="height: 550px;">
                            <div Class="popup-title-wrap">
                                <p class="Line3"></p>
                                <p class="Line4"></p>
                                <h2 id="popup-title-attachment"></h2>
                                <p class="Line3"></p>
                                <p class="Line4"></p>
                            </div>
                            <fieldset style="border: 1px solid #000000; padding: 20px;">
                                <legend class="attachment">　@Message.getMessage(CommonLabel.LABEL_ATTACHMENT)　</legend>

                                <div class="ichiran mb15">
                                    <div id="attachment-count">
                                        @Message.getMessage(CommonLabel.KEY_PAGER_LABEL_ALL)  @Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)
                                    </div>
                                    <div id="div_koumoku_h">
                                        <table class="ich_table" id="tbl_koumoku_h">
                                            <tbody>
                                                <tr>
                                                    <th class="col1">@Message.getMessage(CommonLabel.LABEL_SELECT)</th>
                                                    <th class="col2">@Message.getMessage(CommonLabel.LABEL_FILE_NAME)</th>
                                                    <th class="col3">@Message.getMessage(CommonLabel.LABEL_BYTE_NUM)</th>
                                                    <th class="col4">@Message.getMessage(CommonLabel.LABEL_UPLOAD_DATE)</th>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                    <div style="height:270px; overflow-y:scroll;">
                                        <div id="div_koumoku_d">
                                            <table class="ich_table" id="tbl_koumoku_d">
                                                <tbody id="attachment-list-detail">
                                                    @*一覧表示*@
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>

                                <div Class="PopupButton float-left">
                                    <input id="attachment-modal-add" class="defButton modal-add" type="button" value="@Message.getMessage(CommonLabel.BUTTON_ADD)" onclick="btnAddAttachment_onclick()">
                                </div>
                                <div Class="PopupButton float-right">
                                    <input id="attachment-modal-delete" class="defButton modal-delete" type="button" value="@Message.getMessage(CommonLabel.BUTTON_DELETE)" onclick="btnDeleteAttachment_onclick()">
                                </div>
                            </fieldset>
                            <div Class="PopupButton float-right" style="margin: 15px 10% 0 0;">
                                <input id="attachment-modal-close" class="defButton attachment-modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CLOSE)">
                            </div>
                            <input id="AttachmentAssetNo" name="AttachmentAssetNo" type="hidden">
                        </div>


                        @*ファイル選択のモーダル画面*@
                        @<div id="modal-filedrop-content" style="width: 1000px; height: 170px;">
                            <table align="center">
                                <tr>
                                    <td width="65%" style="text-align:left;">
                                        <div id="filedrop-msg" style="margin: 50px 0">
                                        </div>
                                    </td>
                                    <td>
                                        <input type="file" name="SelectedFiles" id="SelectedFiles" style="display:inline-block;" multiple onchange="FileOnSelect()" />
                                    </td>
                                </tr>
                            </table>
                        </div>
        '2023/03/22 Kawamoto ADD END

        'ロケQR読取画面表示のモーダル画面
                        @<div id="modal-location-content" style="width: 500px; height: 100px;">
                            <p>
                                <div class="text-center" id="lblOrderReadLocation" style="font-size:20px; display:block; margin-bottom:10px;">
                                    @Message.getMessage(CommonLabelSetting.MSG_READ_LOCATION_QR)
                                </div>
                            </p>
                            <div Class="text-center">
                                <input id="location-modal-close" Class="defButton location-modal-close" style="margin:0 auto;" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="btnDeleteLocation_onclick()">
                            </div>
                            <textarea id="TxtLocationQr" style="opacity: 0;"></textarea>
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
                        @Html.HiddenFor(Function(model) model.Last.ASSET_STATUS)
                        @Html.HiddenFor(Function(model) model.Last.EPC_ENC_STATUS)
                        @Html.HiddenFor(Function(model) model.Last.TAG_LINK_STATUS)
                        @Html.HiddenFor(Function(model) model.Last.GEN_ASSET_ID)
                        @Html.HiddenFor(Function(model) model.Last.EPC_SIZE)
                        @Html.HiddenFor(Function(model) model.Last.LEASE_LIMIT)
                        @Html.HiddenFor(Function(model) model.Last.GENERIC_LIMIT)
                        @Html.HiddenFor(Function(model) model.Last.RETIRE_TS)
                        @Html.HiddenFor(Function(model) model.Last.REMARK)

                        @Html.HiddenFor(Function(model) model.Last.LEASE_PLACE_CD)
                        @Html.HiddenFor(Function(model) model.Last.LEASE_PLACE_NAME)
                        @Html.HiddenFor(Function(model) model.Last.LEASE_EMPLOYEE_CD)
                        @Html.HiddenFor(Function(model) model.Last.LEASE_EMPLOYEE_NAME)
                        @Html.HiddenFor(Function(model) model.Last.LEASE_DT)
                        @Html.HiddenFor(Function(model) model.Last.LEASE_DUE_DT)
                        @Html.HiddenFor(Function(model) model.Last.RETIRE_EMPLOYEE_CD)
                        @Html.HiddenFor(Function(model) model.Last.RETIRE_EMPLOYEE_NAME)
                        @Html.HiddenFor(Function(model) model.Last.RETIRE_ASSET_ID)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_TS)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_STATUS)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_LOC_L_CD)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_LOC_M_CD)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_LOC_S_CD)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_LOC_L_NAME)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_LOC_M_NAME)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_LOC_S_NAME)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_EMPLOYEE_CD)
                        @Html.HiddenFor(Function(model) model.Last.PHYINV_EMPLOYEE_NAME)
                        @Html.HiddenFor(Function(model) model.Last.TASK_TS)
                        @Html.HiddenFor(Function(model) model.Last.FREE_ITEM1)
                        @Html.HiddenFor(Function(model) model.Last.AssetIdDivReg)
                        @Html.HiddenFor(Function(model) model.Last.NowImageUrl)
                        @Html.HiddenFor(Function(model) model.FirstOrDefault.OpenFlg)
                        @Html.HiddenFor(Function(model) model.FirstOrDefault.DetailOpenFlg)
                        @Html.HiddenFor(Function(model) model.Last.RemainingAmount)
    End Using
</div>
