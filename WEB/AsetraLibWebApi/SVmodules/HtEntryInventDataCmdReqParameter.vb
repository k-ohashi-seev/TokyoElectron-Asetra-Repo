''' <summary>
''' 実棚情報更新用パラメータ
''' </summary>
Public Class HtEntryInventDataCmdReqParameter

    ''' <summary>
    ''' 実棚明細情報
    ''' </summary>
    Public ie As EntryInventDataDetail()
    ''' <summary>
    ''' 処理ツール区分
    ''' </summary>
    Public td As String
End Class

''' <summary>
''' 実棚明細情報
''' </summary>
Public Class EntryInventDataDetail
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Public an As String
    ''' <summary>
    ''' 実棚日時
    ''' </summary>
    Public pt As String
    ''' <summary>
    ''' 実棚ステータス
    ''' </summary>
    Public ps As String
    ''' <summary>
    ''' 実棚ロケーション大区分コード
    ''' </summary>
    Public lc As String
    ''' <summary>
    ''' 実棚ロケーション中区分コード
    ''' </summary>
    Public mc As String
    ''' <summary>
    ''' 実棚ロケーション小区分コード
    ''' </summary>
    Public sc As String
    ''' <summary>
    ''' 実棚実施者社員コード
    ''' </summary>
    Public ec As String
End Class

