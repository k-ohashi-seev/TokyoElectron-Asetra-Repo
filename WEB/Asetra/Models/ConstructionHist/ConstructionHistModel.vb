Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models

    ''' <summary>
    ''' 工事履歴用モデル
    ''' </summary>
    <Table("T_CONST_LOG")>
    Partial Public Class ConstructionHistModel
        <Key>
        <StringLength(20)>
        <Column(Order:=0)>
        Public Property CONST_NO As String

        <Key>
        <Column(Order:=1)>
        Public Property CONST_WORK_TS As Long

        <Key>
        <Column(Order:=2)>
        <StringLength(2)>
        Public Property CONST_WORK_DIV As String

        <StringLength(1)>
        Public Property CONST_MATCH_DIV As String

        Public Property LENT_NUM As Integer

        Public Property LEASE_NUM As Integer

        Public Property TEMP_REG_NUM As Integer

        Public Property CONST_IN_MATCH_NUM As Integer

        Public Property CONST_OUT_MATCH_NUM As Integer

        Public Property CONST_IN_NUM As Integer

        Public Property TEMP_NUM As Integer

        Public Property CONST_OUT_NUM As Integer

        Public Property INS_DATETIME As Date

        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String

        Public Property UPD_DATETIME As Date

        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String

        <StringLength(255)>
        Public Property UPD_TASK As String
    End Class
End Namespace
