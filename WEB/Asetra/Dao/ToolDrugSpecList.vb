Imports System.Data.SqlClient
Imports System.Drawing
Imports System.IO
Imports Asetra.Misc
Imports GrapeCity.Documents
Imports GrapeCity.Documents.Excel

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：指定数量管理表関連ツール系 Data Access クラス
    ''' 作成日        ：2023/05/01      
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolDrugSpecList
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        ''' <summary>シート名　ヘッダーあり帳票</summary>
        Public Const SheetNm_Header_Report As String = "HEADER_REPORT"
        ''' <summary>シート名　ヘッダーなし帳票</summary>
        Public Const SheetNm_Report As String = "REPORT"

#Region "【クラス定義】"

#Region "帳票セット用 日次重量履歴データクラス"
        Private Class DailyHistData
            '-----インスタンス変数-----
            ''' <summary>重量履歴情報</summary>
            Private _DueHistInfo As DrugSpecInfo.DueHist
            ''' <summary>列番号</summary>
            Private _iColumn As Integer

            '-----プロパティ-----
            ''' <summary>重量履歴情報</summary>
            Public Property DueHistInfo() As DrugSpecInfo.DueHist
                Get
                    Return _DueHistInfo
                End Get
                Set(ByVal value As DrugSpecInfo.DueHist)
                    _DueHistInfo = value
                End Set
            End Property
            ''' <summary>列番号</summary>
            Public Property Column() As Integer
                Get
                    Return _iColumn
                End Get
                Set(ByVal value As Integer)
                    _iColumn = value
                End Set
            End Property
        End Class
#End Region

#End Region

#Region "指定数量管理表PDF作成"
        ''' <summary>
        ''' 指定数量管理表PDF作成
        ''' </summary>
        ''' <param name="sTempFullPath">テンプレートフォルダ　プルパス</param>
        ''' <param name="sOutFullPath">出力フォルダ　プルパス</param>
        ''' <param name="lstDrugSpec">出力用データ</param>
        ''' <returns>
        ''' PDF作成ファイル名（フルパス）　エラー時、String.Empty
        ''' </returns>
        Public Shared Function MakeDrugSpecList(ByVal sTempFullPath As String,
                                                ByVal sOutFullPath As String,
                                                ByVal lstDrugSpec As List(Of DrugSpecInfo)) As String

            'ライセンスキー
            GrapeCity.Documents.Excel.Workbook.SetLicenseKey("株式会社シービジョン,496378668853486#A03NJgzNzYTO4IiOiQWSisnOiQkIsISP3cWe6QTZ95WWvEmMYdTStZ5MltGdv26dWlWazcmYBREamhXNyUEVRdGbOl6SuREarg4L6dVRwZnUz4EZFZzKzYmZv2Ee7gje9tWb5IzZ6x6arQGUVl5UiojITJCL7YzM8cTM8gDO0IicfJye#4Xfd5nI9g4NyIiOiMkIsIiMWBCdl9kLgwWZjhXRgI7bmBCduVWb5N6bEByQHJiOi8kI1tlOiQmcQJCLiEDNzIDNwAiNwITM9EDMyIiOiQncDJCLiM1gjf0gjjrgjP9gjz1gjfrgj1LpnrJvk/IvlrKomLiOiEmTDJCLiYDO4M5Ntg")

            'テンプレートEXCEL用
            Dim wbTemplate As IWorkbook = New Workbook
            Dim wsTemplate As IWorksheet

            'テンプレートファイル読込み
            wbTemplate.Open(sTempFullPath & "\" & CommonConst.DRUG_SPEC_LIST_TEMPLATE_FILENM)

            ' 新規ワークブックの作成
            Dim wbNew As New Workbook

            ' 新規ワークシート
            Dim wsNewSheet As IWorksheet = Nothing

            'PDF一時ファイル名
            Dim sPdfFileNm As String = sOutFullPath & "\" & Path.GetRandomFileName() & ".pdf"

            'データセット用の文言を取得
            Dim sHeaderLocNm As String = CommonRfid.MessageRfid.getMessage(CommonLabel.LABEL_STORAGE_LOCATION) & "："               '保管場所：
            Dim sHeaderSpecifiedQty As String = CommonRfid.MessageRfid.getMessage(CommonLabel.LABEL_SPECIFIED_QTY) & vbCrLf &       '指定数量 + 改行コード + 
                                                CommonRfid.MessageRfid.getMessage(CommonLabel.LABEL_ALL_DRUG_TOTAL) & vbCrLf        '(全在庫薬品の合算) + 改行コード

            '帳票出力用 明細データリスト(日次重量履歴)
            Dim lstDailyHistData As New List(Of DailyHistData)

            '固定値
            Const pageAssetMaxCnt As Integer = 5

            'カウンタ準備
            Dim i As Integer = 0
            Dim asset_i As Integer = 1
            Dim processDtRow_i As Integer = 0

            For Each row As DrugSpecInfo In lstDrugSpec
                '◆データセット用の雛形作成　
                If i Mod pageAssetMaxCnt = 0 Then
                    '※資産5件ごとに新しいシートを作成する
                    '資産情報テンプレートシート取得し、次のシートへコピーする
                    wsTemplate = wbTemplate.Worksheets(SheetNm_Report)
                    wsNewSheet = wsTemplate.Copy(wbNew)

                    'ヘッダー関連のデータをセット
                    wsNewSheet.Range("STORAGE_LOC").Value = sHeaderLocNm & row.StorageLoc
                    wsNewSheet.Range("SPECIFIED_QTY_HEADER").Value = sHeaderSpecifiedQty & String.Format(CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_REQUIRED_LESS_THAN_VALUE), CommonRfid.ToStringNvl(row.LocSpecifiedQty))

                    'カウンタ初期化
                    asset_i = 1

                    lstDailyHistData.Clear()
                End If

                '◆データをセット
                '資産情報を帳票にセット
                wsNewSheet.Range("ASSET_NAME" & (asset_i)).Value = row.AssetNm
                wsNewSheet.Range("CAS_NO" & (asset_i)).Value = row.CasNo
                wsNewSheet.Range("FIRE_SERVICE_ACT" & (asset_i)).Value = row.FireServiceAct
                wsNewSheet.Range("ORGANIC_RULE" & (asset_i)).Value = row.OrganicRule
                wsNewSheet.Range("POISOUNUOS_RULE" & (asset_i)).Value = row.PoisonuosRule
                wsNewSheet.Range("SPECIFIC_RULE" & (asset_i)).Value = row.SpecificRule
                wsNewSheet.Range("PRTR" & (asset_i)).Value = row.PRTR
                wsNewSheet.Range("CHEMICAL_SUBSTANCE" & (asset_i)).Value = row.ChemicalSubstance
                wsNewSheet.Range("PURCHASER" & (asset_i)).Value = row.Purchaser
                wsNewSheet.Range("INTERNAL_CAPACITY_KG" & (asset_i)).Value = CommonRfid.ToStringNvl(row.InternalCapKg)
                wsNewSheet.Range("INTERNAL_CAPACITY_L" & (asset_i)).Value = CommonRfid.ToStringNvl(row.InternalCapL)
                wsNewSheet.Range("SPECIFIC_GRAVITY" & (asset_i)).Value = CommonRfid.ToStringNvl(row.SpecificGravity)
                wsNewSheet.Range("DUE_DATE" & (asset_i)).Value = row.DueDate
                wsNewSheet.Range("INITIAL_WEIGHT" & (asset_i)).Value = row.InitWeight
                wsNewSheet.Range("SPECIFIED_QTY_L" & (asset_i)).Value = CommonRfid.ToStringNvl(row.SpecifiedQtyL)
                If Not IsNothing(row.SpecificGravity) AndAlso Not IsNothing(row.SpecifiedQtyL) Then
                    '比重 × 指定数量（L）
                    wsNewSheet.Range("SPECIFIED_QTY_KG" & (asset_i)).Value = row.SpecificGravity * row.SpecifiedQtyL
                End If

                '日次重量履歴を帳票セット用の形式でリスト保持
                For Each rowDueHist In row.LstDueHist
                    lstDailyHistData.Add(New DailyHistData With {.DueHistInfo = rowDueHist, .Column = asset_i})
                Next

                '日次重量履歴を帳票にセット
                If asset_i = pageAssetMaxCnt OrElse i = (lstDrugSpec.Count - 1) Then
                    '5件目の資産もしくは最終資産の場合
                    processDtRow_i = 0

                    '確認日順の昇順ソート
                    Dim lstDailyHistAsc = lstDailyHistData.OrderBy(Function(x) Date.Parse(x.DueHistInfo.ProcessDt))

                    For Each rowDailyHist In lstDailyHistAsc
                        With rowDailyHist

                            '※確認日を比較し、異なっていれば次の行に値をセットするためカウントアップ
                            If Not IsNothing(wsNewSheet.Range("PROCESS_DT").Cells(processDtRow_i).Value) Then
                                '1行目の確認日がセット済み
                                If wsNewSheet.Range("PROCESS_DT").Cells(processDtRow_i).Value <> .DueHistInfo.ProcessDt Then
                                    '確認日が異なる場合

                                    '1行分の指定数量の合計をセット
                                    wsNewSheet.Range("SPECIFIED_QTY").Cells(processDtRow_i).Value = sumSpecifedQty(lstDrugSpec, CommonRfid.ToStringNvl(wsNewSheet.Range("PROCESS_DT").Cells(processDtRow_i).Value))

                                    processDtRow_i += 1
                                End If
                            End If

                            '明細データが40行を超える場合、次のシートの1行目から続きを出力する
                            If processDtRow_i <> 0 AndAlso processDtRow_i Mod 40 = 0 Then

                                '前シートの資産情報を抽出
                                Dim assetInfoRange As IRange = wsNewSheet.Range("ASSET_INFO")

                                '資産情報テンプレートシート取得し、次のシートへコピーする
                                wsTemplate = wbTemplate.Worksheets(SheetNm_Report)
                                wsNewSheet = wsTemplate.Copy(wbNew)

                                'ヘッダー関連のデータをセット
                                wsNewSheet.Range("STORAGE_LOC").Value = sHeaderLocNm & row.StorageLoc
                                wsNewSheet.Range("SPECIFIED_QTY_HEADER").Value = sHeaderSpecifiedQty & String.Format(CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_REQUIRED_LESS_THAN_VALUE), CommonRfid.ToStringNvl(row.LocSpecifiedQty))

                                '新シートに前シートの資産情報をコピー
                                assetInfoRange.Copy(wsNewSheet.Range("ASSET_INFO"))

                                processDtRow_i = 0
                            End If

                            'データセット
                            wsNewSheet.Range("PROCESS_DT").Cells(processDtRow_i).Value = .DueHistInfo.ProcessDt
                            wsNewSheet.Range("DUE_QTY" & (.Column)).Cells(processDtRow_i).Value = CommonRfid.ToStringNvl(.DueHistInfo.DueQty)
                            wsNewSheet.Range("REMAIN_QTY" & (.Column)).Cells(processDtRow_i).Value = CommonRfid.ToStringNvl(.DueHistInfo.RemainQty)

                            If rowDailyHist Is lstDailyHistAsc.LastOrDefault Then
                                '最終レコードの場合、1行分の指定数量の合計をセット
                                wsNewSheet.Range("SPECIFIED_QTY").Cells(processDtRow_i).Value = sumSpecifedQty(lstDrugSpec, CommonRfid.ToStringNvl(wsNewSheet.Range("PROCESS_DT").Cells(processDtRow_i).Value))
                            End If
                        End With
                    Next
                End If

                i += 1
                asset_i += 1
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

