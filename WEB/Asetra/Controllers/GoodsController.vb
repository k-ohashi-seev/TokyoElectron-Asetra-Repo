Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers
    ''' <summary>
    ''' 商品マスタ画面表示用のクラス
    ''' </summary>
    Public Class GoodsController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _ViewModel As GoodsViewModel = New GoodsViewModel                         'Goods一覧表示用モデル
        Protected _viewResult As List(Of GoodsViewModel) = New List(Of GoodsViewModel)    '結果View
        Private _lstErr As List(Of DataInportResult)
        Private _Message As MessageRfid = Nothing
        Private _dtCsvImport As DataTable = Nothing 'CSV取込用作業テーブル
        Private _lstCsvHed As List(Of String) = Nothing 'CSVヘッダ文字コード格納用リスト
#End Region

#Region "一覧表示処理"
        ''' <summary>
        ''' アクションメソッド(Index)
        ''' スーパークラスの一覧表示処理を呼出します
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns>View表示内容</returns>
        <ValidateInput(False)>
        Public Function Index(ByVal ViewModel As GoodsViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_GOODS)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As GoodsViewModel = Session(CommonSessionKey.CREAGE_GOODS)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '一覧表示処理
                MyBase.BaseIndex(_ViewModel)
                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo

                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '権限情報の取得
                Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Goods, _LoginInfo)
                If authInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                End If
            End If

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.GOODS, "ALL")
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

            '一覧情報にページ情報を設定
            CopyModelData()

            '結果表示
            Return View(_viewResult)

        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()
            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_WORD)
            End If
        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim m = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '一覧表示表用SQL
            sbSQL.Clear()
            sbSQL.Append("SELECT ")
            sbSQL.Append("  GOODS.PRODUCT_NO ")
            sbSQL.Append("  , GOODS.ASSET_DIV ")
            sbSQL.Append("  , GOODS.ASSET_NAME ")
            sbSQL.Append("  , GOODS.MNG_LOC_L_CD ")
            sbSQL.Append("  , GOODS.LOC_L_CD ")
            sbSQL.Append("  , GOODS.LOC_M_CD ")
            sbSQL.Append("  , GOODS.LOC_S_CD ")
            sbSQL.Append("  , GOODS.CAT_L_CD ")
            sbSQL.Append("  , GOODS.CAT_M_CD ")
            sbSQL.Append("  , GOODS.CAT_S_CD ")
            sbSQL.Append("  , GOODS.MAKER ")
            sbSQL.Append("  , GOODS.MODEL_NO ")
            sbSQL.Append("  , GOODS.LEASE_NO ")
            sbSQL.Append("  , GOODS.LEASE_LIMIT ")
            sbSQL.Append("  , GOODS.GENERIC_LIMIT ")
            sbSQL.Append("  , GOODS.FREE_ITEM1 ")
            sbSQL.Append("  , GOODS.FREE_ITEM2 ")
            sbSQL.Append("  , GOODS.FREE_ITEM3 ")
            sbSQL.Append("  , GOODS.FREE_ITEM4 ")
            sbSQL.Append("  , GOODS.FREE_ITEM5 ")
            sbSQL.Append("  , GOODS.FREE_ITEM6 ")
            sbSQL.Append("  , GOODS.FREE_ITEM7 ")
            sbSQL.Append("  , GOODS.FREE_ITEM8 ")
            sbSQL.Append("  , GOODS.FREE_ITEM9 ")
            sbSQL.Append("  , GOODS.FREE_ITEM10 ")
            '2023/03/22 Kawamoto ADD START
            sbSQL.Append("  , GOODS.FREE_ITEM11 ")
            sbSQL.Append("  , GOODS.FREE_ITEM12 ")
            sbSQL.Append("  , GOODS.FREE_ITEM13 ")
            sbSQL.Append("  , GOODS.FREE_ITEM14 ")
            sbSQL.Append("  , GOODS.FREE_ITEM15 ")
            sbSQL.Append("  , GOODS.FREE_ITEM16 ")
            sbSQL.Append("  , GOODS.FREE_ITEM17 ")
            sbSQL.Append("  , GOODS.FREE_ITEM18 ")
            sbSQL.Append("  , GOODS.FREE_ITEM19 ")
            '2023/03/22 Kawamoto ADD END
            sbSQL.Append("  , GOODS.ASSET_PIC_FILENAME ")
            sbSQL.Append("  , GOODS.TASK_TS ")
            sbSQL.Append("  , GOODS.INS_DATETIME ")
            sbSQL.Append("  , GOODS.INS_EMPLOYEE_CD ")
            sbSQL.Append("  , GOODS.INS_EMPLOYEE_NAME ")
            sbSQL.Append("  , GOODS.UPD_DATETIME ")
            sbSQL.Append("  , GOODS.UPD_EMPLOYEE_CD ")
            sbSQL.Append("  , GOODS.UPD_EMPLOYEE_NAME ")
            sbSQL.Append("  , GOODS.UPD_TASK ")
            sbSQL.Append("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.Append("  , LOCL.LOC_NAME AS LocLName ")
            sbSQL.Append("  , LOCM.LOC_NAME AS LocMName ")
            sbSQL.Append("  , LOCS.LOC_NAME AS LocSName ")
            sbSQL.Append("  , CATL.CAT_NAME AS CatLName ")
            sbSQL.Append("  , CATM.CAT_NAME AS CatMName ")
            sbSQL.Append("  , CATS.CAT_NAME AS CatSName ")
            sbSQL.Append("  , MNGLOCL.LOC_NAME AS MngLocLName ")
            sbSQL.Append(" FROM ")
            sbSQL.Append("  M_GOODS GOODS ")
            sbSQL.Append(" LEFT JOIN M_LOCATION LOCL ")
            sbSQL.Append(" ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.Append(" AND GOODS.LOC_L_CD = LOCL.LOC_L_CD ")
            sbSQL.Append(" LEFT JOIN M_LOCATION LOCM ")
            sbSQL.Append(" ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
            sbSQL.Append(" AND GOODS.LOC_L_CD = LOCM.LOC_L_CD ")
            sbSQL.Append(" AND GOODS.LOC_M_CD = LOCM.LOC_M_CD ")
            sbSQL.Append(" LEFT JOIN M_LOCATION LOCS ")
            sbSQL.Append(" ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
            sbSQL.Append(" AND GOODS.LOC_L_CD = LOCS.LOC_L_CD ")
            sbSQL.Append(" AND GOODS.LOC_M_CD = LOCS.LOC_M_CD ")
            sbSQL.Append(" AND GOODS.LOC_S_CD = LOCS.LOC_S_CD ")
            sbSQL.Append(" LEFT JOIN M_CATEGORY CATL ")
            sbSQL.Append(" ON CATL.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
            sbSQL.Append(" AND GOODS.CAT_L_CD = CATL.CAT_L_CD ")
            sbSQL.Append(" LEFT JOIN M_CATEGORY CATM ")
            sbSQL.Append(" ON CATM.CAT_DIV = '" + CAT_DIV.CategoryM + "'")
            sbSQL.Append(" AND GOODS.CAT_L_CD = CATM.CAT_L_CD ")
            sbSQL.Append(" AND GOODS.CAT_M_CD = CATM.CAT_M_CD ")
            sbSQL.Append(" LEFT JOIN M_CATEGORY CATS ")
            sbSQL.Append(" ON CATS.CAT_DIV = '" + CAT_DIV.CategoryS + "'")
            sbSQL.Append(" AND GOODS.CAT_L_CD = CATS.CAT_L_CD ")
            sbSQL.Append(" AND GOODS.CAT_M_CD = CATS.CAT_M_CD ")
            sbSQL.Append(" AND GOODS.CAT_S_CD = CATS.CAT_S_CD ")
            sbSQL.Append(" LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.Append(" ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.Append(" AND GOODS.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")
            sbSQL.Append(" ORDER BY ")
            sbSQL.Append("  PRODUCT_NO ")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()
            'SQLを実行
            Dim query As DbSqlQuery(Of GoodsViewModel) = _db.GoodsView.SqlQuery(_SQL, _paramSqlList.ToArray)
            '画面に表示するViewを設定
            _viewResult = query.ToList
        End Sub

#End Region

#Region "登録・更新処理"
        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim blnReturn As Boolean = True
            Return blnReturn
        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()
        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Return blnReturn
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()
        End Sub

#End Region

#Region "削除処理"
        ''' <summary>
        ''' 後処理
        ''' </summary>
        ''' <param name="model">基底モデル</param>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)
        End Sub

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Return True
        End Function

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' </summary>
        Protected Overrides Sub DeleteQuery()
        End Sub
#End Region

#Region "CSVデータ取込処理"
        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="CsvFile">登録するCSVファイル</param>
        ''' <returns>View表示内容</returns>
        Public Function ImportCsvFile(ByVal CsvFile As HttpPostedFileBase) As ActionResult
            Me._lstErr = New List(Of DataInportResult)
            Dim errRow As DataInportResult = New DataInportResult
            Me._lstErr.Add(errRow)
            Dim sCsvPath As String = String.Empty   'CSVアップロードフルパス

            Dim sProcessContents As String          '操作ログ出力

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
                '結果表示
                Return Json(Me._lstErr)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'CSVマッピング明細マスタ取得
            Dim tblCsvMapMei As DataTable = GetCsvMapDetail()
            '取得したCSVマッピング明細マスタの項目数、項目のチェック
            If Not IsCsvMapMeiKoumokuOK(tblCsvMapMei) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.CsvMapMei
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelGoods.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_CSVMAPMEI) & " " &
                                                     Path.GetFileName(CsvFile.FileName)
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelGoods.TITLE))

                Return Json(Me._lstErr)
            End If

            '読込むCSVファイルがあるかどうか
            If Not IsCsvFileError(CsvFile) Then
                Dim sErrGaiyou As String = String.Empty
                Select Case Me._lstErr(0).ResultCd
                    Case DataInport.DataInportResultCd.NoSelect
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECT)
                    Case DataInport.DataInportResultCd.Empty
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EMPTY)
                    Case DataInport.DataInportResultCd.NotCsv
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTCSV)
                End Select

                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelGoods.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelGoods.TITLE))

                Return Json(Me._lstErr)
            End If

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVIMPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'CSVファイルをアップロード
            sCsvPath = Path.Combine(sFullPath, CsvFile.FileName)
            Dim sCsvUrl As String = Path.Combine(sFullPath, CsvFile.FileName)
            CsvFile.SaveAs(sCsvPath)

            'CSVマッピングマスタの文字コード取得
            Dim sCharCd As String = GetCsvMap()

            'CSVマッピングマスタの文字コードがutf-8・shift_jis以外の場合、エラー
            If Not sCharCd = CommonSettingKey.CsvMapUtf.Key1 AndAlso Not sCharCd = CommonSettingKey.CsvMapShiftJis.Key1 Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.CsvMapCharCd
                '結果表示
                Return Json(Me._lstErr)
            End If

            '設定マスタから文字コード情報を取得
            Dim data As SettingInfo = New SettingInfo
            Dim skeyDiv As String = String.Empty    'エラーメッセージ用
            If sCharCd = CommonSettingKey.CsvMapUtf.Key1 Then
                skeyDiv = CommonSettingKey.CsvMapUtf.KeyDiv
                data = Me._Message.getSettingInfo(CommonSettingKey.CsvMapUtf.KeyDiv, CommonSettingKey.CsvMapUtf.Key1, CommonSettingKey.CsvMapUtf.Key2, CommonSettingKey.CsvMapUtf.Key3)
            ElseIf sCharCd = CommonSettingKey.CsvMapUtf.Key2 Then
                skeyDiv = CommonSettingKey.CsvMapShiftJis.KeyDiv
                data = Me._Message.getSettingInfo(CommonSettingKey.CsvMapShiftJis.KeyDiv, CommonSettingKey.CsvMapShiftJis.Key1, CommonSettingKey.CsvMapShiftJis.Key2, CommonSettingKey.CsvMapShiftJis.Key3)
            End If
            '設定マスタからデータを取得できなかった場合、エラー
            If data Is Nothing Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.SettingInfo
                Me._lstErr(0).ErrGaiyou = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), skeyDiv & " " & sCharCd)
                '結果表示
                Return Json(Me._lstErr)
            End If

            'CSVデータ取得 & 項目数チェック
            Dim isKomokusuuErr As Boolean = False
            Dim blErr As Boolean = False    'CSVファイルにエラーがあるかどうか
            blErr = ReadCsvFile(sCsvPath, Encoding.GetEncoding(CInt(data.DATA2)), isKomokusuuErr, tblCsvMapMei)

            '項目数エラーの場合
            If isKomokusuuErr Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelGoods.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " &
                                                     Path.GetFileName(CsvFile.FileName)
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelGoods.TITLE))

                Return Json(Me._lstErr)
            End If

            'エラーチェック
            If Not IsDataError(tblCsvMapMei) Then
                'If Not IsDataError(tbl, stArrayData) Then
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelGoods.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
                                                     Path.GetFileName(CsvFile.FileName) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                     Me._lstErr.Count.ToString
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelGoods.TITLE))

                Return Json(Me._lstErr)
            End If

            '--登録処理開始
            Dim db As New SystemDBContext
            Dim model As List(Of GoodsModel) = New List(Of GoodsModel)                '商品登録用モデル

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try

                    '商品マスタデータを全削除
                    db.Database.ExecuteSqlCommand("DELETE FROM M_GOODS")

                    '登録用モデルにデータを入れる
                    For icnt As Integer = 0 To Me._dtCsvImport.Rows.Count - 1
                        '--商品登録用モデル
                        model.Add(New GoodsModel())
                        model(icnt).PRODUCT_NO = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.ProductNo).ToString.Trim
                        model(icnt).ASSET_DIV = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.AssetDiv).ToString.Trim
                        model(icnt).ASSET_NAME = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.AssetNm).ToString.Trim
                        If Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.MngLocLCd).ToString.Trim = String.Empty Then
                            model(icnt).MNG_LOC_L_CD = Space(1)
                        Else
                            model(icnt).MNG_LOC_L_CD = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.MngLocLCd).ToString.Trim
                        End If
                        If Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.LocLCd).ToString.Trim = String.Empty Then
                            model(icnt).LOC_L_CD = Space(1)
                        Else
                            model(icnt).LOC_L_CD = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.LocLCd).ToString.Trim
                        End If
                        model(icnt).LOC_M_CD = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.LocMCd).ToString.Trim
                        model(icnt).LOC_S_CD = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.LocSCd).ToString.Trim
                        If Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.CatLCd).ToString.Trim = String.Empty Then
                            model(icnt).CAT_L_CD = Space(1)
                        Else
                            model(icnt).CAT_L_CD = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.CatLCd).ToString.Trim
                        End If
                        model(icnt).CAT_M_CD = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.CatMCd).ToString.Trim
                        model(icnt).CAT_S_CD = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.CatSCd).ToString.Trim
                        model(icnt).MAKER = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.Maker).ToString.Trim
                        model(icnt).MODEL_NO = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.ModelNo).ToString.Trim
                        model(icnt).LEASE_NO = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.LeaseNo).ToString.Trim
                        Dim dtLease As DateTime
                        If DateTime.TryParse(Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.LeaseLimit).ToString.Trim, dtLease) Then
                            model(icnt).LEASE_LIMIT = Convert.ToInt32(dtLease.ToString("yyyyMMdd"))
                        Else
                            model(icnt).LEASE_LIMIT = Nothing
                        End If
                        Dim dtGen As DateTime
                        If DateTime.TryParse(Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.GenLimit).ToString.Trim, dtGen) Then
                            model(icnt).GENERIC_LIMIT = Convert.ToInt32(dtGen.ToString("yyyyMMdd"))
                        Else
                            model(icnt).GENERIC_LIMIT = Nothing
                        End If
                        model(icnt).FREE_ITEM1 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem1).ToString.Trim
                        model(icnt).FREE_ITEM2 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem2).ToString.Trim
                        model(icnt).FREE_ITEM3 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem3).ToString.Trim
                        model(icnt).FREE_ITEM4 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem4).ToString.Trim
                        model(icnt).FREE_ITEM5 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem5).ToString.Trim
                        model(icnt).FREE_ITEM6 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem6).ToString.Trim
                        model(icnt).FREE_ITEM7 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem7).ToString.Trim
                        model(icnt).FREE_ITEM8 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem8).ToString.Trim
                        model(icnt).FREE_ITEM9 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem9).ToString.Trim
                        model(icnt).FREE_ITEM10 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem10).ToString.Trim
                        '2023/03/22 Kawamoto ADD START
                        model(icnt).FREE_ITEM11 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem11).ToString.Trim
                        model(icnt).FREE_ITEM12 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem12).ToString.Trim
                        model(icnt).FREE_ITEM13 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem13).ToString.Trim
                        model(icnt).FREE_ITEM14 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem14).ToString.Trim
                        model(icnt).FREE_ITEM15 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem15).ToString.Trim
                        model(icnt).FREE_ITEM16 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem16).ToString.Trim
                        model(icnt).FREE_ITEM17 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem17).ToString.Trim
                        model(icnt).FREE_ITEM18 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem18).ToString.Trim
                        model(icnt).FREE_ITEM19 = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.FreeItem19).ToString.Trim
                        '2023/03/22 Kawamoto ADD END
                        model(icnt).ASSET_PIC_FILENAME = Me._dtCsvImport.Rows(icnt).Item(CsvMapMei_Goods.ColIdx.AssetPicFileNm).ToString.Trim
                        model(icnt).TASK_TS = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))
                        model(icnt).INS_DATETIME = CommonRfid.getNowDate
                        model(icnt).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(icnt).UPD_DATETIME = CommonRfid.getNowDate
                        model(icnt).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(icnt).UPD_TASK = Me._Message.getMessage(CommonLabelGoods.TITLE)
                    Next

                    'システムコンテキストにモデルを追加
                    db.Goods.AddRange(model)

                    '登録処理を実行
                    db.SaveChanges()

                    tran.Commit()

                    '操作ログ出力
                    sProcessContents = Me._Message.getMessage(CommonLabelGoods.TITLE) & " " &
                                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                             (Me._dtCsvImport.Rows.Count - 1).ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelGoods.TITLE))

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    'logger.Error(sbSQL)    'マスタ系はSQLでの登録ではないのではずしておく
                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using

            Return Json(Me._lstErr)

        End Function
#End Region

#Region "CSVマッピングマスタを取得する"
        ''' <summary>
        ''' CSVマッピングマスタを取得する
        ''' </summary>
        Private Function GetCsvMap() As String
            Dim tbl As New DataTable
            Dim sCharCd As String = String.Empty

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CSVMAP_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT CHAR_CD")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("M_CSVMAP")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("CSV_DIV = '" & CsvDiv.Goods & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                If Not IsNothing(tbl) Then
                    sCharCd = tbl.Rows(0).Item("CHAR_CD")
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return sCharCd
        End Function
#End Region

#Region "CSVマッピング明細マスタを取得する"
        ''' <summary>
        ''' CSVマッピング明細マスタを取得する
        ''' </summary>
        Private Function GetCsvMapDetail() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CSVMAP_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  CSV_DIV ")
                sbSQL.AppendLine("  , CSV_NO ")
                sbSQL.AppendLine("  , TOOL_DIV ")
                sbSQL.AppendLine("  , WORD_DIV ")
                sbSQL.AppendLine("  , WORD_CD ")
                sbSQL.AppendLine("  , REQUIRED_ITEM_FLAG ")
                sbSQL.AppendLine("  , SPECIFIED_DIV ")
                sbSQL.AppendLine("  , CSV_COL_NO ")
                sbSQL.AppendLine("  , CSV_FIX_VAL ")
                sbSQL.AppendLine("  , INS_DATETIME ")
                sbSQL.AppendLine("  , INS_EMPLOYEE_CD ")
                sbSQL.AppendLine("  , INS_EMPLOYEE_NAME ")
                sbSQL.AppendLine("  , UPD_DATETIME ")
                sbSQL.AppendLine("  , UPD_EMPLOYEE_CD ")
                sbSQL.AppendLine("  , UPD_EMPLOYEE_NAME ")
                sbSQL.AppendLine("  , UPD_TASK ")
                sbSQL.AppendLine("   ,TOOL_DIV + WORD_DIV + WORD_CD AS RenketsuWordCd ")
                sbSQL.AppendLine("FROM ")
                sbSQL.AppendLine("  M_CSVMAP_DETAIL ")
                sbSQL.AppendLine("WHERE ")
                sbSQL.AppendLine("CSV_DIV = '" & CsvDiv.Goods & "'")

                sbSQL.AppendLine(" ORDER BY CSV_DIV,CSV_NO")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                Return tbl

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl
        End Function
#End Region

#Region "CSVマッピング明細マスタの項目チェック"
        ''' <summary>
        ''' CSVマッピング明細マスタの項目チェック
        ''' </summary>
        ''' <param name="tblCsvMapMei">CSVマッピング明細マスタのデータテーブル</param>
        Private Function IsCsvMapMeiKoumokuOK(tblCsvMapMei As DataTable) As Boolean
            _lstCsvHed = New List(Of String)
            '商品マスタのCSVマッピング明細の項目定義を取得
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.ProductNo)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.AssetDiv)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.AssetNm)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.MngLocLCd)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.LocLCd)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.LocMCd)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.LocSCd)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.CatLCd)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.CatMCd)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.CatSCd)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.Maker)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.ModelNo)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.LeaseNo)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.LeaseLimit)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.GenLimit)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem1)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem2)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem3)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem4)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem5)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem6)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem7)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem8)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem9)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem10)
            '2023/03/22 Kawamoto ADD START
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem11)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem12)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem13)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem14)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem15)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem16)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem17)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem18)
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.FreeItem19)
            '2023/03/22 Kawamoto ADD END
            _lstCsvHed.Add(CsvMapMei_Goods.CharCd.AssetPicFileNm)

            '項目数チェック
            If _lstCsvHed.Count <> tblCsvMapMei.Rows.Count Then
                Return False
            End If

            '項目値チェック
            For icnt As Integer = 0 To _lstCsvHed.Count - 1
                Dim row As DataRow()
                row = tblCsvMapMei.Select("RenketsuWordCd = '" & _lstCsvHed(icnt).ToString & "'")
                If row.Count = 0 Then
                    '該当項目がない場合、エラー
                    Return False
                End If
            Next

            Return True
        End Function
#End Region

#Region "指定したCSVファイルからデータを読み込む。"
        ''' <summary>
        ''' 指定したCSVファイルからデータを読み込む。
        ''' </summary>
        ''' <param name="path">読み取るファイルのパス</param>
        ''' <param name="enc">エンコード</param>
        ''' <param name="isKomokusuuErr">項目数エラーかどうか</param>
        ''' <param name="tblCsvMapMei">CSVマッピング明細マスタのデータテーブル</param>
        ''' <returns>true:正常終了 false:エラー：</returns>
        ''' <remarks></remarks>
        Public Function ReadCsvFile(ByVal path As String, ByVal enc As System.Text.Encoding, ByRef isKomokusuuErr As Boolean, tblCsvMapMei As DataTable) As Boolean

            Dim blErr As Boolean = False 'エラーがあるかどうか
            Dim sline As String = ""    'CSVファイル読込行データ格納用

            'CSV登録用データテーブル作成
            CreatetblImportCsv()

            '重複チェック用変数（ハッシュテーブルに変更）
            Dim htSumiCd As Hashtable = New Hashtable                   '資産番号重複チェック用（Key:資産番号 Value:資産ID）
            Dim htIDSumiCd As Hashtable = New Hashtable                 '資産ID重複チェック用（Key:資産ID Value:資産番号）

            'CSVマッピング明細マスタで列番指定の項目数取得
            Dim Colrows() As DataRow = tblCsvMapMei.Select("CSV_COL_NO = MAX(CSV_COL_NO) ")
            Dim iColNoCnt As Integer = CInt(Colrows(0).Item("CSV_COL_NO"))

            'ファイルから読み込み
            Using sr As StreamReader = New StreamReader(path, enc)
                Dim iRowCnt As Integer = 0  '行カウント用
                '１行取得
                sline = sr.ReadLine()
                Do Until sline Is Nothing
                    If iRowCnt = 0 Then
                        iRowCnt += 1
                        sline = sr.ReadLine()
                        Continue Do
                    Else
                        iRowCnt += 1
                    End If

                    '--行を分解しString配列にする。
                    Dim fields As String() = clsSV_CsvFile.ReadCsvLineToArray(sline)

                    '項目数がゼロ（空行）の場合は次行に遷移
                    If fields.Length = 0 Then
                        iRowCnt += 1
                        sline = sr.ReadLine()
                        Continue Do
                    End If

                    '項目数チェック
                    If fields.Length < iColNoCnt Then
                        'If fields.Length <> iColNoCnt Then
                        isKomokusuuErr = True
                        Return blErr
                    End If

                    'データ行追加
                    Dim newrow As DataRow = _dtCsvImport.NewRow
                    For iCol As Integer = 0 To _lstCsvHed.Count - 1
                        Dim sStr As String = String.Empty
                        Dim rows As DataRow() = tblCsvMapMei.Select("RenketsuWordCd = '" & _lstCsvHed(iCol).ToString & "'")
                        If rows(0).Item("SPECIFIED_DIV") = CsvMapping_SpecifiedDiv.Index Then
                            If CommonRfid.ToStringNvl(rows(0).Item("CSV_COL_NO")).Trim = String.Empty OrElse CInt(CommonRfid.ToStringNvl(rows(0).Item("CSV_COL_NO")).Trim) = 0 Then
                                sStr = String.Empty
                            Else
                                sStr = fields(CInt(rows(0).Item("CSV_COL_NO")) - 1).Trim.TrimStart("'"c)
                            End If
                        ElseIf rows(0).Item("SPECIFIED_DIV") = CsvMapping_SpecifiedDiv.Kotei Then
                            sStr = tblCsvMapMei(iCol).Item("CSV_FIX_VAL").ToString
                        End If
                        newrow(iCol) = sStr
                    Next
                    _dtCsvImport.Rows.Add(newrow)

                    sline = sr.ReadLine()
                Loop

            End Using

            Return blErr

        End Function
#End Region

#Region "CSVファイルのエラーチェック"
        ''' <summary>
        ''' CSVファイルのエラーチェック
        ''' </summary>
        ''' <param name="file">チェック対象のファイル</param>
        ''' <returns>True:エラー無し　False:エラー有り</returns>
        Public Function IsCsvFileError(file As HttpPostedFileBase) As Boolean
            Dim blRet As Boolean = False

            'ファイルがあるかどうか
            If IsNothing(file) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
                Return blRet
            End If

            'ファイルサイズ
            If file.ContentLength = 0 Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.Empty
                Return blRet
            End If

            '拡張子を検証
            Dim extention As String = Path.GetExtension(file.FileName).Replace(".", "")
            If String.IsNullOrEmpty(extention) OrElse extention <> "csv" Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NotCsv
                Return blRet
            End If

            blRet = True
            Return blRet
        End Function
#End Region

#Region "読込んだＣＳＶファイルデータのエラーチェック"
        ''' <summary>
        ''' 読込んだＣＳＶファイルデータのエラーチェック
        ''' </summary>
        ''' <param name="tblCsvMapMei">CSVマッピング明細マスタのデータテーブル</param>
        ''' <returns>True:エラー無し　False:エラー有り</returns>
        Public Function IsDataError(tblCsvMapMei As DataTable) As Boolean
            Dim blRet As Boolean = False

            'ロケーションマスタデータ取得
            Dim LocaData As DataTable
            LocaData = Dao.ToolLocation.GetLocationDataAll()

            'カテゴリマスタデータ取得
            Dim CategoryData As DataTable
            CategoryData = Dao.ToolCategory.GetCategoryDataAll()

            '2023/03/22 Kawamoto ADD START
            '設定マスタ（薬品管理機能）の取得
            Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
                                                                      CommonSettingKey.Drugs_Function.Key1,
                                                                      CommonSettingKey.Drugs_Function.Key2,
                                                                      CommonSettingKey.Drugs_Function.Key3, True)
            '2023/03/22 Kawamoto ADD END

            Dim lstErrKoumoku() As String =
                {CommonLabel.LABEL_PRODUCT_NO,                                  '品番
                 CommonLabel.LABEL_ASSET_DIV,                                   '資産区分
                 CommonLabel.LABEL_ASSET_NAME,                                  '資産名称
                 CommonLabel.LABEL_MNG_LOC_KBN_CD,                              '管理ロケ区分
                 CommonLabel.LABEL_GEN_LOCATION & CommonLabel.LABEL_LOC_L_CD,   'ロケ大区分コード
                 CommonLabel.LABEL_GEN_LOCATION & CommonLabel.LABEL_LOC_M_CD,   'ロケ中区分コード
                 CommonLabel.LABEL_GEN_LOCATION & CommonLabel.LABEL_LOC_S_CD,   'ロケ小区分コード
                 CommonLabel.LABEL_CATEGORY & CommonLabel.LABEL_CAT_L_CD,       'カテゴリ大分類コード
                 CommonLabel.LABEL_CATEGORY & CommonLabel.LABEL_CAT_M_CD,       'カテゴリ中分類コード
                 CommonLabel.LABEL_CATEGORY & CommonLabel.LABEL_CAT_S_CD,       'カテゴリ小分類コード
                 CommonLabel.LABEL_MAKER,                                       '資産製造元
                 CommonLabel.LABEL_MODEL_NO,                                    '型番
                 CommonLabel.LABEL_LEASE_NO,                                    'リース管理番号
                 CommonLabel.LABEL_LEASE_LIMIT,                                 'リース期限
                 CommonLabel.LABEL_GENERIC_LIMIT,                               '〇〇期限
                 CommonLabel.LABEL_ASSET_KOUNYU_DATE,                           '資産購入日
                 CommonLabel.LABEL_FREE_ITEM2,                                  '備考1
                 CommonLabel.LABEL_FREE_ITEM3,                                  '備考2
                 CommonLabel.LABEL_FREE_ITEM4,                                  '備考3
                 CommonLabel.LABEL_FREE_ITEM5,                                  '備考4
                 CommonLabel.LABEL_FREE_ITEM6,                                  '備考5
                 CommonLabel.LABEL_FREE_ITEM7,                                  '備考6
                 CommonLabel.LABEL_FREE_ITEM8,                                  '備考7
                 CommonLabel.LABEL_FREE_ITEM9,                                  '備考8
                 CommonLabel.LABEL_FREE_ITEM10,                                 '備考9
                 CommonLabel.LABEL_FREE_ITEM11,                                 '備考10
                 CommonLabel.LABEL_FREE_ITEM12,                                 '備考11
                 CommonLabel.LABEL_FREE_ITEM13,                                 '備考12
                 CommonLabel.LABEL_FREE_ITEM14,                                 '備考13
                 CommonLabel.LABEL_FREE_ITEM15,                                 '備考14
                 CommonLabel.LABEL_FREE_ITEM16,                                 '備考15
                 CommonLabel.LABEL_FREE_ITEM17,                                 '備考16
                 CommonLabel.LABEL_FREE_ITEM18,                                 '備考17
                 CommonLabel.LABEL_FREE_ITEM19,                                 '備考18
                 CommonLabel.LABEL_ASSET_PIC_FILENAME                           '資産画像
                 }

            '--データチェック
            blRet = True
            Dim lstSumiCd As List(Of String) = New List(Of String)
            For iRowCnt As Integer = 0 To Me._dtCsvImport.Rows.Count - 1

                '該当のカテゴリーマスタデータを取得する
                Dim CatLrow As DataRow() = Nothing  '大分類
                If Not Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.CatLCd).ToString.Trim = String.Empty Then
                    CatLrow = CategoryData.Select("CAT_L_CD = '" + Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.CatLCd).ToString.Trim + "' AND CAT_DIV = '" + CAT_DIV.CategoryL + "'")
                End If
                Dim CatMrow As DataRow() = Nothing  '中分類
                If Not Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.CatMCd).ToString.Trim = String.Empty AndAlso Not CatLrow Is Nothing AndAlso CatLrow.Length > 0 Then
                    CatMrow = CategoryData.Select("CAT_L_CD = '" + Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.CatLCd).ToString.Trim + "' AND CAT_M_CD = '" + Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.CatMCd).ToString.Trim + "' AND CAT_DIV = '" + CAT_DIV.CategoryM + "'")
                End If
                '該当のロケーションマスタデータを取得する
                Dim LocLrow As DataRow() = Nothing  '大区分
                If Not Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.LocLCd).ToString.Trim = String.Empty Then
                    LocLrow = LocaData.Select("LOC_L_CD = '" + Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.LocLCd).ToString.Trim + "' AND LOC_DIV = '" + LOC_DIV.LocationL + "'")
                End If
                Dim LocMrow As DataRow() = Nothing  '中区分
                If Not Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.LocMCd).ToString.Trim = String.Empty AndAlso Not LocLrow Is Nothing AndAlso LocLrow.Length > 0 Then
                    LocMrow = LocaData.Select("LOC_L_CD = '" + Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.LocLCd).ToString.Trim + "' AND LOC_M_CD = '" + Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.LocMCd).ToString.Trim + "' AND LOC_DIV = '" + LOC_DIV.LocationM + "'")
                End If
                '該当の管理ロケーションマスタデータを取得する
                Dim MngLocLrow As DataRow() = Nothing  '大区分
                If Not Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.MngLocLCd).ToString.Trim = String.Empty Then
                    MngLocLrow = LocaData.Select("LOC_L_CD = '" + Me._dtCsvImport.Rows(iRowCnt).Item(CsvMapMei_Goods.ColIdx.MngLocLCd).ToString.Trim + "' AND LOC_DIV = '" + LOC_DIV.LocationL + "'")
                End If

                For iCalCnt As Integer = 0 To Me._lstCsvHed.Count - 1
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If

                    '列によってチェック条件・設定項目値を分ける
                    Dim sStr As String = Me._dtCsvImport.Rows(iRowCnt).Item(iCalCnt).ToString.Trim

                    Dim required As String = tblCsvMapMei.Select("RenketsuWordCd = '" & _lstCsvHed(iCalCnt).ToString & "'")(0).Item("REQUIRED_ITEM_FLAG")
                    If required = CsvMapping_ReqItemFlag.Yes AndAlso String.IsNullOrEmpty(sStr) Then
                        '空白チェック
                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(lstErrKoumoku(iCalCnt))
                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                        blRet = False

                    ElseIf String.IsNullOrEmpty(sStr) Then
                        Select Case iCalCnt
                            Case CsvMapMei_Goods.ColIdx.LocMCd OrElse CsvMapMei_Goods.ColIdx.LocSCd OrElse
                                 CsvMapMei_Goods.ColIdx.CatMCd OrElse CsvMapMei_Goods.ColIdx.CatSCd
                                '値がない場合、空白を入れる(ロケ中・小区分、カテゴリ中・小分類)
                                Me._dtCsvImport.Rows(1).Item(iCalCnt) = " "
                                Continue For

                            Case Else
                                '空白ならチェックしない
                                Continue For
                        End Select
                    End If

                    Select Case iCalCnt
                        '品番
                        Case CsvMapMei_Goods.ColIdx.ProductNo
                            If sStr.Length > itemlength.ProductNo Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                                '半角英数字記号チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf lstSumiCd.IndexOf(sStr) > -1 Then
                                '重複チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_PRODUCT_NO) & "[{0}]", sStr)
                                blRet = False
                            Else
                                lstSumiCd.Add(sStr)
                            End If
                        '資産区分
                        Case CsvMapMei_Goods.ColIdx.AssetDiv
                            If sStr.Length > itemlength.AssetDiv Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_DIV)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        '資産名称
                        Case CsvMapMei_Goods.ColIdx.AssetNm
                            If sStr.Length > itemlength.AssetNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        '管理ロケ区分
                        Case CsvMapMei_Goods.ColIdx.MngLocLCd
                            If Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MNG_LOC_KBN_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            Else
                                'ロケーションマスタに登録されているロケーションコードかチェック
                                If MngLocLrow Is Nothing OrElse MngLocLrow.Length = 0 Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MNG_LOC_KBN_CD)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.LocLCd
                            'ロケ大区分コード
                            If sStr.Length > itemlength.LocLCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            Else
                                'ロケーションマスタに登録されているロケーションコードかチェック
                                If LocLrow Is Nothing OrElse LocLrow.Length = 0 Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.LocMCd
                            If sStr.Length > itemlength.LocMCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf LocLrow Is Nothing OrElse LocLrow.Length = 0 Then
                                '上位の大区分が登録されているかチェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_LOC_L_CD)
                                blRet = False
                            ElseIf LocMrow Is Nothing OrElse LocMrow.Length = 0 Then
                                'ロケーションマスタに登録されているロケーションコードかチェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                blRet = False
                            End If
                        Case CsvMapMei_Goods.ColIdx.LocSCd
                            'ロケ小区分コード
                            If sStr.Length > itemlength.LocSCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf LocLrow Is Nothing OrElse LocLrow.Length = 0 Then
                                '上位の大区分が登録されているかチェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_LOC_L_CD)
                                blRet = False
                            ElseIf LocMrow Is Nothing OrElse LocMrow.Length = 0 Then
                                '上位の中区分が登録されているかチェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_LOC_M_CD)
                                blRet = False
                            Else
                                'ロケーションマスタに登録されているロケーションコードかチェック
                                Dim Locrow As DataRow() = LocaData.Select("LOC_L_CD = '" + Me._dtCsvImport.Rows(1).Item(CsvMapMei_Goods.ColIdx.LocLCd).ToString + "' AND LOC_M_CD = '" + Me._dtCsvImport.Rows(1).Item(CsvMapMei_Goods.ColIdx.LocMCd).ToString + "' AND LOC_S_CD = '" + sStr + "' AND LOC_DIV = '" + LOC_DIV.LocationS + "'")
                                If Locrow Is Nothing OrElse Locrow.Length = 0 Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.CatLCd
                            'カテゴリ大分類コード
                            If sStr.Length > itemlength.CatLCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            Else
                                'カテゴリマスタに登録されているカテゴリコードかチェック
                                If CatLrow Is Nothing OrElse CatLrow.Length = 0 Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                    'Else
                                    '    'EPCサイズを設定する
                                    '    Me._dtCsvImport.Rows(1).Item("EPC_SIZE") = CInt(CatLrow(0)("EPC_SIZE"))
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.CatMCd
                            'カテゴリ中分類コード
                            If sStr.Length > itemlength.CatMCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf CatLrow Is Nothing OrElse CatLrow.Length = 0 Then
                                '上位の大分類が登録されているかチェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_CAT_L_CD)
                                blRet = False
                            ElseIf CatMrow Is Nothing OrElse CatMrow.Length = 0 Then
                                'カテゴリーマスタに登録されているカテゴリーコードかチェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                blRet = False
                            End If
                        Case CsvMapMei_Goods.ColIdx.CatSCd
                            'カテゴリ小分類コード
                            If sStr.Length > itemlength.CatSCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf CatLrow Is Nothing OrElse CatLrow.Length = 0 Then
                                '上位の大分類が登録されているかチェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_CAT_L_CD)
                                blRet = False
                            ElseIf CatMrow Is Nothing OrElse CatMrow.Length = 0 Then
                                '上位の中分類が登録されているかチェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_CAT_M_CD)
                                blRet = False
                            Else
                                'カテゴリーマスタに登録されているカテゴリーコードかチェック
                                Dim Catrow As DataRow() = CategoryData.Select("CAT_L_CD = '" + Me._dtCsvImport.Rows(1).Item(CsvMapMei_Goods.ColIdx.CatLCd).ToString + "' AND CAT_M_CD = '" + Me._dtCsvImport.Rows(1).Item(CsvMapMei_Goods.ColIdx.CatMCd).ToString + "' AND CAT_S_CD = '" + Me._dtCsvImport.Rows(1).Item(CsvMapMei_Goods.ColIdx.CatSCd).ToString + "' AND CAT_DIV = '" + CAT_DIV.CategoryS + "'")
                                If Catrow Is Nothing OrElse Catrow.Length = 0 Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.Maker
                            'メーカ
                            If sStr.Length > itemlength.Maker Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAKER)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If

                        Case CsvMapMei_Goods.ColIdx.ModelNo
                            '型番
                            If sStr.Length > itemlength.ModelNo Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MODEL_NO)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                                'ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                                '    '半角英数字記号チェック
                                '    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                '    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MODEL_NO)
                                '    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                '    blRet = False
                            End If
                        Case CsvMapMei_Goods.ColIdx.LeaseNo
                            'リース管理番号
                            If sStr.Length > itemlength.LeaseNo Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_NO)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvMapMei_Goods.ColIdx.LeaseLimit
                            'リース期限
                            If sStr.Length > itemlength.LeaseLimit + 2 Then '/が入るのでその分プラスする。
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                                '半角英数字記号チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            Else
                                '日付チェック
                                'DateTimeに変換できるか確かめる
                                Dim dt As DateTime
                                If Not DateTime.TryParse(sStr, dt) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.GenLimit
                            '○○期限
                            If sStr.Length > itemlength.GenLimit + 2 Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                                '半角英数字記号チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            Else
                                '日付チェック
                                'DateTimeに変換できるか確かめる
                                Dim dt As DateTime
                                If Not DateTime.TryParse(sStr, dt) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                End If

                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem1
                            'フリー項目1
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_KOUNYU_DATE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            Else
                                '日付チェック
                                'DateTimeに変換できるか確かめる
                                Dim dt As DateTime
                                If Not DateTime.TryParse(sStr, dt) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_KOUNYU_DATE)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                Else
                                    'スラッシュ区切りの日付形式に編集する
                                    Me._dtCsvImport.Rows(0).Item(CsvMapMei_Goods.ColIdx.FreeItem1) = CommonRfid.ToStringDateYMD(dt)
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem2
                            'フリー項目2
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM2)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem3
                            'フリー項目3
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM3)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                                '2023/03/22 Kawamoto ADD START
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    'マスタ存在チェック
                                    If Not String.IsNullOrEmpty(sStr) AndAlso Not Dao.ToolSetting.ChkExistDrugsFSActNumber(sStr) Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM3)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), sStr)
                                        blRet = False
                                    End If
                                End If
                                '2023/03/22 Kawamoto ADD END
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem4
                            'フリー項目4
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM4)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                                '2023/03/22 Kawamoto ADD START
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    '入力値(Y/N/-)チェック
                                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> Exist.Yes AndAlso sStr <> Exist.No AndAlso sStr <> Exist.None Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM4)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
                                        blRet = False
                                    End If
                                End If
                                '2023/03/22 Kawamoto ADD END
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem5
                            'フリー項目5
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM5)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                                '2023/03/22 Kawamoto ADD START
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    '入力値(1:毒物/2：劇物/-：指定なし)チェック
                                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> PoisonAndDeleterious.Poison AndAlso sStr <> PoisonAndDeleterious.Deleterious AndAlso sStr <> PoisonAndDeleterious.None Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM5)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
                                        blRet = False
                                    End If
                                End If
                                '2023/03/22 Kawamoto ADD END
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem6
                            'フリー項目6
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM6)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                                '2023/03/22 Kawamoto ADD START
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    '入力値(Y/N/-)チェック
                                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> Exist.Yes AndAlso sStr <> Exist.No AndAlso sStr <> Exist.None Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM6)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
                                        blRet = False
                                    End If
                                End If
                                '2023/03/22Kawamoto ADD END
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem7
                            'フリー項目7
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM7)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                                '2023/02/08 Kawamoto UPD START
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    '入力値(Y/N/-)チェック
                                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> Exist.Yes AndAlso sStr <> Exist.No AndAlso sStr <> Exist.None Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM7)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
                                        blRet = False
                                    End If
                                End If
                                '2023/02/08 Kawamoto UPD END
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem8
                            'フリー項目8
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM8)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                                '2023/03/22 Kawamoto UPD START
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    '入力値(Y/N/-)チェック
                                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> Exist.Yes AndAlso sStr <> Exist.No AndAlso sStr <> Exist.None Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM8)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
                                        blRet = False
                                    End If
                                End If
                                '2023/03/22 Kawamoto UPD END
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem9
                            'フリー項目9
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM9)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                                '2023/03/22 Kawamoto UPD START
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    'マスタ存在チェック
                                    If Not String.IsNullOrEmpty(sStr) AndAlso Not Dao.ToolEmployee.ExistEmployeeName(sStr) Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM9)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EMPLOYEECD)
                                        blRet = False
                                    End If
                                End If
                                '2023/03/22 Kawamoto UPD END
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem10
                            'フリー項目10
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM10)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                                '2023/03/22 Kawamoto UPD START
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
                                    If Not String.IsNullOrEmpty(ret) Then
                                        '小数値チェック
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM10)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
                                        blRet = False
                                    End If
                                End If
                            End If
                            '2023/03/22 Kawamoto UPD END
                        '2023/03/22 Kawamoto ADD START
                        Case CsvMapMei_Goods.ColIdx.FreeItem11
                            'フリー項目11
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM11)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
                                    If Not String.IsNullOrEmpty(ret) Then
                                        '小数値チェック
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM11)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
                                        blRet = False
                                    End If
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem12
                            'フリー項目12
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM12)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
                                    If Not String.IsNullOrEmpty(ret) Then
                                        '小数値チェック
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM12)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
                                        blRet = False
                                    End If
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem13
                            'フリー項目13
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM13)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    '日付チェック
                                    'DateTimeに変換できるか確かめる
                                    Dim dt As DateTime
                                    If Not DateTime.TryParse(sStr, dt) Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM13)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                        blRet = False
                                    Else
                                        'スラッシュ区切りの日付形式に編集する
                                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Goods.ColIdx.FreeItem13) = CommonRfid.ToStringDateYMD(dt)
                                    End If
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem14
                            'フリー項目14
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM14)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
                                    If Not String.IsNullOrEmpty(ret) Then
                                        '小数値チェック
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM14)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
                                        blRet = False
                                    End If
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem15
                            'フリー項目15
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM15)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    If Not Dao.ToolSetting.ChkExistDrugsWeighingMachine(sStr) Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM15)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), sStr)
                                        blRet = False
                                    End If
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem16
                            'フリー項目16
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM16)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            Else
                                '薬品機能がONの場合
                                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                                    Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
                                    If Not String.IsNullOrEmpty(ret) Then
                                        '小数値チェック
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM16)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
                                        blRet = False
                                    End If
                                End If
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem17
                            'フリー項目17
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM17)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem18
                            'フリー項目18
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM18)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvMapMei_Goods.ColIdx.FreeItem19
                            'フリー項目19
                            If sStr.Length > itemlength.FreeItem Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM19)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        '2023/03/22 Kawamoto ADD END

                        Case CsvMapMei_Goods.ColIdx.AssetPicFileNm
                            '資産画像ファイル名
                            If sStr.Length > itemlength.AssetPicFileNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_PIC_FILENAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case Else
                            Continue For
                    End Select
                Next
            Next

            'データ項目にエラーがあった場合
            If blRet = False Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG

                '最終行が空白行であれば削除する。
                If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = 0 Then
                    Dim lstRow As DataInportResult = New DataInportResult
                    Me._lstErr.RemoveAt(Me._lstErr.Count - 1)
                End If
            End If

            Return blRet
        End Function
