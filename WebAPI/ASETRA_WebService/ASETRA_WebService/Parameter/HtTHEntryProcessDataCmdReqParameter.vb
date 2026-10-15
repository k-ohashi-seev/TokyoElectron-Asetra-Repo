''' <summary>
''' 処理データ登録用パラメータ
''' </summary>
Public Class HtTHEntryProcessDataCmdReqParameter
    ''' <summary>
    ''' 処理データ明細情報
    ''' </summary>
    Public matchList As THEntryProcessDataDetail()
End Class

''' <summary>
''' 処理データ明細情報
''' </summary>
Public Class THEntryProcessDataDetail
    ''' <summary>
    ''' 処理履歴ID
    ''' </summary>
    Public processLogId As String
    ''' <summary>
    ''' 処理日
    ''' </summary>
    Public processDt As String
    ''' <summary>
    ''' 処理時刻
    ''' </summary>
    Public processTm As String
    ''' <summary>
    ''' ログイン担当者コード
    ''' </summary>
    Public employeeCd As String
    ''' <summary>
    ''' 処理区分
    ''' </summary>
    Public processDiv As String
    ''' <summary>
    ''' 工事番号
    ''' </summary>
    Public contNo As String
    ''' <summary>
    ''' 貸出日
    ''' </summary>
    Public leaseDt As String
    ''' <summary>
    ''' 貸出返却予定日
    ''' </summary>
    Public leaseDueDt As String
    ''' <summary>
    ''' 貸出先担当者社員コード
    ''' </summary>
    Public leaseEmployeeCd As String
    ''' <summary>
    ''' 備考
    ''' </summary>
    Public remark As String

    ''' <summary>
    ''' 貸出リスト
    ''' </summary>
    Public lentAssetList As THEntryLentAssetListDetail()

    ''' <summary>
    ''' 返却リスト
    ''' </summary>
    Public retAssetList As THEntryRetAssetListDetail()

End Class

''' <summary>
''' 貸出リスト
''' </summary>
Public Class THEntryLentAssetListDetail
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Public assetNo As String
End Class

''' <summary>
''' 返却リスト
''' </summary>
Public Class THEntryRetAssetListDetail
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Public assetNo As String

    ''' <summary>
    ''' 検査フラグ
    ''' </summary>
    Public inspeFlg As String
End Class