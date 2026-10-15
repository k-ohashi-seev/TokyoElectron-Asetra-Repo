Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers
    ''' <summary>
    ''' 貸出先資産詳細画面表示用のクラス
    ''' </summary>
    Public Class AssetLeasePlaceDetailController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


#Region "クラス定数"

#End Region

#Region "クラス変数"
        'Private _Model As LeasePlaceModel = New LeasePlaceModel                                 'LeasePlaceモデル
        Private _ViewModel As AssetLeasePlaceDetailViewModel = New AssetLeasePlaceDetailViewModel                     '貸出先資産照会画面表示用モデル
        Protected _viewResult As List(Of AssetLeasePlaceDetailViewModel) = New List(Of AssetLeasePlaceDetailViewModel)    '結果View
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
            If Session(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_FROM_LIST) IsNot Nothing Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_FROM_LIST)
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
        Public Function Index(ByVal ViewModel As AssetLeasePlaceDetailViewModel, ByVal actionname As String) As ActionResult

            'モデルを設定
            _ViewModel = ViewModel

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

            '貸出先一覧画面からの遷移の場合、戻るボタン表示
            If Session(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_FROM_LIST) IsNot Nothing Then
                ViewBag.FromListKey = True
            Else
                ViewBag.FromListKey = String.Empty
            End If

            '貸出先
            GetLeasePlaceData()

            '貸出担当者
            GetEmployeeData()

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.ASSET_LEASE_PLACE_DETAIL)
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

            MyBase._SQL = CreateSQL()

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of AssetLeasePlaceDetailViewModel) = _db.AssetLeasePlaceDetailView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function CsvExport(ViewModel As AssetLeasePlaceDetailViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            Dim GateData As List(Of AssetLeasePlaceDetailViewModel)

            '貸出先資産照会データ取得
            GateData = GetListData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim objSetting As SettingInfo = Setting.getSettingInfo(CommonSettingKey.AssetLeaseDetail.KeyDiv, CommonSettingKey.AssetLeaseDetail.Key1, CommonSettingKey.AssetLeaseDetail.Key2, CommonSettingKey.AssetLeaseDetail.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_LEASE_DETAIL)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_LEASE_DETAIL)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As AssetLeasePlaceDetailViewModel In GateData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_NAME)
                Select Case row.DispLeaseStatus
                    Case LeaseStatus.Cd.Returned
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED))
                    Case LeaseStatus.Cd.Lending
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING))
                    Case LeaseStatus.Cd.LimitExpired
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LIMIT_EXPIRED))
                End Select
                If row.DispLeaseKikanFrom.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.DispLeaseKikanFrom.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                If row.DispLeaseKikanTo.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.DispLeaseKikanTo.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DispLeaseBiko)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DispLeaseEmployeeCd)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DispLeaseEmployeeNm)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PRODUCT_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetLeasePlaceDetail.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     GateData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetLeasePlaceDetail.TITLE))


            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetListData() As List(Of AssetLeasePlaceDetailViewModel)
            Dim lstAssetLeaseDetail As List(Of AssetLeasePlaceDetailViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstAssetLeaseDetail = db.AssetLeasePlaceDetailView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList

            'データを返却
            Return lstAssetLeaseDetail

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
            Dim sbSQL As StringBuilder = New StringBuilder

            '2021/10/09 ADD nv Inoue start
            If IsNothing(Me._Message) Then
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            End If
            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLeasePlaceList, _LoginInfo)
            '2021/10/09 ADD nv Inoue end

            '検索条件を設定する
            'セッションがあれば（画面遷移後の初期表示）セッションから設定する
            If Session(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_KEY) IsNot Nothing Then
                Dim sSession As String() = Session(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_KEY).Split(","c)
                _ViewModel.SearchCode = sSession(0)
                _ViewModel.SearchLeaseLimitExpired = CBool(sSession(1))

                '貸出先一覧画面からの遷移ということを覚えておく
                Session(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_FROM_LIST) = True

                'セッションをクリアする
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LEASEPLACE_DETAIL_KEY)
            End If


            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("    IsNull(AST.LEASE_PLACE_CD, '') AS LEASE_PLACE_CD ")
            sbSQL.AppendLine("  , AST.LEASE_PLACE_NAME ")
            sbSQL.AppendLine("  , CASE ")
            sbSQL.AppendLine("      WHEN AST.LEASE_DUE_DT >= FORMAT(GetDate(), 'yyyyMMdd') THEN " & Misc.LeaseStatus.Cd.Lending.ToString)
            sbSQL.AppendLine("      WHEN AST.LEASE_DUE_DT <  FORMAT(GetDate(), 'yyyyMMdd') THEN " & Misc.LeaseStatus.Cd.LimitExpired.ToString)
            sbSQL.AppendLine("    END AS DispLeaseStatus")
            sbSQL.AppendLine("  , AST.LEASE_DT AS DispLeaseKikanFrom ")             '貸出期間(From)
            sbSQL.AppendLine("  , AST.LEASE_DUE_DT AS DispLeaseKikanTo ")           '貸出期間(To)
            sbSQL.AppendLine("  , AST.REMARK AS DispLeaseBiko ")                    '貸出備考
            sbSQL.AppendLine("  , AST.LEASE_EMPLOYEE_CD AS DispLeaseEmployeeCd ")   '貸出先担当社員コード
            sbSQL.AppendLine("  , AST.LEASE_EMPLOYEE_NAME AS DispLeaseEmployeeNm ") '貸出先担当社員名
            sbSQL.AppendLine("  , AST.ASSET_NO ")                                   '資産番号
            sbSQL.AppendLine("  , AST.ASSET_NAME ")                                 '資産名称
            sbSQL.AppendLine("  , AST.MAKER ")                                      '資産製造元
            sbSQL.AppendLine("  , AST.PRODUCT_NO ")                                 '品番
            sbSQL.AppendLine("  , AST.MODEL_NO ")                                   '型番

            sbSQL.AppendLine("FROM  M_ASSET AST")

            '2021/10/09 ADD nv Inoue start
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbSQL.AppendLine("    ON AUTH.LOC_L_CD = AST.MNG_LOC_L_CD ")
            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{AuthInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{AuthInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{AuthInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{AuthInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType", $"{Misc.UpdAuthType.UPD_AUTH}")
            '2021/10/09 ADD nv Inoue end

            sbSQL.AppendLine("WHERE AST.ASSET_STATUS = " & ASSET_STATUS.Lending)

            '貸出期限切れ SearchLeaseLimitExpired
            If _ViewModel.SearchLeaseLimitExpired Then
                sbSQL.AppendLine(" AND   AST.LEASE_DUE_DT <  FORMAT(GetDate(), 'yyyyMMdd')")
            End If

            '貸出先コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCode) AndAlso _ViewModel.SearchCode <> "" Then
                sbSQL.AppendLine("AND   AST.LEASE_PLACE_CD = @LEASE_PLACE_CD")
            End If

            '貸出先備考 SearchLeaseBiko 
            If (Not IsNothing(_ViewModel.SearchLeaseBiko)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchLeaseBiko.Trim)) Then
                sbSQL.AppendLine("  AND   AST.REMARK like " & "@SearchLeaseBiko")
            End If

            '貸出担当者 SearchLeaseEmployeeCd 
            If Not String.IsNullOrEmpty(_ViewModel.SearchLeaseEmployeeCd) AndAlso _ViewModel.SearchLeaseEmployeeCd <> "" Then
                sbSQL.AppendLine("AND   AST.LEASE_EMPLOYEE_CD = @LEASE_EMPLOYEE_CD")
            End If

            '過去貸出有り SearchLeasePast
            If _ViewModel.SearchLeasePast Then

                sbSQL.AppendLine("UNION ALL")

                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("    IsNull(PLG.LEASE_PLACE_CD, '') AS LEASE_PLACE_CD ")
                sbSQL.AppendLine("  , PLG.LEASE_PLACE_NAME ")
                sbSQL.AppendLine("  , " & Misc.LeaseStatus.Cd.Returned.ToString & " AS DispLeaseStatus")
                sbSQL.AppendLine("  , PLG.LEASE_DT AS DispLeaseKikanFrom ")             '貸出期間(From)
                sbSQL.AppendLine("  , PLG.PROCESS_DT AS DispLeaseKikanTo ")             '貸出期間(To)
                sbSQL.AppendLine("  , PLG.REMARK AS DispLeaseBiko ")                    '貸出備考
                sbSQL.AppendLine("  , PLG.LEASE_EMPLOYEE_CD AS DispLeaseEmployeeCd ")   '貸出先担当社員コード
                sbSQL.AppendLine("  , PLG.LEASE_EMPLOYEE_NAME AS DispLeaseEmployeeNm ") '貸出先担当社員名
                sbSQL.AppendLine("  , PLG.ASSET_NO ")                                   '資産番号
                sbSQL.AppendLine("  , PLG.ASSET_NAME ")                                 '資産名称
                sbSQL.AppendLine("  , PLG.MAKER ")                                      '資産製造元
                sbSQL.AppendLine("  , PLG.PRODUCT_NO ")                                 '品番
                sbSQL.AppendLine("  , PLG.MODEL_NO ")                                   '型番

                sbSQL.AppendLine("FROM  T_PROCESS_LOG PLG")

                '2021/10/09 ADD nv Inoue start
                sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
                sbSQL.AppendLine("    ON AUTH.LOC_L_CD = PLG.MNG_LOC_L_CD ")
                '2021/10/09 ADD nv Inoue end

                sbSQL.AppendLine("WHERE PLG.PROCESS_DIV = '" & ProcessDiv.BI & "'")

                '貸出先コード
                If Not String.IsNullOrEmpty(_ViewModel.SearchCode) AndAlso _ViewModel.SearchCode <> "" Then
                    sbSQL.AppendLine("AND   PLG.LEASE_PLACE_CD = @LEASE_PLACE_CD")
                End If

                '貸出先備考 SearchLeaseBiko 
                If (Not IsNothing(_ViewModel.SearchLeaseBiko)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchLeaseBiko.Trim)) Then
                    sbSQL.AppendLine("  AND   PLG.REMARK like " & "@SearchLeaseBiko")
                End If

                '貸出担当者 SearchLeaseEmployeeCd 
                If Not String.IsNullOrEmpty(_ViewModel.SearchLeaseEmployeeCd) AndAlso _ViewModel.SearchLeaseEmployeeCd <> "" Then
                    sbSQL.AppendLine("AND   PLG.LEASE_EMPLOYEE_CD = @LEASE_EMPLOYEE_CD")
                End If

            End If

            '貸出先コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCode) AndAlso _ViewModel.SearchCode <> "" Then
                addSqlParam("LEASE_PLACE_CD", $"{_ViewModel.SearchCode}")
            End If

            '貸出先備考 SearchLeaseBiko 
            If (Not IsNothing(_ViewModel.SearchLeaseBiko)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchLeaseBiko.Trim)) Then
                addSqlParam("SearchLeaseBiko", $"{"%" & _ViewModel.SearchLeaseBiko.Trim & "%"}")
            End If

            '貸出担当者 SearchLeaseEmployeeCd 
            If Not String.IsNullOrEmpty(_ViewModel.SearchLeaseEmployeeCd) AndAlso _ViewModel.SearchLeaseEmployeeCd <> "" Then
                addSqlParam("LEASE_EMPLOYEE_CD", $"{_ViewModel.SearchLeaseEmployeeCd}")
            End If

            'クエリを返却
            Dim sbSQLALL As StringBuilder = New StringBuilder
            sbSQLALL.AppendLine("SELECT * ")
            sbSQLALL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQLALL.AppendLine("FROM")
            sbSQLALL.AppendLine("(")
            sbSQLALL.AppendLine(sbSQL.ToString)
            sbSQLALL.AppendLine(") MRG")
            sbSQLALL.AppendLine("ORDER BY DispLeaseKikanFrom DESC, ASSET_NO, DispLeaseStatus")

            'クエリを返却
            Return sbSQLALL.ToString()
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

#Region "社員マスタドロップダウンリストデータ取得処理"
        ''' <summary>
        ''' 社員マスタドロップダウンリストデータ取得処理
        ''' </summary>
        Public Sub GetEmployeeData()

            Dim lst As List(Of EmployeeModel)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext
            'M_EMPLOYEEテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT *")
            sbSQL.AppendLine(" FROM ")
            sbSQL.AppendLine("  M_EMPLOYEE ")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("AUTH_DIV <> '" & Misc.AUTH_DIV.SuperAdmin & "'")
            sbSQL.AppendLine("ORDER BY EMPLOYEE_CD")
            'SQLを実行する
            lst = db.Employee.SqlQuery(sbSQL.ToString).ToList
            '貸出先コンボ設定
            If String.IsNullOrEmpty(_ViewModel.SearchCode) Then
                ViewBag.EmployeeList = New SelectList(lst, "EMPLOYEE_CD", "EMPLOYEE_NAME", "")
            Else
                ViewBag.EmployeeList = New SelectList(lst, "EMPLOYEE_CD", "EMPLOYEE_NAME", _ViewModel.SearchLeaseEmployeeCd)
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
                _viewResult.Add(New AssetLeasePlaceDetailViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchCode = _ViewModel.SearchCode
            _viewResult.ElementAt(0).SearchLeaseLimitExpired = _ViewModel.SearchLeaseLimitExpired
            _viewResult.ElementAt(0).SearchLeasePast = _ViewModel.SearchLeasePast
            _viewResult.ElementAt(0).SearchLeaseBiko = _ViewModel.SearchLeaseBiko
            _viewResult.ElementAt(0).SearchLeaseEmployeeCd = _ViewModel.SearchLeaseEmployeeCd
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LEASE_PLACE_DETAIL)
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_LEASE_PLACE_DETAIL).FIX_COL_CNT

        End Sub

        '''' <summary>
        '''' Viewモデルを更新用のモデルにコピーする
        '''' </summary>
        'Private Sub CopyViewModeltoModel(ByVal model As AssetLeasePlaceDetailViewModel)
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