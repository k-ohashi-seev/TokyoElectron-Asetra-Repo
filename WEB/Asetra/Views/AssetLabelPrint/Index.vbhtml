@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of AssetLabelPrintViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelAssetLabelPrint.TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
End Code
<script>
    //
    //資産ラベル発行画面
    //
    const needToInitCheckbox = @(If(Model.FirstOrDefault.needToInitCheckbox, "true", "false"));

    $(function () {
        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            // エラーメッセージ表示後、ライセンスコントローラーを呼び出すように修正
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        // オプション項目表示非表示設定
        if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            $("#cond").removeClass("in");
            $("#cond").addClass("collapse");
            document.getElementById("SearchCondition").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }

        //上限を超えている場合エラーメッセージ
        if ("@ViewBag.isLimitFlg" == "True") {
            showErrMessage("@String.Format(Message.getMessage(CommonLabel.MSG_ERR_LIMIT_DATA), ViewBag.cntList, ViewBag.cntLimit)")
        }

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessNoHakko") {
            //ラベル不要
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_LABEL_NOT_REQUIRED)")

        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessEdit") {
            //ラベル発行
            // ローディング表示
            showLoading();

            document.getElementById("PrinterKey").value = localStorage.getItem("AssetLabelPrintPrintKey");
            var befAction = $('#form').attr("action");
            $('#form').attr("action", "@Url.Content("~/AssetLabelPrint")/CsvExportForLabel");
            $('#form').submit();
            $('#form').attr("action", befAction);
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessHakko") {
            //ラベル発行           
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_LABEL_PRINT)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {

            if ("@Model.FirstOrDefault.ErrTarget".search("Target_") >= 0) {
                showErrMessage("@Model.FirstOrDefault.ErrCode", "#@Model.FirstOrDefault.ErrTarget".replace("Target_",""))
            } else {
                showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
            }

        }else{
            //ロード前のボタン名を設定する
            document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"
        }

        // ロケーション大区分非選択時は中区分を非活性（中区分・小区分は複数選択）
        if (document.getElementById("SearchLocL").value == "") {
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        // ロケーション中区分非選択時は小区分を非活性
        //[CC431] UPD Start 20261007
        // ※場所から選ぶ ON の場合は、小区分非選択時に中区分を非活性
        if (!isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocM").length == 0) {
            clearMultiSelect("#SearchLocS");
            $("#SearchLocS").attr("disabled", "disabled");
        }
        if (isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocS").length == 0) {
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        //[CC431] UPD End 20261007

        // モーダルダイアログの期限エディットボックス日付フォーマット
        $.datepicker.setDefaults({
            showOn: "focus",
            //dateFormat: 'yymmdd'
        });

        // キー値が存在する場合の項目非表示設定
        if ("@ViewBag.AssetLabelPrintKey") {
            $(".search-wrap").hide();
            $("#btnSelectAll").hide();
            $("#btnLabelPrint").hide();
            $("#btnExcelImport").hide();
            $("#btnCsvExport").hide();
        }

        //隠し項目に格納
        document.getElementById("LoadingImg").value = '@Url.Content("~/Images/")' + "Loading.gif" ;

        btnExcelImportEnabled(true)

        // 列定義
        let columns = [
            { id: "@ColKey.AssetLabelPrint.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispDiv.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.CHECK_SELECT", 40), sortOrder: -2, formatter: AssetLabelPrintCheckboxFormatter, cssClass:  "first-column" },
            { id: "@ColKey.AssetLabelPrint.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.ASSET_NO", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag" },
            { id: "@ColKey.AssetLabelPrint.SUB_ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_SUB_ASSET_NO)", field: "SUB_ASSET_NO", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.SUB_ASSET_NO", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)", field: "ASSET_NAME", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.ASSET_NAME", 350), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_MNG_LOC_L_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.MNG_LOC_L_CD", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag", formatter: MngLocLFormatter },
            { id: "@ColKey.AssetLabelPrint.LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_L_COM_NAME)", field: "LOC_L_CD", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.LOC_L_CD", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag", formatter: LocLFormatter },
            { id: "@ColKey.AssetLabelPrint.LOC_M_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_NAME)", field: "LOC_M_CD", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.LOC_M_CD", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispFlag", formatter: LocMFormatter },
            { id: "@ColKey.AssetLabelPrint.LOC_S_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_NAME)", field: "LOC_S_CD", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.LOC_S_CD", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispFlag", formatter: LocSFormatter },
            { id: "@ColKey.AssetLabelPrint.CAT_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)", field: "CAT_L_CD", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.CAT_L_CD", 280), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag", formatter: CatLFormatter },
            { id: "@ColKey.AssetLabelPrint.IS_TANGIBLE", name: "@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)", field: "IS_TANGIBLE", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.IS_TANGIBLE", 140), sortOrder: "@Model.FirstOrDefault.GridInfo("IS_TANGIBLE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("IS_TANGIBLE").DispFlag", formatter: IsTangibleFormatter},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM1", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM1)", field: "FREE_ITEM1", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM1", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM2", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)", field: "FREE_ITEM2", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM2", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM3", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM3)", field: "FREE_ITEM3", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM3", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM4", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM4)", field: "FREE_ITEM4", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM4", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM5", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM5)", field: "FREE_ITEM5", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM5", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM6", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6)", field: "FREE_ITEM6", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM6", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispFlag", formatter: MngEmpFormatter},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM7", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM7)", field: "FREE_ITEM7", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM7", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM8", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM8)", field: "FREE_ITEM8", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM8", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM9", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM9)", field: "FREE_ITEM9", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM9", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM10", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM10)", field: "FREE_ITEM10", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM10", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM11", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM11)", field: "FREE_ITEM11", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM11", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispFlag", formatter: CommaFormatter, cssClass: "cell-right"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM12", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM12)", field: "FREE_ITEM12", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM12", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispFlag", formatter: CommaFormatter, cssClass: "cell-right"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM13", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM13)", field: "FREE_ITEM13", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM13", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM14", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM14)", field: "FREE_ITEM14", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM14", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM15", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)", field: "FREE_ITEM15", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM15", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM16", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM16)", field: "FREE_ITEM16", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM16", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM17", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM17)", field: "FREE_ITEM17", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM17", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM18", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)", field: "FREE_ITEM18", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM18", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM19", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)", field: "FREE_ITEM19", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM19", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM20", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM20)", field: "FREE_ITEM20", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM20", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM20").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM20").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM21", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM21)", field: "FREE_ITEM21", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM21", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM21").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM21").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM22", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM22)", field: "FREE_ITEM22", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM22", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM22").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM22").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM23", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM23)", field: "FREE_ITEM23", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM23", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM23").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM23").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM24", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM24)", field: "FREE_ITEM24", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM24", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM24").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM24").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM25", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM25)", field: "FREE_ITEM25", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM25", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM25").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM25").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM26", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM26)", field: "FREE_ITEM26", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM26", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM26").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM26").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM27", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM27)", field: "FREE_ITEM27", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM27", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM27").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM27").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM28", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM28)", field: "FREE_ITEM28", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM28", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM28").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM28").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM29", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM29)", field: "FREE_ITEM29", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM29", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM29").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM29").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM30", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM30)", field: "FREE_ITEM30", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM30", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM30").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM30").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM31", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM31)", field: "FREE_ITEM31", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM31", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM31").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM31").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM32", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM32)", field: "FREE_ITEM32", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM32", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM32").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM32").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM33", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM33)", field: "FREE_ITEM33", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM33", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM33").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM33").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM34", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM34)", field: "FREE_ITEM34", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM34", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM34").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM34").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM35", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM35)", field: "FREE_ITEM35", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM35", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM35").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM35").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM36", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM36)", field: "FREE_ITEM36", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM36", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM36").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM36").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM37", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM37)", field: "FREE_ITEM37", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM37", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM37").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM37").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM38", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM38)", field: "FREE_ITEM38", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM38", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM38").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM38").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM39", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM39)", field: "FREE_ITEM39", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM39", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM39").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM39").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM40", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM40)", field: "FREE_ITEM40", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM40", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM40").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM40").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM41", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM41)", field: "FREE_ITEM41", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM41", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM41").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM41").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM42", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM42)", field: "FREE_ITEM42", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM42", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM42").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM42").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM43", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM43)", field: "FREE_ITEM43", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM43", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM43").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM43").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM44", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM44)", field: "FREE_ITEM44", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM44", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM44").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM44").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM45", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM45)", field: "FREE_ITEM45", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM45", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM45").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM45").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM46", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM46)", field: "FREE_ITEM46", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM46", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM46").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM46").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM47", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM47)", field: "FREE_ITEM47", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM47", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM47").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM47").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM48", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM48)", field: "FREE_ITEM48", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM48", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM48").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM48").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM49", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM49)", field: "FREE_ITEM49", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM49", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM49").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM49").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM50", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM50)", field: "FREE_ITEM50", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM50", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM50").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM50").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM51", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM51)", field: "FREE_ITEM51", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM51", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM51").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM51").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM52", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM52)", field: "FREE_ITEM52", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM52", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM52").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM52").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM53", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM53)", field: "FREE_ITEM53", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM53", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM53").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM53").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM54", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM54)", field: "FREE_ITEM54", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM54", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM54").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM54").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM55", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM55)", field: "FREE_ITEM55", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM55", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM55").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM55").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM56", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM56)", field: "FREE_ITEM56", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM56", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM56").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM56").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM57", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM57)", field: "FREE_ITEM57", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM57", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM57").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM57").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM58", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM58)", field: "FREE_ITEM58", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM58", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM58").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM58").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM59", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM59)", field: "FREE_ITEM59", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM59", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM59").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM59").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.FREE_ITEM60", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM60)", field: "FREE_ITEM60", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.FREE_ITEM60", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM60").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM60").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.IP_DATETIME", name: "@Message.getMessage(CommonLabel.LABEL_IP_DATETIME)", field: "IP_DATETIME", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.IP_DATETIME", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("IP_DATETIME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("IP_DATETIME").DispFlag", formatter: IpDatetimeFormatter},
            { id: "@ColKey.AssetLabelPrint.IP_FILE_NAME", name: "@Message.getMessage(CommonLabel.LABEL_IP_FILE_NAME)", field: "IP_FILE_NAME", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.IP_FILE_NAME", 470), sortOrder: "@Model.FirstOrDefault.GridInfo("IP_FILE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("IP_FILE_NAME").DispFlag"},
            { id: "@ColKey.AssetLabelPrint.IP_EMPLOYEE_CD", name: "@Message.getMessage(CommonLabel.LABEL_IP_EMPLOYEE_CD)", field: "INS_EMPLOYEE_CD", width: getColumnWidth("@DispId.ASSET_LABEL_PRINT", "@ColKey.AssetLabelPrint.IP_EMPLOYEE_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("IP_EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("IP_EMPLOYEE_CD").DispFlag", formatter: IpEmployeeFormatter},
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
            // Key1:資産番号 key2: カテゴリ大区分コード
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

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列を加算
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispDiv.ASSET_LABEL_PRINT", fixCol);

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

        //ローカルストレージから復元
        const saved = localStorage.getItem("AssetLabelPrintConditions");
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

        // チェックボックス状態最新化
        setting_checklist()
        document.getElementById('needToInitCheckbox').value = "True";

    });

    //ローカルストレージ保存
    function localStorageSave() {
        let cond = { SearchLocL: document.getElementById("SearchLocL").value };

        //ローカルストレージを保存
        localStorage.setItem("AssetLabelPrintConditions", JSON.stringify(cond));
    }

    // チェックボックス(中央寄せ)
    function AssetLabelPrintCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.ASSET_NO) + ',' + convertToEmptyIfNull(dataContext.CAT_L_CD);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
               '<input type="checkbox" class="slick-checkbox" name="checkEdit" ' +
               'value="' + key + '" '+
               (isKeyInclude ? 'checked="checked" ' : '') +
               'onchange="check_onclick(this)" ' +
               '/>' +
               '</div>';
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

    // カテゴリ大分類コード用formatter
    function CatLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_L_CD) + ' ' + convertToEmptyIfNull(dataContext.CatLName);
    }

    // 取込担当者コード用formatter
    function IpEmployeeFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.IP_EMPLOYEE_CD) + ' ' + convertToEmptyIfNull(dataContext.IpEmployeeName);
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

    // 有形・無形区分用formatter
    function IsTangibleFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";
        switch (String(dataContext.IS_TANGIBLE)) {
            case "@TANGIBLE.TANGIBLE" :
        status =  "@Message.getMessage(CommonLabel.LABEL_TANGIBLE_ASSET_NAME)"
        break;
            case "@TANGIBLE.NOT_TANGIBLE" :
        status = "@Message.getMessage(CommonLabel.LABEL_INTANGIBLE_ASSET_NAME)"
        break;
        }
        return status;
    }

    // 取込日時formatter
    function IpDatetimeFormatter(row, cell, value, columnDef, dataContext) {
        if (value == null || value === "") return ""
        let strVal = value.toString();
        if (strVal.length !== 14) return value;
        const yyyy = strVal.substring(0, 4);
        const MM = strVal.substring(4, 6);
        const dd = strVal.substring(6, 8);
        const HH = strVal.substring(8, 10);
        const mm = strVal.substring(10, 12);
        return `${yyyy}/${MM}/${dd} ${HH}:${mm}`;
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
            sImportUrl = '@Url.Content("~/AssetLabelPrint")/ImportExcelFileConfirmed';
        } else {
            formData = new FormData($form.get(0));
            formData.append("ExcelFile", $('#ExcelFile')[0].files[0]);
            formData.append("LocLCd", document.getElementById("SearchLocL").value);
            sImportUrl = '@Url.Content("~/AssetLabelPrint")/ImportExcelFile';
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
                                    url: '@Url.Content("~/AssetLabelPrint")/ImportExcelFileCancel',
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
        return showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/AssetLabelPrint")/IchiranCsvExport");
    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

    //ラベル不要ボタンクリック時処理
    function btnLabelNotRequired_onclick() {
        setting_checklist();

        // 未選択エラーチェック
        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        // ダイアログのメッセージを設定
        strMessage = "@Message.getMessage(CommonLabel.MSG_LABEL_NOT_REQUIRED_CONFIRM)".replace("@@@@CR@@@@", "<BR>");
        $("#show-dialog").html(strMessage);

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass:  "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $(this).dialog("close");

                    //画面ブロックを表示
                    showLoading();

                    document.getElementById('needToInitCheckbox').value = "False";

                    var befAction = $('#form').attr("action");
                    $('#form').attr("action", "@Url.Content("~/AssetLabelPrint")/processLabelNotRequired");
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

    //ラベル発行ボタンクリック時処理
    function btnLabelPrint_onclick() {
        document.getElementById("popup-title-prt").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SELECT_PRINTER)";

        //チェックボックスの状態からキーを設定する
        setting_checklist();

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
                    $.ajax({
                        type: 'POST',
                        url: '@Url.Content("~/AssetLabelPrint")/ShowPrinterList',
                        dataType: 'json',
                        data: { SelectKeys: document.getElementById("SelectKeys").value, NoneSelectKeys: document.getElementById("NoneSelectKeys").value},
                        cache: false,
                        success: function (data, dataType) {
                            if (data == null) {
                                return;
                            } else {
                                // 照会のみのデータがあればエラーメッセージ
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
                                    const savedKey = localStorage.getItem("AssetLabelPrintPrintKey");
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

        if (prtKey) {
            //画面ブロックを表示
            showLoading();

            document.getElementById('needToInitCheckbox').value = "False";

            // プリンタ選択状態保存
            localStorage.setItem("AssetLabelPrintPrintKey", prtKey);
            // ラベル発行処理の実行
            var befAction = $('#form').attr("action");
            $('#form').attr("action", "@Url.Content("~/AssetLabelPrint")/DbUpdateForLabel");
            $('#form').submit();
            $('#form').attr("action", befAction);
        }
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        // ロケーション(会社)必須チェック
        var locL = document.getElementById("SearchLocL").value;
        if (locL.trim() === "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERRNO_SELECT_HUB_LOC_L)")
            return false;
        }

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        // 取込日日付範囲チェック
        var ipDateFrom = document.getElementById("SearchIpDateFrom").value.split("/");
        var ipDateTo = document.getElementById("SearchIpDateTo").value.split("/");

        if(ipDateFrom != "" && ipDateTo != "" ){
            // 両方とも空白でない時
            // 期間前後チェック
            if (ipDateFrom > ipDateTo ){
                showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")
                return false;
            }
        }

        //画面ブロックを表示
        showLoading();

        //ローカルストレージに検索条件保存
        localStorageSave();

        //ページ番号のリセット
        ResetPageNum();

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        return true;
    }


    //ページ番号クリック処理
    function btnPage_onclick(element) {
        //画面ブロックを表示
        showLoading();

        document.getElementById("PageNum").value = element.name;

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("SearchCondition").innerText;

        //チェックボックスの状態からキーを設定する
        setting_checklist();
        document.getElementById('needToInitCheckbox').value = "False";

        $('#form').submit();
    }


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

        //非同期で現在の検索条件での全件を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetLabelPrint")/SelectAll',
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

    //大区分選択
    //※検索条件の中区分・小区分は複数選択のため、initialM・initialS は配列で指定する
    function ChangeLocationL(locL,initialM,initialS,doAutoSearch) {
        //[CC431] UPD Start 20261007
        // 原価センタ・場所をクリアし、場所から選ぶの ON／OFF に合わせて選択肢を作り直す（場所から選ぶの ON／OFF は残す）
        resetSearchLocOptions('Loc', initialM, initialS, function () {
            // 検索処理実行（初期表示時のみ。会社が未選択の場合は実行しない）
            if (doAutoSearch && locL.value && document.getElementById('FirstSearch').value == "True") {
                document.getElementById('Search_Button').click();
            }
        });
        //[CC431] UPD End 20261007
    }

    //中区分選択
    //※検索条件の中区分・小区分は複数選択のため、initialS は配列で指定する
    function ChangeLocationM(locM,initialS,doAutoSearch) {
        var lCode = ""
        if(locM.name == "SearchLocM"){
            lCode = document.getElementById("SearchLocL").value;
        }

        // 選択中の中区分コード（複数選択のため「,」区切りで渡す）
        var mCode = getMultiSelectValues(locM).join(",");

        // 小区分をクリア
        if(locM.name == "SearchLocM"){
            clearMultiSelect("#SearchLocS");
            $("#SearchLocS").prop("disabled", true);
        }

        // 選択されていない場合はここで終了
        if (!mCode) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetLabelPrint")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationS", selectLCode: lCode, selectMCode: mCode },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocS = ""
                    if(locM.name == "SearchLocM"){
                        SearchLocS = document.getElementById("SearchLocS");
                    }
                    SearchLocS.options.length = 0;

                    // 複数選択では選択解除用の空の選択肢は不要
                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocS.appendChild(op);
                    }
                }
                // 小区分データ取得
                if(locM.name == "SearchLocM"){
                    $("#SearchLocS").removeAttr("disabled");
                }

                // 選択肢の入れ替えをSelect2に反映
                setMultiSelectValues(SearchLocS, initialS);
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
            url: '@Url.Content("~/AssetLabelPrint")/GetSearchLocationOptions',
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

    //検索項目欄　詳細検索クリック時処理
    function DetailAccordion_onclick() {
        if (document.getElementById("DetailAccordion").innerHTML == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)"){
            $(".detail-accordion").css("display", "none");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)"
        }else{
            $(".detail-accordion").css("display", "inline-block");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)"
        }
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

