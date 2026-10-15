Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient
Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports GrapeCity.Documents.Excel
Imports GrapeCity.Documents.Excel.Drawing
Imports GrapeCity.Documents

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：貸出確認書関連ツール系 Data Access クラス
    ''' 作成日        ：2021/10/30      nv Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolLendingConfirm
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        ''' <summary>シート名　外部用１枚のみ時</summary>
        Public Const SheetNm_Outside_OneOnly As String = "OUTSIDE_1"
        ''' <summary>シート名　外部用複数ページ時</summary>
        Public Const SheetNm_Outside_MultiFirst As String = "OUTSIDE_2"
        ''' <summary>シート名　外部用複数ページ時</summary>
        Public Const SheetNm_Outside_MultiMid As String = "OUTSIDE_3"
        ''' <summary>シート名　外部用複数ページ時</summary>
        Public Const SheetNm_Outside_MultiLast As String = "OUTSIDE_4"

        ''' <summary>シート名　内部用１枚のみ時</summary>
        Public Const SheetNm_Inside_OneOnly As String = "INSIDE_1"
        ''' <summary>シート名　内部用複数ページ時</summary>
        Public Const SheetNm_Inside_MultiFirst As String = "INSIDE_2"
        ''' <summary>シート名　内部用複数ページ時</summary>
        Public Const SheetNm_Inside_MultiMid As String = "INSIDE_3"
        ''' <summary>シート名　内部用複数ページ時</summary>
        Public Const SheetNm_Inside_MultiLast As String = "INSIDE_4"

        ''' <summary>明細数　外部用１枚のみ時</summary>
        Public Const Cnt_Outside_OneOnly As Integer = 5
        ''' <summary>明細数　外部用複数ページ時</summary>
        Public Const Cnt_Outside_MultiFirst As Integer = 25
        ''' <summary>明細数　外部用複数ページ時</summary>
        Public Const Cnt_Outside_MultiMid As Integer = 44
        ''' <summary>明細数　外部用複数ページ時</summary>
        Public Const Cnt_Outside_MultiLast As Integer = 23


        '''' <summary>明細数　内部用１枚のみ時</summary>
        'Public Const Cnt_Inside_OneOnly As Integer = 5
        '''' <summary>明細数　内部用複数ページ時</summary>
        'Public Const Cnt_Inside_MultiFirst As Integer = 10
        '''' <summary>明細数　内部用複数ページ時</summary>
        'Public Const Cnt_Inside_MultiMid As Integer = 17
        '''' <summary>明細数　内部用複数ページ時</summary>
        'Public Const Cnt_Inside_MultiLast As Integer = 11

        ''' <summary>明細数　内部用１枚のみ時</summary>
        Public Const Cnt_Inside_OneOnly As Integer = 5
        ''' <summary>明細数　内部用複数ページ時</summary>
        Public Const Cnt_Inside_MultiFirst As Integer = 6
        ''' <summary>明細数　内部用複数ページ時</summary>
        Public Const Cnt_Inside_MultiMid As Integer = 7
        ''' <summary>明細数　内部用複数ページ時</summary>
        Public Const Cnt_Inside_MultiLast As Integer = 8

        ''' <summary>TEL 接頭子</summary>
        Public Const CON_TEL As String = "TEL:"
        ''' <summary>FAX 接頭子</summary>
        Public Const CON_FAX As String = "FAX:"
        ''' <summary>郵便番号 接頭子</summary>
        Public Const CON_ZIP_CODE As String = "〒"


