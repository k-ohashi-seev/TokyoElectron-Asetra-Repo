Imports Asetra.Misc
Imports System.IO

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：設定DAOツール クラス
    ''' 作成日        ：2019/08/25      Kusunoki
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolSetting

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "プリンタ情報取得"
        ''' <summary>
        ''' プリンタ情報取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetMstPrinterInfo() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.LabelPrinter.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.LabelPrinter.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.LabelPrinter.Key3 & "'")
                '--2020/02/06 miyaji update start
                'sbSQL.AppendLine("ORDER BY KEY1")
                sbSQL.AppendLine("ORDER BY CONVERT(int,DATA10)")
                '--2020/02/06 miyaji update end
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

#Region "送り状作成情報取得"
        ''' <summary>
        ''' 送り状作成情報取得
        ''' </summary>
        ''' <returns>DataTable</returns>
        Public Shared Function GetLendingInvoiceInfo() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("  , DATA3")
                sbSQL.AppendLine("  , DATA4")
                sbSQL.AppendLine("  , DATA5")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.Lending_Invoice_Output.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.Lending_Invoice_Output.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.Lending_Invoice_Output.Key3 & "'")
                sbSQL.AppendLine("ORDER BY CONVERT(int,DATA10)")
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

#Region "予約確認ラベル情報取得"
        ''' <summary>
        ''' 予約確認ラベル情報取得
        ''' </summary>
        ''' <returns>DataTable</returns>
        Public Shared Function GetLendingLabelInfo() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("  , DATA3")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.Lending_Label_Output.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.Lending_Label_Output.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.Lending_Label_Output.Key3 & "'")
                sbSQL.AppendLine("ORDER BY CONVERT(int,DATA10)")
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

#Region "書込みタグEPC領域サイズ取得"
        ''' <summary>
        ''' 書込みタグEPC領域サイズ取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetTagSizeData() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.TagSize.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.TagSize.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.TagSize.Key3 & "'")
                sbSQL.AppendLine("ORDER BY KEY1")

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

#Region "カテゴリマスタの生成資産IDの選択初期値データ取得"
        ''' <summary>
        ''' カテゴリマスタの生成資産IDの選択初期値データ取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetAssetIDGenerationMethodData() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.AssetIDGenerationMethod.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.AssetIDGenerationMethod.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.AssetIDGenerationMethod.Key3 & "'")
                sbSQL.AppendLine("ORDER BY KEY1")

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

#Region "資産ID生成方法ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 資産ID生成方法ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetAssetIDGenerationDropDownData() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  KEY1 AS Cd")
                sbSQL.AppendLine("  , '' AS Name")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.AssetIDGenerationUsage.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.AssetIDGenerationUsage.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.AssetIDGenerationUsage.Key3 & "'")
                sbSQL.AppendLine("  AND DATA1 = '" & AssetIDGenerationUsageType.Yes & "'")
                sbSQL.AppendLine("ORDER BY DATA10")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                '名称列の設定
                For Each row As DataRow In tbl.Rows
                    Dim sName As String = String.Empty
                    Select Case Integer.Parse(row("Cd"))
                        Case AssetIDGenerationMethod.None
                            sName = Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_NONE)
                        Case AssetIDGenerationMethod.AssetNo
                            sName = Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_ASSET_NO)
                        Case AssetIDGenerationMethod.Auto
                            sName = Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_AUTO)
                        Case AssetIDGenerationMethod.Inp
                            sName = Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_INP)
                        Case AssetIDGenerationMethod.GIAI
                            sName = Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_GIAI)
                        Case AssetIDGenerationMethod.GRAI
                            sName = Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_GRAI)
                    End Select
                    row("Name") = sName
                Next

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

#Region "EPC事業所キードロップダウンリストデータ取得"
        ''' <summary>
        ''' EPC事業所キードロップダウンリストデータ取得
        ''' </summary>
        ''' <param name="assetIdRegDiv">資産ID生成方法</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetEpcCompanyKeyDropDownData(assetIdRegDiv As Byte?) As DataTable
            Dim tbl As New DataTable

            If assetIdRegDiv.HasValue = False Then
                Return tbl
            End If

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  KEY2 AS Cd")
                sbSQL.AppendLine("  , DATA2 AS Name")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.AssetIDGenerationCoPrefix.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY1 = '" & assetIdRegDiv & "'")
                sbSQL.AppendLine("ORDER BY DATA10")

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

#Region "EPC事業所ID取得"
        ''' <summary>
        ''' EPC事業所ID取得
        ''' </summary>
        ''' <param name="key2">キー2</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetEpcCompanyId(key2 As String) As String
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  DATA1")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.AssetIDGenerationCoPrefix.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY1 = '" & AssetIDGenerationMethod.GRAI & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & key2 & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                If tbl.Rows.Count = 0 Then
                    Return ""
                Else
                    Return tbl.Rows(0).Item("DATA1")
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "商品マスタの資産反映項目データ取得"
        ''' <summary>
        ''' 商品マスタの資産反映項目データ取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetGoodsReflectItemData() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  DATA1 ")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.GoodsReflectItem.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.GoodsReflectItem.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.GoodsReflectItem.Key3 & "'")
                sbSQL.AppendLine("  AND DATA2 = '" & AssetIDGenerationUsageType.Yes & "'")

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

        '20220328 ADD Start Kawamoto v2.1
#Region "使用ゲート取得処理"
        ''' <summary>
        ''' 使用ゲート取得処理
        ''' </summary>
        Public Shared Function GetUseGate(ByVal Setting As MessageRfid) As String

            Dim strUseGate As String = GateDiv.Cd.WG

            Dim settingInfo As SettingInfo = Setting.getSettingInfo(CommonSettingKey.Gate_Setting.KeyDiv, CommonSettingKey.Gate_Setting.Key1, CommonSettingKey.Gate_Setting.Key2, CommonSettingKey.Gate_Setting.Key3)
            If Not settingInfo Is Nothing Then
                If IsNothing(settingInfo.DATA2) Then
                    Return strUseGate
                End If
                If settingInfo.DATA2.Trim = GateDiv.Name.SW Then
                    strUseGate = GateDiv.Cd.SW
                Else
                    strUseGate = GateDiv.Cd.WG
                End If
            End If

            Return strUseGate
        End Function
