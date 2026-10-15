Imports Asetra.Misc
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：お知らせダウンロード関連ツール系 Data Access クラス
    ''' 作成日        ：2023/03/23      Kawamoto
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolNewsDownload
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


#Region "お知らせダウンロードデータ(全件)取得"

        ''' <summary>
        ''' お知らせダウンロードデータ(全件)取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetNewsDownloadDataAll() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("SELECT ")
                sbSQL.Append("  NEWS_CD ")
                sbSQL.Append("  , DISPLAY_NAME ")
                sbSQL.Append("  , DOWNLOAD_URL ")
                sbSQL.Append(" FROM ")
                sbSQL.Append("  T_NEWS_DOWNLOAD ")
                sbSQL.Append(" ORDER BY ")
                sbSQL.Append("  NEWS_CD ")
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

    End Class
End Namespace
