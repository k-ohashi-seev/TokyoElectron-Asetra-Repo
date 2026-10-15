Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class T_ASSET_LEASE
    <Key>
    <StringLength(24)>
    Public Property ASSET_NO As String

    <StringLength(15)>
    Public Property LEASE_PLACE_CD As String

    <StringLength(60)>
    Public Property LEASE_PLACE_NAME As String

    <StringLength(15)>
    Public Property LEASE_EMPLOYEE_CD As String

    <StringLength(60)>
    Public Property LEASE_EMPLOYEE_NAME As String

    Public Property LEASE_DUE_DT As Integer?

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
