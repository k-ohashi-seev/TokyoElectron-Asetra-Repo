Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    Public Class ConstructionListTempListViewModel
        <Key>
        Public Property TEMP_ID As Long

        Public Property TEMP_DT As Integer

        Public Property WORK_EMPLOYEE_NAME As String

        Public Property CONST_CONTENTS As String

        Public Property TEMP_INFO1 As String

        Public Property TEMP_INFO2 As String

        Public Property TEMP_INFO3 As String

        Public Property TEMP_INFO4 As String

        Public Property TEMP_INFO5 As String

        Public Property TEMP_INFO6 As String

        Public Property TEMP_INFO7 As String

        Public Property TEMP_NUM As Integer? = 0
    End Class
End Namespace