#Region "予約確認書PDF作成"

        ''' <summary>
        ''' 予約確認書PDF作成
        ''' </summary>
        ''' <param name="sKeyStr">チェック状態連結文字列</param>
        ''' <param name="sTempFullPath">テンプレートフォルダ　プルパス</param>
        ''' <param name="sOutFullPath">出力フォルダ　プルパス</param>
        ''' <returns>
        ''' PDF作成ファイル名（フルパス）　エラー時、String.Empty
        ''' </returns>
        Public Shared Function MakeLendingConfirm(ByVal sKeyStr As String, ByVal sTempFullPath As String, ByVal sOutFullPath As String) As String

            '予約確認書出力用予約データ取得
            Dim tblResv As DataTable = GetConfirmReserveData(sKeyStr)

            '予約確認書出力用予約資産データ取得
            Dim tblRsvDtl As DataTable = GetConfirmReserveAssetData(sKeyStr)


            'ライセンスキー
            GrapeCity.Documents.Excel.Workbook.SetLicenseKey("株式会社シービジョン,496378668853486#A03NJgzNzYTO4IiOiQWSisnOiQkIsISP3cWe6QTZ95WWvEmMYdTStZ5MltGdv26dWlWazcmYBREamhXNyUEVRdGbOl6SuREarg4L6dVRwZnUz4EZFZzKzYmZv2Ee7gje9tWb5IzZ6x6arQGUVl5UiojITJCL7YzM8cTM8gDO0IicfJye#4Xfd5nI9g4NyIiOiMkIsIiMWBCdl9kLgwWZjhXRgI7bmBCduVWb5N6bEByQHJiOi8kI1tlOiQmcQJCLiEDNzIDNwAiNwITM9EDMyIiOiQncDJCLiM1gjf0gjjrgjP9gjz1gjfrgj1LpnrJvk/IvlrKomLiOiEmTDJCLiYDO4M5Ntg")

            'テンプレートEXCEL用
            Dim wbTemplate As IWorkbook = New Workbook
            Dim wsTemplate As IWorksheet
            'テンプレートファイル読込み
            wbTemplate.Open(sTempFullPath & "\" & CommonConst.LEASECONFIRM_TEMPLATE_FILENM)

            ' 新規ワークシート
            Dim wsNew As IWorksheet


            '一時ファイル名
            Dim sOutputFileName As String = Path.GetRandomFileName()

            'PDF一時ファイル名
            Dim sPdfFileNm As String = sOutFullPath & "\" & sOutputFileName & ".pdf"

            Dim wbList As New List(Of Workbook)


            '外部用予約確認書出力
            For Each rowResv As DataRow In tblResv.Rows

                ' 新規ワークブックの作成
                Dim wbNew As New Workbook

                '
                '----- 外部用
                '

                'エクセルデータ作成

                Dim iPageCnt As Integer = 0
                Dim rowDtls As DataRow() = tblRsvDtl.Select("RESERVE_ID = '" & rowResv("RESERVE_ID").ToString & "'", "ASSET_NO, REAL_START_DT")
                If rowDtls.Count = 0 Then
                    'ないはずだが、一応、バイパス
                ElseIf rowDtls.Count <= Cnt_Outside_OneOnly Then
                    'テンプレートシート取得する
                    wsTemplate = wbTemplate.Worksheets(SheetNm_Outside_OneOnly)
                    'テンプレートシートを次のシートにコピーする
                    wsNew = wsTemplate.Copy(wbNew)

                    '明細数　外部用１枚のみ時
                    iPageCnt += 1

                    'EXCEL 項目編集
                    'wsNew.Rows(7).Cells(7).Value = "QQQQQ"
                    'wsNew.Range("A1").Value = "AAAAAA"

                    wsNew.Range("PRINT_PAGE").Value = iPageCnt.ToString & "/1"
                    '2023/03/22 Kawamoto UPD START
                    'wsNew.Range("DEST_NAME").Value = rowResv("DEST_NAME1").ToString.Trim & rowResv("DEST_NAME2").ToString.Trim
                    wsNew.Range("DEST_NAME").Value = rowResv("TO_DEST_NAME").ToString.Trim
                    '2023/03/22 Kawamoto UPD END
                    wsNew.Range("PRINT_DATE").Value = Now.ToString("yyyy/MM/dd")
                    wsNew.Range("DEPARTMENT_NAME").Value = rowResv("DEPARTMENT_NAME").ToString.Trim
                    If String.IsNullOrEmpty(rowResv("PHONE_NUMBER").ToString.Trim) Then
                        wsNew.Range("PHONE_NUMBER").Value = String.Empty
                    Else
                        wsNew.Range("PHONE_NUMBER").Value = CON_TEL & rowResv("PHONE_NUMBER").ToString.Trim
                    End If
                    If String.IsNullOrEmpty(rowResv("FAX").ToString.Trim) Then
                        wsNew.Range("FAX_NUMBER").Value = String.Empty
                    Else
                        wsNew.Range("FAX_NUMBER").Value = CON_FAX & rowResv("FAX").ToString.Trim
                    End If
                    wsNew.Range("CHARGE_EMP_NAME").Value = rowResv("EMPLOYEE_NAME").ToString.Trim
                    wsNew.Range("REAL_DT").Value = ToolCommon.NumDateString(CInt(rowResv("REAL_START_DT"))) & " - " & ToolCommon.NumDateString(CInt(rowResv("USER_END_DT")))
                    wsNew.Range("LEASE_USE").Value = rowResv("LEASE_USE").ToString.Trim
                    wsNew.Range("REMARK").Value = rowResv("REMARK").ToString.Trim
                    '2023/03/22 Kawamoto ADD START
                    wsNew.Range("RET_LEASE_PLACE_NAME").Value = rowResv("RET_LEASE_PLACE_NAME").ToString.Trim
                    wsNew.Range("RET_ZIP_CODE").Value = CON_ZIP_CODE & rowResv("RET_ZIP_CODE").ToString.Trim
                    wsNew.Range("RET_ADDRESS1").Value = rowResv("RET_ADDRESS1").ToString.Trim
                    wsNew.Range("RET_ADDRESS2").Value = rowResv("RET_ADDRESS2").ToString.Trim
                    wsNew.Range("RET_ADDRESS3").Value = rowResv("RET_ADDRESS3").ToString.Trim
                    '2023/03/22 Kawamoto ADD END

                    Dim rngAssetNo = wsNew.Range("ASSET_NO")
                    Dim rngAssetName = wsNew.Range("ASSET_NAME")
                    Dim rngSeirialNo = wsNew.Range("SERIAL_NO")
                    Dim rngQty = wsNew.Range("LEASE_QTY")
                    Dim rngRetDt = wsNew.Range("RETURN_DT")
                    Dim iCnt As Integer = 0
                    For Each rowDtl As DataRow In rowDtls

                        ' セルに値を設定
                        rngAssetNo.Cells(iCnt).Value = rowDtl("ASSET_NO").ToString.Trim
                        rngAssetName.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                        rngSeirialNo.Cells(iCnt).Value = rowDtl("FREE_ITEM10").ToString.Trim
                        rngQty.Cells(iCnt).Value = rowDtl("LEASE_QTY").ToString

                        rngRetDt.Cells(iCnt).Value = GetUserEndDt(rowDtl("REAL_START_DT"), rowDtl("REAL_END_DT"))

                        iCnt += 1

                    Next

                Else
                    Dim iPageAll As Integer = GetPageCntOutSide(rowDtls.Count)

                    '----- １ページ目
                    'テンプレートシート取得する
                    wsTemplate = wbTemplate.Worksheets(SheetNm_Outside_MultiFirst)
                    'テンプレートシートを次のシートにコピーする
                    wsNew = wsTemplate.Copy(wbNew)

                    iPageCnt += 1

                    'EXCEL 項目編集
                    wsNew.Range("PRINT_PAGE").Value = iPageCnt.ToString & "/" & iPageAll.ToString
                    '2023/03/22 Kawamoto UPD START
                    'wsNew.Range("DEST_NAME").Value = rowResv("DEST_NAME1").ToString.Trim & rowResv("DEST_NAME2").ToString.Trim
                    wsNew.Range("DEST_NAME").Value = rowResv("TO_DEST_NAME").ToString.Trim
                    '2023/03/22 Kawamoto UPD END
                    wsNew.Range("PRINT_DATE").Value = Now.ToString("yyyy/MM/dd")
                    wsNew.Range("DEPARTMENT_NAME").Value = rowResv("DEPARTMENT_NAME").ToString.Trim
                    If String.IsNullOrEmpty(rowResv("PHONE_NUMBER").ToString.Trim) Then
                        wsNew.Range("PHONE_NUMBER").Value = String.Empty
                    Else
                        wsNew.Range("PHONE_NUMBER").Value = CON_TEL & rowResv("PHONE_NUMBER").ToString.Trim
                    End If
                    If String.IsNullOrEmpty(rowResv("FAX").ToString.Trim) Then
                        wsNew.Range("FAX_NUMBER").Value = String.Empty
                    Else
                        wsNew.Range("FAX_NUMBER").Value = CON_FAX & rowResv("FAX").ToString.Trim
                    End If
                    wsNew.Range("CHARGE_EMP_NAME").Value = rowResv("EMPLOYEE_NAME").ToString.Trim
                    wsNew.Range("REAL_DT").Value = ToolCommon.NumDateString(CInt(rowResv("REAL_START_DT"))) & " - " & ToolCommon.NumDateString(CInt(rowResv("USER_END_DT")))
                    wsNew.Range("LEASE_USE").Value = rowResv("LEASE_USE").ToString.Trim


                    Dim rngAssetNo = wsNew.Range("ASSET_NO")
                    Dim rngAssetName = wsNew.Range("ASSET_NAME")
                    Dim rngSeirialNo = wsNew.Range("SERIAL_NO")
                    Dim rngQty = wsNew.Range("LEASE_QTY")
                    Dim rngRetDt = wsNew.Range("RETURN_DT")
                    Dim iCnt As Integer = 0
                    Dim iMeiCnt As Integer = 0
                    Dim rowDtl As DataRow
                    For ii As Integer = 0 To Cnt_Outside_MultiFirst - 1
                        iMeiCnt += 1
                        If iMeiCnt > rowDtls.Count Then
                            Exit For
                        End If
                        rowDtl = rowDtls(iMeiCnt - 1)

                        ' セルに値を設定
                        rngAssetNo.Cells(iCnt).Value = rowDtl("ASSET_NO").ToString.Trim
                        rngAssetName.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                        rngSeirialNo.Cells(iCnt).Value = rowDtl("FREE_ITEM10").ToString.Trim
                        rngQty.Cells(iCnt).Value = rowDtl("LEASE_QTY").ToString
                        rngRetDt.Cells(iCnt).Value = GetUserEndDt(rowDtl("REAL_START_DT"), rowDtl("REAL_END_DT"))

                        iCnt += 1
                    Next

                    '----- 中間ページ
                    For iMid As Integer = iPageCnt + 1 To iPageAll - 1
                        'テンプレートシート取得する
                        wsTemplate = wbTemplate.Worksheets(SheetNm_Outside_MultiMid)
                        'テンプレートシートを次のシートにコピーする
                        wsNew = wsTemplate.Copy(wbNew)

                        iPageCnt += 1

                        'EXCEL 項目編集
                        wsNew.Range("PRINT_PAGE").Value = iPageCnt.ToString & "/" & iPageAll.ToString

                        rngAssetNo = wsNew.Range("ASSET_NO")
                        rngAssetName = wsNew.Range("ASSET_NAME")
                        rngSeirialNo = wsNew.Range("SERIAL_NO")
                        rngQty = wsNew.Range("LEASE_QTY")
                        rngRetDt = wsNew.Range("RETURN_DT")
                        iCnt = 0
                        For ii As Integer = 0 To Cnt_Outside_MultiMid - 1
                            iMeiCnt += 1
                            If iMeiCnt > rowDtls.Count Then
                                Exit For
                            End If
                            rowDtl = rowDtls(iMeiCnt - 1)

                            ' セルに値を設定
                            rngAssetNo.Cells(iCnt).Value = rowDtl("ASSET_NO").ToString.Trim
                            rngAssetName.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                            rngSeirialNo.Cells(iCnt).Value = rowDtl("FREE_ITEM10").ToString.Trim
                            rngQty.Cells(iCnt).Value = rowDtl("LEASE_QTY").ToString
                            rngRetDt.Cells(iCnt).Value = GetUserEndDt(rowDtl("REAL_START_DT"), rowDtl("REAL_END_DT"))

                            iCnt += 1
                        Next
                    Next

                    '----- 最終ページ
                    'テンプレートシート取得する
                    wsTemplate = wbTemplate.Worksheets(SheetNm_Outside_MultiLast)
                    'テンプレートシートを次のシートにコピーする
                    wsNew = wsTemplate.Copy(wbNew)

                    iPageCnt += 1

                    'EXCEL 項目編集
                    wsNew.Range("PRINT_PAGE").Value = iPageCnt.ToString & "/" & iPageAll.ToString
                    wsNew.Range("REMARK").Value = rowResv("REMARK").ToString.Trim
                    '2023/03/22 Kawamoto ADD START
                    wsNew.Range("RET_LEASE_PLACE_NAME").Value = rowResv("RET_LEASE_PLACE_NAME").ToString.Trim
                    wsNew.Range("RET_ZIP_CODE").Value = CON_ZIP_CODE & rowResv("RET_ZIP_CODE").ToString.Trim
                    wsNew.Range("RET_ADDRESS1").Value = rowResv("RET_ADDRESS1").ToString.Trim
                    wsNew.Range("RET_ADDRESS2").Value = rowResv("RET_ADDRESS2").ToString.Trim
                    wsNew.Range("RET_ADDRESS3").Value = rowResv("RET_ADDRESS3").ToString.Trim
                    '2023/03/22 Kawamoto ADD END


                    rngAssetNo = wsNew.Range("ASSET_NO")
                    rngAssetName = wsNew.Range("ASSET_NAME")
                    rngSeirialNo = wsNew.Range("SERIAL_NO")
                    rngQty = wsNew.Range("LEASE_QTY")
                    rngRetDt = wsNew.Range("RETURN_DT")
                    iCnt = 0
                    For ii As Integer = 0 To Cnt_Outside_MultiLast - 1
                        iMeiCnt += 1
                        If iMeiCnt > rowDtls.Count Then
                            Exit For
                        End If
                        rowDtl = rowDtls(iMeiCnt - 1)

                        ' セルに値を設定
                        rngAssetNo.Cells(iCnt).Value = rowDtl("ASSET_NO").ToString.Trim
                        rngAssetName.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                        rngSeirialNo.Cells(iCnt).Value = rowDtl("FREE_ITEM10").ToString.Trim
                        rngQty.Cells(iCnt).Value = rowDtl("LEASE_QTY").ToString
                        rngRetDt.Cells(iCnt).Value = GetUserEndDt(rowDtl("REAL_START_DT"), rowDtl("REAL_END_DT"))

                        iCnt += 1
                    Next
                End If

                '
                '----- 内部用
                '
                'エクセルデータ作成

                iPageCnt = 0
                If rowDtls.Count = 0 Then
                    'ないはずだが、一応、バイパス
                ElseIf rowDtls.Count <= Cnt_Outside_OneOnly Then
                    'テンプレートシート取得する
                    wsTemplate = wbTemplate.Worksheets(SheetNm_Inside_OneOnly)
                    'テンプレートシートを次のシートにコピーする
                    wsNew = wsTemplate.Copy(wbNew)

                    '明細数　外部用１枚のみ時
                    iPageCnt += 1

                    'EXCEL 項目編集
                    wsNew.Range("PRINT_PAGE").Value = iPageCnt.ToString & "/1"
                    wsNew.Range("RESERVE_NO").Value = rowResv("RESERVE_ID").ToString.Trim
                    wsNew.Range("REQUEST_DT").Value = ToolCommon.NumDateString(CInt(rowResv("REQUEST_DT")))
                    wsNew.Range("REAL_DT").Value = ToolCommon.NumDateString(CInt(rowResv("REQUEST_DT")))
                    wsNew.Range("REQ_EMP_NAME").Value = rowResv("REQ_EMP_NAME").ToString.Trim
                    wsNew.Range("REAL_DT").Value = ToolCommon.NumDateString(CInt(rowResv("REAL_START_DT"))) & " - " & ToolCommon.NumDateString(CInt(rowResv("REAL_END_DT")))
                    wsNew.Range("USER_DT").Value = ToolCommon.NumDateString(CInt(rowResv("USER_START_DT"))) & " - " & ToolCommon.NumDateString(CInt(rowResv("USER_END_DT")))
                    wsNew.Range("DEST_NAME").Value = rowResv("DEST_NAME1").ToString.Trim & rowResv("DEST_NAME2").ToString.Trim
                    wsNew.Range("LEASE_USE").Value = rowResv("LEASE_USE").ToString.Trim
                    wsNew.Range("CHARGE_EMP_NAME").Value = rowResv("EMPLOYEE_NAME").ToString.Trim
                    wsNew.Range("PRINT_DATE").Value = Now.ToString("yyyy/MM/dd")
                    wsNew.Range("REMARK").Value = rowResv("REMARK_INTERNAL").ToString.Trim

                    Dim rngAssetNo = wsNew.Range("ASSET_NO")
                    Dim rngAssetName = wsNew.Range("ASSET_NAME")
                    Dim rngSeirialNo = wsNew.Range("SERIAL_NO")
                    Dim rngLocNmt = wsNew.Range("LOCATION_NAME")
                    Dim iCnt As Integer = 0
                    For Each rowDtl As DataRow In rowDtls

                        ' セルに値を設定
                        rngAssetNo.Cells(iCnt).Value = rowDtl("ASSET_NO").ToString.Trim
                        rngAssetName.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                        rngSeirialNo.Cells(iCnt).Value = rowDtl("FREE_ITEM10").ToString.Trim
                        rngLocNmt.Cells(iCnt).Value = rowDtl("MNG_LOC_L_NAME").ToString

                        iCnt += 1

                    Next

                Else
                    Dim iPageAll As Integer = GetPageCntInSide(rowDtls.Count)

                    '----- １ページ目
                    'テンプレートシート取得する
                    wsTemplate = wbTemplate.Worksheets(SheetNm_Inside_MultiFirst)
                    'テンプレートシートを次のシートにコピーする
                    wsNew = wsTemplate.Copy(wbNew)

                    iPageCnt += 1

                    'EXCEL 項目編集
                    wsNew.Range("PRINT_PAGE").Value = iPageCnt.ToString & "/" & iPageAll.ToString
                    wsNew.Range("RESERVE_NO").Value = rowResv("RESERVE_ID").ToString.Trim
                    wsNew.Range("REQUEST_DT").Value = ToolCommon.NumDateString(CInt(rowResv("REQUEST_DT")))
                    wsNew.Range("REAL_DT").Value = ToolCommon.NumDateString(CInt(rowResv("REQUEST_DT")))
                    wsNew.Range("REQ_EMP_NAME").Value = rowResv("REQ_EMP_NAME").ToString.Trim
                    wsNew.Range("REAL_DT").Value = ToolCommon.NumDateString(CInt(rowResv("REAL_START_DT"))) & " - " & ToolCommon.NumDateString(CInt(rowResv("REAL_END_DT")))
                    wsNew.Range("USER_DT").Value = ToolCommon.NumDateString(CInt(rowResv("USER_START_DT"))) & " - " & ToolCommon.NumDateString(CInt(rowResv("USER_END_DT")))
                    wsNew.Range("DEST_NAME").Value = rowResv("DEST_NAME1").ToString.Trim & rowResv("DEST_NAME2").ToString.Trim
                    wsNew.Range("LEASE_USE").Value = rowResv("LEASE_USE").ToString.Trim
                    wsNew.Range("CHARGE_EMP_NAME").Value = rowResv("EMPLOYEE_NAME").ToString.Trim
                    wsNew.Range("PRINT_DATE").Value = Now.ToString("yyyy/MM/dd")

                    Dim rngAssetNo = wsNew.Range("ASSET_NO")
                    Dim rngAssetName = wsNew.Range("ASSET_NAME")
                    Dim rngSeirialNo = wsNew.Range("SERIAL_NO")
                    Dim rngLocNmt = wsNew.Range("LOCATION_NAME")
                    Dim iCnt As Integer = 0
                    Dim iMeiCnt As Integer = 0
                    Dim rowDtl As DataRow
                    For ii As Integer = 0 To Cnt_Inside_MultiFirst - 1
                        iMeiCnt += 1
                        If iMeiCnt > rowDtls.Count Then
                            Exit For
                        End If
                        rowDtl = rowDtls(iMeiCnt - 1)

                        ' セルに値を設定
                        rngAssetNo.Cells(iCnt).Value = rowDtl("ASSET_NO").ToString.Trim
                        rngAssetName.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                        rngSeirialNo.Cells(iCnt).Value = rowDtl("FREE_ITEM10").ToString.Trim
                        rngLocNmt.Cells(iCnt).Value = rowDtl("MNG_LOC_L_NAME").ToString

                        iCnt += 1
                    Next

                    '----- 中間ページ
                    For iMid As Integer = iPageCnt + 1 To iPageAll - 1
                        'テンプレートシート取得する
                        wsTemplate = wbTemplate.Worksheets(SheetNm_Inside_MultiMid)
                        'テンプレートシートを次のシートにコピーする
                        wsNew = wsTemplate.Copy(wbNew)

                        iPageCnt += 1

                        'EXCEL 項目編集
                        wsNew.Range("PRINT_PAGE").Value = iPageCnt.ToString & "/" & iPageAll.ToString

                        rngAssetNo = wsNew.Range("ASSET_NO")
                        rngAssetName = wsNew.Range("ASSET_NAME")
                        rngSeirialNo = wsNew.Range("SERIAL_NO")
                        rngLocNmt = wsNew.Range("LOCATION_NAME")
                        iCnt = 0
                        For ii As Integer = 0 To Cnt_Inside_MultiMid - 1
                            iMeiCnt += 1
                            If iMeiCnt > rowDtls.Count Then
                                Exit For
                            End If
                            rowDtl = rowDtls(iMeiCnt - 1)

                            ' セルに値を設定
                            rngAssetNo.Cells(iCnt).Value = rowDtl("ASSET_NO").ToString.Trim
                            rngAssetName.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                            rngSeirialNo.Cells(iCnt).Value = rowDtl("FREE_ITEM10").ToString.Trim
                            rngLocNmt.Cells(iCnt).Value = rowDtl("MNG_LOC_L_NAME").ToString

                            iCnt += 1
                        Next
                    Next

                    '----- 最終ページ
                    'テンプレートシート取得する
                    wsTemplate = wbTemplate.Worksheets(SheetNm_Inside_MultiLast)
                    'テンプレートシートを次のシートにコピーする
                    wsNew = wsTemplate.Copy(wbNew)

                    iPageCnt += 1

                    'EXCEL 項目編集
                    wsNew.Range("PRINT_PAGE").Value = iPageCnt.ToString & "/" & iPageAll.ToString
                    wsNew.Range("REMARK").Value = rowResv("REMARK_INTERNAL").ToString.Trim


                    rngAssetNo = wsNew.Range("ASSET_NO")
                    rngAssetName = wsNew.Range("ASSET_NAME")
                    rngSeirialNo = wsNew.Range("SERIAL_NO")
                    rngLocNmt = wsNew.Range("LOCATION_NAME")
                    iCnt = 0
                    For ii As Integer = 0 To Cnt_Inside_MultiLast - 1
                        iMeiCnt += 1
                        If iMeiCnt > rowDtls.Count Then
                            Exit For
                        End If
                        rowDtl = rowDtls(iMeiCnt - 1)

                        ' セルに値を設定
                        rngAssetNo.Cells(iCnt).Value = rowDtl("ASSET_NO").ToString.Trim
                        rngAssetName.Cells(iCnt).Value = rowDtl("ASSET_NAME").ToString.Trim
                        rngSeirialNo.Cells(iCnt).Value = rowDtl("FREE_ITEM10").ToString.Trim
                        rngLocNmt.Cells(iCnt).Value = rowDtl("MNG_LOC_L_NAME").ToString

                        iCnt += 1
                    Next
                End If

                '----- Book 追加
                'テンプレートシートを削除する
                'wb.Worksheets(0).Delete()

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

        '-- Private 関数

#Region "貸出確認書出力用予約データ取得"

        ''' <summary>
        ''' 送り状出力用予約データ取得
        ''' </summary>
        ''' <param name="sLstReserveId">予約IDのコンマ区切り文字列</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Private Shared Function GetConfirmReserveData(sLstReserveId As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("   T_RESERVE.RESERVE_ID")
                sbSQL.AppendLine("  ,T_RESERVE.DEST_NAME1")
                sbSQL.AppendLine("  ,T_RESERVE.DEST_NAME2")
                sbSQL.AppendLine("  ,T_RESERVE.REQUEST_DT")
                sbSQL.AppendLine("  ,T_RESERVE.REAL_START_DT")
                sbSQL.AppendLine("  ,T_RESERVE.REAL_END_DT")
                sbSQL.AppendLine("  ,T_RESERVE.USER_START_DT")
                sbSQL.AppendLine("  ,T_RESERVE.USER_END_DT")
                sbSQL.AppendLine("  ,T_RESERVE.LEASE_USE")
                sbSQL.AppendLine("  ,T_RESERVE.REMARK")
                sbSQL.AppendLine("  ,T_RESERVE.REMARK_INTERNAL")
                '2023/03/22 Kawamoto ADD START
                sbSQL.AppendLine("  ,T_RESERVE.TO_DEST_NAME")       
                sbSQL.AppendLine("  ,M_LEASE_PLACE.LEASE_PLACE_NAME AS RET_LEASE_PLACE_NAME")
                sbSQL.AppendLine("  ,T_RESERVE.RET_ADDRESS1")
                sbSQL.AppendLine("  ,T_RESERVE.RET_ADDRESS2")
                sbSQL.AppendLine("  ,T_RESERVE.RET_ADDRESS3")
                sbSQL.AppendLine("  ,T_RESERVE.RET_ZIP_CODE")
                '2023/03/22 Kawamoto ADD END
                sbSQL.AppendLine("  ,REQ_EMP.EMPLOYEE_NAME AS REQ_EMP_NAME")
                sbSQL.AppendLine("  ,M_EMPLOYEE.EMPLOYEE_NAME")
                sbSQL.AppendLine("  ,M_DEPARTMENT.DEPARTMENT_NAME")
                sbSQL.AppendLine("  ,M_DEPARTMENT.PHONE_NUMBER")
                sbSQL.AppendLine("  ,M_DEPARTMENT.FAX")
                sbSQL.AppendLine("  ,M_DEPARTMENT.ZIP_CODE")
                sbSQL.AppendLine("FROM T_RESERVE")
                sbSQL.AppendLine("INNER JOIN M_EMPLOYEE")
                sbSQL.AppendLine("ON   M_EMPLOYEE.EMPLOYEE_CD = T_RESERVE.CHARGE_EMP_CD")
                sbSQL.AppendLine("LEFT OUTER JOIN M_DEPARTMENT")                               '2023/02/09 UPD Shirakawa
                sbSQL.AppendLine("ON   M_DEPARTMENT.DEPARTMENT_CD = M_EMPLOYEE.DEPARTMENT_CD")
                sbSQL.AppendLine("INNER JOIN M_EMPLOYEE REQ_EMP")
                sbSQL.AppendLine("ON   REQ_EMP.EMPLOYEE_CD = T_RESERVE.REQ_EMP_CD")
                '2023/03/22 Kawamoto ADD START
                sbSQL.AppendLine("LEFT OUTER JOIN M_LEASE_PLACE")
                sbSQL.AppendLine("ON   M_LEASE_PLACE.LEASE_PLACE_CD = T_RESERVE.RET_LEASE_PLACE_CD")
                '2023/03/22 Kawamoto ADD END
                sbSQL.AppendLine("WHERE T_RESERVE.RESERVE_ID IN (" & sLstReserveId & ")")
                sbSQL.AppendLine("ORDER BY T_RESERVE.REAL_START_DT DESC, T_RESERVE.RESERVE_ID")
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

#Region "貸出確認書出力用予約資産データ取得"

        ''' <summary>
        ''' 送り状出力用予約資産データ取得
        ''' </summary>
        ''' <param name="sLstReserveId">予約IDのコンマ区切り文字列</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Private Shared Function GetConfirmReserveAssetData(sLstReserveId As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("   T_RESERVE_DETAIL.RESERVE_ID")
                sbSQL.AppendLine("  ,T_RESERVE_DETAIL.ASSET_NO")
                sbSQL.AppendLine("  ,T_RESERVE_DETAIL.REAL_START_DT")
                sbSQL.AppendLine("  ,M_ASSET.ASSET_NAME")
                sbSQL.AppendLine("  ,M_ASSET.FREE_ITEM10")
                sbSQL.AppendLine("  ,1 AS LEASE_QTY")
                sbSQL.AppendLine("  ,T_RESERVE_DETAIL.REAL_END_DT")
                sbSQL.AppendLine("    , MNGLOC.LOC_NAME AS MNG_LOC_L_NAME ")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL")
                sbSQL.AppendLine("INNER JOIN M_ASSET")
                sbSQL.AppendLine("ON   M_ASSET.ASSET_NO = T_RESERVE_DETAIL.ASSET_NO")
                sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOC ")
                sbSQL.AppendLine("ON   MNGLOC.LOC_DIV = @LocDivL")
                sbSQL.AppendLine("AND  M_ASSET.MNG_LOC_L_CD = MNGLOC.LOC_L_CD")
                sbSQL.AppendLine("WHERE T_RESERVE_DETAIL.RESERVE_ID IN (" & sLstReserveId & ")")

                params.Add(New SqlParameter($"@LocDivL", LOC_DIV.LocationL))

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

#Region "ユーザー貸出終了日取得"

        ''' <summary>
        ''' ユーザー貸出終了日取得
        ''' </summary>
        ''' <param name="sStartDt">実質貸出開始日</param>
        ''' <param name="sEndDt">実質貸出終了日</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Private Shared Function GetUserEndDt(sStartDt As String, sEndDt As String) As Date
            Dim dtStart As Date = CommonRfid.ToStringIntYMD(sStartDt)
            Dim dtRet As Date = CommonRfid.ToStringIntYMD(sEndDt)

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Try
                '返却リードタイム取得
                Dim setting_HenkyakuLT As SettingInfo = Misc.CommonRfid.MessageRfid.getSettingInfo(CommonSettingKey.Return_LeadTime.KeyDiv, CommonSettingKey.Return_LeadTime.Key1, CommonSettingKey.Return_LeadTime.Key2, CommonSettingKey.Return_LeadTime.Key3, True)
                Dim iHenkyakuLT As Integer = CInt(setting_HenkyakuLT.DATA1)

                '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                Dim sHenkyakuWorkFlg As String = WorkFlg.Jitsubi
                If setting_HenkyakuLT.DATA2 = WorkFlg.Kadobi Then
                    sHenkyakuWorkFlg = WorkFlg.Kadobi
                End If

                '対象日付取得
                Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(dtRet, iHenkyakuLT, sHenkyakuWorkFlg)
                If iTargetDate <> 0 Then
                    Dim dtUserEndDt As Date = CommonRfid.ToStringIntYMD(iTargetDate)
                    If dtUserEndDt > dtStart Then
                        dtRet = dtUserEndDt
                    End If
                End If
                Return dtRet
            Catch ex As Exception
                logger.Error(ex.Message)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return dtRet
        End Function
