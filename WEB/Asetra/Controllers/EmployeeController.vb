Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Threading
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers
    ''' <summary>
    ''' 社員マスタメンテナンス画面表示用のクラス
    ''' </summary>
    Public Class EmployeeController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


#Region "クラス定数"

        ''' <summary>
        ''' データ取込画面　結果表示用クラス
        ''' </summary>
        Private Class CsvEmployee
            Public Class ColIdx
                ''' <summary>社員コード</summary>
                Public Const Cd As Integer = 0
                ''' <summary>氏名</summary>
                Public Const Name As Integer = 1
                ''' <summary>退職フラグ</summary>
                Public Const RetireFlg As Integer = 2
                ''' <summary>部署コード</summary>
                Public Const BushoCd As Integer = 3
                ''' <summary>部署名</summary>
                Public Const BushoName As Integer = 4
                ''' <summary>権限区分</summary>
                Public Const KengenKbn As Integer = 5
                ''' <summary>メールアドレス</summary>
                Public Const MailAddress As Integer = 6
                ''' <summary>所属拠点大区分コード(開始)</summary>
                Public Const LocLCd As Integer = 7
            End Class
            'Public Class ColNm
            '    ''' <summary>社員コード</summary>
            '    Public Const Cd As String = "社員コード"
            '    ''' <summary>社員名</summary>
            '    Public Const Name As String = "社員名"
            '    ''' <summary>部署コード</summary>
            '    Public Const BushoCd As String = "部署コード"
            '    ''' <summary>権限区分</summary>
            '    Public Const KengenKbn As String = "権限区分"
            'End Class
        End Class
        ''' <summary>
        ''' ページ間のチェックボックス対応
        ''' </summary>
        Private Enum EmployeeListKey
            EMPLOYEE_CD = 0
        End Enum
        'Public Const AUTH_OPTION As String = "U_HUB_AUTH"     '特殊権限項目  '2021/12/02 nv miyaji 関数を使うように変更のためコメントアウト
#End Region

#Region "クラス変数"
        Private _Model As EmployeeModel = New EmployeeModel                                'Employeeモデル
        Private _ViewModel As EmployeeViewModel = New EmployeeViewModel                    'Employee一覧表示用モデル
        Protected _viewResult As List(Of EmployeeViewModel) = New List(Of EmployeeViewModel)     '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _lstsHubLocLCd As List(Of String)   '2021/09/06 nv miyaji add 追加する社員所属拠点のリスト
        Private _dtCsvImport As DataTable = Nothing 'CSV取込用作業テーブル
        Private _lstCsvHed As List(Of String) = Nothing 'CSVヘッダ文字コード格納用リスト
        Private _params As List(Of SqlParameter)                'SQLパラメータ
        Private _AuthInfo As AuthInfo = Nothing                 '権限情報
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As EmployeeViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
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
        Public Function Index(ByVal ViewModel As EmployeeViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_EMPLOYEE)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As EmployeeViewModel = Session(CommonSessionKey.CREAGE_EMPLOYEE)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            ViewBag.DrugsFunc = False

            '検索ボタンがクリックされた場合
            If Me._Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session(CommonSessionKey.SEARCH_FLG) = True
            End If

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '--2021/09/16 nv miyaji add start
                'ログインユーザーを再設定する。
                UpdateLoginInfo()
                '--2021/09/16 nv miyaji add start

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo

                ''2021/11/19 nv miyaji CreateSQL()関数の中に移動
                ''--2021/09/27 nv miyaji add start
                ''メッセージ一覧取得
                'Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                ''権限情報の取得
                'Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Employee, _LoginInfo)
                'If AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                '    '更新・照会権限有り
                '    If _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Kyoten_Admin Then
                '        '拠点責任者の場合
                '        '権限オプション＝"U_HUB_AUTH"の場合
                '        If AuthInfo.AuthOption = AUTH_OPTION Then
                '            '自拠点の権限のみ更新可能
                '            ViewBag.AuthProcFlg = AuthProcFlg.JiAll
                '        Else
                '            '自拠点の照会のみ
                '            ViewBag.AuthProcFlg = AuthProcFlg.JiShokaiNomi
                '        End If
                '    Else
                '        '拠点責任者でない場合
                '        ViewBag.AuthProcFlg = AuthProcFlg.All
                '    End If
                'Else
                '    '照会のみ
                '    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                'End If
                ''--2021/09/27 nv miyaji add end

                '権限情報の取得
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Employee, _LoginInfo)

                '最新のチェック状態でセッション情報を更新する(追加および削除)
                UpdateChecedInfo(_ViewModel.SelectKeys, _ViewModel.NoneSelectKeys)

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
            End If

            ''直近登録情報の設定
            'Dim tbl As DataTable = Dao.ToolCommon.GetMstInsInfo("M_EMPLOYEE")
            'If tbl IsNot Nothing AndAlso tbl.Rows.Count <> 0 Then
            '    ViewBag.LastInsDateTime = CommonRfid.ToStringDateYMDHM(Date.Parse(tbl.Rows(0).Item("INS_DATETIME").ToString))
            '    ViewBag.LastInsEmployeeNm = tbl.Rows(0).Item("INS_EMPLOYEE_NAME").ToString
            'End If

            ' プリンタ情報取得
            GetPrinter()

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.EMPLOYEE, IIf(String.IsNullOrEmpty(_ViewModel.SearchHubLocLCd), COLSET_LOC.TEMPLATE, _ViewModel.SearchHubLocLCd))
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

            '部署コンボボックス設定
            DropDownListCreate()

            'モーダルダイアログの所属拠点ドロップダウンリスト作成
            GetLocationLData()
            GetLocationMData(_ViewModel.SearchHubLocLCd)
            If Not _ViewModel.HUB_LOC_L Is Nothing Then
                For i As Integer = 0 To _ViewModel.HUB_LOC_L.Count - 1
                    GetLocationHubMData(_ViewModel.HUB_LOC_L(i), i)
                Next
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
                Session.Remove(CommonSessionKey.CREAGE_EMPLOYEE)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()

            'クエリを返却
            MyBase._SQL = CreateSQL(False)

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of EmployeeViewModel) = _db.EmployeeView.SqlQuery(_SQL, _params.ToArray)
            'Dim query As DbSqlQuery(Of EmployeeViewModel) = _db.EmployeeView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As EmployeeViewModel, ByVal OnCreteButton As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Employee, _LoginInfo)

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
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.EMPLOYEE_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEENM) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.EMPLOYEE_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelDepartment.TITLE))

                    _ViewModel.States = StatusType.SuccessCreate
                Else

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.EMPLOYEE_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEENM) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.EMPLOYEE_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelDepartment.TITLE))

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
            Session(CommonSessionKey.CREAGE_EMPLOYEE) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_EMPLOYEE) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            'IDが登録済みかチェックする
            Dim Employee As EmployeeModel = _db.Employee.Find(_Model.EMPLOYEE_CD)

            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(Employee) Then
                ModelState.AddModelError("EMPLOYEE_CD", CommonLabel.MSG_ERR_DATAJUUFUKU)
                blnReturn = False
            End If

            '登録する社員所属拠点をチェックする
            If Not CheckHubLocCd() Then
                blnReturn = False
            End If

            Return blnReturn

        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()
            '登録日時・登録社員コード・登録社員名・登録プロセスを登録
            _Model.INS_DATETIME = CommonRfid.getNowDate
            _Model.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE)

            _db.Employee.Add(_Model)

            Try
                '社員所属拠点マスタ削除
                EmployeeHubDelete()
                '社員所属拠点マスタ登録
                For i As Integer = 0 To CommonConst.MAX_HUBLOC_CNT - 1
                    If Not String.IsNullOrEmpty(_ViewModel.HUB_LOC_L(i)) Then
                        EmployeeHubInsert(_ViewModel.HUB_LOC_L(i), _ViewModel.HUB_LOC_M(i))
                    End If
                Next

            Catch ex As Exception
                ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)
            End Try

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As EmployeeModel = db.Employee.Find(_Model.EMPLOYEE_CD)

            '他端末で更新されているかチェックする
            If _Model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") Then
                'モデルとDBの更新日付が異なっている場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                blnReturn = False
            End If

            '登録する社員所属拠点をチェックする
            If Not CheckHubLocCd() Then
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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE)

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified

            Try
                '社員所属拠点マスタ削除
                EmployeeHubDelete()
                '社員所属拠点マスタ登録
                For i As Integer = 0 To CommonConst.MAX_HUBLOC_CNT - 1
                    If Not String.IsNullOrEmpty(_ViewModel.HUB_LOC_L(i)) Then
                        EmployeeHubInsert(_ViewModel.HUB_LOC_L(i), _ViewModel.HUB_LOC_M(i))
                    End If
                Next

            Catch ex As Exception
                ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)
            End Try

        End Sub

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ID">ID</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal ID As String) As JsonResult
            Dim db As New SystemDBContext

            'IDをキーにモデルを取得する
            Dim model As EmployeeModel = db.Employee.Find(ID)
            'model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            'model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
                '結果表示
                Return Json(Me._lstErr)
            End If
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Employee, _LoginInfo)

            'IDをキーに社員所属拠点データを取得する
            Dim tbl As DataTable = GetEmployeeHubData(ID)

            '取得したデータをビューモデルにコピーする
            Dim ViewModel As EmployeeViewModel = New EmployeeViewModel
            CopyModelToViewModel(ViewModel, model, tbl)

            Return Json(ViewModel)

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
        Public Overridable Function Delete(ByVal ViewModel As EmployeeViewModel) As ActionResult
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

            '削除処理
            MyBase.BaseDelete(_ViewModel)

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                '操作ログ出力
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.EMPLOYEE_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEENM) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.EMPLOYEE_NAME).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelDepartment.TITLE))

                _ViewModel.States = StatusType.SuccessDelete
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
            Session(CommonSessionKey.CREAGE_EMPLOYEE) = _ViewModel

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
            Dim employee As EmployeeModel = _db.Employee.Find(_Model.EMPLOYEE_CD)

            '削除を実行する
            _db.Employee.Remove(employee)

            '社員所属拠点マスタ削除
            EmployeeHubDelete()

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
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Employee, _LoginInfo)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

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
                sProcessContents = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE))

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

            'CSVヘッダー取得
            Dim sHedder As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_EMPLOYEE)
            Dim stArrayData As String() = sHedder.Split(","c)

            'CSVデータ取得 & 項目数チェック
            Dim isKomokusuuErr As Boolean = False
            Dim tbl As DataTable = clsSV_CsvFile.ReadCsvFileToDataTable(sCsvPath, Encoding.UTF8, CChar(","), True, "Employee", stArrayData.Length, isKomokusuuErr)
            '項目数エラーの場合
            If isKomokusuuErr Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " &
                                                     Path.GetFileName(CsvFile.FileName)
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE))

                Return Json(Me._lstErr)
            End If

            'エラーチェック
            If Not IsDataError(tbl, stArrayData) Then
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
                                                     Path.GetFileName(CsvFile.FileName) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                     Me._lstErr.Count.ToString
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE))

                Return Json(Me._lstErr)
            End If

            'パスワード列の追加
            tbl.Columns.Add("PASSWORD", Type.GetType("System.String"))
            For Each row As DataRow In tbl.Rows
                '--社員データを取得
                Dim tblEmployee As DataTable = Dao.ToolEmployee.GetEmployeeData(row(CsvEmployee.ColIdx.Cd).ToString.Trim.TrimStart("'"c))
                '--パスワード設定
                If tblEmployee.Rows.Count = 0 Then
                    row("PASSWORD") = Nothing
                Else
                    row("PASSWORD") = tblEmployee.Rows(0)("PASSWORD").ToString
                End If
            Next

            '--登録処理開始
            Dim db As New SystemDBContext
            Dim model As List(Of EmployeeModel) = New List(Of EmployeeModel)                '社員登録用モデル
            Dim modelHub As List(Of EmployeeHubModel) = New List(Of EmployeeHubModel)                '社員所属拠点登録用モデル

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try

                    '社員マスタデータを全削除
                    '　スーパー管理者以外かつ自拠点のデータ
                    Dim sbSQL As StringBuilder = New StringBuilder
                    Dim params As New List(Of SqlParameter)
                    sbSQL.AppendLine("DELETE FROM M_EMPLOYEE ")
                    sbSQL.AppendLine(" WHERE AUTH_DIV <> '" & Misc.AUTH_DIV.SuperAdmin & "'")
                    sbSQL.AppendLine("  AND EXISTS")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine(" SELECT 1 FROM M_EMPLOYEE_HUB HUB")
                    sbSQL.AppendLine(" INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                    sbSQL.AppendLine(" ON HUB.HUB_LOC_L_CD = ULOC.LOC_L_CD ")
                    sbSQL.AppendLine(" AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = HUB.HUB_LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
                    params.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter("@ReadAuthFlg", _AuthInfo.ReadAuthFlg))
                    params.Add(New SqlParameter("@UpdAuthFlg", _AuthInfo.UpdAuthFlg))
                    params.Add(New SqlParameter("@ReadHubAuthFlg", _AuthInfo.ReadHubAuthFlg))
                    params.Add(New SqlParameter("@UpdHubAuthFlg", _AuthInfo.UpdHubAuthFlg))
                    params.Add(New SqlParameter("@UpdAuthOption", _AuthInfo.AuthOption))
                    sbSQL.AppendLine(" WHERE HUB.EMPLOYEE_CD = M_EMPLOYEE.EMPLOYEE_CD")
                    sbSQL.AppendLine(")")
                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray())

                    '社員所属拠点マスタデータを全削除
                    '　社員マスタに該当するコードがないもののみ
                    sbSQL.Clear()
                    sbSQL.AppendLine("DELETE FROM M_EMPLOYEE_HUB")
                    sbSQL.AppendLine(" WHERE NOT EXISTS")
                    sbSQL.AppendLine("( SELECT 1 FROM M_EMPLOYEE EMP")
                    sbSQL.AppendLine(" WHERE M_EMPLOYEE_HUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
                    sbSQL.AppendLine(")")
                    db.Database.ExecuteSqlCommand(sbSQL.ToString())

                    '登録用モデルにデータを入れる
                    For icnt As Integer = 0 To tbl.Rows.Count - 1
                        '--社員登録用モデル
                        model.Add(New EmployeeModel())
                        '--2021/09/14 nv miyaji update start
                        model(icnt).EMPLOYEE_CD = tbl.Rows(icnt).Item(CsvEmployee.ColIdx.Cd).ToString.Trim.TrimStart("'"c)
                        model(icnt).EMPLOYEE_NAME = tbl.Rows(icnt).Item(CsvEmployee.ColIdx.Name).ToString.Trim.TrimStart("'"c)
                        model(icnt).RETIRE_FLG = tbl.Rows(icnt).Item(CsvEmployee.ColIdx.RetireFlg).ToString.Trim.TrimStart("'"c)
                        model(icnt).PASSWORD = tbl.Rows(icnt).Item("PASSWORD").ToString.Trim
                        model(icnt).DEPARTMENT_CD = tbl.Rows(icnt).Item(CsvEmployee.ColIdx.BushoCd).ToString.Trim.TrimStart("'"c)
                        model(icnt).AUTH_DIV = tbl.Rows(icnt).Item(CsvEmployee.ColIdx.KengenKbn).ToString.Trim.TrimStart("'"c)
                        model(icnt).MAIL_ADDRESS = tbl.Rows(icnt).Item(CsvEmployee.ColIdx.MailAddress).ToString.Trim.TrimStart("'"c)
                        model(icnt).INS_DATETIME = CommonRfid.getNowDate
                        model(icnt).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(icnt).UPD_DATETIME = CommonRfid.getNowDate
                        model(icnt).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(icnt).UPD_TASK = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE)

                        '--社員所属拠点登録用モデル
                        For iHubEmployeeCnt As Integer = 0 To CommonConst.MAX_HUBLOC_CNT - 1
                            Dim cdCol As Integer = CsvEmployee.ColIdx.LocLCd + （4 * iHubEmployeeCnt)
                            Dim sLocL As String = tbl.Rows(icnt).Item(cdCol).ToString.Trim.TrimStart("'"c)
                            Dim sLocM As String = tbl.Rows(icnt).Item(cdCol + 2).ToString.Trim.TrimStart("'"c)

                            If Not String.IsNullOrEmpty(sLocL) AndAlso Not String.IsNullOrEmpty(sLocM) Then
                                '所属拠点コードが入っている場合、社員所属拠点マスタを追加する
                                Dim hubModel = New EmployeeHubModel()
                                With hubModel
                                    .EMPLOYEE_CD = tbl.Rows(icnt).Item(CsvEmployee.ColIdx.Cd).ToString.Trim.TrimStart("'"c)
                                    .HUB_LOC_L_CD = sLocL
                                    .HUB_LOC_M_CD = sLocM
                                    .INS_DATETIME = CommonRfid.getNowDate
                                    .INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                    .INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                    .UPD_DATETIME = CommonRfid.getNowDate
                                    .UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                    .UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                    .UPD_TASK = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE)
                                End With
                                modelHub.Add(hubModel)
                            End If
                        Next
                    Next

                    'システムコンテキストにモデルを追加
                    db.Employee.AddRange(model)
                    db.EmployeeHub.AddRange(modelHub)   '2021/09/16 nv miyaji add

                    '登録処理を実行
                    db.SaveChanges()

                    ''所属拠点マスタ全データ削除
                    'Dim sDelete As String = String.Format("DELETE FROM M_EMPLOYEE_HUB")
                    'db.Database.ExecuteSqlCommand(sDelete)

                    tran.Commit()

                    '操作ログ出力
                    sProcessContents = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE) & " " &
                                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                             tbl.Rows.Count.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE))
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

#Region "全データCSV出力処理"
        ''' <summary>
        ''' 全データCSV出力処理
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
            Dim EmployeeData As List(Of EmployeeViewModel)

            '社員マスタデータ取得
            'SQLを実行する
            EmployeeData = db.EmployeeView.SqlQuery(CreateSQL(True), _params.ToArray).ToList
            'EmployeeData = GetAllEmployeeData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.Employee.KeyDiv, CommonSettingKey.Employee.Key1, CommonSettingKey.Employee.Key2, CommonSettingKey.Employee.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            'Dim sUrlFileName As String = String.Format(sFileNm + "_{0}.csv", System.DateTime.Now.ToString("yyyyMMddHHmmss"))
            Dim sUrlFileName As String = Path.GetRandomFileName()
            Dim sDspFileName As String = String.Format(sFileNm + "_{0}.csv", System.DateTime.Now.ToString("yyyyMMdd"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sUrlFileName)

            '◆CSV出力用リスト作成
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'CSVヘッダー取得
            'Dim sHedder As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_EMPLOYEE)
            Dim sHedder As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_EMPLOYEE)
            Dim singleQuotation As String = "'"

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHedder))
            'データ行挿入
            Dim col As String = ""
            For Each row As EmployeeViewModel In EmployeeData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.RETIRE_FLG)  '2021/09/13 nv miyaji add
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.DEPARTMENT_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DEPARTMENT_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.AUTH_DIV)
                '--2021/09/13 nv miyaji add start
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAIL_ADDRESS)
                Dim arrHubLocCd() As String = New String() {}
                Dim arrHubLocName() As String = New String() {}    '2021/11/25 nv miyaji add
                If Not String.IsNullOrEmpty(row.HUB_LOC_L_CD) Then
                    arrHubLocCd = row.HUB_LOC_L_CD.Split(",")
                    If row.HUB_LOC_L_NAME IsNot Nothing Then
                        arrHubLocName = row.HUB_LOC_L_NAME.Split(",")    '2021/11/25 nv miyaji add
                    End If
                End If
                Dim arrCD() As String = New String() {}
                Dim arrName() As String = New String() {}
                For iCnt As Integer = 0 To CommonConst.MAX_HUBLOC_CNT - 1
                    Dim str As String = String.Empty
                    If arrHubLocCd.Length > iCnt AndAlso arrHubLocName.Length > iCnt Then
                        arrCD = arrHubLocCd(iCnt).Split("-")            '所属拠点大区分コード-中区分コード
                        arrName = arrHubLocName(iCnt).Split("-")        '所属拠点大区分名称-中区分名称
                        str = singleQuotation + arrHubLocCd(iCnt)
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + arrCD(0))       '所属拠点大区分コード
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(arrName(0))                       '所属拠点大区分名称
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + arrCD(1))       '所属拠点中区分コード
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(arrName(1))                       '所属拠点中区分名称
                    Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                    End If
                Next

                col += ""
                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                'ファイルをダウンロードさせる
                Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)
                'Return File(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

                'ダウンロードさせた後、ファイルを削除。
                System.IO.File.Delete(sFullPath + "\" + sUrlFileName)
                'Response.Redirect("index")

                ''10025,INF,,,%1が完了しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 10025, "CSVファイルの出力")
            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     EmployeeData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE))


            '結果表示
            'Return Json(EmployeeData)
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function

#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAllEmployeeData() As List(Of EmployeeViewModel)
            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  EMP.EMPLOYEE_CD ")
            sbSQL.AppendLine("  , EMP.EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , EMP.DEPARTMENT_CD ")
            sbSQL.AppendLine("  , EMP.PASSWORD ")
            sbSQL.AppendLine("  , EMP.AUTH_DIV ")
            '--2021/09/13 nv miyaji add start
            sbSQL.AppendLine("  , EMP.RETIRE_FLG ")
            sbSQL.AppendLine("  , EMP.MAIL_ADDRESS ")
            '--2021/09/13 nv miyaji add end
            sbSQL.AppendLine("  , EMP.INS_DATETIME ")
            sbSQL.AppendLine("  , EMP.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , EMP.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , EMP.UPD_DATETIME ")
            sbSQL.AppendLine("  , EMP.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , EMP.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , EMP.UPD_TASK ")
            sbSQL.AppendLine("  , DEP.DEPARTMENT_NAME ")
            '--2021/09/13 nv miyaji add start
            sbSQL.AppendLine("  ,REPLACE(")
            sbSQL.AppendLine("     STUFF((SELECT '|||' + HUB.HUB_LOC_L_CD")
            sbSQL.AppendLine("     FROM  M_EMPLOYEE_HUB AS HUB WITH (NOLOCK)")
            sbSQL.AppendLine("     WHERE HUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("     ORDER BY HUB.HUB_LOC_L_CD")
            sbSQL.AppendLine("     FOR XML PATH (''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 3, ''), '|||', ',') AS HUB_LOC_L_CD")
            '--2021/09/13 nv miyaji add end
            sbSQL.AppendLine("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM M_EMPLOYEE EMP")
            sbSQL.AppendLine("LEFT JOIN M_DEPARTMENT DEP ")
            sbSQL.AppendLine("ON EMP.DEPARTMENT_CD = DEP.DEPARTMENT_CD ")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("EMP.AUTH_DIV <> '" & Misc.AUTH_DIV.SuperAdmin & "'")

            sbSQL.AppendLine(" ORDER BY EMP.EMPLOYEE_CD")

            Dim lstEmployee As List(Of EmployeeViewModel)     '社員リスト
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstEmployee = db.EmployeeView.SqlQuery(sbSQL.ToString).ToList

            'データを返却
            Return lstEmployee

        End Function
#End Region

#Region "コンボボックス用部署マスタ取得処理"
        ''' <summary>
        ''' コンボボックス用に部署マスタを取得します
        ''' </summary>
        Public Sub DropDownListCreate()

            Dim lstDepartment As List(Of DepartmentModel)     '部署リスト
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext
            'M_DEPARTMENTテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT *")
            sbSQL.AppendLine(" FROM ")
            sbSQL.AppendLine("  M_DEPARTMENT ")
            sbSQL.AppendLine(" ORDER BY DEPARTMENT_CD ")
            'SQLを実行する
            lstDepartment = db.Department.SqlQuery(sbSQL.ToString).ToList
            '一覧画面の部署コンボ設定
            If String.IsNullOrEmpty(_ViewModel.SearchBusho) Then
                ViewBag.DepartmentList = New SelectList(lstDepartment, "DEPARTMENT_CD", "DEPARTMENT_NAME", String.Empty)
            Else
                ViewBag.DepartmentList = New SelectList(lstDepartment, "DEPARTMENT_CD", "DEPARTMENT_NAME", _ViewModel.SearchBusho)
            End If

            'モーダル画面の部署コンボ設定
            Dim SelectedCd As String = _ViewModel.DEPARTMENT_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalDepartmentList = New SelectList(lstDepartment, "DEPARTMENT_CD", "DEPARTMENT_NAME", String.Empty)
            Else
                ViewBag.modalDepartmentList = New SelectList(lstDepartment, "DEPARTMENT_CD", "DEPARTMENT_NAME", SelectedCd)
            End If

        End Sub
