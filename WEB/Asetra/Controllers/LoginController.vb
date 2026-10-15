Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Threading

Namespace Controllers
    ''' <summary>
    ''' ログイン画面表示用のクラス
    ''' </summary>
    Public Class LoginController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"
        ''' <summary>
        ''' ライセンスチェック結果
        ''' </summary>
        Private Enum licenseCheckResult
            OK = 0
            NG = -1
        End Enum
#End Region

#Region "クラス変数"
        'Private _Model As EmployeeModel = New EmployeeModel                                'Employeeモデル
        Private _ViewModel As LoginViewModel = New LoginViewModel                           'ログイン用表示用モデル
        Protected _viewResult As List(Of LoginViewModel) = New List(Of LoginViewModel)      '結果View
        Private _NewsDownload As New DataTable                                              'お知らせダウンロード
        Private _Message As MessageRfid = Nothing
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
        Function Index(ByVal ViewModel As LoginViewModel, ByVal actionname As String) As ActionResult

            '--2020/04/20 nv miyaji update start
            'ライセンスセッションが切れていたらエラーコードを設定し、一覧表示しない
            If IsNothing(Session(CommonSessionKey.REGIST_OK)) Then
                If CheckLicense() = False Then
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_LICENSE_SESSION
                    _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN

                    'バージョンの取得
                    ViewBag.Version = Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()

                    '2023/03/22 Kawamoto ADD START
                    'お知らせダウンロード情報の取得
                    _NewsDownload = Dao.ToolNewsDownload.GetNewsDownloadDataAll()
                    '2023/03/22 Kawamoto ADD END

                    '一覧情報にページ情報を設定
                    CopyModelData()

                    Return View(_viewResult)
                End If
            End If

            'ログアウトボタンを押されての遷移の場合、ユーザーセッションを初期化する
            If actionname = "Logout" Then
                'ログインユーザーのセッションを初期化する
                Session.Remove(CommonSessionKey.CREAGE_LOGIN)
            End If

            'ログイン画面からのリダイレクト以外、社員セッションを初期化する
            If actionname <> CommonConst.KEY_FROM_LOGIN Then
                '社員情報のセッションを初期化する
                Session.Remove(CommonSessionKey.CREAGE_EMPLOYEE_INFO)
                'カゴ情報のセッションを初期化する       '--2021/11/17 add nv iwamoto
                Session.Remove(CommonSessionKey.CREAGE_CART_INFO)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As LoginViewModel = Session(CommonSessionKey.CREAGE_EMPLOYEE_INFO)
            If Not IsNothing(model) Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'If IsNothing(Session(CommonSessionKey.REGIST_OK)) Then
            '        _ViewModel.ErrCode = CommonLabel.MSG_ERR_LICENSE_SESSION
            '        _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            'Else
            '    'ログアウトボタンを押されての遷移の場合、ユーザーセッションを初期化する
            '    If actionname = "Logout" Then
            '        'ログインユーザーのセッションを初期化する
            '        Session.Remove(CommonSessionKey.CREAGE_LOGIN)
            '    End If

            '    'ログイン画面からのリダイレクト以外、社員セッションを初期化する
            '    If actionname <> CommonConst.KEY_FROM_LOGIN Then
            '        '社員情報のセッションを初期化する
            '        Session.Remove(CommonSessionKey.CREAGE_EMPLOYEE_INFO)
            '    End If

            '    'モデルを設定(セッションにある場合はセッションから設定)
            '    Dim model As LoginViewModel = Session(CommonSessionKey.CREAGE_EMPLOYEE_INFO)
            '    If Not IsNothing(model) Then
            '        _ViewModel = model
            '    Else
            '        _ViewModel = ViewModel
            '    End If

            '    'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            '    Dim LoginInfo As RfidEntity.LoginUser = Nothing
            '    LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            '    If Not IsNothing(LoginInfo) Then
            '        '画面右上に社員名を表示
            '        ViewBag.LoginUserNm = LoginInfo.EmployeeNm
            '        '権限取得
            '        ViewBag.LoginKengenKbn = LoginInfo.KengenKbn
            '    End If
            'End If
            '--2020/04/20 nv miyaji update end

            'バージョンの取得
            ViewBag.Version = Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()

            '2023/03/22 Kawamoto ADD START
            'お知らせダウンロード情報の取得
            _NewsDownload = Dao.ToolNewsDownload.GetNewsDownloadDataAll()
            '2023/03/22 Kawamoto ADD END

            '一覧情報にページ情報を設定
            CopyModelData()

            Return View(_viewResult)
        End Function
