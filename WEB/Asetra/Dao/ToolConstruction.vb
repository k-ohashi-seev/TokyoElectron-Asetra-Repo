Imports Asetra.Misc
Imports System.IO
Imports Asetra.Models
Imports System.Data.SqlClient
Imports GrapeCity.Documents
Imports GrapeCity.Documents.Excel

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：工事関連ツール系 Data Access クラス
    ''' 作成日        ：2024/10/23
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolConstruction

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        ''' <summary>シート名</summary>
        Public Const SheetNm As String = "REPORT"

        ''' <summary>明細数</summary>
        Public Const Cnt As Integer = 100


#Region "データ取得(全項目)"
        ''' <summary>
        ''' データ取得(全項目)
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetConstructionDataAll() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM  M_CONSTRUCTION ")
                sbSQL.AppendLine("ORDER BY CONST_NO")


                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("CONST_NO")}

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


#Region "工事帳票PDF作成"
        ''' <summary>
        ''' 工事帳票PDF作成
        ''' </summary>
        ''' <param name="sKeyStr">チェック状態連結文字列</param>
        ''' <param name="sTempFullPath">テンプレートフォルダ　プルパス</param>
        ''' <param name="sOutFullPath">出力フォルダ　プルパス</param>
        ''' <returns>
        ''' PDF作成ファイル名（フルパス）　エラー時、String.Empty
        ''' </returns>
        Public Shared Function MakeConstPrint(ByVal sKeyStr As String, ByVal sTempFullPath As String, ByVal sOutFullPath As String) As String

            '工事帳票出力用予約データ取得
            Dim tblConst As DataTable = GetConstData(sKeyStr)

            'ライセンスキー
            GrapeCity.Documents.Excel.Workbook.SetLicenseKey("株式会社シービジョン,496378668853486#A03NJgzNzYTO4IiOiQWSisnOiQkIsISP3cWe6QTZ95WWvEmMYdTStZ5MltGdv26dWlWazcmYBREamhXNyUEVRdGbOl6SuREarg4L6dVRwZnUz4EZFZzKzYmZv2Ee7gje9tWb5IzZ6x6arQGUVl5UiojITJCL7YzM8cTM8gDO0IicfJye#4Xfd5nI9g4NyIiOiMkIsIiMWBCdl9kLgwWZjhXRgI7bmBCduVWb5N6bEByQHJiOi8kI1tlOiQmcQJCLiEDNzIDNwAiNwITM9EDMyIiOiQncDJCLiM1gjf0gjjrgjP9gjz1gjfrgj1LpnrJvk/IvlrKomLiOiEmTDJCLiYDO4M5Ntg")

            'テンプレートEXCEL用
            Dim wbTemplate As IWorkbook = New Workbook
            Dim wsTemplate As IWorksheet
            'テンプレートファイル読込み
            wbTemplate.Open(sTempFullPath & "\" & CommonConst.CONST_PRINT_TEMPLATE_FILENM)

            ' 新規ワークシート
            Dim wsNew As IWorksheet


            '一時ファイル名
            Dim sOutputFileName As String = Path.GetRandomFileName()

            'PDF一時ファイル名
            Dim sPdfFileNm As String = sOutFullPath & "\" & sOutputFileName & ".pdf"

            Dim wbList As New List(Of Workbook)


            '工事帳票出力
            For Each rowConst As DataRow In tblConst.Rows

                ' 新規ワークブックの作成
                Dim wbNew As New Workbook

                'エクセルデータ作成

                '工事帳票出力用仮置きデータ取得
                Dim tblTemp As DataTable = GetTemporaryData(rowConst("CONST_NO").ToString)

                '工事帳票出力用工事資産データ取得
                Dim tblTempAsset As DataTable = GetTempAssetData(rowConst("CONST_NO").ToString)

                Dim iPageCnt As Integer = 0
                '仮置き場所オフセット
                Dim tempOffset As Integer = tblTemp.Rows.Count - 3
                '明細行計算
                Dim calcCnt As Integer = Cnt - tempOffset * 2

                'テンプレートシート取得する
                wsTemplate = wbTemplate.Worksheets(SheetNm)
                'テンプレートシートを次のシートにコピーする
                wsNew = wsTemplate.Copy(wbNew)

                '工事資産が0件の場合
                If tblTempAsset.Rows.Count = 0 Then

                    iPageCnt += 1

                    'ヘッダデータの設定
                    wsNew.Range("CONST_START_DT").Value = ToolCommon.NumDateString(CInt(rowConst("CONST_START_DT")))
                    wsNew.Range("CONST_INFO8").Value = rowConst("CONST_INFO9").ToString.Replace("$$$", " ～ ").Trim
                    wsNew.Range("CONST_NO").Value = rowConst("CONST_INFO2").ToString.Trim
                    wsNew.Range("CONST_CONTENTS").Value = rowConst("CONST_INFO3").ToString.Replace("$$$", " ～ ").Trim
                    wsNew.Range("CONST_INFO2").Value = rowConst("CONST_CONTENTS").ToString.Trim
                    wsNew.Range("CONST_INFO1").Value = rowConst("CONST_INFO1").ToString.Replace("$$$", " ～ ").Trim
                    wsNew.Range("CONST_EMPLOYEE_NAME").Value = rowConst("CONST_EMPLOYEE_NAME").ToString.Trim
                    wsNew.Range("SITE_EMPLOYEE_NAME").Value = rowConst("SITE_EMPLOYEE_NAME").ToString.Trim
                    wsNew.Range("CONST_INFO").Value = rowConst("CONST_INFO4").ToString.Replace("$$$", " ～ ").Trim & "  " &
                                                      rowConst("CONST_INFO5").ToString.Replace("$$$", " ～ ").Trim & "  " &
                                                      rowConst("CONST_INFO6").ToString.Replace("$$$", " ～ ").Trim & "  " &
                                                      rowConst("CONST_INFO7").ToString.Replace("$$$", " ～ ").Trim & "  " &
                                                      rowConst("CONST_INFO8").ToString.Replace("$$$", " ～ ").Trim

                    'フッターデータの設定
                    wsNew.Range("CONST_CONFIRM_TS").Value = If(rowConst("CONST_CONFIRM_TS").ToString.Length = itemlength.ConstConfirmTs,
                                                                "",
                                                                DateTime.ParseExact(rowConst("CONST_CONFIRM_TS").ToString,
                                                                                    "yyyyMMddHHmmss",
                                                                                    Globalization.CultureInfo.InvariantCulture).ToString("yyyy/MM/dd hh:mm:ss"))
                    wsNew.Range("CONST_CONFIRM_REMARK").Value = rowConst("CONST_CONFIRM_REMARK").ToString.Trim

                    '仮置き場所の件数分明細の調整
                    For i As Integer = 0 To tempOffset - 1
                        '明細行末尾を削除
                        wsNew.Range("A62").EntireRow.Delete()
                        '仮置き場所行追加
                        wsNew.Range("A11").EntireRow.Insert()
                        wsNew.Range("B10").Copy(wsNew.Range("B11:E11"))
                        wsNew.Range("B11:E11").Merge()
                        wsNew.Range("TEMP_INFO").Copy(wsNew.Range("F11:AF11"))
                        wsNew.Range("F11:AF11").Merge()
                    Next
                    '仮置き場所設定
                    For Each rowTmp In tblTemp.AsEnumerable.Select(Function(Value, Index) New With {Value, Index})
                        Dim colIndex = 8
                        wsNew.Range("F" & (colIndex + rowTmp.Index).ToString).Value = rowTmp.Value("TEMP_INFO1").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO2").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO3").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO4").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO5").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO6").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO7").ToString.Replace("$$$", "").Trim
                    Next

                    '----- Book 追加
                    wbList.Add(wbNew)

                    Continue For
                End If

                Dim rngAssetNameLeft = wsNew.Range("ASSET_NAME_LEFT")
                Dim rngStorageOutNumLeft = wsNew.Range("STORAGE_OUT_NUM_LEFT")
                Dim rngConstInNumLeft = wsNew.Range("CONST_IN_NUM_LEFT")
                Dim rngConstTempNumLeft = wsNew.Range("CONST_TEMP_NUM_LEFT")
                Dim rngConstOutNumLeft = wsNew.Range("CONST_OUT_NUM_LEFT")
                Dim rngResultLeft = wsNew.Range("RESULT_LEFT")
                Dim rngAssetNameRight = wsNew.Range("ASSET_NAME_RIGHT")
                Dim rngStorageOutNumRight = wsNew.Range("STORAGE_OUT_NUM_RIGHT")
                Dim rngConstInNumRight = wsNew.Range("CONST_IN_NUM_RIGHT")
                Dim rngConstTempNumRight = wsNew.Range("CONST_TEMP_NUM_RIGHT")
                Dim rngConstOutNumRight = wsNew.Range("CONST_OUT_NUM_RIGHT")
                Dim rngResultRight = wsNew.Range("RESULT_RIGHT")
                Dim iCnt As Integer = 0
                Dim iTotalCnt As Integer = 0
                For Each rowDtl As DataRow In tblTempAsset.Rows

                    '半分が終了した場合
                    If iCnt + 1 > calcCnt / 2 Then
                        iCnt = 0
                    End If

                    '初回または1ページ終了時
                    If iTotalCnt = 0 OrElse iTotalCnt + 1 - (calcCnt * (iPageCnt - 1)) > calcCnt Then
                        iPageCnt += 1

                        '2ページ目以降はシート追加
                        If iPageCnt >= 2 Then
                            'テンプレートシート取得する
                            wsTemplate = wbTemplate.Worksheets(SheetNm)
                            'テンプレートシートを次のシートにコピーする
                            wsNew = wsTemplate.Copy(wbNew)
                            '明細連番の初期化
                            wsNew.Range("FIRST_NUM").Value = iTotalCnt + 1
                        End If

                        'ヘッダデータの設定
                        wsNew.Range("CONST_START_DT").Value = ToolCommon.NumDateString(CInt(rowConst("CONST_START_DT")))
                        wsNew.Range("CONST_INFO8").Value = rowConst("CONST_INFO9").ToString.Replace("$$$", " ～ ").Trim
                        wsNew.Range("CONST_NO").Value = rowConst("CONST_INFO2").ToString.Trim
                        wsNew.Range("CONST_CONTENTS").Value = rowConst("CONST_INFO3").ToString.Replace("$$$", " ～ ").Trim
                        wsNew.Range("CONST_INFO2").Value = rowConst("CONST_CONTENTS").ToString.Trim
                        wsNew.Range("CONST_INFO1").Value = rowConst("CONST_INFO1").ToString.Replace("$$$", " ～ ").Trim
                        wsNew.Range("CONST_EMPLOYEE_NAME").Value = rowConst("CONST_EMPLOYEE_NAME").ToString.Trim
                        wsNew.Range("SITE_EMPLOYEE_NAME").Value = rowConst("SITE_EMPLOYEE_NAME").ToString.Trim
                        wsNew.Range("CONST_INFO").Value = rowConst("CONST_INFO4").ToString.Replace("$$$", " ～ ").Trim & "  " &
                                                          rowConst("CONST_INFO5").ToString.Replace("$$$", " ～ ").Trim & "  " &
                                                          rowConst("CONST_INFO6").ToString.Replace("$$$", " ～ ").Trim & "  " &
                                                          rowConst("CONST_INFO7").ToString.Replace("$$$", " ～ ").Trim & "  " &
                                                          rowConst("CONST_INFO8").ToString.Replace("$$$", " ～ ").Trim

                        'フッターデータの設定
                        wsNew.Range("CONST_CONFIRM_TS").Value = If(rowConst("CONST_CONFIRM_TS").ToString.Length = itemlength.ConstConfirmTs,
                                                                    DateTime.ParseExact(rowConst("CONST_CONFIRM_TS").ToString,
                                                                                        "yyyyMMddHHmmss",
                                                                                        Globalization.CultureInfo.InvariantCulture).ToString("yyyy/MM/dd HH:mm"),
                                                                    "")
                        wsNew.Range("CONST_CONFIRM_REMARK").Value = rowConst("CONST_CONFIRM_REMARK").ToString.Trim

                        '仮置き場所の件数分明細の調整
                        For i As Integer = 0 To tempOffset - 1
                            '明細行末尾を削除
                            wsNew.Range("A62").EntireRow.Delete()
                            '仮置き場所行追加
                            wsNew.Range("A11").EntireRow.Insert()
                            wsNew.Range("B10").Copy(wsNew.Range("B11:E11"))
                            wsNew.Range("B11:E11").Merge()
                            wsNew.Range("TEMP_INFO").Copy(wsNew.Range("F11:AF11"))
                            wsNew.Range("F11:AF11").Merge()
                        Next
                        '仮置き場所設定
                        For Each rowTmp In tblTemp.AsEnumerable.Select(Function(Value, Index) New With {Value, Index})
                            Dim colIndex = 8
                            wsNew.Range("F" & (colIndex + rowTmp.Index).ToString).Value = rowTmp.Value("TEMP_INFO1").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO2").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO3").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO4").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO5").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO6").ToString.Replace("$$$", "").Trim & "  " &
                                                                                          rowTmp.Value("TEMP_INFO7").ToString.Replace("$$$", "").Trim
                        Next

                        rngAssetNameLeft = wsNew.Range("ASSET_NAME_LEFT")
                        rngStorageOutNumLeft = wsNew.Range("STORAGE_OUT_NUM_LEFT")
                        rngConstInNumLeft = wsNew.Range("CONST_IN_NUM_LEFT")
                        rngConstTempNumLeft = wsNew.Range("CONST_TEMP_NUM_LEFT")
                        rngConstOutNumLeft = wsNew.Range("CONST_OUT_NUM_LEFT")
                        rngResultLeft = wsNew.Range("RESULT_LEFT")
                        rngAssetNameRight = wsNew.Range("ASSET_NAME_RIGHT")
                        rngStorageOutNumRight = wsNew.Range("STORAGE_OUT_NUM_RIGHT")
                        rngConstInNumRight = wsNew.Range("CONST_IN_NUM_RIGHT")
                        rngConstTempNumRight = wsNew.Range("CONST_TEMP_NUM_RIGHT")
                        rngConstOutNumRight = wsNew.Range("CONST_OUT_NUM_RIGHT")
                        rngResultRight = wsNew.Range("RESULT_RIGHT")
                    End If

                    '明細データの設定
                    If iTotalCnt + 1 - (calcCnt * (iPageCnt - 1)) <= calcCnt / 2 Then
                        'rngAssetNameLeft.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                        rngAssetNameLeft.Cells(iCnt).Value = rowDtl("GOODS_ASSET_NAME").ToString.Trim
                        rngStorageOutNumLeft.Cells(iCnt).Value = rowDtl("STORAGE_OUT_NUM").ToString.Trim
                        rngConstInNumLeft.Cells(iCnt).Value = rowDtl("CONST_IN_NUM").ToString.Trim
                        rngConstTempNumLeft.Cells(iCnt).Value = rowDtl("CONST_TEMP_NUM").ToString.Trim
                        rngConstOutNumLeft.Cells(iCnt).Value = rowDtl("CONST_OUT_NUM").ToString.Trim
                        rngResultLeft.Cells(iCnt).Value = If(rowDtl("CONST_IN_NUM") = rowDtl("CONST_TEMP_NUM") + rowDtl("CONST_OUT_NUM"),
                                                                    Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_CIRCLE),
                                                                    Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_CROSS_MARK))
                    Else
                        rngAssetNameRight.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                        rngStorageOutNumRight.Cells(iCnt).Value = rowDtl("STORAGE_OUT_NUM").ToString.Trim
                        rngConstInNumRight.Cells(iCnt).Value = rowDtl("CONST_IN_NUM").ToString.Trim
                        rngConstTempNumRight.Cells(iCnt).Value = rowDtl("CONST_TEMP_NUM").ToString.Trim
                        rngConstOutNumRight.Cells(iCnt).Value = rowDtl("CONST_OUT_NUM").ToString.Trim
                        rngResultRight.Cells(iCnt).Value = If(rowDtl("CONST_IN_NUM") = rowDtl("CONST_TEMP_NUM") + rowDtl("CONST_OUT_NUM"),
                                                                    Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_CIRCLE),
                                                                    Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_CROSS_MARK))
                    End If
                    iCnt += 1
                    iTotalCnt += 1
                Next

                '----- Book 追加
                wbList.Add(wbNew)


            Next

            ' プリントマネージャーで複数bookをPDFに保存
            Dim printManager As New Excel.PrintManager
            Dim outputStream = File.Create(sPdfFileNm)
            printManager.SavePDF(outputStream, wbList.ToArray)
            outputStream.Close()


            Return sPdfFileNm

        End Function