#End Region

#Region "ファイルをダウンロードさせます。"
        ''' <summary>
        ''' ファイルをダウンロードさせます。
        ''' </summary>
        ''' <param name="FileName">ダウンロードするファイル名を指定します。</param>
        ''' <param name="MimeType">ダウンロードするファイルのMIMEタイプを指定します。</param>
        ''' <param name="sFileFullPath">ダウンロードするファイルのフルパス</param>
        Private Sub FileDownload(FileName As String, MimeType As String, ByVal sFileFullPath As String)

            ''相対パスから物理ファイルパス取得
            'Dim FilePath As String = "D:\DATA\CSV\社員_20190702.csv"

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
            If String.IsNullOrEmpty(extention) OrElse extention.ToLower <> "csv" Then
                'ViewBag.Message = "CSVファイルを選択してください。"
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
        ''' <param name="tbl">チェック対象のデータテーブル</param>
        ''' <returns>True:エラー無し　False:エラー有り</returns>
        Public Function IsDataError(tbl As DataTable, stArrayData As String()) As Boolean
            Dim blRet As Boolean = False

            '項目数チェック
            If IsError(tbl) OrElse tbl.Columns.Count <> stArrayData.Length Then
                'エラー
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                Return blRet
            End If

            '部署マスタデータ取得
            Dim DapartmentData As DataTable
            DapartmentData = Dao.ToolDepartment.GetDepartmentData(String.Empty)

            'ロケーションマスタデータ取得(所属拠点のみ)
            Dim LocData As DataTable
            LocData = Dao.ToolLocation.GetHubLocationFromDiv(LOC_DIV.LocationM, _LoginInfo, _AuthInfo)

            '担当者マスタ取得(所属拠点外)
            Dim OtherHubEmpData As DataTable
            OtherHubEmpData = Dao.ToolEmployee.GetOtherHubEmpData(_LoginInfo, _AuthInfo)

            '担当者マスタ取得
            Dim EmpData As DataTable
            EmpData = Dao.ToolEmployee.GetEmployeeDataAll()

            '--データチェック
            blRet = True
            Dim lstSumiCd As List(Of String) = New List(Of String)

            For iRowCnt As Integer = 0 To tbl.Rows.Count - 1
                For iCalCnt As Integer = 0 To stArrayData.Length - 1
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If

                    '列によってチェック条件・設定項目値を分ける
                    Dim sStr As String = tbl.Rows(iRowCnt).Item(iCalCnt).ToString.Trim.TrimStart("'"c)
                    Select Case iCalCnt
                        '社員コード
                        Case CsvEmployee.ColIdx.Cd
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.EmployeeCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf lstSumiCd.IndexOf(sStr) > -1 Then
                                '重複チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID) & "[{0}]", sStr)
                                blRet = False
                            Else
                                Dim OtherHubEmpDataRow As DataRow() = Nothing
                                OtherHubEmpDataRow = OtherHubEmpData.Select("EMPLOYEE_CD = '" + sStr + "'")
                                If OtherHubEmpDataRow Is Nothing OrElse OtherHubEmpDataRow.Length > 0 Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERRNO_EMPLOYEE_OTHER_HUB)
                                    blRet = False
                                Else
                                    lstSumiCd.Add(sStr)
                                End If
                            End If
                        '退職フラグ
                        Case CsvEmployee.ColIdx.RetireFlg
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_RETIRE_FLG)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr <> RetireFlg.Cd.Yes AndAlso sStr <> RetireFlg.Cd.No Then
                                '文字制約チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_RETIRE_FLG)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                        '社員名
                        Case CsvEmployee.ColIdx.Name
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEENM)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.EmployeeNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEENM)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        '部署コード
                        Case CsvEmployee.ColIdx.BushoCd
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_BUSHOCODE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                                '項目長チェック
                            ElseIf sStr.Length > itemlength.DepartmentCd Then
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_BUSHOCODE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_BUSHOCODE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            Else
                                Dim DepartmentRow As DataRow() = Nothing
                                DepartmentRow = DapartmentData.Select("DEPARTMENT_CD = '" + sStr + "'")
                                If DepartmentRow Is Nothing OrElse DepartmentRow.Length = 0 Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_BUSHOCODE)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If

                                If _AuthInfo.UpdHubAuthFlg = AUTH_FLG.No Then
                                    '他拠点更新権限がない場合
                                    Dim employeeRow As DataRow() = Nothing
                                    employeeRow = EmpData.Select("EMPLOYEE_CD = '" + tbl.Rows(iRowCnt).Item(CsvEmployee.ColIdx.Cd).ToString.Trim.TrimStart("'"c) + "'")
                                    If Not employeeRow Is Nothing AndAlso employeeRow.Length > 0 AndAlso
                                        employeeRow(0).Item("DEPARTMENT_CD") <> sStr Then
                                        '元のデータから部署コードが変更されていた場合はエラー
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_BUSHOCODE)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelEmployee.MSG_ERR_NOT_EDIT_DEPARTMENT_CD)
                                        blRet = False
                                    End If
                                End If
                            End If

                        '権限区分
                        Case CsvEmployee.ColIdx.KengenKbn
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_KENGEN)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.KengenKbn Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_KENGEN)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso
                                Not (sStr = AUTH_DIV.Nomal OrElse sStr = AUTH_DIV.Admin OrElse sStr = AUTH_DIV.SuperAdmin) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_KENGEN)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso sStr = AUTH_DIV.SuperAdmin Then
                                'スーパーユーザーの場合はエラー
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_KENGEN)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                        'メールアドレス
                        Case CsvEmployee.ColIdx.MailAddress
                            If Not String.IsNullOrEmpty(sStr) Then
                                '項目長チェック
                                If sStr.Length > itemlength.MailAddress Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAIL_ADDRESS)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                    blRet = False
                                ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                                    '半角英数字記号チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAIL_ADDRESS)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                End If
                            End If
                    End Select

                    If iCalCnt >= CsvEmployee.ColIdx.LocLCd Then
                        If (iCalCnt - CsvEmployee.ColIdx.LocLCd) Mod 4 = 0 Then
                            '所属拠点大区分コード
                            If Not String.IsNullOrEmpty(sStr) Then
                                '項目長チェック
                                If sStr.Length > itemlength.LocLCd Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_COM_CODE)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                    blRet = False
                                ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                    '半角英数字チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_COM_CODE)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                End If
                            End If
                        ElseIf (iCalCnt - CsvEmployee.ColIdx.LocLCd) Mod 2 = 0 Then
                            '所属拠点中区分コード
                            If Not String.IsNullOrEmpty(sStr) Then
                                '項目長チェック
                                If sStr.Length > itemlength.LocMCd Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_CODE)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                    blRet = False
                                ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                    '半角英数字チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_CODE)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                End If
                            End If
                        End If
                    End If
                Next

                Dim lstLoc As List(Of String) = New List(Of String)    '所属拠点(重複確認用)
                Dim hasLocError As Boolean = False
                For kyoten As Integer = 0 To CommonConst.MAX_HUBLOC_CNT - 1
                    Dim cdCol As Integer = CsvEmployee.ColIdx.LocLCd + (4 * kyoten)
                    Dim sLocL As String = tbl.Rows(iRowCnt).Item(cdCol).ToString.Trim.TrimStart("'"c)
                    Dim sLocM As String = tbl.Rows(iRowCnt).Item(cdCol + 2).ToString.Trim.TrimStart("'"c)

                    If Not String.IsNullOrEmpty(sLocL) AndAlso String.IsNullOrEmpty(sLocM) Then
                        '所属拠点大区分コードが入っていて所属拠点中区分コードが入っていない場合
                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_COM_CODE)
                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERRNO_EMPLOYEE_HUB_LOC_SET)
                        blRet = False
                        hasLocError = True
                        Exit For
                    End If

                    If String.IsNullOrEmpty(sLocL) AndAlso Not String.IsNullOrEmpty(sLocM) Then
                        '所属拠点大区分コードが入っていなくて所属拠点中区分コードが入っている場合
                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_COM_CODE)
                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERRNO_EMPLOYEE_HUB_LOC_SET)
                        blRet = False
                        hasLocError = True
                        Exit For
                    End If

                    If Not String.IsNullOrEmpty(sLocL) AndAlso Not String.IsNullOrEmpty(sLocM) Then
                        '所属拠点大区分コード、所属拠点中区分コードが入っている場合
                        If lstLoc.IndexOf(sLocL & "|" & sLocM) > -1 Then
                            '所属拠点が重複している場合はエラー
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_COM_CODE)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_HUBLOCLCD_JUUFUKU)
                            blRet = False
                            hasLocError = True
                            Exit For
                        End If

                        '所属拠点を重複確認用リストに追加
                        lstLoc.Add(sLocL & "|" & sLocM)

                        '所属拠点に存在していない場合はエラー
                        Dim LocRow As DataRow() = Nothing
                        LocRow = LocData.Select("LOC_L_CD = '" + sLocL + "' AND LOC_M_CD = '" + sLocM + "'")
                        If LocRow Is Nothing OrElse LocRow.Length = 0 Then
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_CODE)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERRNO_EMPLOYEE_HUB_NOT_EXISTS)
                            blRet = False
                            hasLocError = True
                            Exit For
                        End If
                    End If
                Next
                If Not hasLocError AndAlso lstLoc.Count = 0 Then
                    '所属拠点が1件もない場合はエラー
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_COM_CODE)
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERRNO_SELECTED_HUB_LOC_IMP)
                    blRet = False
                End If
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