#End Region

#Region "ログイン処理"
        ''' <summary>
        ''' アクションメソッド(Login)
        ''' 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        ''' 詳細については、http://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function Login(ByVal ViewModel As LoginViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'ライセンスセッションが切れていたらライセンスチェック
            If IsNothing(Session(CommonSessionKey.REGIST_OK)) Then
                '--2020/04/20 nv miyaji update start
                If CheckLicense() = False Then
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_LICENSE_SESSION
                    _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN

                    '結果表示
                    Return RedirectToAction("Index")
                End If
                '--2020/04/20 nv miyaji update end
            End If

            'ログインユーザーのセッションを初期化する
            Session.Remove(CommonSessionKey.CREAGE_LOGIN)

            '資産一覧画面リダイレクト情報を初期化する
            Session.Remove(RedirectAssetListInfo.ID)

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)


            ''メッセージ一覧取得
            'Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '--2019/09/05 upd start nv iwamoto
            'Dim db As New SystemDBContext
            ''IDをキーにモデルを取得する
            '_Model = db.Employee.Find(_ViewModel.EMPLOYEE_CD)

            ''該当の社員データがない場合エラー
            'If IsNothing(_Model) Then
            '    _ViewModel.ErrCode = CommonLabel.MSG_ERR_EMPLOYEECD
            '    _ViewModel.ErrTarget = "EMPLOYEE_CD"

            '    'モデルの内容をセッションに保持
            '    Session(CommonSessionKey.CREAGE_EMPLOYEE_INFO) = _ViewModel

            '    '結果表示
            '    Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_FROM_LOGIN})
            'Else
            '    'パスワードチェック
            '    If ChekPassword() Then
            '        If Not _ViewModel.NewPassword = String.Empty Then
            '            'パスワード登録処理
            '            UpdatePassword()
            '        End If
            '    Else
            '        'パスワードエラー

            '        'モデルの内容をセッションに保持
            '        Session(CommonSessionKey.CREAGE_EMPLOYEE_INFO) = _ViewModel

            '        '結果表示
            '        Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_FROM_LOGIN})
            '    End If

            '    '該当データがある場合セッションにログインユーザ情報を保存します。
            '    Dim User As RfidEntity.LoginUser = Nothing
            '    User = CreateUser()
            '    Session(CommonSessionKey.CREAGE_LOGIN) = User
            '    ViewBag.LoginUserNm = User.EmployeeNm

            '    'TODO:保存期限切れの操作イベントログ削除処理をここに書く

            '    '結果表示
            '    Return RedirectToAction("Index", "AssetList")
            'End If

            '社員マスタデータ取得
            Dim tbl As DataTable = Dao.ToolEmployee.GetEmployeeData(_ViewModel.EMPLOYEE_CD)

            '該当の社員データがない場合エラー
            If tbl.Rows.Count = 0 Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_EMPLOYEECD
                _ViewModel.ErrTarget = "EMPLOYEE_CD"

                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_EMPLOYEE_INFO) = _ViewModel

                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_FROM_LOGIN})
            End If

            '退職者の場合はエラー     2021/09/28 nv iwamoto add
            If tbl.Rows(0).Item("RETIRE_FLG").ToString = ValidFlg.Cd.Valid Then
                _ViewModel.ErrCode = CommonLabelLogin.MSG_ERR_RETIRE_EMPLOYEE
                _ViewModel.ErrTarget = "EMPLOYEE_CD"

                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_EMPLOYEE_INFO) = _ViewModel

                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_FROM_LOGIN})
            End If

            'パスワードチェック
            If ChekPassword(tbl.Rows(0)) = False Then
                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_EMPLOYEE_INFO) = _ViewModel

                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_FROM_LOGIN})
            End If

            'パスワード登録処理
            If Not _ViewModel.NewPassword = String.Empty Then
                UpdatePassword()
            End If

            '該当データがある場合セッションにログインユーザ情報を保存します。
            Dim User As RfidEntity.LoginUser = Nothing
            User = CreateUser(tbl.Rows(0))
            Session(CommonSessionKey.CREAGE_LOGIN) = User
            ViewBag.LoginUserNm = User.EmployeeNm

            '期間削除処理
            Dim dtDelExecDate As Date = Now.Date
            If (Not CommonRfid.GdtDeletedDate.HasValue) OrElse CommonRfid.GdtDeletedDate < dtDelExecDate Then
                Call DeleteProc(dtDelExecDate)
                CommonRfid.GdtDeletedDate = dtDelExecDate
            End If

            '--2021/10/05 sv suzuki upd start
            '予約機能取得
            Dim Setting As MessageRfid      '設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim settingInfo As SettingInfo = Setting.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3)
            If settingInfo Is Nothing Then
                '結果表示
                Return RedirectToAction("Init", "AssetList")
            Else
                Dim sValidFlg As String = settingInfo.DATA1.Trim
                If sValidFlg <> ValidFlg.Cd.Valid Then
                    '結果表示
                    Return RedirectToAction("Init", "AssetList")
                Else
                    '結果表示
                    Return RedirectToAction("Index", "LendingStatus")
                End If
            End If
            ''結果表示
            'Return RedirectToAction("Init", "AssetList")   '2020/02/12 nv miyaji
            'Return RedirectToAction("Index", "AssetList")   '2020/02/12 nv miyaji
            '--2019/09/05 upd end nv iwamoto
            '--2021/10/05 sv suzuki upd end

        End Function
