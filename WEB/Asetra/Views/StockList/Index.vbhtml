@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@Imports System.ComponentModel
@ModelType List(Of StockListViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelStockList.TITLE)
End Code
<script>
    //
    //  在庫照会
    //
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

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        // ロケーション大区分非選択時は中区分を非活性（中区分・小区分は複数選択）
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocL").value == "") {
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        // ロケーション中区分非選択時は小区分を非活性
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

        // 列定義
        let columns = [
            { id: "@ColKey.StockList.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.STOCK_LIST", "@ColKey.StockList.CHECK_SELECT", 40), sortOrder: -1, formatter: DepCheckboxFormatter, cssClass: "first-column" },
            { id: "@ColKey.StockList.MNG_LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_MNG_LOC_L_CD_NAME)", field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.STOCK_LIST", "@ColKey.StockList.MNG_LOC_L_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag", formatter: MngLocCdFormatter },
            { id: "@ColKey.StockList.LOC_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_L_COM_NAME)", field: "LOC_L_CD", width: getColumnWidth("@DispId.STOCK_LIST", "@ColKey.StockList.LOC_L_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag", formatter: LocLCdFormatter },
            { id: "@ColKey.StockList.LOC_M_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_NAME)", field: "LOC_M_CD", width: getColumnWidth("@DispId.STOCK_LIST", "@ColKey.StockList.LOC_M_CD", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispFlag", formatter: LocMCdFormatter },
            { id: "@ColKey.StockList.LOC_S_CD", name: "@Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_NAME)", field: "LOC_S_CD", width: getColumnWidth("@DispId.STOCK_LIST", "@ColKey.StockList.LOC_S_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispFlag", formatter: LocSCdFormatter },
            { id: "@ColKey.StockList.CAT_L_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)", field: "CAT_L_CD", width: getColumnWidth("@DispId.STOCK_LIST", "@ColKey.StockList.CAT_L_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag", formatter: CatLCdFormatter },
            { id: "@ColKey.StockList.CAT_M_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_M_CD_NAME)", field: "CAT_M_CD", width: getColumnWidth("@DispId.STOCK_LIST", "@ColKey.StockList.CAT_M_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispFlag", formatter: CatMCdFormatter },
            { id: "@ColKey.StockList.CAT_S_CD", name: "@Message.getMessage(CommonLabel.LABEL_CAT_S_CD_NAME)", field: "CAT_S_CD", width: getColumnWidth("@DispId.STOCK_LIST", "@ColKey.StockList.CAT_S_CD", 240), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispFlag", formatter: CatSCdFormatter },
            { id: "@ColKey.StockList.QTY", name: "@Message.getMessage(CommonLabel.LABEL_QUANTITY)", field: "QTY", width: getColumnWidth("@DispId.STOCK_LIST", "@ColKey.StockList.QTY", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("QTY").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("QTY").DispFlag", formatter: QtyFormatter },
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

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // Grid表示判定
        if ("@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "KEY_VALUE");
            dataView.endUpdate();
        }

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");

        if (fixCol > 0) {
            fixCol = fixCol + 1       //選択列と変更列を加算
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.STOCK_LIST", fixCol);

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");

        //ローカルストレージから復元
        const saved = localStorage.getItem("StockListSearchConditions");
        if("@Model.FirstOrDefault.IsRestoreOwnSearchConditions" == "True") {
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
        let cond = {SearchLocL: document.getElementById("SearchLocL").value,};

        //ローカルストレージを保存
        localStorage.setItem("StockListSearchConditions", JSON.stringify(cond));
    }

    //選択用formatter
    function DepCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:center; height:100%;">' +
               '<input type="checkbox" class="slick-checkbox" ' +
               'value="' + dataContext.KEY_VALUE + '" ' +
               (value == 1 ? 'checked="checked"' : '') + ' />' +
               '</div>';
    }

    // 管理ロケーションコード/名formatter
    function MngLocCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.MNG_LOC_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.MNG_LOC_L_NAME)
    }

    // ロケーション大区分コード/名formatter
    function LocLCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LOC_L_NAME)
    }

    // ロケーション中区分コード/名formatter
    function LocMCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_M_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LOC_M_NAME)
    }

    // ロケーション小区分コード/名formatter
    function LocSCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.LOC_S_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.LOC_S_NAME)
    }

    // カテゴリー大分類コード/名formatter
    function CatLCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_L_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CAT_L_NAME)
    }

    // カテゴリー中分類コード/名formatter
    function CatMCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_M_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CAT_M_NAME)
    }

    // カテゴリー小分類コード/名formatter
    function CatSCdFormatter(row, cell, value, columnDef, dataContext) {
        return convertToEmptyIfNull(dataContext.CAT_S_CD) + "@CommonConst.CodeNameDelimiter" + convertToEmptyIfNull(dataContext.CAT_S_NAME)
    }

    // 数量formatter
    function QtyFormatter(row, cell, value, columnDef, dataContext) {
        return '<div style="text-align:right; width:100%;">' + dataContext.QUANTITY + '</div>'
    }

    // 資産詳細ボタンクリック処理
    function btnShowAssetHistory_onclick() {
        // 選択行の開始宣言日時を取得
        var keyValue = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                var row = checkBoxes[i].parentNode.parentNode;
                keyValue.push(checkBoxes[i].value);
            }
        }
        if (keyValue.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        } else if (keyValue.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }
        document.getElementById("keyValue").value = keyValue[0];
        // 実行
        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/StockList")/ShowAssetHistory");
        $('#form').submit();
        $('#form').attr("action", befAction);
        return true;
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        return showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/StockList")/CsvExport");
    }

    //検索ボタンクリック処理
    function btnSearch_onclick() {
        // ロケーション(会社)必須チェック
        var locL = document.getElementById("SearchLocL").value;
        if (locL.trim() === "") {
            showErrMessage("@Message.getMessage(CommonErrorMsg.MSG_ERRNO_SELECTED_LOC_L)")
            return false;
        }

        //画面ブロックを表示
        showLoading();

        //ページ番号のリセット
        ResetPageNum();

        //ローカルストレージに検索条件保存
        localStorageSave();

        return true;
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        //画面ブロックを表示
        showLoading();

        $('#form').submit();   //2020/02/20 nv miyaji
    }

    //---------- ロケーション
    //大区分選択
    function ChangeLocationL(locL, initialM, initialS, doAutoSearch) {
        //[CC431] UPD Start 20261007
        // 中区分・小区分をクリアし、場所から選ぶの ON／OFF に合わせて選択肢を取得する（中区分・小区分は複数選択）
        // ※初期表示では initialM／initialS の選択値を選び直す
        resetSearchLocOptions('Loc', initialM, initialS, function () {
            // 検索処理実行（初期表示時のみ。×で選択解除した場合は実行しない）
            if (doAutoSearch && document.getElementById('FirstSearch').value == "True") {
                document.getElementById('Search_Button').click();
            }
        });
        //[CC431] UPD End 20261007
    }

    //中区分選択
    function ChangeLocationM(locM, initialS, doAutoSearch) {
        var lCode = document.getElementById("SearchLocL").value;

        // 選択中の中区分コード（複数選択のため「,」区切りで渡す）
        var mCode = getMultiSelectValues(locM).join(",");

        // 小区分をクリア
        clearMultiSelect("#SearchLocS");
        $("#SearchLocS").prop("disabled", true);

        // 選択されていない場合はここで終了
        if (!mCode) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/StockList")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationS", selectLCode: lCode, selectMCode: mCode },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocS = document.getElementById("SearchLocS");
                    SearchLocS.options.length = 0;

                    // 複数選択のため、選択解除用の空の選択肢は不要
                    for (var i = 0; i < data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data[i].LOC_CD;
                        op.text = data[i].LOC_NAME;
                        SearchLocS.appendChild(op);
                    }
                }
                // 小区分データ取得
                $("#SearchLocS").removeAttr("disabled");
                // 選択肢の入れ替えをSelect2に反映（onchange は発生させない）
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
            url: '@Url.Content("~/StockList")/GetSearchLocationOptions',
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

    //検索条件リンククリック時処理
    @*function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
    }*@

