''' <summary>
''' 戻り値Enum
''' </summary>
Public Enum ReturnCodeEnum
    OK = 0                  ' 正常終了
    NG = 1                  ' 異常終了
    WARN = 2                ' 警告終了（正常終了扱い）
    CONFIRM = 3             ' 確認
    '2021/09/15 高瀬 v2.0 予約機能 START
    'WARN_TAG = 4            ' 警告終了（正常終了扱い）タグ紐付エラーあり
    'WARN_LEASE_UPPER_ERR = 5    ' 警告終了（正常終了扱い）貸出上限越え※登録済み     2020/02/12 戸祭 v1.1 貸出上限機能
    'WARN_LEASE_UPPER_WARN = 6   ' 警告終了（正常終了扱い）貸出上限越え※未登録       2020/02/12 戸祭 v1.1 貸出上限機能
    '2021/09/15 高瀬 v2.0 予約機能 END    
    WARN_ASSET_NO_SEND = 5  ' 警告終了（資産情報未送信、校正（メンテナンス）あり） '2023/01/31 川元 v2.3 Asetra 資産１００万件対応
    RESERVED_SELF = 8       '自分の予約あり
    RESERVED_OTHER = 9      '他者の予約あり
End Enum

''' <summary>
''' WebAPI戻り値格納クラス
''' </summary>
Public Class CommonResParameter
    ''' <summary>
    ''' 戻り値
    ''' </summary>
    Property returnCode As Integer

    ''' <summary>
    ''' 文言コード
    ''' </summary>
    Property wordCd As String

    Property lockAssetList As List(Of LockAsset)
End Class

''' <summary>
''' WebAPI戻り値格納クラス(TH用)
''' </summary>
Public Class CommonResParameterTH
    ''' <summary>
    ''' 戻り値
    ''' </summary>
    Property returnCode As Integer

    ''' <summary>
    ''' 文言コード
    ''' </summary>
    Property message As String

End Class

''' <summary>
''' ロックエラーリスト
''' </summary>
Public Class LockAsset
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    ''' <returns></returns>
    Property assetNo As String
End Class

