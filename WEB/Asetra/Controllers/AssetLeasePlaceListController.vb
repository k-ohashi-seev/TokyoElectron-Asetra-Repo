Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports Asetra.Dao


Namespace Controllers
    ''' <summary>
    ''' 貸出先一覧画面表示用のクラス
    ''' </summary>
    Public Class AssetLeasePlaceListController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)



#Region "クラス定数"

#End Region

#Region "クラス変数"
        'Private _Model As LeasePlaceModel = New LeasePlaceModel                                 'LeasePlaceモデル
        Private _ViewModel As AssetLeasePlaceListViewModel = New AssetLeasePlaceListViewModel                     'LeasePlace一覧表示用モデル
        Protected _viewResult As List(Of AssetLeasePlaceListViewModel) = New List(Of AssetLeasePlaceListViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As StockListViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_SERCH_KEY) IsNot Nothing Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_SERCH_KEY)
            End If

            Return RedirectToAction("Index")
        End Function
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
        Public Function Index(ByVal ViewModel As AssetLeasePlaceListViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_LEASE_PLACE)
            End If

            'モデルを設定
            _ViewModel = ViewModel

            '貸出先資産照会画面から戻った時の処理
            Dim senimaeModel As AssetLeasePlaceListViewModel = Session(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_SERCH_KEY)
            If senimaeModel IsNot Nothing Then
                _ViewModel = senimaeModel
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_SERCH_KEY)
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_FROM_LIST)
            End If

            'メッセージ一覧取得              2021/10/03 ADD nv Inoue
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

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
            End If

            '貸出先
            GetLeasePlaceData()

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.ASSET_LEASE_PLACE_LIST)
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
                Session.Remove(CommonSessionKey.CREAGE_LEASE_PLACE)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            'クエリを返却
            MyBase._SQL = CreateSQL()

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of AssetLeasePlaceListViewModel) = _db.AssetLeasePlaceListView.SqlQuery(_SQL, _paramSqlList.ToArray)

            '画面に表示するViewを設定
            _viewResult = query.ToList

        End Sub

#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ViewModel As AssetLeasePlaceListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            Dim GateData As List(Of AssetLeasePlaceListViewModel)

            '貸出先一覧データ取得
            GateData = GetListData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim objSetting As SettingInfo = Setting.getSettingInfo(CommonSettingKey.AssetLease.KeyDiv, CommonSettingKey.AssetLease.Key1, CommonSettingKey.AssetLease.Key2, CommonSettingKey.AssetLease.Key3, True)
            Dim sFileNm As String = objSetting.DATA1

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            Dim sUrlFileName As String = Path.GetRandomFileName()
            Dim sDspFileName As String = String.Format(sFileNm + "_{0}.csv", Now.ToString("yyyyMMdd"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sUrlFileName)

            '◆CSV出力用リスト作成
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'CSVヘッダー取得
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_LEASE)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_LEASE)


            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As AssetLeasePlaceListViewModel In GateData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DispLeaseCount)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DispLeaseLimitExpiredCount)

                col += ""
                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                'ファイルをダウンロードさせる
                Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

                'ダウンロードさせた後、ファイルを削除。
                System.IO.File.Delete(sFullPath + "\" + sUrlFileName)

            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetLeasePlaceList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     GateData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetLeasePlaceList.TITLE))


            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetListData() As List(Of AssetLeasePlaceListViewModel)
            Dim lstAssetLease As List(Of AssetLeasePlaceListViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstAssetLease = db.AssetLeasePlaceListView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList

            'データを返却
            Return lstAssetLease

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


#Region "一覧データ取得用SQLを作成する"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '2021/10/02 ADD nv Inoue start
            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLeasePlaceList, _LoginInfo)
            '2021/10/02 ADD nv Inoue end

            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("    LPL.LEASE_PLACE_CD ")
            sbSQL.AppendLine("  , LPL.LEASE_PLACE_NAME ")
            sbSQL.AppendLine("  , ISNULL(LCNT.DispLeaseCount, 0) AS DispLeaseCount ")
            sbSQL.AppendLine("  , ISNULL(LCNT.DispLeaseLimitExpiredCount, 0) AS DispLeaseLimitExpiredCount ")

            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM  M_LEASE_PLACE LPL")

            sbSQL.AppendLine("LEFT JOIN  ")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("  SELECT ")
            sbSQL.AppendLine("        LEASE_PLACE_CD")
            sbSQL.AppendLine("      , COUNT(CASE WHEN LEASE_DUE_DT >= FORMAT(GetDate(), 'yyyyMMdd') THEN 1 END) AS DispLeaseCount")
            sbSQL.AppendLine("      , COUNT(CASE WHEN LEASE_DUE_DT < FORMAT(GetDate(), 'yyyyMMdd') THEN 1 END) AS DispLeaseLimitExpiredCount")
            sbSQL.AppendLine("  FROM  M_ASSET")

            '2021/10/02 ADD nv Inoue start
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbSQL.AppendLine("    ON AUTH.LOC_L_CD = M_ASSET.MNG_LOC_L_CD ")
            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{AuthInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{AuthInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{AuthInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{AuthInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType", $"{Misc.UpdAuthType.UPD_AUTH}")
            '2021/10/02 ADD nv Inoue end

            sbSQL.AppendLine("  WHERE ASSET_STATUS = " & ASSET_STATUS.Lending)
            sbSQL.AppendLine("  GROUP BY LEASE_PLACE_CD")
            sbSQL.AppendLine(") LCNT")

            sbSQL.AppendLine("ON  LPL.LEASE_PLACE_CD = LCNT.LEASE_PLACE_CD")

            sbSQL.AppendLine("WHERE 1 = 1 ")

            '検索条件を設定する
            '貸出先コード
            If Not IsNothing(_ViewModel.SearchCode) AndAlso _ViewModel.SearchCode <> "" Then
                sbSQL.AppendLine("AND LPL.LEASE_PLACE_CD = @leasePlaceCd")
                addSqlParam("leasePlaceCd", $"{_ViewModel.SearchCode}")
            End If

            '貸出期限切れ　SearchLeaseLimitExpired
            If _ViewModel.SearchLeaseLimitExpired Then
                sbSQL.AppendLine(" AND ISNULL(LCNT.DispLeaseLimitExpiredCount, 0) > 0")
            End If

            '現在貸出有り　SearchLeaseNow
            If _ViewModel.SearchLeaseNow Then
                sbSQL.AppendLine(" AND ISNULL(LCNT.DispLeaseCount, 0) > 0")
            End If

            sbSQL.AppendLine(" ORDER BY LPL.LEASE_PLACE_CD")

            'クエリを返却
            Return sbSQL.ToString()
        End Function
#End Region

#Region "貸出先コード・元画面の検索条件をセッションに保存し、貸出先資産照会画面に遷移します"
        ''' <summary>
        ''' 貸出先コード・元画面の検索条件をセッションに保存し、貸出先資産照会画面に遷移します
        ''' </summary>
        <ValidateInput(False)>
        Public Function SetLeasePlaceCd(ByVal ViewModel As AssetLeasePlaceListViewModel, ByVal SelectedLeasePlaceCd As String) As ActionResult
            'Public Sub SetAssetNo(Model As AssetListViewModel)
            Session.Remove(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_KEY)
            Session(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_KEY) = SelectedLeasePlaceCd

            '検索条件をセッションに保存
            Session(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_SERCH_KEY) = ViewModel

            '結果表示
            Return RedirectToAction("Index", "AssetLeasePlaceDetail")
        End Function