#Region "ロケ区分ドロップダウン変更処理"
        ''' <summary>
        ''' ロケ区分ドロップダウン変更処理
        ''' </summary>
        ''' <param name="locationLevel"></param>
        ''' <param name="selectLCode"></param>
        ''' <returns></returns>
        Public Function ChangeLocation(ByVal locationLevel As String, ByVal selectLCode As String, ByVal controlName As String) As ActionResult
            Dim settingRfid As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)               'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel        'ロケーション使用区分
            Dim setting As SettingInfo = settingRfid.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingRfid.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            modelLocationUsage.LocUsage_L = setting.DATA1
            modelLocationUsage.LocUsage_M = setting.DATA2
            modelLocationUsage.LocUsage_S = setting.DATA3

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Employee, _LoginInfo)

            If locationLevel = LOC_DIV.LocationM Then
                If modelLocationUsage.LocUsage_M = Misc.ItemUse.Yes Then
                    If controlName = "SearchHubLocLCd" Then
                        Return Json(GetLocationMData(selectLCode))
                    ElseIf controlName.IndexOf("HUB_LOC_L") >= 0 Then
                        Dim indexNo As Integer
                        If Integer.TryParse(controlName.Replace("HUB_LOC_L[", "").Replace("]", ""), indexNo) Then
                            Return Json(GetLocationHubMData(selectLCode, indexNo))
                        Else
                            Return Nothing
                        End If
                    Else
                        Return Nothing
                    End If
                Else
                    '中区分 使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationLData() As JsonResult

            Dim tbl As DataTable
            If Not _LoginInfo Is Nothing Then
                tbl = Dao.ToolLocation.GetLocationLData(_LoginInfo, _AuthInfo)
            Else
                tbl = Dao.ToolLocation.GetLocationLData()
            End If
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.lstLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchHubLocLCd)
            _ViewModel.locLDropdownData = lstLocation

            'モーダル画面の社員所属拠点コンボ設定
            For i As Integer = 0 To CommonConst.MAX_HUBLOC_CNT - 1
                If String.IsNullOrEmpty(_ViewModel.HUB_LOC_L(i)) Then
                    _ViewModel.locLOptions(i) = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", String.Empty)
                Else
                    _ViewModel.locLOptions(i) = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.HUB_LOC_L(i))
                End If
                _ViewModel.hubLocLDropdownDatas(i) = lstLocation
            Next
            ViewBag.LocLOptions = _ViewModel.locLOptions
            Return Json(lstLocation)

        End Function
