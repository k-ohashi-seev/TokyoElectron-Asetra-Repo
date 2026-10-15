Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class M_CATEGORY
    <Key>
    <Column(Order:=0)>
    <StringLength(1)>
    Public Property CAT_DIV As String

    <Key>
    <Column(Order:=1)>
    <StringLength(3)>
    Public Property CAT_L_CD As String

    <Key>
    <Column(Order:=2)>
    <StringLength(3)>
    Public Property CAT_M_CD As String

    <Key>
    <Column(Order:=3)>
    <StringLength(3)>
    Public Property CAT_S_CD As String

    <Required>
    <StringLength(60)>
    Public Property CAT_NAME As String

    <StringLength(1)>
    Public Property LEASE_LIMIT_ALERT_DIV As String

    Public Property LEASE_LIMIT_BEF_DAYS As Integer?

    <StringLength(1)>
    Public Property GENERIC_LIMIT_ALERT_DIV As String

    Public Property GENERIC_LIMIT_BEF_DAYS As Integer?

    Public Property ASSET_ID_REG_DIV As Byte?

    Public Property EPC_SIZE As Integer?

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
