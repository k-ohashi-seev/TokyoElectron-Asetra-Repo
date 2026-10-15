Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：部署関連ツール系 Data Access クラス
    ''' 作成日        ：2021/10/11      Suzuki
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolGoods

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "商品データ取得"
        ''' <summary>
        ''' 商品データ取得
        ''' </summary>
        ''' <param name="sProductNo">品番</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetGoodsData(ByVal sProductNo As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_DEPARTMENTテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM M_GOODS ")
                If Not String.IsNullOrEmpty(sProductNo) Then
                    sbSQL.AppendLine("WHERE PRODUCT_NO = @PRODUCT_NO ")
                    params.Add(New SqlParameter($"@PRODUCT_NO", sProductNo))
                End If

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

    End Class
End Namespace