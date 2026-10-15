''' <summary>
''' マスタデータ受信用パラメータ
''' </summary>
Public Class HtGetMasterDataCmdReqParameter

    '2023/01/31 v2.3 Asetra 資産１００万件対応 川元 START
    ''' <summary>
    ''' 社員コード
    ''' </summary>
    Public employeeCd As String
    '2023/01/31 v2.3 Asetra 資産１００万件対応 川元 END

    ''' <summary>
    ''' マスタデータ明細情報
    ''' </summary>
    Public masterDataList As GetMasterDataDetail()

End Class

''' <summary>
''' マスタデータ明細情報
''' </summary>
Public Class GetMasterDataDetail
    ''' <summary>
    ''' マスター名
    ''' </summary>
    Public masterName As String
End Class