#End Region

#Region "ユーザーオブジェクトを作成"
        '''' <summary>
        '''' ユーザーオブジェクトを作成
        '''' </summary>
        '''' <remarks>
        '''' </remarks>
        'Private Function CreateUser() As RfidEntity.LoginUser
        '    Dim db As New SystemDBContext

        '    '該当データがある場合、ユーザーオブジェクトを作成します。
        '    Dim user As New RfidEntity.LoginUser
        '    '社員コード
        '    user.EmployeeCd = String.Empty
        '    If Not IsDBNull(_Model.EMPLOYEE_CD) Then
        '        user.EmployeeCd = _Model.EMPLOYEE_CD
        '    End If
        '    '社員名
        '    user.EmployeeNm = String.Empty
        '    If Not IsDBNull(_Model.EMPLOYEE_NAME) Then
        '        user.EmployeeNm = _Model.EMPLOYEE_NAME
        '    End If
        '    '部門コード・部門名
        '    Dim DepartmentModel As DepartmentModel = Nothing
        '    user.DepartmentCd = String.Empty
        '    user.DepartmentNm = String.Empty
        '    If Not String.IsNullOrEmpty(_Model.DEPARTMENT_CD) Then
        '        '部門コード
        '        user.DepartmentCd = _Model.DEPARTMENT_CD
        '        'IDをキーにモデルを取得する
        '        DepartmentModel = db.Department.Find(_Model.DEPARTMENT_CD)
        '        '部門名
        '        user.DepartmentNm = DepartmentModel.DEPARTMENT_NAME
        '    End If
        '    '権限区分
        '    user.KengenKbn = Misc.AUTH_DIV.Nomal
        '    If Not IsDBNull(_Model.AUTH_DIV) Then
        '        user.KengenKbn = _Model.AUTH_DIV
        '    End If

        '    Return user

        'End Function
        ''' <summary>
        ''' ユーザーオブジェクトを作成
        ''' </summary>
        Private Function CreateUser(row As DataRow) As RfidEntity.LoginUser
            Dim db As New SystemDBContext

            '該当データがある場合、ユーザーオブジェクトを作成します。
            Dim user As New RfidEntity.LoginUser
            '社員コード
            user.EmployeeCd = String.Empty
            If Not IsDBNull(row("EMPLOYEE_CD")) Then
                user.EmployeeCd = row("EMPLOYEE_CD").ToString
            End If
            '社員名
            user.EmployeeNm = String.Empty
            If Not IsDBNull(row("EMPLOYEE_NAME")) Then
                user.EmployeeNm = row("EMPLOYEE_NAME").ToString
            End If
            '部門コード・部門名
            Dim DepartmentModel As DepartmentModel = Nothing
            user.DepartmentCd = String.Empty
            user.DepartmentNm = String.Empty
            user.HeadOfficeFlg = Misc.HeadOfficeFlg.Cd.Kyoten       '初期値は拠点権限にしておく

            'If Not String.IsNullOrEmpty(row("DEPARTMENT_CD")) Then
            If (Not IsDBNull(row("DEPARTMENT_CD").ToString)) AndAlso (Not String.IsNullOrEmpty(row("DEPARTMENT_CD").ToString)) Then
                '部門コード
                user.DepartmentCd = row("DEPARTMENT_CD").ToString
                'IDをキーにモデルを取得する
                DepartmentModel = db.Department.Find(row("DEPARTMENT_CD").ToString)
                '部門名
                If DepartmentModel IsNot Nothing Then
                    user.DepartmentNm = DepartmentModel.DEPARTMENT_NAME
                    user.HeadOfficeFlg = DepartmentModel.HEAD_OFFICE_FLG
                End If
            End If
            '権限区分
            user.KengenKbn = Misc.AUTH_DIV.Nomal
            If Not IsDBNull(row("AUTH_DIV")) Then
                user.KengenKbn = row("AUTH_DIV")
            End If

            '権限タイプ
            Dim Setting As SettingInfo = CommonRfid.MessageRfid.getSettingInfo(CommonSettingKey.Authority_Type.KeyDiv, CommonSettingKey.Authority_Type.Key1, CommonSettingKey.Authority_Type.Key2, CommonSettingKey.Authority_Type.Key3)
            If IsNothing(Setting) OrElse String.IsNullOrEmpty(Setting.DATA1) OrElse Setting.DATA1 <> Misc.Authority_Type.Cd.Kyoten Then
                user.AuthType = Misc.Authority_Type.Cd.Nomal
                user.HeadOfficeFlg = Misc.HeadOfficeFlg.Cd.Honbu       '拠点権限：利用しない場合は、本部扱い
            Else
                user.AuthType = Misc.Authority_Type.Cd.Kyoten
            End If
            '詳細権限区分（WEB内使用）
            If user.AuthType = Misc.Authority_Type.Cd.Kyoten Then
                '拠点権限：利用する場合
                If user.HeadOfficeFlg = Misc.HeadOfficeFlg.Cd.Honbu Then
                    '本部の場合
                    Select Case user.KengenKbn
                        Case Misc.AUTH_DIV.SuperAdmin
                            user.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.SuperAdmin
                        Case Misc.AUTH_DIV.Admin
                            user.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Honbu_Admin
                        Case Misc.AUTH_DIV.Nomal
                            user.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Honbu_Nomal
                    End Select
                Else
                    '拠点の場合
                    Select Case user.KengenKbn
                        Case Misc.AUTH_DIV.SuperAdmin
                            user.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.SuperAdmin
                            user.HeadOfficeFlg = Misc.HeadOfficeFlg.Cd.Honbu            'ここに来ることはあり得ないはずだが、来たら、本部にする
                        Case Misc.AUTH_DIV.Admin
                            user.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Kyoten_Admin
                        Case Misc.AUTH_DIV.Nomal
                            user.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Kyoten_Nomal
                    End Select
                End If
            Else
                '拠点権限：利用しない場合
                user.HeadOfficeFlg = Misc.HeadOfficeFlg.Cd.Honbu                'この場合は、みんな本部扱い
                Select Case user.KengenKbn
                    Case Misc.AUTH_DIV.SuperAdmin
                        user.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.SuperAdmin
                    Case Misc.AUTH_DIV.Admin
                        user.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Tsujo_Admin
                    Case Misc.AUTH_DIV.Nomal
                        user.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Tsujo_Nomal
                End Select
            End If

            Return user

        End Function
