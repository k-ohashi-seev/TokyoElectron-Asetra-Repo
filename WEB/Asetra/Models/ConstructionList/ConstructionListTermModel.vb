Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    Public Class ConstructionListTermModel
        <Key>
        Public Property CONST_NO As String

        Public Property CONST_LENT_START_DT As Integer

        Public Property CONST_LENT_END_DT As Integer

    End Class
End Namespace
