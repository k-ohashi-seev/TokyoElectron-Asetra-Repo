Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：操作ログ関連ツール系 Data Access クラス
    ''' 作成日        ：2019/09/11      Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolOperationLog
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "操作ログ書き込み処理"
        ''' <summary>
        ''' 操作ログ書き込み処理
        ''' </summary>
        ''' <param name="sProcessCat">処理種別</param>
        ''' <param name="sProcessContents">処理内容</param>
        ''' <param name="loginUser">ログイン情報</param>
        ''' <param name="sTask">処理名</param>
        ''' <param name="sLocLcd">ロケ大区分コード</param>
        ''' <param name="sLocMcd">ロケ中区分コード</param>
        ''' <param name="sLocScd">ロケ小区分コード</param>
        Public Shared Sub InsOpeLog(ByVal sProcessCat As String,
                                         ByVal sProcessContents As String,
                                         ByVal loginUser As RfidEntity.LoginUser,
                                         ByVal sTask As String,
                                         Optional ByVal sLocLcd As String = "",
                                         Optional ByVal sLocMcd As String = "",
                                         Optional ByVal sLocScd As String = ""
                                         )

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)
            Dim tblLoc As DataTable

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))

            End If

            Try
                Dim sLocLNm As String = String.Empty
                Dim sLocMNm As String = String.Empty
                Dim sLocSNm As String = String.Empty

                If String.IsNullOrEmpty(sLocLcd) And String.IsNullOrEmpty(sLocMcd) And String.IsNullOrEmpty(sLocScd) Then
                    'この場合は、ロケーション名は出さない。
                Else
                    tblLoc = New DataTable

                    params = New List(Of SqlParameter)

                    sbSQL.Clear()

                    sbSQL.AppendLine("SELECT")
                    sbSQL.AppendLine("   LOCL.LOC_NAME AS LocLName ")
                    sbSQL.AppendLine("  ,LOCM.LOC_NAME AS LocMName ")
                    sbSQL.AppendLine("  ,LOCS.LOC_NAME AS LocSName ")
                    sbSQL.AppendLine("FROM (SELECT")
                    '--2020/03/05 nv miyaji update start
                    sbSQL.AppendLine("           @LOC_L_CD AS LOC_L_CD")
                    sbSQL.AppendLine("          ,@LOC_M_CD AS LOC_M_CD")
                    sbSQL.AppendLine("          ,@LOC_S_CD AS LOC_S_CD")
                    'sbSQL.AppendLine("           '" & sLocLcd & "' AS LOC_L_CD")
                    'sbSQL.AppendLine("          ,'" & sLocMcd & "' AS LOC_M_CD")
                    'sbSQL.AppendLine("          ,'" & sLocScd & "' AS LOC_S_CD")
                    '--2020/03/05 nv miyaji update end
                    sbSQL.AppendLine("      ) TMP")
                    sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
                    sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
                    sbSQL.AppendLine("AND TMP.LOC_L_CD = LOCL.LOC_L_CD")
                    sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
                    sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
                    sbSQL.AppendLine("AND TMP.LOC_L_CD = LOCM.LOC_L_CD")
                    sbSQL.AppendLine("AND TMP.LOC_M_CD = LOCM.LOC_M_CD")
                    sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
                    sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
                    sbSQL.AppendLine("AND TMP.LOC_L_CD = LOCS.LOC_L_CD")
                    sbSQL.AppendLine("AND TMP.LOC_M_CD = LOCS.LOC_M_CD")
                    sbSQL.AppendLine("AND TMP.LOC_S_CD = LOCS.LOC_S_CD")

                    '--2020/03/05 nv miyaji add start
                    params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(sLocLcd)))
                    params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(sLocMcd)))
                    params.Add(New SqlParameter($"@LOC_S_CD", CommonRfid.ToStringNvl(sLocScd)))
                    '--2020/03/05 nv miyaji add end

                    'params.Add(New SqlParameter($"@LOC_L_CD", sLocLcd))
                    'params.Add(New SqlParameter($"@LOC_M_CD", sLocMcd))
                    'params.Add(New SqlParameter($"@LOC_S_CD", sLocScd))

                    tblLoc = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                    'tblLoc = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                    '--2020/03/05 nv miyaji update end

                    sLocLNm = Misc.CommonRfid.ToStringNvl(tblLoc.Rows(0).Item("LocLName"))
                    sLocMNm = Misc.CommonRfid.ToStringNvl(tblLoc.Rows(0).Item("LocMName"))
                    sLocSNm = Misc.CommonRfid.ToStringNvl(tblLoc.Rows(0).Item("LocSName"))

                End If

                sbSQL.Clear()
                params = New List(Of SqlParameter)

                sbSQL.AppendLine("INSERT INTO T_OPERATION_LOG")
                sbSQL.AppendLine("(")
                sbSQL.AppendLine("     PROCESS_DT")
                sbSQL.AppendLine("    ,PROCESS_TM")
                sbSQL.AppendLine("    ,PROCESS_TOOL_DIV")
                sbSQL.AppendLine("    ,PROCESS_CAT")
                sbSQL.AppendLine("    ,LOC_L_CD")
                sbSQL.AppendLine("    ,LOC_M_CD")
                sbSQL.AppendLine("    ,LOC_S_CD")
                sbSQL.AppendLine("    ,LOC_L_NAME")
                sbSQL.AppendLine("    ,LOC_M_NAME")
                sbSQL.AppendLine("    ,LOC_S_NAME")
                sbSQL.AppendLine("    ,PROCESS_CONTENTS")
                sbSQL.AppendLine("    ,OPE_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,OPE_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,UPD_DATETIME")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,UPD_TASK")
                sbSQL.AppendLine(") VALUES (")
                sbSQL.AppendLine("     CONVERT(int, FORMAT(GetDate(), 'yyyyMMdd'))")
                sbSQL.AppendLine("    ,FORMAT(GetDate(), 'HHmmssfff')")
                sbSQL.AppendLine("    ,@PROCESS_TOOL_DIV")
                sbSQL.AppendLine("    ,@PROCESS_CAT")
                sbSQL.AppendLine("    ,@LOC_L_CD")
                sbSQL.AppendLine("    ,@LOC_M_CD")
                sbSQL.AppendLine("    ,@LOC_S_CD")
                sbSQL.AppendLine("    ,@LOC_L_NAME")
                sbSQL.AppendLine("    ,@LOC_M_NAME")
                sbSQL.AppendLine("    ,@LOC_S_NAME")
                sbSQL.AppendLine("    ,@PROCESS_CONTENTS")
                sbSQL.AppendLine("    ,@OPE_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@OPE_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,GetDate()")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@UPD_TASK")
                sbSQL.AppendLine(")")

                params.Add(New SqlParameter($"@PROCESS_TOOL_DIV", Misc.ProcessToolDiv.Web))
                params.Add(New SqlParameter($"@PROCESS_CAT", sProcessCat))
                params.Add(New SqlParameter($"@LOC_L_CD", sLocLcd))
                params.Add(New SqlParameter($"@LOC_M_CD", sLocMcd))
                params.Add(New SqlParameter($"@LOC_S_CD", sLocScd))

                params.Add(New SqlParameter($"@LOC_L_NAME", sLocLNm))
                params.Add(New SqlParameter($"@LOC_M_NAME", sLocMnm))
                params.Add(New SqlParameter($"@LOC_S_NAME", sLocSnm))

                If sProcessContents.Length > Misc.CommonConst.OpeLogContentsLen Then
                    sProcessContents = Misc.CommonConst.OpeLogContentsLen
                End If
                params.Add(New SqlParameter($"@PROCESS_CONTENTS", sProcessContents))

                If IsNothing(loginUser) Then
                    params.Add(New SqlParameter($"@OPE_EMPLOYEE_CD", String.Empty))
                    params.Add(New SqlParameter($"@OPE_EMPLOYEE_NAME", String.Empty))

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", String.Empty))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", String.Empty))
                Else
                    params.Add(New SqlParameter($"@OPE_EMPLOYEE_CD", loginUser.EmployeeCd))
                    params.Add(New SqlParameter($"@OPE_EMPLOYEE_NAME", loginUser.EmployeeNm))

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", loginUser.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", loginUser.EmployeeNm))
                End If
                params.Add(New SqlParameter($"@UPD_TASK", sTask))

                Dim ret As Integer = clsDB.SVFncExecuteNonQuery(sbSQL.ToString, params)

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)

                '操作ログの書き込みエラー時は、そのまま、流す
                'Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Sub
#End Region

    End Class
End Namespace