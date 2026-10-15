Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class T_INV_RESULT
    <Key>
    <Column(Order:=0)>
    <DatabaseGenerated(DatabaseGeneratedOption.None)>
    Public Property START_DEC_TS As Long

    <Key>
    <Column(Order:=1)>
    <StringLength(24)>
    Public Property ASSET_NO As String

    <StringLength(128)>
    Public Property ASSET_ID As String

    Public Property EPC_SIZE As Integer?

    <StringLength(64)>
    Public Property GEN_ASSET_ID As String

    <StringLength(60)>
    Public Property ASSET_DIV As String

    <StringLength(60)>
    Public Property ASSET_NAME As String

    <StringLength(5)>
    Public Property LOC_L_CD As String

    <StringLength(5)>
    Public Property LOC_M_CD As String

    <StringLength(5)>
    Public Property LOC_S_CD As String

    <StringLength(3)>
    Public Property CAT_L_CD As String

    <StringLength(3)>
    Public Property CAT_M_CD As String

    <StringLength(3)>
    Public Property CAT_S_CD As String

    <StringLength(60)>
    Public Property MAKER As String

    <StringLength(30)>
    Public Property PRODUCT_NO As String

    <StringLength(30)>
    Public Property MODEL_NO As String

    <StringLength(30)>
    Public Property LEASE_NO As String

    Public Property LEASE_LIMIT As Integer?

    Public Property GENERIC_LIMIT As Integer?

    <StringLength(255)>
    Public Property FREE_ITEM1 As String

    <StringLength(255)>
    Public Property FREE_ITEM2 As String

    <StringLength(255)>
    Public Property FREE_ITEM3 As String

    <StringLength(255)>
    Public Property FREE_ITEM4 As String

    <StringLength(255)>
    Public Property FREE_ITEM5 As String

    <StringLength(255)>
    Public Property FREE_ITEM6 As String

    <StringLength(255)>
    Public Property FREE_ITEM7 As String

    <StringLength(255)>
    Public Property FREE_ITEM8 As String

    <StringLength(255)>
    Public Property FREE_ITEM9 As String

    <StringLength(255)>
    Public Property FREE_ITEM10 As String

    <StringLength(255)>
    Public Property ASSET_PIC_FILENAME As String

    Public Property LEASE_CNT As Integer

    Public Property ASSET_STATUS As Byte

    Public Property EPC_ENC_STATUS As Byte

    Public Property TAG_LINK_STATUS As Byte

    Public Property RETIRE_TS As Long?

    <StringLength(15)>
    Public Property RETIRE_EMPLOYEE_CD As String

    <StringLength(60)>
    Public Property RETIRE_EMPLOYEE_NAME As String

    <StringLength(128)>
    Public Property RETIRE_ASSET_ID As String

    <StringLength(60)>
    Public Property LOC_L_NAME As String

    <StringLength(60)>
    Public Property LOC_M_NAME As String

    <StringLength(60)>
    Public Property LOC_S_NAME As String

    <StringLength(60)>
    Public Property CAT_L_NAME As String

    <StringLength(60)>
    Public Property CAT_M_NAME As String

    <StringLength(60)>
    Public Property CAT_S_NAME As String

    Public Property PHYINV_TS As Long?

    <StringLength(1)>
    Public Property PHYINV_STATUS As String

    <StringLength(5)>
    Public Property PHYINV_LOC_L_CD As String

    <StringLength(5)>
    Public Property PHYINV_LOC_M_CD As String

    <StringLength(5)>
    Public Property PHYINV_LOC_S_CD As String

    <StringLength(60)>
    Public Property PHYINV_LOC_L_NAME As String

    <StringLength(60)>
    Public Property PHYINV_LOC_M_NAME As String

    <StringLength(60)>
    Public Property PHYINV_LOC_S_NAME As String

    <StringLength(15)>
    Public Property PHYINV_EMPLOYEE_CD As String

    <StringLength(60)>
    Public Property PHYINV_EMPLOYEE_NAME As String

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
