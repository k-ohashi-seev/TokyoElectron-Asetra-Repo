Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

''' <summary>
''' ÉÅÅ[ÉãópÉÇÉfÉã
''' </summary>
<Table("T_MAIL")>
Partial Public Class MailModel
    <Key>
    <DatabaseGenerated(DatabaseGeneratedOption.Identity)>
    Public Property MAIL_ID As Long

    Public Property MAIL_DIV As Integer

    <Required>
    <StringLength(200)>
    Public Property MAIL_SUBJECT As String

    <Required>
    <StringLength(2000)>
    Public Property MAIL_BODY As String

    <Required>
    <StringLength(1)>
    Public Property MAIL_STATUS As String

    Public Property MAIL_RETRY_CNT As Integer

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
