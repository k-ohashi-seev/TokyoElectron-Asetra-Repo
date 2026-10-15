Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace Models

    Public Class ReserveMainteInfoSF
        <Key>
        Public Property MAINTE_EXEC_FLG As String = String.Empty
        Public Property MAINTE_DIV As String = String.Empty
        Public Property ST_DT As Integer? = 0
        Public Property END_DT As Integer? = 0
        Public Property LIMIT_DT As Integer = 0
    End Class
End Namespace
