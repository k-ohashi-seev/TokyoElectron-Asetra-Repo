Imports System.Data
Imports System.Data.Common
Imports System.Configuration
'Imports System.Data.OracleClient    'オラクル参照 使用しない時はコメントにする
'Imports System.Data.OleDb           'アクセス参照 使用しない時はコメントにする 
Imports System.Data.SqlClient       'SQLServer参照 使用しない時はコメントにする 

''' <summary>
''' 株式会社 シービジョン
''' クラス名：データベース操作クラス
''' 作成日  ：2018/04/01  Ver.1.0.0.0
''' 修正日  ：0000/00/00  Ver.0.0.0.0
''' </summary>
''' <remarks></remarks>
Public Class clsSV_DB

    Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "定数宣言"

#Const ORACLE_VALID = 0     'オラクル 使用する=1, 使用しない=0
#Const ACCESS_VALID = 0     'アクセス 使用する=1, 使用しない=0
#Const SQLSERVER_VALID = 1  'SQLServer 使用する=1, 使用しない=0

    ''' <summary>クラス名</summary>
    Private Const CLASS_NAME As String = "clsSV_DB"
    ''' <summary>リトライ回数</summary>
    Private Const RETRY_MAX_COUNT As Integer = 1

    ''' <summary>SQLServer用システム日付取得コマンド文字列</summary>
    Public Const SQL_GET_DATE As String = "GetDate()"   'SQLServer

    ''' <summary>SQLServer用システム日付取得コマンド文字列</summary>
    Public Const SQL_GET_DATE2 As String = "CONVERT(datetime, CONVERT(char(19), GETDATE(), 126))"   'SQLServer

    ''' <summary>日時表示文字列変換書式 SQLSERVER側(ODBC 標準)</summary>
    Public Const DATE_DISP_STR_SQL As String = "120"

    ''' <summary>日時表示文字列変換書式 VB側(ODBC 標準にあわせる)</summary>
    Public Const DATE_DISP_STR_VB As String = "yyyy-MM-dd HH:mm:ss"

#End Region

#Region "列挙体定義"
    ''' <summary>データベース種別選択</summary>
    Public Enum EN_DbKind
        ''' <summary>非選択</summary>
        UnKnown = -1
#If ORACLE_VALID > 0 Then
    ''' <summary>Oracle</summary>
        Oracle = 1
#End If
#If ACCESS_VALID > 0 Then
    ''' <summary>Access</summary>
        Access = 2
#End If
#If SQLSERVER_VALID > 0 Then
        ''' <summary>SQLServer</summary>
        SQLServer = 3
#End If
    End Enum

#End Region

#Region "構造体定義"

#If ORACLE_VALID > 0 Then
    ''' <summary>Oracle コネクションクラス</summary>
    Private Class ORA_CONNECTION
    ''' <summary>コネクション</summary>
        Public ClsOleCn As New OracleConnection           
    ''' <summary>トランザクション</summary>
        Public ClsTrans As OracleTransaction = Nothing    
    ''' <summary>データアダプタ </summary>
        Public ClsAdapter As OracleDataAdapter = Nothing  
    End Class
#End If

#If ACCESS_VALID > 0 Then
    ''' <summary>Access コネクションクラス</summary>
    Private Class ACC_CONNECTION
    ''' <summary>コネクション</summary>
        Public ClsOleCn As New OleDbConnection             
    ''' <summary>トランザクション</summary>
        Public ClsTrans As OleDbTransaction = Nothing      
    ''' <summary>データアダプタ </summary>
        Public ClsAdapter As OleDbDataAdapter = Nothing    
    End Class
#End If

#If SQLSERVER_VALID > 0 Then
    ''' <summary>SQLServer コネクションクラス</summary>
    Private Class SQL_S_CONNECTION
        ''' <summary>コネクション</summary>
        Public ClsOleCn As New SqlConnection
        ''' <summary>トランザクション</summary>
        Public ClsTrans As SqlTransaction = Nothing
        ''' <summary>データアダプタ</summary>
        Public ClsAdapter As SqlDataAdapter = Nothing
    End Class
#End If

#End Region

#Region "変数定義"

