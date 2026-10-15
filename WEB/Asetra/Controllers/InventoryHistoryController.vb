Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports Asetra.Dao
Imports GrapeCity.Documents.Excel
Imports System.IO.Compression


Namespace Controllers
    ''' <summary>
    ''' 棚卸実績一覧画面表示用のクラス
    ''' </summary>
    Public Class InventoryHistoryController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _Model As InventoryModel = New InventoryModel                                'Employeeモデル
        Private _ViewModel As InventoryHistoryViewModel = New InventoryHistoryViewModel                         'InventoryHistory一覧表示用モデル
        Protected _viewResult As List(Of InventoryHistoryViewModel) = New List(Of InventoryHistoryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _params As List(Of SqlParameter)                'SQLパラメータ
        Private _AuthInfo As AuthInfo = Nothing                 '権限情報
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As InventoryHistoryViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
            End If

            If Session(CommonSessionKey.INV_RETURN_LOC) IsNot Nothing Then
                Session.Remove(CommonSessionKey.INV_RETURN_LOC)
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
        Public Function Index(ByVal ViewModel As InventoryHistoryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_HISTORY)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '検索ボタンがクリックされた場合
            If Me._Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session(CommonSessionKey.SEARCH_FLG) = True
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As InventoryHistoryViewModel = Session(CommonSessionKey.CREAGE_INVENTORY_HISTORY)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            '戻ってきた際の検索
            If Session(CommonSessionKey.INV_RETURN_LOC) IsNot Nothing Then
                _ViewModel.SearchLocL = Session(CommonSessionKey.INV_RETURN_LOC)
                'クリア
                Session.Remove(CommonSessionKey.INV_RETURN_LOC)
            End If


            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
                ViewBag.modelLocL = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                CopyModelData()
                Return View(_viewResult)
            Else
                '--2021/11/22 add start nv iwamoto
                '権限情報の取得
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryHistory, _LoginInfo)
                If _AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                End If
                '--2021/11/22 add end nv iwamoto

                '検索条件の復元有無
                _ViewModel.IsRestoreOwnSearchConditions = True

                'セッションSEARCH_FLGがNothingの場合
                If Session(CommonSessionKey.SEARCH_FLG) Is Nothing Then
                    '初期表示(一覧表示はしない)
                    '初回表示フラグ True
                    _ViewModel.FirstSearch = True
                Else
                    '検索ボタンから表示
                    '初回表示フラグ False
                    _ViewModel.FirstSearch = False
                    '表示する
                    MyBase.BaseIndex(_ViewModel)
                End If

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.INVENTORY_HISTORY, COLSET_LOC.TEMPLATE)
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

            'ロケーション
            ' 大区分ドロップダウンリスト作成
            GetLocationLData()

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
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_HISTORY)
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
            Dim query As DbSqlQuery(Of InventoryHistoryViewModel) = _db.InventoryHistoryView.SqlQuery(_SQL, _params.ToArray)
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
        <ValidateInput(False)>
        Public Function Create(ByVal ViewModel As InventoryHistoryViewModel, ByVal OnCreteButton As String) As ActionResult
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

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_INV_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.INV_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE))

                    _ViewModel.States = StatusType.SuccessCreate
                Else
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_INV_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.INV_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE))

                    _ViewModel.States = StatusType.SuccessEdit
                End If
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
            Session(CommonSessionKey.CREAGE_INVENTORY_HISTORY) = _ViewModel
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
            Session(CommonSessionKey.CREAGE_INVENTORY_HISTORY) = _ViewModel
        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True
            'IDが登録済みかチェックする
            Dim InventoryHistory As InventoryModel = _db.Inventory.Find(_Model.INV_ID)
            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(InventoryHistory) Then
                ModelState.AddModelError("START_DEC_TS", CommonErrorMsgMember.DUPLICATION_ID)
                blnReturn = False
            End If
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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE)
            _db.Inventory.Add(_Model)
        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As InventoryModel = db.Inventory.Find(_Model.INV_ID)

            '他端末で更新されているかチェックする
            If _Model.UPD_DATETIME.ToString() <> model.UPD_DATETIME.ToString() Then
                'モデルとDBの更新日付が異なっている場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                blnReturn = False
            End If

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE)

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

            '棚卸IDだけ取得
            ID = ID.Split("/")(1)

            'IDをキーにモデルを取得する
            Dim model As InventoryModel = db.Inventory.Find(Convert.ToInt64(ID))
            model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

            Return Json(model)

        End Function

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

#Region "棚卸実績処理"
        ''' <summary>
        ''' 棚卸実績処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ByVal ViewModel As InventoryHistoryViewModel, ByVal SelectKeys As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            _ViewModel = ViewModel

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.InventoryHistory.KeyDiv, CommonSettingKey.InventoryHistory.Key1, CommonSettingKey.InventoryHistory.Key2, CommonSettingKey.InventoryHistory.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

            'エクセルファイル出力
            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'CSVヘッダー取得
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_STOCK_LIST)
            Dim sHeaderAbumn As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_CONSTRUCTION_ASSET_LIST)
            Dim sHeaderAS02 As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_CONSTRUCTION_HISTORY_LIST)

            '各件数取得
            Dim AbumnDatacnt As Integer = 0
            Dim Pattern1Datacnt As Integer = 0
            Dim Pattern2Datacnt As Integer = 0

            'テンプレートフォルダ
            Dim sTempFullPath As String = Server.MapPath(CommonConst.TEMPLATE_FOLDERNM)

            Dim ZipFile As String = ToolInventory.InvoiceResultOutPut(SelectKeys, sFullPath, sTempFullPath, sHeaderAbumn, sHeaderAS02, AbumnDatacnt, Pattern1Datacnt, Pattern2Datacnt)

            '棚卸IDで棚卸テーブルから棚卸名を取得
            Dim db As New SystemDBContext
            'DBのデータを取得する
            Dim model As InventoryModel = db.Inventory.Find(Convert.ToInt64(SelectKeys))

            'zipファイル名前
            Dim ZipFileName As String = String.Format(model.INV_NAME + "_{0}.zip", Now.ToString("yyyyMMddHHmmdd"))
            'zipファイルダウンロード
            Call FileDownload(ZipFileName, "application/zip", ZipFile)

            'zipファイル削除
            System.IO.File.Delete(ZipFile)

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_INVENTRY_RESULT_OUTPUT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     "ABUMN_MALSY.xlsx　" & AbumnDatacnt & "," &
                                                    "AS02Pattern_1_MALSY.xlsx　" & Pattern1Datacnt & "," &
                                                    "AS02Pattern_2_MALSY.xlsx　" & Pattern2Datacnt
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE))



            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region

        '#Region "CSV出力用のデータを取得"
        '        ''' <summary>
        '        ''' CSV出力用のデータを取得します
        '        ''' </summary>
        '        Private Function GetInventoryHistoryData() As List(Of InventoryHistoryViewModel)
        '            Dim lstInventoryHistory As List(Of InventoryHistoryViewModel)
        '            Dim db As SystemDBContext = New SystemDBContext
        '            'SQLを実行する
        '            lstInventoryHistory = db.InventoryHistoryView.SqlQuery(CreateSQL(), _params.ToArray).ToList
        '            'データを返却
        '            Return lstInventoryHistory
        '        End Function
        '#End Region

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

            'ダウンロード完了検知用にクッキーを設定
            Response.Cookies.Add(New HttpCookie("fileDownloadToken", "done") With {.Path = "/"})

            'HTTPヘッダー情報・MIMEタイプ設定
            Response.AddHeader("Content-Disposition", String.Format("attachment;filename={0}", dlFileName))
            Response.ContentType = MimeType

            'ファイルを書き出し
            Response.WriteFile(sFileFullPath)

            Response.Flush()
            Response.End()
        End Sub
