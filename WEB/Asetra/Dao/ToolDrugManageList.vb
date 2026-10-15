Imports Asetra.Misc
Imports System.Data.SqlClient
Imports GrapeCity.Documents.Excel
Imports GrapeCity.Documents
Imports System.IO
Imports System.Drawing

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：管理簿出力関連ツール Data Access クラス
    ''' 作成日        ：2023/04/30      
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ToolDrugManageList
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        ''' <summary>シート名　ヘッダー</summary>
        Public Const SheetNm_Page_Header As String = "HEADER"
        ''' <summary>シート名　行ヘッダー</summary>
        Public Const SheetNm_Row_Header As String = "ROW_HEADER"
        ''' <summary>シート名　明細部(背景色　緑）</summary>
        Public Const SheetNm_Row_Light_Green As String = "ROW_LIGHT_GREEN"
        ''' <summary>シート名　明細部(背景色　白）</summary>
        Public Const SheetNm_Row_White As String = "ROW_WHITE"

#Region "管理簿PDF作成"
        ''' <summary>
        ''' 管理簿PDF作成
        ''' </summary>
        ''' <param name="sTempFullPath">テンプレートフォルダ　プルパス</param>
        ''' <param name="sOutFullPath">出力フォルダ　プルパス</param>
        ''' <param name="lstDrugManage">出力用データ</param>
        ''' <returns>
        ''' PDF作成ファイル名（フルパス）　エラー時、String.Empty
        ''' </returns>
        Public Shared Function MakeDrugManageList(ByVal sTempFullPath As String,
                                                  ByVal sOutFullPath As String,
                                                  ByVal lstDrugManage As List(Of DrugManageInfo),
                                                  ByVal dateFrom As String,
                                                  ByVal dateTo As String) As String

            'ライセンスキー
            GrapeCity.Documents.Excel.Workbook.SetLicenseKey("株式会社シービジョン,496378668853486#A03NJgzNzYTO4IiOiQWSisnOiQkIsISP3cWe6QTZ95WWvEmMYdTStZ5MltGdv26dWlWazcmYBREamhXNyUEVRdGbOl6SuREarg4L6dVRwZnUz4EZFZzKzYmZv2Ee7gje9tWb5IzZ6x6arQGUVl5UiojITJCL7YzM8cTM8gDO0IicfJye#4Xfd5nI9g4NyIiOiMkIsIiMWBCdl9kLgwWZjhXRgI7bmBCduVWb5N6bEByQHJiOi8kI1tlOiQmcQJCLiEDNzIDNwAiNwITM9EDMyIiOiQncDJCLiM1gjf0gjjrgjP9gjz1gjfrgj1LpnrJvk/IvlrKomLiOiEmTDJCLiYDO4M5Ntg")

            'テンプレートEXCEL用
            Dim wbTemplate As IWorkbook = New Workbook
            Dim wsTemplate As IWorksheet

            'テンプレートファイル読込み
            wbTemplate.Open(sTempFullPath & "\" & CommonConst.DRUG_MANAGE_LIST_TEMPLATE_FILENM)

            ' 新規ワークブックの作成
            Dim wbNew As New Workbook

            ' 新規ワークシート
            Dim wsNewSheet As IWorksheet = Nothing

            'PDF一時ファイル名
            Dim sPdfFileNm As String = sOutFullPath & "\" & Path.GetRandomFileName() & ".pdf"

            'データセット用の文言を取得
            Dim sHeaderDrugNm As String = CommonRfid.MessageRfid.getMessage(CommonLabel.LABEL_DRUG_NAME_SHORT) & "："               '保管場所：

            'テンプレートシート取得
            Dim wsTempRowHeader As IWorksheet = wbTemplate.Worksheets(SheetNm_Row_Header)               '行ヘッダー
            Dim wsTempRowLightGreen As IWorksheet = wbTemplate.Worksheets(SheetNm_Row_Light_Green)      '明細部(緑)
            Dim wsTempRowWhite As IWorksheet = wbTemplate.Worksheets(SheetNm_Row_White)                 '明細部(白)
            Dim irRowHeader As IRange = wsTempRowHeader.Range("A1:L2")
            Dim irRowGreen As IRange = wsTempRowLightGreen.Range("A1:L1")
            Dim irRowWhite As IRange = wsTempRowWhite.Range("A1:L1")

            Const DETAIL_INIT_I As Integer = 7

            'ループ内で使用する変数
            Dim row_i As Integer = DETAIL_INIT_I                '※明細部が始まる行数を指定
            Dim newHeaderFlg As Boolean                         '改ページフラグ
            Dim rowPrev As DrugManageInfo = Nothing             '前回ループ時の値保持用
            Dim dConvCheck As Decimal                           '数値チェック用

            For Each row In lstDrugManage
                newHeaderFlg = False

                '◆次のページへ進むかどうかを判定
                If row_i > (DETAIL_INIT_I + 39) Then
                    '明細部が40行出力済み
                    rowPrev = Nothing
                    row_i = DETAIL_INIT_I

                ElseIf Not IsNothing(rowPrev) Then
                    '明細部の1行目以降

                    If rowPrev.StorageLoc <> row.StorageLoc Then
                        '保管場所が異なる場合
                        rowPrev = Nothing
                        row_i = DETAIL_INIT_I

                    ElseIf rowPrev.RelateLaw <> row.RelateLaw Then
                        '関係法令が異なる場合
                        rowPrev = Nothing
                        row_i = DETAIL_INIT_I

                    ElseIf row_i >= (DETAIL_INIT_I + 37) AndAlso rowPrev.AssetName <> row.AssetName Then
                        '37行目以降　かつ　資産名が異なる場合
                        rowPrev = Nothing
                        row_i = DETAIL_INIT_I
                    End If
                End If

                '◆ページヘッダーと行ヘッダーの内、どちらを作成するかを判定
                If row_i = DETAIL_INIT_I Then
                    'ページヘッダーを作成

                    'テンプレートシート取得する
                    wsTemplate = wbTemplate.Worksheets(SheetNm_Page_Header)
                    'テンプレートシートを次のシートにコピーする
                    wsNewSheet = wsTemplate.Copy(wbNew)

                    wsNewSheet.Range("STORAGE_LOC").Value = row.StorageLoc                                      '保管場所
                    wsNewSheet.Range("EXPAND_PERIOD").Value = dateFrom & "～" & dateTo                          '抽出期間
                    wsNewSheet.Range("RELATED_LAW").Value = row.RelateLaw                                       '関係法令
                    newHeaderFlg = True
                Else
                    '行ヘッダーを作成
                    If Not IsNothing(rowPrev) Then
                        If rowPrev.AssetName <> row.AssetName OrElse rowPrev.InternalQty <> row.InternalQty Then
                            '資産名が異なる場合、行ヘッダーを作成
                            '※行ヘッダー追加時は、1行間隔をあける
                            irRowHeader.Copy(wsNewSheet.Range($"A{row_i + 1}:L{row_i + 1}"))
                            newHeaderFlg = True
                        End If
                    End If
                End If

                '◆新ヘッダー設定
                If newHeaderFlg Then
                    If row_i = DETAIL_INIT_I Then
                        '新ページ追加時
                        wsNewSheet.Range($"E{row_i}").Value = row.InternalQty                                   '内容量
                        wsNewSheet.Range($"F{row_i}").Value = row.InternalQtyUnit                               '内容量単位
                        wsNewSheet.Range($"G{row_i}").Value = sHeaderDrugNm & row.AssetName                     '資産名
                        row_i += 2
                    Else
                        '行ヘッダー追加時
                        '※ブランク1行分も考慮
                        wsNewSheet.Range($"E{row_i + 1}").Value = row.InternalQty                               '内容量
                        wsNewSheet.Range($"F{row_i + 1}").Value = row.InternalQtyUnit                           '内容量単位
                        wsNewSheet.Range($"G{row_i + 1}").Value = sHeaderDrugNm & row.AssetName                 '資産名
                        row_i += 3
                    End If
                End If

                '◆明細データ作成
                '明細行レイアウトをコピー
                If row_i Mod 2 = 0 Then
                    irRowWhite.Copy(wsNewSheet.Range($"A{row_i}:L{row_i}"))
                Else
                    irRowGreen.Copy(wsNewSheet.Range($"A{row_i}:L{row_i}"))
                End If

                'データセット
                wsNewSheet.Range($"A{row_i}").Value = row.ProcessDt                                             '日付
                wsNewSheet.Range($"B{row_i}").Value = row.LotNo                                                 '使用ロット

                If Decimal.TryParse(row.UseQty, dConvCheck) Then                                                '使用量
                    wsNewSheet.Range($"C{row_i}").Value = dConvCheck
                Else
                    wsNewSheet.Range($"C{row_i}").Value = row.UseQty
                End If

                If Decimal.TryParse(row.RemainQty, dConvCheck) Then                                             '残量
                    wsNewSheet.Range($"E{row_i}").Value = dConvCheck
                Else
                    wsNewSheet.Range($"E{row_i}").Value = row.RemainQty
                End If

                wsNewSheet.Range($"G{row_i}").Value = row.SpecialReport                                         '特記事項
                wsNewSheet.Range($"H{row_i}").Value = row.WorkerEmp                                             '作業者
                wsNewSheet.Range($"I{row_i}").Value = row.WorkOutline                                           '作業概要
                wsNewSheet.Range($"J{row_i}").Value = row.WorkTime                                              '作業時間
                wsNewSheet.Range($"L{row_i}").Value = row.ResposeEmp                                            '取扱責任者

                rowPrev = row
                row_i += 1
            Next

            Dim printManager As New Excel.PrintManager

            'フッターにページ番号を表示するように指定
            Dim pages As IList(Of PageInfo) = printManager.Paginate(wbNew)
            For Each page In pages
                '※フォントサイズ20
                page.PageSettings.CenterFooter = "&20 &P"
            Next

            ' プリントマネージャーでPDFに保存
            Dim outputStream = System.IO.File.Create(sPdfFileNm)
            printManager.SavePDF(outputStream, pages)
            outputStream.Close()


            Return sPdfFileNm

        End Function
#End Region

#Region "管理簿出力用データ取得"
        ''' <summary>
        ''' 管理簿出力用データ取得
        ''' </summary>
        Public Shared Function GetDrugManageListData(Optional ByVal iProcessDtFrom As Integer = -1,
                                                     Optional ByVal iProcessDtTo As Integer = -1,
                                                     Optional ByVal sLocLCd As String = "",
                                                     Optional ByVal sLocMCd As String = "",
                                                     Optional ByVal sLocSCd As String = "",
                                                     Optional ByVal sCatLCd As String = "",
                                                     Optional ByVal sCatMCd As String = "",
                                                     Optional ByVal sCatSCd As String = "",
                                                     Optional ByVal sAssetNo As String = "",
                                                     Optional ByVal sAssetNm As String = "") As List(Of DrugManageInfo)
            Dim lstDrugManage As New List(Of DrugManageInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  ASSET_NO ")
                sbSQL.AppendLine("  , ASSET_NAME ")
                sbSQL.AppendLine("  , MODEL_NO ")
                sbSQL.AppendLine("  , FREE_ITEM4 ")
                sbSQL.AppendLine("  , FREE_ITEM5 ")
                sbSQL.AppendLine("  , FREE_ITEM6 ")
                sbSQL.AppendLine("  , FREE_ITEM11 ")
                sbSQL.AppendLine("  , FREE_ITEM12 ")
                sbSQL.AppendLine("  , FREE_ITEM13 ")
                sbSQL.AppendLine("  , CASE WHEN ISNULL(FREE_ITEM10, '') = '' THEN FREE_ITEM11 ELSE FREE_ITEM10 END AS INTERNAL_QTY ")
                sbSQL.AppendLine("  , CASE WHEN ISNULL(FREE_ITEM10, '') = '' THEN 'L' ELSE 'kg' END AS INTERNAL_QTY_UNIT ")
                sbSQL.AppendLine("  , DRUG_WORK_DIV ")
                sbSQL.AppendLine("  , FORMAT(CONVERT(DATE, CONVERT(NVARCHAR, PROCESS_DT)), 'yyyy/MM/dd') AS PROCESS_DT ")
                sbSQL.AppendLine("  , CONVERT(INT, PROCESS_TM) AS PROCESS_TM ")
                sbSQL.AppendLine("  , WORKER_EMPLOYEE_CD ")
                sbSQL.AppendLine("  , RESPONSE_EMPLOYEE_CD ")
                sbSQL.AppendLine("  , USE_QTY ")
                sbSQL.AppendLine("  , REMAIN_QTY ")
                sbSQL.AppendLine("  , WORK_OUTLINE ")
                sbSQL.AppendLine("  , WORK_TIME ")
                sbSQL.AppendLine("  , LOC_NAME AS LOC_M_NAME ")
                sbSQL.AppendLine("  , WORK_EMP.EMPLOYEE_NAME AS WORK_EMP_NAME ")
                sbSQL.AppendLine("  , RES_EMP.EMPLOYEE_NAME AS RES_EMP_NAME ")
                sbSQL.AppendLine("  , RANK() OVER (PARTITION BY ASSET_NAME ORDER BY PROCESS_DT) AS RANK ")
                sbSQL.AppendLine("FROM ( ")
                sbSQL.AppendLine("      SELECT ")
                sbSQL.AppendLine("          ASSET.ASSET_NO ")
                sbSQL.AppendLine("          , ASSET.ASSET_NAME ")
                sbSQL.AppendLine("          , ASSET.MODEL_NO ")
                sbSQL.AppendLine("          , ASSET.FREE_ITEM4 ")
                sbSQL.AppendLine("          , ASSET.FREE_ITEM5 ")
                sbSQL.AppendLine("          , ASSET.FREE_ITEM6 ")
                sbSQL.AppendLine("          , ASSET.FREE_ITEM10 ")
                sbSQL.AppendLine("          , ASSET.FREE_ITEM11 ")
                sbSQL.AppendLine("          , ASSET.FREE_ITEM12 ")
                sbSQL.AppendLine("          , ASSET.FREE_ITEM13 ")
                sbSQL.AppendLine("          , DUE_LOG.DRUG_WORK_DIV ")
                sbSQL.AppendLine("          , DUE_LOG.PROCESS_DT ")
                sbSQL.AppendLine("          , DUE_LOG.PROCESS_TM ")
                sbSQL.AppendLine("          , DUE_LOG.WORKER_EMPLOYEE_CD ")
                sbSQL.AppendLine("          , DUE_LOG.RESPONSE_EMPLOYEE_CD ")
                sbSQL.AppendLine("          , DUE_LOG.USE_QTY ")
                sbSQL.AppendLine("          , DUE_LOG.REMAIN_QTY ")
                sbSQL.AppendLine("          , DUE_LOG.WORK_OUTLINE ")
                sbSQL.AppendLine("          , DUE_LOG.WORK_TIME ")
                sbSQL.AppendLine("          , LOCM.LOC_M_CD ")
                sbSQL.AppendLine("          , ISNULL(LOCL.LOC_NAME, '') + ' ' + ISNULL(LOCM.LOC_NAME, '') AS LOC_NAME")
                sbSQL.AppendLine("      FROM ")
                sbSQL.AppendLine("          M_ASSET ASSET ")
                sbSQL.AppendLine("          LEFT JOIN T_DRUG_DUE_LOG DUE_LOG ")
                sbSQL.AppendLine("              ON ASSET.ASSET_NO = DUE_LOG.ASSET_NO ")
                sbSQL.AppendLine("          LEFT JOIN M_LOCATION LOCL ")
                sbSQL.AppendLine($"             ON LOCL.LOC_DIV = '{LOC_DIV.LocationL}' ")
                sbSQL.AppendLine("              AND ASSET.LOC_L_CD = LOCL.LOC_L_CD ")
                sbSQL.AppendLine("          LEFT JOIN M_LOCATION LOCM ")
                sbSQL.AppendLine($"             ON LOCM.LOC_DIV = '{LOC_DIV.LocationM}' ")
                sbSQL.AppendLine("              AND ASSET.LOC_L_CD = LOCM.LOC_L_CD ")
                sbSQL.AppendLine("              AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
                sbSQL.AppendLine("          WHERE 1 = 1 ")
                If iProcessDtFrom <> -1 Then
                    sbSQL.AppendLine($"  AND DUE_LOG.PROCESS_DT >= {iProcessDtFrom} ")
                End If
                If iProcessDtTo <> -1 Then
                    sbSQL.AppendLine($"  AND DUE_LOG.PROCESS_DT <= {iProcessDtTo} ")
                End If
                If sLocLCd <> "" Then
                    sbSQL.AppendLine($"  AND ASSET.LOC_L_CD = '{sLocLCd}' ")
                End If
                If sLocMCd <> "" Then
                    sbSQL.AppendLine($"  AND ASSET.LOC_M_CD = '{sLocMCd}' ")
                End If
                If sLocSCd <> "" Then
                    sbSQL.AppendLine($"  AND ASSET.LOC_S_CD = '{sLocSCd}' ")
                End If
                If sCatLCd <> "" Then
                    sbSQL.AppendLine($"  AND ASSET.CAT_L_CD = '{sCatLCd}' ")
                End If
                If sCatMCd <> "" Then
                    sbSQL.AppendLine($"  AND ASSET.CAT_M_CD = '{sCatMCd}' ")
                End If
                If sCatSCd <> "" Then
                    sbSQL.AppendLine($"  AND ASSET.CAT_S_CD = '{sCatSCd}' ")
                End If
                If sAssetNo <> "" Then
                    sbSQL.AppendLine($"  AND ASSET.ASSET_NO LIKE '{sAssetNo}%' ")
                End If
                If sAssetNm <> "" Then
                    sbSQL.AppendLine($"  AND ASSET.ASSET_NAME LIKE '%{sAssetNm}%' ")
                End If
                sbSQL.AppendLine("      ) AS DRUG_MANAGE ")
                sbSQL.AppendLine("      LEFT JOIN M_EMPLOYEE WORK_EMP ")
                sbSQL.AppendLine("          ON DRUG_MANAGE.WORKER_EMPLOYEE_CD = WORK_EMP.EMPLOYEE_CD ")
                sbSQL.AppendLine("      LEFT JOIN M_EMPLOYEE RES_EMP ")
                sbSQL.AppendLine("          ON DRUG_MANAGE.RESPONSE_EMPLOYEE_CD = RES_EMP.EMPLOYEE_CD ")
                sbSQL.AppendLine("ORDER BY ")
                sbSQL.AppendLine("  LOC_M_CD ")
                sbSQL.AppendLine("  , CASE ")
                sbSQL.AppendLine($"      WHEN FREE_ITEM5 IN ('{PoisonAndDeleterious.Poison}', '{PoisonAndDeleterious.Deleterious}') THEN 1 ")
                sbSQL.AppendLine($"      WHEN FREE_ITEM6 = '{ValidFlg.Cd.Valid}' THEN 2 ")
                sbSQL.AppendLine($"      WHEN FREE_ITEM4 = '{ValidFlg.Cd.Valid}'  THEN 3 ")
                sbSQL.AppendLine("  ELSE 99 END ")
                sbSQL.AppendLine("  , CONVERT(DATE, FREE_ITEM13) ")
                sbSQL.AppendLine("  , ASSET_NAME ")
                sbSQL.AppendLine("  , INTERNAL_QTY ")
                sbSQL.AppendLine("  , MODEL_NO ")
                sbSQL.AppendLine("  , DRUG_WORK_DIV ")
                sbSQL.AppendLine("  , PROCESS_DT ")
                sbSQL.AppendLine("  , PROCESS_TM ")

                Dim tbl As New DataTable
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                'データが存在しない場合
                If IsNothing(tbl) OrElse tbl.Rows.Count <= 0 Then
                    Return Nothing
                End If

                Dim dConvCheck As Decimal

                For Each dtRow In tbl.Rows
                    Dim rowDrugManage As New DrugManageInfo
                    With rowDrugManage
                        '保管場所
                        rowDrugManage.StorageLoc = CommonRfid.ToStringNvl(dtRow("LOC_M_NAME"))

                        '関係法令
                        rowDrugManage.RelateLaw = ConvertWordRelateLaw(CommonRfid.ToStringNvl(dtRow("FREE_ITEM6")),
                                                                       CommonRfid.ToStringNvl(dtRow("FREE_ITEM5")),
                                                                       CommonRfid.ToStringNvl(dtRow("FREE_ITEM4")))

                        '資産番号
                        rowDrugManage.AssetNo = CommonRfid.ToStringNvl(dtRow("ASSET_NO"))

                        '資産名
                        rowDrugManage.AssetName = CommonRfid.ToStringNvl(dtRow("ASSET_NAME"))

                        '日付
                        rowDrugManage.ProcessDt = CommonRfid.ToStringNvl(dtRow("PROCESS_DT"))

                        '使用ロット
                        rowDrugManage.LotNo = CommonRfid.ToStringNvl(dtRow("MODEL_NO"))

                        '作業者
                        rowDrugManage.WorkerEmp = CommonRfid.ToStringNvl(dtRow("WORK_EMP_NAME"))

                        '責任者
                        rowDrugManage.ResposeEmp = CommonRfid.ToStringNvl(dtRow("RES_EMP_NAME"))

                        '作業概要
                        If CommonRfid.ToStringNvl(dtRow("WORK_OUTLINE")) = String.Empty Then
                            rowDrugManage.WorkOutline = "ー"
                        Else
                            rowDrugManage.WorkOutline = CommonRfid.ToStringNvl(dtRow("WORK_OUTLINE"))
                        End If

                        '作業時間
                        If CommonRfid.ToStringNvl(dtRow("WORK_TIME")) = String.Empty Then
                            rowDrugManage.WorkTime = "ー"
                        Else
                            rowDrugManage.WorkTime = CommonRfid.ToStringNvl(dtRow("WORK_TIME"))
                        End If

                        '内容量（内容量kgが未設定の場合、内容量L）
                        If Decimal.TryParse(CommonRfid.ToStringNvl(dtRow("INTERNAL_QTY")), dConvCheck) Then
                            rowDrugManage.InternalQty = (dConvCheck).ToString
                        Else
                            rowDrugManage.InternalQty = CommonRfid.ToStringNvl(dtRow("INTERNAL_QTY"))
                        End If

                        '内容量単位
                        rowDrugManage.InternalQtyUnit = CommonRfid.ToStringNvl(dtRow("INTERNAL_QTY_UNIT"))

                        '使用量
                        If CommonRfid.ToStringNvl(dtRow("USE_QTY")) = String.Empty Then
                            rowDrugManage.UseQty = "ー"
                        Else
                            rowDrugManage.UseQty = CommonRfid.ToStringNvl(dtRow("USE_QTY"))
                        End If

                        '残量
                        rowDrugManage.RemainQty = CommonRfid.ToStringNvl(dtRow("REMAIN_QTY"))

                        '特記事項
                        rowDrugManage.SpecialReport = ConvertWordSpecialRepo(CommonRfid.ToStringNvl(dtRow("DRUG_WORK_DIV")))

                    End With

                    lstDrugManage.Add(rowDrugManage)
                Next

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return lstDrugManage

        End Function
#End Region

        '-- Private 関数
#Region "関係法令文言取得処理"
        ''' <summary>
        ''' 関係法令に該当する文言を取得
        ''' </summary>
        ''' <param name="sSpecific">特化則の該当有無</param>
        ''' <param name="sPoisonAndDeleterious">毒劇物の有無</param>
        ''' <param name="sOrganic">有規則の該当有無</param>
        ''' <returns></returns>
        Private Shared Function ConvertWordRelateLaw(ByVal sSpecific As String, ByVal sPoisonAndDeleterious As String, ByVal sOrganic As String) As String
            Try
                If sSpecific = ValidFlg.Cd.Valid Then
                    '特化則が有
                    Return CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_SPECIFIC)

                ElseIf sPoisonAndDeleterious = PoisonAndDeleterious.Poison OrElse
                       sPoisonAndDeleterious = PoisonAndDeleterious.Deleterious Then
                    '毒劇物が有
                    Return CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_POISOUNUOS)

                ElseIf sOrganic = ValidFlg.Cd.Valid Then
                    '有規則が有
                    Return CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_ORGANIC)

                Else
                    '該当なしの場合は有規則を返す
                    Return CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_ORGANIC)
                End If

            Catch ex As Exception
                logger.Error(ex)
                Return ""
            End Try
        End Function
#End Region

#Region "特記事項文言取得処理"
        ''' <summary>
        ''' 特記事項に該当する文言を取得
        ''' </summary>
        ''' <param name="sDrugWorkDiv">薬品作業区分</param>
        ''' <returns></returns>
        Private Shared Function ConvertWordSpecialRepo(ByVal sDrugWorkDiv As String) As String
            Try
                '薬品作業区分により文言を取得
                Select Case sDrugWorkDiv
                    Case DrugsWorkDiv.Accept
                        '受入
                        Return CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_ACCEPT)
                    Case DrugsWorkDiv.Confirm
                        '確認
                        Return CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_STOCK_QTY_CHECK_G)
                    Case Else
                        Return ""
                End Select

            Catch ex As Exception
                logger.Error(ex)
                Return ""
            End Try
        End Function
#End Region
    End Class

End Namespace

