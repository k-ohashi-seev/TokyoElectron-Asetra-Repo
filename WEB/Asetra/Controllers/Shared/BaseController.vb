Imports System.Data.SqlClient
Imports System.Data.Entity
Imports Asetra.Models
Imports Asetra.Misc

Namespace Controllers
    ''' <summary>
    ''' 基本のコントローラクラス
    ''' </summary>
    Public MustInherit Class BaseController
        Inherits System.Web.Mvc.Controller

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス変数"
        Protected _db As SystemDBContext = New SystemDBContext                       'DB接続
        Protected _tran As Common.DbTransaction                                      'トランザクション
        Protected _paramSqlList As List(Of SqlParameter) = New List(Of SqlParameter) 'SQLパラメータ
        Protected _SQL As String = String.Empty                                      'SQL
        Protected _dbSet As DbSet
#End Region

#Region "後処理"
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)

            '2019/09/17 「タイムアウトに達しました。プールから接続を取得する前にタイムアウト期間が過ぎました。」
            '           の件の対応
            If Not IsNothing(_db) Then
                'If _db.Database.Connection.State = ConnectionState.Open Then
                _db.Database.Connection.Close()
                'End If
            End If

            If (disposing) Then
                _db.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

#End Region

#Region "一覧表示処理"
        ''' <summary>
        ''' アクションメソッド(Index)
        ''' 一覧表示についてはViewQueryをオーバーライドして実装します
        ''' </summary>
        ''' <param name="BaseModel"></param>
        Protected Sub BaseIndex(ByVal BaseModel As BaseModel, Optional ByVal isPaging As Boolean = True)

            If BaseModel.PageNum <= 0 Then
                BaseModel.PageNum = 1
            End If

            '初期化処理
            InitializeIndex()

            '一覧表示のクエリを作成(サブクラスで実装)
            ViewQuery()

            'ページ遷移のクエリを作成
            If isPaging Then
                ViewQueryPager(BaseModel.PageNum, BaseModel)
            End If

            'SQLの実行(サブクラスで実装)
            Try             'ADD 2019/10/06 ADD Nicevision Inoue
                'SQLをログに出力
                logger.Info($"SQL: {_SQL}")
                If _paramSqlList IsNot Nothing Then
                    For Each p In _paramSqlList
                        logger.Info($"Param: {p.ParameterName}={p.Value}")
                    Next
                End If

                ViewExecQuery()
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(_SQL)

                Throw
            End Try

            '後処理
            DisposeIndex()

        End Sub

        ''' <summary>
        ''' Indexアクションの初期化処理をします
        ''' </summary>
        Protected Overridable Sub InitializeIndex()

            '初期化処理
            _paramSqlList.Clear()           'SQLパラメータのクリア
            UpdateLoginName()               '2021/10/07 tomatsuri Add

        End Sub

        ''' <summary>
        ''' Indexアクションの後処理をします
        ''' </summary>
        Protected Overridable Sub DisposeIndex()
        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' サブクラスでオーバーライドして実装します
        ''' </summary>
        Protected MustOverride Sub ViewQuery()

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' サブクラスでオーバーライドして実装します
        ''' </summary>
        Protected MustOverride Sub ViewExecQuery()

        ''' <summary>
        ''' ページ遷移のクエリを作成します
        ''' サブクラスでオーバーライドして実装します
        ''' </summary>
        ''' <param name="PageNum"></param>
        ''' <param name="model"></param>
        Protected MustOverride Sub ViewQueryPager(ByVal PageNum As Integer, ByVal model As BaseModel)

#End Region