#If ORACLE_VALID > 0 Then
    ''' <summary>Oracle コネクションクラス</summary>
    Private _ClsConectOra As ORA_CONNECTION = Nothing   'Oracle
#End If

#If ACCESS_VALID > 0 Then
    ''' <summary>Access コネクションクラス</summary>
    Private _ClsConectAcc As ACC_CONNECTION = Nothing   'Access
#End If

#If SQLSERVER_VALID > 0 Then
    ''' <summary>SQLServer コネクションクラス</summary>
    Private _ClsConectSql As SQL_S_CONNECTION = Nothing  'SQLServer
#End If

    ''' <summary>プロバイダーファクトリー</summary>
    Private _factory As DbProviderFactory
    ''' <summary>コネクション識別</summary>
    Private _conectKind As EN_DbKind = EN_DbKind.UnKnown
    '''' <summary>テーブルデータ</summary>
    'Private _tableData As DataTable = Nothing

#End Region

#Region "ログ設定プロパティ"
    ''' <summary>エラーログ用 呼出し元クラス名</summary>
    Private _className As String = CLASS_NAME
    ''' <summary>エラーログ用 呼出し元クラス名</summary>
    Public WriteOnly Property p_logClassName As String
        Set(ByVal value As String)
            If value.Trim.Length > 0 Then
                _className = value
            Else
                _className = CLASS_NAME
            End If
        End Set
    End Property

    ''' <summary>エラーログ用 呼出し元プロシージャ名</summary>
    Private _procName As String = String.Empty
    ''' <summary>エラーログ用 呼出し元プロシージャ名</summary>
    Public WriteOnly Property p_logProcName As String
        Set(ByVal value As String)
            _procName = value
        End Set
    End Property

    ''' <summary>ログ出力先サブフォルダ名</summary>
    Private _logSubFolderName As String = String.Empty
    ''' <summary>ログ出力先サブフォルダ名</summary>
    ''' <remarks>※ソケット通信等マルチスレッド処理を行なう場合でかつ、ハンディ毎にログ出力先フォルダを振分ける場合などに指定する</remarks>
    Public Property p_logSubFolderName As String
        Get
            Return _logSubFolderName
        End Get
        Set(ByVal value As String)
            _logSubFolderName = value
        End Set
    End Property

#End Region

#Region "エラー返却プロパティ"
    ''' <summary>エラーオブジェクト</summary>
    Private _ClsErrorObj As Exception = Nothing
    ''' <summary>エラーオブジェクト</summary>
    Public ReadOnly Property p_errorObj As Exception
        Get
            Return _ClsErrorObj
        End Get
    End Property

    ''' <summary>一意制約エラー</summary>
    Public ReadOnly Property p_primaryErr As Boolean
        Get
            'エラーメッセージから一意制約エラーを検出する
            Select Case _conectKind
#If ORACLE_VALID > 0 Then
                Case EN_DbKind.Oracle
                    '---- Oracle ----
                    If Not _ClsErrorObj Is Nothing AndAlso _
                        _ClsErrorObj.Message.IndexOf("ORA-00001") >= 0 Then
                        Return True
                    End If
#End If
#If ACCESS_VALID > 0 Then
                Case EN_DbKind.Access
                    '---- Access ----
                    If Not _ClsErrorObj Is Nothing AndAlso _
                       _ClsErrorObj.Message.IndexOf("主キー") >= 0 Then
                        Return True
                    End If
#End If
#If SQLSERVER_VALID > 0 Then
                Case EN_DbKind.SQLServer
                    '---- SQLServer ----
                    If Not _ClsErrorObj Is Nothing AndAlso
                       _ClsErrorObj.Message.IndexOf("PRIMARY KEY") >= 0 Then
                        Return True
                    End If
#End If
            End Select
            Return False
        End Get
    End Property

#End Region

