''' <summary>
''' 送信用時刻データパラメータ
''' </summary>
Public Class HtGetDateTimeCmdResParameter
    Inherits CommonResParameter

    ''' <summary>
    ''' システム日付・時刻
    ''' </summary>
    Property getSystemDateTime As GetDateTime()

End Class

''' <summary>
''' 時刻データ
''' </summary>
Public Class GetDateTime

    ''' <summary>
    ''' システム日付
    ''' </summary>
    Property systemDt As String

    ''' <summary>
    ''' システム時刻
    ''' </summary>
    Property systemTm As String

End Class
