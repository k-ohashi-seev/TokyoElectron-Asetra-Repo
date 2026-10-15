Imports Asetra.Misc
Imports System.IO

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：CSVヘッダDAOツール クラス
    ''' 作成日        ：2024/11/07      Iseda
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolCSVHeader

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "CSVヘッダ並び順取得"
        ''' <summary>
        ''' CSVヘッダ並び順取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetCSVHeaderOrder(ByVal prmFunctionId As String) As List(Of String)
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            Dim lstCsvHeaderOrderList As New List(Of String)

            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CSVHEADER_ORDERテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append(" WORD_CD")
                sbSQL.Append(" FROM  M_CSVHEADER_ORDER ")
                sbSQL.Append(" WHERE  FUNCTION_ID = '" & prmFunctionId & "'")
                sbSQL.Append(" ORDER BY RANK")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                For Each row As DataRow In tbl.Rows
                    lstCsvHeaderOrderList.Add(row("WORD_CD").ToString)
                Next

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return lstCsvHeaderOrderList

        End Function
#End Region


    End Class
End Namespace