#Region "登録処理"
        ''' <summary>
        ''' アクションメソッド(Create)
        ''' </summary>
        Protected Sub BaseCreate(ByRef model As BaseModel)

            'エラーでない場合、登録処理を行う
            If ModelState.IsValid Then
                '初期化処理
                InitializeCreate()

                'チェック処理
                If CreateChcek() Then

                    '登録処理のクエリを作成(サブクラスで実装)
                    CreateQuery()

                    'SQLの実行
                    ExecQuery()

                End If
            End If

            'エラー情報を設定
            SetErrInfo(model)

            '後処理
            DisposeCreate()

        End Sub

        ''' <summary>
        ''' Createアクションの初期化処理をします
        ''' </summary>
        Protected Overridable Sub InitializeCreate()

            '初期化処理
            _paramSqlList.Clear()

        End Sub

        ''' <summary>
        ''' Createアクションの後処理をします
        ''' </summary>
        Protected Overridable Sub DisposeCreate()
        End Sub

        ''' <summary>
        ''' 登録処理のコンテキストを作成します
        ''' サブクラスでオーバーライドして実装します
        ''' </summary>
        Protected MustOverride Sub CreateQuery()

        ''' <summary>
        ''' チェック処理をします
        ''' サブクラスでオーバーライドして実装します
        ''' </summary>
        ''' <returns></returns>
        Protected MustOverride Function CreateChcek() As Boolean

#End Region

#Region "更新処理"
        ''' <summary>
        ''' アクションメソッド(Edit)
        ''' </summary>
        Protected Sub BaseEdit(ByRef BaseModel As BaseModel)

            'エラーでない場合、更新処理を行う
            If ModelState.IsValid Then
                '初期化処理
                InitializeEdit()

                'チェック処理
                If EditChcek() Then
                    '更新処理のクエリを作成(サブクラスで実装)
                    EditQuery()

                    'SQLの実行
                    ExecQuery()
                End If
            End If

            'エラー情報を設定
            SetErrInfo(BaseModel)

            '後処理
            DisposeEdit()

        End Sub

        ''' <summary>
        ''' Editアクションの初期化処理をします
        ''' </summary>
        Protected Overridable Sub InitializeEdit()

            '初期化処理
            _paramSqlList.Clear()           'SQLパラメータのクリア

        End Sub

        ''' <summary>
        ''' チェック処理をします
        ''' サブクラスでオーバーライドして実装します
        ''' </summary>
        ''' <returns></returns>
        Protected MustOverride Function EditChcek() As Boolean

        ''' <summary>
        ''' Editアクションの後処理をします
        ''' </summary>
        Protected Overridable Sub DisposeEdit()
        End Sub

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' サブクラスでオーバーライドして実装します
        ''' </summary>
        Protected MustOverride Sub EditQuery()

#End Region

#Region "削除処理"
        ''' <summary>
        ''' アクションメソッド(Delete)
        ''' </summary>
        Protected Sub BaseDelete(ByRef BaseModel As BaseModel)

            'エラーでない場合、削除処理を行う
            If ModelState.IsValid Then
                '初期化処理
                InitializeDelete()

                'チェック処理
                If DeleteChcek() Then
                    '削除処理のクエリを作成(サブクラスで実装)
                    DeleteQuery()

                    'SQLの実行
                    ExecQuery()
                End If
            End If

            'エラー情報を設定
            SetErrInfo(BaseModel)

            '後処理
            DisposeDelete(BaseModel)

        End Sub

        ''' <summary>
        ''' Deleteアクションの初期化処理をします
        ''' </summary>
        Protected Overridable Sub InitializeDelete()

            '初期化処理
            _paramSqlList.Clear()           'SQLパラメータのクリア

        End Sub

        ''' <summary>
        ''' チェック処理をします
        ''' サブクラスでオーバーライドして実装します
        ''' </summary>
        ''' <returns></returns>
        Protected MustOverride Function DeleteChcek() As Boolean

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' サブクラスでオーバーライドして実装します
        ''' </summary>
        Protected MustOverride Sub DeleteQuery()

        ''' <summary>
        ''' Deleteアクションの後処理をします
        ''' </summary>
        Protected Overridable Sub DisposeDelete(ByRef BaseModel As BaseModel)
        End Sub


#End Region

