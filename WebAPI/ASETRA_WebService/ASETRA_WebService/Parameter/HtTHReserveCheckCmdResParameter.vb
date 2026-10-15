
''' <summary>
''' 送信用データパラメータ
''' </summary>
Public Class HtTHReserveCheckCmdResParameter
    Inherits CommonResParameterTH

    ''' <summary>
    ''' 期間From
    ''' </summary>
    Property startDt As String

    ''' <summary>
    ''' 期間To
    ''' </summary>
    Property endDt As String

    ''' <summary>
    ''' 予約者
    ''' </summary>
    Property employeeName As String

    ''' <summary>
    ''' 工事番号
    ''' </summary>
    Property constNo As String

End Class