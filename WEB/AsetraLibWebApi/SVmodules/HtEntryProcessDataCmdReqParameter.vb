''' <summary>
''' 処理データ登録用パラメータ
''' </summary>
Public Class HtEntryProcessDataCmdReqParameter
    ''' <summary>
    ''' 処理データ明細情報
    ''' </summary>
    Public processDataList As EntryProcessDataDetail()
End Class

''' <summary>
''' 処理データ明細情報
''' </summary>
Public Class EntryProcessDataDetail
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
    ''' 資産番号
    ''' </summary>
    Public assetNo As String
    ''' <summary>
    ''' 資産ID
    ''' </summary>
    Public assetId As String
    ''' <summary>
    ''' 処理ツール区分
    ''' </summary>
    Public processToolDiv As String
    ''' <summary>
    ''' 処理区分
    ''' </summary>
    Public processDiv As String
    ''' <summary>
    ''' 貸出先場所コード
    ''' </summary>
    Public leasePlaceCd As String
    ''' <summary>
    ''' 貸出先担当者社員コード
    ''' </summary>
    Public leaseEmployeeCd As String
    ''' <summary>
    ''' 貸出返却予定日
    ''' </summary>
    Public leaseDueDt As String
    ''' <summary>
    ''' 備考
    ''' </summary>
    Public remark As String
    ''' <summary>
    ''' ロケーション大区分コード
    ''' </summary>
    Public locLCd As String
    ''' <summary>
    ''' ロケーション中区分コード
    ''' </summary>
    Public locMCd As String
    ''' <summary>
    ''' ロケーション小区分コード
    ''' </summary>
    Public locSCd As String
    ''' <summary>
    ''' ログイン担当者コード
    ''' </summary>
    Public employeeCd As String

End Class
