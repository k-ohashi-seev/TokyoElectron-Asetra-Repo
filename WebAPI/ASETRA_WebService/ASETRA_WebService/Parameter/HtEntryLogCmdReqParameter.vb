''' <summary>
''' 操作ログ登録用パラメータ
''' </summary>
Public Class HtEntryLogCmdReqParameter
    ''' <summary>
    ''' 操作ログ情報
    ''' </summary>
    Public logDataDetailList As EntryLogDataDetail()

End Class

''' <summary>
''' 操作ログ情報
''' </summary>
Public Class EntryLogDataDetail
    ''' <summary>
    ''' 操作ログID
    ''' </summary>
    Public opeLogId As String

    ''' <summary>
    ''' 処理日
    ''' </summary>
    Public processDt As String

    ''' <summary>
    ''' 処理時刻
    ''' </summary>
    Public processTm As String

    ''' <summary>
    ''' 処理ツール区分
    ''' </summary>
    Public processToolDiv As String

    ''' <summary>
    ''' 処理種別
    ''' </summary>
    Public processCat As String

    ''' <summary>
    ''' 処理内容
    ''' </summary>
    Public processContents As String

    ''' <summary>
    ''' 操作者社員コード
    ''' </summary>
    Public opeEmployeeCd As String

End Class