#End Region

#Region "開始宣言日時をセッションに保存し、実棚実績照会画面に遷移します"
        ''' <summary>
        ''' 開始宣言日時をセッションに保存し、実棚実績照会画面に遷移します
        ''' </summary>
        <ValidateInput(False)>
        Public Function SetKeys(ByVal ViewModel As AssetListViewModel, ByVal SelectKeys As String) As ActionResult
            '開始宣言日時をセッションに保存
            Session.Remove(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY)
            Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY) = SelectKeys

            '結果表示
            Return RedirectToAction("Index", "InventoryAssetHistory")

        End Function
#End Region

#Region "過去履歴を削除します"
        ''' <summary>
        ''' 過去履歴を削除します
        ''' </summary>
        <ValidateInput(False)>
        Public Function Delete(ViewModel As InventoryHistoryViewModel, ByVal SelectKeys As String) As ActionResult
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

            '過去実績削除処理
            Dim skeys As String() = SelectKeys.Split(",")
            DeleteProc(skeys)

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_INVENTORY_HISTORY) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "過去実績削除実処理"
        ''' <summary>
        ''' 過去実績削除実処理
        ''' </summary>
        ''' <param name="skeys">削除するデータの主キー（開始宣言日時）</param>
        Public Sub DeleteProc(skeys As String())
            Dim db As New SystemDBContext
            Dim sql As New StringBuilder
            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try
                    For icnt As Integer = 0 To skeys.Length - 1
                        '棚卸管理テーブル・過去棚卸実績テーブル
                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_INVENTORY")
                        sql.AppendLine("WHERE INV_ID = " & skeys(icnt))
                        db.Database.ExecuteSqlCommand(sql.ToString)

                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_INV_DETAIL")
                        sql.AppendLine("WHERE INV_ID = " & skeys(icnt))
                        db.Database.ExecuteSqlCommand(sql.ToString)

                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_INV_RESULT")
                        sql.AppendLine("WHERE INV_ID = " & skeys(icnt))
                        db.Database.ExecuteSqlCommand(sql.ToString)
                    Next

                    tran.Commit()

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                      skeys.Length.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE))

                    '正常終了時にメッセージを表示する
                    _ViewModel.States = StatusType.SuccessDelete

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sql)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using


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

