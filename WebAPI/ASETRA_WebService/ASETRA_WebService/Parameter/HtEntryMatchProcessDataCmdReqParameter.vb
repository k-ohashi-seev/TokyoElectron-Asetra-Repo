''' <summary>
''' 処理データ登録用パラメータ
''' </summary>
Public Class HtEntryMatchProcessDataCmdReqParameter
    ''' <summary>
    ''' 処理データ明細情報
    ''' </summary>
    Public matchProcessDataList As EntryMatchProcessDataDetail()
End Class

''' <summary>
''' 処理データ明細情報
''' </summary>
Public Class EntryMatchProcessDataDetail
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
    Public constNo As String
    ''' <summary>
    ''' 貸出先担当者社員コード
    ''' </summary>
    Public leaseEmployeeCd As String
    ''' <summary>
    ''' 貸出返却予定日
    ''' </summary>
    Public leaseDueDt As String
    ''' <summary>
    ''' 照合結果
    ''' </summary>
    Public contMatchDiv As String
    ''' <summary>
    ''' 照合数
    ''' </summary>
    Public contMatchNum As String

    ''' <summary>
    ''' 貸出リスト
    ''' </summary>
    Public lentAssetList As EntryLentAssetListDetail()

    ''' <summary>
    ''' 返却リスト
    ''' </summary>
    Public retAssetList As EntryRetAssetListDetail()

    ''' <summary>
    ''' 照合資産リスト
    ''' </summary>
    Public matchAssetList As EntryMatchAssetListDetail()

    ''' <summary>
    ''' 仮置きリスト
    ''' </summary>
    Public tempList As EntryTempListDetail()

End Class

''' <summary>
''' 貸出リスト
''' </summary>
Public Class EntryLentAssetListDetail
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Public assetNo As String
    ''' <summary>
    ''' 持出日
    ''' </summary>
    Public leaseDt As String
    ''' <summary>
    ''' 備考
    ''' </summary>
    Public remark As String

End Class

''' <summary>
''' 返却リスト
''' </summary>
Public Class EntryRetAssetListDetail
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Public assetNo As String

    ''' <summary>
    ''' 照合結果
    ''' </summary>
    Public contMatchDiv As String

    ''' <summary>
    ''' 検査フラグ
    ''' </summary>
    Public inspeFlag As String

End Class

''' <summary>
''' 照合資産リスト
''' </summary>
Public Class EntryMatchAssetListDetail
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Public assetNo As String
    ''' <summary>
    ''' 照合結果
    ''' </summary>
    Public contMatchDiv As String

End Class


''' <summary>
''' 仮置きリスト
''' </summary>
Public Class EntryTempListDetail
    ''' <summary>
    ''' 仮置きID
    ''' </summary>
    Public tempId As String
    ''' <summary>
    ''' 持出日
    ''' </summary>
    Public tempDt As String
    ''' <summary>
    ''' 作業責任者コード
    ''' </summary>
    Public tempEmpCd As String
    ''' <summary>
    ''' 作業責任者名
    ''' </summary>
    Public tempEmpName As String
    ''' <summary>
    ''' 仮置き情報1
    ''' </summary>
    Public tempInfo1 As String
    ''' <summary>
    ''' 仮置き情報2
    ''' </summary>
    Public tempInfo2 As String
    ''' <summary>
    ''' 仮置き情報3
    ''' </summary>
    Public tempInfo3 As String
    ''' <summary>
    ''' 仮置き情報4
    ''' </summary>
    Public tempInfo4 As String
    ''' <summary>
    ''' 仮置き情報5
    ''' </summary>
    Public tempInfo5 As String
    ''' <summary>
    ''' 仮置き情報6
    ''' </summary>
    Public tempInfo6 As String
    ''' <summary>
    ''' 仮置き情報7
    ''' </summary>
    Public tempInfo7 As String
    ''' <summary>
    ''' 備考
    ''' </summary>
    Public tempRemark As String

    ''' <summary>
    ''' 仮置き資産リスト
    ''' </summary>
    Public tempAssetList As EntryTempAssetListDetail()

End Class

''' <summary>
''' 仮置き資産リスト
''' </summary>
Public Class EntryTempAssetListDetail
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Public no As String

End Class

