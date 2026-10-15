Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models
    ''' <summary>
    ''' ó\ñÒê\êøìoò^âÊñ Å@ÉÇÉfÉã
    ''' </summary>
    <Table("T_RESERVE_DETAIL")>
    Partial Public Class ReserveDetailModel
        <Key>
        <Column(Order:=0)>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property RESERVE_ID As Long

        <Key>
        <Column(Order:=1)>
        <StringLength(24)>
        Public Property ASSET_NO As String

        <Key>
        <Column(Order:=2)>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property REAL_START_DT As Integer

        Public Property REAL_END_DT As Integer

        <Required>
        <StringLength(15)>
        Public Property CHARGE_EMP_CD As String

        Public Property LENDING_DT As Integer?

        Public Property RETURN_DT As Integer?

        Public Property PR_ST_DT As Integer

        Public Property PR_END_DT As Integer

        Public Property LEASE_CHG_TIMES As Integer

        <StringLength(1)>
        Public Property HANDOVER_DIV As String

        Public Property HANDOVER_RESERVE_ID As Long?

        <StringLength(1)>
        Public Property RESERVE_STATUS As String

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
End Namespace