#Region "指定数量管理表出力用データ取得"
        ''' <summary>
        ''' 指定数量管理表出力用データ取得
        ''' </summary>
        ''' <param name="sLocLCd">ロケーション大区分コード</param>
        ''' <param name="iProcessDtFrom">処理日（From）</param>
        ''' <param name="iProcessDtTo">処理日（To）</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetDrugSpecListData(Optional ByVal sLocLCd As String = "", Optional ByVal iProcessDtFrom As Integer = -1, Optional ByVal iProcessDtTo As Integer = -1) As List(Of DrugSpecInfo)
            Dim lstDrugSpecInfo As New List(Of DrugSpecInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  FORMAT(CONVERT(DATE, CONVERT(NVARCHAR, LOG_DAILY.PROCESS_DT)), 'yyyy/MM/dd') AS PROCESS_DT ")
                sbSQL.AppendLine("  , ASSET.ASSET_NO ")
                sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM2 AS CAS_NO ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM3 AS FIRE_SERVICE_ACT ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM4 AS ORGANIC_RULE ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM5 AS POISOUNUOS_RULE ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM6 AS SPECIFIC_RULE ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM7 AS PRTR ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM8 AS CHEMICAL_SUBSTANCE ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM9 AS PURCHASER ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM10 AS INTERNAL_CAPACITY_KG ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM11 AS INTERNAL_CAPACITY_L ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM12 AS SPECIFIC_GRAVITY ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM13 AS DUE_DATE ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM14 AS INITIAL_WEIGHT ")
                sbSQL.AppendLine("  , LOCL.SPECIFIED_QTY ")
                sbSQL.AppendLine("  , LOCL.LOC_NAME ")
                sbSQL.AppendLine("  , SETT_QTY.DATA2 AS SPECIFIED_QTY_L ")
                sbSQL.AppendLine("  , LOG_DAILY.DUE_QTY ")
                sbSQL.AppendLine("  , LOG_DAILY.REMAIN_QTY ")
                sbSQL.AppendLine("  , RANK() OVER (PARTITION BY ASSET.ASSET_NO ORDER BY LOG_DAILY.PROCESS_DT) AS RANK ")
                sbSQL.AppendLine("FROM ")
                sbSQL.AppendLine("  M_ASSET ASSET ")
                sbSQL.AppendLine("  INNER JOIN T_DRUG_LOG_DAILY LOG_DAILY ")
                sbSQL.AppendLine("      ON ASSET.ASSET_NO = LOG_DAILY.ASSET_NO ")
                sbSQL.AppendLine("  LEFT JOIN M_LOCATION LOCL ")
                sbSQL.AppendLine("      ON LOCL.LOC_DIV = 'L'")
                sbSQL.AppendLine("      AND ASSET.LOC_L_CD = LOCL.LOC_L_CD ")
                sbSQL.AppendLine("  LEFT JOIN M_SETTING SETT_QTY")
                sbSQL.AppendLine("      ON SETT_QTY.KEY_DIV = 'Drugs_FSAct_Number' ")
                sbSQL.AppendLine("      AND ASSET.FREE_ITEM3 = SETT_QTY.DATA1 ")
                sbSQL.AppendLine("WHERE ")
                sbSQL.AppendLine("  1 = 1 ")
                If sLocLCd <> "" Then
                    sbSQL.AppendLine($"  AND ASSET.LOC_L_CD = '{sLocLCd}' ")
                End If
                If iProcessDtFrom <> -1 Then
                    sbSQL.AppendLine($"  AND LOG_DAILY.PROCESS_DT >= {iProcessDtFrom} ")
                End If
                If iProcessDtTo <> -1 Then
                    sbSQL.AppendLine($"  AND LOG_DAILY.PROCESS_DT <= {iProcessDtTo} ")
                End If

                sbSQL.AppendLine("ORDER BY ")
                sbSQL.AppendLine("  CONVERT(DATE, ASSET.FREE_ITEM13) ")

                Dim tbl As New DataTable
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                'データが存在しない場合
                If IsNothing(tbl) OrElse tbl.Rows.Count <= 0 Then
                    Return Nothing
                End If

                'リストにセット
                Dim dConvertChk As Decimal
                For Each dtRow In tbl.Rows

                    If lstDrugSpecInfo.Exists(Function(x) x.AssetNo = CommonRfid.ToStringNvl(dtRow("ASSET_NO")).Trim) Then
                        '追加済みの資産名の場合、重量履歴情報を追加する
                        lstDrugSpecInfo.Find(Function(x) x.AssetNo = CommonRfid.ToStringNvl(dtRow("ASSET_NO")).Trim) _
                                       .AddDueHist(CommonRfid.ToStringNvl(dtRow("PROCESS_DT")).Trim,
                                                   CommonRfid.ToStringNvl(dtRow("DUE_QTY")).Trim,
                                                   CommonRfid.ToStringNvl(dtRow("REMAIN_QTY")).Trim,
                                                   CommonRfid.ToStringNvl(dtRow("SPECIFIC_GRAVITY")).Trim,
                                                   CommonRfid.ToStringNvl(dtRow("SPECIFIED_QTY_L")).Trim)
                        Continue For
                    End If

                    '資産情報をセット
                    Dim rowInfo As New DrugSpecInfo
                    rowInfo.AssetNo = CommonRfid.ToStringNvl(dtRow("ASSET_NO")).Trim
                    rowInfo.AssetNm = CommonRfid.ToStringNvl(dtRow("ASSET_NAME")).Trim
                    rowInfo.CasNo = CommonRfid.ToStringNvl(dtRow("CAS_NO")).Trim
                    rowInfo.FireServiceAct = CommonRfid.ToStringNvl(dtRow("FIRE_SERVICE_ACT")).Trim
                    rowInfo.OrganicRule = ConvertWordValidOrInvalid(CommonRfid.ToStringNvl(dtRow("ORGANIC_RULE")).Trim)
                    rowInfo.PoisonuosRule = ConvertWordPoisonOrDeleterious(CommonRfid.ToStringNvl(dtRow("POISOUNUOS_RULE")).Trim)
                    rowInfo.SpecificRule = ConvertWordValidOrInvalid(CommonRfid.ToStringNvl(dtRow("SPECIFIC_RULE")).Trim)
                    rowInfo.PRTR = ConvertWordValidOrInvalid(CommonRfid.ToStringNvl(dtRow("PRTR")).Trim)
                    rowInfo.ChemicalSubstance = ConvertWordValidOrInvalid(CommonRfid.ToStringNvl(dtRow("CHEMICAL_SUBSTANCE")).Trim)
                    rowInfo.Purchaser = CommonRfid.ToStringNvl(dtRow("PURCHASER")).Trim
                    rowInfo.DueDate = CommonRfid.ToStringNvl(dtRow("DUE_DATE")).Trim
                    rowInfo.StorageLoc = CommonRfid.ToStringNvl(dtRow("LOC_NAME")).Trim

                    '※数値であればセット
                    If Decimal.TryParse(CommonRfid.ToStringNvl(dtRow("INTERNAL_CAPACITY_KG")), dConvertChk) Then
                        rowInfo.InternalCapKg = dConvertChk
                    End If
                    If Decimal.TryParse(CommonRfid.ToStringNvl(dtRow("INTERNAL_CAPACITY_L")), dConvertChk) Then
                        rowInfo.InternalCapL = dConvertChk
                    End If
                    If Decimal.TryParse(CommonRfid.ToStringNvl(dtRow("SPECIFIC_GRAVITY")), dConvertChk) Then
                        rowInfo.SpecificGravity = dConvertChk
                    End If
                    If Decimal.TryParse(CommonRfid.ToStringNvl(dtRow("SPECIFIED_QTY_L")), dConvertChk) Then
                        rowInfo.SpecifiedQtyL = dConvertChk
                    End If
                    If Decimal.TryParse(CommonRfid.ToStringNvl(dtRow("INITIAL_WEIGHT")), dConvertChk) Then
                        rowInfo.InitWeight = dConvertChk
                    End If
                    If Decimal.TryParse(CommonRfid.ToStringNvl(dtRow("SPECIFIED_QTY")), dConvertChk) Then
                        rowInfo.LocSpecifiedQty = dConvertChk
                    Else
                        rowInfo.LocSpecifiedQty = 0                         '未設定時は0をセットする
                    End If

                    '重量履歴情報をセット
                    rowInfo.AddDueHist(CommonRfid.ToStringNvl(dtRow("PROCESS_DT")).Trim,
                                       CommonRfid.ToStringNvl(dtRow("DUE_QTY")).Trim,
                                       CommonRfid.ToStringNvl(dtRow("REMAIN_QTY")).Trim,
                                       CommonRfid.ToStringNvl(dtRow("SPECIFIC_GRAVITY")).Trim,
                                       CommonRfid.ToStringNvl(dtRow("SPECIFIED_QTY_L")).Trim)

                    lstDrugSpecInfo.Add(rowInfo)
                Next

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return lstDrugSpecInfo

        End Function
#End Region

        '-- Private 関数
#Region "有無文言変換処理"
        ''' <summary>
        ''' 有無に該当する文言を取得
        ''' </summary>
        ''' <param name="sTarget">チェック対象文字列</param>
        ''' <returns></returns>
        Private Shared Function ConvertWordValidOrInvalid(ByVal sTarget As String) As String
            Try
                If sTarget = "Y" Then
                    '有に変換
                    Return CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_VALID_1_LETTER)
                ElseIf sTarget = "N" Then
                    '無に変換
                    Return CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_INVALID_1_LETTER)
                Else
                    '該当なしの場合はそのまま返却
                    Return sTarget
                End If

            Catch ex As Exception
                logger.Error(ex)
                Return sTarget
            End Try
        End Function
#End Region

#Region "毒物及び劇物文言変換処理"
        ''' <summary>
        ''' 毒物及び劇物に該当する文言を取得
        ''' </summary>
        ''' <param name="sTarget">チェック対象文字列</param>
        ''' <returns></returns>
        Private Shared Function ConvertWordPoisonOrDeleterious(ByVal sTarget As String) As String
            Try
                If sTarget = "1" Then
                    '毒物に変換
                    Return CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_POISIN)
                ElseIf sTarget = "2" Then
                    '劇物に変換
                    Return CommonRfid.MessageRfid.getMessage(CommonLabel.VALUE_DELETERIOUS)
                Else
                    '該当なしの場合はそのまま返却
                    Return sTarget
                End If

            Catch ex As Exception
                logger.Error(ex)
                Return sTarget
            End Try
        End Function
#End Region

#Region "全薬品指定数量の合計取得処理"
        ''' <summary>
        ''' 全薬品指定数量の合計取得処理
        ''' </summary>
        ''' <param name="lstDrugSpec">薬品管理表リスト</param>
        ''' <param name="sProcessDt">確認日</param>
        ''' <returns></returns>
        Private Shared Function sumSpecifedQty(ByVal lstDrugSpec As List(Of DrugSpecInfo), ByVal sProcessDt As String) As Decimal
            Dim sumQty As Decimal

            '重量履歴から確認日が一致する全薬品の指定数量の合計を取得する
            For Each row In lstDrugSpec
                sumQty += row.LstDueHist.Where(Function(x) x.ProcessDt = sProcessDt).Sum(Function(dueHist) dueHist.SpecifiedQty)
            Next

            Return sumQty
        End Function
#End Region

    End Class
End Namespace