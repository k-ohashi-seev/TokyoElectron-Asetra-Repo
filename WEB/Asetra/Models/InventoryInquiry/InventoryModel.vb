Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    <Table("T_INVENTORY")>
    Partial Public Class InventoryModel

        <Key>
        Public Property INV_ID As Int64? = 0
        Public Property START_DEC_TS As Int64? = 0
        Public Property START_DEC_EMPLOYEE_CD As String = ""
        Public Property START_DEC_NAME As String = ""
        Public Property INV_NAME As String = ""
        Public Property INV_CONFIRM_TS As Int64? = 0
        Public Property INV_CONFIRM_EMPLOYEE_CD As String = ""
        Public Property INV_CONFIRM_NAME As String = ""

        Public Property INV_ALL_CONFIRM_TS As Int64? = 0
        Public Property UPD_DATETIME As DateTime?
        Public Property UPD_EMPLOYEE_CD As String = ""
        Public Property UPD_EMPLOYEE_NAME As String = ""
        Public Property UPD_TASK As String = ""

        ''' <summary>
        ''' ç≈èIìoò^çXêVì˙éû ï∂éöóÒ
        ''' </summary>
        ''' <returns>ç≈èIìoò^çXêVì˙éû ï∂éöóÒ</returns>
        <NotMapped>
        Public Property UpdDatetimeString As String = String.Empty

        '20231018 UPD Start Shirakawa
        <NotMapped>
        Public Property LOGIC_INV_NUM As Int64? = 0
        <NotMapped>
        Public Property PHYINV_NUM As Int64? = 0
        <NotMapped>
        Public Property LOC_DIFF_NUM As Int64? = 0
        <NotMapped>
        Public Property NO_ITEM_NUM As Int64? = 0
        '20231018 UPD End Shirakawa

    End Class

End Namespace