#End Region

#Region "工事帳票出力用工事データ取得"

        ''' <summary>
        ''' 工事帳票出力用工事データ取得
        ''' </summary>
        ''' <param name="sLstConstNo">工事番号のコンマ区切り文字列</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Private Shared Function GetConstData(sLstConstNo As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM  M_CONSTRUCTION ")
                sbSQL.AppendLine("WHERE M_CONSTRUCTION.CONST_NO IN (" & sLstConstNo & ")")
                sbSQL.AppendLine("ORDER BY CONST_NO")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("CONST_NO")}

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

#Region "工事帳票出力用仮置きデータ取得"
        ''' <summary>
        ''' 工事帳票出力用仮置きデータ取得
        ''' </summary>
        ''' <param name="sConstNo">工事番号</param>
        ''' <returns>仮置きデータ</returns>
        Private Shared Function GetTemporaryData(ByVal sConstNo As String) As DataTable
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
                sbSQL.AppendLine("  TEMP.TEMP_ID")
                sbSQL.AppendLine("  ,TEMP.TEMP_INFO1")
                sbSQL.AppendLine("  ,TEMP.TEMP_INFO2")
                sbSQL.AppendLine("  ,TEMP.TEMP_INFO3")
                sbSQL.AppendLine("  ,TEMP.TEMP_INFO4")
                sbSQL.AppendLine("  ,TEMP.TEMP_INFO5")
                sbSQL.AppendLine("  ,TEMP.TEMP_INFO6")
                sbSQL.AppendLine("  ,TEMP.TEMP_INFO7")
                sbSQL.AppendLine("FROM  T_TEMPORARY TEMP")
                'sbSQL.AppendLine("WHERE TEMP.REL_CONST_NO = @REL_CONST_NO")
                sbSQL.AppendLine("WHERE TEMP.REG_CONST_NO = @REG_CONST_NO")
                sbSQL.AppendLine("ORDER BY TEMP.TEMP_ID")

                params.Add(New SqlParameter($"@REG_CONST_NO", sConstNo.Trim))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("TEMP_ID")}

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

#Region "工事帳票出力用工事資産データ取得"
        ''' <summary>
        ''' 工事帳票出力用工事資産データ取得
        ''' </summary>
        ''' <param name="sConstNo">工事番号</param>
        ''' <returns>予約データ</returns>
        Private Shared Function GetTempAssetData(ByVal sConstNo As String) As DataTable
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
                sbSQL.AppendLine("    CLOG.CONST_NO ")
                sbSQL.AppendLine("    ,MAX(ISNULL(ASSET.ASSET_NAME, '')) AS ASSET_NAME ")
                sbSQL.AppendLine("    ,MAX(ISNULL(GOODS.ASSET_NAME, '')) AS GOODS_ASSET_NAME ")
                sbSQL.AppendLine("    ,ISNULL(ASSET.PRODUCT_NO, '') AS PRODUCT_NO ")
                sbSQL.AppendLine("    ,COUNT(CASE CONST_WORK_DIV WHEN '3' THEN 1 ELSE NULL END) AS STORAGE_OUT_NUM ")
                sbSQL.AppendLine("    ,COUNT(CASE CONST_WORK_DIV WHEN '2' THEN 1 WHEN '3' THEN 1 ELSE NULL END) AS CONST_IN_NUM ")
                sbSQL.AppendLine("    ,COUNT(DISTINCT CASE CONST_WORK_DIV WHEN '5' THEN ASSET.ASSET_NO ELSE NULL END) AS CONST_TEMP_NUM ")
                sbSQL.AppendLine("    ,COUNT(CASE CONST_WORK_DIV WHEN '7' THEN 1 WHEN '8' THEN 1 ELSE NULL END) AS CONST_OUT_NUM ")
                sbSQL.AppendLine("FROM T_CONST_LOG CLOG ")
                sbSQL.AppendLine("INNER JOIN T_CONST_ASSET_LOG CALOG ")
                sbSQL.AppendLine("ON CLOG.CONST_NO = CALOG.CONST_NO ")
                sbSQL.AppendLine("AND CLOG.CONST_WORK_TS = CALOG.CONST_WORK_TS ")
                sbSQL.AppendLine("INNER JOIN M_ASSET ASSET ")
                sbSQL.AppendLine("ON CALOG.ASSET_NO = ASSET.ASSET_NO ")
                sbSQL.AppendLine("LEFT JOIN M_GOODS GOODS ")
                sbSQL.AppendLine("ON ASSET.PRODUCT_NO = GOODS.PRODUCT_NO ")
                sbSQL.AppendLine("WHERE CLOG.CONST_NO = @CONST_NO ")
                sbSQL.AppendLine("GROUP BY CLOG.CONST_NO,ASSET.PRODUCT_NO ")
                sbSQL.AppendLine("ORDER BY MAX(ISNULL(ASSET.ASSET_NAME, '')) ")

                params.Add(New SqlParameter($"@CONST_NO", sConstNo.Trim))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("CONST_NO"), tbl.Columns("ASSET_NAME"), tbl.Columns("PRODUCT_NO")}

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

