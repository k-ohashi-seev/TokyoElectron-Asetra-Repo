@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of AssetLeaseHitoryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelAssetLeaseHitory.TITLE)
End Code
<script>
    //
    //貸出履歴照会画面
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

            //上限を超えている場合はCSV出力できないようにする。
            $("#csvBtn").prop("disabled", true);
            $("#csvBtn").attr('style', 'background-color:#cecece;');
            $("#csvBtn").css("cursor", "default");
        }
        //20230904 ADD End Shirakawa

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

        // 列定義
        let columns = [
            { id: "@ColKey.AssetLeaseHistory.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_MNG_LOC_L_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.MNG_LOC_L_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag", formatter: MngLocLFormatter },
            { id: "@ColKey.AssetLeaseHistory.CAT_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)", field: "CAT_L_CD", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.CAT_L_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag", formatter: CatLFormatter },
            { id: "@ColKey.AssetLeaseHistory.CAT_M_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_M_CD_NAME)", field: "CAT_M_CD", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.CAT_M_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispFlag", formatter: CatMFormatter },
            { id: "@ColKey.AssetLeaseHistory.CAT_S_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_S_CD_NAME)", field: "CAT_S_CD", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.CAT_S_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispFlag", formatter: CatSFormatter },
            { id: "@ColKey.AssetLeaseHistory.ASSET_NO", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)", field: "ASSET_NO", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.ASSET_NO", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag" },
            { id: "@ColKey.AssetLeaseHistory.ASSET_NAME", name: "@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)", field: "ASSET_NAME", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.ASSET_NAME", 250), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag" },
            { id: "@ColKey.AssetLeaseHistory.DISPOSE", name: "@Message.getMessage(CommonLabel.LABEL_DISPOSE)", field: "DISPOSE", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.DISPOSE", 50), sortOrder: "@Model.FirstOrDefault.GridInfo("DISPOSE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DISPOSE").DispFlag", formatter: CenterAlignFormatter },
            { id: "@ColKey.AssetLeaseHistory.LEASE_CNT", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_COUNT)", field: "LEASE_CNT", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.LEASE_CNT", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_CNT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_CNT").DispFlag", formatter: RightAlignIfNullFormatter },
            { id: "@ColKey.AssetLeaseHistory.LEASE_DAY", name: "@Message.getMessage(CommonLabel.LABEL_LEASE_DAY)", field: "LEASE_DAY", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.LEASE_DAY", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_DAY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_DAY").DispFlag", formatter: RightAlignIfNullFormatter },
            { id: "@ColKey.AssetLeaseHistory.MAINTE_DAY", name: "@Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)", field: "MAINTE_DAY", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.MAINTE_DAY", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("MAINTE_DAY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAINTE_DAY").DispFlag", formatter: RightAlignIfNullFormatter },
            { id: "@ColKey.AssetLeaseHistory.EIGYO_DAY", name: "@Message.getMessage(CommonLabel.LABEL_EIGYO_DAY)", field: "EIGYO_DAY", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.EIGYO_DAY", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("EIGYO_DAY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("EIGYO_DAY").DispFlag", formatter: RightAlignIfNullFormatter },
            { id: "@ColKey.AssetLeaseHistory.LENDING_RATE", name: "@Message.getMessage(CommonLabel.LABEL_LENDING_RATE)", field: "LENDING_RATE", width: getColumnWidth("@DispId.ASSET_LEASE_HITORY", "@ColKey.AssetLeaseHistory.LENDING_RATE", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("LENDING_RATE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LENDING_RATE").DispFlag", formatter: LendingRateFormatter }
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

        let jsonData = []

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            // 安全に使用できるようにJSON文字列をエンコード
            jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");

        grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.ASSET_LEASE_HITORY", fixCol);

    });

    // 管理ロケーションコード/名用formatter
    function MngLocLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.MNG_LOC_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.MNG_LOC_L_NAME);
    }

    // カテゴリ大分類コード/名用formatter
    function CatLFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CAT_L_NAME);
    }

    // カテゴリ中分類コード/名用formatter
    function CatMFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_M_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CAT_M_NAME);
    }

    // カテゴリ小分類コード/名用formatter
    function CatSFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_S_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CAT_S_NAME);
    }

    // 貸出率用formatter
    function LendingRateFormatter(row, cell, value, columnDef, dataContext) {
        let lendingRate = 0;

        if (dataContext.EIGYO_DAY != 0) {
            let leaseDay = Number(dataContext.LEASE_DAY);
            let mainteDay = Number(dataContext.MAINTE_DAY);
            let eigyoDay = Number(dataContext.EIGYO_DAY)

            // 貸出率：(貸出日数 + メンテ日数)/営業日数-　※小数点以下5桁を四捨五入
            lendingRate = toRound((leaseDay + mainteDay) / eigyoDay, 5);
        }

        return '<div style="text-align:right; width:100%;">' + lendingRate + '</div>';
    }

    // テキスト表示(右寄せ)
    function RightAlignIfNullFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:right; width:100%;">' + convertToEmptyIfNull(value) + '</div>';
    }

    // テキスト表示(中央寄せ)
    function CenterAlignFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:center; width:100%;">' + value + '</div>'
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        //抽出期間(From)が未入力の場合はエラー
        let targetPeriodFrom = $("#SearchTargetPeriodFrom").val();
        if (targetPeriodFrom == "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_NOT_ENTERED_DATE_FROM)")
            return false;
        }
        //抽出期間(To)が未入力の場合はエラー
        let targetPeriodTo = $("#SearchTargetPeriodTo").val();
        if (targetPeriodTo == "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_NOT_ENTERED_DATE_TO)")
            return false;
        }
        //抽出期間前後エラー
        targetPeriodFrom = parseFloat(targetPeriodFrom.replace(/-/g, ""));
        targetPeriodTo = parseFloat(targetPeriodTo.replace(/-/g, ""));
        if (targetPeriodFrom > targetPeriodTo) {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")
            return false;
        }

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

        //確認メッセージ
        return showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/AssetLeaseHitory")/CsvExport");
    }

    //検索ボタンクリック処理
    function btnSearch_onclick(element) {
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");

        //抽出期間(From)が未入力の場合はエラー
        let targetPeriodFrom = $("#SearchTargetPeriodFrom").val();
        if (targetPeriodFrom == "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_NOT_ENTERED_DATE_FROM)")
            return false;
        }
        //抽出期間(To)が未入力の場合はエラー
        let targetPeriodTo = $("#SearchTargetPeriodTo").val();
        if (targetPeriodTo == "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_NOT_ENTERED_DATE_TO)")
            return false;
        }
        //抽出期間前後エラー
        targetPeriodFrom = parseFloat(targetPeriodFrom.replace(/-/g, ""));
        targetPeriodTo = parseFloat(targetPeriodTo.replace(/-/g, ""));
        if (targetPeriodFrom > targetPeriodTo) {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERR_DATE_ORDER)")
            return false;
        }

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

        //ページ番号のリセット
        ResetPageNum();

        //20230904 ADD Start Shirakawa
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/AssetLeaseHitory")/SearchClick',
        });
        //20230904 ADD End Shirakawa

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
            url: '@Url.Content("~/AssetLeaseHitory")/SelectAll',
            data: { isSelectAll: chkFlag },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
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
            url: '@Url.Content("~/AssetLeaseHitory")/ChangeCategory',
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
            url: '@Url.Content("~/AssetLeaseHitory")/ChangeCategory',
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

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }

</script>

<div id="asset-lease-hitory">
    @Using (Html.BeginForm("Index", "AssetLeaseHitory", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    <input id="csvBtn" class="defButton csv-Export-btn submit-button" type="button" name="CsvExport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
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
                            @* 抽出期限 *@
                            <div class="col-sm-2">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_EXTRACTION_PERIOD)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchTargetPeriodFrom, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                    @Html.Raw("～")
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchTargetPeriodTo, New With {.htmlAttributes = New With {.class = "cond-control-data", .type = "date"}})
                                </div>
                            </div>
                            @* 除去 *@
                            <div class="col-sm-1" style="max-width:110px">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_DISPOSE)</div>
                                <div class="grid-condition-col" >
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchDispose) @Message.getMessage(CommonLabel.LABEL_FUKUMERU)</div>
                                </div>
                            </div>
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
                        </div>
                    </div>
                    <div class="row" id="item-conditions">
                        @* 管理ロケーション *@
                        <div class="col-md-12">
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_MNG_LOC)</div>
                                <div class="grid-condition-col">
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
                                </div>
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
                    @* 検索ボタン *@
                    <div Class="search-btn">
                        <input id="btn-search" class="defButton submit-button" name="actionname" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick(this)" />
                    </div>
                </div>
            </div>
        </div>

        @*管理ロケーションチェックボックス選択値の引き渡し用隠し項目*@
        @<input id="SelectMngLocKeys" name="SelectMngLocKeys" type="hidden">


        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div class="list-view">
            <div id="ListGrids" style="height:auto;padding-bottom: 20px;" />
        </div>

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
    End Using
</div>