#End Region

#Region "貸出先マスタドロップダウンリストデータ取得処理"
        ''' <summary>
        ''' 貸出先マスタドロップダウンリストデータ取得処理
        ''' </summary>
        Public Sub GetLeasePlaceData()

            Dim lst As List(Of LeasePlaceModel)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext
            'M_DEPARTMENTテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT *")
            sbSQL.AppendLine(" FROM ")
            sbSQL.AppendLine("  M_LEASE_PLACE ")
            sbSQL.AppendLine("ORDER BY LEASE_PLACE_CD")
            'SQLを実行する
            lst = db.LeasePlace.SqlQuery(sbSQL.ToString).ToList
            '貸出先コンボ設定
            If String.IsNullOrEmpty(_ViewModel.SearchCode) Then
                ViewBag.LeasePlaceList = New SelectList(lst, "LEASE_PLACE_CD", "LEASE_PLACE_NAME", "")
            Else
                ViewBag.LeasePlaceList = New SelectList(lst, "LEASE_PLACE_CD", "LEASE_PLACE_NAME", _ViewModel.SearchCode)
            End If

        End Sub
#End Region

#Region "CSVヘッダー取得"
        ''' <summary>
        ''' プリンタ情報取得
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
                _viewResult.Add(New AssetLeasePlaceListViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchCode = _ViewModel.SearchCode
            _viewResult.ElementAt(0).SearchLeaseLimitExpired = _ViewModel.SearchLeaseLimitExpired
            _viewResult.ElementAt(0).SearchLeaseNow = _ViewModel.SearchLeaseNow
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LEASE_PLACE_LIST)
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_LEASE_PLACE_LIST).FIX_COL_CNT

        End Sub

        '''' <summary>
        '''' Viewモデルを更新用のモデルにコピーする
        '''' </summary>
        'Private Sub CopyViewModeltoModel(ByVal model As AssetLeasePlaceListViewModel)
        '    _Model.LEASE_PLACE_CD = model.LEASE_PLACE_CD
        '    _Model.LEASE_PLACE_NAME = model.LEASE_PLACE_NAME
        '    _Model.INS_DATETIME = model.INS_DATETIME
        '    _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
        '    _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
        '    _Model.UPD_DATETIME = model.UPD_DATETIME
        '    _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
        '    _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
        '    _Model.UPD_TASK = model.UPD_TASK
        'End Sub
#End Region



        Protected Overrides Sub CreateQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub DeleteQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub EditQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Function CreateChcek() As Boolean
            Throw New NotImplementedException()
        End Function

        Protected Overrides Function DeleteChcek() As Boolean
            Throw New NotImplementedException()
        End Function

        Protected Overrides Function EditChcek() As Boolean
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace