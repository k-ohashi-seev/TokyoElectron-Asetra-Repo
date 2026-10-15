Imports System.IO

''' <summary>
''' CSVファイルクラス
''' </summary>
''' <remarks>
''' </remarks>
Public Class clsSV_CsvFile

    '**************************************************************************************************
    'TODO:システムに応じて適宜変更する
    '**************************************************************************************************

    ''' <summary>エンコーダー（Shift_Jis)</summary>
    Private Shared ReadOnly ENC As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_Jis")

    '**************************************************************************************************
    '以下はどのシステムであっても同じなので変更しない
    '**************************************************************************************************

    '-----定数-----

    ''' <summary>クラス名</summary>
    Private Const CLASS_NAME As String = "CsvFile"
    ''' <summary>CSVセパレータ</summary>
    Private Const CSV_SEPARATOR As Char = ","c
    ''' <summary>ダブルクォート</summary>
    Private Const DBLQUATE As Char = Chr(34)

    '-----静的 publicメソッド-----

    ''' <summary>
    ''' CSVファイルを作成します。
    ''' </summary>
    ''' <param name="sFileName">CSVファイル名</param>
    ''' <param name="dt">出力データ</param>
    ''' <param name="isOutputHeader">ヘッダを出力する場合はTrue。(省略時true)</param>
    ''' <param name="isDblQuate">フィールドをダブルクォートで囲む場合はTrue。(省略時false)</param>
    ''' <remarks></remarks>
    Public Shared Sub CreateCsvFile(ByVal sFileName As String, ByVal dt As DataTable, ByVal sCsvPath As String, Optional ByVal isOutputHeader As Boolean = True, Optional ByVal isDblQuate As Boolean = False)

        '指定されたファイルが既に存在する場合は削除。
        System.IO.File.Delete(sCsvPath & "\" & sFileName)

        'CSVを出力
        Using writer As New System.IO.StreamWriter(sCsvPath & "\" & sFileName, False, ENC)
            Dim sCsv As String = CreateCsvStr(dt, isOutputHeader, isDblQuate)
            writer.Write(sCsv)
        End Using

    End Sub

    ''' <summary>
    ''' CSVファイル文字列を作成します。
    ''' </summary>
    ''' <param name="tblData"></param>
    ''' <param name="isOutputHeader">ヘッダを出力する場合はTrue。(省略時true)</param>
    ''' <param name="isDblQuate">フィールドをダブルクォートで囲む場合はTrue。(省略時false)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreateCsvStr(ByVal tblData As DataTable, Optional ByVal isOutputHeader As Boolean = True, Optional ByVal isDblQuate As Boolean = False) As String
        Dim strCsv As New System.Text.StringBuilder
        Dim lstLineData As New List(Of String)
        'ヘッダ
        If isOutputHeader Then
            lstLineData.Clear()
            For Each col As DataColumn In tblData.Columns
                lstLineData.Add(col.ColumnName)
            Next
            strCsv.AppendLine(String.Join(CSV_SEPARATOR, lstLineData.ToArray))
        End If
        '明細
        Dim sFieldData As String
        For Each row As DataRow In tblData.Rows
            lstLineData.Clear()
            For Each col As DataColumn In tblData.Columns
                sFieldData = row.Item(col).ToString
                If isDblQuate Then
                    '--フォールドデータをダブルクォートで囲む
                    sFieldData = DBLQUATE & sFieldData & DBLQUATE
                Else
                    '--フィールドデータをダブルクォートで囲まない場合、データ中のカンマを置換する
                    sFieldData = sFieldData.Replace(CSV_SEPARATOR, "")

                    '--フィールドデータをダブルクォートで囲まない場合、データ中の改行を置換する Update 2017/10/14 Nicevision Inoue
                    sFieldData = sFieldData.Replace(vbCrLf, "")
                    sFieldData = sFieldData.Replace(vbLf, "")
                    sFieldData = sFieldData.Replace(vbCr, "")

                End If
                lstLineData.Add(sFieldData)
            Next
            strCsv.AppendLine(String.Join(CSV_SEPARATOR, lstLineData.ToArray))
        Next

        Return strCsv.ToString

    End Function

    ''' <summary>
    ''' 指定したCSVファイルからデータを読み込みDataTableで返す。
    ''' </summary>
    ''' <param name="path">読み取るファイルのパス</param>
    ''' <param name="enc">エンコード</param>
    ''' <param name="cSeparator">区切文字</param>
    ''' <param name="isFirstTitle">1行目がタイトルかどうか</param>
    ''' <param name="sTblName">DataTable名</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ReadCsvFileToDataTable(ByVal path As String, ByVal enc As System.Text.Encoding, ByVal cSeparator As Char, ByVal isFirstTitle As Boolean, ByVal sTblName As String, ByVal sKoumokusuu As Integer, ByRef isKomokusuuErr As Boolean) As DataTable

        'ファイルから読み込み
        Dim sLines As String() = System.IO.File.ReadAllLines(path, enc)

        'ファイルから読み込んだデータを分解し、リストに格納
        Dim lstRows As New List(Of String())
        For Each line As String In sLines
            '--行を分解しString配列にする。
            Dim fields As String() = ReadCsvLineToArray(line)
            '--2021/11/25 upd start nv iwamoto
            'lstRows.Add(fields)
            If fields.Length > 0 Then
                lstRows.Add(fields)
            End If
            '--2021/11/25 upd end nv iwamoto
        Next

        '読み込んだデータより最大列数を取得する
        Dim iMaxField As Integer
        For Each fields As String() In lstRows
            If iMaxField < fields.Length Then
                iMaxField = fields.Length
            End If
        Next

        'テーブルに列を追加する。
        Dim tbl As New DataTable(sTblName)
        For idx As Integer = 0 To iMaxField - 1
            tbl.Columns.Add("Field" & (idx + 1), GetType(String))
        Next

        'テーブルにデータを追加する。
        For iRow As Integer = 0 To lstRows.Count - 1
            Dim fields As String() = lstRows(iRow)

            '2019/09/27 nv miyaji add start
            '項目数チェック
            If fields.Length <> sKoumokusuu Then
                isKomokusuuErr = True
                Return Nothing
            End If
            '2019/09/27 nv miyaji add end

            If isFirstTitle AndAlso iRow = 0 Then
                For iCol As Integer = 0 To fields.Length - 1
                    tbl.Columns(iCol).ColumnName = fields(iCol)
                Next
            Else
                Dim newrow As DataRow = tbl.NewRow
                For iCol As Integer = 0 To fields.Length - 1
                    newrow(iCol) = fields(iCol)
                Next
                tbl.Rows.Add(newrow)
            End If
        Next


        ''Test出力
        ''--------------------
        'For idx As Integer = 0 To tbl.Columns.Count - 1
        '    If idx <> 0 Then Console.Write(",")
        '    Console.Write(tbl.Columns(idx).ColumnName)
        'Next
        'Console.WriteLine("")
        'Console.WriteLine("-------------------------------")
        'For Each row As DataRow In tbl.Rows
        '    For idx As Integer = 0 To row.ItemArray.Length - 1
        '        If idx <> 0 Then Console.Write(",")
        '        Console.Write(row(idx))
        '    Next
        '    Console.WriteLine("")
        'Next
        ''--------------------

        Return tbl
    End Function
    ''' <summary>
    ''' 1行分のCSVデータ文字列を、String配列で返す
    ''' </summary>
    ''' <param name="line">ファイルから読み取った1行分の文字列データ</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ReadCsvLineToArray(ByVal line As String) As String()
        Dim lstField As New List(Of String)

        For iSt As Integer = 0 To line.Length - 1
            'フィールドの開始がダブルクォートで始まっているかを判定
            Dim isWQuote As Boolean = False
            If line(iSt) = DBLQUATE Then
                isWQuote = True
                iSt += 1
            End If
            'フィールドの終了位置を検索
            Dim sFind As String
            If isWQuote Then
                sFind = DBLQUATE + CSV_SEPARATOR
            Else
                'ダブルクォート を使用しない場合、


                'sFind = CSV_SEPARATOR
                sFind = CSV_SEPARATOR
            End If
            Dim iEd As Integer = line.IndexOf(sFind, iSt)
            If iEd = -1 Then
                iEd = line.Length
                If isWQuote Then
                    If line(iEd - 1) = DBLQUATE Then
                        iEd = line.Length - 1
                    End If
                End If
            End If
            'フィールドを取得
            Dim sField As String
            sField = line.Substring(iSt, (iEd - iSt))
            'フィールドから2つの連続するダブルクォートを1つのダブルクォートに置換する
            sField = sField.Replace(DBLQUATE + DBLQUATE, DBLQUATE)
            'フィールドを追加
            lstField.Add(sField)
            '開始インデックスをずらす
            If isWQuote Then
                iSt = iEd + 1
            Else
                iSt = iEd
            End If
        Next

        '最終項目が、ダブルクオーテーションでくくられておらず、文字０の場合の対応  Update 2017/10/11 Nicevision Inoue
        If line.Length > 0 Then
            If line(line.Length - 1) = CSV_SEPARATOR Then
                lstField.Add(String.Empty)
            End If
        End If

        Return lstField.ToArray
    End Function

End Class
