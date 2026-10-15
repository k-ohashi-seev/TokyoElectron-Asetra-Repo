Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 履歴照会(資産)用表示用モデル
    ''' </summary>
    Public Class AssetDetailViewModel
        Inherits BaseModel

        <Key>
        Public Property PROCESS_LOG_ID As Long

        Public Property PROCESS_DT As Integer

        Public Property PROCESS_TM As String

        <StringLength(24)>
        Public Property ASSET_NO As String

        <StringLength(24)>
        Public Property SUB_ASSET_NO As String

        <StringLength(128)>
        Public Property ASSET_ID As String

        <StringLength(1)>
        Public Property PROCESS_TOOL_DIV As String

        <StringLength(2)>
        Public Property PROCESS_DIV As String

        <StringLength(15)>
        Public Property LEASE_PLACE_CD As String

        <StringLength(60)>
        Public Property LEASE_PLACE_NAME As String

        <StringLength(15)>
        Public Property LEASE_EMPLOYEE_CD As String

        <StringLength(255)>
        Public Property REMARK As String    '2019/11/19 nv miyaji add

        <StringLength(60)>
        Public Property LEASE_EMPLOYEE_NAME As String

        Public Property LEASE_DT As Integer?  '2020/03/06 nv miyaji add

        Public Property LEASE_DUE_DT As Integer?

        Public Property LEASE_STATUS As Byte?    '2021/10/06 nv miyaji add

        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String      '2021/10/06 nv miyaji add

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String

        <StringLength(60)>
        Public Property MNG_LOC_L_NAME As String      '2021/10/06 nv miyaji add

        <StringLength(60)>
        Public Property LOC_L_NAME As String

        <StringLength(60)>
        Public Property LOC_M_NAME As String

        <StringLength(60)>
        Public Property LOC_S_NAME As String

        Public Property ASSET_NAME As String

        Public Property MAKER As String

        Public Property PRODUCT_NO As String

        Public Property MODEL_NO As String

        '2023/03/22 Kawamoto ADD START
        Public Property DUE_QTY As Decimal?

        Public Property WORKER_EMPLOYEE_CD As String

        Public Property WORKER_EMPLOYEE_NAME As String

        Public Property WORKER_MINUTE As Int16?

        Public Property WORKER_OVERVIEW As String

        Public Property WORKER_REMARK1 As String

        Public Property WORKER_REMARK2 As String
        '2023/03/22 Kawamoto ADD END

        Public Property REMARK1 As String

        Public Property REMARK2 As String

        Public Property REMARK3 As String

        Public Property REMARK4 As String

        Public Property REMARK5 As String

        Public Property REMARK6 As String

        Public Property REMARK7 As String

        Public Property REMARK8 As String

        Public Property REMARK9 As String

        Public Property REMARK10 As String

        Public Property TASK_RESULT As String

        Public Property UPD_DATETIME As Date

        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String

        <StringLength(255)>
        Public Property UPD_TASK As String

        Public Property ASSET_STATUS As Byte = 0

        Public Property EPC_ENC_STATUS As Byte = 0

        Public Property TAG_LINK_STATUS As Byte = 0

        Public Property CAT_L_CD As String = String.Empty

        Public Property CAT_M_CD As String = String.Empty

        Public Property CAT_S_CD As String = String.Empty

        Public Property CAT_L_NAME As String = String.Empty

        Public Property CAT_M_NAME As String = String.Empty

        Public Property CAT_S_NAME As String = String.Empty

        Public Property LEASE_CNT As Integer = 0


        ''' <summary>
        '''  資産マスタデータ
        ''' </summary>
        ''' <returns>資産マスタデータ</returns>
        <NotMapped>
        Public Property mstAssetData As List(Of MstAssetModel)

        ''' <summary>
        ''' 固定列(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntTop As Integer

        ''' <summary>
        ''' 資産履歴照会情報(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoTop As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' 固定列(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntBottom As Integer

        ''' <summary>
        ''' 資産履歴照会情報(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoBottom As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
