Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    <Table("T_INV_ASSET")>
    Partial Public Class InventoryInquiryModel

        <Key>
        <Column(Order:=0)>
        Public Property INV_ID As Long?

        <Key>
        <Column(Order:=1)>
        <StringLength(10)>
        Public Property INV_LOC_L_CD As String

        <Key>
        <Column(Order:=2)>
        <StringLength(24)>
        Public Property ASSET_NO As String

        <StringLength(128)>
        Public Property ASSET_ID As String

        <StringLength(24)>
        Public Property SUB_ASSET_NO As String

        Public Property EPC_SIZE As Integer?

        '<StringLength(64)>  UPD 2022/8/1
        <StringLength(64)>
        Public Property GEN_ASSET_ID As String

        <StringLength(60)>
        Public Property ASSET_DIV As String

        <StringLength(60)>
        Public Property ASSET_NAME As String

        <StringLength(10)>
        Public Property LOC_L_CD As String

        <StringLength(10)>
        Public Property LOC_M_CD As String

        <StringLength(10)>
        Public Property LOC_S_CD As String

        <StringLength(8)>
        Public Property CAT_L_CD As String

        <StringLength(8)>
        Public Property CAT_M_CD As String

        <StringLength(8)>
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
        Public Property FREE_ITEM11 As String
        <StringLength(255)>
        Public Property FREE_ITEM12 As String
        <StringLength(255)>
        Public Property FREE_ITEM13 As String
        <StringLength(255)>
        Public Property FREE_ITEM14 As String
        <StringLength(255)>
        Public Property FREE_ITEM15 As String
        <StringLength(255)>
        Public Property FREE_ITEM16 As String
        <StringLength(255)>
        Public Property FREE_ITEM17 As String
        <StringLength(255)>
        Public Property FREE_ITEM18 As String
        <StringLength(255)>
        Public Property FREE_ITEM19 As String
        <StringLength(255)>
        Public Property FREE_ITEM20 As String
        <StringLength(255)>
        Public Property FREE_ITEM21 As String
        <StringLength(255)>
        Public Property FREE_ITEM22 As String
        <StringLength(255)>
        Public Property FREE_ITEM23 As String
        <StringLength(255)>
        Public Property FREE_ITEM24 As String
        <StringLength(255)>
        Public Property FREE_ITEM25 As String
        <StringLength(255)>
        Public Property FREE_ITEM26 As String
        <StringLength(255)>
        Public Property FREE_ITEM27 As String
        <StringLength(255)>
        Public Property FREE_ITEM28 As String
        <StringLength(255)>
        Public Property FREE_ITEM29 As String
        <StringLength(255)>
        Public Property FREE_ITEM30 As String
        <StringLength(255)>
        Public Property FREE_ITEM31 As String
        <StringLength(255)>
        Public Property FREE_ITEM32 As String
        <StringLength(255)>
        Public Property FREE_ITEM33 As String
        <StringLength(255)>
        Public Property FREE_ITEM34 As String
        <StringLength(255)>
        Public Property FREE_ITEM35 As String
        <StringLength(255)>
        Public Property FREE_ITEM36 As String
        <StringLength(255)>
        Public Property FREE_ITEM37 As String
        <StringLength(255)>
        Public Property FREE_ITEM38 As String
        <StringLength(255)>
        Public Property FREE_ITEM39 As String
        <StringLength(255)>
        Public Property FREE_ITEM40 As String
        <StringLength(255)>
        Public Property FREE_ITEM41 As String
        <StringLength(255)>
        Public Property FREE_ITEM42 As String
        <StringLength(255)>
        Public Property FREE_ITEM43 As String
        <StringLength(255)>
        Public Property FREE_ITEM44 As String
        <StringLength(255)>
        Public Property FREE_ITEM45 As String
        <StringLength(255)>
        Public Property FREE_ITEM46 As String
        <StringLength(255)>
        Public Property FREE_ITEM47 As String
        <StringLength(255)>
        Public Property FREE_ITEM48 As String
        <StringLength(255)>
        Public Property FREE_ITEM49 As String
        <StringLength(255)>
        Public Property FREE_ITEM50 As String
        <StringLength(255)>
        Public Property FREE_ITEM51 As String
        <StringLength(255)>
        Public Property FREE_ITEM52 As String
        <StringLength(255)>
        Public Property FREE_ITEM53 As String
        <StringLength(255)>
        Public Property FREE_ITEM54 As String
        <StringLength(255)>
        Public Property FREE_ITEM55 As String
        <StringLength(255)>
        Public Property FREE_ITEM56 As String
        <StringLength(255)>
        Public Property FREE_ITEM57 As String
        <StringLength(255)>
        Public Property FREE_ITEM58 As String
        <StringLength(255)>
        Public Property FREE_ITEM59 As String
        <StringLength(255)>
        Public Property FREE_ITEM60 As String

        <StringLength(255)>
        Public Property ASSET_PIC_FILENAME As String

        Public Property LEASE_CNT As Integer

        Public Property ASSET_STATUS As Byte

        Public Property EPC_ENC_STATUS As Byte

        Public Property TAG_LINK_STATUS As Byte

        Public Property LEASE_STATUS As Byte

        <StringLength(15)>
        Public Property LEASE_PLACE_CD As String

        <StringLength(60)>
        Public Property LEASE_PLACE_NAME As String

        <StringLength(255)>
        Public Property REMARK As String

        <StringLength(15)>
        Public Property LEASE_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property LEASE_EMPLOYEE_NAME As String

        Public Property LEASE_DT As Integer?

        Public Property LEASE_DUE_DT As Integer?

        Public Property MAINTE_LEASE_NUM As Integer?

        Public Property MAINTE_LEASE_DAY As Integer?

        Public Property MAINTE_LEASE_DAY_WORK_FLAG As Char

        Public Property MAINTE_DAY As Integer?

        Public Property MAINTE_DAY_WORK_FLAG As Char?

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

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property PHYINV_LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property PHYINV_LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
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


        <StringLength(30)>
        Public Property OPE_STATUS As String
        <StringLength(30)>
        Public Property DIVERSION_REMOVE As String
        <StringLength(30)>
        Public Property REMOVE_DATE As String
        <StringLength(30)>
        Public Property REMOVE_REASON As String
        <StringLength(255)>
        Public Property REMOVE_CONTENT As String
        <StringLength(10)>
        Public Property CHANGE_LOC_M_CD As String
        <StringLength(10)>
        Public Property CHANGE_LOC_S_CD As String
        <StringLength(15)>
        Public Property CHANGE_EMPLOYEE As String
        <StringLength(1)>
        Public Property IS_TANGIBLE As Char?
        <StringLength(255)>
        Public Property REMARK1 As String
        <StringLength(255)>
        Public Property REMARK2 As String
        <StringLength(255)>
        Public Property REMARK3 As String
        <StringLength(255)>
        Public Property REMARK4 As String
        <StringLength(255)>
        Public Property REMARK5 As String
        <StringLength(255)>
        Public Property REMARK6 As String
        <StringLength(255)>
        Public Property REMARK7 As String
        <StringLength(255)>
        Public Property REMARK8 As String
        <StringLength(255)>
        Public Property REMARK9 As String
        <StringLength(255)>
        Public Property REMARK10 As String
        <StringLength(15)>
        Public Property LOCK_EMPLOYEE_CD As String


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
