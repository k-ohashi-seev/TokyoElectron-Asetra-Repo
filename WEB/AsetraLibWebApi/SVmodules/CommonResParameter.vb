''' <summary>
''' 戻り値Enum
''' </summary>
Public Enum ReturnCodeEnum
    OK = 0                  ' 正常終了
    NG = 1                  ' 異常終了
    WARN = 2                ' 警告終了（正常終了扱い）
    CONFIRM = 3             ' 確認
    'WARN_TAG = 4            ' 警告終了（正常終了扱い）タグ紐付エラーあり
    'LEASE_UPPER_ERR = 5     ' 貸出上限エラー           20200309 Add Sasaki V1.2 貸出上限
    'LEASE_UPPER_WARN = 6    ' 貸出上限警告             20200309 Add Sasaki V1.2 貸出上限
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
End Class

