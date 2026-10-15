Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ロケーションマスタ(ドロップダウン用)
    ''' </summary>
    Partial Public Class LocationDropdownModel

        <Key>
        <StringLength(10)>
        Public Property LOC_CD As String

        <StringLength(60)>
        Public Property LOC_NAME As String

    End Class

End Namespace