#Region "データベース接続処理"
    ''' <summary>
    ''' データベース接続処理
    ''' </summary>
    ''' <param name="prmConnectionStringKey">connectionStringのKEY文字列</param>
    ''' <returns>True=接続成功, False=接続失敗</returns>
    ''' <remarks>
    ''' </remarks>
    Public Function SVFncOpenConnection(ByVal prmConnectionStringKey As String) As Boolean

        Dim conectString As String = String.Empty       '接続文字列
        Dim setting As ConnectionStringSettings

        Try

            Dim retryCount As Integer = 0               'リトライ回数

#If ORACLE_VALID > 0 Then
            '------- Oracleコネクション -------
            _conectKind = EN_DbKind.Oracle     '種別保存

            conectString &= "Data Source = " & prmServiceID & ";"
            conectString &= "User ID     = " & prmUserID & ";"
            conectString &= "PassWord    = " & prmPassWord & ";"

            _ClsConectOra = New ORA_CONNECTION

            For retryCount = 1 To RETRY_MAX_COUNT
                Try
                    _ClsConectOra.ClsOleCn = New OracleConnection(conectString)
                    _ClsConectOra.ClsOleCn.Open()       '接続　エラー→Catch
                    Return True

                Catch ex As Exception
                    _ClsErrorObj = ex
                End Try
            Next retryCount
            _ClsConectOra = Nothing                     '接続NG
#End If
#If ACCESS_VALID > 0 Then
            '------- Accessコネクション -------
            _conectKind = EN_DbKind.Access     '種別保存

            conectString &= "Provider    = Microsoft.Jet.OLEDB.4.0;"
            conectString &= "Data Source = " & prmDataBaseName & ";"
            conectString &= "User ID     = " & prmUserID & ";"
            conectString &= "Jet OLEDB:Database PassWord = " & prmPassWord

            _ClsConectAcc = New ACC_CONNECTION
            _ClsConectAcc.ClsOleCn.conectString = conectString

            For retryCount = 1 To RETRY_MAX_COUNT
                Try
                    _ClsConectAcc.ClsOleCn.Open()       '接続　エラー→Catch
                    Return True

                Catch ex As Exception
                    _ClsErrorObj = ex
                End Try
            Next retryCount
            _ClsConectAcc = Nothing                     '接続NG
#End If
#If SQLSERVER_VALID > 0 Then
            '------ SQLServerコネクション -------
            _conectKind = EN_DbKind.SQLServer     '種別保存

            setting = ConfigurationManager.ConnectionStrings(prmConnectionStringKey)
            Me._factory = DbProviderFactories.GetFactory(setting.ProviderName)

            _ClsConectSql = New SQL_S_CONNECTION
            _ClsConectSql.ClsOleCn.ConnectionString = setting.ConnectionString

            For retryCount = 1 To RETRY_MAX_COUNT
                Try
                    _ClsConectSql.ClsOleCn.Open()       '接続　エラー→Catch
                    Return True

                Catch ex As Exception
                    _ClsErrorObj = ex
                End Try
            Next retryCount
            _ClsConectSql = Nothing                     '接続NG
#End If

            'TODO LOG処理変更
            'Call SVSubWriteLog(EN_LogType.Err, _className, _procName, "データベース接続エラー", _ClsErrorObj.Message & "(" & conectString & ")", _logSubFolderName)

            Return False
        Catch ex As Exception
            _ClsErrorObj = ex

            logger.Error(ex.Message)
            Throw
        Finally

        End Try

    End Function

#End Region

#Region "データベース解放処理"
    ''' <summary>
    ''' データベース解放処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SVSubCloseConnection()
        Try

            Select Case _conectKind
#If ORACLE_VALID > 0 Then
            Case EN_DbKind.Oracle
                '---- Oracle ----
                If Not _ClsConectOra.ClsTrans Is Nothing Then         'トランザクション未完了 ロールバック
                    SVFncFinishTransaction(False)
                End If

                _ClsConectOra.ClsOleCn.Close()        'DB切断
                _ClsConectOra.ClsOleCn.Dispose()
                _ClsConectOra = Nothing
#End If
#If ACCESS_VALID > 0 Then
            Case EN_DbKind.Access
                '---- Access ----
                If Not _ClsConectAcc.ClsTrans Is Nothing Then         'トランザクション未完了 ロールバック
                    SVFncFinishTransaction(False)
                End If

                _ClsConectAcc.ClsOleCn.Close()        'DB切断
                _ClsConectAcc.ClsOleCn.Dispose()
                _ClsConectAcc = Nothing
#End If
#If SQLSERVER_VALID > 0 Then
                Case EN_DbKind.SQLServer
                    '---- SQLServer ----
                    If Not _ClsConectSql.ClsTrans Is Nothing Then     'トランザクション未完了 ロールバック
                        SVFncFinishTransaction(False)
                    End If

                    _ClsConectSql.ClsOleCn.Close()    'DB切断
                    _ClsConectSql.ClsOleCn.Dispose()
                    _ClsConectSql = Nothing
#End If
                Case Else
                    Return
            End Select

            _conectKind = EN_DbKind.UnKnown
        Catch ex As Exception
            _ClsErrorObj = ex

            logger.Error(ex.Message)
            Throw
        End Try

    End Sub

#End Region

#Region "データベース接続状態チェック処理"
    ''' <summary>
    ''' データベース接続状態チェック処理
    ''' </summary>
    ''' <returns>True=接続, False=未接続</returns>
    ''' <remarks></remarks>
    Public Function SVFncCheckConnectionStatus() As Boolean
        Try