#End Region
        '20220328 ADD End Kawamoto v2.1

'2023/02/08 Kawamoto ADD START
#Region "危険物の指定数量情報取得"
        ''' <summary>
        ''' 危険物の指定数量情報取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetDrugsFSActNumberInfo() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("  , DATA3")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.Drugs_FSAct_Number.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.Drugs_FSAct_Number.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.Drugs_FSAct_Number.Key3 & "'")
                sbSQL.AppendLine("ORDER BY CONVERT(int,KEY1)")
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

#Region "危険物の指定数量の存在チェック"
        ''' <summary>
        ''' 危険物の指定数量の存在チェック
        ''' <param name="prmProperty">性質</param>
        ''' <returns>True:チェックOK False:チェックNG</returns>
        ''' </summary>
        Public Shared Function ChkExistDrugsFSActNumber(ByVal prmProperty As String) As Boolean
            Dim isRet As Boolean = False

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  *")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.Drugs_FSAct_Number.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.Drugs_FSAct_Number.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.Drugs_FSAct_Number.Key3 & "'")
                sbSQL.AppendLine("  AND DATA1 = '" & prmProperty & "'")
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                If tbl.Rows.Count > 0 Then
                    isRet = True
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return isRet
        End Function
#End Region

        '#Region "危険物の指定数量の単位(DATA3)の取得"
        '        ''' <summary>
        '        ''' 危険物の指定数量の単位(DATA3)の取得
        '        ''' <param name="prmProperty">性質</param>
        '        ''' <returns>True:チェックOK False:チェックNG</returns>
        '        ''' </summary>
        '        Public Shared Function GetDrugsFSActNumberUnit(ByVal prmProperty As String) As String
        '            Dim isRet As String = String.Empty

        '            Dim sbSQL As StringBuilder = New StringBuilder
        '            Dim clsDB As clsSV_DB = New clsSV_DB
        '            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
        '                '接続失敗
        '                Return Nothing
        '            End If
        '            Try
        '                'M_SETTINGテーブルから取得するSQLを作成
        '                sbSQL.AppendLine("SELECT ")
        '                sbSQL.AppendLine("  DATA3")
        '                sbSQL.AppendLine("FROM")
        '                sbSQL.AppendLine("  M_SETTING")
        '                sbSQL.AppendLine("WHERE")
        '                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.Drugs_FSAct_Number.KeyDiv & "'")
        '                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.Drugs_FSAct_Number.Key2 & "'")
        '                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.Drugs_FSAct_Number.Key3 & "'")
        '                sbSQL.AppendLine("  AND DATA1 = '" & prmProperty & "'")
        '                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)

        '                If tbl.Rows.Count > 0 Then
        '                    isRet = tbl.Rows(0).Item("DATA3")
        '                End If

        '            Catch ex As Exception
        '                logger.Error(ex.Message)
        '                logger.Error(sbSQL)
        '                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        '            Finally
        '                'DB CLOSE
        '                clsDB.SVSubCloseConnection()
        '            End Try

        '            Return isRet
        '        End Function
        '#End Region