#Region "アクション"
        ''' <summary>
        ''' アクション メソッドの実行前処理
        ''' </summary>
        ''' <param name="filterContext">コンテキスト</param>
        Protected Overrides Sub OnActionExecuting(ByVal filterContext As ActionExecutingContext)
            '2019/09/17 「タイムアウトに達しました。プールから接続を取得する前にタイムアウト期間が過ぎました。」
            '           の件の対応
            'Dim cn As Common.DbConnection = _db.Database.Connection
            'cn.Open()
            '_tran = cn.BeginTransaction
            '_db.Database.UseTransaction(_tran)

            If _db.Database.Connection.State = ConnectionState.Closed Then
                _db.Database.Connection.Open()
                _tran = _db.Database.Connection.BeginTransaction
                _db.Database.UseTransaction(_tran)
            End If

        End Sub

        ''' <summary>
        ''' アクション メソッドの実行後処理
        ''' </summary>
        ''' <param name="filterContext">コンテキスト</param>
        Protected Overrides Sub OnActionExecuted(ByVal filterContext As ActionExecutedContext)
            If Not IsNothing(_tran) AndAlso Not IsNothing(_tran.Connection) Then
                If ModelState.IsValid Then
                    _tran.Commit()
                Else
                    _tran.Rollback()
                End If
            End If
            _tran = Nothing
        End Sub

        ''' <summary>
        ''' 例外が発生したときの処理
        ''' </summary>
        ''' <param name="filterContext">コンテキスト</param>
        Protected Overrides Sub OnException(ByVal filterContext As ExceptionContext)
            'システム日時
            Dim strNowDate As Date = DateTime.Now

            'エラー情報をセッションに保持
            Session(CommonSessionKey.SYSTEM_ERR_DATE) = strNowDate.ToString("yyyy/MM/dd HH:mm")     'エラー日時
            Session(CommonSessionKey.SYSTEM_ERR_MESSAGE) = filterContext.Exception.Message          'エラーメッセージ

            If Not IsNothing(_tran) AndAlso Not IsNothing(_tran.Connection) Then
                _tran.Rollback()
            End If

            Try
                Dim loginInfo As RfidEntity.LoginUser = Session(CommonSessionKey.CREAGE_LOGIN)

                '操作ログ出力
                Dim sProcessContents As String = filterContext.Exception.Message
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Error, sProcessContents, loginInfo, String.Empty)

            Catch ex As Exception
                '操作ログの書き込みエラー時は、そのまま、流す
            End Try

        End Sub
#End Region

