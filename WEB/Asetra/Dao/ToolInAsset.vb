Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Asetra.Misc
Imports System.Data.SqlClient

Namespace Dao
    Public Class ToolInAsset
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        ''' <summary>
        ''' カテゴリ大分類コードが設定マスタにあるレコードを取得する
        ''' </summary>
        ''' <param name="sWhereSQL">抽出条件</param>
        ''' <param name="sParam">パラメーター</param>
        Public Shared Function GetInAssetTangible(ByVal sWhereSQL As String, ByVal sParam As List(Of SqlParameter)) As List(Of String)
            Dim tbl As New DataTable
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                '有形資産取得SQL
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("    INA.ASSET_NO ")
                sbSQL.AppendLine("  , INA.CAT_L_CD ")
                sbSQL.AppendLine("FROM ")
                sbSQL.AppendLine("  T_IN_ASSET INA ")
                sbSQL.AppendLine(" INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC")
                sbSQL.AppendLine(" ON INA.MNG_LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.AppendLine(" AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = INA.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
                sbSQL.AppendLine(" WHERE EXISTS ( ")
                sbSQL.AppendLine(" SELECT KEY1 FROM M_SETTING MST ")
                sbSQL.AppendLine($" WHERE KEY_DIV ='{CommonSettingKey.TangibleCategory.KeyDiv}' ")
                sbSQL.AppendLine($" AND KEY1 = INA.CAT_L_CD ")
                sbSQL.AppendLine($" AND KEY2 = '{CommonSettingKey.TangibleCategory.Key2}' ")
                sbSQL.AppendLine($" AND KEY3 = '{CommonSettingKey.TangibleCategory.Key3}' ")
                sbSQL.AppendLine(") ")
                sbSQL.AppendLine(sWhereSQL)

                'パラメーターをコピーする(SqlParameterがSqlParameterCollectionに含まれるエラー回避のため)
                Dim lstParamCopy As New List(Of SqlParameter)
                Dim param As New SqlParameter
                For Each param In sParam
                    lstParamCopy.Add(New SqlParameter(param.ParameterName, param.Value))
                Next

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, lstParamCopy)

                'リストに追加
                Dim lst As New List(Of String)
                For Each r As DataRow In tbl.Rows
                    Dim asset = r.Field(Of String)("ASSET_NO")
                    Dim catL = r.Field(Of String)("CAT_L_CD")
                    lst.Add(asset & "," & catL)
                Next

                Return lst

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return Nothing

        End Function
    End Class
End Namespace
