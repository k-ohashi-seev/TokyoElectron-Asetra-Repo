Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class T_PROCESS_LOG
    <Key>
    Public Property PROCESS_LOG_ID As Long

    Public Property PROCESS_DT As Integer

    <Required>
    <StringLength(9)>
    Public Property PROCESS_TM As String

    <StringLength(24)>
    Public Property ASSET_NO As String

    <StringLength(128)>
    Public Property ASSET_ID As String

    <Required>
    <StringLength(1)>
    Public Property PROCESS_TOOL_DIV As String

    <Required>
    <StringLength(2)>
    Public Property PROCESS_DIV As String

    <StringLength(15)>
    Public Property LEASE_PLACE_CD As String

    <StringLength(60)>
    Public Property LEASE_PLACE_NAME As String

    <StringLength(15)>
    Public Property LEASE_EMPLOYEE_CD As String

    <StringLength(60)>
    Public Property LEASE_EMPLOYEE_NAME As String

    Public Property LEASE_DUE_DT As Integer?

    <StringLength(5)>
    Public Property LOC_L_CD As String

    <StringLength(5)>
    Public Property LOC_M_CD As String

    <StringLength(5)>
    Public Property LOC_S_CD As String

    <StringLength(60)>
    Public Property LOC_L_NAME As String

    <StringLength(60)>
    Public Property LOC_M_NAME As String

    <StringLength(60)>
    Public Property LOC_S_NAME As String

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
