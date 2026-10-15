Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：ローケーション関連ツール系 Data Access クラス
    ''' 作成日        ：2019/08/14      Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolAsetraVer
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "ASETRAバージョンテーブルデータ取得"
        ''' <summary>
        ''' ASETRAバージョンテーブルデータ取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetAsetraVerInfo() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT TOP 1 *")
                sbSQL.AppendLine("FROM M_ASETRA_VER")

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

#Region "LICENSE_KEY更新"
        ''' <summary>
        ''' LICENSE_KEY更新
        ''' </summary>
        ''' <param name="sLicenseKey">LICENSE_KEY</param>
        Public Shared Sub UpdLicenseKey(ByVal sLicenseKey As String)

            '設定マスタの発番データ取得
            Dim tbl As New DataTable


            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            clsDB.SVFncStartTransaction()

            Try
                sbSQL.AppendLine("UPDATE M_ASETRA_VER SET ")
                sbSQL.AppendLine("  LICENSE_KEY = @LICENSE_KEY")
                params.Add(New SqlParameter($"@LICENSE_KEY", sLicenseKey))

                clsDB.SVFncExecuteNonQuery(sbSQL.ToString, params)

                clsDB.SVFncFinishTransaction(True)


            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                clsDB.SVFncFinishTransaction(False)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Sub
#End Region

    End Class
End Namespace