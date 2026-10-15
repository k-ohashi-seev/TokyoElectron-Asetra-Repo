Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models

    ''' <summary>
    ''' 資産ラベル発行用モデル
    ''' </summary>
    <Table("T_IN_ASSET")>
    Partial Public Class AssetLabelPrintModel
        <Key>
        <StringLength(24)>
        Public Property ASSET_NO As String

        <StringLength(24)>
        Public Property SUB_ASSET_NO As String

        <StringLength(60)>
        Public Property ASSET_NAME As String

        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String

        <StringLength(10)>
        Public Property LOC_L_CD As String

        <StringLength(10)>
        Public Property LOC_M_CD As String

        <StringLength(10)>
        Public Property LOC_S_CD As String

        <StringLength(8)>
        Public Property CAT_L_CD As String

        Public Property ASSET_STATUS As Byte = 0

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

        <StringLength(1)>
        Public Property IS_TANGIBLE As String

        Public Property IP_DATETIME As Long?

        <StringLength(15)>
        Public Property IP_EMPLOYEE_CD As String

        <StringLength(255)>
        Public Property IP_FILE_NAME As String

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
