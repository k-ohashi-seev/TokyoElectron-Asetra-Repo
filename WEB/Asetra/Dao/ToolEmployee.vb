Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient
Imports Asetra.Models

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：社員関連ツール系 Data Access クラス
    ''' 作成日        ：2019/09/04      Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolEmployee

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "社員データ取得"
        ''' <summary>
        ''' 社員データ取得データ取得
        ''' </summary>
        ''' <param name="sEmployeeCd">社員コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetEmployeeData(ByVal sEmployeeCd As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM M_EMPLOYEE ")
                '--2020/03/05 nv miyaji update start
                sbSQL.AppendLine("WHERE EMPLOYEE_CD = @EMPLOYEE_CD ")
                params.Add(New SqlParameter($"@EMPLOYEE_CD", CommonRfid.ToStringNvl(sEmployeeCd)))
                'sbSQL.AppendLine("WHERE EMPLOYEE_CD = '" & sEmployeeCd & "' ")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

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

#Region "社員名、部署名取得データ取得"
        ''' <summary>
        ''' 社員名、部署名取得データ取得
        ''' </summary>
        ''' <param name="sEmployeeCd">社員コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetEmployeeDepartmentNm(ByVal sEmployeeCd As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT EMPLOYEE_NAME, MAIL_ADDRESS, ISNULL(DEPARTMENT_NAME, '') AS DEPARTMENT_NAME")
                sbSQL.AppendLine("FROM M_EMPLOYEE ")
                sbSQL.AppendLine("LEFT JOIN M_DEPARTMENT")
                sbSQL.AppendLine("ON   M_DEPARTMENT.DEPARTMENT_CD = M_EMPLOYEE.DEPARTMENT_CD")
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

#Region "社員の所属拠点データ取得"
        ''' <summary>
        ''' 社員の所属拠点データ取得
        ''' </summary>
        ''' <param name="sEmployeeCd">社員コード</param>
        ''' <returns>所属拠点データ</returns>
        Public Shared Function GetHubLocData(sEmployeeCd As String) As DataTable
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("   EMP_HUB.HUB_LOC_L_CD")
                sbSQL.AppendLine("  ,LOC.LOC_NAME AS HUB_LOC_L_NAME")
                sbSQL.AppendLine("FROM M_EMPLOYEE_HUB EMP_HUB")
                sbSQL.AppendLine("   LEFT JOIN M_LOCATION LOC")
                sbSQL.AppendLine("   ON  LOC.LOC_DIV = '" & LOC_DIV.LocationL & "'")
                sbSQL.AppendLine("   AND LOC.LOC_L_CD = EMP_HUB.HUB_LOC_L_CD")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("   EMP_HUB.EMPLOYEE_CD = @EMPLOYEE_CD")
                sbSQL.AppendLine("ORDER BY")
                sbSQL.AppendLine("   EMP_HUB.HUB_LOC_L_CD")
                params.Add(New SqlParameter($"@EMPLOYEE_CD", sEmployeeCd))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                Return tbl
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "社員の所属拠点存在チェック"
        ''' <summary>
        ''' 社員の所属拠点存在チェック
        ''' </summary>
        ''' <param name="sEmployeeCd">社員コード</param>
        ''' <returns>True:存在する　False:存在しない</returns>
        Public Shared Function IsExistsEmpHubLoc(sEmployeeCd As String, sHubLocCd As String) As Boolean
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("   EMP_HUB.HUB_LOC_L_CD")
                sbSQL.AppendLine("FROM M_EMPLOYEE_HUB EMP_HUB")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("   EMP_HUB.EMPLOYEE_CD = @EMPLOYEE_CD")
                sbSQL.AppendLine("AND")
                sbSQL.AppendLine("   EMP_HUB.HUB_LOC_L_CD = @HUB_LOC_L_CD")
                params.Add(New SqlParameter($"@EMPLOYEE_CD", sEmployeeCd))
                params.Add(New SqlParameter($"@HUB_LOC_L_CD", sHubLocCd))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                If tbl.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "社員の予約時U_EMPHUB 条件チェック"
        ''' <summary>
        ''' 社員の予約時U_EMPHUB 条件チェック
        ''' </summary>
        ''' <param name="sEmployeeCd">対象社員コード</param>
        ''' <param name="sChkEmpCd1">チェックする社員コード１</param>
        ''' <param name="sChkEmpCd2">チェックする社員コード２</param>
        ''' <returns>True:存在する　False:存在しない</returns>
        Public Shared Function IsChkAuthEmpHub(sEmployeeCd As String, sChkEmpCd1 As String, sChkEmpCd2 As String) As Boolean
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("   EMP_HUB.HUB_LOC_L_CD")
                sbSQL.AppendLine("FROM M_EMPLOYEE_HUB EMP_HUB")
                sbSQL.AppendLine("WHERE EMP_HUB.EMPLOYEE_CD = @EMPLOYEE_CD")
                sbSQL.AppendLine("AND EXISTS")
                sbSQL.AppendLine("(")
                sbSQL.AppendLine("  SELECT 1 FROM M_EMPLOYEE_HUB TMPEHAB")
                sbSQL.AppendLine("  WHERE TMPEHAB.HUB_LOC_L_CD = EMP_HUB.HUB_LOC_L_CD")
                sbSQL.AppendLine("  AND   TMPEHAB.EMPLOYEE_CD IN (@CHK_EMP_CD1, @CHK_EMP_CD2)")
                sbSQL.AppendLine(")")
                params.Add(New SqlParameter($"@EMPLOYEE_CD", sEmployeeCd))
                params.Add(New SqlParameter($"@CHK_EMP_CD1", sChkEmpCd1))
                params.Add(New SqlParameter($"@CHK_EMP_CD2", sChkEmpCd2))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                If tbl.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "社員権限区分取得処理"
        ''' <summary>
        ''' 社員権限区分取得処理　2022/07/25 add sv sato
        ''' </summary>
        ''' <param name="sEmployeeCd">社員コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetAuthDiv(ByVal sEmployeeCd As String) As String
            Dim ret As String = ""
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM M_EMPLOYEE ")
                sbSQL.AppendLine("WHERE EMPLOYEE_CD = @EMPLOYEE_CD ")
                params.Add(New SqlParameter($"@EMPLOYEE_CD", CommonRfid.ToStringNvl(sEmployeeCd)))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                If tbl.Rows.Count <> 0 Then
                    ret = tbl.Rows(0).Item("AUTH_DIV").ToString
                End If

                Return ret
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return ret

        End Function

