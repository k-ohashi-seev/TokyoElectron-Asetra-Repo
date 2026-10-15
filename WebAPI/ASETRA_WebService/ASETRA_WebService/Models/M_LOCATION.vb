Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class M_LOCATION
    <Key>
    <Column(Order:=0)>
    <StringLength(1)>
    Public Property LOC_DIV As String

    <Key>
    <Column(Order:=1)>
    <StringLength(5)>
    Public Property LOC_L_CD As String

    <Key>
    <Column(Order:=2)>
    <StringLength(5)>
    Public Property LOC_M_CD As String

    <Key>
    <Column(Order:=3)>
    <StringLength(5)>
    Public Property LOC_S_CD As String

    <Required>
    <StringLength(60)>
    Public Property LOC_NAME As String

    <Required>
    <StringLength(1)>
    Public Property LOC_RFID_FLG As String

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
