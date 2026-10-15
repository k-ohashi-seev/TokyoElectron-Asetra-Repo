Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient
Imports GrapeCity.Documents.Excel
Imports System.IO.Compression


Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：棚卸実績出力
    ''' 作成日        ：2025/10/07      
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolInventory
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "棚卸実績出力取得"

        ''' <summary>
        ''' 棚卸実績出力取得
        ''' </summary>
        ''' <param name="sInvId">棚卸ID</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function InvoiceResultOutPut(ByVal sInvId As String,
                                                   ByVal sFullPath As String,
                                                   ByVal sTempFullPath As String,
                                                   ByVal sHeaderAbumn As String,
                                                   ByVal sHeaderAS02 As String,
                                                   ByRef AbumnDatacnt As Integer,
                                                   ByRef Pattern1Datacnt As Integer,
                                                   ByRef Pattern2Datacnt As Integer) As String

            'ライセンスキー
            GrapeCity.Documents.Excel.Workbook.SetLicenseKey("株式会社シービジョン,496378668853486#A03NJgzNzYTO4IiOiQWSisnOiQkIsISP3cWe6QTZ95WWvEmMYdTStZ5MltGdv26dWlWazcmYBREamhXNyUEVRdGbOl6SuREarg4L6dVRwZnUz4EZFZzKzYmZv2Ee7gje9tWb5IzZ6x6arQGUVl5UiojITJCL7YzM8cTM8gDO0IicfJye#4Xfd5nI9g4NyIiOiMkIsIiMWBCdl9kLgwWZjhXRgI7bmBCduVWb5N6bEByQHJiOi8kI1tlOiQmcQJCLiEDNzIDNwAiNwITM9EDMyIiOiQncDJCLiM1gjf0gjjrgjP9gjz1gjfrgj1LpnrJvk/IvlrKomLiOiEmTDJCLiYDO4M5Ntg")
            Dim tblData As DataTable

            '各ファイルのデータが取得できない場合もヘッダーだけは出力するのかを確認

            '-----ABUMN_MALS 作成-----
            'テンプレートEXCEL用
            Dim WB As IWorkbook = New Workbook
            Dim WS As IWorksheet
            Dim WSTemp As IWorksheet
            Dim rowIndex As Integer

            '棚卸実績データ取得　ABUMN_MALS
            tblData = GetAbumnData(sInvId)

            '読み込むテンプレートファイル
            WB.Open(sTempFullPath & "\" & CommonConst.INVENTRY_RESULT_FILENM)

            'テンプレートシート取得する
            WS = WB.Worksheets(0)

            ''テンプレートシートを次のシートにコピーする
            'WS = WSTemp.Copy()

            rowIndex = 2  ' データは2行目から
            Dim colIndex As Integer = 1  ' 列は1から始まる

            Dim Abumnheaders() As String = sHeaderAbumn.Split(",")

            ' --- ヘッダー出力 ---
            For i As Integer = 0 To Abumnheaders.Length - 1
                WS.Range(Chr(Asc("A") + i) & "1").Value = Abumnheaders(i)
            Next

            ' --- データ出力 ---
            rowIndex = 2  ' データは2行目から

            If tblData IsNot Nothing AndAlso tblData.Rows.Count > 0 Then
                '件数保存
                AbumnDatacnt = tblData.Rows.Count
                For Each dt As DataRow In tblData.Rows
                    WS.Range("A" & rowIndex).Value = dt("LOC_L_CD").ToString()
                    WS.Range("B" & rowIndex).Value = dt("CAT_L_CD").ToString()
                    WS.Range("C" & rowIndex).Value = dt("ASSET_NO").ToString()
                    WS.Range("D" & rowIndex).Value = dt("FREE_ITEM13").ToString()
                    WS.Range("E" & rowIndex).Value = dt("CHANGE_LOC_M_CD").ToString()
                    WS.Range("F" & rowIndex).Value = dt("OPE_STATUS").ToString()
                    WS.Range("G" & rowIndex).Value = dt("CHANGE_LOC_S_CD").ToString()
                    WS.Range("H" & rowIndex).Value = dt("EMPLOYEE_NO").ToString()
                    rowIndex += 1
                Next

            End If

            ''テンプレートシートを削除する
            'WB.Worksheets(0).Delete()

            'ファイル出力先
            Dim abumnFileName As String = "ABUMN_MALSY.xlsx"
            Dim abumnFile As String = sFullPath & "\" & abumnFileName                             '出力Excelファイル名
            '.xlsxドキュメントをエクスポートします
            WB.Save(abumnFile, SaveFileFormat.Xlsx)

            '-----AS02Pattern_1_MALSY_FILENM 作成-----

            '宣言と逆にして開放する。
            tblData = Nothing
            WS = Nothing
            WB = Nothing
            WSTemp = Nothing


            '棚卸実績データ取得　AS02Pattern1
            tblData = GetAS02Pattern1Data(sInvId)

            WB = New Workbook()

            '読み込むテンプレートファイル
            WB.Open(sTempFullPath & "\" & CommonConst.INVENTRY_RESULT_FILENM)

            'テンプレートシート取得する
            WS = WB.Worksheets(0)

            ''テンプレートシートを次のシートにコピーする
            'WS = WSTemp.Copy()

            Dim AS02headers() As String = sHeaderAS02.Split(",")

            ' --- ヘッダー出力 ---
            For i As Integer = 0 To AS02headers.Length - 1
                WS.Range(Chr(Asc("A") + i) & "1").Value = AS02headers(i)
            Next

            ' --- データ出力 ---
            rowIndex = 2  ' データは2行目から

            If tblData IsNot Nothing AndAlso tblData.Rows.Count > 0 Then
                '件数保存
                Pattern1Datacnt = tblData.Rows.Count
                For Each dt As DataRow In tblData.Rows
                    WS.Range("A" & rowIndex).Value = dt("LOC_L_CD").ToString()
                    WS.Range("B" & rowIndex).Value = dt("ASSET_NO").ToString()
                    WS.Range("C" & rowIndex).Value = dt("FREE_ITEM13").ToString()
                    WS.Range("D" & rowIndex).Value = dt("CHANGE_LOC_S_CD").ToString()
                    WS.Range("E" & rowIndex).Value = dt("OPE_STATUS").ToString()
                    WS.Range("F" & rowIndex).Value = dt("EMPLOYEE_NO").ToString()
                    rowIndex += 1
                Next

            End If

            ''テンプレートシートを次のシートにコピーする
            'WS = WSTemp.CopyAfter(WS)

            ''テンプレートシートを削除する
            'WB.Worksheets(0).Delete()

            'ファイル出力先
            Dim AS02Pattern1FileName As String = "AS02Pattern_1_MALSY.xlsx"
            Dim AS02Pattern1File As String = sFullPath & "\" & AS02Pattern1FileName                             '出力Excelファイル名
            '.xlsxドキュメントをエクスポートします
            WB.Save(AS02Pattern1File, SaveFileFormat.Xlsx)


            '-----AS02Pattern_2_MALSY_FILENM 作成-----

            '宣言と逆にして開放する。
            tblData = Nothing
            WS = Nothing
            WB = Nothing
            WSTemp = Nothing

            '棚卸実績データ取得　AS02Pattern1
            tblData = GetAS02Pattern2Data(sInvId)

            WB = New Workbook()

            '読み込むテンプレートファイル
            WB.Open(sTempFullPath & "\" & CommonConst.INVENTRY_RESULT_FILENM)

            'テンプレートシート取得する
            WS = WB.Worksheets(0)

            ''テンプレートシートを次のシートにコピーする
            'WS = WSTemp.Copy()

            ' --- ヘッダー出力 ---
            For i As Integer = 0 To AS02headers.Length - 1
                WS.Range(Chr(Asc("A") + i) & "1").Value = AS02headers(i)
            Next

            ' --- データ出力 ---
            rowIndex = 2  ' データは2行目から

            If tblData IsNot Nothing AndAlso tblData.Rows.Count > 0 Then
                '件数保存
                Pattern2Datacnt = tblData.Rows.Count
                For Each dt As DataRow In tblData.Rows
                    WS.Range("A" & rowIndex).Value = dt("LOC_L_CD").ToString()
                    WS.Range("B" & rowIndex).Value = dt("ASSET_NO").ToString()
                    WS.Range("C" & rowIndex).Value = dt("FREE_ITEM13").ToString()
                    WS.Range("D" & rowIndex).Value = dt("CHANGE_LOC_S_CD").ToString()
                    WS.Range("E" & rowIndex).Value = dt("OPE_STATUS").ToString()
                    WS.Range("F" & rowIndex).Value = dt("EMPLOYEE_NO").ToString()
                    rowIndex += 1
                Next

            End If

            ''テンプレートシートを次のシートにコピーする
            'WS = WSTemp.CopyAfter(WS)

            ''テンプレートシートを削除する
            'WB.Worksheets(0).Delete()

            'ファイル出力先
            Dim AS02Pattern2FileName As String = "AS02Pattern_2_MALSY.xlsx"
            Dim AS02Pattern2File As String = sFullPath & "\" & AS02Pattern2FileName                             '出力Excelファイル名
            '.xlsxドキュメントをエクスポートします
            WB.Save(AS02Pattern2File, SaveFileFormat.Xlsx)


            '-----Zip 作成-----

            Dim ZipFile As String = sFullPath & "\" & "work.zip"


            ' --- ZIPファイル作成 ---

            Using zipToOpen As FileStream = New FileStream(ZipFile, FileMode.Create)
                Using archive As New ZipArchive(zipToOpen, ZipArchiveMode.Create)
                    archive.CreateEntryFromFile(abumnFile, Path.GetFileName(abumnFile))
                    archive.CreateEntryFromFile(AS02Pattern1File, Path.GetFileName(AS02Pattern1File))
                    archive.CreateEntryFromFile(AS02Pattern2File, Path.GetFileName(AS02Pattern2File))
                End Using
            End Using


            'zipを作成した後削除
            'ダウンロードさせた後、ファイルを削除。
            System.IO.File.Delete(abumnFile)
            'ダウンロードさせた後、ファイルを削除。
            System.IO.File.Delete(AS02Pattern1File)
            'ダウンロードさせた後、ファイルを削除。
            System.IO.File.Delete(AS02Pattern2File)

            'エラーとかの場合は""を返す

            Return ZipFile

        End Function
#End Region

#Region "棚卸実績データ取得　ABUMN_MALS"
        Public Shared Function GetAbumnData(ByVal sInvId As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("LOC_L_CD ")           '会社コード
                sbSQL.AppendLine(",CAT_L_CD ")          '資産クラス
                sbSQL.AppendLine(",ASSET_NO ")          '資産番号
                sbSQL.AppendLine(",FREE_ITEM13 ")       '補助番号
                sbSQL.AppendLine(",CASE WHEN TRIM(ISNULL(CHANGE_LOC_M_CD,'')) = TRIM(ISNULL(LOC_M_CD,'')) THEN '/'")
                sbSQL.AppendLine("      WHEN TRIM(ISNULL(CHANGE_LOC_M_CD,'')) = '' THEN '/'")
                sbSQL.AppendLine("      ELSE CHANGE_LOC_M_CD ")
                sbSQL.AppendLine(" END AS CHANGE_LOC_M_CD")    '原価センタ
                sbSQL.AppendLine(",CASE WHEN ISNULL(OPE_STATUS,'') = ISNULL(FREE_ITEM3,'') THEN '/'")
                sbSQL.AppendLine("      WHEN ISNULL(OPE_STATUS , '') = '' THEN '/'")
                sbSQL.AppendLine("      ELSE OPE_STATUS ")
                sbSQL.AppendLine(" END AS OPE_STATUS")  '稼働区分
                sbSQL.AppendLine(",CASE WHEN TRIM(ISNULL(CHANGE_LOC_S_CD,'')) = TRIM(ISNULL(LOC_S_CD,'')) THEN '/'")
                sbSQL.AppendLine("      WHEN TRIM(ISNULL(CHANGE_LOC_S_CD , '')) = '' THEN '/'")
                sbSQL.AppendLine("      ELSE CHANGE_LOC_S_CD ")
                sbSQL.AppendLine(" END AS CHANGE_LOC_S_CD")    '場所コード
                sbSQL.AppendLine(",CASE WHEN ISNULL(CHANGE_EMPLOYEE,'') = ISNULL(FREE_ITEM6,'') THEN '/'")
                sbSQL.AppendLine("      WHEN ISNULL(CHANGE_EMPLOYEE , '') = '' THEN '/'")
                sbSQL.AppendLine("      ELSE CHANGE_EMPLOYEE ")
                sbSQL.AppendLine(" END AS EMPLOYEE_NO") '管理者社員番号

                sbSQL.AppendLine("FROM T_INV_RESULT")
                sbSQL.AppendLine("WHERE INV_ID = @INV_ID")
                params.Add(New SqlParameter($"@INV_ID", CommonRfid.ToStringNvl(sInvId)))
                sbSQL.AppendLine("AND TRIM(ISNULL(LOC_M_CD,'')) <> TRIM(ISNULL(CHANGE_LOC_M_CD,''))")     'ロケ中区分が変更された場合
                sbSQL.AppendLine("AND TRIM(ISNULL(CHANGE_LOC_M_CD,'')) <> '' ")          '変更後ロケ中区分がnullでない場合
                sbSQL.AppendLine("ORDER BY ASSET_NO")

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

#Region "棚卸実績データ取得　AS02Pattern_1_MALSY"
        Public Shared Function GetAS02Pattern1Data(ByVal sInvId As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("LOC_L_CD ")           '会社コード
                sbSQL.AppendLine(",ASSET_NO ")          '資産番号
                sbSQL.AppendLine(",FREE_ITEM13 ")          '補助番号

                sbSQL.AppendLine(",CASE WHEN TRIM(ISNULL(CHANGE_LOC_S_CD,'')) = TRIM(ISNULL(LOC_S_CD,'')) THEN '/'")
                sbSQL.AppendLine("      WHEN TRIM(ISNULL(CHANGE_LOC_S_CD,'')) = '' THEN '/'")
                sbSQL.AppendLine("      ELSE CHANGE_LOC_S_CD ")
                sbSQL.AppendLine(" END AS CHANGE_LOC_S_CD")    '場所コード
                sbSQL.AppendLine(",CASE WHEN ISNULL(OPE_STATUS,'') = ISNULL(FREE_ITEM3,'') THEN '/'")
                sbSQL.AppendLine("      WHEN ISNULL(OPE_STATUS , '') = '' THEN '/'")
                sbSQL.AppendLine("      ELSE OPE_STATUS ")
                sbSQL.AppendLine(" END AS OPE_STATUS")  '稼働区分
                sbSQL.AppendLine(",CASE WHEN ISNULL(CHANGE_EMPLOYEE,'') = ISNULL(FREE_ITEM6,'') THEN '/'")
                sbSQL.AppendLine("      WHEN ISNULL(CHANGE_EMPLOYEE , '') = '' THEN '/'")
                sbSQL.AppendLine("      ELSE CHANGE_EMPLOYEE ")
                sbSQL.AppendLine(" END AS EMPLOYEE_NO") '管理者社員番号

                sbSQL.AppendLine("FROM T_INV_RESULT")
                sbSQL.AppendLine("WHERE INV_ID = @INV_ID")
                params.Add(New SqlParameter($"@INV_ID", CommonRfid.ToStringNvl(sInvId)))
                sbSQL.AppendLine("AND (TRIM(ISNULL(CHANGE_LOC_M_CD,''))='' OR TRIM(ISNULL(LOC_M_CD,'')) = TRIM(ISNULL(CHANGE_LOC_M_CD,''))) ")     'ロケ中区分が変更されていない場合
                sbSQL.AppendLine("AND TRIM(ISNULL(LOC_S_CD,'')) <> TRIM(ISNULL(CHANGE_LOC_S_CD,'')) ")    'ロケ小区分が変更された場合
                sbSQL.AppendLine("AND TRIM(ISNULL(CHANGE_LOC_S_CD,'')) <> '' ")          '変更後ロケ小区分がnullでない場合
                sbSQL.AppendLine("ORDER BY ASSET_NO")

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

#Region "棚卸実績データ取得　AS02Pattern_2_MALSY"
        Public Shared Function GetAS02Pattern2Data(ByVal sInvId As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("LOC_L_CD ")           '会社コード
                sbSQL.AppendLine(",ASSET_NO ")          '資産番号
                sbSQL.AppendLine(",FREE_ITEM13 ")          '補助番号

                sbSQL.AppendLine(",CASE WHEN TRIM(ISNULL(CHANGE_LOC_S_CD,'')) = TRIM(ISNULL(LOC_S_CD,'')) THEN '/'")
                sbSQL.AppendLine("      WHEN TRIM(ISNULL(CHANGE_LOC_S_CD,'')) = '' THEN '/'")
                sbSQL.AppendLine("      ELSE CHANGE_LOC_S_CD ")
                sbSQL.AppendLine(" END AS CHANGE_LOC_S_CD")    '場所コード
                sbSQL.AppendLine(",CASE WHEN ISNULL(OPE_STATUS,'') = ISNULL(FREE_ITEM3,'') THEN '/'")
                sbSQL.AppendLine("      WHEN ISNULL(OPE_STATUS , '') = '' THEN '/'")
                sbSQL.AppendLine("      ELSE OPE_STATUS ")
                sbSQL.AppendLine(" END AS OPE_STATUS")  '稼働区分
                sbSQL.AppendLine(",CASE WHEN ISNULL(CHANGE_EMPLOYEE,'') = ISNULL(FREE_ITEM6,'') THEN '/'")
                sbSQL.AppendLine("      WHEN ISNULL(CHANGE_EMPLOYEE , '') = '' THEN '/'")
                sbSQL.AppendLine("      ELSE CHANGE_EMPLOYEE ")
                sbSQL.AppendLine(" END AS EMPLOYEE_NO") '管理者社員番号

                sbSQL.AppendLine("FROM T_INV_RESULT")
                sbSQL.AppendLine("WHERE INV_ID = @INV_ID")
                params.Add(New SqlParameter($"@INV_ID", CommonRfid.ToStringNvl(sInvId)))
                sbSQL.AppendLine("AND ( TRIM(ISNULL(LOC_M_CD,'')) = TRIM(ISNULL(CHANGE_LOC_M_CD,'')) ")   'ロケ中区分が変更されていない場合
                sbSQL.AppendLine("OR TRIM(ISNULL(CHANGE_LOC_M_CD,'')) = '' )")            '変更後ロケ中区分がnullでない場合
                sbSQL.AppendLine("AND ( TRIM(ISNULL(LOC_S_CD,'')) = TRIM(ISNULL(CHANGE_LOC_S_CD,'')) ")   'ロケ小区分が変更された場合
                sbSQL.AppendLine("OR TRIM(ISNULL(CHANGE_LOC_S_CD,'')) = '' )")            '変更後ロケ小区分がnullでない場合
                sbSQL.AppendLine("ORDER BY ASSET_NO")

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

#Region "棚卸中か取得する"
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sAssetNo"></param>
        ''' <returns>True: 棚卸中、False:棚卸中ではない</returns>
        Public Shared Function IsIventory(invId As Long, invLocLCd As String, AssetNoList As List(Of String)) As Boolean
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try

                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine(" t.INV_CONFIRM_EMPLOYEE_CD ")
                sbSQL.AppendLine(" FROM T_INV_ASSET a ")
                sbSQL.AppendLine(" LEFT JOIN T_INVENTORY t ON a.INV_ID = t.INV_ID ")
                sbSQL.AppendLine($"WHERE a.ASSET_NO IN ('{String.Join("','", AssetNoList)}')")
                sbSQL.AppendLine($" AND a.INV_ID = {invId} ")
                sbSQL.AppendLine($" AND a.INV_LOC_L_CD = '{invLocLCd}' ")
                sbSQL.AppendLine("GROUP BY INV_CONFIRM_EMPLOYEE_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                For Each r As DataRow In tbl.Rows
                    Dim comfirmEmployeeCd = r.Field(Of String)("INV_CONFIRM_EMPLOYEE_CD")
                    If Not String.IsNullOrWhiteSpace(comfirmEmployeeCd) Then
                        Return True
                    End If

                Next

                Return False

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return False


        End Function
#End Region



    End Class
End Namespace
