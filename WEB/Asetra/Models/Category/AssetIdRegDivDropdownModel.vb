Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 資産ID登録方法ドロップダウン用
    ''' </summary>
    Partial Public Class AssetIdRegDivDropdownModel
        <Key>
        Public Property Cd As Integer

        Public Property Name As String
    End Class
End Namespace
