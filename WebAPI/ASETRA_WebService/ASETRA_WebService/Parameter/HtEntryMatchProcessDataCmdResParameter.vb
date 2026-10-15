''' <summary>
''' 送信用データパラメータ
''' </summary>
Public Class HtEntryMatchProcessDataCmdResParameter
    Inherits CommonResParameter
    ''' <summary>
    ''' 文言コード
    ''' </summary>
    ''' <returns></returns>
    Property wordCdTag As String

    ''' <summary>
    ''' 持出負荷情報
    ''' </summary>
    Property outErrorList As outErrorList()

    ''' <summary>
    ''' 仮置きID
    ''' </summary>
    Property tempIdList As tempIdList()

End Class

''' <summary>
''' 持出負荷情報
''' </summary>
Public Class outErrorList

    ''' <summary>
    ''' 置換文字列1
    ''' </summary>
    Property rep1 As String

    ''' <summary>
    ''' 置換文字列2
    ''' </summary>
    Property rep2 As String


End Class

''' <summary>
''' 仮置きID
''' </summary>
Public Class tempIdList

    ''' <summary>
    ''' 変換前仮置きID
    ''' </summary>
    Property beforeTempId As String

    ''' <summary>
    ''' 変換後仮置きID
    ''' </summary>
    Property afterTempId As String

    ''' <summary>
    ''' 工事番号
    ''' </summary>
    Property constId As String

End Class