#If ORACLE_VALID > 0 Then
            '---- Oracle ----
            If _ClsConectOra.ClsOleCn.State = ConnectionState.Open Then
                'Return True
                '接続チェックＳＱＬ
                Dim dtBuff As DataTable = SVFncSelectDataTable(" SELECT 1 FROM DUAL")

                If dtBuff Is Nothing Then
                    Return False
                Else
                    Return True
                End If
            End If
#End If

#If ACCESS_VALID > 0 Then
            '---- Access ----
            If _ClsConectAcc.ClsOleCn.State = ConnectionState.Open Then
                Return True
            End If
#End If
#If SQLSERVER_VALID > 0 Then
            '---- SQLServer ----
            If _ClsConectSql.ClsOleCn.State = ConnectionState.Open Then
                Return True
            End If
#End If

            Return False
        Catch ex As Exception
            _ClsErrorObj = ex

            logger.Error(ex.Message)
            Throw
        End Try

    End Function

#End Region

#Region "トランザクション開始処理"
    ''' <summary>
    ''' トランザクション開始処理
    ''' </summary>
    ''' <returns>True=トランザクション開始成功, False=トランザクション開始失敗</returns>
    ''' <remarks></remarks>
    Public Function SVFncStartTransaction() As Boolean
        Try

#If ORACLE_VALID > 0 Then
            '---- Oracle ----
            Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "トランザクション開始", _ClsConectOra.ClsOleCn.ConnectionString, _logSubFolderName)
            _ClsConectOra.ClsTrans = _ClsConectOra.ClsOleCn.BeginTransaction
#End If
#If ACCESS_VALID > 0 Then
            '---- Access ----
            Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "トランザクション開始", _ClsConectAcc.ClsOleCn.ConnectionString, _logSubFolderName)
            _ClsConectAcc.ClsTrans = _ClsConectAcc.ClsOleCn.BeginTransaction
#End If
#If SQLSERVER_VALID > 0 Then
            '---- SQLServer ----
            'TODO LOG処理変更
            'Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "トランザクション開始", _ClsConectSql.ClsOleCn.ConnectionString, _logSubFolderName)
            _ClsConectSql.ClsTrans = _ClsConectSql.ClsOleCn.BeginTransaction
#End If

            Return True
        Catch ex As Exception
            _ClsErrorObj = ex

            logger.Error(ex.Message)
            Throw
        End Try

    End Function

#End Region

#Region "トランザクション終了処理"
    ''' <summary>
    ''' トランザクション終了処理
    ''' </summary>
    ''' <param name="prmCommit">True=コミット, False=ロールバック</param>
    ''' <returns>True=正常終了, False=異常終了</returns>
    ''' <remarks></remarks>
    Public Function SVFncFinishTransaction(ByVal prmCommit As Boolean) As Boolean
        Try
#If ORACLE_VALID > 0 Then
            '---- Oracle ----
            If Not _ClsConectOra.ClsTrans Is Nothing Then
                If prmCommit Then
                    Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "コミット", _ClsConectOra.ClsOleCn.ConnectionString, _logSubFolderName)
                    _ClsConectOra.ClsTrans.Commit()    'コミット
                Else
                    Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "ロールバック", _ClsConectOra.ClsOleCn.ConnectionString, _logSubFolderName)
                    _ClsConectOra.ClsTrans.Rollback()  'ロールバック
                End If
                _ClsConectOra.ClsTrans = Nothing
            End If
#End If
#If ACCESS_VALID > 0 Then
            '---- Access ----
            If Not _ClsConectAcc.ClsTrans Is Nothing Then
                If prmCommit Then
                    Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "コミット", _ClsConectAcc.ClsOleCn.ConnectionString, _logSubFolderName)
                    _ClsConectAcc.ClsTrans.Commit()    'コミット
                Else
                    Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "ロールバック", _ClsConectAcc.ClsOleCn.ConnectionString, _logSubFolderName)
                    _ClsConectAcc.ClsTrans.Rollback()  'ロールバック
                End If
                _ClsConectAcc.ClsTrans = Nothing
            End If
#End If
#If SQLSERVER_VALID > 0 Then
            '---- SQLServer ----
            If Not _ClsConectSql.ClsTrans Is Nothing Then
                If prmCommit Then

                    'TODO LOG処理変更
                    'Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "コミット", _ClsConectSql.ClsOleCn.ConnectionString, _logSubFolderName)
                    _ClsConectSql.ClsTrans.Commit()    'コミット
                Else
                    'TODO LOG処理変更
                    'Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "ロールバック", _ClsConectSql.ClsOleCn.ConnectionString, _logSubFolderName)
                    _ClsConectSql.ClsTrans.Rollback()  'ロールバック
                End If
                _ClsConectSql.ClsTrans = Nothing
            End If
#End If

            Return True
        Catch ex As Exception
            _ClsErrorObj = ex

            logger.Error(ex.Message)
            Throw
        End Try
    End Function

#End Region

#Region "データベース検索処理"
    ''' <summary>
    ''' データベース検索処理
    ''' </summary>
    ''' <param name="prmSQL">検索SELECT文</param>
    ''' <param name="params">パラメータList()</param>
    ''' <param name="prmLogHalt">ログ出力区分  True=ログ出力しない, False=ログ出力する</param>
    ''' <returns>データテーブル、エラーの時はNothingを返す</returns>
    ''' <remarks></remarks>
    Public Function SVFncSelectDataTable(ByVal prmSQL As String, Optional ByVal params As List(Of SqlParameter) = Nothing, Optional ByVal prmLogHalt As Boolean = False) As DataTable
        SVFncSelectDataTable = Nothing

        Try

            If prmLogHalt = False Then
                'TODO LOG処理変更
                'Call SVSubWriteLog(EN_LogType.DbSelect, _className, _procName, "データベース検索", prmSQL, _logSubFolderName)
            End If

            Dim tbl As DataTable = New DataTable

#If ORACLE_VALID > 0 Then
            '---- Oracle ----
            Dim cmd As OracleCommand = New OracleCommand(prmSQL, _ClsConectOra.ClsOleCn)    'コマンド SQL実行
            If Not IsNothing(params) Then
                cmd.Parameters.AddRange(params.ToArray)
            End If
            If Not _ClsConectOra.ClsTrans Is Nothing Then
                cmd.Transaction = _ClsConectOra.ClsTrans                                    'トランザクション
            End If
            _ClsConectOra.ClsAdapter = New OracleDataAdapter(cmd)
            _ClsConectOra.ClsAdapter.Fill(tbl)                                       'データテーブルを返す
#End If
#If ACCESS_VALID > 0 Then
            '---- Access ----
            Dim cmd As New OleDbCommand                                                     'コマンド
            If Not IsNothing(params) Then
                cmd.Parameters.AddRange(params.ToArray)
            End If
            cmd = _ClsConectAcc.ClsOleCn.CreateCommand                                      'コネクションからコマンドオブジェクト
            cmd.CommandType = CommandType.Text
            cmd.CommandText = prmSQL
            If Not _ClsConectAcc.ClsTrans Is Nothing Then
                cmd.Transaction = _ClsConectAcc.ClsTrans                                    'トランザクション
            End If

            _ClsConectAcc.ClsAdapter = New OleDbDataAdapter(cmd)                            'SQL実行
            _ClsConectAcc.ClsAdapter.Fill(tbl)                                       'データテーブルを返す
#End If
#If SQLSERVER_VALID > 0 Then
            '---- SQLServer ----
            Dim cmd As SqlCommand = New SqlCommand(prmSQL, _ClsConectSql.ClsOleCn)          'コマンド SQL実行

            cmd.CommandTimeout = 600            '2020/02/27 Inoue ADD

            If Not IsNothing(params) Then
                cmd.Parameters.AddRange(params.ToArray)
            End If
            If Not _ClsConectSql.ClsTrans Is Nothing Then
                cmd.Transaction = _ClsConectSql.ClsTrans                                    'トランザクション
            End If

            _ClsConectSql.ClsAdapter = New SqlDataAdapter(cmd)
            _ClsConectSql.ClsAdapter.Fill(tbl)                                       'データテーブルを返す
#End If

            Return tbl

        Catch ex As Exception
            _ClsErrorObj = ex

            logger.Error(ex.Message)
            Throw
        End Try

    End Function

#End Region

#Region "データベース更新処理"
    ''' <summary>
    ''' データベース更新処理
    ''' </summary>
    ''' <param name="prmSQL">更新SQL文</param>
    ''' <param name="params">パラメータList()</param>
    ''' <param name="prmPrimayLogPass">True=一意制約エラー時のログ省略</param>
    ''' <returns>更新件数、＜0:更新エラー</returns>
    ''' <remarks></remarks>
    Public Function SVFncExecuteNonQuery(ByVal prmSQL As String, Optional ByVal params As List(Of SqlParameter) = Nothing, Optional ByVal prmPrimayLogPass As Boolean = False) As Integer
        Dim result As Integer = -1
        Try

            If prmPrimayLogPass = False Then
                'TODO LOG処理変更
                'SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "ＤＢ登録", prmSQL, _logSubFolderName)
            End If

#If ORACLE_VALID > 0 Then
                '---- Oracle ----
                Dim cmd As OracleCommand = New OracleCommand(prmSQL, _ClsConectOra.ClsOleCn)    'コマンドオブジェクト
                If Not _ClsConectOra.ClsTrans Is Nothing Then
                    cmd.Transaction = _ClsConectOra.ClsTrans                                    'トランザクション
                End If
                If Not IsNothing(params) Then
                    cmd.Parameters.AddRange(params.ToArray)
                End If
                result = cmd.ExecuteNonQuery()                                                  'ＳＱＬ実行
#End If
#If ACCESS_VALID > 0 Then
                '---- Access ----
                Dim cmd As New OleDbCommand
                cmd = _ClsConectAcc.ClsOleCn.CreateCommand                                      'コマンドオブジェクト
                If Not _ClsConectAcc.ClsTrans Is Nothing Then
                    cmd.Transaction = _ClsConectAcc.ClsTrans                                    'トランザクション
                End If
                cmd.CommandText = prmSQL
                 If Not IsNothing(params) Then
                    cmd.Parameters.AddRange(params.ToArray)
                End If
               result = cmd.ExecuteNonQuery()                                                  'ＳＱＬ実行
#End If
#If SQLSERVER_VALID > 0 Then
            '---- SQLServer ----
            Dim cmd As SqlCommand = New SqlCommand(prmSQL, _ClsConectSql.ClsOleCn)          'コマンドオブジェクト

            cmd.CommandTimeout = 600            '2020/02/27 Inoue ADD

            If Not _ClsConectSql.ClsTrans Is Nothing Then
                cmd.Transaction = _ClsConectSql.ClsTrans                                    'トランザクション
            End If

            If Not IsNothing(params) Then
                cmd.Parameters.AddRange(params.ToArray)
            End If
            result = cmd.ExecuteNonQuery()                                                  'ＳＱＬ実行
#End If

            If prmPrimayLogPass Then    '更新OK 事後ログ作成
                'TODO LOG処理変更
                'Call SVSubWriteLog(EN_LogType.DbIUD, _className, _procName, "データベース登録", prmSQL, _logSubFolderName)
            End If

            Return result

        Catch ex As Exception
            _ClsErrorObj = ex

            logger.Error(ex.Message)
            Throw
        End Try

    End Function
#End Region

#Region "DBNull変換処理"
    ''' <summary>
    ''' DBNull変換処理
    ''' </summary>
    ''' <param name="prmValue">変換対象</param>
    ''' <param name="prmNum">数値種別 True=数値, False=文字列</param>
    ''' <returns>変換後文字列</returns>
    ''' <remarks>Nullを判定しNullの場合は空文字列か0を返す</remarks>
    Public Function SVFncCnvDBNullToString(ByVal prmValue As Object, Optional ByVal prmNum As Boolean = False) As String
        Try
            If prmValue Is DBNull.Value Then
                If Not prmNum Then
                    Return String.Empty
                Else
                    Return "0"
                End If
            Else
                Return prmValue.ToString
            End If

        Catch ex As Exception
            logger.Error(ex.Message)
            Throw
        End Try
    End Function

