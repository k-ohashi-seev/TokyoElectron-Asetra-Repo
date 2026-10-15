Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class T_INVENTORY
    <Key>
    <DatabaseGenerated(DatabaseGeneratedOption.None)>
    Public Property START_DEC_TS As Long

    <Required>
    <StringLength(15)>
    Public Property START_DEC_EMPLOYEE_CD As String

    <Required>
    <StringLength(60)>
    Public Property START_DEC_NAME As String

    <StringLength(60)>
    Public Property INV_NAME As String

    Public Property INV_CONFIRM_TS As Long?

    <StringLength(15)>
    Public Property INV_CONFIRM_EMPLOYEE_CD As String

    <StringLength(60)>
    Public Property INV_CONFIRM_NAME As String

    Public Property LOGIC_INV_NUM As Long

    Public Property PHYINV_NUM As Long

    Public Property LOC_DIFF_NUM As Long

    Public Property NO_ITEM_NUM As Long

    Public Property INV_ALL_CONFIRM_TS As Long?

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