#End Region

#Region "社員コード存在チェック処理"
        Public Shared Function ExistEmployeeCd(ByRef db As SystemDBContext, ByVal sEmployeeCd As String) As Boolean
            Dim ret As Boolean = False
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Try
                '社員コードがマスタに存在するかをチェックする
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM M_EMPLOYEE ")
                sbSQL.AppendLine("WHERE EMPLOYEE_CD = @EMPLOYEE_CD ")
                params.Add(New SqlParameter($"@EMPLOYEE_CD", CommonRfid.ToStringNvl(sEmployeeCd)))

                Dim empRecords As List(Of EmployeeModel) = db.Employee.SqlQuery(sbSQL.ToString, params.ToArray).ToList
                If Not IsNothing(empRecords) AndAlso empRecords.Count > 0 Then
                    ret = True
                End If

                Return ret
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            End Try

            Return ret

        End Function
#End Region

'2023/02/08 Kawamoto ADD START
#Region "社員データ（社員名）取得処理"
        ''' <summary>
        ''' 社員データ（社員名）取得処理
        ''' </summary>
        ''' <param name="sEmployeeName">社員名</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetEmployeeDataEmployeeName(ByVal sEmployeeName As String) As DataTable
            Dim tbl As New DataTable

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM M_EMPLOYEE ")
                sbSQL.AppendLine("WHERE EMPLOYEE_NAME = @EMPLOYEE_NAME ")
                params.Add(New SqlParameter($"@EMPLOYEE_NAME", CommonRfid.ToStringNvl(sEmployeeName)))

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

#Region "社員名存在チェック処理"
        ''' <summary>
        ''' 社員名存在チェック処理
        ''' </summary>
        ''' <param name="sEmployeeName">社員名</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function ExistEmployeeName(ByVal sEmployeeName As String) As Boolean
            Dim ret As Boolean = False

            Try
                '社員データ（社員名）取得
                If GetEmployeeDataEmployeeName(sEmployeeName).Rows.Count > 0 Then
                    ret = True
                End If

                Return ret
            Catch ex As Exception
                logger.Error(ex.Message)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            End Try

            Return ret

        End Function
#End Region
        '2023/02/08 Kawamoto ADD END