#Region "共通"

        ''' <summary>
        ''' 登録処理のクエリを実行します
        ''' </summary>
        Protected Overridable Sub ExecQuery()

            '登録処理を実行
            _db.SaveChanges()

        End Sub

        ''' <summary>
        ''' SQLパラメータを設定します
        ''' </summary>
        ''' <param name="strKey">キー(@なし)</param>
        ''' <param name="strValue">値</param>
        Protected Sub addSqlParam(ByVal strKey As String, ByVal strValue As String)
            _paramSqlList.Add(New SqlParameter($"@{strKey}", strValue))
        End Sub

        ''' <summary>
        ''' エラー情報を設定します
        ''' </summary>
        Private Sub SetErrInfo(ByRef model As BaseModel)
            Dim count As Integer = 0
            For Each value In ModelState.Values
                'If value.Errors.Count = 1 Then     'Upd 2020.03.09 Iwamoto
                If value.Errors.Count >= 1 Then
                    model.ErrCode = value.Errors(0).ErrorMessage
                    model.ErrTarget = ModelState.Keys(count)
                    Exit For
                End If
                count = count + 1
            Next
        End Sub
#End Region

#Region "複数選択・複数入力の検索条件 共通処理"
        ''' <summary>
        ''' 複数選択された値のIN句を作成し、SQLパラメータを設定します
        ''' </summary>
        ''' <param name="column">項目名（例：ASSET.LOC_M_CD）</param>
        ''' <param name="paramName">パラメータ名(@なし)。末尾に連番を付けて設定します（例：SearchLocM0, SearchLocM1…）</param>
        ''' <param name="values">複数選択された値</param>
        ''' <returns>「  AND 項目名 IN (…)」（未選択時は空文字）</returns>
        ''' <remarks>空の値・重複は CommonRfid.GetSelectedValues で除きます</remarks>
        Protected Function CreateInCondition(ByVal column As String, ByVal paramName As String, ByVal values As String()) As String
            Dim lstValue As String() = CommonRfid.GetSelectedValues(values)
            If lstValue.Length = 0 Then
                Return String.Empty
            End If

            Dim lstParam As New List(Of String)
            For i As Integer = 0 To lstValue.Length - 1
                Dim key As String = $"{paramName}{i}"
                lstParam.Add($"@{key}")
                addSqlParam(key, lstValue(i))
            Next

            Return "  AND " & column & " IN (" & String.Join(", ", lstParam) & ")"
        End Function

        ''' <summary>
        ''' 複数入力された値のLIKE条件（OR）を作成し、SQLパラメータを設定します
        ''' </summary>
        ''' <param name="columns">項目名（複数指定時は、いずれかの項目に一致すれば対象。例：{"ASSET.ASSET_NO", "ASSET.SUB_ASSET_NO"}）</param>
        ''' <param name="paramName">パラメータ名(@なし)。末尾に連番を付けて設定します（例：SearchAssetNo0, SearchAssetNo1…）</param>
        ''' <param name="values">複数入力された値（CommonRfid.GetMultiInputValues で分割したもの）</param>
        ''' <param name="isPartialMatch">True：部分一致（%値%）、False：前方一致（値%）</param>
        ''' <returns>「  AND ( 項目名 Like @xx0 OR … )」（未入力時は空文字）</returns>
        ''' <remarks>空の値・重複は CommonRfid.GetSelectedValues で除きます</remarks>
        Protected Function CreateLikeCondition(ByVal columns As String(), ByVal paramName As String, ByVal values As String(), Optional ByVal isPartialMatch As Boolean = False) As String
            Dim lstValue As String() = CommonRfid.GetSelectedValues(values)
            If lstValue.Length = 0 Then
                Return String.Empty
            End If

            Dim lstCond As New List(Of String)
            For i As Integer = 0 To lstValue.Length - 1
                Dim key As String = $"{paramName}{i}"
                For Each column As String In columns
                    lstCond.Add($"{column} Like @{key}")
                Next
                addSqlParam(key, If(isPartialMatch, "%", "") & lstValue(i) & "%")
            Next

            Return "  AND ( " & String.Join(" OR ", lstCond) & " ) "
        End Function
#End Region

#Region "ログインユーザーと部署名を再設定する。"
        ''' <summary>
        ''' ログインユーザーと部署名を再設定する。
        ''' </summary>
        ''' <remarks>2021/10/07 tomatsuri create</remarks>
        Public Sub UpdateLoginName()
            Dim db As New SystemDBContext
            Dim loginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報

            loginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If loginInfo Is Nothing Then
                Exit Sub
            End If

            Dim EmployeeModel As EmployeeModel = Nothing
            EmployeeModel = db.Employee.Find(loginInfo.EmployeeCd)
            '社員名
            loginInfo.EmployeeNm = EmployeeModel.EMPLOYEE_NAME
            '部門コード・部門名
            loginInfo.DepartmentCd = EmployeeModel.DEPARTMENT_CD
            'IDをキーにモデルを取得する
            Dim DepartmentModel As DepartmentModel = Nothing
            DepartmentModel = db.Department.Find(EmployeeModel.DEPARTMENT_CD)
            '部門名
            If DepartmentModel IsNot Nothing Then
                loginInfo.DepartmentNm = DepartmentModel.DEPARTMENT_NAME
            End If
        End Sub
#End Region
    End Class


End Namespace