#End Region

#Region "ファイルをダウンロードさせます。"
        ''' <summary>
        ''' ファイルをダウンロードさせます。
        ''' </summary>
        ''' <param name="FileName">ダウンロードするファイル名を指定します。</param>
        ''' <param name="MimeType">ダウンロードするファイルのMIMEタイプを指定します。</param>
        ''' <param name="sFileFullPath">ダウンロードするファイルのフルパス</param>
        Private Sub FileDownload(FileName As String, MimeType As String, ByVal sFileFullPath As String)
            'ダウンロードするファイル名
            Dim dlFileName As String

            'ファイル名が日本語の場合を考慮したダウンロードファイル名を作成
            If Request.Browser.Browser = "IE" Then
                'IEの場合はファイル名をURLエンコード
                dlFileName = HttpUtility.UrlEncode(FileName)
            Else
                'IE以外の場合はそのままでOK
                dlFileName = FileName
            End If

            'ダウンロード処理
            'Response情報クリア
            Response.ClearContent()

            'バッファリング
            Response.Buffer = True

            'HTTPヘッダー情報・MIMEタイプ設定
            Response.AddHeader("Content-Disposition", String.Format("attachment;filename={0}", dlFileName))
            Response.ContentType = MimeType

            'ファイルを書き出し
            Response.WriteFile(sFileFullPath)

            Response.Flush()
            Response.End()
        End Sub
