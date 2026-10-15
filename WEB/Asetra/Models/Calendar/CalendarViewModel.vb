Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' カレンダーメンテ用表示用モデル
    ''' </summary>
    Partial Public Class CalendarViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 日付
        ''' </summary>
        ''' <returns>日付</returns>
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property DT As Integer

        ''' <summary>
        ''' 非稼働日フラグ
        ''' </summary>
        ''' <returns></returns>
        <Required>
        <StringLength(1)>
        Public Property NOT_WORK_FLG As String

        ''' <summary>
        ''' 最終登録更新日時
        ''' </summary>
        ''' <returns></returns>
        Public Property LAST_INS_DATETIME As Date

        ''' <summary>
        ''' 最終登録更新者氏名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property LAST_INS_EMPLOYEE_NAME As String

        ''' <summary>
        ''' 検索条件 年月日From
        ''' </summary>
        ''' <returns>検索条件 年月日From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchDtFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 年月日To
        ''' </summary>
        ''' <returns>検索条件 年月日To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchDtTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 稼働日
        ''' </summary>
        ''' <returns>検索条件 稼働日</returns>
        <NotMapped>
        Public Property SearchNotWorkFlgNo As Boolean = False

        ''' <summary>
        ''' 検索条件 非稼働日
        ''' </summary>
        ''' <returns>検索条件 非稼働日</returns>
        <NotMapped>
        Public Property SearchNotWorkFlgYes As Boolean = False

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' カレンダー情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
