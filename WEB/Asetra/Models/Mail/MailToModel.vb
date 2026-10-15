Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

''' <summary>
''' ÉÅÅ[Éãà∂êÊópÉÇÉfÉã
''' </summary>
<Table("T_MAIL_TO")>
Partial Public Class MailToModel
    <Key>
    <Column(Order:=0)>
    <DatabaseGenerated(DatabaseGeneratedOption.None)>
    Public Property MAIL_ID As Long

    <Key>
    <Column(Order:=1)>
    <StringLength(3)>
    Public Property MAIL_SEND_DIV As String

    <Key>
    <Column(Order:=2)>
    <StringLength(50)>
    Public Property MAIL_ADDRESS As String

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
