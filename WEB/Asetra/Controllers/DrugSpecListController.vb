Imports Asetra.Models
Imports System.Web.Mvc
Imports Asetra.Misc
Imports System.Data.SqlClient
Imports System.IO

Namespace Controllers
    Public Class DrugSpecListController
        Inherits PageBaseController

#Region "クラス定数"

#End Region

#Region "クラス変数"
        Private _ViewModel As DrugSpecListViewModel = New DrugSpecListViewModel                     '指定数量管理表出力用表示用モデル
        Private _LoginInfo As RfidEntity.LoginUser = Nothing                                        'ログイン情報
        Private _Message As MessageRfid = Nothing

#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As DrugSpecListViewModel, ByVal actionname As String) As ActionResult

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
        Function Index(ByVal ViewModel As DrugSpecListViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_DRUGSPECLIST)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As DrugSpecListViewModel = Session(CommonSessionKey.CREAGE_DRUGSPECLIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
                Session.Remove(CommonSessionKey.CREAGE_DRUGSPECLIST)
            Else
                If String.IsNullOrWhiteSpace(actionname) Then
                    '先月の月末をFrom、システム日付の前日をToへセット
                    ViewModel.SearchDateFrom = Date.Parse(Date.Now.ToString("yyyy-MM-") + "01").AddDays(-1).ToString("yyyy-MM-dd")
                    ViewModel.SearchDateTo = Date.Now.AddDays(-1).ToString("yyyy-MM-dd")
                End If
                _ViewModel = ViewModel
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm

                ViewBag.LoginInfo = _LoginInfo
            End If

            '保管場所ドロップダウンリストデータ取得
            GetStorageLocData()

            Return View(_ViewModel)
        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()
        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()
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

#Region "保管場所ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 保管場所ドロップダウンリストデータ取得
        ''' </summary>
        Private Sub GetStorageLocData()

            '必要な情報がない場合は空白をセット（タイムアウトの場合など）
            If IsNothing(_Message) OrElse IsNothing(_LoginInfo) Then
                ViewBag.modelLocL = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                Return
            End If

            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            'SQL文
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  AUTH.LOC_L_CD AS LOC_CD")
            sbSQL.AppendLine("  , CONCAT(AUTH.LOC_L_CD, '" & CommonConst.CodeNameDelimiter & "', AUTH.LOC_NAME) AS LOC_NAME ")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH ")
            sbSQL.AppendLine("ORDER BY LOC_CD ")

            '条件設定
            params.Add(New SqlParameter("EmployeeCd", $"{_LoginInfo.EmployeeCd}"))
            params.Add(New SqlParameter("ReadAuthFlg", $"{AuthInfo.ReadAuthFlg}"))
            params.Add(New SqlParameter("UpdAuthFlg", $"{AuthInfo.UpdAuthFlg}"))
            params.Add(New SqlParameter("ReadHubAuthFlg", $"{AuthInfo.ReadHubAuthFlg}"))
            params.Add(New SqlParameter("UpdHubAuthFlg", $"{AuthInfo.UpdHubAuthFlg}"))
            params.Add(New SqlParameter("UpdAuthType", $"{Misc.UpdAuthType.UPD_AUTH}"))

            'ロケ大区分データ取得
            Dim lstLocation As List(Of LocationDropdownModel) = _db.LocationDropdown.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            '画面のロケーションコンボ設定
            ViewBag.modelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", "")
            If Not IsNothing(_ViewModel) Then
                _ViewModel.StorageLocDropdownData = lstLocation
            End If

        End Sub
#End Region

#Region "帳票出力"
        ''' <summary>
        ''' 帳票出力
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="actionname">アクション名</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Function ReportOutput(ByVal ViewModel As DrugSpecListViewModel, ByVal actionname As String) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            _ViewModel = ViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '期限（From）が空白もしくは数値ではない場合、-1をセット
            Dim iSearhDtFrom As Integer
            If String.IsNullOrEmpty(ViewModel.SearchDateFrom) OrElse Integer.TryParse(ViewModel.SearchDateFrom.Replace("-", ""), iSearhDtFrom) = False Then
                iSearhDtFrom = -1
            End If

            '期限（To）が空白もしくは数値ではない場合、-1をセット
            Dim iSearhDtTo As Integer
            If String.IsNullOrEmpty(ViewModel.SearchDateTo) OrElse Integer.TryParse(ViewModel.SearchDateTo.Replace("-", ""), iSearhDtTo) = False Then
                iSearhDtTo = -1
            End If

            '出力用データ取得
            Dim lstDrugLogDaily As List(Of DrugSpecInfo) = Dao.ToolDrugSpecList.GetDrugSpecListData(ViewModel.SelectStorageLoc, iSearhDtFrom, iSearhDtTo)
            If IsNothing(lstDrugLogDaily) OrElse lstDrugLogDaily.Count <= 0 Then
                '出力データが存在しない場合
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_NOT_EXISTS_OUT_PUT_DATA

                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_DRUGSPECLIST) = _ViewModel

                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'CSVファイル出力先フォルダに作成する
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'Excelテンプレートパスの設定
            Dim sTempFullPath As String = Server.MapPath(CommonConst.TEMPLATE_FOLDERNM)

            'PDFファイル出力名取得
            Dim setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.DirectPrint.KeyDiv, CommonSettingKey.DirectPrint.Key1, CommonSettingKey.DirectPrint.Key2, CommonSettingKey.DirectPrint.Key3, True)
            Dim sDspFileName As String = String.Format(setting.DATA1 + "_{0}.pdf", Now.ToString("yyyyMMddHHmmss"))

            '指定数量管理表PDF一時ファイル名取得
            Dim sTmpPdfFileNm As String = Dao.ToolDrugSpecList.MakeDrugSpecList(sTempFullPath, sFullPath, lstDrugLogDaily)

            'ファイルをダウンロードさせる
            Call FileDownload(sDspFileName, "application/pdf", sTmpPdfFileNm)

            'ダウンロードさせた後、ファイルを削除。
            System.IO.File.Delete(sTmpPdfFileNm)

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelDrugSpecList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_REPORT_OUTPUT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     lstDrugLogDaily.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelDrugSpecList.TITLE))

            '結果表示
            Return New EmptyResult()
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

    End Class
End Namespace