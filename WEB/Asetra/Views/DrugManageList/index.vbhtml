@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of DrugManageListViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelDrugManageList.TITLE)
End Code

<script>
    //
    //管理簿出力画面
    //
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            //エラーメッセージ表示後、ライセンスコントローラーを呼び出す
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        // オプション項目表示非表示設定
        if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_DRUG_SPECIFY_CLOSE)") {
            $(".detail-accordion").css("display", "inline-block");
            document.getElementById("DetailAccordion").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }

        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessCreate") {
            //登録
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE)")
        }

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        // ロケーション大区分非選択時は中区分を非活性
        if (document.getElementById("SearchLocL").value == "") {
            document.getElementById("SearchLocM").value = ""
            $("#SearchLocM").attr("disabled", "disabled");
        }
        // ロケーション中区分非選択時は小区分を非活性
        if (document.getElementById("SearchLocM").value == "") {
            document.getElementById("SearchLocS").value = "";
            $("#SearchLocS").attr("disabled", "disabled");
        }
        // カテゴリー大分類非選択時は中区分を非活性
        if (document.getElementById("SearchCatL").value == "") {
            document.getElementById("SearchCatM").value = ""
            $("#SearchCatM").attr("disabled", "disabled");
        }
        // カテゴリー中分類非選択時は小区分を非活性
        if (document.getElementById("SearchCatM").value == "") {
            document.getElementById("SearchCatS").value = "";
            $("#SearchCatS").attr("disabled", "disabled");
        }

        let updFlg = false;

        //拠点の更新権限チェック
        let arrayAuthLoc = @Html.Raw(Json.Encode(ViewBag.lstAuthLocSF));
        for(let i = 0; i < arrayAuthLoc.length; i++) {
            if ("@Model.FirstOrDefault.SearchLocL" === arrayAuthLoc[i].LOC_CD) {
                if (arrayAuthLoc[i].UPD_FLG == "Y") {
                    updFlg = true;
                    break;
                }
            }
        }

        //管理簿確認ボタン押下可否設定
        if ("@ViewBag.AuthUpdFlg" == "@AUTH_FLG.Yes" && updFlg) {
            btnManageBookCheckEnabled(true)
        }else{
            btnManageBookCheckEnabled(false)
        }
        //listcheckEnabled(true)
        //btnSelectAllEnabled(true)

        // 列定義
        let columns = [
						{ id: "@ColKey.DrugManageList.CHECK_SELECT"
						, name: "@Message.getMessage(CommonLabel.LABEL_SELECT)"
						, field: "CHECK_SELECT"
						, sortOrder:  -2
						, width: getColumnWidth("@DispId.DRUG_MANAGE_LIST", "@ColKey.DrugManageList.CHECK_SELECT", 40)
						, formatter: CheckSelectFormatter
						, cssClass: "first-column"
						},

						{ id: "@ColKey.DrugManageList.ASSET_NO"
						, name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)"
						, field: "ASSET_NO"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag"
						, width: getColumnWidth("@DispId.DRUG_MANAGE_LIST", "@ColKey.DrugManageList.ASSET_NO", 250)
						, formatter: AssetNoFormatter
						},

						{ id: "@ColKey.DrugManageList.ASSET_NAME"
						, name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)"
						, field: "ASSET_NAME"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag"
						, width: getColumnWidth("@DispId.DRUG_MANAGE_LIST", "@ColKey.DrugManageList.ASSET_NAME", 250)
						, formatter: AssetNameFormatter
						},

						{ id: "@ColKey.DrugManageList.LOT_NO"
						, name: "@Message.getMessage(CommonLabel.LABEL_LOT_NO)"
						, field: "LOT_NO"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("LOT_NO").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("LOT_NO").DispFlag"
						, width: getColumnWidth("@DispId.DRUG_MANAGE_LIST", "@ColKey.DrugManageList.LOT_NO", 250)
						, formatter: LotNoFormatter
						},

						{ id: "@ColKey.DrugManageList.PURCHASER"
						, name: "@Message.getMessage(CommonLabel.LABEL_PURCHASER)"
						, field: "PURCHASER"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("PURCHASER").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("PURCHASER").DispFlag"
						, width: getColumnWidth("@DispId.DRUG_MANAGE_LIST", "@ColKey.DrugManageList.PURCHASER", 250)
						, formatter: PurchaserFormatter
						},

						{ id: "@ColKey.DrugManageList.DUE_DATE"
						, name: "@Message.getMessage(CommonLabel.LABEL_DUE_DATE)"
						, field: "DUE_DATE"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("DUE_DATE").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("DUE_DATE").DispFlag"
						, width: getColumnWidth("@DispId.DRUG_MANAGE_LIST", "@ColKey.DrugManageList.DUE_DATE", 100)
						, formatter: DueDateFormatter
						},

						{ id: "@ColKey.DrugManageList.LATEST_CHECK_DATE"
						, name: "@Message.getMessage(CommonLabel.LABEL_LATEST_CHECK_DATE)"
						, field: "LATEST_CHECK_DATE"
						, sortOrder:  "@Model.FirstOrDefault.GridInfo("LATEST_CHECK_DATE").DispOrder"
						, visible: "@Model.FirstOrDefault.GridInfo("LATEST_CHECK_DATE").DispFlag"
						, width: getColumnWidth("@DispId.DRUG_MANAGE_LIST", "@ColKey.DrugManageList.LATEST_CHECK_DATE", 100)
						, formatter: LatestCheckDateFormatter
						},

        ]
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
            fixCol = fixCol + 1       //選択列と変更列を加算
        }

        // Grid表示判定
        let data = [];
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.DRUG_MANAGE_LIST", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, "@DispId.DRUG_MANAGE_LIST", fixCol);
        }

    });

    //検索項目欄　詳細検索クリック時処理
    function DetailAccordion_onclick() {
        if (document.getElementById("DetailAccordion").innerHTML == "@Message.getMessage(CommonLabel.LABEL_DRUG_SPECIFY_CLOSE)") {
            $(".detail-accordion").css("display", "none");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_DRUG_SPECIFY_OEPN)"
        } else {
            $(".detail-accordion").css("display", "inline-block");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_DRUG_SPECIFY_CLOSE)"
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

    //大区分選択
    function ChangeLocationL(locL, initialM, initialS) {
        if (locL.name == "SearchLocL") {
            // 中区分をクリア
            document.getElementById("SearchLocM").value = "";
            $("#SearchLocM").attr("disabled", "disabled");
            // 小区分をクリア
            document.getElementById("SearchLocS").value = "";
            $("#SearchLocS").attr("disabled", "disabled");
        } else {
            // 中区分をクリア
            document.getElementById("LOC_M_CD").value = "";
            $("#LOC_M_CD").attr("disabled", "disabled");
            // 小区分をクリア
            document.getElementById("LOC_S_CD").value = "";
            $("#LOC_S_CD").attr("disabled", "disabled");
        }


        // 選択されていない場合はここで終了
        if (!locL.value) {
            return;
        }

        let updFlg = false;

        //拠点の更新権限チェック
        let arrayAuthLoc = @Html.Raw(Json.Encode(ViewBag.lstAuthLocSF));
        for(let i = 0; i < arrayAuthLoc.length; i++) {
            if (locL.value === arrayAuthLoc[i].LOC_CD) {
                if (arrayAuthLoc[i].UPD_FLG == "Y") {
                    updFlg = true;
                    break;
                }
            }
        }

        //管理簿確認と帳票ボタン押下可否設定
        if ("@ViewBag.AuthUpdFlg" == "@AUTH_FLG.Yes" && updFlg) {
            btnManageBookCheckEnabled(true)
        } else {
            btnManageBookCheckEnabled(false)
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/DrugManageList")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationM", selectLCode: locL.value, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocM = ""
                    if (locL.name == "SearchLocL") {
                        SearchLocM = document.getElementById("SearchLocM");
                    } else {
                        SearchLocM = document.getElementById("LOC_M_CD");
                    }

                    SearchLocM.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchLocM.appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocM.appendChild(op);
                    }
                }
                // 中区分データ取得
                if (locL.name == "SearchLocL") {
                    $("#SearchLocM").removeAttr("disabled");
                } else {
                    $("#LOC_M_CD").removeAttr("disabled");
                }
                SearchLocM.value = data.locMDropdownData;
                if (initialM) {
                    SearchLocM.value = initialM;
                    ChangeLocationM(SearchLocM, initialS);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //中区分選択
    function ChangeLocationM(locM, initialS) {
        var lCode = ""
        if (locM.name == "SearchLocM") {
            lCode = document.getElementById("SearchLocL").value;
        } else {
            lCode = document.getElementById("LOC_L_CD").value;
        }

        // 小区分をクリア
        if (locM.name == "SearchLocM") {
            document.getElementById("SearchLocS").value = "";
            $("#SearchLocS").attr("disabled", "disabled");
        } else {
            document.getElementById("LOC_S_CD").value = "";
            $("#LOC_S_CD").attr("disabled", "disabled");
        }


        // 選択されていない場合はここで終了
        if (!locM.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/DrugManageList")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationS", selectLCode: lCode, selectMCode: locM.value },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocS = ""
                    if (locM.name == "SearchLocM") {
                        SearchLocS = document.getElementById("SearchLocS");
                    } else {
                        SearchLocS = document.getElementById("LOC_S_CD");
                    }
                    SearchLocS.options.length = 0;

                    let opBlank = document.createElement("option");
                    opBlank.value = "";
                    opBlank.text = "";
                    SearchLocS.appendChild(opBlank);

                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocS.appendChild(op);
                    }
                }
                // 小区分データ取得
                if (locM.name == "SearchLocM") {
                    $("#SearchLocS").removeAttr("disabled");
                } else {
                    $("#LOC_S_CD").removeAttr("disabled");
                }


                SearchLocS.value = data.locSDropdownData;
                if (initialS) {
                    SearchLocS.value = initialS;
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //カテゴリー大分類選択
    function ChangeCategoryL(catL, initialM, initialS) {
        if (catL.name == "SearchCatL") {
            // 中分類をクリア
            document.getElementById("SearchCatM").value = "";
            $("#SearchCatM").attr("disabled", "disabled");
            // 小分類をクリア
            document.getElementById("SearchCatS").value = "";
            $("#SearchCatS").attr("disabled", "disabled");
        } else {
            // 中区分をクリア
            document.getElementById("CAT_M_CD").value = "";
            $("#CAT_M_CD").attr("disabled", "disabled");
            // 小区分をクリア
            document.getElementById("CAT_S_CD").value = "";
            $("#CAT_S_CD").attr("disabled", "disabled");
        }

        // 選択されていない場合はここで終了
        if (!catL.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/DrugManageList")/ChangeCategory',
            data: { categoryLevel: "@CAT_DIV.CategoryM", selectLCode: catL.value, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchCatM = ""
                    if (catL.name == "SearchCatL") {
                        SearchCatM = document.getElementById("SearchCatM");
                    } else {
                        SearchCatM = document.getElementById("CAT_M_CD");
                    }

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
                if (catL.name == "SearchCatL") {
                    $("#SearchCatM").removeAttr("disabled");
                } else {
                    $("#CAT_M_CD").removeAttr("disabled");
                }

                SearchCatM.value = data.catMDropdownData;
                if (initialM) {
                    SearchCatM.value = initialM;
                    ChangeCategoryM(SearchCatM, initialS);
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //カテゴリー中分類選択
    function ChangeCategoryM(catM, initialS) {
        var lCode = ""
        if (catM.name == "SearchCatM") {
            lCode = document.getElementById("SearchCatL").value;
        } else {
            lCode = document.getElementById("CAT_L_CD").value;
        }

        // 小分類をクリア
        if (catM.name == "SearchCatM") {
            document.getElementById("SearchCatS").value = "";
            $("#SearchCatS").attr("disabled", "disabled");
        } else {
            document.getElementById("CAT_S_CD").value = "";
            $("#CAT_S_CD").attr("disabled", "disabled");
        }


        // 選択されていない場合はここで終了
        if (!catM.value) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/DrugManageList")/ChangeCategory',
            data: { categoryLevel: "@CAT_DIV.CategoryS", selectLCode: lCode, selectMCode: catM.value },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchCatS = ""
                    if (catM.name == "SearchCatM") {
                        SearchCatS = document.getElementById("SearchCatS");
                    } else {
                        SearchCatS = document.getElementById("CAT_S_CD");
                    }

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
                if (catM.name == "SearchCatM") {
                    $("#SearchCatS").removeAttr("disabled");
                } else {
                    $("#CAT_S_CD").removeAttr("disabled");
                }

                SearchCatS.value = data.catSDropdownData;
                if (initialS) {
                    SearchCatS.value = initialS;
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //管理簿確認ボタン押下可否設定
    function btnManageBookCheckEnabled(enable) {
        if (enable) {
            //活性
            $("#manage-check-btn").attr("disabled", false)
            $("#manage-check-btn").css("background-color", "#79A7DA");
        } else {
            //非活性
            $("#manage-check-btn").attr("disabled", "disabled")
            $("#manage-check-btn").css("background-color", "#cecece");
        }
    }

    //全選択ボタン押下可否設定
    function btnSelectAllEnabled(enable) {
        if (enable) {
            //活性
            $("#btnSelectAll").attr("disabled", false)
            $("#btnSelectAll").css("background-color", "#79A7DA");
        } else {
            //非活性
            $("#btnSelectAll").attr("disabled", "disabled")
            $("#btnSelectAll").css("background-color", "#cecece");
        }
    }

    //一覧選択チェックボックス押下可否設定
    function listcheckEnabled(enable) {
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            //チェックボックスの場合
            if (checkBoxes[i].type == "checkbox") {
                //キーを取得する
                if (enable) {
                    //活性
                    checkBoxes[i].disabled = false
                } else {
                    //非活性
                    checkBoxes[i].disabled = true
                }
            }
        }

    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        //出力条件チェック
        if (!inputCheck()) { return false }

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        //ページ番号のリセット
        ResetPageNum();
        return true;
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;

        // 詳細項目の開閉状態を取得
        document.getElementById("OpenFlg").value = document.getElementById("DetailAccordion").innerHTML;

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        //ページ切替用検索ボタン
        document.getElementById("search-btn-page-id").click();
    }

    //全選択ボタンクリック時処理
    function btnSelectAll_onclick() {
        var chkFlag = false;
        var btn = document.getElementById("btnSelectAll");
        if (btn.value == "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)") {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_OFF)";
            $('input[name="checkEdit"]').prop('checked', true)
            chkFlag = true;
        } else {
            btn.value = "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
            $('input[name="checkEdit"]').prop('checked', false)
            chkFlag = false;
        }

        //非同期で現在の検索条件での全件を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/DrugManageList")/SelectAll',
            data: { blnSelectAll: chkFlag },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                //件数を設定する
                document.getElementById("selectcount").innerHTML = data;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
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

    function inputCheck() {
        //◆期限
        var searchDateFrom = document.getElementById("SearchDateFrom").value;
        var searchDateTo = document.getElementById("SearchDateTo").value;

        //未入力チェック
        if (searchDateFrom == "" && searchDateTo == "") {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_ENTERED_PERIOD_FROM_OR_TO)")
            return false
        }

        // 期間前後チェック
        if (searchDateFrom != "" && searchDateTo != "") {
            // 両方とも空白でない時
            if (searchDateFrom > searchDateTo) {
                showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")
                return false;
            }
        }

        //◆保管場所
        var storageLoc = document.getElementById("SearchLocL").value;

        //未入力チェック
        if (storageLoc == "") {
            showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_STORAGE_LOC)")
            return false
        }

        return true;
    }

    //管理簿確認ボタンクリック処理
    function btnManageBookCheck_onclick() {

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        //確認ダイアログを表示
        return showConfirm("@Message.getMessage(CommonInfoMsg.DRUG_MANEGE_CHECK_CONFIRM)", "@Url.Content("~/DrugManageList")/ManageBookCheck");

    }

    //帳票出力ボタンクリック処理
    function btnReportOutput_onclick() {

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        //出力条件チェック
        if (!inputCheck()) { return false }

        //確認ダイアログを表示
        return showConfirm("@Message.getMessage(CommonInfoMsg.REPORT_OUTPUT_COFIRM)", "@Url.Content("~/DrugManageList")/ReportOutput");
    }

    // 選択formatter
    function CheckSelectFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.ASSET_NO);
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;">' +
               '<input type="checkbox" class="slick-checkbox"  name="checkEdit" ' +
               'value="' + key + '" '+
               (isKeyInclude ? 'checked="checked" ' : '') +
               'onchange="check_onclick(this)" ' +
               ' />' +
               '</div>';
    }
    // 資産番号formatter
    function AssetNoFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.ASSET_NO) ;
    }
    // 資産名称formatter
    function AssetNameFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.ASSET_NAME) ;
    }
    // ロット番号formatter
    function LotNoFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOT_NO) ;
    }
    // 購入者formatter
    function PurchaserFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.PURCHASER) ;
    }
    // 納入日formatter
    function DueDateFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.DUE_DATE) ;
    }
    // 直近確認日formatter
    function LatestCheckDateFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LATEST_CHECK_DATE) ;
    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

