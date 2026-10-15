Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient
Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports GrapeCity.Documents.Excel
Imports GrapeCity.Documents.Excel.Drawing

Namespace Controllers
    Public Class TemporaryListController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"
        ''' <summary>
        ''' ページ間のチェックボックス対応
        ''' </summary>
        Private Enum TemporaryListKey
            TEMP_ID = 0
        End Enum

        Public Const DELIMITER As String = "$$$"
#End Region

#Region "クラス変数"
        Private _Model As TemporaryListModel = New TemporaryListModel                   'Temporaryモデル
        Private _ViewModel As TemporaryListViewModel = New TemporaryListViewModel       'Temporary一覧表示用モデル
        Protected _viewResult As List(Of TemporaryListViewModel) = New List(Of TemporaryListViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _dtCsvImport As DataTable = Nothing 'CSV取込用作業テーブル
        Private _lstCsvHed As List(Of String) = Nothing 'CSVヘッダ文字コード格納用リスト
        Private _sBeforeAssetID As String = String.Empty '新規登録時、0埋め・自動生成前の資産IDを格納する
        Private _iCsvImportRowCount As Integer = 0 'CSV取込件数

        Private _LocationLList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationMList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationSList As List(Of LocationDropdownModel)     'ロケーションマスタリスト

        Private _RegistInfo As New List(Of Dictionary(Of String, String))     '登録情報
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As TemporaryListViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(RedirectConstructionListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectConstructionListInfo.ID)
                'チェックボックス状態の選択状態をクリアする
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
            End If

            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
            End If

            'リダイレクト情報作成
            Dim info As New RedirectConstructionListInfo
            info.RedirectPage = RedirectConstructionListInfo.RedirectPageType.Menu
            info.KeyValue = String.Empty

            Session(RedirectConstructionListInfo.ID) = info

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
        Public Function Index(ByVal ViewModel As TemporaryListViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_TEMPORARY_LIST)
            End If

            Dim lstSelectKeys As New List(Of String)
            If Not IsNothing(Session(CommonSessionKey.LIST_CHECKED_KEYS)) Then
                lstSelectKeys = Session(CommonSessionKey.LIST_CHECKED_KEYS)
            End If

            'メッセージ一覧取得
            Dim Message As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '検索ボタンがクリックされた場合、選択状態のセッションをクリアする
            If Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                'セッションSEARCH_FLGを設定
                Session(CommonSessionKey.SEARCH_FLG) = True
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As TemporaryListViewModel = Session(CommonSessionKey.CREAGE_TEMPORARY_LIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            Dim info As RedirectConstructionListInfo = Session(RedirectConstructionListInfo.ID)
            If info IsNot Nothing Then
                If Session(CommonSessionKey.CREAGE_TEMPORARY_LIST_SEARCH_KEY) IsNot Nothing Then
                    Dim senimaeModel As TemporaryListViewModel = Session(CommonSessionKey.CREAGE_TEMPORARY_LIST_SEARCH_KEY)
                    _ViewModel = senimaeModel
                    Session.Remove(CommonSessionKey.CREAGE_TEMPORARY_LIST_SEARCH_KEY)
                End If

                If info.RedirectPage = RedirectConstructionListInfo.RedirectPageType.TemporaryList Then
                    '仮置資産一覧画面から戻ったときの処理
                    'リダイレクト情報作成(メニューから呼び出した時と同じ状態にする)
                    Dim info2 As New RedirectConstructionListInfo
                    info2.RedirectPage = RedirectConstructionListInfo.RedirectPageType.Menu
                    info2.KeyValue = String.Empty

                    Session(RedirectConstructionListInfo.ID) = info2
                Else
                    ViewBag.ConstructionListKey = info.KeyValue
                End If
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else

                '初期表示
                '項目配置情報取得
                Dim dictGridInfo = GetGridInfo()

                '検索条件リストデータ取得
                Dim inputItemList = GetInputItem()

                'セッションSEARCH_FLGがNothingの場合
                If Session(CommonSessionKey.SEARCH_FLG) Is Nothing Then
                    '検索条件の初期値設定
                    SetInitCondition(inputItemList, dictGridInfo)
                    If String.IsNullOrEmpty(_ViewModel.SearchRelConstStatus) Then
                        _ViewModel.SearchRelConstStatus = REL_CONST_STATUS.Unlinked
                    End If
                End If

                '表示する
                MyBase.BaseIndex(_ViewModel)

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            UpdateChecedInfo(_ViewModel.SelectKeys, _ViewModel.NoneSelectKeys)

            '入力設定リストデータ取得
            GetInputItem(True)

            '仮置き紐付状態ドロップダウンリストデータ取得
            GetRelConstStatusData()

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.TEMPORARY_LIST)
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
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()

            'クエリを返却
            MyBase._SQL = CreateSQL()

            '検索条件(全件)を指定したSQLとパラメーターをセッションに保持する
            Session(CommonSessionKey.LIST_VIEW_SQL) = MyBase._SQL
            Session(CommonSessionKey.LIST_VIEW_PARAM) = MyBase._paramSqlList

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LIST)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of TemporaryListViewModel) = _db.TemporaryListView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As TemporaryListViewModel, ByVal OnCreteButton As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim db As New SystemDBContext

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '入力設定リストデータ取得
            Dim inputItemList = GetInputItem()

            '仮置き情報1～7を設定する(単位が設定されている場合)
            Dim unit = ""
            unit = inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault.DATA4
            If Not String.IsNullOrEmpty(unit) Then
                _ViewModel.TEMP_INFO1 = If(IsNothing(_ViewModel.TEMP_INFO1), "", ViewModel.TEMP_INFO1 & DELIMITER & unit)
            End If
            unit = inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault.DATA4
            If Not String.IsNullOrEmpty(unit) Then
                _ViewModel.TEMP_INFO2 = If(IsNothing(_ViewModel.TEMP_INFO2), "", ViewModel.TEMP_INFO2 & DELIMITER & unit)
            End If
            unit = inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault.DATA4
            If Not String.IsNullOrEmpty(unit) Then
                _ViewModel.TEMP_INFO3 = If(IsNothing(_ViewModel.TEMP_INFO3), "", ViewModel.TEMP_INFO3 & DELIMITER & unit)
            End If
            unit = inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault.DATA4
            If Not String.IsNullOrEmpty(unit) Then
                _ViewModel.TEMP_INFO4 = If(IsNothing(_ViewModel.TEMP_INFO4), "", ViewModel.TEMP_INFO4 & DELIMITER & unit)
            End If
            unit = inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault.DATA4
            If Not String.IsNullOrEmpty(unit) Then
                _ViewModel.TEMP_INFO5 = If(IsNothing(_ViewModel.TEMP_INFO5), "", ViewModel.TEMP_INFO5 & DELIMITER & unit)
            End If
            unit = inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault.DATA4
            If Not String.IsNullOrEmpty(unit) Then
                _ViewModel.TEMP_INFO6 = If(IsNothing(_ViewModel.TEMP_INFO6), "", ViewModel.TEMP_INFO6 & DELIMITER & unit)
            End If
            unit = inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault.DATA4
            If Not String.IsNullOrEmpty(unit) Then
                _ViewModel.TEMP_INFO7 = If(IsNothing(_ViewModel.TEMP_INFO7), "", ViewModel.TEMP_INFO7 & DELIMITER & unit)
            End If

            'モデルの再検証
            ModelState.Clear()
            TryValidateModel(_ViewModel)

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                MyBase.BaseCreate(_ViewModel)
            Else
                '更新処理
                MyBase.BaseEdit(_ViewModel)
            End If

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                '登録処理
                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then

                    _ViewModel.States = StatusType.SuccessCreate

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelTemporaryList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_TEMP_ID) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.TEMP_ID).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelTemporaryList.TITLE))

                    '更新処理
                Else
                    _ViewModel.States = StatusType.SuccessEdit

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelTemporaryList.TITLE) & " " &
                                                 Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                 Me._Message.getMessage(CommonLabel.LABEL_TEMP_ID) & ": " &
                                                 CommonRfid.ToStringNvl(_ViewModel.TEMP_ID).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelTemporaryList.TITLE))
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
            Session(CommonSessionKey.CREAGE_TEMPORARY_LIST) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_TEMPORARY_LIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            '仮置が登録済みかチェックする
            Dim model As TemporaryListModel = db.TemporaryList.Find(_Model.TEMP_ID)

            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(model) Then
                ModelState.AddModelError("TEMP_ID", CommonLabel.MSG_ERR_DATAJUUFUKU)
                blnReturn = False
            End If

            If Not ModelState.IsValid Then
                blnReturn = False  'エラーがあった場合、Falseを返す
            End If

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelTemporaryList.TITLE)

            '処理日時を登録
            _Model.TASK_TS = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified
        End Sub

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ID">ID</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal ID As Integer) As JsonResult
            Dim db As New SystemDBContext

            'ログイン情報を取得。
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'IDをキーにモデルを取得する
            Dim model As TemporaryListModel = db.TemporaryList.Find(ID)
            model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

            '仮置き情報1～5の単位以前の値を設定
            If Not String.IsNullOrEmpty(model.TEMP_INFO1) Then
                Dim index As Integer = model.TEMP_INFO1.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    ' セパレータ以前の部分を取得
                    model.TEMP_INFO1 = model.TEMP_INFO1.Substring(0, index)
                End If
            End If
            If Not String.IsNullOrEmpty(model.TEMP_INFO2) Then
                Dim index As Integer = model.TEMP_INFO2.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.TEMP_INFO2 = model.TEMP_INFO2.Substring(0, index)
                End If
            End If
            If Not String.IsNullOrEmpty(model.TEMP_INFO3) Then
                Dim index As Integer = model.TEMP_INFO3.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.TEMP_INFO3 = model.TEMP_INFO3.Substring(0, index)
                End If
            End If
            If Not String.IsNullOrEmpty(model.TEMP_INFO4) Then
                Dim index As Integer = model.TEMP_INFO4.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.TEMP_INFO4 = model.TEMP_INFO4.Substring(0, index)
                End If
            End If
            If Not String.IsNullOrEmpty(model.TEMP_INFO5) Then
                Dim index As Integer = model.TEMP_INFO5.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.TEMP_INFO5 = model.TEMP_INFO5.Substring(0, index)
                End If
            End If
            If Not String.IsNullOrEmpty(model.TEMP_INFO6) Then
                Dim index As Integer = model.TEMP_INFO6.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.TEMP_INFO6 = model.TEMP_INFO6.Substring(0, index)
                End If
            End If
            If Not String.IsNullOrEmpty(model.TEMP_INFO7) Then
                Dim index As Integer = model.TEMP_INFO7.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.TEMP_INFO7 = model.TEMP_INFO7.Substring(0, index)
                End If
            End If

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
        <ValidateInput(False)>
        Public Overridable Function Delete(ByVal ViewModel As TemporaryListViewModel) As ActionResult
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

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                _ViewModel.States = StatusType.SuccessDelete

                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '操作ログ出力
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelTemporaryList.TITLE) & " " &
                                                 Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                 Me._Message.getMessage(CommonLabel.LABEL_TEMP_ID) & ": " &
                                                 CommonRfid.ToStringNvl(_ViewModel.TEMP_ID).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelTemporaryList.TITLE))
            End If

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
            Session(CommonSessionKey.CREAGE_TEMPORARY_LIST) = _ViewModel

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
            Dim temporaryList As TemporaryListModel = _db.TemporaryList.Find(_Model.TEMP_ID)

            '削除を実行する
            _db.TemporaryList.Remove(temporaryList)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
        End Sub
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function IchiranCsvExport(ViewModel As TemporaryListViewModel) As ActionResult
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

            Dim TemporaryData As List(Of TemporaryListViewModel)

            '仮置きテーブルデータ取得
            TemporaryData = GetTemporaryListData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.TemporaryList.KeyDiv, CommonSettingKey.TemporaryList.Key1, CommonSettingKey.TemporaryList.Key2, CommonSettingKey.TemporaryList.Key3, True)
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
            'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_TEMPORARY_LIST)
            sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_TEMPORARY_LIST)


            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As TemporaryListViewModel In TemporaryData
                col = ""

                If String.IsNullOrEmpty(row.REL_CONST_NO) Then
                    col += SVClsCSVExp.SVFncCSVAddDquot(REL_CONST_STATUS_CSV.Unlinked)
                Else
                    col += SVClsCSVExp.SVFncCSVAddDquot(REL_CONST_STATUS_CSV.Linked)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REL_CONST_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REG_CONST_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.TEMP_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_CONTENTS)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_REMARK)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_NUM)

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelTemporaryList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     TemporaryData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelTemporaryList.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetTemporaryListData() As List(Of TemporaryListViewModel)
            Dim lstTemporaryList As List(Of TemporaryListViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstTemporaryList = db.TemporaryListView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList

            'データを返却
            Return lstTemporaryList
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

#Region "紐付き工事更新処理"
        ''' <summary>
        ''' 紐付き工事更新処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function UpdRelConstNo(ByVal ViewModel As TemporaryListViewModel, ByVal SelectKeys As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '遷移前の工事番号を取得
            Dim info As RedirectConstructionListInfo = Session(RedirectConstructionListInfo.ID)
            Dim constNo = info.KeyValue

            '最新のチェック状態でセッション情報を更新する(追加および削除) (キー：仮置きID)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)
            Dim sKeyStr As String = "'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split("/")(TemporaryListKey.TEMP_ID)).ToList) & "'"

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder

            Dim params As List(Of SqlParameter)

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try
                    '仮置き更新
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("UPDATE T_TEMPORARY SET")
                    sbSQL.AppendLine("    REL_CONST_NO = @REL_CONST_NO")
                    sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                    sbSQL.AppendLine("WHERE TEMP_ID IN (" & sKeyStr & ")")

                    params.Add(New SqlParameter($"@REL_CONST_NO", constNo))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '工事資産登録
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("INSERT INTO T_CONST_ASSET")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("     CONST_NO")
                    sbSQL.AppendLine("    ,ASSET_NO")
                    sbSQL.AppendLine("    ,CONST_ASSET_STATUS")
                    sbSQL.AppendLine("    ,TEMP_ID")
                    sbSQL.AppendLine("    ,PREV_TEMP_FLAG")
                    sbSQL.AppendLine("    ,TASK_TS")
                    sbSQL.AppendLine("    ,INS_DATETIME")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_DATETIME")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK")
                    sbSQL.AppendLine(")")
                    sbSQL.AppendLine("SELECT")
                    sbSQL.AppendLine("     @CONST_NO")
                    sbSQL.AppendLine("    ,TASSET.ASSET_NO")
                    sbSQL.AppendLine("    ,'" + CONST_ASSET_STATUS.Lending + "'")
                    sbSQL.AppendLine("    ,NULL")
                    sbSQL.AppendLine("    ,'" + PrevTempFlg.Cd.Yes + "'")
                    sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                    sbSQL.AppendLine("    ,@INS_DATETIME")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_DATETIME")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_TASK")
                    sbSQL.AppendLine("FROM T_TEMP_ASSET TASSET")

                    sbSQL.AppendLine("WHERE  TASSET.TEMP_ID IN (" & sKeyStr & ")")

                    params.Add(New SqlParameter($"@CONST_NO", constNo))
                    params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '工事マスタ更新
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("UPDATE M_CONSTRUCTION SET")
                    'sbSQL.AppendLine("     PREV_TEMP_NUM = (SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND PREV_TEMP_FLAG = '" + PrevTempFlg.Cd.Yes + "')")
                    'sbSQL.AppendLine("    ,CONST_IN_NUM = (SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO)")
                    'sbSQL.AppendLine("    ,CONST_OUT_NUM = (SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND TEMP_ID IS NULL)")
                    'sbSQL.AppendLine("    ,CUR_TEMP_NUM = (SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND CONST_ASSET_STATUS = '" + CONST_ASSET_STATUS.Lending + "')")
                    'sbSQL.AppendLine($"   PREV_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND PREV_TEMP_FLAG = 'Y')")        '前回仮置き
                    'sbSQL.AppendLine($"  ,CONST_IN_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND PREV_TEMP_FLAG = 'N' AND TEMP_ID IS NULL )")      '持込数
                    'sbSQL.AppendLine($"  ,NOW_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND PREV_TEMP_FLAG = 'N' AND TEMP_ID IS NOT NULL )")   '今回仮置き数
                    'sbSQL.AppendLine($"  ,CONST_OUT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1')")                                                   '持帰数
                    'sbSQL.AppendLine($"  ,CUR_LENT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND TEMP_ID IS NULL )")                               '現在貸出数
                    'sbSQL.AppendLine($"  ,CUR_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                             '現在仮置数

                    sbSQL.AppendLine($"   PREV_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND PREV_TEMP_FLAG = 'Y')")                                                        '前回仮置き
                    sbSQL.AppendLine($"  ,CONST_IN_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1')")                                                    '持込数
                    sbSQL.AppendLine($"  ,NOW_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND PREV_TEMP_FLAG = 'N' AND TEMP_ID IS NOT NULL )")   '今回仮置き数
                    sbSQL.AppendLine($"  ,CONST_OUT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NULL AND CONST_ASSET_STATUS = '1')")                               '持帰数
                    sbSQL.AppendLine($"  ,CUR_LENT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND TEMP_ID IS NULL )")                               '現在貸出数
                    sbSQL.AppendLine($"  ,CUR_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                             '現在仮置数

                    sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                    sbSQL.AppendLine(" FROM M_CONSTRUCTION CON ")
                    sbSQL.AppendLine(" WHERE CONST_NO = @CONST_NO")

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))
                    params.Add(New SqlParameter($"@CONST_NO", constNo))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '工事履歴登録
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("INSERT INTO T_CONST_LOG")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("     CONST_NO")
                    sbSQL.AppendLine("    ,CONST_WORK_TS")
                    sbSQL.AppendLine("    ,CONST_WORK_DIV")
                    sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                    sbSQL.AppendLine("    ,LENT_NUM")
                    sbSQL.AppendLine("    ,LEASE_NUM")
                    sbSQL.AppendLine("    ,TEMP_REG_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_MATCH_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_MATCH_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_NUM")
                    sbSQL.AppendLine("    ,TEMP_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_NUM")
                    sbSQL.AppendLine("    ,INS_DATETIME")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_DATETIME")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK")
                    sbSQL.AppendLine(") VALUES (")
                    sbSQL.AppendLine("     @CONST_NO")
                    sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                    sbSQL.AppendLine("    ,'" + ConstWorkDiv.TempLink + "'")
                    sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                    sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_TEMP_ASSET WHERE TEMP_ID IN (" & sKeyStr & "))")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND CONST_ASSET_STATUS = " + "'" + CONST_ASSET_STATUS.Lending + "')")
                    sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = " + "'" + CONST_ASSET_STATUS.Lending + "')")
                    sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND TEMP_ID IS NULL AND CONST_ASSET_STATUS = " + "'" + CONST_ASSET_STATUS.Lending + "')")
                    sbSQL.AppendLine("    ,@INS_DATETIME")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_DATETIME")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_TASK")
                    sbSQL.AppendLine(")")

                    params.Add(New SqlParameter($"@CONST_NO", constNo))
                    params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '工事資産履歴登録
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("     CONST_NO")
                    sbSQL.AppendLine("    ,CONST_WORK_TS")
                    sbSQL.AppendLine("    ,ASSET_NO")
                    sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                    sbSQL.AppendLine("    ,CONST_MATCH_STATUS")
                    sbSQL.AppendLine("    ,TEMP_FLAG")
                    sbSQL.AppendLine("    ,TEMP_ID")
                    sbSQL.AppendLine("    ,INS_DATETIME")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_DATETIME")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK")
                    sbSQL.AppendLine(")")
                    sbSQL.AppendLine("SELECT")
                    sbSQL.AppendLine("     @CONST_NO")
                    sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                    sbSQL.AppendLine("    ,TASSET.ASSET_NO")
                    sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                    sbSQL.AppendLine("    ,'" + CONST_MATCH_STATUS.Lending + "'")
                    sbSQL.AppendLine("    ,'" + TempFlg.Cd.Yes + "'")
                    sbSQL.AppendLine("    ,TASSET.TEMP_ID")
                    sbSQL.AppendLine("    ,@INS_DATETIME")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_DATETIME")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_TASK")
                    sbSQL.AppendLine("FROM T_TEMP_ASSET TASSET")

                    sbSQL.AppendLine("WHERE  TASSET.TEMP_ID IN (" & sKeyStr & ")")

                    params.Add(New SqlParameter($"@CONST_NO", constNo))
                    params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '紐付け元
                    '選択した件数ループ
                    'コンマ区切りで分割する
                    Dim tempIDs As String()
                    tempIDs = sKeyStr.Split(","c)
                    'tempIDごとに処理
                    For Each tempID As String In tempIDs
                        '工事資産更新
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("UPDATE T_CONST_ASSET SET")
                        sbSQL.AppendLine("     CONST_ASSET_STATUS = '3' ")
                        sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                        sbSQL.AppendLine(" WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ")")
                        sbSQL.AppendLine(" AND ASSET_NO IN (SELECT ASSET_NO FROM T_TEMP_ASSET WHERE TEMP_ID = " + tempID + ")")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))
                        params.Add(New SqlParameter($"@CONST_NO", constNo))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("UPDATE M_CONSTRUCTION SET")
                        'sbSQL.AppendLine("     PREV_TEMP_NUM = CONST_IN_NUM - (SELECT COUNT(*) FROM T_TEMP_ASSET WHERE TEMP_ID = " + tempID + ")")
                        'sbSQL.AppendLine("    ,CONST_IN_NUM = CONST_IN_NUM - (SELECT COUNT(*) FROM T_TEMP_ASSET WHERE TEMP_ID = " + tempID + ")")
                        'sbSQL.AppendLine($"   PREV_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND PREV_TEMP_FLAG = 'Y')")        '前回仮置き
                        'sbSQL.AppendLine($"  ,CONST_IN_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND PREV_TEMP_FLAG = 'N' AND TEMP_ID IS NULL )")      '持込数
                        'sbSQL.AppendLine($"  ,NOW_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND PREV_TEMP_FLAG = 'N' AND TEMP_ID IS NOT NULL )")   '今回仮置き数
                        'sbSQL.AppendLine($"  ,CONST_OUT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1')")                                                   '持帰数
                        'sbSQL.AppendLine($"  ,CUR_LENT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND TEMP_ID IS NULL )")                               '現在貸出数
                        'sbSQL.AppendLine($"  ,CUR_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = AST.CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                             '現在仮置数

                        sbSQL.AppendLine($"   PREV_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND PREV_TEMP_FLAG = 'Y')")                                                        '前回仮置き
                        sbSQL.AppendLine($"  ,CONST_IN_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1')")                                                    '持込数
                        sbSQL.AppendLine($"  ,NOW_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND PREV_TEMP_FLAG = 'N' AND TEMP_ID IS NOT NULL )")   '今回仮置き数
                        sbSQL.AppendLine($"  ,CONST_OUT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NULL AND CONST_ASSET_STATUS = '1')")                               '持帰数
                        sbSQL.AppendLine($"  ,CUR_LENT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND TEMP_ID IS NULL )")                               '現在貸出数
                        sbSQL.AppendLine($"  ,CUR_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                             '現在仮置数

                        sbSQL.AppendLine("    ,TASK_TS = CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                        sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                        sbSQL.AppendLine(" FROM M_CONSTRUCTION CON ")
                        sbSQL.AppendLine(" WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ")")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))
                        params.Add(New SqlParameter($"@CONST_NO", constNo))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                        '工事履歴登録
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("INSERT INTO T_CONST_LOG")
                        sbSQL.AppendLine("(")
                        sbSQL.AppendLine("     CONST_NO")
                        sbSQL.AppendLine("    ,CONST_WORK_TS")
                        sbSQL.AppendLine("    ,CONST_WORK_DIV")
                        sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                        sbSQL.AppendLine("    ,LENT_NUM")
                        sbSQL.AppendLine("    ,LEASE_NUM")
                        sbSQL.AppendLine("    ,TEMP_REG_NUM")
                        sbSQL.AppendLine("    ,CONST_IN_MATCH_NUM")
                        sbSQL.AppendLine("    ,CONST_OUT_MATCH_NUM")
                        sbSQL.AppendLine("    ,CONST_IN_NUM")
                        sbSQL.AppendLine("    ,TEMP_NUM")
                        sbSQL.AppendLine("    ,CONST_OUT_NUM")
                        sbSQL.AppendLine("    ,INS_DATETIME")
                        sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_DATETIME")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_TASK")
                        sbSQL.AppendLine(") VALUES (")
                        sbSQL.AppendLine("     (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ")")
                        sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                        sbSQL.AppendLine("    ,'" + ConstWorkDiv.AnotherConstLink + "'")
                        sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                        sbSQL.AppendLine("    ,(SELECT LENT_NUM FROM T_CONST_LOG WHERE CONST_NO = ( SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + " ) AND CONST_WORK_TS = ( SELECT MAX(CONST_WORK_TS) FROM T_CONST_LOG WHERE CONST_NO = ( SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + " ) )) - (SELECT COUNT(*) FROM T_TEMP_ASSET WHERE TEMP_ID = " + tempID + ")")
                        sbSQL.AppendLine("    ,0")
                        sbSQL.AppendLine("    ,0")
                        sbSQL.AppendLine("    ,0")
                        sbSQL.AppendLine("    ,0")
                        sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ") AND CONST_ASSET_STATUS = " + "'" + CONST_ASSET_STATUS.Lending + "')")
                        sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ") AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = " + "'" + CONST_ASSET_STATUS.Lending + "')")
                        sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ") AND TEMP_ID IS NULL AND CONST_ASSET_STATUS = " + "'" + CONST_ASSET_STATUS.Lending + "')")
                        sbSQL.AppendLine("    ,@INS_DATETIME")
                        sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,@UPD_DATETIME")
                        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,@UPD_TASK")
                        sbSQL.AppendLine(")")

                        params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                        params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                        '工事資産履歴登録
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
                        sbSQL.AppendLine("(")
                        sbSQL.AppendLine("     CONST_NO")
                        sbSQL.AppendLine("    ,CONST_WORK_TS")
                        sbSQL.AppendLine("    ,ASSET_NO")
                        sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                        sbSQL.AppendLine("    ,CONST_MATCH_STATUS")
                        sbSQL.AppendLine("    ,TEMP_FLAG")
                        sbSQL.AppendLine("    ,TEMP_ID")
                        sbSQL.AppendLine("    ,INS_DATETIME")
                        sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_DATETIME")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_TASK")
                        sbSQL.AppendLine(")")
                        sbSQL.AppendLine("SELECT")
                        sbSQL.AppendLine("     (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ")")
                        sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                        sbSQL.AppendLine("    ,TASSET.ASSET_NO")
                        sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                        sbSQL.AppendLine("    ,'" + CONST_MATCH_STATUS.Lending + "'")
                        sbSQL.AppendLine("    ,'" + TempFlg.Cd.Yes + "'")
                        sbSQL.AppendLine("    ,TASSET.TEMP_ID")
                        sbSQL.AppendLine("    ,@INS_DATETIME")
                        sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,@UPD_DATETIME")
                        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,@UPD_TASK")
                        sbSQL.AppendLine("FROM T_TEMP_ASSET TASSET")

                        sbSQL.AppendLine("WHERE  TASSET.TEMP_ID = " + tempID + "")

                        params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                        params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
                    Next


                    tran.Commit()

                    '正常時、正常終了のステータスを返す
                    If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                        '操作ログ出力
                        Dim sProcessContents As String = Me._Message.getMessage(CommonLabelTemporaryList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabelTemporaryList.MSG_SUCCESS_ADD_REL_COMPLETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CONST_NO) & ": " &
                                                     constNo
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelTemporaryList.TITLE))

                        _ViewModel.States = StatusType.SuccessAddTemp

                        '一覧の選択状態をクリアする。
                        _ViewModel.SelectKeys = ""
                        '一覧の選択状態をクリアする。
                        Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                        '全選択ボタン押下フラグをクリアする
                        Session.Remove(CommonSessionKey.ALL_SELECT_FLG)

                    End If

                    DisposeCreate()

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)
                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "仮置き紐付状態ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 仮置き紐付状態ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetRelConstStatusData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = REL_CONST_STATUS.Unlinked,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_REL_CONSTRUCTION_STATUS_UNLINKED).ToString
                },
                New DropdownModel With {
                    .Cd = REL_CONST_STATUS.Linked,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_REL_CONSTRUCTION_STATUS_LINKED).ToString
                }}

            '一覧画面の仮置き紐付状態コンボ設定
            ViewBag.RelConstStatusList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchRelConstStatus)

            Return Json(lst)
        End Function
#End Region

#Region "入力設定情報取得"
        ''' <summary>
        ''' 入力設定情報取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetInputItem(Optional defaultSet As Boolean = True) As List(Of SearchInputItemModel)
            Dim tbl As DataTable = Dao.ToolSetting.GetTemporaryInputItemInfo()
            Dim lst As New List(Of SearchInputItemModel)

            For Each row As DataRow In tbl.Rows
                'ドロップダウンデータ取得
                Dim dropTbl As DataTable = Dao.ToolSetting.GetItemDropDownInfo(row("DATA5").ToString)
                Dim dropLst As New List(Of DropdownModel)
                For Each rowDrop As DataRow In dropTbl.Rows
                    dropLst.Add(
                    New DropdownModel With {
                        .Cd = rowDrop("DATA2").ToString,
                        .Name = rowDrop("DATA1").ToString
                    }
                )
                Next

                '共通関数で利用するためDATA1にDATA2の値を設定(DATA1はWebでは利用しない)
                lst.Add(
                    New SearchInputItemModel With {
                        .KEY1 = row("KEY1").ToString,
                        .DATA1 = row("DATA2").ToString,
                        .DATA2 = row("DATA2").ToString,
                        .DATA3 = row("DATA3").ToString,
                        .DATA4 = row("DATA4").ToString,
                        .DATA5 = row("DATA5").ToString,
                        .DropdownModel = dropLst,
                        .DropdownList = New SelectList(dropLst, "Cd", "Name", If(defaultSet = True, row("DATA3").ToString, ""))
                    }
                )
            Next
            ViewBag.inputItemList = lst
            Return lst
        End Function
#End Region

#Region "仮置きID・元画面の検索条件をセッションに保存し、仮置資産一覧画面に遷移します"
        ''' <summary>
        ''' 仮置きID・元画面の検索条件をセッションに保存し、仮置資産一覧画面に遷移します
        ''' </summary>
        <ValidateInput(False)>
        Public Function SetTempId(ByVal ViewModel As TemporaryListViewModel, ByVal SelectedTempId As String) As ActionResult
            Session.Remove(CommonSessionKey.CREAGE_TEMP_ASSET_LIST_KEY)
            Session(CommonSessionKey.CREAGE_TEMP_ASSET_LIST_KEY) = SelectedTempId

            '検索条件をセッションに保存
            Session(CommonSessionKey.CREAGE_TEMPORARY_LIST_SEARCH_KEY) = ViewModel

            'リダイレクト情報作成
            Dim info As RedirectConstructionListInfo = Session(RedirectConstructionListInfo.ID)
            If info Is Nothing OrElse info.RedirectPage <> RedirectConstructionListInfo.RedirectPageType.ConstructionList Then
                '遷移情報セッションクリア
                If Session(RedirectConstructionListInfo.ID) IsNot Nothing Then
                    Session.Remove(RedirectConstructionListInfo.ID)
                End If
                Dim info2 As New RedirectConstructionListInfo
                info2.RedirectPage = RedirectConstructionListInfo.RedirectPageType.TemporaryList
                info2.KeyValue = String.Empty

                Session(RedirectConstructionListInfo.ID) = info2
            End If

            '結果表示
            Return RedirectToAction("Index", "TempAssetList")
        End Function
