Imports Asetra.Models
Imports Asetra.Misc
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：薬品重量履歴関連ツール Data Access クラス
    ''' 作成日        ：2023/02/08      Kawamoto
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolDrugDueLog
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "薬品重量履歴の登録処理"
        ''' <summary>
        ''' 薬品重量履歴の登録処理
        ''' </summary>
        ''' <param name="drugWorkDiv">薬品作業区分</param>
        ''' <param name="assetNo">資産番号</param>
        ''' <param name="workerEmployeeCd">作業者社員コード</param>
        ''' <param name="workerEmployeeName">作業者名</param>
        ''' <param name="dueQty">返却重量</param>
        ''' <param name="remainQty">返却残量</param>
        ''' <param name="loginUser">ログイン情報</param>
        ''' <param name="sTask">処理名</param>
        Public Shared Sub InsDrugDueLog(ByVal drugWorkDiv As String,
                                        ByVal assetNo As String,
                                        ByVal workerEmployeeCd As String,
                                        ByVal workerEmployeeName As String,
                                        ByVal dueQty As String,
                                        ByVal remainQty As String,
                                        ByVal loginUser As RfidEntity.LoginUser,
                                        ByVal sTask As String,
                                        ByVal sBuyDate As String)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Try
                sbSQL.Clear()
                params = New List(Of SqlParameter)

                sbSQL.AppendLine("INSERT INTO T_DRUG_DUE_LOG")
                sbSQL.AppendLine("(")
                sbSQL.AppendLine("     DRUG_WORK_DIV")
                sbSQL.AppendLine("    ,PROCESS_DT")
                sbSQL.AppendLine("    ,PROCESS_TM")
                sbSQL.AppendLine("    ,ASSET_NO")
                sbSQL.AppendLine("    ,WORKER_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,WORKER_EMPLOYEE_NAME")       '2023/03/22 Kawamoto ADD
                sbSQL.AppendLine("    ,RESPONSE_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,RESPONSE_EMPLOYEE_NAME")     '2023/03/22 Kawamoto ADD
                sbSQL.AppendLine("    ,USE_QTY")
                sbSQL.AppendLine("    ,DUE_QTY")
                sbSQL.AppendLine("    ,REMAIN_QTY")
                sbSQL.AppendLine("    ,WORK_OUTLINE")
                sbSQL.AppendLine("    ,WORK_TIME")
                sbSQL.AppendLine("    ,REMARK1")
                sbSQL.AppendLine("    ,REMARK2")
                sbSQL.AppendLine("    ,UPD_DATETIME")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,UPD_TASK")
                sbSQL.AppendLine(") VALUES (")
                sbSQL.AppendLine("     @DRUG_WORK_DIV")
                sbSQL.AppendLine("    ,@BUYDATE")
                sbSQL.AppendLine("    ,'000000000'")
                sbSQL.AppendLine("    ,@ASSET_NO")
                sbSQL.AppendLine("    ,@WORKER_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@WORKER_EMPLOYEE_NAME")      '2023/03/22 Kawamoto ADD
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,NULL")                       '2023/03/22 Kawamoto ADD
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,@DUE_QTY")
                sbSQL.AppendLine("    ,@REMAIN_QTY")
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,GetDate()")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@UPD_TASK")
                sbSQL.AppendLine(")")

                params.Add(New SqlParameter($"@DRUG_WORK_DIV", drugWorkDiv))
                params.Add(New SqlParameter($"@BUYDATE", sBuyDate.Replace("-", "")))
                params.Add(New SqlParameter($"@ASSET_NO", assetNo))
                params.Add(New SqlParameter($"@WORKER_EMPLOYEE_CD", workerEmployeeCd))
                params.Add(New SqlParameter($"@WORKER_EMPLOYEE_NAME", workerEmployeeName))
                params.Add(New SqlParameter($"@DUE_QTY", Decimal.Parse(dueQty)))
                params.Add(New SqlParameter($"@REMAIN_QTY", Decimal.Parse(remainQty)))
                If IsNothing(loginUser) Then
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", String.Empty))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", String.Empty))
                Else
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", loginUser.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", loginUser.EmployeeNm))
                End If
                params.Add(New SqlParameter($"@UPD_TASK", sTask))

                Dim ret As Integer = clsDB.SVFncExecuteNonQuery(sbSQL.ToString, params)

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Sub
#End Region