#End Region

#Region "中区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLocationMData(ByVal selectLCode As String) As JsonResult
            Dim tbl As DataTable
            If Not _LoginInfo Is Nothing Then
                tbl = Dao.ToolLocation.GetLocationMData(selectLCode, _LoginInfo, _AuthInfo)
            Else
                tbl = Dao.ToolLocation.GetLocationMData(selectLCode)
            End If
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            'データを入れる
            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            '一覧画面のロケーションコンボ設定
            ViewBag.lstLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchHubLocMCd)
            _ViewModel.locMDropdownData = lstLocation

            Return Json(lstLocation)
        End Function

        ''' <summary>
        ''' 中区分ドロップダウン(所属拠点)リストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLocationHubMData(ByVal selectLCode As String, ByVal indexNo As Integer) As JsonResult
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            If Not String.IsNullOrEmpty(selectLCode) Then

                Dim tbl As DataTable
                If Not _LoginInfo Is Nothing Then
                    tbl = Dao.ToolLocation.GetLocationMData(selectLCode, _LoginInfo, _AuthInfo)
                Else
                    tbl = Dao.ToolLocation.GetLocationMData(selectLCode)
                End If

                'データを入れる
                For Each row As DataRow In tbl.Rows
                    lstLocation.Add(
                            New LocationDropdownModel With {
                                .LOC_CD = row("LOC_CD").ToString,
                                .LOC_NAME = row("LOC_NAME").ToString
                            }
                        )
                Next
            End If

            '一覧画面のロケーションコンボ設定
            _ViewModel.hubLocMDropdownDatas(indexNo) = lstLocation
            _ViewModel.locMOptions(indexNo) = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.HUB_LOC_M(indexNo))
            ViewBag.LocMOptions = _ViewModel.locMOptions

            Return Json(lstLocation)

        End Function
