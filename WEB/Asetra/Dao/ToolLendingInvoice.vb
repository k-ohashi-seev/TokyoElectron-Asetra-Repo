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
    Public Class ToolLendingInvoice
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "送り状マッピングデータ取得"

        ''' <summary>
        ''' 送り状マッピングデータ取得
        ''' </summary>
        ''' <param name="sInvoiceDiv">送り状区分</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetInvoiceMapData(ByVal sInvoiceDiv As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT M_INVOICEMAP.*")
                sbSQL.AppendLine("FROM M_INVOICEMAP")
                sbSQL.AppendLine("WHERE M_INVOICEMAP.INVOICE_DIV = @InvoiceDiv")
                sbSQL.AppendLine("ORDER BY M_INVOICEMAP.INVOICE_DIV, M_INVOICEMAP.INVOICE_NO")
                params.Add(New SqlParameter($"@InvoiceDiv", CommonRfid.ToStringNvl(sInvoiceDiv)))

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

#Region "送り状出力用予約データ取得"

        ''' <summary>
        ''' 送り状出力用予約データ取得
        ''' </summary>
        ''' <param name="sLstReserveId">予約IDのコンマ区切り文字列</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetInvoiceReserveData(sLstReserveId As String) As DataTable
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
                sbSQL.AppendLine("   T_RESERVE.RESERVE_ID")
                sbSQL.AppendLine("  ,T_RESERVE.DEST_PHONE_NUMBER")
                sbSQL.AppendLine("  ,T_RESERVE.DEST_ZIP_CODE")
                sbSQL.AppendLine("  ,T_RESERVE.DEST_ADDRESS1")
                sbSQL.AppendLine("  ,T_RESERVE.DEST_ADDRESS2")
                sbSQL.AppendLine("  ,T_RESERVE.DEST_ADDRESS3")
                sbSQL.AppendLine("  ,T_RESERVE.DEST_NAME1")
                sbSQL.AppendLine("  ,T_RESERVE.DEST_NAME2")
                sbSQL.AppendLine("  ,M_EMPLOYEE.EMPLOYEE_NAME")
                sbSQL.AppendLine("  ,M_DEPARTMENT.PHONE_NUMBER")
                sbSQL.AppendLine("  ,M_DEPARTMENT.ZIP_CODE")
                sbSQL.AppendLine("  ,M_DEPARTMENT.ADDRESS")
                sbSQL.AppendLine("FROM T_RESERVE")
                sbSQL.AppendLine("INNER JOIN M_EMPLOYEE")
                sbSQL.AppendLine("ON   M_EMPLOYEE.EMPLOYEE_CD = T_RESERVE.CHARGE_EMP_CD")
                sbSQL.AppendLine("INNER JOIN M_DEPARTMENT")
                sbSQL.AppendLine("ON   M_DEPARTMENT.DEPARTMENT_CD = M_EMPLOYEE.DEPARTMENT_CD")
                sbSQL.AppendLine("WHERE T_RESERVE.RESERVE_ID IN (" & sLstReserveId & ")")
                sbSQL.AppendLine("ORDER BY T_RESERVE.REAL_START_DT DESC, T_RESERVE.RESERVE_ID")
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

#Region "送り状出力用予約資産データ取得"

        ''' <summary>
        ''' 送り状出力用予約資産データ取得
        ''' </summary>
        ''' <param name="sLstReserveId">予約IDのコンマ区切り文字列</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetInvoiceReserveAssetData(sLstReserveId As String) As DataTable
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
                sbSQL.AppendLine("  ,T_RESERVE_DETAIL.REAL_START_DT")
                sbSQL.AppendLine("  ,M_ASSET.ASSET_NAME")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL")
                sbSQL.AppendLine("INNER JOIN M_ASSET")
                sbSQL.AppendLine("ON   M_ASSET.ASSET_NO = T_RESERVE_DETAIL.ASSET_NO")
                sbSQL.AppendLine("WHERE T_RESERVE_DETAIL.RESERVE_ID IN (" & sLstReserveId & ")")
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("RESERVE_ID"), tbl.Columns("ASSET_NO"), tbl.Columns("REAL_START_DT")}

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

#Region "送り状用文字列切り出し編集"
        ''' <summary>
        ''' 送り状用文字列切り出し編集
        ''' </summary>
        ''' <param name="sStr">対象文字列</param>
        ''' <param name="iStart">開始位置  1 start</param>
        ''' <param name="iEnd">終了位置 1 start</param>
        ''' <returns>
        ''' 切り出し編集後文字列
        ''' </returns>
        Public Shared Function InvoiceSubString(ByVal sStr As String, ByVal iStart As Integer, ByVal iEnd As Integer) As String

            Dim iLen As Integer = (iEnd - iStart) + 1

            If iStart > sStr.Length Then
                Return String.Empty
            End If

            Dim sRet As String = sStr.Substring(iStart - 1)

            If sRet.Length >= iLen And iLen <> 0 Then
                sRet = sRet.Substring(0, iLen)
            End If

            Return sRet
        End Function
#End Region

    End Class
End Namespace
