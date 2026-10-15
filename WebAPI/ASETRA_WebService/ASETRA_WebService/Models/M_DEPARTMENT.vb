Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class M_DEPARTMENT
    <Key>
    <StringLength(15)>
    Public Property DEPARTMENT_CD As String

    <StringLength(60)>
    Public Property DEPARTMENT_NAME As String

    <StringLength(1)>
    Public Property HEAD_OFFICE_FLG As String

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
