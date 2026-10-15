Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' EPC領域サイズドロップダウン用
    ''' </summary>
    Partial Public Class EpcSizeDropdownModel
        <Key>
        Public Property Cd As Integer

        Public Property Name As String
    End Class
End Namespace
