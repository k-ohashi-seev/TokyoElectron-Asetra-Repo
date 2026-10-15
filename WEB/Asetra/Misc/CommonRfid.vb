Namespace Misc
    Public Class CommonRfid
        ''' <summary>
        ''' サーバー側モジュールでも MessageRfid を使用できるよう、ここでも定義
        ''' </summary>
        Public Shared MessageRfid As MessageRfid

        Public Shared GdtDeletedDate As Date?              '削除済み日（ログイン時のデータ削除処理で比較、更新する）


        ''' <summary>
        ''' Nothing, Dbnull を考慮した ToString
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Shared Function ToStringNvl(ByVal obj As Object) As String
            If IsNothing(obj) OrElse IsDBNull(obj) Then
                Return String.Empty
            Else
                Return obj.ToString()
            End If
        End Function

        ''' <summary>
        ''' Date型からStringに変換します
        ''' </summary>
        ''' <param name="strData"></param>
        ''' <returns></returns>
        Public Shared Function toStringDate(ByVal strData As Date, ByVal strFormat As String) As String
            Dim strRetData As String = String.Empty
            If Not IsNothing(strData) Then
                strRetData = strData.ToString(strFormat)
            End If
            Return strRetData
        End Function

        ''' <summary>
        ''' 現在日時を返却します
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function getNowDate() As String
            'Return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
            Return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        End Function

        ''' <summary>
        ''' 文字列バイト数取得処理
        ''' </summary>
        ''' <param name="prmValue">文字列</param>
        ''' <returns></returns>
        ''' <remarks>半角文字は1バイト､全角文字は2バイトとして</remarks>
        Public Shared Function SVFncLenB(ByVal prmValue As String) As Integer
            Return System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(prmValue)
        End Function

        ''' <summary>
        ''' 文字列に全角文字が含まれているかをチェックします
        ''' </summary>
        ''' <param name="strText">対象文字列</param>
        ''' <returns>True:全角文字あり　False:全角文字なし</returns>
        Public Shared Function ContainsFullWidth(strText As String) As Boolean
            For Each c As Char In strText
                If IsFullWidth(c) Then
                    '全角文字あり
                    Return True
                End If
            Next
            Return False
        End Function

        ''' <summary>
        ''' 全角文字かどうかをチェックします。
        ''' </summary>
        ''' <param name="targetChar">対象文字</param>
        ''' <returns>True:全角　False:全角以外</returns>
        Public Shared Function IsFullWidth(targetChar As Char) As Boolean
            Dim u As Integer = Convert.ToInt32(targetChar)
            Return (u >= &H1100 AndAlso u <= &HFF60) OrElse (u >= &HFFE0 AndAlso u <= &HFFE6)
        End Function

        ''' <summary>
        ''' 半角数字かどうかをチェックします
        ''' </summary>
        ''' <param name="strText">文字列</param>
        ''' <returns></returns>
        ''' <remarks>True:半角数字 False:半角数字以外</remarks>
        Public Shared Function IsHankakuNumber(ByVal strText As String) As Boolean
            Return Regex.IsMatch(strText, "^[0-9]+$")
        End Function

        ''' <summary>
        ''' マイナスを許容する半角数字かどうかをチェックします
        ''' </summary>
        ''' <param name="strText">文字列</param>
        ''' <returns></returns>
        ''' <remarks>True:半角数字 False:半角数字以外</remarks>
        Public Shared Function IsMinusHankakuNumber(ByVal strText As String) As Boolean
            Return Regex.IsMatch(strText, "^-?[0-9]+$")
        End Function

        ''' <summary>
        ''' 半角英数字かどうかをチェックします
        ''' </summary>
        ''' <param name="strText">文字列</param>
        ''' <returns></returns>
        ''' <remarks>True:半角英数字 False:半角英数字以外</remarks>
        Public Shared Function IsHankaku(ByVal strText As String) As Boolean

            Return Regex.IsMatch(strText, "^[a-zA-Z0-9]+$")

        End Function

        '2022/08/31 ADD Strat Kawamoto v2.2
        ''' <summary>
        ''' 資産番号（半角英数字記号(#$*+-.:;=<>_)かどうか）をチェックします
        ''' </summary>
        ''' <param name="strText">文字列</param>
        ''' <returns></returns>
        ''' <remarks>True:半角英数字記号(#$*+-.:;=<>_) False:半角英数字記号(#$*+-.:;=<>_)以外</remarks>
        Public Shared Function IsAssetNo(ByVal strText As String) As Boolean

            Return Regex.IsMatch(strText, "^[a-zA-Z0-9#\$\*\+\-\.:;=<>_]+$")

        End Function
        '2022/08/31 ADD End Kawamoto v2.2

        ''' <summary>
        ''' 半角英数字記号かどうかをチェックします
        ''' </summary>
        ''' <param name="strText">文字列</param>
        ''' <returns></returns>
        ''' <remarks>True:半角英数字記号 False:半角英数字記号以外</remarks>
        Public Shared Function IsHanEiKigou(ByVal strText As String) As Boolean

            Return Regex.IsMatch(strText, "^[a-zA-Z0-9!-/:-@¥[-`{-~]*$")

        End Function

        ''' <summary>
        ''' 半角数字+"-"かどうかをチェックします
        ''' </summary>
        ''' <param name="strText">文字列</param>
        ''' <returns></returns>
        ''' <remarks>True:半角英数字記号 False:半角英数字記号以外</remarks>
        Public Shared Function IsHanSuuHaifun(ByVal strText As String) As Boolean

            Return Regex.IsMatch(strText, "^[0-9-]*$")

        End Function

        '2023/02/08 Kawamoto ADD START
        ''' <summary>
        ''' 小数値かどうかをチェックします
        ''' </summary>
        ''' <param name="prmValue">チェック値</param>
        ''' <param name="prmMaxLength">最大桁数</param>
        ''' <param name="prmDecLength">小数部桁数</param>
        ''' <param name="prmMinus">マイナス値の許容</param>
        ''' <returns></returns>
        ''' <remarks>0:正常、1:マイナス、2:桁数エラー、3:小数桁エラー、4:数値エラー</remarks>
        Public Shared Function IsDecimal(ByVal prmValue As String,
                                         ByVal prmMaxLength As Integer,
                                         Optional ByVal prmDecLength As Integer = 0,
                                         Optional ByVal prmMinus As Boolean = True) As String
            '数値チェック
            If Not IsNumeric(prmValue) Then
                Return CommonLabel.MSG_ERR_NUMERIC
            End If

            '整数部チェック
            If prmValue.IndexOf(".") < 0 Then
                If SVFncLenB(prmValue.Replace(",", "")) > prmMaxLength Then
                    Return CommonLabel.MSG_ERR_NUMERIC_RANGE
                End If
            Else
                If prmDecLength > 0 Then
                    If SVFncLenB(prmValue.Replace(",", "").Substring(prmValue.IndexOf(".") + 1)) > prmDecLength Then
                        Return CommonLabel.MSG_ERR_DIGITS_RANGE
                    End If
                End If
                If SVFncLenB(prmValue.Replace(",", "").Substring(0, prmValue.IndexOf("."))) > prmMaxLength Then
                    Return CommonLabel.MSG_ERR_NUMERIC_RANGE
                End If
            End If

            'マイナス許容チェック
            If Not prmMinus Then
                If Decimal.Parse(prmValue) < 0 Then
                    Return CommonLabel.MSG_ERR_NUMERIC_PLUS
                End If
            End If

            Return ""
        End Function
        '2023/02/08 Kawamoto ADD END

        ''' <summary>
        ''' 引数に指定した数値を引数に指定した書式に従って、日付型に変換します。
        ''' 変換後、日付をフォーマットした文字列を返します。
        ''' 日付変換できない場合はString.Emptyを返します。
        ''' </summary>
        ''' <param name="target">日付</param>
        ''' <param name="sFormat">日付文字列の書式</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToStringDateYMD(ByVal target As Integer, ByVal ParamArray sFormat() As String) As String
            Dim ret As String = String.Empty
            Dim dt As Date? = ToDate(target.ToString, sFormat)
            If dt.HasValue = False Then
                Return String.Empty
            End If
            ret = dt.Value.ToString("yyyy/MM/dd")
            Return ret
        End Function

        ''' <summary>
        ''' 日付をフォーマットした文字列を返します。
        ''' </summary>
        ''' <param name="target">日付</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToStringDateYMD(ByVal target As Date) As String
            Return target.ToString("yyyy/MM/dd")
        End Function

        ''' <summary>
        ''' 日付をフォーマットした文字列を返します(時分込み)。
        ''' </summary>
        ''' <param name="target">日付</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToStringDateYMDHM(ByVal target As Date) As String
            Return target.ToString("yyyy/MM/dd HH:mm")
        End Function

        ''' <summary>
        ''' 日付をフォーマットした文字列を返します(時分込み)。
        ''' </summary>
        ''' <param name="target">日付</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToStringLongYMDHM(ByVal target As Long) As String
            Dim ret As String = String.Empty
            Dim starget As String = target.ToString
            ret = starget.Substring(0, 4) & "/" & starget.Substring(4, 2) & "/" & starget.Substring(6, 2) & " " & starget.Substring(8, 2) & ":" & starget.Substring(10, 2)
            Return ret
        End Function

        ''' <summary>
        ''' 日付をフォーマットした文字列を返します(時分秒込み)。
        ''' </summary>
        ''' <param name="target">日付</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToStringLongYMDHMS(ByVal target As Long) As String
            Dim ret As String = String.Empty
            Dim starget As String = target.ToString
            ret = starget.Substring(0, 4) & "/" & starget.Substring(4, 2) & "/" & starget.Substring(6, 2) & " " &
                    starget.Substring(8, 2) & ":" & starget.Substring(10, 2) & ":" & starget.Substring(12, 2)
            Return ret
        End Function

        ''' <summary>
        ''' 日付をフォーマットした文字列を返します(時分秒ミリ秒込み)。
        ''' </summary>
        ''' <param name="target">日付</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToStringLongYMDHMSF(ByVal target As Long) As String
            Dim ret As String = String.Empty
            Dim starget As String = target.ToString
            ret = starget.Substring(0, 4) & "/" & starget.Substring(4, 2) & "/" & starget.Substring(6, 2) & " " &
                    starget.Substring(8, 2) & ":" & starget.Substring(10, 2) & ":" & starget.Substring(12, 2) & "." & starget.Substring(14, 3)
            Return ret
        End Function

        ''' <summary>
        ''' 日付をフォーマットした文字列を返します(時分込み)。
        ''' </summary>
        ''' <param name="target">日付</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToStringStringYMDHM(ByVal target As Long) As String
            Dim ret As String = String.Empty
            Dim starget As String = target.ToString
            ret = starget.Substring(0, 4) & "/" & starget.Substring(4, 2) & "/" & starget.Substring(6, 2) & " " & starget.Substring(8, 2) & ":" & starget.Substring(10, 2)
            Return ret
        End Function

        ''' <summary>
        ''' 日付をフォーマットした文字列を返します。
        ''' </summary>
        ''' <param name="target">日付</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToStringIntYMD(ByVal target As Integer) As String
            Dim ret As String = String.Empty
            Dim starget As String = target.ToString
            ret = starget.Substring(0, 4) & "/" & starget.Substring(4, 2) & "/" & starget.Substring(6, 2)
            Return ret
        End Function

        ''' <summary>
        ''' 日付をフォーマットした文字列を返します。(日付をハイフンで区切ります)
        ''' </summary>
        ''' <param name="target">日付</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToHaifunYMD(ByVal target As Date) As String
            Dim ret As String = String.Empty
            ret = target.ToString("yyyy-MM-dd")
            Return ret
        End Function

        ''' <summary>
        ''' 時間をフォーマットした文字列を返します（時分）。
        ''' </summary>
        ''' <param name="target">時間</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToStringLongHM(ByVal target As Long) As String
            Dim ret As String = String.Empty
            Dim starget As String = target.ToString
            ret = starget.Substring(0, 2) & ":" & starget.Substring(2, 2)
            Return ret
        End Function

        ''' <summary>
        ''' 引数に指定した文字列を引数に指定した書式に従って、日付型に変換します。
        ''' 変換できない場合はNothingを返します。
        ''' </summary>
        ''' <param name="stTarget">日付文字列</param>
        ''' <param name="sFormat">日付文字列の書式</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToDate(ByVal stTarget As String, ByVal ParamArray sFormat() As String) As Nullable(Of DateTime)
            Dim ret As Nullable(Of Date) = Nothing
            Dim dt As DateTime
            For Each fmt As String In sFormat
                If DateTime.TryParseExact(stTarget, fmt, Nothing, Nothing, dt) = True Then
                    ret = dt
                    Exit For
                End If
            Next
            Return ret
        End Function

        ''' <summary>
        ''' 引数に指定した文字列を数値に変換します。
        ''' 変換できない場合はNothingを返します。
        ''' </summary>
        ''' <param name="sTarget">数値文字列</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToNumber(ByVal sTarget As String) As Nullable(Of Decimal)
            Dim ret As Nullable(Of Decimal) = Nothing
            Dim dec As Decimal
            If Decimal.TryParse(sTarget, dec) = True Then
                ret = dec
            End If
            Return ret
        End Function

        ''' <summary>
        ''' 引数に指定した文字列が引数に指定した書式に従って、日付に変換できるかを返します。
        ''' </summary>
        ''' <param name="stTarget">日付文字列</param>
        ''' <param name="sFormat">日付文字列の書式</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function IsDate(ByVal stTarget As String, ByVal ParamArray sFormat() As String) As Boolean
            Dim ret As Boolean = False
            Dim dt As DateTime
            For Each fmt As String In sFormat
                If DateTime.TryParseExact(stTarget, fmt, Nothing, Nothing, dt) = True Then
                    ret = True
                    Exit For
                End If
            Next
            Return ret
        End Function

        ''' <summary>
        ''' 引数に指定した文字列が数値に変換できるかを返します。
        ''' </summary>
        ''' <param name="sTarget">数値文字列</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function IsNumber(ByVal sTarget As String) As Boolean
            Dim ret As Boolean = False
            Dim dec As Decimal
            ret = Decimal.TryParse(sTarget, dec)
            Return ret
        End Function

        ''' <summary>
        ''' バイト単位で文字列切り出し（任意の範囲）
        ''' </summary>
        ''' <param name="prmTarget">元の文字列</param>
        ''' <param name="prmStart">切り出し開始位置</param>
        ''' <param name="prmByteSize">切り出しバイト数</param>
        ''' <returns>切り出し文字列</returns>
        ''' <remarks></remarks>
        Public Shared Function SVFncMidB(ByVal prmTarget As String, ByVal prmStart As Integer, ByVal prmByteSize As Integer) As String

            Dim hEncoding As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_JIS")
            Dim bytes As Byte() = hEncoding.GetBytes(prmTarget)

            '' 開始が最大文字数より後ろだった場合、空文字を戻す
            If bytes.Length < prmStart Then
                Return String.Empty
            End If

            '' prmByteSizeが最大文字数を超えないように調整
            If (bytes.Length < (prmStart - 1) + prmByteSize) Then
                prmByteSize = bytes.Length - (prmStart - 1)
            End If
            '' 指定バイト数取りだし
            Dim strMidStr = hEncoding.GetString(bytes, prmStart - 1, prmByteSize)

            ''最初の文字が全角の途中で切れていた場合はカット
            Dim strLeft As String = hEncoding.GetString(bytes, 0, prmStart)
            Dim strFirstMoji As Char = prmTarget(strLeft.Length - 1)
            If (strMidStr <> "" AndAlso strFirstMoji <> strMidStr(0)) Then
                strMidStr = strMidStr.Substring(1)
            End If

            ''最後の文字が全角の途中で切れていた場合はカット
            strLeft = hEncoding.GetString(bytes, 0, (prmStart - 1) + prmByteSize)
            Dim strLastMoji As Char = prmTarget(strLeft.Length - 1)
            If (strMidStr <> "" AndAlso strLastMoji <> strMidStr(strMidStr.Length - 1)) Then
                strMidStr = strMidStr.Substring(0, strMidStr.Length - 1)
            End If

            Return strMidStr

        End Function

        ''' <summary>
        ''' 指定フォルダ内の、指定日より古い更新日のファイルを削除します。
        ''' </summary>
        ''' <param name="targetDate">削除対象日</param>
        ''' <param name="dirPath">対象フォルダのフルパス</param>
        ''' <returns>処理件数</returns>
        Public Shared Function DeleteFiles(targetDate As Date, dirPath As String) As Integer

            Dim count As Integer = 0

            'フォルダの存在チェック
            If System.IO.Directory.Exists(dirPath) = False Then
                Return 0
            End If

            '指定フォルダ内の全ファイルを取得
            Dim files As String() = System.IO.Directory.GetFiles(dirPath)

            'ファイルの更新日が指定日よりも古い場合は削除
            For Each file As String In files
                Dim updDate As Date = System.IO.File.GetLastWriteTime(file)
                If updDate.Date < targetDate.Date Then
                    Try
                        System.IO.File.Delete(file)
                        count += 1
                    Catch ex As Exception
                        '権限設定等で、Exception に飛ぶ場合があるので、エラーがでないようにする
                    End Try
                End If
            Next

            Return count

        End Function

        ''' <summary>
        ''' 指定した小数点以下有効桁数になるよう切り上げます。
        ''' </summary>
        ''' <param name="dValue">切り上げ対象の数値</param>
        ''' <param name="iDigits">小数点以下有効桁数</param>
        ''' <returns></returns>
        Public Shared Function ToRoundUp(ByVal dValue As Decimal, ByVal iDigits As Integer) As Decimal
            '対象の数値が負数かどうかを取得します。
            Dim isMinus As Boolean = dValue < 0

            '対象の数値の絶対値を取得します。
            dValue = System.Math.Abs(dValue)

            '10 の iDigits乗を取得します。
            Dim dCoef As Decimal = CDec(System.Math.Pow(10, iDigits))

            '切り上げ処理を行います。
            ''--Ceiling メソッドは、指定した数値以上の最小の整数を返します。
            'dValue = System.Math.Ceiling(dValue * dCoef) / dCoef
            '--Floor メソッドは、指定した数値以下の最大の整数を返します。
            dValue = CDec(System.Math.Floor(dValue * dCoef + 0.9)) / dCoef

            '負数の場合は絶対値をマイナスに戻します。
            If isMinus Then
                dValue = dValue * -1
            End If

            Return dValue

            'テスト結果
            '123.456 ,3 =123.456
            '123.456 ,2 =123.46
            '123.456 ,1 =123.5
            '123.456 ,0 =124

            '-123.456 ,3 = -123.456
            '-123.456 ,2 = -123.46
            '-123.456 ,1 = -123.5
            '-123.456 ,0 = -124

        End Function

        ''' <summary>
        ''' 指定した小数点以下有効桁数になるよう切り捨てます。
        ''' </summary>
        ''' <param name="dValue">切り上げ対象の数値</param>
        ''' <param name="iDigits">小数点以下有効桁数</param>
        ''' <returns></returns>
        Public Shared Function ToRoundDown(ByVal dValue As Decimal, ByVal iDigits As Integer) As Decimal

            '対象の数値が負数かどうかを取得します。
            Dim isMinus As Boolean = dValue < 0

            '対象の数値の絶対値を取得します。
            dValue = System.Math.Abs(dValue)

            '10 の iDigits乗を取得します。
            Dim dCoef As Decimal = CDec(System.Math.Pow(10, iDigits))

            '切り捨て処理を行います。
            '--Floor メソッドは、指定した数値以下の最大の整数を返します。
            dValue = System.Math.Floor(dValue * dCoef) / dCoef

            '負数の場合は絶対値をマイナスに戻します。
            If isMinus Then
                dValue = dValue * -1
            End If

            Return dValue


            'テスト結果
            '123.456 ,3 =123.456
            '123.456 ,2 =123.45
            '123.456 ,1 =123.4
            '123.456 ,0 =123

            '-123.456 ,3 = -123.456
            '-123.456 ,2 = -123.45
            '-123.456 ,1 = -123.4
            '-123.456 ,0 = -123

        End Function

        ''' <summary>
        ''' 指定した小数点以下有効桁数になるよう四捨五入します。
        ''' </summary>
        ''' <param name="dValue">切り上げ対象の数値</param>
        ''' <param name="iDigits">小数点以下有効桁数</param>
        ''' <returns></returns>
        Public Shared Function ToRound(ByVal dValue As Decimal, ByVal iDigits As Integer) As Decimal
            '対象の数値が負数かどうかを取得します。
            Dim isMinus As Boolean = dValue < 0

            '対象の数値の絶対値を取得します。
            dValue = System.Math.Abs(dValue)

            '10 の iDigits乗を取得します。
            Dim dCoef As Decimal = CDec(System.Math.Pow(10, iDigits))

            '四捨五入処理を行います。
            '--Floor メソッドは、指定した数値以下の最大の整数を返します。
            dValue = CDec(System.Math.Floor(dValue * dCoef + 0.5)) / dCoef

            '負数の場合は絶対値をマイナスに戻します。
            If isMinus Then
                dValue = dValue * -1
            End If

            Return dValue


            'テスト結果
            '123.456 ,3 =123.456
            '123.456 ,2 =123.46
            '123.456 ,1 =123.5
            '123.456 ,0 =123

            '-123.456 ,3 = -123.456
            '-123.456 ,2 = -123.46
            '-123.456 ,1 = -123.5
            '-123.456 ,0 = -123
        End Function

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' 文字列（ASCII）を16進数に変換
        ''' </summary>
        ''' <param name="prmString">変換する文字列</param>
        Public Shared Function FncStringToHex(prmString As String) As String
            '文字列が空白の場合は空文字を返す
            If String.IsNullOrEmpty(prmString) Then
                Return String.Empty
            End If

            Dim strHex As New StringBuilder
            For Each c As Char In prmString
                strHex.Append(Hex(Asc(c)))
            Next

            '変換した値を返す
            Return strHex.ToString
        End Function
        '2023/03/22 Kawamoto ADD END

        ''' <summary>
        ''' 指定文字数の桁数切り捨て      2023/10/17 ADD Sato v2.5
        ''' </summary>
        ''' <param name="inputString">文字列</param>
        ''' <param name="truncLength">桁数</param>
        ''' <returns></returns>
        Public Shared Function TruncateString(inputString As String, truncLength As Integer) As String
            '空の場合はそのまま返す
            If String.IsNullOrEmpty(inputString) Then
                Return inputString
            End If

            ' 文字列を文字配列に変換
            Dim chars As Char() = inputString.ToCharArray()

            ' 指定された桁数が文字列の桁数よりも多い場合、元の文字列をそのまま返す
            If chars.Length <= truncLength Then
                Return inputString
            End If

            ' 文字列を指定された桁数で切り捨てる
            Return New String(chars, 0, truncLength)
        End Function

        ''' <summary>
        ''' 周波数を表示用のフォーマットに変換
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Shared Function FormatFrequency(input As String) As String
            ' 変換が成功したかをチェックするための変数
            Dim decimalValue As Decimal

            ' 文字列が数値に変換可能かをチェック
            If Decimal.TryParse(input, decimalValue) Then
                ' 変換後の値が0であれば空白を返す
                If decimalValue = 0 Then
                    Return ""
                Else
                    ' 数値を "0.0###" 形式でフォーマット
                    Return decimalValue.ToString("0.0###")
                End If
            Else
                ' 数値変換が不可能な場合はそのまま返す
                Return input
            End If
        End Function

#Region "複数選択・複数入力の検索条件 共通処理"
        ''' <summary>
        ''' 複数選択された値から、空の値を除いた配列を取得します
        ''' </summary>
        ''' <param name="values">複数選択された値（未選択時は Nothing）</param>
        ''' <returns>空の値を除き、前後の空白を除去して重複を除いた配列</returns>
        Public Shared Function GetSelectedValues(ByVal values As String()) As String()
            If values Is Nothing Then
                Return New String() {}
            End If

            Return values.Where(Function(value) Not String.IsNullOrWhiteSpace(value)) _
                         .Select(Function(value) value.Trim) _
                         .Distinct() _
                         .ToArray()
        End Function

        ''' <summary>
        ''' 複数選択された値の先頭を取得します
        ''' </summary>
        ''' <param name="values">複数選択された値（未選択時は Nothing）</param>
        ''' <returns>先頭の値（未選択時は空文字）</returns>
        ''' <remarks>単一の値しか指定できない処理（モーダル画面のコンボ設定など）で使用します</remarks>
        Public Shared Function GetFirstSelectedValue(ByVal values As String()) As String
            Dim lstValue As String() = GetSelectedValues(values)
            If lstValue.Length = 0 Then
                Return String.Empty
            End If

            Return lstValue(0)
        End Function

        ''' <summary>
        ''' 「|」区切りで複数入力された値から、空の値を除いた配列を取得します
        ''' </summary>
        ''' <param name="value">「|」区切りの入力値</param>
        ''' <returns>空の値を除き、前後の空白を除去して重複を除いた配列</returns>
        ''' <remarks>区切り文字（CommonConst.MULTI_INPUT_DELIMITER）は Scripts\multi-input-dialog.js と合わせています</remarks>
        Public Shared Function GetMultiInputValues(ByVal value As String) As String()
            If String.IsNullOrEmpty(value) Then
                Return New String() {}
            End If

            Return GetSelectedValues(value.Split(CommonConst.MULTI_INPUT_DELIMITER))
        End Function
#End Region

    End Class
End Namespace