#End Region

#Region "ＤＢ書き込み文字列生成処理"
    ''' <summary>
    ''' ＤＢ書き込み文字列生成処理
    ''' </summary>
    ''' <param name="prmValue">対象値</param>
    ''' <param name="prmEmptyIsNull">
    ''' True=対象値が文字列型で空文字列の場合､対象値が数値型で0の場合は"Null"を返す
    ''' False=対象値に合った変換文字列を返す
    ''' </param>
    ''' <returns>変換文字列</returns>
    ''' <remarks>書き込みSQL文作成時に、型別に書き込み文字列を生成する</remarks>
    Public Function SVFncCreateSQLValueString(ByVal prmValue As Object, Optional ByVal prmEmptyIsNull As Boolean = False) As String
        Try
            '今回、SQLSERVER 専用で、海外対応のため、日付書式を付けいています。
#If SQLSERVER_VALID > 0 Then
            '対象値のデータ型チェック
            Select Case (VarType(prmValue))
                ' 文字列型
                Case VariantType.String, VariantType.Char
                    If prmValue.ToString = SQL_GET_DATE Or prmValue.ToString = SQL_GET_DATE2 Then
                        'システム日付指定時
                        Return " " & prmValue.ToString
                    Else
                        If prmEmptyIsNull AndAlso String.IsNullOrEmpty(prmValue.ToString) Then
                            Return " Null "
                        Else
                            Return " '" & prmValue.ToString & "' "
                        End If
                    End If

                    ' 数値型
                Case VariantType.Integer, VariantType.Long, VariantType.Short,
                     VariantType.Byte, VariantType.Currency, VariantType.Decimal,
                     VariantType.Double, VariantType.Single, VariantType.Boolean
                    If prmEmptyIsNull AndAlso Convert.ToDecimal(prmValue) = 0 Then
                        Return " Null "
                    Else
                        Return prmValue.ToString
                    End If

                    ' 日付型
                Case VariantType.Date
                    If prmEmptyIsNull AndAlso IsNothing(prmValue) Then
                        Return " Null "
                    Else
                        Return " 'CONVERT(datetime, '" & Convert.ToDateTime(prmValue).ToString(DATE_DISP_STR_VB) & "'," & DATE_DISP_STR_SQL & ")"
                    End If

                Case Else 'VariantType.Null, VariantType.Empty    
                    Return " Null "
            End Select
