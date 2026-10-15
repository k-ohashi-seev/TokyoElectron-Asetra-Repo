Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports System.Web.WebPages
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' 仮置資産一覧画面表示用のクラス
    ''' </summary>
    Public Class TempAssetListController
        Inherits PageBaseController

#Region "クラス定数"

#End Region

#Region "クラス変数"
        Private _Model As TempAssetListModel = New TempAssetListModel                                 'TempAssetListモデル
        Private _ViewModel As TempAssetListViewModel = New TempAssetListViewModel                     'TempAssetList一覧表示用モデル
        Protected _viewResult As List(Of TempAssetListViewModel) = New List(Of TempAssetListViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _params As List(Of SqlParameter)                'SQLパラメータ
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
        Public Function Index(ByVal ViewModel As TempAssetListViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_TEMP_ASSET_LIST)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As TempAssetListViewModel = Session(CommonSessionKey.CREAGE_TEMP_ASSET_LIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
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

            '仮置き一覧データ取得
            GetTblTempData()

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
                Session.Remove(CommonSessionKey.CREAGE_TEMP_ASSET_LIST)
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
            Dim query As DbSqlQuery(Of TempAssetListViewModel) = _db.TempAssetListView.SqlQuery(_SQL, _params.ToArray)

            '画面に表示するViewを設定
            _viewResult = query.ToList

        End Sub

#End Region

#Region "登録・更新処理"
        ''' <summary>
        ''' アクションメソッド(Create)
        ''' 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        ''' 詳細については、http://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="OnCreteButton">クリックされたボタン</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        Public Function Create(ByVal ViewModel As TempAssetListViewModel, ByVal OnCreteButton As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                MyBase.BaseCreate(_ViewModel)
            Else
                '更新処理
                MyBase.BaseEdit(_ViewModel)
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeCreate()

            '親の初期処理
            MyBase.InitializeCreate()

            'モデルを登録用モデルにコピー
            CopyViewModeltoModel(_ViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeCreate()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_TEMP_ASSET_LIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeEdit()

            '親の初期処理
            MyBase.InitializeCreate()

            'モデルを登録用モデルにコピー
            CopyViewModeltoModel(_ViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeEdit()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_TEMP_ASSET_LIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            Return blnReturn
        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()
            '登録日時・登録社員コード・登録社員名・登録プロセスを登録
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelAssetHistory.TITLE)

            _db.TempAssetList.Add(_Model)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            Return blnReturn
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()
            '更新日時・更新社員コード・更新社員名・更新プロセスを登録
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelAssetHistory.TITLE)

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified

        End Sub

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ID">ID</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal ID As String) As JsonResult
            Dim db As New SystemDBContext

            'IDをキーにモデルを取得する
            Dim model As TempAssetListModel = db.TempAssetList.Find(ID)

            Return Json(model)

        End Function

#End Region

#Region "削除処理"
        ''' <summary>
        ''' アクションメソッド(Delete)
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        Public Overridable Function Delete(ByVal ViewModel As TempAssetListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '削除処理
            MyBase.BaseDelete(_ViewModel)

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeDelete()

            '親の初期処理
            MyBase.InitializeDelete()

            'モデルを削除用モデルにコピー
            CopyViewModeltoModel(_ViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        ''' <param name="model">基底モデル</param>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)

            MyBase.DisposeDelete(model)

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_TEMP_ASSET_LIST) = _ViewModel

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
            '特に無し
        End Sub
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ViewModel As TempAssetListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim TemporaryListData As TemporaryListViewModel

            '仮置一覧データ取得
            TemporaryListData = GetTblTempData().FirstOrDefault()

            Dim TempAssetListData As List(Of TempAssetListViewModel)

            '仮置資産一覧データ取得
            TempAssetListData = GetTempAssetListData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      '設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.TempAssetList.KeyDiv, CommonSettingKey.TempAssetList.Key1, CommonSettingKey.TempAssetList.Key2, CommonSettingKey.TempAssetList.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

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
            'CSVヘッダー取得
            Dim sHeader As String
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_TEMP_ASSET_LIST)
            sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_TEMPORARY_ASSET_LIST)


            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As TempAssetListViewModel In TempAssetListData
                col = ""

                If String.IsNullOrEmpty(TemporaryListData.REL_CONST_NO) Then
                    col += SVClsCSVExp.SVFncCSVAddDquot(REL_CONST_STATUS_CSV.Unlinked)
                Else
                    col += SVClsCSVExp.SVFncCSVAddDquot(REL_CONST_STATUS_CSV.Linked)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.REL_CONST_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.REG_CONST_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(TemporaryListData.TEMP_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.CONST_CONTENTS)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.TEMP_INFO1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.TEMP_INFO2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.TEMP_INFO3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.TEMP_INFO4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.TEMP_INFO5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.TEMP_INFO6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.TEMP_INFO7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.TEMP_REMARK)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(TemporaryListData.TEMP_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetHistory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     TempAssetListData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetHistory.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetTempAssetListData() As List(Of TempAssetListViewModel)
            Dim lstTempAssetList As List(Of TempAssetListViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstTempAssetList = db.TempAssetListView.SqlQuery(CreateSQL(), _params.ToArray).ToList

            'データを返却
            Return lstTempAssetList
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

#Region "仮置き一覧データ取得"
        ''' <summary>
        ''' 仮置き一覧データ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetTblTempData() As List(Of TemporaryListViewModel)
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.TempAssetList, _LoginInfo)
            Dim lst As New List(Of TemporaryListViewModel)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("    TEMP.TEMP_ID ")
            sbSQL.AppendLine("    ,TEMP.REG_CONST_NO ")
            sbSQL.AppendLine("    ,TEMP.REL_CONST_NO ")
            sbSQL.AppendLine("    ,TEMP.TEMP_DT ")
            sbSQL.AppendLine("    ,CONST.CONST_CONTENTS ")
            sbSQL.AppendLine("    ,TEMP.WORK_EMPLOYEE_CD ")
            sbSQL.AppendLine("    ,TEMP.WORK_EMPLOYEE_NAME ")
            sbSQL.AppendLine("    ,TEMP.TEMP_INFO1 ")
            sbSQL.AppendLine("    ,TEMP.TEMP_INFO2 ")
            sbSQL.AppendLine("    ,TEMP.TEMP_INFO3 ")
            sbSQL.AppendLine("    ,TEMP.TEMP_INFO4 ")
            sbSQL.AppendLine("    ,TEMP.TEMP_INFO5 ")
            sbSQL.AppendLine("    ,TEMP.TEMP_INFO6 ")
            sbSQL.AppendLine("    ,TEMP.TEMP_INFO7 ")
            sbSQL.AppendLine("    ,TEMP.TEMP_REMARK ")
            sbSQL.AppendLine("    ,TEMP.TASK_TS ")
            sbSQL.AppendLine("    ,TEMP.INS_DATETIME ")
            sbSQL.AppendLine("    ,TEMP.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("    ,TEMP.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("    ,TEMP.UPD_DATETIME ")
            sbSQL.AppendLine("    ,TEMP.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("    ,TEMP.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("    ,TEMP.UPD_TASK ")
            sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_TEMP_ASSET WHERE TEMP_ID = TEMP.TEMP_ID ) AS TEMP_NUM ")
            sbSQL.AppendLine("    , AUTH.UPD_FLG AS UPD_FLG ")
            sbSQL.AppendLine("    ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM T_TEMPORARY TEMP ")
            sbSQL.AppendLine("LEFT JOIN M_CONSTRUCTION CONST ")
            sbSQL.AppendLine("ON TEMP.REG_CONST_NO = CONST.CONST_NO ")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbSQL.AppendLine("ON CONST.MNG_LOC_L_CD = AUTH.LOC_L_CD")

            params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@ReadAuthFlg", AuthInfo.ReadAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthFlg", AuthInfo.UpdAuthFlg))
            params.Add(New SqlParameter($"@ReadHubAuthFlg", AuthInfo.ReadHubAuthFlg))
            params.Add(New SqlParameter($"@UpdHubAuthFlg", AuthInfo.UpdHubAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthType", UpdAuthType.UPD_AUTH))

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '検索条件を設定する
            Dim selectTempId As String = Session(CommonSessionKey.CREAGE_TEMP_ASSET_LIST_KEY)

            '仮置きID
            If Not String.IsNullOrEmpty(selectTempId) Then
                sbSQL.AppendLine("  AND TEMP.TEMP_ID = @TEMP_ID")
                params.Add(New SqlParameter($"@TEMP_ID", selectTempId))
            End If

            'ORDER句の設定
            sbSQL.AppendLine(" ORDER BY TEMP.TEMP_ID")

            lst = _db.TemporaryListView.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            If lst Is Nothing OrElse lst.Count = 0 Then
                Return Nothing
            End If

            ViewBag.tblTemporaryListData = lst
            _ViewModel.tblTemporaryListData = lst

            Return lst
        End Function
#End Region

#Region "一覧データ取得用SQLを作成する"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String

            If IsNothing(Me._Message) Then
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            End If
            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.TempAssetList, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            _params = New List(Of SqlParameter)

            '一覧表示表用SQL
            sbSQL.AppendLine("  SELECT  ")
            sbSQL.AppendLine("    TASSET.TEMP_ID ")
            sbSQL.AppendLine("  , TASSET.ASSET_NO ")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("  , TASSET.INS_DATETIME ")
            sbSQL.AppendLine("  , TASSET.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , TASSET.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , TASSET.UPD_DATETIME ")
            sbSQL.AppendLine("  , TASSET.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , TASSET.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , TASSET.UPD_TASK ")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("  FROM T_TEMP_ASSET TASSET ")
            sbSQL.AppendLine("  LEFT JOIN M_ASSET ASSET ")
            sbSQL.AppendLine("    ON ASSET.ASSET_NO = TASSET.ASSET_NO ")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '検索条件を設定する
            Dim selectTempId As String = Session(CommonSessionKey.CREAGE_TEMP_ASSET_LIST_KEY)
            '仮置きID
            If Not String.IsNullOrEmpty(selectTempId) Then
                sbSQL.AppendLine("  AND TASSET.TEMP_ID = @TEMP_ID")
                _params.Add(New SqlParameter($"@TEMP_ID", selectTempId))
            End If

            sbSQL.AppendLine(" ORDER BY TASSET.ASSET_NO")

            Return sbSQL.ToString()
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
                _viewResult.Add(New TempAssetListViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).GridInfoTop = ToolColSet.GetGridInfo(_db, DispId.TEMP_ASSET_LIST_TOP)
            _viewResult.ElementAt(0).FixColCntTop = _db.ColSet.Find(DispId.TEMP_ASSET_LIST_TOP).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfoBottom = ToolColSet.GetGridInfo(_db, DispId.TEMP_ASSET_LIST_BOTTOM)
            _viewResult.ElementAt(0).FixColCntBottom = _db.ColSet.Find(DispId.TEMP_ASSET_LIST_BOTTOM).FIX_COL_CNT
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As TempAssetListViewModel)
            _Model.ASSET_NO = model.ASSET_NO
            _Model.ASSET_NAME = model.ASSET_NAME
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub
#End Region

    End Class
End Namespace