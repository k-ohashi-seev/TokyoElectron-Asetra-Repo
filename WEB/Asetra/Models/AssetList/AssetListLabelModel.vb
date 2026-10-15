Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 資産一覧タグ出力用モデル
    ''' </summary>
    Public Class AssetListLabelModel
        <Key>
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

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
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

        '2023/02/08 Kawamoto ADD START
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
        '2023/02/08 Kawamoto END START

        Public Property ASSET_STATUS As Byte?

        Public Property TAG_LINK_STATUS As Byte?

        Public Property LOC_L_NAME As String

        Public Property LOC_M_NAME As String

        Public Property LOC_S_NAME As String

        Public Property CAT_L_NAME As String

        Public Property CAT_M_NAME As String

        Public Property CAT_S_NAME As String

        Public Property MNG_LOC_L_CD As String

        Public Property MNG_LOC_L_NAME As String
    End Class
End Namespace
