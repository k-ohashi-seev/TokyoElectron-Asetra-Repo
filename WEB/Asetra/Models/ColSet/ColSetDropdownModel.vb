Imports System.ComponentModel.DataAnnotations

Namespace Models
    Public Class ColSetDropdownModel
        ''' <summary>
        ''' 画面ID
        ''' </summary>
        <Key>
        <StringLength(100)>
        Public Property DISP_ID As String

        ''' <summary>
        ''' 項目名
        ''' </summary>
        <Required>
        <StringLength(2048)>
        Public Property COL_NAME As String
    End Class

End Namespace