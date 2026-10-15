''' <summary>
''' マスタデータ受信用パラメータ
''' </summary>
Public Class HtTHGetMasterDataCmdReqParameter

    ''' <summary>
    ''' ログイン担当者コード
    ''' </summary>
    Public employeeCd As String

    ''' <summary>
    ''' 
    ''' </summary>
    Public masterDataList As MasterDataList()

End Class

Public Class MasterDataList
    ''' <summary>
    ''' ログイン担当者名
    ''' </summary>
    Public employeeName As String
End Class
