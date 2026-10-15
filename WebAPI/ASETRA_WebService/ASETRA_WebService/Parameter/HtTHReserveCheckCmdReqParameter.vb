''' <summary>
''' 予約確認用パラメータ
''' </summary>
Public Class HtTHReserveCheckCmdReqParameter

    ''' <summary>
    ''' ログイン担当者コード
    ''' </summary>
    Public employeeCd As String

    ''' <summary>
    ''' 貸出返却区分
    ''' </summary>
    Public lentRetFlg As String

    ''' <summary>
    ''' 工事番号
    ''' </summary>
    Public contNo As String

    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Public assetNo As String

    ''' <summary>
    ''' 処理日
    ''' </summary>
    Public systemDt As String

End Class