''' <summary>
''' 送信用データパラメータ
''' </summary>
Public Class HtEntryProcessDataCmdResParameter
    Inherits CommonResParameter
    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 文言コード(資産紐付エラー)
    ''' </summary>
    ''' <returns></returns>
    Property wordCdTag As String

    ''' <summary>
    ''' 文言コード(貸出上限エラー)
    ''' </summary>
    ''' <returns></returns>
    Property wordCdLeaseErr As String

    ''' <summary>
    ''' 文言コード(貸出上限警告)
    ''' </summary>
    ''' <returns></returns>
    Property wordCdLeaseWarn As String

    ''' <summary>
    ''' 文言コード(予約なしエラー)
    ''' </summary>
    ''' <returns></returns>
    Property wordCdReservation As String
    '2021/09/15 高瀬 v2.0 予約機能 END

    ''' <summary>
    ''' 資産紐付エラーデータ
    ''' </summary>
    Property assetTagList As AssetTagError()

    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 START --
    ''' <summary>
    ''' 貸出上限エラーデータ
    ''' </summary>
    Property assetLeaseList As AssetLeaseError()
    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 END --

    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 貸出対象エラーデータ(貸出時に該当する予約なし)
    ''' </summary>
    ''' <returns></returns>
    Property reserveList As ReserveError()
    '2021/09/15 高瀬 v2.0 予約機能 END

    ''' <summary>
    ''' ロケ大区分存在エラーデータ(返却時に該当するロケ大区分データなし)
    ''' </summary>
    ''' <returns></returns>
    Property notExistLocLCd As Boolean = False
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

'-- 2020/02/12 v1.2 戸祭 貸出上限機能 START --
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
'-- 2020/02/12 v1.2 戸祭 貸出上限機能 END --

'2021/09/15 高瀬 v2.0 予約機能 START
''' <summary>
''' 貸出対象エラーデータ(貸出時に該当する予約なし)
''' </summary>
Public Class ReserveError

    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Property assetNo As String

    ''' <summary>
    ''' 資産ID
    ''' </summary>
    Property assetId As String

End Class
'2021/09/15 高瀬 v2.0 予約機能 END