#End Region

#Region "最新のチェック状態でセッション情報を更新する"
        ''' <summary>
        ''' 最新のチェック状態でセッション情報を更新する
        ''' 重複を排除したチェックされたキーを返却する
        ''' </summary>
        ''' <param name="SelectKeys">選択キー</param>
        ''' <param name="NoneSelectKeys">未選択キー</param>
        ''' <returns>チェックされた行のキーリスト</returns>
        Private Function UpdateChecedInfo(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As List(Of String)
            '一覧にない資産は未選択キーとする
            If Not IsNothing(_ViewModel.TEMP_ID) Then
                Dim results = From data In _viewResult Where data.TEMP_ID = _ViewModel.TEMP_ID
                If results.Count <= 0 Then
                    NoneSelectKeys = String.Join("/", New String() {NoneSelectKeys, $"{_ViewModel.TEMP_ID}"})
                End If
            End If

            'セッションの選択キー情報を取得する
            Dim lstSelectKeys As New List(Of String)
            If Not IsNothing(Session(CommonSessionKey.LIST_CHECKED_KEYS)) Then
                lstSelectKeys = Session(CommonSessionKey.LIST_CHECKED_KEYS)
            End If

            '選択キーを追加する
            If Not IsNothing(SelectKeys) AndAlso SelectKeys <> String.Empty Then
                lstSelectKeys.AddRange(SelectKeys.Split("/").ToList)
            End If

            '重複を排除する
            lstSelectKeys = lstSelectKeys.Distinct.ToList

            '未選択キーを削除する
            If Not IsNothing(NoneSelectKeys) AndAlso NoneSelectKeys <> String.Empty Then
                For Each deleteKey In NoneSelectKeys.Split("/").ToList
                    lstSelectKeys.Remove(deleteKey)
                Next
            End If

            'セッションを更新する
            Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstSelectKeys

            'チェックされた行のキーリストを返却する
            Return lstSelectKeys
        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String
            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.TemporaryList, _LoginInfo)
            If AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                '自拠点の更新・照会権限有り
                If AuthInfo.UpdHubAuthFlg = AUTH_FLG.Yes Then
                    '他拠点の更新・照会権限有り
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    '他拠点の更新・照会権限無し
                    ViewBag.AuthProcFlg = AuthProcFlg.JiAll
                End If
            Else
                '照会のみ
                ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
            End If

#Region "検索条件SQLを作成"
            Dim sbWhr As New StringBuilder

            '仮置き紐付状態 SearchRelConstStatus
            If (Not IsNothing(_ViewModel.SearchRelConstStatus)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchRelConstStatus.Trim)) Then
                Select Case _ViewModel.SearchRelConstStatus
                    Case REL_CONST_STATUS.Unlinked
                        sbWhr.AppendLine("  AND TEMP.REL_CONST_NO IS NULL ")
                    Case REL_CONST_STATUS.Linked
                        sbWhr.AppendLine("  AND TEMP.REL_CONST_NO IS NOT NULL ")
                End Select
            End If

            '紐付工事番号 SearchRelConstNo
            If (Not IsNothing(_ViewModel.SearchRelConstNo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchRelConstNo.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.REL_CONST_NO like " & "@SearchRelConstNo")
                addSqlParam("SearchRelConstNo", $"{"%" & _ViewModel.SearchRelConstNo.Trim & "%"}")
            End If

            '登録工事番号 SearchRegConstNo
            If (Not IsNothing(_ViewModel.SearchRegConstNo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchRegConstNo.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.REG_CONST_NO like " & "@SearchRegConstNo")
                addSqlParam("SearchRegConstNo", $"{"%" & _ViewModel.SearchRegConstNo.Trim & "%"}")
            End If

            '仮置き日 SearchTempDt
            If (Not IsNothing(_ViewModel.SearchTempDt)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempDt.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_DT = " & "@SearchTempDt")
                addSqlParam("SearchTempDt", $"{_ViewModel.SearchTempDt.Replace("-", "")}")
            End If

            '工事内容 SearchConstContents
            If (Not IsNothing(_ViewModel.SearchConstContents)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstContents.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_CONTENTS like " & "@SearchConstContents")
                addSqlParam("SearchConstContents", $"{"%" & _ViewModel.SearchConstContents.Trim & "%"}")
            End If

            '仮置き情報1 SearchTempInfo1
            If (Not IsNothing(_ViewModel.SearchTempInfo1)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempInfo1.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_INFO1 like " & "@SearchTempInfo1")
                addSqlParam("SearchTempInfo1", $"{"%" & _ViewModel.SearchTempInfo1.Trim & "%"}")
            End If

            '仮置き情報2 SearchTempInfo2
            If (Not IsNothing(_ViewModel.SearchTempInfo2)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempInfo2.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_INFO2 like " & "@SearchTempInfo2")
                addSqlParam("SearchTempInfo2", $"{"%" & _ViewModel.SearchTempInfo2.Trim & "%"}")
            End If

            '仮置き情報3 SearchTempInfo3
            If (Not IsNothing(_ViewModel.SearchTempInfo3)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempInfo3.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_INFO3 like " & "@SearchTempInfo3")
                addSqlParam("SearchTempInfo3", $"{"%" & _ViewModel.SearchTempInfo3.Trim & "%"}")
            End If

            '仮置き情報4 SearchTempInfo4
            If (Not IsNothing(_ViewModel.SearchTempInfo4)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempInfo4.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_INFO4 like " & "@SearchTempInfo4")
                addSqlParam("SearchTempInfo4", $"{"%" & _ViewModel.SearchTempInfo4.Trim & "%"}")
            End If

            '仮置き情報5 SearchTempInfo5
            If (Not IsNothing(_ViewModel.SearchTempInfo5)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempInfo5.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_INFO5 like " & "@SearchTempInfo5")
                addSqlParam("SearchTempInfo5", $"{"%" & _ViewModel.SearchTempInfo5.Trim & "%"}")
            End If

            '仮置き情報6 SearchTempInfo6
            If (Not IsNothing(_ViewModel.SearchTempInfo6)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempInfo6.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_INFO6 like " & "@SearchTempInfo6")
                addSqlParam("SearchTempInfo6", $"{"%" & _ViewModel.SearchTempInfo6.Trim & "%"}")
            End If

            '仮置き情報5 SearchTempInfo7
            If (Not IsNothing(_ViewModel.SearchTempInfo7)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempInfo7.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_INFO7 like " & "@SearchTempInfo7")
                addSqlParam("SearchTempInfo7", $"{"%" & _ViewModel.SearchTempInfo7.Trim & "%"}")
            End If

            '仮置き備考 SearchTempRemark
            If (Not IsNothing(_ViewModel.SearchTempRemark)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempRemark.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_REMARK like " & "@SearchTempRemark")
                addSqlParam("SearchTempRemark", $"{"%" & _ViewModel.SearchTempRemark.Trim & "%"}")
            End If

#End Region

            Dim sbSQL As New StringBuilder

            '一覧表示表用SQL
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

            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{AuthInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{AuthInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{AuthInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{AuthInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType", $"{Misc.UpdAuthType.UPD_AUTH}")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            sbSQL.AppendLine(sbWhr.ToString)

            'ORDER句の設定
            sbSQL.AppendLine(" ORDER BY TEMP.TEMP_ID")

            'クエリを返却
            Return sbSQL.ToString
        End Function

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New TemporaryListViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchRelConstStatus = _ViewModel.SearchRelConstStatus
            _viewResult.ElementAt(0).SearchRelConstNo = _ViewModel.SearchRelConstNo
            _viewResult.ElementAt(0).SearchRegConstNo = _ViewModel.SearchRegConstNo
            _viewResult.ElementAt(0).SearchTempDt = _ViewModel.SearchTempDt
            _viewResult.ElementAt(0).SearchConstContents = _ViewModel.SearchConstContents
            _viewResult.ElementAt(0).SearchTempInfo1 = _ViewModel.SearchTempInfo1
            _viewResult.ElementAt(0).SearchTempInfo2 = _ViewModel.SearchTempInfo2
            _viewResult.ElementAt(0).SearchTempInfo3 = _ViewModel.SearchTempInfo3
            _viewResult.ElementAt(0).SearchTempInfo4 = _ViewModel.SearchTempInfo4
            _viewResult.ElementAt(0).SearchTempInfo5 = _ViewModel.SearchTempInfo5
            _viewResult.ElementAt(0).SearchTempInfo6 = _ViewModel.SearchTempInfo6
            _viewResult.ElementAt(0).SearchTempInfo7 = _ViewModel.SearchTempInfo7
            _viewResult.ElementAt(0).SearchTempRemark = _ViewModel.SearchTempRemark
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.TEMPORARY_LIST).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfo = GetGridInfo()
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As TemporaryListViewModel)
            _Model.TEMP_ID = model.TEMP_ID
            _Model.REG_CONST_NO = model.REG_CONST_NO
            _Model.REL_CONST_NO = model.REL_CONST_NO
            _Model.TEMP_DT = model.TEMP_DT
            _Model.WORK_EMPLOYEE_CD = model.WORK_EMPLOYEE_CD
            _Model.WORK_EMPLOYEE_NAME = model.WORK_EMPLOYEE_NAME
            _Model.TEMP_INFO1 = model.TEMP_INFO1
            _Model.TEMP_INFO2 = model.TEMP_INFO2
            _Model.TEMP_INFO3 = model.TEMP_INFO3
            _Model.TEMP_INFO4 = model.TEMP_INFO4
            _Model.TEMP_INFO5 = model.TEMP_INFO5
            _Model.TEMP_INFO6 = model.TEMP_INFO6
            _Model.TEMP_INFO7 = model.TEMP_INFO7
            _Model.TEMP_REMARK = model.TEMP_REMARK
            _Model.TASK_TS = model.TASK_TS
            _Model.INS_DATETIME = model.INS_DATETIME
            _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub
#End Region

#Region "項目配置情報を取得"
        Private Function GetGridInfo() As Dictionary(Of String, SlickGridInfo)
            '項目配置詳細マスタを取得
            Dim sbSQL As New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("  DISP_ID")
            sbSQL.AppendLine(", COLUMN_NAME")
            sbSQL.AppendLine(", DISP_ORDER")
            sbSQL.AppendLine(", TOOL_DIV")
            sbSQL.AppendLine(", WORD_DIV")
            sbSQL.AppendLine(", WORD_CD")
            sbSQL.AppendLine(", DISP_FLAG")
            sbSQL.AppendLine(", INS_DATETIME")
            sbSQL.AppendLine(", INS_EMPLOYEE_CD")
            sbSQL.AppendLine(", INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(", UPD_DATETIME")
            sbSQL.AppendLine(", UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(", UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(", UPD_TASK")
            sbSQL.AppendLine("FROM M_COL_SET_DETAIL")
            sbSQL.AppendLine("WHERE DISP_ID = @DISP_ID")

            params.Add(New SqlParameter($"@DISP_ID", DispId.TEMPORARY_LIST))

            Dim colSetDetail As List(Of ColSetDetailModel) = _db.ColSetDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            'Viewに渡す用に変換
            Dim dictGridInfo As New Dictionary(Of String, SlickGridInfo)
            For Each row In colSetDetail
                If Not dictGridInfo.ContainsKey(row.COLUMN_NAME) Then
                    'Key:DB項目名 Value:Grid表示情報
                    dictGridInfo.Add(row.COLUMN_NAME, New SlickGridInfo With {.DispOrder = row.DISP_ORDER, .DispFlag = row.DISP_FLAG})
                End If
            Next

            Return dictGridInfo
        End Function
#End Region

#Region "検索条件の初期値設定"
        Private Sub SetInitCondition(ByVal searchItemList As List(Of SearchInputItemModel), ByVal dictGridInfo As Dictionary(Of String, SlickGridInfo))

            'Dim searchItem = New SearchInputItemModel

            ''viewModelに初期値を設定(仮置き情報1～5)
            ''設定マスタが存在、項目配置明細マスタ.表示フラグ:表示、初期表示の場合
            'searchItem = searchItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault
            'If searchItem IsNot Nothing AndAlso dictGridInfo("TEMP_INFO1").DispFlag = "1" AndAlso _ViewModel.SearchTempInfo1 IsNot Nothing AndAlso _ViewModel.SearchTempInfo1 = "" Then
            '    '入力属性:リストでドロップダウン項目に含まれない場合は初期値を設定しない(画面は選択なしのため)
            '    If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA3)) Then
            '        _ViewModel.SearchTempInfo1 = searchItem.DATA3
            '    End If
            'End If
            'searchItem = searchItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault
            'If searchItem IsNot Nothing AndAlso dictGridInfo("TEMP_INFO2").DispFlag = "1" AndAlso _ViewModel.SearchTempInfo2 IsNot Nothing AndAlso _ViewModel.SearchTempInfo2 = "" Then
            '    If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA3)) Then
            '        _ViewModel.SearchTempInfo2 = searchItem.DATA3
            '    End If
            'End If
            'searchItem = searchItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault
            'If searchItem IsNot Nothing AndAlso dictGridInfo("TEMP_INFO3").DispFlag = "1" AndAlso _ViewModel.SearchTempInfo3 IsNot Nothing AndAlso _ViewModel.SearchTempInfo3 = "" Then
            '    If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA3)) Then
            '        _ViewModel.SearchTempInfo3 = searchItem.DATA3
            '    End If
            'End If
            'searchItem = searchItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault
            'If searchItem IsNot Nothing AndAlso dictGridInfo("TEMP_INFO4").DispFlag = "1" AndAlso _ViewModel.SearchTempInfo4 IsNot Nothing AndAlso _ViewModel.SearchTempInfo4 = "" Then
            '    If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA3)) Then
            '        _ViewModel.SearchTempInfo4 = searchItem.DATA3
            '    End If
            'End If
            'searchItem = searchItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault
            'If searchItem IsNot Nothing AndAlso dictGridInfo("TEMP_INFO5").DispFlag = "1" AndAlso _ViewModel.SearchTempInfo5 IsNot Nothing AndAlso _ViewModel.SearchTempInfo5 = "" Then
            '    If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA3)) Then
            '        _ViewModel.SearchTempInfo5 = searchItem.DATA3
            '    End If
            'End If
        End Sub
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

    End Class
End Namespace