#Region "計量器情報取得"
        ''' <summary>
        ''' 計量器情報取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetDrugsWeighingMachineInfo() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("  , DATA3")
                sbSQL.AppendLine("  , DATA4")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.Drugs_Weighing_Machine.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.Drugs_Weighing_Machine.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.Drugs_Weighing_Machine.Key3 & "'")
                sbSQL.AppendLine("  AND DATA2 = '" & Exist.Yes & "'")
                sbSQL.AppendLine("ORDER BY CONVERT(int,KEY1)")
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

#Region "計量器連携あり情報（リスト）の取得"
        ''' <summary>
        ''' 計量器連携あり情報（リスト）の取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetWeighingMachineLinkList() As List(Of String)
            
            GetWeighingMachineLinkList = New List(Of String)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("  , DATA3")
                sbSQL.AppendLine("  , DATA4")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.Drugs_Weighing_Machine.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.Drugs_Weighing_Machine.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.Drugs_Weighing_Machine.Key3 & "'")
                sbSQL.AppendLine("  AND DATA1 = '" & Exist.Yes & "'")
                sbSQL.AppendLine("  AND DATA2 = '" & Exist.Yes & "'")
                sbSQL.AppendLine("ORDER BY CONVERT(int,KEY1)")

                Dim tbl As New DataTable
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                For Each row As DataRow In tbl.Rows
                    'リストに追加
                    GetWeighingMachineLinkList.Add(row.Item("DATA3"))
                Next

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return GetWeighingMachineLinkList

        End Function
#End Region

