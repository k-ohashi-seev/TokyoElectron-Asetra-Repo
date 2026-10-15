Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：ゲート関連ツール系 Data Access クラス
    ''' 作成日        ：2020/02/20      nv miyaji
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolGate

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "ゲートマスタ・アンテナマスタ・サーモマスタデータ取得"
        ''' <summary>
        ''' ゲートマスタ・アンテナマスタ・サーモマスタデータ取得データ取得
        ''' </summary>
        ''' <param name="sGateCd">ゲートコード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetGateData(ByVal GateDiv As String, ByVal sGateCd As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_GATEテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT GATE.*")
                sbSQL.AppendLine("       ,ANTENNA1.ANTENNA_VALID_FLAG As ANTENNA1_VALID_FLAG")
                sbSQL.AppendLine("       ,ANTENNA1.ANTENNA_RF_POWER As ANTENNA1_RF_POWER")
                sbSQL.AppendLine("       ,ANTENNA2.ANTENNA_VALID_FLAG As ANTENNA2_VALID_FLAG")
                sbSQL.AppendLine("       ,ANTENNA2.ANTENNA_RF_POWER As ANTENNA2_RF_POWER")
                sbSQL.AppendLine("       ,ANTENNA3.ANTENNA_VALID_FLAG As ANTENNA3_VALID_FLAG")
                sbSQL.AppendLine("       ,ANTENNA3.ANTENNA_RF_POWER As ANTENNA3_RF_POWER")
                sbSQL.AppendLine("       ,ANTENNA4.ANTENNA_VALID_FLAG As ANTENNA4_VALID_FLAG")
                sbSQL.AppendLine("       ,ANTENNA4.ANTENNA_RF_POWER As ANTENNA4_RF_POWER")
                sbSQL.AppendLine("       ,THERMO1.THERMO_VALID_FLAG As THERMO1_VALID_FLAG")
                sbSQL.AppendLine("       ,THERMO2.THERMO_VALID_FLAG As THERMO2_VALID_FLAG")
                sbSQL.AppendLine("FROM M_GATE GATE")
                sbSQL.AppendLine("LEFT JOIN M_GATE_ANTENNA ANTENNA1")
                sbSQL.AppendLine("ON ANTENNA1.GATE_CD = GATE.GATE_CD")
                sbSQL.AppendLine("AND ANTENNA1.ANTENNA_NO = 1")
                sbSQL.AppendLine("AND ANTENNA1.GATE_DIV = GATE.GATE_DIV")       '20220328 ADD Kawamoto v2.1
                sbSQL.AppendLine("LEFT JOIN M_GATE_ANTENNA ANTENNA2")
                sbSQL.AppendLine("ON ANTENNA2.GATE_CD = GATE.GATE_CD")
                sbSQL.AppendLine("AND ANTENNA2.ANTENNA_NO = 2")
                sbSQL.AppendLine("AND ANTENNA2.GATE_DIV = GATE.GATE_DIV")       '20220328 ADD Kawamoto v2.1
                sbSQL.AppendLine("LEFT JOIN M_GATE_ANTENNA ANTENNA3")
                sbSQL.AppendLine("ON ANTENNA3.GATE_CD = GATE.GATE_CD")
                sbSQL.AppendLine("AND ANTENNA3.ANTENNA_NO = 3")
                sbSQL.AppendLine("AND ANTENNA3.GATE_DIV = GATE.GATE_DIV")       '20220328 ADD Kawamoto v2.1
                sbSQL.AppendLine("LEFT JOIN M_GATE_ANTENNA ANTENNA4")
                sbSQL.AppendLine("ON ANTENNA4.GATE_CD = GATE.GATE_CD")
                sbSQL.AppendLine("AND ANTENNA4.ANTENNA_NO = 4")
                sbSQL.AppendLine("AND ANTENNA4.GATE_DIV = GATE.GATE_DIV")       '20220328 ADD Kawamoto v2.1
                sbSQL.AppendLine("LEFT JOIN M_GATE_THERMO THERMO1")
                sbSQL.AppendLine("ON THERMO1.GATE_CD = GATE.GATE_CD")
                sbSQL.AppendLine("AND THERMO1.THERMO_NO = 1")
                sbSQL.AppendLine("LEFT JOIN M_GATE_THERMO THERMO2")
                sbSQL.AppendLine("ON THERMO2.GATE_CD = GATE.GATE_CD")
                sbSQL.AppendLine("AND THERMO2.THERMO_NO = 2")
                '--2020/03/05 nv miyaji update start
                sbSQL.AppendLine("WHERE GATE.GATE_CD = @GATE_CD ")
                sbSQL.AppendLine("AND GATE.GATE_DIV = @GATE_DIV")               '20220328 ADD Kawamoto v2.1
                params.Add(New SqlParameter($"@GATE_CD", CommonRfid.ToStringNvl(sGateCd)))
                params.Add(New SqlParameter($"@GATE_DIV", GateDiv))             '20220328 ADD Kawamoto v2.1
                'sbSQL.AppendLine("WHERE GATE.GATE_CD = '" & sGateCd & "'")


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


    End Class
End Namespace