</script>

<div id="asset-list">
    @Using (Html.BeginForm("Index", "AssetLabelPrint", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    <div>
                        @If ViewBag.hasUpdAuthFlg = AUTH_FLG.Yes Then
                            @<input id = "labelNotRequiredBtn" Class="defButton update-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_LABEL_NOT_REQUIRED)" onclick="btnLabelNotRequired_onclick()">
                        End If
                        @* 全選択/全解除ボタン *@
                        @Code
                            Dim AllSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG)
                            Dim sDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                            If AllSelectFlg Then
                                sDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                            End If
                        End Code
                        <input class="defButton" type="button" id="btnSelectAll" value="@sDispStr" onclick="btnSelectAll_onclick()">
                        @If ViewBag.hasUpdAuthFlg = AUTH_FLG.Yes Then
                            @<input Class="defButton update-btn" type="button" id="btnLabelPrint" value="@Message.getMessage(CommonLabel.BUTTON_LABEL_PRINT)" onclick="btnLabelPrint_onclick()">
                        End If
                    </div>
                </div>
                <div Class="view-area-button-right">
                    @If ViewBag.hasUpdAuthFlg = AUTH_FLG.Yes Then
                        @<input Class="defButton csv-import-btn" type="button" id="btnExcelImport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_ASSET_LEDGER_IMPORT)" onclick="btnExcelImport_onclick()">
                    End If
                    <input Class="defButton csv-Export-btn  submit-button" type="button" id="btnCsvExport" name="CsvExport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnIchiranCsvExport_onclick()">
                </div>
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
                                @*[CC431] DEL 20261007 「場所から選ぶ」の画面イメージ確認用のコメントを削除（実装済みのため）*@
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
                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchLocS, DirectCast(ViewBag.modelLocS, IEnumerable(Of SelectListItem)), New With {.onchange = "onSearchLocChange('Loc', '" & LOC_DIV.LocationS & "')", .class = "cond-control select2-clearable"})
                                </div>
                                @*[CC431] UPD End 20261007*@
                            </div>
                            @* カテゴリー大分類（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                            <div class="col-sm-2 combo-area" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                                <div class="grid-condition-col">
                                    @* ListBoxFor で複数選択（select multiple）。select2-clearable クラスで×ボタン付きSelect2になる。空の選択肢は不要 *@
                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), New With {.class = "cond-control select2-clearable"})
                                </div>
                            </div>
                            @* 有形・無形区分 *@
                            <div class="col-sm-2" style="display: block; height: 56px;">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchIsTangibleY, New With {.style = "margin-right:5px;"})
                                    @Message.getMessage(CommonLabel.VALUE_TANGIBLE_ASSET)
                                    @Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchIsTangibleN, New With {.style = "margin-left:15px; margin-right:5px;"})
                                    @Message.getMessage(CommonLabel.VALUE_INTANGIBLE_ASSET)
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
                            @* 管理社員番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_MANAGER_EMPLOYEE_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem6, New With {.class = "cond-control", .maxlength = 15})
                                </div>
                            </div>
                            @* 資産グループコード *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem15, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 取込日 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_IP_DATE)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchIpDateFrom, New With {.class = "cond-control-data", .type = "date"})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchIpDateTo, New With {.class = "cond-control-data", .type = "date"})
                                </div>
                            </div>
                            @* 取込担当者コード *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_IMPORT_EMPLOYEE_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchInsEmployeeCd, New With {.class = "cond-control", .maxlength = 15})
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
                                        New KeyValuePair(Of Integer, String)(AssetLabelPrintSort.CatLAsc, Message.getMessage(CommonLabel.LABEL_CAT_L)),
                                        New KeyValuePair(Of Integer, String)(AssetLabelPrintSort.AssetNoAsc, Message.getMessage(CommonLabel.LABEL_ASSET_NO)),
                                        New KeyValuePair(Of Integer, String)(AssetLabelPrintSort.LocMAsc, Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_CODE)),
                                        New KeyValuePair(Of Integer, String)(AssetLabelPrintSort.LocSAsc, Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_CODE)),
                                        New KeyValuePair(Of Integer, String)(AssetLabelPrintSort.FreeItem2Asc, Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)),
                                        New KeyValuePair(Of Integer, String)(AssetLabelPrintSort.FreeItem6Asc, Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6_CD))
                                    }
                                End Code
                                @For Each kvSort As KeyValuePair(Of Integer, String) In lstSort
                                    @<div class="input-select-item">
                                        @Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchSortOrder, kvSort.Key)@(kvSort.Value & sSortAsc)
                                    </div>
                                Next
                            </div>
                        </div>
                    </div>
                    <div class="view-area-label" style="height: 25px"></div>
                    <input class="float-right defButton submit-button" id="Search_Button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>


        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
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
        @*@<input id="SelectKeys" name="SelectKeys" type="hidden" >
        @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden">*@
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
                Dim prtSessionModel As AssetLabelPrintViewModel = Session(CommonSessionKey.CREAGE_ASSET_LABEL_PRINT)
                If Not IsNothing(prtSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(prtSessionModel)
                Else
                    Model.Add(New AssetLabelPrintViewModel())
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
            @Html.AntiForgeryToken()
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

        @<input type="hidden" id="IsRestoreOwnSearchConditions" name="IsRestoreOwnSearchConditions" value="@Model.FirstOrDefault().IsRestoreOwnSearchConditions.ToString()" />
        @<input type="hidden" id="FirstSearch" name="FirstSearch" value="@Model.FirstOrDefault().FirstSearch.ToString()" />
        @<input type="hidden" id="needToInitCheckbox" name="needToInitCheckbox" value="@Model.FirstOrDefault().needToInitCheckbox.ToString()" />

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
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OpenFlg)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.SelectKeys)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.NoneSelectKeys)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.PrinterKey)
                End Using
</div>
