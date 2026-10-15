''' <summary>
''' 送信用データパラメータ
''' </summary>
Public Class HtEntryProcessDataCmdResParameter
    Inherits CommonResParameter

    '20210918 Add Takase V2.0 貸出対象 START
    ''' <summary>
    ''' 文言コード(予約エラー)
    ''' </summary>
    ''' <returns></returns>
    Property wordCdReservation As String

    ''' <summary>
    ''' 文言コード(貸出上限エラー)
    ''' </summary>
    ''' <returns></returns>
    Property wordCdLeaseErr As String

    ''' <summary>
    ''' 文言コード(タグ紐付エラー)
    ''' </summary>
    ''' <returns></returns>
    Property wordCdTag As String

    ''' <summary>
    ''' 文言コード(貸出上限警告)
    ''' </summary>
    ''' <returns></returns>
    Property wordCdLeaseWarn As String

    '20210918 Add Takase V2.0 貸出対象 END

    ''' <summary>
    ''' 資産紐付エラーデータ
    ''' </summary>
    Property assetTagList As AssetTagError()

    ''' <summary>
    ''' 貸出上限エラーデータ
    ''' </summary>
    Property assetLeaseList As AssetLeaseError()      '20200310 Add Sasaki V1.2 貸出上限

    '20210918 Add Takase V2.0 貸出対象 START
    ''' <summary>
    ''' 貸出対象エラーデータ
    ''' </summary>
    Property reserveList As AssetReserveError()
    '20210918 Add Takase V2.0 貸出対象 END

End Class

''' <summary>
''' タグ紐付エラーデータ
''' </summary>
Public Class AssetTagError
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Property assetNo As String

    ''' <summary>
    ''' 資産ID
    ''' </summary>
    Property assetId As String

    ''' <summary>
    ''' 重複資産番号
    ''' </summary>
    Property dupAssetNo As String


End Class

'20200310 Add Start Sasaki V1.2 貸出上限
''' <summary>
''' 貸出上限エラーデータ
''' </summary>
Public Class AssetLeaseError

    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Property assetNo As String

    ''' <summary>
    ''' 資産ID
    ''' </summary>
    Property assetId As String

    ''' <summary>
    ''' 貸出上限数
    ''' </summary>
    Property upperLimitCount As Integer

End Class
'20200310 Add End Sasaki V1.2 貸出上限


'20210918 Add Takase V2.0 貸出対象 START
''' <summary>
''' 貸出上限エラーデータ
''' </summary>
Public Class AssetReserveError

    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Property assetNo As String

    ''' <summary>
    ''' 資産ID
    ''' </summary>
    Property assetId As String

End Class
'20210918 Add Takase V2.0 貸出対象 END