#Region "薬品重量履歴(薬品作業区分：受入)の削除処理"
        ''' <summary>
        ''' 薬品重量履歴(薬品作業区分：受入)の削除処理
        ''' </summary>
        ''' <param name="db">SystemDBContext</param>
        ''' <param name="assetNo">資産番号</param>
        Public Shared Sub DelDrugDueLogAccept(ByRef db As SystemDBContext, ByVal assetNo As String)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Try

                sbSQL.AppendLine("DELETE FROM T_DRUG_DUE_LOG")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  ASSET_NO = @ASSET_NO")
                sbSQL.AppendLine("AND")
                sbSQL.AppendLine("  DRUG_WORK_DIV = '" & DrugsWorkDiv.Accept & "'")

                params = New List(Of SqlParameter)
                params.Add(New SqlParameter($"@ASSET_NO", assetNo))

                db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Sub
#End Region

#Region "薬品重量履歴情報(薬品作業区分：受入以外)取得"
        ''' <summary>
        ''' 薬品重量履歴(薬品作業区分：受入以外)情報取得
        ''' </summary>
        ''' <param name="assetNo">資産番号</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetDrugDueLogNotAccept(ByVal assetNo As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'T_DRUG_DUE_LOGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  *")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  T_DRUG_DUE_LOG")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  ASSET_NO = '" & assetNo & "'")
                sbSQL.AppendLine("AND")
                sbSQL.AppendLine(" (DRUG_WORK_DIV = '" & DrugsWorkDiv.Returned & "'")
                sbSQL.AppendLine("  OR DRUG_WORK_DIV = '" & DrugsWorkDiv.Confirm & "')")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

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

#Region "薬品重量履歴の登録処理（薬品作業区分：確認）"
        ''' <summary>
        ''' 薬品重量履歴の登録処理（薬品作業区分：確認）
        ''' </summary>
        ''' <param name="assetNo">資産番号</param>
        ''' <param name="loginUser">ログイン情報</param>
        ''' <param name="sTask">処理名</param>
        Public Shared Sub InsDrugDueLogDivCheck(ByVal assetNo As String,
                                                ByVal loginUser As RfidEntity.LoginUser,
                                                ByVal sTask As String)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Try
                sbSQL.Clear()
                params = New List(Of SqlParameter)

                sbSQL.AppendLine("INSERT INTO T_DRUG_DUE_LOG")
                sbSQL.AppendLine("(")
                sbSQL.AppendLine("     DRUG_WORK_DIV")
                sbSQL.AppendLine("    ,PROCESS_DT")
                sbSQL.AppendLine("    ,PROCESS_TM")
                sbSQL.AppendLine("    ,ASSET_NO")
                sbSQL.AppendLine("    ,WORKER_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,WORKER_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,RESPONSE_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,RESPONSE_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,USE_QTY")
                sbSQL.AppendLine("    ,DUE_QTY")
                sbSQL.AppendLine("    ,REMAIN_QTY")
                sbSQL.AppendLine("    ,WORK_OUTLINE")
                sbSQL.AppendLine("    ,WORK_TIME")
                sbSQL.AppendLine("    ,REMARK1")
                sbSQL.AppendLine("    ,REMARK2")
                sbSQL.AppendLine("    ,UPD_DATETIME")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,UPD_TASK")
                sbSQL.AppendLine(") ")
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine($"   {DrugsWorkDiv.Confirm}")
                sbSQL.AppendLine("    ,FORMAT(GetDate(), 'yyyyMMdd')")
                sbSQL.AppendLine("    ,FORMAT(GetDate(), 'HHmmssfff')")
                sbSQL.AppendLine("    ,LATEST_HIST.ASSET_NO")
                sbSQL.AppendLine("    ,LATEST_HIST.WORKER_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,LATEST_HIST.WORKER_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@RESPONSE_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@RESPONSE_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,LATEST_HIST.DUE_QTY")
                sbSQL.AppendLine("    ,LATEST_HIST.REMAIN_QTY")
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,NULL")
                sbSQL.AppendLine("    ,GetDate()")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@UPD_TASK ")
                sbSQL.AppendLine("FROM ")
                sbSQL.AppendLine("( ")
                sbSQL.AppendLine("     SELECT")
                sbSQL.AppendLine("          ASSET_NO")
                sbSQL.AppendLine("          ,WORKER_EMPLOYEE_CD")
                sbSQL.AppendLine("          ,WORKER_EMPLOYEE_NAME")
                sbSQL.AppendLine("          ,DUE_QTY")
                sbSQL.AppendLine("          ,REMAIN_QTY")
                sbSQL.AppendLine("          ,RANK() OVER (PARTITION BY ASSET_NO ORDER BY PROCESS_DT DESC, PROCESS_TM DESC) AS RANK")
                sbSQL.AppendLine("      FROM")
                sbSQL.AppendLine("          T_DRUG_DUE_LOG")
                sbSQL.AppendLine("      WHERE")
                sbSQL.AppendLine("          ASSET_NO = @ASSET_NO")
                sbSQL.AppendLine(") AS LATEST_HIST ")
                sbSQL.AppendLine("WHERE ")
                sbSQL.AppendLine("    LATEST_HIST.RANK = 1")

                params.Add(New SqlParameter($"@ASSET_NO", assetNo))
                If IsNothing(loginUser) Then
                    params.Add(New SqlParameter($"@RESPONSE_EMPLOYEE_CD", String.Empty))
                    params.Add(New SqlParameter($"@RESPONSE_EMPLOYEE_NAME", String.Empty))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", String.Empty))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", String.Empty))
                Else
                    params.Add(New SqlParameter($"@RESPONSE_EMPLOYEE_CD", loginUser.EmployeeCd))
                    params.Add(New SqlParameter($"@RESPONSE_EMPLOYEE_NAME", loginUser.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", loginUser.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", loginUser.EmployeeNm))
                End If
                params.Add(New SqlParameter($"@UPD_TASK", sTask))

                Dim ret As Integer = clsDB.SVFncExecuteNonQuery(sbSQL.ToString, params)

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Sub
#End Region

    End Class
End Namespace