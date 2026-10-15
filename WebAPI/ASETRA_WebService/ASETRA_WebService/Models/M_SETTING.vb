Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class M_SETTING
    <Key>
    <Column(Order:=0)>
    <StringLength(30)>
    Public Property KEY_DIV As String

    <Key>
    <Column(Order:=1)>
    <StringLength(30)>
    Public Property KEY1 As String

    <Key>
    <Column(Order:=2)>
    <StringLength(30)>
    Public Property KEY2 As String

    <Key>
    <Column(Order:=3)>
    <StringLength(30)>
    Public Property KEY3 As String

    <StringLength(255)>
    Public Property REMARK As String

    <StringLength(255)>
    Public Property DATA1 As String

    <StringLength(255)>
    Public Property DATA2 As String

    <StringLength(255)>
    Public Property DATA3 As String

    <StringLength(255)>
    Public Property DATA4 As String

    <StringLength(255)>
    Public Property DATA5 As String

    <StringLength(255)>
    Public Property DATA6 As String

    <StringLength(255)>
    Public Property DATA7 As String

    <StringLength(255)>
    Public Property DATA8 As String

    <StringLength(255)>
    Public Property DATA9 As String

    <StringLength(255)>
    Public Property DATA10 As String

    <StringLength(255)>
    Public Property SPECIAL_DATA_DIV As String

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