#End Region

#Region "社員所属拠点チェック"
        ''' <summary>
        ''' 登録する社員所属拠点をチェックする。重複するデータある、1件もない場合はエラーを返す。
        ''' </summary>
        ''' <returns>エラーの有った項目名</returns>
        Public Function CheckHubLocCd() As Boolean
            Dim lstLoc As New List(Of String)       '重複確認用リスト
            For i As Integer = 0 To CommonConst.MAX_HUBLOC_CNT - 1
                If Not String.IsNullOrEmpty(Me._ViewModel.HUB_LOC_L(i)) Then
                    '所属拠点大区分がある
                    If String.IsNullOrEmpty(Me._ViewModel.HUB_LOC_M(i)) Then
                        '所属拠点大区分があるが中区分がない場合はエラー
                        ModelState.AddModelError("HUB_LOC_M[" & i & "]", CommonErrorMsg.MSG_ERRNO_EMPLOYEE_HUB_LOC_M)
                        Return False
                    End If

                    Dim loc = Me._ViewModel.HUB_LOC_L(i) & "|" & Me._ViewModel.HUB_LOC_M(i)
                    If lstLoc.IndexOf(loc) > -1 Then
                        '所属拠点が重複している場合はエラー
                        ModelState.AddModelError("HUB_LOC_L[" & i & "]", CommonLabel.MSG_ERR_HUBLOCLCD_JUUFUKU)
                        Return False
                    End If

                    '所属拠点を重複確認用リストに追加
                    lstLoc.Add(loc)
                End If
            Next

            If lstLoc.Count = 0 Then
                '所属拠点が1件もない場合はエラー
                ModelState.AddModelError("HUB_LOC_L[0]", CommonErrorMsg.MSG_ERRNO_SELECTED_HUB_LOC)
                Return False
            End If

            Return True

        End Function
#End Region

#Region "社員所属拠点マスタ登録処理"
        ''' <summary>
        ''' 社員所属拠点マスタ登録処理
        ''' </summary>
        Public Sub EmployeeHubInsert(sHubLocLCd As String, sHubLocMCd As String)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.Clear()
            sbSQL.AppendLine("INSERT INTO M_EMPLOYEE_HUB")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("   EMPLOYEE_CD")
            sbSQL.AppendLine("  ,HUB_LOC_L_CD")
            sbSQL.AppendLine("  ,HUB_LOC_M_CD")
            sbSQL.AppendLine("  ,INS_DATETIME")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_DATETIME")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_TASK")
            sbSQL.AppendLine(") VALUES (")
            sbSQL.AppendLine(" @EMPLOYEE_CD")
            sbSQL.AppendLine(",@HUB_LOC_L_CD")
            sbSQL.AppendLine(",@HUB_LOC_M_CD")
            sbSQL.AppendLine(",@INS_DATETIME")
            sbSQL.AppendLine(",@INS_EMPLOYEE_CD")
            sbSQL.AppendLine(",@INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_DATETIME")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_TASK")
            sbSQL.AppendLine(")")

            params.Add(New SqlParameter($"@EMPLOYEE_CD", _ViewModel.EMPLOYEE_CD))
            params.Add(New SqlParameter($"@HUB_LOC_L_CD", sHubLocLCd))
            params.Add(New SqlParameter($"@HUB_LOC_M_CD", sHubLocMCd))
            params.Add(New SqlParameter($"@INS_DATETIME", _Model.INS_DATETIME))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _Model.INS_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _Model.INS_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_DATETIME", _Model.UPD_DATETIME))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _Model.UPD_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _Model.UPD_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_TASK", _Model.UPD_TASK))

            _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
        End Sub
#End Region

#Region "社員所属拠点マスタ削除処理"
        ''' <summary>
        ''' 社員所属拠点マスタ削除処理
        ''' </summary>
        Public Sub EmployeeHubDelete()
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.Clear()
            sbSQL.AppendLine("DELETE FROM M_EMPLOYEE_HUB")
            sbSQL.AppendLine("WHERE EMPLOYEE_CD = '" & _ViewModel.EMPLOYEE_CD & "'")
            _db.Database.ExecuteSqlCommand(sbSQL.ToString)
        End Sub
#End Region