#End If

        Catch ex As Exception
            logger.Error(ex.Message)
            Throw
        End Try
    End Function

#End Region

#Region "ＤＢ登録禁止文字チェック処理"
    ''' <summary>
    ''' ＤＢ登録禁止文字チェック処理
    ''' </summary>
    ''' <param name="prmValue">チェック文字列</param>
    ''' <returns>チェック文字列含まれている禁止文字</returns>
    ''' <remarks></remarks>
    Public Function SVFncCheckInhibitChar(ByVal prmValue As String) As String
        Try
            If prmValue.IndexOf(","c) >= 0 Then
                Return ","
            End If
            If prmValue.IndexOf("'"c) >= 0 Then
                Return "'"
            End If

            Return String.Empty
        Catch ex As Exception
            logger.Error(ex.Message)
            Throw
        End Try

    End Function

#End Region

#Region "ＤＢ登録禁止文字変換処理"
    ''' <summary>
    ''' ＤＢ登録禁止文字変換処理
    ''' </summary>
    ''' <param name="prmValue">文字列</param>
    ''' <returns>変換後文字列</returns>
    ''' <remarks></remarks>
    Public Function SVFncReplaceInhibitChar(ByVal prmValue As String) As String
        Try
            If prmValue.IndexOf("'") >= 0 Then
                Return prmValue.Replace("'", "''")  'シングルコーテーション エスケープ処理
            Else
                Return prmValue
            End If
        Catch ex As Exception
            logger.Error(ex.Message)
            Throw
        End Try

    End Function

#End Region

End Class
