@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@ModelType List(Of GoodsViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelGoods.TITLE)
End Code
<script>
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            //エラーメッセージ表示後、ライセンスコントローラーを呼び出す
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            return;
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        btnCsvImportEnabled(true)       // 2022/7/25 sv sato add

        // 列定義
        let columns = [
            { id: "@ColKey.GOODS.PRODUCT_NO", name: '@Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)', field: "PRODUCT_NO", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.PRODUCT_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PRODUCT_NO").DispFlag"},
            { id: "@ColKey.GOODS.ASSET_NAME", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)', field: "ASSET_NAME", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.ASSET_NAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM1", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_KOUNYU_DATE)', field: "FREE_ITEM1", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM1", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispFlag"},
            { id: "@ColKey.GOODS.MNG_LOC_L_CD", name: '@Message.getMessage(CommonLabel.LABEL_MNG_LOC_L_CD_NAME)', field: "MNG_LOC_L_CD", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.MNG_LOC_L_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MNG_LOC_L_CD").DispFlag", formatter: MngLocLCdFormatter},
            { id: "@ColKey.GOODS.LOC_L_CD", name: '@Message.getMessage(CommonLabel.LABEL_LOC_L_CD_NAME)', field: "LOC_L_CD", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.LOC_L_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_L_CD").DispFlag", formatter: LocLCdFormatter},
            { id: "@ColKey.GOODS.LOC_M_CD", name: '@Message.getMessage(CommonLabel.LABEL_LOC_M_CD_NAME)', field: "LOC_M_CD", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.LOC_M_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_M_CD").DispFlag", formatter: LocMCdFormatter},
            { id: "@ColKey.GOODS.LOC_S_CD", name: '@Message.getMessage(CommonLabel.LABEL_LOC_S_CD_NAME)', field: "LOC_S_CD", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.LOC_S_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOC_S_CD").DispFlag", formatter: LocSCdFormatter},
            { id: "@ColKey.GOODS.CAT_L_CD", name: '@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)', field: "CAT_L_CD", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.CAT_L_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag", formatter: CatLCdFormatter},
            { id: "@ColKey.GOODS.CAT_M_CD", name: '@Message.getMessage(CommonLabel.LABEL_CAT_M_CD_NAME)', field: "CAT_M_CD", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.CAT_M_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_M_CD").DispFlag", formatter: CatMCdFormatter},
            { id: "@ColKey.GOODS.CAT_S_CD", name: '@Message.getMessage(CommonLabel.LABEL_CAT_S_CD_NAME)', field: "CAT_S_CD", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.CAT_M_CD", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_S_CD").DispFlag", formatter: CatSCdFormatter},
            { id: "@ColKey.GOODS.GENERIC_LIMIT", name: '@Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)', field: "GENERIC_LIMIT", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.GENERIC_LIMIT", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("GENERIC_LIMIT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("GENERIC_LIMIT").DispFlag", formatter: GenericLimitFormatter},
            { id: "@ColKey.GOODS.MAKER", name: '@Message.getMessage(CommonLabel.LABEL_MAKER)', field: "MAKER", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.MAKER", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("MAKER").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MAKER").DispFlag"},
            { id: "@ColKey.GOODS.MODEL_NO", name: '@Message.getMessage(CommonLabel.LABEL_MODEL_NO)', field: "MODEL_NO", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.MODEL_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("MODEL_NO").DispFlag"},
            { id: "@ColKey.GOODS.ASSET_DIV", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_DIV)', field: "ASSET_DIV", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.ASSET_DIV", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_DIV").DispFlag"},
            { id: "@ColKey.GOODS.LEASE_NO", name: '@Message.getMessage(CommonLabel.LABEL_LEASE_NO)', field: "LEASE_NO", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.LEASE_NO", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_NO").DispFlag"},
            { id: "@ColKey.GOODS.LEASE_LIMIT", name: '@Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)', field: "LEASE_LIMIT", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.LEASE_LIMIT", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("LEASE_LIMIT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LEASE_LIMIT").DispFlag", formatter: LeaseLimitFormatter},
            { id: "@ColKey.GOODS.FREE_ITEM2", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM2)', field: "FREE_ITEM2", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM2", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM3", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM3)', field: "FREE_ITEM3", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM3", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM4", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM4)', field: "FREE_ITEM4", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM4", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM5", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM5)', field: "FREE_ITEM5", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM5", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM6", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM6)', field: "FREE_ITEM6", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM6", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM7", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM7)', field: "FREE_ITEM7", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM7", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM8", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM8)', field: "FREE_ITEM8", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM8", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM9", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM9)', field: "FREE_ITEM9", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM9", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM10", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM10)', field: "FREE_ITEM10", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM10", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM11", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM11)', field: "FREE_ITEM11", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM11", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM12", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM12)', field: "FREE_ITEM12", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM12", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM13", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM13)', field: "FREE_ITEM13", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM13", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM14", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM14)', field: "FREE_ITEM14", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM14", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM15", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM15)', field: "FREE_ITEM15", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM15", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM16", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM16)', field: "FREE_ITEM16", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM16", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM17", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM17)', field: "FREE_ITEM17", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM17", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM18", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM18)', field: "FREE_ITEM18", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM18", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispFlag"},
            { id: "@ColKey.GOODS.FREE_ITEM19", name: '@Message.getMessage(CommonLabel.LABEL_FREE_ITEM19)', field: "FREE_ITEM19", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.FREE_ITEM19", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispFlag"},
            { id: "@ColKey.GOODS.ASSET_PIC_FILENAME", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_PIC_FILENAME)', field: "ASSET_PIC_FILENAME", width: getColumnWidth("@DispId.GOODS", "@ColKey.GOODS.ASSET_PIC_FILENAME", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_PIC_FILENAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_PIC_FILENAME").DispFlag"},
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
            fixCol = fixCol       //選択列を加算
        }

        // Gridのデータ管理オブジェクトにセット
        let dataView = new Slick.Data.DataView();

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            dataView.beginUpdate();
            dataView.setItems(jsonData, "PRODUCT_NO");
            dataView.endUpdate();
        }

        // Grid初期化
        grid = initializeGrid("ListGrids", dataView, columns, options, "@DispId.GOODS", fixCol);

    });

    //管理ロケーションコード/名用formatter
    function MngLocLCdFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (convertToEmptyIfNull(dataContext.MNG_LOC_L_CD) != ' ') {
            status = dataContext.MNG_LOC_L_CD + ' '
            if (convertToEmptyIfNull(dataContext.MngLocLName) != '') {
                status += dataContext.MngLocLName
            }
        }
        if (status != '  ') {
            return status
        }
    }

    //ロケーション大区分コード/名用formatter
    function LocLCdFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (convertToEmptyIfNull(dataContext.LOC_L_CD) != ' ') {
            status = dataContext.LOC_L_CD + ' '
            if (convertToEmptyIfNull(dataContext.LocLName) != '') {
                status += dataContext.LocLName
            }
        }
        if (status != '  ') {
            return status
        }
    }

    //ロケーション中区分コード/名用formatter
    function LocMCdFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (convertToEmptyIfNull(dataContext.LOC_M_CD) != '') {
            status = dataContext.LOC_M_CD + ' '
            if (convertToEmptyIfNull(dataContext.LocMName) != '') {
                status += dataContext.LocMName
            }
        }
        if (status != '') {
            return status
        }
    }

    //ロケーション小区分コード/名用formatter
    function LocSCdFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (convertToEmptyIfNull(dataContext.LOC_S_CD) != '') {
            status = dataContext.LOC_S_CD + ' '
            if (convertToEmptyIfNull(dataContext.LocSName) != '') {
                status += dataContext.LocSName
            }
        }
        if (status != '') {
            return status
        }
    }

    //カテゴリー大分類コード/名用formatter
    function CatLCdFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (convertToEmptyIfNull(dataContext.CAT_L_CD) != ' ') {
            status = dataContext.CAT_L_CD + ' '
            if (convertToEmptyIfNull(dataContext.CatLName) != '') {
                status += dataContext.CatLName
            }
        }
        if (status != '  ') {
            return status
        }
    }

    //カテゴリー中分類コード/名用formatter
    function CatMCdFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (convertToEmptyIfNull(dataContext.CAT_M_CD) != '') {
            status = dataContext.CAT_M_CD + ' '
            if (convertToEmptyIfNull(dataContext.CatMName) != '') {
                status += dataContext.CatMName
            }
        }
        if (status != '') {
            return status
        }
    }

    //カテゴリー小分類コード/名用formatter
    function CatSCdFormatter(row, cell, value, columnDef, dataContext) {
        let status = ''
        if (convertToEmptyIfNull(dataContext.CAT_S_CD) != '') {
            status = dataContext.CAT_S_CD + ' '
            if (convertToEmptyIfNull(dataContext.CatSName) != '') {
                status += dataContext.CatSName
            }
        }
        if (status != '') {
            return status
        }
    }

    //○○期限用formatter GENERIC_LIMIT
    function GenericLimitFormatter(row, cell, value, columnDef, dataContext) {
        let status = String(value)
        if (convertToEmptyIfNull(status)) {
            if (status.length == 8) {
                let yyyy = status.substr(0, 4) + '/'
                let mm = status.substr(4, 2) + '/'
                let dd = status.substr(6, 2)

                return yyyy + mm + dd
            }
        }
    }

    //リース期限用formatter	 LEASE_LIMIT
    function LeaseLimitFormatter(row, cell, value, columnDef, dataContext) {
        let status = String(value)
        if (convertToEmptyIfNull(status)) {
            if (status.length == 8) {
                let yyyy = status.substr(0, 4) + '/'
                let mm = status.substr(4, 2) + '/'
                let dd = status.substr(6, 2)

                return yyyy + mm + dd
            }
        }
    }


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

    //CSVデータ取込画面　取込開始ボタンクリック処理
    function btnCsvImportStart_onclick() {
        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.OK";
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
            url: '@Url.Content("~/Goods")/ImportCsvFile',
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
                                $("#csv-err-ichiran").append('<tr><td class="col1" title=' + data[icnt].ErrRowIdx + '>'+ data[icnt].ErrRowIdx +'</td><td class="col2" title=' + data[icnt].ErrKoumoku + '>'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" title=' + data[icnt].ErrGaiyou + '>'+ data[icnt].ErrGaiyou +'</td></tr>');
                            }

                        }
                        break;
                    case @DataInport.DataInportResultCd.LoginSession:
                        // ログインセッション切れ
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_LOGININFO_EMPTY)";
                        break;
                    case @DataInport.DataInportResultCd.CsvMapMei:
                        // CSVマッピング明細マスタエラー
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_CSVMAPMEI)";
                        break;
                    case @DataInport.DataInportResultCd.CsvMapCharCd:
                        // CSVマッピングマスタ文字コードエラー
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_CHARCD)";
                        break;
                    case @DataInport.DataInportResultCd.SettingInfo:
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

    //全データCSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        showConfirm("@Message.getMessage(CommonInfoMsg.CSVEXPORT_CONFIRM)", "@Url.Content("~/Goods")/CsvExport");

    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;

        $('#form').submit();
    }

</script>

<div id="goods-master">
    @Using (Html.BeginForm("Index", "Goods", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20">
            <div id="view-area-button-parent">
                <div class="view-area-button-right">
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @* CSVデータ取込 *@
                        @<input Class="defButton" style="width:120px;" type="button" value="@Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)" onclick="return btnCsvImport_onclick()" />
                    End If
                    <input class="defButton csv-Export-btn  submit-button" type="button" name="CsvExport" style="width:120px" value="@Message.getMessage(CommonLabel.BUTTON_CSVEXPORT)" onclick="btnCsvExport_onclick()">
                </div>
            </div>
        </div>
        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @<div class="list-view">
            <div id="ListGrids" style="height:auto;padding-bottom: 20px;" />
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
                Dim csvSessionModel As GoodsViewModel = Session(CommonSessionKey.CREAGE_GOODS)
                If Not IsNothing(csvSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(csvSessionModel)
                Else
                    Model.Add(New GoodsViewModel())
                End If
            END CODE
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

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)

     End Using
</div>