#Region "該当社員コードのデータを社員所属拠点マスタから取得"
        ''' <summary>
        ''' 該当社員コードのデータを社員所属拠点マスタから取得
        ''' </summary>
        ''' <param name="sEmployeeCd">社員コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetEmployeeHubData(ByVal sEmployeeCd As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_EMPLOYEE_HUBテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT EMPLOYEE_CD")
                sbSQL.AppendLine(", HUB_LOC_L_CD")
                sbSQL.AppendLine(", HUB_LOC_M_CD")
                sbSQL.AppendLine("FROM M_EMPLOYEE_HUB")
                sbSQL.AppendLine("WHERE EMPLOYEE_CD = @EMPLOYEE_CD ")
                params.Add(New SqlParameter($"@EMPLOYEE_CD", CommonRfid.ToStringNvl(sEmployeeCd)))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

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
                sbSQL.AppendLine("CSV_DIV = '" & CsvDiv.Employee & "'")

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
                sbSQL.AppendLine("CSV_DIV = '" & CsvDiv.Employee & "'")

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
            '資産マスタのCSVマッピング明細の項目定義を取得
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.EmployeeCd)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.EmployeeNm)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.RetireFlg)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.DepartmentCd)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.AuthDiv)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.MailAddress)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd1)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd2)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd3)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd4)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd5)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd6)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd7)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd8)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd9)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd10)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd11)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd12)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd13)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd14)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd15)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd16)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd17)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd18)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd19)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd20)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd21)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd22)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd23)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd24)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd25)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd26)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd27)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd28)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd29)
            _lstCsvHed.Add(CsvMapMei_Employee.CharCd.HubLocLCd30)

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

            '重複チェック用変数（ハッシュテーブルに変更）     Update Inoue
            Dim htSumiCd As Hashtable = New Hashtable                   '資産番号重複チェック用（Key:資産番号 Value:資産ID）
            Dim htIDSumiCd As Hashtable = New Hashtable                 '資産ID重複チェック用（Key:資産ID Value:資産番号）

            'CSVマッピング明細マスタで列番指定の項目数取得
            Dim Colrows() As DataRow = tblCsvMapMei.Select("CSV_COL_NO = MAX(CSV_COL_NO) ")
            Dim iColNoCnt As Integer = CInt(Colrows(0).Item("CSV_COL_NO"))
            'Dim Colrows() As DataRow = tblCsvMapMei.Select("SPECIFIED_DIV = '" & CsvMapping_SpecifiedDiv.Index & "'")
            'Dim iColNoCnt As Integer = Colrows.Length

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

                    '--2021/11/30 add start nv iwamoto
                    '項目数がゼロ（空行）の場合は次行に遷移
                    If fields.Length = 0 Then
                        iRowCnt += 1
                        sline = sr.ReadLine()
                        Continue Do
                    End If
                    '--2021/11/30 add end nv iwamoto

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

#Region "ログインユーザーを再設定する。"
        ''' <summary>
        ''' ログインユーザーを再設定する。
        ''' </summary>
        ''' <remarks>2021/09/16 nv miyaji create</remarks>
        Public Sub UpdateLoginInfo()
            Dim db As New SystemDBContext

            Dim EmployeeModel As EmployeeModel = Nothing
            EmployeeModel = db.Employee.Find(_LoginInfo.EmployeeCd)
            '社員名
            _LoginInfo.EmployeeNm = EmployeeModel.EMPLOYEE_NAME
            '部門コード・部門名
            _LoginInfo.DepartmentCd = EmployeeModel.DEPARTMENT_CD
            'IDをキーにモデルを取得する
            Dim DepartmentModel As DepartmentModel = Nothing
            DepartmentModel = db.Department.Find(EmployeeModel.DEPARTMENT_CD)
            '部門名
            If DepartmentModel IsNot Nothing Then
                _LoginInfo.DepartmentNm = DepartmentModel.DEPARTMENT_NAME
            End If
            '権限区分
            _LoginInfo.KengenKbn = EmployeeModel.AUTH_DIV
        End Sub
#End Region

#Region "プリンター選択プルダウン用データ取得"
        ''' <summary>
        ''' プリンター選択プルダウン用データ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function ShowPrinterList() As ActionResult
            Dim lstSetting As List(Of PrinterDropdownModel)   'プリンタリスト
            lstSetting = GetPrinter()
            Return Json(lstSetting)
        End Function
#End Region

#Region "プリンタ情報取得"
        ''' <summary>
        ''' プリンタ情報取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetPrinter() As List(Of PrinterDropdownModel)
            Dim tbl As DataTable = Dao.ToolSetting.GetMstPrinterInfo()
            Dim lst As New List(Of PrinterDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New PrinterDropdownModel With {
                        .KEY1 = row("KEY1").ToString,
                        .DATA1 = row("DATA1").ToString
                    }
                )
            Next
            ViewBag.modelPrinter = New SelectList(lst, "KEY1", "DATA1")
            Return lst
        End Function
#End Region

