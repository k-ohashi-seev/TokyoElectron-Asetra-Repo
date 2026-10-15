Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models

    ''' <summary>
    ''' ・ｽ・ｽ・ｽY・ｽ・ｽ・ｽ・ｽ・ｽﾆ会ｿｽp・ｽ・ｽ・ｽf・ｽ・ｽ
    ''' </summary>
    <Table("T_PROCESS_LOG")>
    Partial Public Class AssetHistoryModel
        <Key>
        Public Property PROCESS_LOG_ID As Long

        Public Property PROCESS_DT As Integer

        Public Property PROCESS_TM As String

        <StringLength(24)>
        Public Property ASSET_NO As String

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

        <StringLength(255)>
        Public Property REMARK As String    '2019/11/19 nv miyaji add

        <StringLength(15)>
        Public Property LEASE_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property LEASE_EMPLOYEE_NAME As String

        Public Property LEASE_DT As Integer?    '2020/03/06 nv miyaji add

        Public Property LEASE_DUE_DT As Integer?

        Public Property LEASE_STATUS As Byte?       '2021/10/04 nv miyaji add

        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String       '2021/10/04 nv miyaji add

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String

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

        <StringLength(15)>
        Public Property WORKER_EMPLOYEE_CD As String

        <StringLength(255)>
        Public Property WORKER_EMPLOYEE_NAME As String

        Public Property WORKER_MINUTE As Int16?

        <StringLength(255)>
        Public Property WORKER_OVERVIEW As String

        <StringLength(255)>
        Public Property WORKER_REMARK1 As String

        <StringLength(255)>
        Public Property WORKER_REMARK2 As String
        '2023/03/22 Kawamoto ADD END

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


        Public Property CAT_L_CD As String

        Public Property CAT_NAME As String

        Public Property SUB_ASSET_NO As String

        Public Property TASK_RESULT As String

        Public Property UPD_DATETIME As Date

        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String

        <StringLength(255)>
        Public Property UPD_TASK As String

        ''' <summary>
        '''  ・ｽ・ｽ謨ｪ・ｽh・ｽ・ｽ・ｽb・ｽv・ｽ_・ｽE・ｽ・ｽ・ｽ・ｽ・ｽX・ｽg・ｽf・ｽ[・ｽ^
        ''' </summary>
        ''' <returns>・ｽ・ｽ謨ｪ・ｽh・ｽ・ｽ・ｽb・ｽv・ｽ_・ｽE・ｽ・ｽ・ｽ・ｽ・ｽX・ｽg・ｽf・ｽ[・ｽ^</returns>
        <NotMapped>
        Public Property locLDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  ・ｽ・ｽ・ｽ謨ｪ・ｽh・ｽ・ｽ・ｽb・ｽv・ｽ_・ｽE・ｽ・ｽ・ｽ・ｽ・ｽX・ｽg・ｽf・ｽ[・ｽ^
        ''' </summary>
        ''' <returns>・ｽ・ｽ・ｽ謨ｪ・ｽh・ｽ・ｽ・ｽb・ｽv・ｽ_・ｽE・ｽ・ｽ・ｽ・ｽ・ｽX・ｽg・ｽf・ｽ[・ｽ^</returns>
        <NotMapped>
        Public Property locMDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  ・ｽ・ｽ・ｽ謨ｪ・ｽh・ｽ・ｽ・ｽb・ｽv・ｽ_・ｽE・ｽ・ｽ・ｽ・ｽ・ｽX・ｽg・ｽf・ｽ[・ｽ^
        ''' </summary>
        ''' <returns>・ｽ・ｽ・ｽ謨ｪ・ｽh・ｽ・ｽ・ｽb・ｽv・ｽ_・ｽE・ｽ・ｽ・ｽ・ｽ・ｽX・ｽg・ｽf・ｽ[・ｽ^</returns>
        <NotMapped>
        Public Property locSDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  ・ｽJ・ｽe・ｽS・ｽ・ｽ・ｽ[・ｽ@・ｽ蝠ｪ・ｽﾞド・ｽ・ｽ・ｽb・ｽv・ｽ_・ｽE・ｽ・ｽ・ｽ・ｽ・ｽX・ｽg・ｽf・ｽ[・ｽ^
        ''' </summary>
        ''' <returns>・ｽ蝠ｪ・ｽﾞド・ｽ・ｽ・ｽb・ｽv・ｽ_・ｽE・ｽ・ｽ・ｽ・ｽ・ｽX・ｽg・ｽf・ｽ[・ｽ^</returns>
        <NotMapped>
        Public Property catLDropdownData As List(Of CategoryDropdownModel)

    End Class
End Namespace
