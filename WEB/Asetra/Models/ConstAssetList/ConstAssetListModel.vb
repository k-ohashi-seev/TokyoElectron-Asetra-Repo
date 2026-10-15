Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models

    ''' <summary>
    ''' 工事履歴用モデル
    ''' </summary>
    <Table("T_CONST_ASSET")>
    Partial Public Class ConstAssetListModel
        <Key>
        <StringLength(20)>
        <Column(Order:=0)>
        Public Property CONST_NO As String

        <Key>
        <StringLength(24)>
        <Column(Order:=1)>
        Public Property ASSET_NO As String

        <StringLength(1)>
        Public Property CONST_ASSET_STATUS As String

        Public Property TEMP_ID As Long?

        <StringLength(1)>
        Public Property PREV_TEMP_FLAG As String

        Public Property TASK_TS As Long?

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