#End Region

#Region "パスワードチェック処理"
        '''' <summary>
        '''' パスワードチェック処理
        '''' </summary>
        '''' <remarks>
        '''' </remarks>
        'Private Function ChekPassword() As Boolean
        '    Dim bRet As Boolean = False

        '    If String.IsNullOrEmpty(_Model.PASSWORD) Then
        '        'データベースのパスワード設定無しの場合
        '        If _ViewModel.NewPassword = String.Empty Then
        '            '新パスワードが入力されていない
        '            _ViewModel.ErrCode = CommonLabel.MSG_ERR_INPUT_NEWPASSWORD
        '            _ViewModel.ErrTarget = "NewPassword"
        '            Return bRet
        '        ElseIf _ViewModel.KakuninPassword = String.Empty Then
        '            '確認パスワードが入力されていない
        '            _ViewModel.ErrCode = CommonLabel.MSG_ERR_INPUT_KAKUNINPASSWORD
        '            _ViewModel.ErrTarget = "KakuninPassword"
        '            Return bRet
        '        ElseIf Not _ViewModel.NewPassword = _ViewModel.KakuninPassword Then
        '            '確認パスワードと新パスワードが一致しない
        '            _ViewModel.ErrCode = CommonLabel.MSG_ERR_NEWPASSWORD
        '            _ViewModel.ErrTarget = "KakuninPassword"
        '            Return bRet
        '        End If
        '    Else
        '        'データベースのパスワード設定有りの場合
        '        If Not _ViewModel.PASSWORD = _Model.PASSWORD Then
        '            'パスワードが一致しない
        '            _ViewModel.ErrCode = CommonLabel.MSG_ERR_PASSWORD
        '            _ViewModel.ErrTarget = "PASSWORD"
        '            Return bRet
        '        Else
        '            If Not _ViewModel.NewPassword = String.Empty Then
        '                '新パスワードが入力されている場合
        '                If Not _ViewModel.KakuninPassword = _ViewModel.NewPassword Then
        '                    '確認パスワードと新パスワードが一致しない
        '                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_NEWPASSWORD
        '                    _ViewModel.ErrTarget = "KakuninPassword"
        '                    Return bRet
        '                End If
        '            End If
        '        End If
        '    End If

        '    bRet = True

        '    Return bRet

        'End Function
        ''' <summary>
        ''' パスワードチェック処理
        ''' </summary>
        Private Function ChekPassword(row As DataRow) As Boolean
            Dim bRet As Boolean = False

            'If String.IsNullOrEmpty(row("PASSWORD").ToString) Then
            If (IsDBNull(row("PASSWORD").ToString)) OrElse (String.IsNullOrEmpty(row("PASSWORD").ToString)) Then
                'データベースのパスワード設定無しの場合
                If _ViewModel.NewPassword = String.Empty Then
                    '新パスワードが入力されていない
                    _ViewModel.OpenFlg = _Message.getMessage(CommonLabel.LABEL_LOGINOPTION_CLOSE)
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_INPUT_NEWPASSWORD
                    _ViewModel.ErrTarget = "NewPassword"
                    Return bRet
                ElseIf _ViewModel.KakuninPassword = String.Empty Then
                    '確認パスワードが入力されていない
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_INPUT_KAKUNINPASSWORD
                    _ViewModel.ErrTarget = "KakuninPassword"
                    Return bRet
                ElseIf Not _ViewModel.NewPassword = _ViewModel.KakuninPassword Then
                    '確認パスワードと新パスワードが一致しない
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_NEWPASSWORD
                    _ViewModel.ErrTarget = "KakuninPassword"
                    Return bRet
                End If
            Else
                'データベースのパスワード設定有りの場合
                If Not _ViewModel.PASSWORD = row("PASSWORD").ToString Then
                    'パスワードが一致しない
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_PASSWORD
                    _ViewModel.ErrTarget = "PASSWORD"
                    Return bRet
                Else
                    If Not _ViewModel.KakuninPassword = _ViewModel.NewPassword Then
                        '確認パスワードと新パスワードが一致しない
                        If _ViewModel.NewPassword = String.Empty Then
                            '新パスワードが空白
                            _ViewModel.ErrCode = CommonLabel.MSG_ERR_NEWPASSWORD
                            _ViewModel.ErrTarget = "NewPassword"
                            Return bRet
                        Else
                            '新パスワードが空白以外
                            _ViewModel.ErrCode = CommonLabel.MSG_ERR_NEWPASSWORD
                            _ViewModel.ErrTarget = "KakuninPassword"
                            Return bRet
                        End If
                    End If
                End If
            End If

            bRet = True

            Return bRet

        End Function