</script>

<div id="stock-list">
    @Using (Html.BeginForm("Index", "StockList", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    @*<input class="defButton asset-detail-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_ASSET_DETAIL)" onclick="btnShowAssetHistory_onclick();">*@
                    @*<input class="defButton asset-detail-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_ASSET_DETAIL)" onclick="btnShowAssetHistory_onclick();location.href='@Url.Action("Index", "AssetList", New With {.keyValue = "GetKeyValue()"})'">*@
                    @*Html.ActionLink("Index", "AssetList", routeValues:=Nothing, htmlAttributes:=New With {.keyValue = "GetKeyValue()"})*@
                    <input class="defButton asset-detail-btn" type="button" value="@Message.getMessage(CommonLabel.BUTTON_ASSET_DETAIL)" onclick="btnShowAssetHistory_onclick()">
                    <input class="defButton csv-Export-btn  submit-button" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
                </div>
            </div>
        </div>
        @* 検索条件 *@
        @<div Class="search-wrap">
            <a id="SearchCondition" data-toggle="collapse" href="#cond" aria-expanded="true" onclick="SearchCondition_onclick()">
                @Message.getMessage(CommonLabel.TITLE_SEARCH)
            </a>
            <div class="search-border">
                <div id="cond" class="container collapse in">
                    @* condition-flex：複数選択で項目の高さが伸びても段が崩れないよう flex で並べる（Content\Site.css） *@
                    @*[CC431] UPD Start 20261013 condition-flex を row ではなく、他の画面と同じ row 直下の col-md-12 に付ける（Site.css の指定が当たらず、資産名称が資産番号の下に回り込んでいたため）*@
                    <div class="row" id="item-conditions">
                    <div class="col-md-12 condition-flex">
                    @*[CC431] UPD End 20261013*@
                        @* ロケーション 大区分*@
                        <div class="col-sm-2 filter-combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L)<font Class="label-require">※</font></div>
                            <div Class="grid-condition-col">
                                @* select2-clearable クラスで入力して絞り込めるSelect2になる（Scripts\select2-common.js）。選択肢・選択後とも「コード 名称」を表示 *@
                                @* ×で選択解除するため、空の選択肢を追加している *@
                                @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocL, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeLocationL(this)", .class = "cond-control select2-clearable"})
                            </div>
                        </div>
                        @* ロケーション 中区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area）*@
                        <div class="col-sm-2 combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag = "1" AndAlso Model.FirstOrDefault.GridInfo("LOC_M_CD").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_M)</div>
                            <div Class="grid-condition-col">
                                @* ListBoxFor で複数選択（select multiple）。select2-clearable クラスで×ボタン付きSelect2になる。空の選択肢は不要 *@
                                @*[CC431] UPD Start 20261007*@
                                @* 選択値を変えたら、場所から選ぶ OFF のとき場所の選択肢を作り直す *@
                                @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchLocM, DirectCast(ViewBag.modelLocM, IEnumerable(Of SelectListItem)), New With {.onchange = "onSearchLocChange('Loc', '" & LOC_DIV.LocationM & "')", .class = "cond-control select2-clearable"})
                                @*[CC431] UPD End 20261007*@
                            </div>
                        </div>
                        @* ロケーション 小区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area）*@
                        <div class="col-sm-2 combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag = "1" AndAlso Model.FirstOrDefault.GridInfo("LOC_M_CD").DispFlag = "1" AndAlso Model.FirstOrDefault.GridInfo("LOC_S_CD").DispFlag = "1", "display", "None"))">
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
                        @* カテゴリー 大区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area）*@
                        <div class="col-sm-2 combo-area" style="display:@(If(Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag = "1", "display", "None"))">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                            <div Class="grid-condition-col">
                                @* ListBoxFor で複数選択（select multiple）。select2-clearable クラスで×ボタン付きSelect2になる。空の選択肢は不要 *@
                                @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), New With {.class = "cond-control select2-clearable"})
                            </div>
                        </div>
                        @* 資産番号 *@
                        <div class="col-sm-2">
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
                        @* 資産ID(EPC) *@
                        <div class="col-sm-2">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_ID)</div>
                            <div class="grid-condition-col">
                                @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetID, New With {.class = "cond-control", .maxlength = 128})
                            </div>
                        </div>
                        @* 資産名称 *@
                        <div class="col-sm-2">
                            <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</div>
                            <div class="grid-condition-col">
                                @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetName, New With {.class = "cond-control", .maxlength = 60})
                            </div>
                        </div>
                    @*[CC431] ADD Start 20261013 col-md-12 condition-flex の閉じタグ*@
                    </div>
                    @*[CC431] ADD End 20261013*@
                    </div>
                    <div class="row" id="item-conditions">
                    @*[CC431] ADD Start 20261013 1 行目と左端の位置を揃えるため、同じ col-md-12 condition-flex に入れる*@
                    <div class="col-md-12 condition-flex">
                    @*[CC431] ADD End 20261013*@
                        @* 集約表示 ロケーション *@
                        <div class="col-sm-2">
                            <div class="grid-head-col">@(Message.getMessage(CommonLabel.LABEL_SUM_DISP) + " " + Message.getMessage(CommonLabel.LABEL_GEN_LOCATION))</div>
                            <div class="grid-condition-col">
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelectLocation, LOC_DIV.LocationL, New With {.id = "LOC_DIV_L"}) @Message.getMessage(CommonLabel.VALUE_LOC_L_COM)</div>
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelectLocation, LOC_DIV.LocationM, New With {.id = "LOC_DIV_M"}) @Message.getMessage(CommonLabel.VALUE_LOC_M_DEP)</div>
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelectLocation, LOC_DIV.LocationS, New With {.id = "LOC_DIV_S"}) @Message.getMessage(CommonLabel.VALUE_LOC_S_PLA)</div>
                            </div>
                        </div>
                        @* 集約表示 カテゴリー *@
                        <div class="col-sm-2">
                            <div class="grid-head-col">@(Message.getMessage(CommonLabel.LABEL_SUM_DISP) + " " + Message.getMessage(CommonLabel.LABEL_CATEGORY))</div>
                            <div class="grid-condition-col">
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelectCategory, CAT_DIV.CategoryL, New With {.id = "CAT_DIV_L"}) @Message.getMessage(CommonLabel.VALUE_CAT_L)</div>
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelectCategory, CAT_DIV.CategoryM, New With {.id = "CAT_DIV_M"}) @Message.getMessage(CommonLabel.VALUE_CAT_M)</div>
                                <div class="input-select-item">@Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchDispSelectCategory, CAT_DIV.CategoryS, New With {.id = "CAT_DIV_S"}) @Message.getMessage(CommonLabel.VALUE_CAT_S)</div>
                            </div>
                        </div>
                    @*[CC431] ADD Start 20261013 col-md-12 condition-flex の閉じタグ*@
                    </div>
                    @*[CC431] ADD End 20261013*@
                    </div>
                    <div>
                        <input Class="float-right defButton submit-button" style="margin-top: -25px;" id="Search_Button" name="actionname" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                    </div>
                </div>
            </div>
        </div>

        @<input type="hidden" id="IsRestoreOwnSearchConditions" name="IsRestoreOwnSearchConditions" value="@Model.FirstOrDefault().IsRestoreOwnSearchConditions.ToString()" />
        @<input type="hidden" id="FirstSearch" name="FirstSearch" value="@Model.FirstOrDefault().FirstSearch.ToString()" />

        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div>
            <div class="list-view">
                <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
            </div>
        </div>
        'チェックボックス選択値の引き渡し用隠し項目
        @<input id="keyValue" name="keyValue" type="hidden">


        'CSV出力のモーダル画面
        @<div id="modal-csv-content">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title-csv"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
            @CODE
                Dim cnt As Integer = 1
                'モデルの値を設定
                Dim csvSessionModel As StockListViewModel = Session(CommonSessionKey.CREAGE_STOCK_LIST)
                If Not IsNothing(csvSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(csvSessionModel)
                Else
                    Model.Add(New StockListViewModel())
                End If
            END CODE
            @Html.AntiForgeryToken()
            <div Class="PopupButton" style="margin-bottom: 20px;">
                <div style="float:left;">
                    <input type="file" name="CsvFile" id="CsvFile" />
                </div>
                <div style="float:right;">
                    <input id="modal-import-btn" class="defButton modal-close CsvImport submit-button" style="width:120px" formaction="~/StockList/CsvImport" type="submit" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)">
                </div>
                <div style="clear:both;"></div>
            </div>
            <div class="ichiran">
                <div id="div_koumoku_h">
                    <table class="ich_table" id="csv_tbl_koumoku_h">
                        <tbody>
                            <tr>
                                <th class="col1"></th>
                                <th class="col2">@Message.getMessage(CommonLabel.LABEL_ROW)</th>
                                <th class="col3">@Message.getMessage(CommonLabel.LABEL_DATA)</th>
                                <th class="col2">@Message.getMessage(CommonLabel.LABEL_ERRORGAIYOU)</th>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <div id="div_koumoku_d">
                    <table class="ich_table" id="csv_tbl_koumoku_d">
                        <tbody>
                            @If Model.FirstOrDefault.TotalCount > 0 Then
                                @For Each item In Model
                                    @<tr>
                                        <td class="col1">@cnt</td>
                                        <td class="col2" title="@item.LOC_L_CD">
                                            @Html.DisplayFor(Function(modelItem) item.LOC_L_CD)
                                        </td>
                                        <td class="col3"></td>
                                        <td class="col2"></td>
                                    </tr>
                                Next
                            End If
                        </tbody>
                    </table>
                </div>
            </div>

            <div id="modal-msg" style="display:inline-block;">
                @*"@ViewBag.ModalMsg"*@
            </div>

            <div Class="PopupButton" style="float:right; margin:15px 0 0 0;">
                <input id="csvimport-modal-close" class="defButton modal-close" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="fileNmClear()">
            </div>
        </div>

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.SearchDispSelectLocation)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.SearchDispSelectCategory)

    End Using
</div>
