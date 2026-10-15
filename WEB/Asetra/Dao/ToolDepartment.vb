Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：部署関連ツール系 Data Access クラス
    ''' 作成日        ：2019/09/05      Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolDepartment

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "部署データ取得"
        ''' <summary>
        ''' 部署データ取得
        ''' </summary>
        ''' <param name="sDepartmentCd">部署コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetDepartmentData(ByVal sDepartmentCd As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_DEPARTMENTテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM M_DEPARTMENT ")
                If Not String.IsNullOrEmpty(sDepartmentCd) Then
                    '--2020/03/05 nv miyaji update start
                    sbSQL.AppendLine("WHERE DEPARTMENT_CD = @DEPARTMENT_CD ")
                    params.Add(New SqlParameter($"@DEPARTMENT_CD", sDepartmentCd))
                    'sbSQL.AppendLine("WHERE DEPARTMENT_CD = '" & sDepartmentCd & "' ")
                    '--2020/03/05 nv miyaji update end
                End If

                '--2020/03/05 nv miyaji update start
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

#Region "部署使用チェック"
        ''' <summary>
        ''' 部署使用チェック(指定コードが、引数のもの以外のロケーションＭ内、資産Ｍに存在するか、否か)
        ''' </summary>
        ''' <param name="sDepartmentCd">部署コード</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsUseDepartment(ByVal sDepartmentCd As String) As Boolean
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try

                sbSQL.AppendLine("SELECT DISTINCT EMPLOYEE_CD ")
                sbSQL.AppendLine("FROM M_EMPLOYEE ")
                '--2020/03/05 nv miyaji update start
                sbSQL.AppendLine("WHERE DEPARTMENT_CD = @DEPARTMENT_CD ")
                params.Add(New SqlParameter($"@DEPARTMENT_CD", CommonRfid.ToStringNvl(sDepartmentCd)))
                'sbSQL.AppendLine("WHERE DEPARTMENT_CD = '" & sDepartmentCd & "'")
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count > 0 Then
                    Return True
                End If

                '未使用の場合
                Return False

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

    End Class
End Namespace