#Region "社員データ（拠点）取得処理"
        ''' <summary>
        ''' 社員データ（拠点）取得処理
        ''' </summary>
        ''' <param name="sEmployeeCd">社員コード</param>
        ''' <param name="AuthInfo">権限情報</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetEmployeeDataHubLocLCd(ByVal sEmployeeCd As String, AuthInfo As AuthInfo) As DataTable
            Dim tbl As New DataTable

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Dim sb As StringBuilder = New StringBuilder
            Try
                'M_EMPLOYEE_HUBテーブルから取得するSQLを作成
                sb.AppendLine("WITH A AS ( ")
                sb.AppendLine("    SELECT")
                sb.AppendLine("        EMPLOYEE_CD")
                sb.AppendLine("        , EMPLOYEE_NAME ")
                sb.AppendLine("    FROM")
                sb.AppendLine("        M_EMPLOYEE E ")
                sb.AppendLine("    WHERE")
                sb.AppendLine("        RETIRE_FLG = 'N' ")
                sb.AppendLine("        AND AUTH_DIV <> 'S' ")
                sb.AppendLine("        AND EXISTS ( ")
                sb.AppendLine("            SELECT")
                sb.AppendLine("                1 ")
                sb.AppendLine("            FROM")
                sb.AppendLine("                M_EMPLOYEE_HUB H ")
                sb.AppendLine("            WHERE")
                sb.AppendLine("                E.EMPLOYEE_CD = H.EMPLOYEE_CD ")
                sb.AppendLine("                AND EXISTS ( ")
                sb.AppendLine("                    SELECT")
                sb.AppendLine("                        1 ")
                sb.AppendLine("                    FROM")
                sb.AppendLine("                        M_EMPLOYEE_HUB H2 ")
                sb.AppendLine("                    WHERE")
                sb.AppendLine("                        H.HUB_LOC_L_CD = H2.HUB_LOC_L_CD ")
                sb.AppendLine($"                        AND H2.EMPLOYEE_CD = '{sEmployeeCd}'")
                sb.AppendLine("                )")
                sb.AppendLine("        )")
                sb.AppendLine(") ")
                sb.AppendLine("SELECT")
                sb.AppendLine("    1 AS RC")
                sb.AppendLine("    , * ")
                sb.AppendLine("FROM")
                sb.AppendLine("    A ")
                sb.AppendLine("UNION ")
                sb.AppendLine("SELECT")
                sb.AppendLine("    2 AS RC")
                sb.AppendLine("    , EMPLOYEE_CD")
                sb.AppendLine("    , EMPLOYEE_NAME ")
                sb.AppendLine("FROM")
                sb.AppendLine("    M_EMPLOYEE B ")
                sb.AppendLine("WHERE")
                sb.AppendLine("    RETIRE_FLG = 'N' ")
                sb.AppendLine("    AND AUTH_DIV <> 'S' ")
                sb.AppendLine("    AND NOT EXISTS ( ")
                sb.AppendLine("        SELECT")
                sb.AppendLine("            * ")
                sb.AppendLine("        FROM")
                sb.AppendLine("            A ")
                sb.AppendLine("        WHERE")
                sb.AppendLine("            A.EMPLOYEE_CD = B.EMPLOYEE_CD")
                sb.AppendLine("    ) ")
                sb.AppendLine("ORDER BY")
                sb.AppendLine("    RC")
                sb.AppendLine("    , EMPLOYEE_NAME")

                tbl = clsDB.SVFncSelectDataTable(sb.ToString)

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sb)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region

#Region "データ取得(全項目)"
        ''' <summary>
        ''' データ取得(全項目)
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetEmployeeDataAll() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM  M_EMPLOYEE ")
                sbSQL.AppendLine("ORDER BY EMPLOYEE_CD")


                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("EMPLOYEE_CD")}

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

#Region "所属拠点以外で登録されている社員データを取得"
        ''' <summary>
        ''' 所属拠点以外で登録されている社員データを取得
        ''' </summary>
        ''' <param name="loginInfo">ログイン者情報</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetOtherHubEmpData(loginInfo As RfidEntity.LoginUser, authInfo As AuthInfo) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_EMPLOYEEテーブルから取得するSQLを作成
                sbSQL.Append("SELECT * ")
                sbSQL.AppendLine(" FROM M_EMPLOYEE ")
                sbSQL.AppendLine(" WHERE EMPLOYEE_CD NOT IN (")
                sbSQL.AppendLine(" SELECT EMPLOYEE_CD ")
                sbSQL.AppendLine(" FROM M_EMPLOYEE ")
                sbSQL.AppendLine(" WHERE EXISTS")
                sbSQL.AppendLine("(")
                sbSQL.AppendLine(" SELECT 1 FROM M_EMPLOYEE_HUB HUB")
                sbSQL.AppendLine(" INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.AppendLine(" ON HUB.HUB_LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.AppendLine(" AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = HUB.HUB_LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
                params.Add(New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))
                sbSQL.AppendLine(" WHERE HUB.EMPLOYEE_CD = M_EMPLOYEE.EMPLOYEE_CD")
                sbSQL.AppendLine(" )")
                sbSQL.AppendLine(")")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

            Catch ex As Exception
                'SQL ERR
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
    End Class
End Namespace