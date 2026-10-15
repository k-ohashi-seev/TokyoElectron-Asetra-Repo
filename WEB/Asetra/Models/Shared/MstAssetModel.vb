Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 資産マスタ取得用モデルクラス
    ''' </summary>
    Public Class MstAssetModel

        <Key>
        Public Property ASSET_NO As String = String.Empty
        Public Property SUB_ASSET_NO As String = String.Empty

        Public Property ASSET_ID As String = String.Empty

        Public Property EPC_SIZE As Integer? = Nothing

        Public Property GEN_ASSET_ID As String = String.Empty

        Public Property ASSET_DIV As String = String.Empty

        Public Property ASSET_NAME As String = String.Empty

        Public Property MNG_LOC_L_CD As String '2021/10/06 nv miyaji add

        Public Property LOC_L_CD As String = String.Empty

        Public Property LOC_M_CD As String = String.Empty

        Public Property LOC_S_CD As String = String.Empty

        Public Property CAT_L_CD As String = String.Empty

        Public Property CAT_M_CD As String = String.Empty

        Public Property CAT_S_CD As String = String.Empty

        Public Property MAKER As String = String.Empty

        Public Property PRODUCT_NO As String = String.Empty

        Public Property MODEL_NO As String = String.Empty

        Public Property LEASE_NO As String = String.Empty

        Public Property LEASE_LIMIT As Integer? = Nothing

        Public Property GENERIC_LIMIT As Integer? = Nothing

        Public Property FREE_ITEM1 As String = String.Empty

        Public Property FREE_ITEM2 As String = String.Empty

        Public Property FREE_ITEM3 As String = String.Empty

        Public Property FREE_ITEM4 As String = String.Empty

        Public Property FREE_ITEM5 As String = String.Empty

        Public Property FREE_ITEM6 As String = String.Empty

        Public Property FREE_ITEM7 As String = String.Empty

        Public Property FREE_ITEM8 As String = String.Empty

        Public Property FREE_ITEM9 As String = String.Empty

        Public Property FREE_ITEM10 As String = String.Empty

        Public Property ASSET_PIC_FILENAME As String = String.Empty

        Public Property LEASE_CNT As Integer = 0

        Public Property ASSET_STATUS As Byte = 0

        Public Property EPC_ENC_STATUS As Byte = 0

        Public Property TAG_LINK_STATUS As Byte = 0

        Public Property LEASE_STATUS As Byte = 0 '2021/10/06 nv miyaji add

        Public Property LEASE_PLACE_CD As String = String.Empty

        Public Property LEASE_PLACE_NAME As String = String.Empty

        Public Property REMARK As String    '2019/11/19 nv miyaji add

        Public Property LEASE_EMPLOYEE_CD As String = String.Empty

        Public Property LEASE_EMPLOYEE_NAME As String = String.Empty

        Public Property LEASE_DT As Integer? = Nothing

        Public Property LEASE_DUE_DT As Integer? = Nothing

        '--2021/10/06 nv miyaji add start
        Public Property MAINTE_LEASE_NUM As Integer? = Nothing

        Public Property MAINTE_LEASE_DAY As Integer? = Nothing

        <StringLength(1)>
        Public Property MAINTE_LEASE_DAY_WORK_FLAG As String = String.Empty

        Public Property MAINTE_DAY As Integer? = Nothing

        <StringLength(1)>
        Public Property MAINTE_DAY_WORK_FLAG As String = String.Empty
        '--2021/10/06 nv miyaji add end

        Public Property RETIRE_TS As Long? = Nothing

        Public Property RETIRE_EMPLOYEE_CD As String = String.Empty

        Public Property RETIRE_EMPLOYEE_NAME As String = String.Empty

        Public Property RETIRE_ASSET_ID As String = String.Empty

        Public Property PHYINV_TS As Long? = Nothing

        Public Property PHYINV_STATUS As String = String.Empty

        Public Property PHYINV_LOC_L_CD As String = String.Empty

        Public Property PHYINV_LOC_M_CD As String = String.Empty

        Public Property PHYINV_LOC_S_CD As String = String.Empty

        Public Property PHYINV_LOC_L_NAME As String = String.Empty

        Public Property PHYINV_LOC_M_NAME As String = String.Empty

        Public Property PHYINV_LOC_S_NAME As String = String.Empty

        Public Property PHYINV_EMPLOYEE_CD As String = String.Empty

        Public Property PHYINV_EMPLOYEE_NAME As String = String.Empty

        Public Property CAT_L_NAME As String = String.Empty

        Public Property CAT_M_NAME As String = String.Empty

        Public Property CAT_S_NAME As String = String.Empty

        Public Property MNG_LOC_L_NAME As String = String.Empty '2021/10/06 nv miyaji add

    End Class
End Namespace