#Region "引数に該当する予約明細データを取得します"
        ''' <summary>
        ''' 引数に該当する予約明細データを取得します
        ''' </summary>
        ''' <param name="sConstNo">工事番号</param>
        ''' <returns>予約明細データ</returns>
        Public Shared Function GetReserveDetailData(ByVal sConstNo As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'T_RESERVE_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESVM ")
                sbSQL.AppendLine("INNER JOIN T_RESERVE RSV ")
                sbSQL.AppendLine("ON RESVM.RESERVE_ID = RSV.RESERVE_ID ")
                sbSQL.AppendLine("INNER JOIN M_ASSET ASSET ")
                sbSQL.AppendLine("ON ASSET.ASSET_NO = RESVM.ASSET_NO ")
                sbSQL.AppendLine("AND RSV.REMARK = @CONST_NO")

                params.Add(New SqlParameter($"@CONST_NO", sConstNo.Trim))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("RESERVE_ID"), tbl.Columns("ASSET_NO"), tbl.Columns("REAL_START_DT")}

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

#Region "引数に該当する予約明細データを取得します(警告フラグも取得)"
        ''' <summary>
        ''' 引数に該当する予約明細データを取得します(警告メンテナンス区分も取得)
        ''' </summary>
        ''' <param name="sConstNo">工事番号</param>
        ''' <returns>予約明細データ</returns>
        Public Shared Function GetReserveDetailDataWarning(ByVal sConstNo As String, ByVal sConstLentStartDt As String, ByVal sConstLentEntDt As String, ByVal sWorkDayFlg As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'T_RESERVE_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESVM ")
                sbSQL.AppendLine("INNER JOIN T_RESERVE RSV ")
                sbSQL.AppendLine("ON RESVM.RESERVE_ID = RSV.RESERVE_ID ")
                sbSQL.AppendLine("INNER JOIN M_ASSET ASSET ")
                sbSQL.AppendLine("ON ASSET.ASSET_NO = RESVM.ASSET_NO ")
                sbSQL.AppendLine("AND RSV.REMARK = @CONST_NO")

                '警告メンテナンスデータ取得 ストアド
                sbSQL.AppendLine("CROSS APPLY ")
                sbSQL.AppendLine($"dbo.WARN_MAINTE_GET_SF(ASSET.ASSET_NO, RSV.RESERVE_ID,{sConstLentStartDt},{sConstLentEntDt},'{sWorkDayFlg}') ")
                'メンテナンス区分とかぶってるデータが後ろに来るように並び替え
                sbSQL.AppendLine("ORDER BY WARN_MAINTE_KBN ")

                params.Add(New SqlParameter($"@CONST_NO", sConstNo.Trim))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("RESERVE_ID"), tbl.Columns("ASSET_NO"), tbl.Columns("REAL_START_DT")}

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