</script>

<div id="specifed-qty-list-output">
    @Using (Html.BeginForm("Index", "DrugManageList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
    @Html.AntiForgeryToken()
        @* 画面上部のボタン *@
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    <input id="manage-check-btn" class="defButton submit-button" type="button" value="@Message.getMessage(CommonLabel.BUTTON_MANAGEBOOK_CHECK)" onclick="btnManageBookCheck_onclick()">
                    <input id="report-btn" class="defButton submit-button" type="button" value="@Message.getMessage(CommonLabel.BUTTON_REPORT_OUTPUT)" onclick="btnReportOutput_onclick()">
                </div>
            </div>
        </div>
        @* 出力条件 *@
        @<div Class="search-wrap">
            <a id="SearchCondition" data-toggle="collapse" href="#cond" aria-expanded="true" onclick="SearchCondition_onclick()">
                @Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)
            </a>
            <div class="search-border">
                <div id="cond" class="container collapse in">
                    <div class="row" id="item-conditions">
                        <div class="col-md-12">
                            <div class="col-sm-3">
                                <div class="grid-head-col"><div width="20" class="require" style="display:inline-block;">※</div>@Message.getMessage(CommonLabel.LABEL_PERIOD)</div>
                                <div class="grid-condition-col-common">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchDateFrom, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                    @Html.Raw("～")
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchDateTo, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="row" id="item-conditions">
                        <div class="col-md-12">
                            @* ロケーション 保管場所 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col"><span width="20" class="require" style="display:inline-block;">※</span>@(Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) + " " + Message.getMessage(CommonLabel.LABEL_STORAGE_LOCATION))</div>
                                <div Class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocL, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeLocationL(this)", .class = "cond-control"})
                                </div>
                            </div>
                            @* ロケーション 階 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col">@(Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) + " " + Message.getMessage(CommonLabel.LABEL_FLOOR))</div>
                                <div Class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocM, DirectCast(ViewBag.modelLocM, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeLocationM(this)", .class = "cond-control"})
                                </div>
                            </div>
                            @* ロケーション 棚 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col">@(Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) + " " + Message.getMessage(CommonLabel.LABEL_SHELF))</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocS, DirectCast(ViewBag.modelLocS, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeLocationS(this)", .class = "cond-control"})
                                </div>
                            </div>
                            @* カテゴリー 大区分 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeCategoryL(this)", .class = "cond-control"})
                                </div>
                            </div>
                            @* カテゴリー中分類 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_M)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatM, DirectCast(ViewBag.modelCatM, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeCategoryM(this)", .class = "cond-control"})
                                </div>
                            </div>
                            @* カテゴリー小分類 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_S)</div>
                                <div class="grid-condition-col">
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchCatS, DirectCast(ViewBag.modelCatS, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeCategoryS(this)", .class = "cond-control"})
                                </div>
                            </div>
                        </div>
                    </div>

                    @* 資産指定ボタン *@
                    <div Class="detail-row" style="display:@(If(Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag = "1" OrElse Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag = "1", "display", "None"))">
                        <div class="condition-col" style="width:150px">
                            <a href="#" id="DetailAccordion" onclick="DetailAccordion_onclick();return false;">@Message.getMessage(CommonLabel.LABEL_DRUG_SPECIFY_OEPN)</a>
                        </div>
                    </div>
                    @* 資産指定 *@
                    <table Class="detail-table">
                        <tbody class="detail-accordion" style="display:none;">
                            <tr class="row" id="item-conditions">
                                <td class="col-md-12">
                                    @* 資産番号 *@
                                    <div class="col-sm-5" style="display:@(If(Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag = "1", "display", "None"))">
                                        <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_DRUG_NO)</div>
                                        <div class="grid-condition-col">
                                            @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchDrugNo, New With {.class = "cond-control", .maxlength = 24})
                                        </div>
                                    </div>
                                    @* 資産名称 *@
                                    <div class="col-sm-5" style="display:@(If(Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag = "1", "display", "None"))">
                                        <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_DRUG_NAME)</div>
                                        <div class="grid-condition-col">
                                            @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchDrugName, New With {.class = "cond-control", .maxlength = 60})
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    @* 検索ボタン *@
                    <div Class="view-area-label" style="height: 25px"></div>
                    <input Class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>

        @*明細データチェックボックス選択値の引き渡し用隠し項目*@
        @<input id="SelectKeys" name="SelectKeys" type="hidden">
        @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden">

        @<div class="drug_specify">

            @*ページャー表示*@
            @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
            @Code
                Dim lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
                Dim CheckedCount As String = "0"
                If Not IsNothing(lstCheckedKey) Then
                    CheckedCount = lstCheckedKey.Count.ToString
                End If
            @Html.Label(CheckedCount, New With {.id = "selectcount", .style = "display:none"})
            End Code

            @Code
                Dim AllSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG)
                Dim sDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                If AllSelectFlg Then
                    sDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                End If
            End Code
            <input Class="defButton" type="button" id="btnSelectAll" style="margin-bottom:10px" value="@sDispStr" onclick="btnSelectAll_onclick()">
        </div>
        @<div Class="list-view">
            <div id="ListGrids" style="width:100%;height:auto;" />
        </div>
        

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OpenFlg)
        End Using

    </div>