#Region "大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationLData() As JsonResult

            If IsNothing(_AuthInfo) Then
                '権限情報の取得
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryHistory, _LoginInfo)
            End If

            Dim tbl As DataTable = Dao.ToolLocation.GetLocationLData(_LoginInfo, _AuthInfo)
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next
            '一覧画面のロケーションコンボ設定
            ViewBag.modelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocL)

            Return Json(lstLocation)
        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String
            Dim sbSQL As StringBuilder = New StringBuilder
            '20231013 UPD Start Shirakawa
            '一覧表示表用SQL
            sbSQL.Clear()
            'sbSQL.AppendLine("SELECT")
            'sbSQL.AppendLine("INVD.INV_ID")
            'sbSQL.AppendLine(",STRING_AGG(INVD.INV_LOC_L_CD,'：') As LOC_L_CD")
            'sbSQL.AppendLine(",STRING_AGG(MLCL.LOC_NAME,'：') As LOC_L_NAME")
            'sbSQL.AppendLine(",STRING_AGG(INVD.INV_LOC_L_CD +　'：' + MLCL.LOC_NAME,' ')As LOC_CD_NAME")
            'sbSQL.AppendLine(",INV.START_DEC_TS")
            'sbSQL.AppendLine(",INV.START_DEC_EMPLOYEE_CD")
            'sbSQL.AppendLine(",INV.START_DEC_NAME")
            'sbSQL.AppendLine(",INV.INV_NAME")
            'sbSQL.AppendLine(",INV.INV_CONFIRM_TS")
            'sbSQL.AppendLine(",INV.INV_CONFIRM_EMPLOYEE_CD")
            'sbSQL.AppendLine(",INV.INV_CONFIRM_NAME")
            'sbSQL.AppendLine(",SUM(INVD.LOGIC_INV_NUM) As LOGIC_INV_NUM")
            'sbSQL.AppendLine(",SUM(INVD.PHYINV_NUM) As PHYINV_NUM")
            'sbSQL.AppendLine(",SUM(INVD.LOC_DIFF_NUM) As LOC_DIFF_NUM")
            'sbSQL.AppendLine(",SUM(INVD.NO_ITEM_NUM) As NO_ITEM_NUM")
            'sbSQL.AppendLine(",INVD.INV_ALL_CONFIRM_TS")
            'sbSQL.AppendLine(",INV.UPD_DATETIME")
            'sbSQL.AppendLine(",INV.UPD_EMPLOYEE_CD")
            'sbSQL.AppendLine(",INV.UPD_EMPLOYEE_NAME")
            'sbSQL.AppendLine(",INV.UPD_TASK")
            'sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount ")
            'sbSQL.AppendLine("FROM T_INVENTORY INV ")
            'sbSQL.AppendLine("LEFT JOIN T_INV_DETAIL INVD ")
            'sbSQL.AppendLine("ON  INVD.INV_ID = INV.INV_ID ")
            'sbSQL.AppendLine("LEFT JOIN M_LOCATION MLCL")
            'sbSQL.AppendLine("ON  MLCL.LOC_DIV = '" & LOC_DIV.LocationL & "'")
            'sbSQL.AppendLine("AND MLCL.LOC_L_CD = INVD.INV_LOC_L_CD")
            'sbSQL.AppendLine("WHERE INV.INV_CONFIRM_TS IS NOT NULL ")
            'sbSQL.AppendLine("  AND INVD.INV_ALL_CONFIRM_TS IS NOT NULL ")
            'sbSQL.AppendLine("GROUP BY")
            'sbSQL.AppendLine("INVD.INV_ID,")
            'sbSQL.AppendLine("INV.START_DEC_TS,")
            'sbSQL.AppendLine("INV.START_DEC_EMPLOYEE_CD,")
            'sbSQL.AppendLine("INV.START_DEC_NAME,")
            'sbSQL.AppendLine("INV.INV_NAME,")
            'sbSQL.AppendLine("INV.INV_CONFIRM_TS,")
            'sbSQL.AppendLine("INV.INV_CONFIRM_EMPLOYEE_CD,")
            'sbSQL.AppendLine("INV.INV_CONFIRM_NAME,")
            'sbSQL.AppendLine("INVD.INV_ALL_CONFIRM_TS,")
            'sbSQL.AppendLine("INV.UPD_DATETIME,")
            'sbSQL.AppendLine("INV.UPD_EMPLOYEE_CD,")
            'sbSQL.AppendLine("INV.UPD_EMPLOYEE_NAME,")
            'sbSQL.AppendLine("INV.UPD_TASK")
            'sbSQL.AppendLine("ORDER BY")
            'sbSQL.AppendLine("INV.START_DEC_TS DESC")

            Dim sb As StringBuilder = New StringBuilder
            _params = New List(Of SqlParameter)
            sb.Append("SELECT ")
            sb.Append("    INV.INV_ID ")
            sb.Append("    , (SELECT ISNULL(STRING_AGG(DET.INV_LOC_L_CD, '：'), ' ') FROM T_INV_DETAIL DET WHERE DET.INV_ID = INV.INV_ID) As LOC_L_CD ")
            sb.Append("    , (SELECT STRING_AGG(MLCL.LOC_NAME, '：') FROM T_INV_DETAIL DET INNER JOIN M_LOCATION MLCL ON MLCL.LOC_DIV = 'L' AND MLCL.LOC_L_CD = DET.INV_LOC_L_CD WHERE DET.INV_ID = INV.INV_ID ) As LOC_L_NAME ")
            sb.Append("    , (SELECT STRING_AGG(DET.INV_LOC_L_CD + '：' + MLCL.LOC_NAME, ' ') FROM T_INV_DETAIL DET INNER JOIN M_LOCATION MLCL ON MLCL.LOC_DIV = '" & LOC_DIV.LocationL & "' AND MLCL.LOC_L_CD = DET.INV_LOC_L_CD WHERE DET.INV_ID = INV.INV_ID ) As LOC_CD_NAME ")
            sb.Append("    , INV.START_DEC_TS ")
            sb.Append("    , INV.START_DEC_EMPLOYEE_CD ")
            sb.Append("    , INV.START_DEC_NAME ")
            sb.Append("    , ISNULL(INV.INV_NAME, '') AS INV_NAME ")
            sb.Append("    , INV.INV_ALL_CONFIRM_TS INV_CONFIRM_TS ")
            sb.Append("    , INV.INV_CONFIRM_EMPLOYEE_CD ")
            sb.Append("    , INV.INV_CONFIRM_NAME ")
            sb.Append("    , CONVERT(BIGINT,LOGIC_INV_NUM) AS LOGIC_INV_NUM")
            sb.Append("    , CONVERT(BIGINT,PHYINV_NUM) AS PHYINV_NUM")
            sb.Append("    , CONVERT(BIGINT,LOC_DIFF_NUM) AS LOC_DIFF_NUM")
            sb.Append("    , CONVERT(BIGINT,NO_ITEM_NUM) AS NO_ITEM_NUM")
            sb.Append("    , INV.INV_ALL_CONFIRM_TS ")
            sb.Append("    , INV.UPD_DATETIME ")
            sb.Append("    , INV.UPD_EMPLOYEE_CD ")
            sb.Append("    , INV.UPD_EMPLOYEE_NAME ")
            sb.Append("    , INV.UPD_TASK ")
            sb.Append("    , COUNT(*) OVER () AS TotalCount ")
            sb.Append("FROM ")
            sb.Append("    ( ")
            sb.Append("        SELECT ")
            sb.Append("            INV.INV_ID ")
            sb.Append("            , INV.START_DEC_TS ")
            sb.Append("            , INV.START_DEC_EMPLOYEE_CD ")
            sb.Append("            , INV.START_DEC_NAME ")
            sb.Append("            , INV.INV_NAME ")
            sb.Append("            , INV.INV_CONFIRM_TS ")
            sb.Append("            , INV.INV_CONFIRM_EMPLOYEE_CD ")
            sb.Append("            , INV.INV_CONFIRM_NAME ")
            sb.Append("            , INV.UPD_DATETIME ")
            sb.Append("            , INV.UPD_EMPLOYEE_CD ")
            sb.Append("            , INV.UPD_EMPLOYEE_NAME ")
            sb.Append("            , INV.UPD_TASK ")
            sb.Append("            , INV.INV_ALL_CONFIRM_TS ")
            sb.Append("            , INVD.INV_LOC_L_CD ")
            sb.Append("        FROM ")
            sb.Append("            T_INVENTORY INV ")
            sb.Append("        INNER JOIN T_INV_DETAIL INVD ")
            sb.Append("        ON INV.INV_ID = INVD.INV_ID ")
            sb.Append("        WHERE ")
            sb.Append("            INV.INV_CONFIRM_TS IS NOT NULL ")
            '検索条件を設定する
            'ロケ大区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                sb.Append("  AND INVD.INV_LOC_L_CD = @SearchLocL")
                _params.Add(New SqlParameter($"@SearchLocL", _ViewModel.SearchLocL))
            End If
            sb.Append("    ) INV ")

            sb.Append(" LEFT JOIN (")
            sb.Append("  SELECT")
            sb.Append("  ia.INV_ID,ia.LOC_L_CD")
            '-- 明細部　帳簿在庫数 SUM(保管、貸出)
            sb.Append($", SUM(")
            sb.Append($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN 1")
            sb.Append($"  ELSE 0 END")
            sb.Append($"  ) LOGIC_INV_NUM")
            '-- 明細部　正常数 SUM(正常)
            sb.Append($", SUM(")
            sb.Append($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sb.Append($"     CASE WHEN PHYINV_STATUS = '{PHYINV_STATUS.Normal}' THEN 1 ELSE 0 END")
            sb.Append($"  ELSE 0 END")
            sb.Append($"  ) PHYINV_NUM")
            '-- 明細部　ロケ違い数 SUM(警告)
            sb.Append($", SUM(")
            sb.Append($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sb.Append($"     CASE WHEN PHYINV_STATUS = '{PHYINV_STATUS.Warning}' THEN 1 ELSE 0 END")
            sb.Append($"  ELSE 0 END")
            sb.Append($"  ) LOC_DIFF_NUM")
            '-- 明細部　在庫なし数 SUM(現物なし(未処理を含む))
            sb.Append($", SUM(")
            sb.Append($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sb.Append($"     CASE WHEN PHYINV_STATUS = '{PHYINV_STATUS.NoSpot}' THEN 1 ELSE 0 END")
            sb.Append($"  ELSE 0 END")
            sb.Append($"  ) NO_ITEM_NUM")
            sb.Append(" FROM T_INV_RESULT ia")
            sb.Append(" INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AUTH")
            sb.Append(" ON  AUTH.LOC_L_CD = ia.LOC_L_CD")
            sb.Append(" AND ((AUTH.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND AUTH.LOC_M_CD = ia.LOC_M_CD) OR (AUTH.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            sb.Append(" GROUP BY ia.INV_ID,ia.LOC_L_CD ")
            sb.Append(" ) ia ")
            sb.Append(" ON  INV.INV_ID = ia.INV_ID AND INV.INV_LOC_L_CD = ia.LOC_L_CD ")

            _params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
            _params.Add(New SqlParameter($"@ReadAuthFlg", _AuthInfo.ReadAuthFlg))
            _params.Add(New SqlParameter($"@UpdAuthFlg", _AuthInfo.UpdAuthFlg))
            _params.Add(New SqlParameter($"@ReadHubAuthFlg", _AuthInfo.ReadHubAuthFlg))
            _params.Add(New SqlParameter($"@UpdHubAuthFlg", _AuthInfo.UpdHubAuthFlg))
            _params.Add(New SqlParameter($"@UpdAuthOption", _AuthInfo.AuthOption))

            sb.Append("ORDER BY ")
            sb.Append("    INV.START_DEC_TS DESC ")


            Return sb.ToString()
            '20231013 UPD End Shirakawa
        End Function

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New InventoryHistoryViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchLocL = _ViewModel.SearchLocL
            _viewResult.ElementAt(0).CsvPath = _ViewModel.CsvPath
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            '_viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_HISTORY, "TEMPLATE")
            '_viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_HISTORY, "TEMPLATE").FIX_COL_CNT

            If String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_HISTORY, COLSET_LOC.TEMPLATE)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_HISTORY, COLSET_LOC.TEMPLATE).FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_HISTORY, _ViewModel.SearchLocL)
                If _viewResult.ElementAt(0).GridInfo.Count > 0 Then
                    _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_HISTORY, _ViewModel.SearchLocL).FIX_COL_CNT
                Else
                    _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_HISTORY, COLSET_LOC.TEMPLATE).FIX_COL_CNT
                    _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_HISTORY, COLSET_LOC.TEMPLATE)
                End If
            End If
            _viewResult.ElementAt(0).IsRestoreOwnSearchConditions = _ViewModel.IsRestoreOwnSearchConditions
            _viewResult.ElementAt(0).FirstSearch = _ViewModel.FirstSearch

        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As InventoryHistoryViewModel)
            _Model.INV_ID = model.INV_ID
            _Model.START_DEC_TS = model.START_DEC_TS
            _Model.START_DEC_EMPLOYEE_CD = model.START_DEC_EMPLOYEE_CD
            _Model.START_DEC_NAME = model.START_DEC_NAME
            _Model.INV_NAME = model.INV_NAME
            _Model.INV_CONFIRM_TS = model.INV_CONFIRM_TS
            _Model.INV_CONFIRM_EMPLOYEE_CD = model.INV_CONFIRM_EMPLOYEE_CD
            _Model.INV_CONFIRM_NAME = model.INV_CONFIRM_NAME

            _Model.LOGIC_INV_NUM = model.LOGIC_INV_NUM
            _Model.PHYINV_NUM = model.PHYINV_NUM
            _Model.LOC_DIFF_NUM = model.LOC_DIFF_NUM
            _Model.NO_ITEM_NUM = model.NO_ITEM_NUM

            _Model.INV_ALL_CONFIRM_TS = model.INV_ALL_CONFIRM_TS

            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub

#End Region

    End Class
End Namespace