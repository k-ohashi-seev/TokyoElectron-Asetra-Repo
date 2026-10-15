Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class WordViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 言語区分
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=0)>
        <StringLength(2)>
        Public Property LANG_DIV As String

        ''' <summary>
        ''' 利用ツール区分
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=1)>
        <StringLength(1)>
        Public Property TOOL_DIV As String

        ''' <summary>
        ''' 文言区分
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=2)>
        <StringLength(1)>
        Public Property WORD_DIV As String

        ''' <summary>
        ''' 文言コード
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=3)>
        <StringLength(6)>
        Public Property WORD_CD As String

        ''' <summary>
        ''' 文言
        ''' </summary>
        ''' <returns></returns>
        <StringLength(2048)>
        Public Property WORDING_EN As String

        ''' <summary>
        ''' 文言
        ''' </summary>
        ''' <returns></returns>
        <StringLength(2048)>
        Public Property WORDING_JA As String

        ''' <summary>
        ''' 文言
        ''' </summary>
        ''' <returns></returns>
        <StringLength(2048)>
        Public Property WORDING_LU As String

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
        ''' 検索条件 利用ツール
        ''' </summary>
        ''' <returns>検索条件 利用ツール</returns>
        <NotMapped>
        Public Property SearchToolDiv As String = String.Empty

        ''' <summary>
        ''' 検索条件 文言区分
        ''' </summary>
        ''' <returns>検索条件 文言区分</returns>
        <NotMapped>
        Public Property SearchWordDiv As String = String.Empty

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' 文言マスタ情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

    End Class

End Namespace
