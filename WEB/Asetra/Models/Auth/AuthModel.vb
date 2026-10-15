Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' å†å¿É}ÉXÉ^
    ''' </summary>
    <Table("M_AUTH")>
    Partial Public Class AuthModel
        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property PROCESS_TOOL_DIV As String

        <Key>
        <Column(Order:=1)>
        <StringLength(50)>
        Public Property FUNCTION_ID As String

        <Key>
        <Column(Order:=2)>
        <StringLength(1)>
        Public Property AUTH_DIV As String

        <Key>
        <Column(Order:=3)>
        <StringLength(1)>
        Public Property AUTH_TYPE As String

        <Key>
        <Column(Order:=4)>
        <StringLength(1)>
        Public Property HEAD_OFFICE_FLG As String

        <StringLength(1)>
        Public Property READ_AUTH_FLG As String

        <StringLength(1)>
        Public Property UPD_AUTH_FLG As String

        <StringLength(1)>
        Public Property READ_HUB_AUTH_FLG As String

        <StringLength(1)>
        Public Property UPD_HUB_AUTH_FLG As String

        <StringLength(50)>
        Public Property AUTH_OPTION As String

        <StringLength(50)>
        Public Property MENU_ID As String

        Public Property INS_DATETIME As Date

        <Required>
        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String

        <Required>
        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String

        Public Property UPD_DATETIME As Date

        <Required>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String

        <Required>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String

        <Required>
        <StringLength(255)>
        Public Property UPD_TASK As String
    End Class
End Namespace
