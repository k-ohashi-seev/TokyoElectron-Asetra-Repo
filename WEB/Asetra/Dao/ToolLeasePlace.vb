Imports System.Data.SqlClient
Imports Asetra.Misc
Imports Asetra.Models

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：貸出先マスタ関連ツール クラス
    ''' 作成日        ：2022/10/03      Sato
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolLeasePlace
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "貸出先コード存在チェック処理"
        Public Shared Function ExistLeasePlaceCd(ByRef db As SystemDBContext, ByVal sLeasePlaceCd As String, Optional sSecLeasePlaseFlg As Boolean = True) As Boolean
            Dim ret As Boolean = False
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Try
                '社員コードがマスタに存在するかをチェックする
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM M_LEASE_PLACE ")
                sbSQL.AppendLine("WHERE LEASE_PLACE_CD = @LEASE_PLACE_CD ")
                '2023/03/22 Kawamoto ADD START
                If Not sSecLeasePlaseFlg Then
                    sbSQL.AppendLine("AND SEC_LEASE_PLACE_FLG = '" & SecLeasePlaceFlg.Cd.Yes & "'")
                End If
                '2023/03/22 Kawamoto ADD END
                params.Add(New SqlParameter($"@LEASE_PLACE_CD", CommonRfid.ToStringNvl(sLeasePlaceCd)))

                Dim leaseRecords As List(Of LeasePlaceModel) = db.LeasePlace.SqlQuery(sbSQL.ToString, params.ToArray).ToList
                If Not IsNothing(leaseRecords) AndAlso leaseRecords.Count > 0 Then
                    ret = True
                End If

                Return ret
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            End Try

            Return ret

        End Function
#End Region

    End Class
End Namespace