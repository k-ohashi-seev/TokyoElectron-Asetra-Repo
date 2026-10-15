Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' カテゴリーマスタ(ドロップダウン用)
    ''' </summary>
    Partial Public Class CategoryDropdownModel

        <Key>
        <StringLength(8)>
        Public Property CAT_CD As String

        <StringLength(60)>
        Public Property CAT_NAME As String

    End Class
End Namespace
