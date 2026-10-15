
''' <summary>
''' 送信用データパラメータ
''' </summary>
Public Class HtTHEmployeeAuthCmdResParameter
    Inherits CommonResParameterTH

    ''' <summary>
    ''' 社員名
    ''' </summary>
    Property employeeName As String

    ''' <summary>
    ''' 本部フラグ
    ''' </summary>
    Property headOfficeFlg As String

    ''' <summary>
    ''' 権限区分
    ''' </summary>
    Property employeeAuth As String

    ''' <summary>
    ''' RFIDタグ拠点
    ''' </summary>
    Property locRFIDFlg As String

End Class