#End Region

#Region "パスワード登録処理"
        ''' <summary>
        ''' パスワード登録処理
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        Private Sub UpdatePassword()

            Dim db As New SystemDBContext

            ''登録用モデルに新パスワードを設定
            '_Model.PASSWORD = _ViewModel.NewPassword

            ''アタッチする
            'db.Entry(_Model).State = Entity.EntityState.Modified

            ''登録処理を実行
            'db.SaveChanges()

            '新パスワードを設定
            Dim sql As New StringBuilder
            sql.AppendLine("UPDATE M_EMPLOYEE SET")
            sql.AppendLine("PASSWORD = '" & _ViewModel.NewPassword & "'")
            sql.AppendLine("WHERE EMPLOYEE_CD = '" & _ViewModel.EMPLOYEE_CD & "'")
            db.Database.ExecuteSqlCommand(sql.ToString)

        End Sub
#End Region


#Region "期間削除処理"
        ''' <summary>
        ''' 期間削除処理
        ''' </summary>
        ''' <param name="dtExecDate">実行日（基本的には、システム日をセットする）</param>
        Public Sub DeleteProc(dtExecDate As Date)

            Try
                Dim sFullPath As String
                Dim dtDelDate As Date?

                '一時ファイルの削除(QR画像保存フォルダ)
                sFullPath = Server.MapPath(CommonConst.QRIMAGE_FOLDERNM)
                CommonRfid.DeleteFiles(dtExecDate.AddDays(-1), sFullPath)

                '一時ファイルの削除(イメージ画像一時保存フォルダ)
                sFullPath = Server.MapPath(CommonConst.ROOT_FOLDERNM & CommonConst.IMAGETMP_FOLDERNM)
                CommonRfid.DeleteFiles(dtExecDate.AddDays(-1), sFullPath)

                '一時ファイルの削除(ログフォルダ)
                dtDelDate = GetDelDate(dtExecDate, CommonSettingKey.DeleteKikan.Key1.T_OPERATION_LOG)
                If dtDelDate.HasValue Then
                    sFullPath = Server.MapPath(CommonConst.LOG_FOLDERNM)
                    CommonRfid.DeleteFiles(dtExecDate.AddDays(-1), sFullPath)
                End If

                Dim sql As New StringBuilder
                Dim db As New SystemDBContext
                Using db
                    '操作ログテーブル
                    dtDelDate = GetDelDate(dtExecDate, CommonSettingKey.DeleteKikan.Key1.T_OPERATION_LOG)
                    If dtDelDate.HasValue Then
                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_OPERATION_LOG")
                        sql.AppendLine("WHERE PROCESS_DT < " & dtDelDate.Value.ToString("yyyyMMdd"))
                        db.Database.ExecuteSqlCommand(sql.ToString)
                    End If

                    '資産処理履歴テーブル
                    dtDelDate = GetDelDate(dtExecDate, CommonSettingKey.DeleteKikan.Key1.T_PROCESS_LOG)
                    If dtDelDate.HasValue Then
                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_PROCESS_LOG")
                        sql.AppendLine("WHERE PROCESS_DT < " & dtDelDate.Value.ToString("yyyyMMdd"))
                        db.Database.ExecuteSqlCommand(sql.ToString)
                    End If

                    'ゲート通過履歴    '2020/03/06 nv miyaji add
                    dtDelDate = GetDelDate(dtExecDate, CommonSettingKey.DeleteKikan.Key1.T_GATE_PASS_LOG)
                    If dtDelDate.HasValue Then
                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_GATE_PASS_LOG")
                        sql.AppendLine("WHERE GATE_PASS_DT < " & dtDelDate.Value.ToString("yyyyMMdd"))
                        db.Database.ExecuteSqlCommand(sql.ToString)
                    End If

                    '棚卸管理テーブル・過去棚卸実績テーブル
                    dtDelDate = GetDelDate(dtExecDate, CommonSettingKey.DeleteKikan.Key1.T_INV_RESULT)
                    If dtDelDate.HasValue Then
                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_INVENTORY")
                        sql.AppendLine("WHERE START_DEC_TS < " & dtDelDate.Value.ToString("yyyyMMdd") & "000000")
                        sql.AppendLine(" AND ISNULL(INV_CONFIRM_TS,'')<>'' ")
                        db.Database.ExecuteSqlCommand(sql.ToString)

                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_INV_DETAIL")
                        sql.AppendLine("WHERE NOT EXISTS(SELECT 1 FROM T_INVENTORY WHERE T_INVENTORY.INV_ID = T_INV_DETAIL.INV_ID)")
                        db.Database.ExecuteSqlCommand(sql.ToString)

                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_INV_RESULT")
                        sql.AppendLine("WHERE NOT EXISTS(SELECT 1 FROM T_INVENTORY WHERE T_INVENTORY.INV_ID = T_INV_RESULT.INV_ID)")
                        db.Database.ExecuteSqlCommand(sql.ToString)

                    End If

                    '資産マスタ
                    dtDelDate = GetDelDate(dtExecDate, CommonSettingKey.DeleteKikan.Key1.ASSET_JOKYKU)
                    If dtDelDate.HasValue Then
                        sql.Clear()
                        sql.AppendLine("DELETE FROM M_ASSET")
                        sql.AppendLine("WHERE ASSET_STATUS = '" & ASSET_STATUS.Removed & "'")
                        sql.AppendLine("AND   RETIRE_TS < " & dtDelDate.Value.ToString("yyyyMMdd") & "000000")
                        db.Database.ExecuteSqlCommand(sql.ToString)
                    End If

                    'new 予約明細,予約メンテナンス,予約履歴テーブル 211208追加
                    dtDelDate = GetDelDate(dtExecDate, CommonSettingKey.DeleteKikan.Key1.T_RESERVATION_STAMP)

                    '予約T.予約IDより 予約明細T,予約メンテT,予約Tを削除
                    If dtDelDate.HasValue Then
                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_RESERVE_DETAIL")
                        sql.AppendLine("WHERE RESERVE_ID  in( ")
                        sql.AppendLine("SELECT RESERVE_ID FROM T_RESERVE ")
                        sql.AppendLine("WHERE REAL_END_DT < " & dtDelDate.Value.ToString("yyyyMMdd") & ")")
                        db.Database.ExecuteSqlCommand(sql.ToString)

                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_RESERVE_MAINTE")
                        sql.AppendLine("WHERE RESERVE_ID  in( ")
                        sql.AppendLine("SELECT RESERVE_ID FROM T_RESERVE ")
                        sql.AppendLine("WHERE REAL_END_DT < " & dtDelDate.Value.ToString("yyyyMMdd") & ")")
                        db.Database.ExecuteSqlCommand(sql.ToString)

                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_RESERVE")
                        sql.AppendLine("WHERE RESERVE_ID  in( ")
                        sql.AppendLine("SELECT RESERVE_ID FROM T_RESERVE ")
                        sql.AppendLine("WHERE REAL_END_DT < " & dtDelDate.Value.ToString("yyyyMMdd") & ")")
                        db.Database.ExecuteSqlCommand(sql.ToString)


                        '予約履歴Tと予約メンテTを日時に基づいて削除。
                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_RESERVE_MAINTE")
                        sql.AppendLine("WHERE MAINTE_END_DT < " & dtDelDate.Value.ToString("yyyyMMdd"))
                        db.Database.ExecuteSqlCommand(sql.ToString)

                        sql.Clear()
                        sql.AppendLine("DELETE FROM T_RESERVE_LOG")
                        sql.AppendLine("WHERE PROCESS_DT < " & dtDelDate.Value.ToString("yyyyMMdd"))
                        db.Database.ExecuteSqlCommand(sql.ToString)

                    End If


                End Using

            Catch ex As Exception
                'データ削除の Exception はログには出力するが、エラーにはしない
                logger.Error(ex.Message)

            End Try

        End Sub
