Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' 商品マスタ
    ''' </summary>
    <Table("M_GOODS")>
    Partial Public Class GoodsModel
        <Key>
        <Required>
        <StringLength(30)>
        Public Property PRODUCT_NO As String

        <StringLength(60)>
        Public Property ASSET_DIV As String

        <StringLength(60)>
        Public Property ASSET_NAME As String

        '<Required>
        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String

        '<Required>
        <StringLength(10)>
        Public Property LOC_L_CD As String

        <StringLength(10)>
        Public Property LOC_M_CD As String

        <StringLength(10)>
        Public Property LOC_S_CD As String

        '<Required>
        <StringLength(3)>
        Public Property CAT_L_CD As String

        <StringLength(3)>
        Public Property CAT_M_CD As String

        <StringLength(3)>
        Public Property CAT_S_CD As String

        <StringLength(60)>
        Public Property MAKER As String

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

        '2023/03/22 Kawamoto ADD START
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
        '2023/03/22 Kawamoto ADD END

        <StringLength(255)>
        Public Property ASSET_PIC_FILENAME As String

        Public Property TASK_TS As Long?

        <DataType(DataType.Date)>
        <Required>
        Public Property INS_DATETIME As Date

        <StringLength(15)>
        <Required>
        Public Property INS_EMPLOYEE_CD As String

        <StringLength(60)>
        <Required>
        Public Property INS_EMPLOYEE_NAME As String

        <DataType(DataType.Date)>
        <Required>
        Public Property UPD_DATETIME As Date

        <StringLength(15)>
        <Required>
        Public Property UPD_EMPLOYEE_CD As String

        <StringLength(60)>
        <Required>
        Public Property UPD_EMPLOYEE_NAME As String

        <StringLength(255)>
        <Required>
        Public Property UPD_TASK As String

    End Class

End Namespace