#Region "社員ラベル発行用CSV出力処理"
        ''' <summary>
        ''' 社員ラベル発行用CSV出力処理
        ''' </summary>
        ''' <param name="SelectKeys">画面で選択された項目のキー値(スラッシュ区切り)</param>
        ''' <param name="PrinterKey">選択したラベルプリンターのKEY1</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExportForLabel(ByVal ViewModel As EmployeeViewModel, SelectKeys As String, PrinterKey As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            Dim EmployeeData As List(Of EmployeeCsvModel)

            '最新のチェック状態でセッション情報を更新する(追加および削除) (キー：社員コード)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            '社員マスタデータ取得
            EmployeeData = GetSelectEmployeeData(lstSelectKeys)

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ラベルプリンター情報の取得
            Dim labelPrinter As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LabelPrinter.KeyDiv, PrinterKey, CommonSettingKey.LabelPrinter.Key2, CommonSettingKey.LabelPrinter.Key3, True)

            '出力ファイル名取得
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.Employee.KeyDiv, CommonSettingKey.Employee.Key1, CommonSettingKey.Employee.Key2, CommonSettingKey.Employee.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

            '◆CSVファイル出力先フォルダ存在チェック
            'Dim sFullPath As String = Server.MapPath(CommonConst.ROOT_FOLDERNM & labelPrinter.DATA4)

            'Dim sFullPath As String = Server.MapPath(labelPrinter.DATA4)
            Dim sFullPath As String = System.IO.Path.GetFullPath(labelPrinter.DATA5)

            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            Dim sUrlFileName As String = Path.GetRandomFileName().Substring(0, 8)
            Dim sPrnFileName As String = String.Format(sFileNm & sUrlFileName & "{0}.csv", Now.ToString("HHmmss"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sPrnFileName)

            '◆CSV出力用リスト作成
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'データ行挿入
            Dim col As String = ""
            For Each row As EmployeeCsvModel In EmployeeData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(LabelType.EP)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(labelPrinter.DATA1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(labelPrinter.DATA2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAIL_ADDRESS)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DEPARTMENT_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DEPARTMENT_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EMPLOYEE_CD.ToString.Count)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot("1")          '発行枚数の1 を追加  Inoue 2019/09/03

                col += ""
                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                Dim labelPrintMethodSelect As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LabelPrntMethodSelect.KeyDiv, CommonSettingKey.LabelPrntMethodSelect.Key1, CommonSettingKey.LabelPrntMethodSelect.Key2, CommonSettingKey.LabelPrntMethodSelect.Key3, False)
                If labelPrintMethodSelect IsNot Nothing AndAlso labelPrintMethodSelect.DATA1 = "Y" Then
                    'ファイルをダウンロードさせる
                    Call FileDownload(sPrnFileName, "text/comma-separated-values", sFullPath + "\" + sPrnFileName)
                    'ダウンロードさせた後、ファイルを削除。
                    System.IO.File.Delete(sFullPath + "\" + sPrnFileName)
                End If

                ''10025,INF,,,%1が完了しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 10025, "CSVファイルの出力")
            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_LOC_LABEL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     EmployeeData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE))

            '正常時、正常終了のステータスを返す
            _ViewModel.States = StatusType.SuccessHakko

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_EMPLOYEE) = _ViewModel

            '再表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region

#Region "社員ラベル発行用CSV出力のデータを取得"
        ''' <summary>
        ''' 社員ラベル発行用CSV出力のデータを取得します
        ''' </summary>
        Private Function GetSelectEmployeeData(SelectKeys As List(Of String)) As List(Of EmployeeCsvModel)
            Dim lstEmployee As New List(Of EmployeeCsvModel)   'ロケーションリスト
            Dim db As SystemDBContext = New SystemDBContext

            For Each key As String In SelectKeys

                Dim sbSQL As StringBuilder = New StringBuilder
                sbSQL.Append("SELECT ")
                sbSQL.Append("  employee.EMPLOYEE_CD, ")
                sbSQL.Append("  employee.EMPLOYEE_NAME, ")
                sbSQL.Append("  employee.DEPARTMENT_CD, ")
                sbSQL.Append("  employee.MAIL_ADDRESS, ")
                sbSQL.Append("  department.DEPARTMENT_NAME ")
                sbSQL.Append("FROM ")
                sbSQL.Append("  M_EMPLOYEE AS employee ")
                sbSQL.Append("LEFT JOIN  ")
                sbSQL.Append("  M_DEPARTMENT AS department ")
                sbSQL.Append("ON  ")
                sbSQL.Append("  employee.DEPARTMENT_CD = department.DEPARTMENT_CD ")
                sbSQL.Append("WHERE ")
                sbSQL.Append("  employee.EMPLOYEE_CD = '" & key & "' ")
                sbSQL.Append(" ORDER BY employee.EMPLOYEE_CD")

                'SQLを実行する
                Dim lst As List(Of EmployeeCsvModel)
                lst = db.EmployeeCsv.SqlQuery(sbSQL.ToString).ToList
                If lst.Count > 0 Then
                    lstEmployee.Add(lst(0))
                End If
            Next

            'データを返却
            Return lstEmployee
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
        ''' <remarks>2020/02/12 miyaji add</remarks>
        Private Function UpdateChecedInfo(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As List(Of String)
            '登録・変更・削除ボタン押下時の一覧画面呼び出しの場合
            If Session(CommonSessionKey.CREAGE_LOCATION) IsNot Nothing Then
                '一覧にない社員コードは未選択キーとする
                If Not IsNothing(_ViewModel.EMPLOYEE_CD) Then
                    Dim results = From data In _viewResult Where data.EMPLOYEE_CD = _ViewModel.EMPLOYEE_CD
                    If results.Count <= 0 Then
                        NoneSelectKeys = String.Join("/", New String() {NoneSelectKeys, $"{_ViewModel.EMPLOYEE_CD}"})
                    End If
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

#Region "一覧データ取得用SQLを作成する"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <param name="isAll">一覧表示時に取得したデータを取得</param>
        ''' <returns></returns>
        Private Function CreateSQL(isAll As Boolean) As String
            '--2021/09/27 nv miyaji add start
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '権限情報の取得
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Employee, _LoginInfo)
            If Dao.ToolCommon.IsExistsAuthOption(_AuthInfo.AuthOption, AuthOptionName.U_NOEDIT_DEP) Then
                ViewBag.AuthNoEditDep = AUTH_FLG.Yes
            Else
                ViewBag.AuthNoEditDep = AUTH_FLG.No
            End If
            If _AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                '自拠点の更新・照会権限有り
                If _AuthInfo.UpdHubAuthFlg = AUTH_FLG.Yes Then
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

            Dim sbSQL As StringBuilder = New StringBuilder
            _params = New List(Of SqlParameter)

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  EMP.EMPLOYEE_CD ")
            sbSQL.AppendLine("  , EMP.EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , EMP.DEPARTMENT_CD ")
            sbSQL.AppendLine("  , EMP.PASSWORD ")
            sbSQL.AppendLine("  , EMP.AUTH_DIV ")
            '--2021/09/02 nv miyaji add start
            sbSQL.AppendLine("  , EMP.RETIRE_FLG ")
            sbSQL.AppendLine("  , EMP.MAIL_ADDRESS ")
            '--2021/09/02 nv miyaji add end
            sbSQL.AppendLine("  , EMP.INS_DATETIME ")
            sbSQL.AppendLine("  , EMP.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , EMP.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , EMP.UPD_DATETIME ")
            sbSQL.AppendLine("  , EMP.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , EMP.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , EMP.UPD_TASK ")
            sbSQL.AppendLine("  , DEP.DEPARTMENT_NAME ")
            '--2021/09/07 nv miyaji add start  2021/11/07 upd nv Inoue
            '参考サイト:https://sql55.com/query/concatenate-string-values-with-carriage-returns.php

            sbSQL.AppendLine("  ,REPLACE(")
            sbSQL.AppendLine("     STUFF((SELECT '|||' + HUB.HUB_LOC_L_CD + '-' + HUB.HUB_LOC_M_CD")
            sbSQL.AppendLine("     FROM  M_EMPLOYEE_HUB AS HUB WITH (NOLOCK)")
            sbSQL.AppendLine("     WHERE HUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("     ORDER BY HUB.HUB_LOC_L_CD,HUB.HUB_LOC_M_CD")
            sbSQL.AppendLine("     FOR XML PATH (''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 3, ''), '|||', ',') AS HUB_LOC_L_CD")

            sbSQL.AppendLine("  ,REPLACE(")
            sbSQL.AppendLine("     STUFF((SELECT '|||' + LOCL.LOC_NAME + '-' + LOCM.LOC_NAME")
            sbSQL.AppendLine("     FROM  M_EMPLOYEE_HUB AS HUB WITH (NOLOCK)")
            sbSQL.AppendLine("      LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("      ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("      AND HUB.HUB_LOC_L_CD = LOCL.LOC_L_CD")
            sbSQL.AppendLine("      LEFT JOIN M_LOCATION LOCM ")
            sbSQL.AppendLine("      ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
            sbSQL.AppendLine("      AND HUB.HUB_LOC_M_CD = LOCM.LOC_M_CD")
            sbSQL.AppendLine("     WHERE HUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("     ORDER BY HUB.HUB_LOC_L_CD,HUB.HUB_LOC_M_CD")
            sbSQL.AppendLine("     FOR XML PATH (''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 3, ''), '|||', ',') AS HUB_LOC_L_NAME")

            '--2021/09/07 nv miyaji add end
            sbSQL.AppendLine("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("   ,CASE WHEN UPD.EMPLOYEE_CD IS NULL THEN '" & AUTH_LOC_M_FLG.Yes & "' ELSE '" & AUTH_LOC_M_FLG.No & "' END AS UPD_FLG ")

            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  M_EMPLOYEE EMP ")
            sbSQL.AppendLine("LEFT JOIN M_DEPARTMENT DEP ")
            sbSQL.AppendLine("ON EMP.DEPARTMENT_CD = DEP.DEPARTMENT_CD ")
            'sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE_HUB SEARCH_HUB ")   '2021/11/07 nv miyaji 使ってないのでコメントアウト
            'sbSQL.AppendLine("ON EMP.EMPLOYEE_CD = SEARCH_HUB.EMPLOYEE_CD ")
            'sbSQL.AppendLine("AND SEARCH_HUB.HUB_LOC_L_CD = SEARCH_HUB.EMPLOYEE_CD ")
            sbSQL.AppendLine("LEFT JOIN ( ")
            sbSQL.AppendLine("      SELECT DISTINCT HUB.EMPLOYEE_CD ")
            sbSQL.AppendLine("      FROM M_EMPLOYEE_HUB HUB ")
            sbSQL.AppendLine("      LEFT JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
            sbSQL.AppendLine("      ON HUB.HUB_LOC_L_CD = ULOC.LOC_L_CD ")
            sbSQL.AppendLine("      AND ((ULOC.LOC_M_FLG='" & AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = HUB.HUB_LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & AUTH_LOC_M_FLG.No & "')) ")
            sbSQL.AppendLine("      AND ULOC.UPD_FLG='" & AUTH_LOC_M_FLG.Yes & "' ")
            sbSQL.AppendLine("      WHERE ULOC.LOC_L_CD IS NULL  ")
            sbSQL.AppendLine(") UPD ")
            sbSQL.AppendLine("ON EMP.EMPLOYEE_CD = UPD.EMPLOYEE_CD ")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  EMP.AUTH_DIV <> '" & Misc.AUTH_DIV.SuperAdmin & "' ")

            '一覧画面表示時
            If isAll = False Then
                '検索条件を設定する
                'コード
                If Not IsNothing(_ViewModel.SearchCode) AndAlso _ViewModel.SearchCode <> "" Then
                    sbSQL.AppendLine("  AND EMP.EMPLOYEE_CD Like @searchEmployeeCd")
                    _params.Add(New SqlParameter("@searchEmployeeCd", $"{_ViewModel.SearchCode}%"))
                End If

                '氏名
                If Not IsNothing(_ViewModel.SearchName) AndAlso _ViewModel.SearchName <> "" Then
                    sbSQL.AppendLine("  AND EMP.EMPLOYEE_NAME Like @name")
                    _params.Add(New SqlParameter("@name", $"%{_ViewModel.SearchName}%"))
                End If

                '部署
                If Not IsNothing(_ViewModel.SearchBusho) AndAlso _ViewModel.SearchBusho <> "" Then
                    sbSQL.AppendLine("  AND EMP.DEPARTMENT_CD = @busho")
                    _params.Add(New SqlParameter("@busho", $"{_ViewModel.SearchBusho}"))
                End If

                '所属拠点
                If Not String.IsNullOrEmpty(_ViewModel.SearchHubLocLCd) Then
                    sbSQL.AppendLine("  and exists")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine(" select 1 from M_EMPLOYEE_HUB")
                    sbSQL.AppendLine(" WHERE M_EMPLOYEE_HUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
                    sbSQL.AppendLine(" AND   M_EMPLOYEE_HUB.HUB_LOC_L_CD = @HubLocLCd")
                    If Not String.IsNullOrEmpty(_ViewModel.SearchHubLocMCd) Then
                        sbSQL.AppendLine("  AND M_EMPLOYEE_HUB.HUB_LOC_M_CD = @HubLocMCd")
                    End If
                    sbSQL.AppendLine(")")
                    _params.Add(New SqlParameter("@HubLocLCd", $"{_ViewModel.SearchHubLocLCd}"))
                    If Not String.IsNullOrEmpty(_ViewModel.SearchHubLocMCd) Then
                        _params.Add(New SqlParameter("@HubLocMCd", $"{_ViewModel.SearchHubLocMCd}"))
                    End If
                End If

                '退職者
                If _ViewModel.SearchRetire = False Then
                    '退職者を含むにチェックされていない場合、就業中の人のデータのみ取得
                    sbSQL.AppendLine("  AND EMP.RETIRE_FLG = @retireflg")
                    _params.Add(New SqlParameter("@retireflg", $"{RetireFlg.Cd.No}"))
                End If
                '--2021/09/07 nv miyaji add end

                '権限
                '--2021/09/07 nv miyaji update start
                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '通常権限
                If Not IsNothing(_ViewModel.SearchKengen_KyotenN) AndAlso _ViewModel.SearchKengen_KyotenN <> "" Then
                    sbSQL.AppendLine("  AND EMP.AUTH_DIV = @kengen")
                    _params.Add(New SqlParameter("@kengen", $"{_ViewModel.SearchKengen_KyotenN}"))
                End If
            End If

            '--2021/09/28 nv miyaji add start
            '自拠点権限の場合
            If ViewBag.AuthProcFlg = AuthProcFlg.JiAll Then
                sbSQL.AppendLine("  and exists")
                sbSQL.AppendLine("(")
                sbSQL.AppendLine(" select 1 from M_EMPLOYEE_HUB HUB")
                sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.AppendLine("ON HUB.HUB_LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.AppendLine(" WHERE HUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
                sbSQL.AppendLine(")")
            ElseIf ViewBag.AuthProcFlg = AuthProcFlg.JiShokaiNomi Then
                sbSQL.AppendLine("  and exists")
                sbSQL.AppendLine("(")
                sbSQL.AppendLine(" select 1 from M_EMPLOYEE_HUB HUB")
                sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.AppendLine("ON HUB.HUB_LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.AppendLine("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = HUB.HUB_LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
                sbSQL.AppendLine(" WHERE HUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
                sbSQL.AppendLine(")")
            End If

            _params.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
            _params.Add(New SqlParameter("@ReadAuthFlg", _AuthInfo.ReadAuthFlg))
            _params.Add(New SqlParameter("@UpdAuthFlg", _AuthInfo.UpdAuthFlg))
            _params.Add(New SqlParameter("@ReadHubAuthFlg", _AuthInfo.ReadHubAuthFlg))
            _params.Add(New SqlParameter("@UpdHubAuthFlg", _AuthInfo.UpdHubAuthFlg))
            _params.Add(New SqlParameter("@UpdAuthOption", _AuthInfo.AuthOption))

            '--2021/09/28 nv miyaji add end

            sbSQL.AppendLine(" ORDER BY EMP.EMPLOYEE_CD")


            Return sbSQL.ToString()
        End Function
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
                _viewResult.Add(New EmployeeViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchCode = _ViewModel.SearchCode
            _viewResult.ElementAt(0).SearchName = _ViewModel.SearchName
            _viewResult.ElementAt(0).SearchBusho = _ViewModel.SearchBusho
            If _ViewModel.SearchKengen_KyotenN Is Nothing Then
                _viewResult.ElementAt(0).SearchKengen_KyotenN = String.Empty
            Else
                _viewResult.ElementAt(0).SearchKengen_KyotenN = _ViewModel.SearchKengen_KyotenN
            End If
            '--2021/09/07 nv miyaji add start
            _viewResult.ElementAt(0).SearchHubLocLCd = _ViewModel.SearchHubLocLCd
            _viewResult.ElementAt(0).SearchHubLocMCd = _ViewModel.SearchHubLocMCd
            _viewResult.ElementAt(0).SearchRetire = _ViewModel.SearchRetire
            If _ViewModel.SearchKengen_KyotenY Is Nothing Then
                _viewResult.ElementAt(0).SearchKengen_KyotenY = String.Empty
            Else
                _viewResult.ElementAt(0).SearchKengen_KyotenY = _ViewModel.SearchKengen_KyotenY
            End If
            '--2021/09/07 nv miyaji add end
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.EMPLOYEE, IIf(String.IsNullOrEmpty(_ViewModel.SearchHubLocLCd), COLSET_LOC.TEMPLATE, _ViewModel.SearchHubLocLCd))
            If _viewResult.ElementAt(0).GridInfo.Count > 0 Then
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.EMPLOYEE, IIf(String.IsNullOrEmpty(_ViewModel.SearchHubLocLCd), COLSET_LOC.TEMPLATE, _ViewModel.SearchHubLocLCd)).FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.EMPLOYEE, COLSET_LOC.TEMPLATE).FIX_COL_CNT
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.EMPLOYEE, COLSET_LOC.TEMPLATE)
            End If

            _viewResult.ElementAt(0).IsRestoreOwnSearchConditions = _ViewModel.IsRestoreOwnSearchConditions
            _viewResult.ElementAt(0).FirstSearch = _ViewModel.FirstSearch
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As EmployeeViewModel)
            _Model.EMPLOYEE_CD = model.EMPLOYEE_CD
            _Model.EMPLOYEE_NAME = model.EMPLOYEE_NAME
            _Model.DEPARTMENT_CD = model.DEPARTMENT_CD
            _Model.PASSWORD = model.PASSWORD
            _Model.AUTH_DIV = model.AUTH_DIV
            If model.DispRetireFlg Then    '2021/09/02 nv miyaji add
                _Model.RETIRE_FLG = RetireFlg.Cd.Yes
            Else
                _Model.RETIRE_FLG = RetireFlg.Cd.No
            End If
            _Model.MAIL_ADDRESS = model.MAIL_ADDRESS    '2021/09/02 nv miyaji add
            _Model.INS_DATETIME = model.INS_DATETIME
            _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub

        ''' <summary>
        ''' 更新用のモデルをViewモデルにコピーする
        ''' </summary>
        ''' <remarks>2021/09/02 nv miyaji add</remarks>
        Private Sub CopyModelToViewModel(ViewModel As EmployeeViewModel, ByVal model As EmployeeModel, ByVal tblHub As DataTable)
            ViewModel.EMPLOYEE_CD = model.EMPLOYEE_CD
            ViewModel.EMPLOYEE_NAME = model.EMPLOYEE_NAME
            ViewModel.DEPARTMENT_CD = model.DEPARTMENT_CD
            ViewModel.PASSWORD = model.PASSWORD
            ViewModel.AUTH_DIV = model.AUTH_DIV
            ViewModel.RETIRE_FLG = model.RETIRE_FLG
            ViewModel.MAIL_ADDRESS = model.MAIL_ADDRESS
            ViewModel.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            ViewModel.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

            '所属拠点
            For i As Integer = 0 To CommonConst.MAX_HUBLOC_CNT - 1
                If i < tblHub.Rows.Count Then
                    ViewModel.HUB_LOC_L(i) = tblHub.Rows(i).Item("HUB_LOC_L_CD").ToString
                    ViewModel.HUB_LOC_M(i) = tblHub.Rows(i).Item("HUB_LOC_M_CD").ToString
                Else
                    ViewModel.HUB_LOC_L(i) = String.Empty
                    ViewModel.HUB_LOC_M(i) = String.Empty
                End If
                GetLocationHubMData(ViewModel.HUB_LOC_L(i), i)
            Next

            'ロケーションリストをセットする
            ViewModel.hubLocLDropdownDatas = _ViewModel.hubLocLDropdownDatas
            ViewModel.locLOptions = _ViewModel.locLOptions
            ViewModel.hubLocMDropdownDatas = _ViewModel.hubLocMDropdownDatas
            ViewModel.locMOptions = _ViewModel.locMOptions

            ViewModel.INS_DATETIME = model.INS_DATETIME
            ViewModel.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            ViewModel.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            ViewModel.UPD_DATETIME = model.UPD_DATETIME
            ViewModel.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            ViewModel.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            ViewModel.UPD_TASK = model.UPD_TASK
        End Sub
#End Region

    End Class
End Namespace