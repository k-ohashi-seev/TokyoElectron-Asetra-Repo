Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：送り状関連ツール系 Data Access クラス
    ''' 作成日        ：2021/10/17      nv Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolLendingLabel
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


#Region "貸出ラベルデータ取得"

        ''' <summary>
        ''' 貸出ラベルデータ取得
        ''' </summary>
        ''' <param name="sLstReserveId">予約IDのコンマ区切り文字列</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLeaseLabelData(sLstReserveId As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("   T_RESERVE_DETAIL.RESERVE_ID")
                sbSQL.AppendLine("  ,T_RESERVE_DETAIL.ASSET_NO")
                sbSQL.AppendLine("  ,M_ASSET.ASSET_NAME")
                sbSQL.AppendLine("  ,M_ASSET.FREE_ITEM10")
                sbSQL.AppendLine("  ,T_RESERVE.USER_END_DT")
                sbSQL.AppendLine("  ,M_DEPARTMENT.DEPARTMENT_NAME")
                sbSQL.AppendLine("  ,M_EMPLOYEE.EMPLOYEE_NAME")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL")
                sbSQL.AppendLine("INNER JOIN T_RESERVE")
                sbSQL.AppendLine("ON   T_RESERVE.RESERVE_ID = T_RESERVE_DETAIL.RESERVE_ID")
                sbSQL.AppendLine("INNER JOIN M_ASSET")
                sbSQL.AppendLine("ON   M_ASSET.ASSET_NO = T_RESERVE_DETAIL.ASSET_NO")
                sbSQL.AppendLine("INNER JOIN M_EMPLOYEE")
                sbSQL.AppendLine("ON   M_EMPLOYEE.EMPLOYEE_CD = T_RESERVE.CHARGE_EMP_CD")
                sbSQL.AppendLine("INNER JOIN M_DEPARTMENT")
                sbSQL.AppendLine("ON   M_DEPARTMENT.DEPARTMENT_CD = M_EMPLOYEE.DEPARTMENT_CD")
                sbSQL.AppendLine("WHERE T_RESERVE_DETAIL.RESERVE_ID IN (" & sLstReserveId & ")")
                sbSQL.AppendLine("ORDER BY T_RESERVE.REAL_START_DT DESC, T_RESERVE.RESERVE_ID, T_RESERVE_DETAIL.ASSET_NO")
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("RESERVE_ID"), tbl.Columns("ASSET_NO")}

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

    End Class
End Namespace
