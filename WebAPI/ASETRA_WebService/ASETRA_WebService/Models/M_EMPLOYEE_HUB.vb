Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class M_EMPLOYEE_HUB
    <Key>
    <Column(Order:=0)>
    <StringLength(15)>
    Public Property EMPLOYEE_CD As String

    <Key>
    <Column(Order:=2)>
    <Required>
    <StringLength(10)>
    Public Property HUB_LOC_L_CD As String

    <Key>
    <Column(Order:=3)>
    <StringLength(10)>
    Public Property HUB_LOC_M_CD As String

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