#End Region

#Region "設定Mデータより、削除対象日（この日未満を削除）を求める"
        ''' <summary>
        ''' 設定Mデータより、削除対象日（この日未満を削除）を求める
        ''' </summary>
        ''' <param name="dtExecDate">実行日（基本的には、システム日をセットする）</param>
        ''' <param name="sKey1">KEY1(ﾃﾞｰﾀ種類)</param>
        ''' <returns>削除対象日（この日未満を削除） Null時、削除なし</returns>
        Private Function GetDelDate(ByVal dtExecDate As Date, ByVal sKey1 As String) As Date?
            Dim dtDelDate As Date? = Nothing

            Dim Setting As SettingInfo = CommonRfid.MessageRfid.getSettingInfo(CommonSettingKey.DeleteKikan.KeyDiv, sKey1, CommonSettingKey.DeleteKikan.Key2, CommonSettingKey.DeleteKikan.Key3)
            If IsNothing(Setting) Then
                Return dtDelDate
            End If

            If String.IsNullOrEmpty(Setting.DATA1) OrElse String.IsNullOrEmpty(Setting.DATA2) OrElse (Not IsNumeric(Setting.DATA2.Trim)) Then
                Return dtDelDate
            End If

            If CInt(Setting.DATA2.Trim) = 0 Then
                Return dtDelDate
            End If

            Select Case Setting.DATA1
                Case CommonSettingKey.DeleteKikan.DATA1.YY
                    dtDelDate = dtExecDate.AddYears(0 - CInt(Setting.DATA2.Trim))
                Case CommonSettingKey.DeleteKikan.DATA1.MM
                    dtDelDate = dtExecDate.AddMonths(0 - CInt(Setting.DATA2.Trim))
                Case CommonSettingKey.DeleteKikan.DATA1.DD
                    dtDelDate = dtExecDate.AddDays(0 - CInt(Setting.DATA2.Trim))
                Case Else
                    Return dtDelDate
            End Select

            Return dtDelDate

        End Function