#Region "資産予約登録処理"
        ''' <summary>
        ''' 資産予約登録処理
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function AddAssetReserve(ByRef db As SystemDBContext, ByVal ViewModel As ConstructionListViewModel, loginInfo As RfidEntity.LoginUser, messageRfid As MessageRfid) As String

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)
            Dim returnValue As Integer = 0
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Try
                '予約明細データ取得
                Dim resDetail = GetReserveDetailData(ViewModel.CONST_NO)

                '0件の場合は終了
                If resDetail.Rows.Count = 0 Then
                    Return ("ok")
                End If

                '予約明細登録
                '照合機能の予約実施設定を取得
                Dim MatchingReserve As SettingInfo = messageRfid.getSettingInfo(CommonSettingKey.MatchingReserveFunction.KeyDiv,
                                                              CommonSettingKey.MatchingReserveFunction.Key1,
                                                              CommonSettingKey.MatchingReserveFunction.Key2,
                                                              CommonSettingKey.MatchingReserveFunction.Key3, True)

                Dim sWorkDayFlg As String = MatchingReserve.DATA2

                '予約テーブル更新
                sbSQL.Clear()
                params = New List(Of SqlParameter)

                sbSQL.AppendLine("UPDATE T_RESERVE SET")
                sbSQL.AppendLine("    REQ_EMP_CD = @REQ_EMP_CD")
                sbSQL.AppendLine("    ,CHARGE_EMP_CD = @CHARGE_EMP_CD")
                sbSQL.AppendLine("    ,REAL_START_DT = @REAL_START_DT")
                sbSQL.AppendLine("    ,REAL_END_DT = @REAL_END_DT")
                sbSQL.AppendLine("    ,USER_START_DT = @USER_START_DT")
                sbSQL.AppendLine("    ,USER_END_DT = @USER_END_DT")
                sbSQL.AppendLine("    ,MNG_LOC_L_CD = @MNG_LOC_L_CD")
                sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                params.Add(New SqlParameter($"@REQ_EMP_CD", ViewModel.CONST_EMPLOYEE_CD))
                params.Add(New SqlParameter($"@CHARGE_EMP_CD", ViewModel.CONST_EMPLOYEE_CD))
                params.Add(New SqlParameter($"@REAL_START_DT", If(String.IsNullOrEmpty(ViewModel.ConstStartDtString), 0, CInt(CDate(ViewModel.ConstStartDtString).ToString("yyyyMMdd")))))
                params.Add(New SqlParameter($"@REAL_END_DT", If(String.IsNullOrEmpty(ViewModel.ConstEndDtString), 0, CInt(CDate(ViewModel.ConstEndDtString).ToString("yyyyMMdd")))))
                params.Add(New SqlParameter($"@USER_START_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentStartDtString), 0, CInt(CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd")))))
                params.Add(New SqlParameter($"@USER_END_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentEndDtString), 0, CInt(CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd")))))
                params.Add(New SqlParameter($"@MNG_LOC_L_CD", ViewModel.MNG_LOC_L_CD))
                params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", loginInfo.EmployeeCd))
                params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", loginInfo.EmployeeNm))
                params.Add(New SqlParameter($"@UPD_TASK", messageRfid.getMessage(CommonLabelConstructionList.TITLE)))
                params.Add(New SqlParameter($"@RESERVE_ID", resDetail.Rows(0)("RESERVE_ID")))

                db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                '貸出中データ更新
                Dim lendingDataList = resDetail.Select("RESERVE_STATUS ='" + ReserveStatus.Lending + "'")
                For Each lendingData As DataRow In lendingDataList
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)
                    returnValue = 0

                    sbSQL.AppendLine("DECLARE @nRet INT;")
                    sbSQL.AppendLine("EXEC @nRet =dbo.[ASSET_REPLACE_RESERVE_POSSIBLE_SP]")
                    sbSQL.AppendLine("            @IN_PRODUCT_NO")
                    sbSQL.AppendLine("            ,@ASSET_NO")
                    sbSQL.AppendLine("            ,@IN_FROM_DT")
                    sbSQL.AppendLine("            ,@IN_TO_DT")
                    sbSQL.AppendLine("            ,@IN_KADO_JITU")
                    sbSQL.AppendLine("            ,@IN_CONST_NO")
                    sbSQL.AppendLine("            ,@IN_CONST_RESERVE_ID")
                    sbSQL.AppendLine("            ,@IN_REG_EMP_CD")
                    sbSQL.AppendLine("            ,@IN_REG_EMP_NAME")
                    sbSQL.AppendLine("            ,@IN_REG_TASK")
                    sbSQL.AppendLine(";")
                    sbSQL.AppendLine("SELECT @RETURN_VALUE = @nRet")

                    params.Add(New SqlParameter($"@IN_PRODUCT_NO", lendingData("PRODUCT_NO").ToString))
                    params.Add(New SqlParameter($"@ASSET_NO", lendingData("ASSET_NO").ToString))
                    params.Add(New SqlParameter($"@IN_FROM_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentStartDtString), 0, CInt(CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd")))))
                    params.Add(New SqlParameter($"@IN_TO_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentEndDtString), 0, CInt(CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd")))))
                    params.Add(New SqlParameter($"@IN_KADO_JITU", sWorkDayFlg))
                    params.Add(New SqlParameter($"@IN_CONST_NO", ViewModel.CONST_NO))
                    params.Add(New SqlParameter($"@IN_CONST_RESERVE_ID", lendingData("RESERVE_ID").ToString))
                    params.Add(New SqlParameter($"@IN_REG_EMP_CD", loginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@IN_REG_EMP_NAME", loginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@IN_REG_TASK", messageRfid.getMessage(CommonLabelConstructionList.TITLE)))

                    Dim returnValueParam = New SqlParameter()
                    returnValueParam.ParameterName = $"@RETURN_VALUE"
                    returnValueParam.SqlDbType = SqlDbType.Int
                    returnValueParam.Direction = ParameterDirection.Output
                    params.Add(returnValueParam)

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    returnValue = params.Where(Function(x) x.ParameterName = "@RETURN_VALUE").FirstOrDefault.Value

                    '代替品番予約更新ストアドプロシージャエラー
                    If returnValue = AssetReplaceReservePossibleSpResult.TermError Then
                        Return (String.Format(messageRfid.getMessage(CommonLabelConstructionList.MSG_CANNOT_CHANGE_LENDING_ASSET), lendingData("ASSET_NO").ToString))
                    End If
                Next

                '予約中データ更新
                Dim reserveDataList = resDetail.Select("RESERVE_STATUS ='" + ReserveStatus.Mi + "'")
                For Each reserveData As DataRow In reserveDataList

                    sbSQL.Clear()
                    params = New List(Of SqlParameter)
                    returnValue = 0

                    sbSQL.AppendLine("DECLARE @nRet INT;")
                    sbSQL.AppendLine("EXEC @nRet =dbo.[ASSET_REPLACE_RESERVE_POSSIBLE_SP]")
                    sbSQL.AppendLine("            @IN_PRODUCT_NO")
                    sbSQL.AppendLine("            ,@ASSET_NO")
                    sbSQL.AppendLine("            ,@IN_FROM_DT")
                    sbSQL.AppendLine("            ,@IN_TO_DT")
                    sbSQL.AppendLine("            ,@IN_KADO_JITU")
                    sbSQL.AppendLine("            ,@IN_CONST_NO")
                    sbSQL.AppendLine("            ,@IN_CONST_RESERVE_ID")
                    sbSQL.AppendLine("            ,@IN_REG_EMP_CD")
                    sbSQL.AppendLine("            ,@IN_REG_EMP_NAME")
                    sbSQL.AppendLine("            ,@IN_REG_TASK")
                    sbSQL.AppendLine(";")
                    sbSQL.AppendLine("SELECT @RETURN_VALUE = @nRet")

                    params.Add(New SqlParameter($"@IN_PRODUCT_NO", reserveData("PRODUCT_NO").ToString))
                    params.Add(New SqlParameter($"@ASSET_NO", reserveData("ASSET_NO").ToString))
                    params.Add(New SqlParameter($"@IN_FROM_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentStartDtString), 0, CInt(CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd")))))
                    params.Add(New SqlParameter($"@IN_TO_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentEndDtString), 0, CInt(CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd")))))
                    params.Add(New SqlParameter($"@IN_KADO_JITU", sWorkDayFlg))
                    params.Add(New SqlParameter($"@IN_CONST_NO", ViewModel.CONST_NO))
                    params.Add(New SqlParameter($"@IN_CONST_RESERVE_ID", reserveData("RESERVE_ID").ToString))
                    params.Add(New SqlParameter($"@IN_REG_EMP_CD", loginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@IN_REG_EMP_NAME", loginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@IN_REG_TASK", messageRfid.getMessage(CommonLabelConstructionList.TITLE)))

                    Dim returnValueParam = New SqlParameter()
                    returnValueParam.ParameterName = $"@RETURN_VALUE"
                    returnValueParam.SqlDbType = SqlDbType.Int
                    returnValueParam.Direction = ParameterDirection.Output
                    params.Add(returnValueParam)

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    returnValue = params.Where(Function(x) x.ParameterName = "@RETURN_VALUE").FirstOrDefault.Value

                    '代替品番予約更新ストアドプロシージャエラー
                    If returnValue = AssetReplaceReservePossibleSpResult.ChangeError Then
                        Return (String.Format(messageRfid.getMessage(CommonLabelConstructionList.MSG_CANNOT_CHANGE_RESERVE), reserveData("ASSET_NO").ToString))
                    ElseIf returnValue = AssetReplaceReservePossibleSpResult.TermError Then
                        Return (String.Format(messageRfid.getMessage(CommonLabelConstructionList.MSG_CANNOT_CHANGE_TERM), reserveData("ASSET_NO").ToString))
                    End If
                Next

            Catch ex As Exception

                logger.Error(ex.Message)
                logger.Error(sbSQL)

                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return ("ok")
        End Function
#End Region
    End Class
End Namespace