#End Region

#Region "貸出確認書外部用ページ数取得"

        ''' <summary>
        ''' 貸出確認書外部用ページ数取得
        ''' </summary>
        ''' <param name="iMeisaiCnt">明細行数</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Private Shared Function GetPageCntOutSide(iMeisaiCnt As Integer) As Integer

            If iMeisaiCnt <= Cnt_Outside_OneOnly Then
                '1ページ内
                Return 1
            End If

            If iMeisaiCnt <= Cnt_Outside_MultiFirst Then
                '複数ページだが、1ページで明細は納まる場合
                Return 2
            End If

            Dim iPage As Integer = 2
            Dim iZanCnt As Integer = iMeisaiCnt - (Cnt_Outside_MultiFirst + Cnt_Outside_MultiLast)

            Do While iZanCnt > 0
                iPage += 1
                iZanCnt -= Cnt_Outside_MultiMid
            Loop

            Return iPage

        End Function
#End Region

#Region "貸出確認書内部用ページ数取得"

        ''' <summary>
        ''' 貸出確認書内部用ページ数取得
        ''' </summary>
        ''' <param name="iMeisaiCnt">明細行数</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Private Shared Function GetPageCntInSide(iMeisaiCnt As Integer) As Integer

            If iMeisaiCnt <= Cnt_Inside_OneOnly Then
                '1ページ内
                Return 1
            End If

            If iMeisaiCnt <= Cnt_Inside_MultiFirst Then
                '複数ページだが、1ページで明細は納まる場合
                Return 2
            End If

            Dim iPage As Integer = 2
            Dim iZanCnt As Integer = iMeisaiCnt - (Cnt_Inside_MultiFirst + Cnt_Inside_MultiLast)

            Do While iZanCnt > 0
                iPage += 1
                iZanCnt -= Cnt_Inside_MultiMid
            Loop

            Return iPage

        End Function
#End Region

    End Class
End Namespace