#End Region

#Region "ライセンスチェック処理"
        ''' <summary>
        ''' ライセンスチェック処理
        ''' </summary>
        ''' <returns>ライセンスチェック結果　true:認証OK　false:認証NG</returns>
        ''' <remarks>2020/04/20 nv miyaji add</remarks>
        Public Function CheckLicense() As Boolean

            'ASETRAバージョンテーブルデータ取得
            Dim tbl As DataTable = Dao.ToolAsetraVer.GetAsetraVerInfo()

            Dim sNo As String = String.Empty            'プロダクト番号
            Dim sKey As String = String.Empty            'プロダクトキー

            'WebサーバーMACアドレスを元に、プロダクト番号を取得
            sNo = AsetraLibRegist.LibRegist.GetProductNo(tbl.Rows(0).Item("LICENSE_KEY").ToString)

            'logger.Info("●●● ProductNo=[" & sNo & "]")


            'プロダクト番号を元に、プロダクトキーを取得する
            sKey = AsetraLibRegist.LibRegist.GetProductkey(sNo)

            'logger.Info("●●● Productkey=[" & sKey & "]")

            'ライセンスチェック
            Dim iRet As Integer = AsetraLibRegist.LibRegist.CheckLicense(sKey)
            ''●●●　テスト用
            'Dim iRet As Integer = -1
            Select Case iRet
                Case licenseCheckResult.OK
                    '認証OK
                    Session(CommonSessionKey.REGIST_OK) = True

                    Return True
                    'Return RedirectToAction("Index", "Login")
                Case licenseCheckResult.NG
                    Return False
                    'Return RedirectToAction("Index", "EntryProductKey")
                Case Else
                    Return False
                    'Return RedirectToAction("Index", "EntryProductKey")
            End Select

        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New LoginViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).EMPLOYEE_CD = _ViewModel.EMPLOYEE_CD
            _viewResult.ElementAt(0).EMPLOYEE_NAME = _ViewModel.EMPLOYEE_NAME
            _viewResult.ElementAt(0).DEPARTMENT_CD = _ViewModel.DEPARTMENT_CD
            _viewResult.ElementAt(0).PASSWORD = _ViewModel.PASSWORD
            _viewResult.ElementAt(0).NewPassword = _ViewModel.NewPassword
            _viewResult.ElementAt(0).KakuninPassword = _ViewModel.KakuninPassword
            _viewResult.ElementAt(0).AUTH_DIV = _ViewModel.AUTH_DIV
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            '_viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            '_viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton

            '2023/03/22 Kawamoto ADD START
            For i As Integer = 0 To _NewsDownload.Rows.Count - 1
                If i = 0 Then
                    _viewResult.ElementAt(i).NEWS_CD = _NewsDownload.Rows(i).Item("NEWS_CD")
                    _viewResult.ElementAt(i).DISPLAY_NAME = _NewsDownload.Rows(i).Item("DISPLAY_NAME")
                    _viewResult.ElementAt(i).DOWNLOAD_URL = CommonRfid.ToStringNvl(_NewsDownload.Rows(i).Item("DOWNLOAD_URL"))
                Else
                    Dim model As New LoginViewModel()
                    model.NEWS_CD = _NewsDownload.Rows(i).Item("NEWS_CD")
                    model.DISPLAY_NAME = _NewsDownload.Rows(i).Item("DISPLAY_NAME")
                    model.DOWNLOAD_URL = CommonRfid.ToStringNvl(_NewsDownload.Rows(i).Item("DOWNLOAD_URL"))
                    _viewResult.Add(model)
                End If
            Next
            '2023/03/22 Kawamoto ADD END

        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As LoginViewModel)
            '_Model.EMPLOYEE_CD = model.EMPLOYEE_CD
            '_Model.EMPLOYEE_NAME = model.EMPLOYEE_NAME
            '_Model.DEPARTMENT_CD = model.DEPARTMENT_CD
            '_Model.PASSWORD = model.PASSWORD
            '_Model.AUTH_DIV = model.AUTH_DIV
            '_Model.INS_DATETIME = model.INS_DATETIME
            '_Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            '_Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            '_Model.UPD_DATETIME = model.UPD_DATETIME
            '_Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            '_Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            '_Model.UPD_TASK = model.UPD_TASK
        End Sub
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

        Protected Overrides Sub ViewExecQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub ViewQuery()
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