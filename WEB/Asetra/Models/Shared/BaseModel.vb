Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' 規定モデルクラス
    ''' </summary>
    Partial Public Class BaseModel

        ''' <summary>
        ''' 一覧表示の総件数
        ''' </summary>
        ''' <returns></returns>
        Public Property TotalCount As Integer = 0

        ''' <summary>
        ''' ページングクラス
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property Pager As Pager = New Pager()

        ''' <summary>
        ''' ページングの番号
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property PageNum As Integer = 1

        ''' <summary>
        ''' 登録ボタン
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property OnCreteButton As String = String.Empty

        ''' <summary>
        ''' エラーコード
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property ErrCode As String = String.Empty

        ''' <summary>
        ''' エラーターゲット
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property ErrTarget As String = String.Empty

        ''' <summary>
        ''' ステータス
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property States As String = String.Empty

    End Class
End Namespace
