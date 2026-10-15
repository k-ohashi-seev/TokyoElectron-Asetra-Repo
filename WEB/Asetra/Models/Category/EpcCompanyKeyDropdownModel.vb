Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' EPC事業所ドロップダウン用
    ''' </summary>
    Partial Public Class EpcCompanyKeyDropdownModel
        <Key>
        Public Property Cd As Integer

        Public Property Name As String
    End Class
End Namespace