#End Region

#Region "CSV登録用データテーブルの作成"
        ''' <summary>
        ''' CSV登録用データテーブルの作成
        ''' </summary>
        Public Sub CreatetblImportCsv()
            'CSVマッピング明細マスタで定義されている項目の列を追加
            _dtCsvImport = New DataTable("EmployeeList")
            For icnt As Integer = 0 To _lstCsvHed.Count - 1
                _dtCsvImport.Columns.Add(_lstCsvHed(icnt).ToString, GetType(String))
            Next
        End Sub
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        Public Function CsvExport() As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim db As SystemDBContext = New SystemDBContext
            Dim GoodsData As List(Of GoodsViewModel)

            '商品マスタデータ取得
            'SQLを実行する
            ViewQuery()
            GoodsData = db.GoodsView.SqlQuery(_SQL, _paramSqlList.ToArray).ToList

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.Goods.KeyDiv, CommonSettingKey.Goods.Key1, CommonSettingKey.Goods.Key2, CommonSettingKey.Goods.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            Dim sUrlFileName As String = Path.GetRandomFileName()
            Dim sDspFileName As String = String.Format(sFileNm + "_{0}.csv", System.DateTime.Now.ToString("yyyyMMdd"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sUrlFileName)

            '◆CSV出力用リスト作成
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'CSVヘッダー取得
            Dim sHedder As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_GOODS)
            Dim singleQuotation As String = "'"


            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHedder))
            'データ行挿入
            Dim col As String = ""
            For Each row As GoodsViewModel In GoodsData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PRODUCT_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_DIV)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.MNG_LOC_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_M_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_S_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CAT_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CAT_M_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CAT_S_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LEASE_NO)
                Dim dtLease As DateTime
                If row.LEASE_LIMIT.HasValue Then
                    If DateTime.TryParseExact(row.LEASE_LIMIT.Value, "yyyyMMdd", Nothing, Globalization.DateTimeStyles.None, dtLease) Then
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_LIMIT.Value))
                    Else
                            col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_LIMIT.Value)
                        End If
                    Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If

                Dim dtGen As DateTime
                If row.GENERIC_LIMIT.HasValue Then
                    If DateTime.TryParseExact(row.GENERIC_LIMIT.Value, "yyyyMMdd", Nothing, Globalization.DateTimeStyles.None, dtGen) Then
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.GENERIC_LIMIT.Value))
                    Else
                            col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_LIMIT.Value)
                        End If
                    Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM8)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM9)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM10)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM11)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM12)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM13)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM14)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM15)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM16)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM17)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM18)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM19)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_PIC_FILENAME)
                col += ""
                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                'ファイルをダウンロードさせる
                Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

                'ダウンロードさせた後、ファイルを削除。
                System.IO.File.Delete(sFullPath + "\" + sUrlFileName)
            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelGoods.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     GoodsData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE))


            '結果表示
            'Return Json(EmployeeData)
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function

#End Region

#Region "CSVヘッダー取得"
        ''' <summary>
        ''' CSVヘッダー取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCSVHeader(prmFunctionId As String) As String
            Dim csvHeader As String = String.Empty
            Dim wordList As New List(Of String)
            Dim csvHeaderWord As List(Of String) = Dao.ToolCSVHeader.GetCSVHeaderOrder(prmFunctionId)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            For Each word As String In csvHeaderWord
                wordList.Add(Me._Message.getMessage(word))
            Next

            csvHeader = String.Join(",", wordList)

            Return csvHeader
        End Function
#End Region

#Region "共通"
        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New GoodsViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).TASK_TS = _ViewModel.TASK_TS
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).INS_DATETIME = _viewResult.ElementAt(0).INS_DATETIME
            _viewResult.ElementAt(0).INS_EMPLOYEE_NAME = _viewResult.ElementAt(0).INS_EMPLOYEE_NAME
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.GOODS, "ALL")
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.GOODS, "ALL").FIX_COL_CNT
        End Sub

#End Region

    End Class
End Namespace