#Region "計量器の存在チェック"
        ''' <summary>
        ''' 計量器の存在チェック
        ''' <param name="prmMachine">計量機</param>
        ''' <returns>True:チェックOK False:チェックNG</returns>
        ''' </summary>
        Public Shared Function ChkExistDrugsWeighingMachine(ByVal prmMachine As String) As Boolean
            Dim isRet As Boolean = False

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  *")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.Drugs_Weighing_Machine.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.Drugs_Weighing_Machine.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.Drugs_Weighing_Machine.Key3 & "'")
                sbSQL.AppendLine("  AND DATA3 = '" & prmMachine & "'")
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                If tbl.Rows.Count > 0 Then
                    isRet = True
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return isRet
        End Function
#End Region
        '2023/02/08 Kawamoto ADD END

#Region "工事一覧検索条件情報取得"
        ''' <summary>
        ''' 工事一覧検索条件情報取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetConstructionSearchItemInfo() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("  , DATA3")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.ConstructionSearchItem.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.ConstructionSearchItem.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.ConstructionSearchItem.Key3 & "'")
                sbSQL.AppendLine("ORDER BY KEY1")
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

#Region "工事一覧入力設定情報取得"
        ''' <summary>
        ''' 工事一覧入力設定情報取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetConstructionInputItemInfo() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("  , DATA3")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.ConstructionInputItem.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.ConstructionInputItem.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.ConstructionInputItem.Key3 & "'")
                sbSQL.AppendLine("ORDER BY KEY1")
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

#Region "仮置一覧入力設定情報取得"
        ''' <summary>
        ''' 仮置一覧入力設定情報取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetTemporaryInputItemInfo() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("  , DATA3")
                sbSQL.AppendLine("  , DATA4")
                sbSQL.AppendLine("  , DATA5")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & CommonSettingKey.TemporaryInputItem.KeyDiv & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & CommonSettingKey.TemporaryInputItem.Key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & CommonSettingKey.TemporaryInputItem.Key3 & "'")
                sbSQL.AppendLine("ORDER BY KEY1")
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

#Region "検索条件プルダウン情報取得"
        ''' <summary>
        ''' 検索条件プルダウン情報取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetItemDropDownInfo(keyDiv As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  KEY_DIV")
                sbSQL.AppendLine("  , KEY1")
                sbSQL.AppendLine("  , KEY2")
                sbSQL.AppendLine("  , KEY3")
                sbSQL.AppendLine("  , DATA1")
                sbSQL.AppendLine("  , DATA2")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & keyDiv & "'")
                sbSQL.AppendLine("ORDER BY KEY1")
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

#Region "M_SETTINGの1レコードの取得"
        ''' <summary>
        ''' M_SETTINGの1レコードの取得
        ''' <returns>設定情報、取得できなかったらNothing</returns>
        ''' </summary>
        Public Shared Function GetSetting(keyDiv As String, key1 As String, key2 As String, key3 As String) As SettingInfo

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            Dim ret = Nothing
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  *")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & keyDiv & "'")
                sbSQL.AppendLine("  AND KEY1 = '" & key1 & "'")
                sbSQL.AppendLine("  AND KEY2 = '" & key2 & "'")
                sbSQL.AppendLine("  AND KEY3 = '" & key3 & "'")

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)


                If tbl.Rows.Count > 0 Then
                    Dim settingInfo = New SettingInfo()
                    Dim r = tbl.Rows(0)
                    settingInfo.KeyDiv = r.Field(Of String)("KEY_DIV")
                    settingInfo.Key1 = r.Field(Of String)("KEY1")
                    settingInfo.Key2 = r.Field(Of String)("KEY2")
                    settingInfo.Key3 = r.Field(Of String)("KEY3")
                    settingInfo.DATA1 = r.Field(Of String)("DATA1")
                    settingInfo.DATA2 = r.Field(Of String)("DATA2")
                    settingInfo.DATA3 = r.Field(Of String)("DATA3")
                    settingInfo.DATA4 = r.Field(Of String)("DATA4")
                    settingInfo.DATA5 = r.Field(Of String)("DATA5")
                    settingInfo.DATA6 = r.Field(Of String)("DATA6")
                    settingInfo.DATA7 = r.Field(Of String)("DATA7")
                    settingInfo.DATA8 = r.Field(Of String)("DATA8")
                    settingInfo.DATA9 = r.Field(Of String)("DATA9")
                    settingInfo.DATA10 = r.Field(Of String)("DATA10")
                    ret = settingInfo
                End If
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return ret
        End Function
#End Region

#Region "M_SETTINGのレコードの取得"
        ''' <summary>
        ''' M_SETTINGの1レコードの取得
        ''' <returns>設定情報、取得できなかったらNothing</returns>
        ''' </summary>
        Public Shared Function GetSettingList(keyDiv As String) As List(Of SettingInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            Dim ret As List(Of SettingInfo) = New List(Of SettingInfo)
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Return Nothing
            End If
            Try
                'M_SETTINGテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  *")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("  M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  KEY_DIV = '" & keyDiv & "'")

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                For Each r As DataRow In tbl.Rows
                    Dim settingInfo = New SettingInfo()
                    settingInfo.KeyDiv = r.Field(Of String)("KEY_DIV")
                    settingInfo.Key1 = r.Field(Of String)("KEY1")
                    settingInfo.Key2 = r.Field(Of String)("KEY2")
                    settingInfo.Key3 = r.Field(Of String)("KEY3")
                    settingInfo.DATA1 = r.Field(Of String)("DATA1")
                    settingInfo.DATA2 = r.Field(Of String)("DATA2")
                    settingInfo.DATA3 = r.Field(Of String)("DATA3")
                    settingInfo.DATA4 = r.Field(Of String)("DATA4")
                    settingInfo.DATA5 = r.Field(Of String)("DATA5")
                    settingInfo.DATA6 = r.Field(Of String)("DATA6")
                    settingInfo.DATA7 = r.Field(Of String)("DATA7")
                    settingInfo.DATA8 = r.Field(Of String)("DATA8")
                    settingInfo.DATA9 = r.Field(Of String)("DATA9")
                    settingInfo.DATA10 = r.Field(Of String)("DATA10")
                    ret.Add(settingInfo)
                Next
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return ret
        End Function
#End Region

    End Class
End Namespace