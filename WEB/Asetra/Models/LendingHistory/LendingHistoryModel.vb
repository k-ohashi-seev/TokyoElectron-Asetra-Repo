Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' ó\ñÒóöóÉÅÉìÉeópÉÇÉfÉã
    ''' </summary>
    <Table("T_RESERVE_LOG")>
    Partial Public Class LendingHistoryModel
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.Identity)>
        Public Property RESERVE_LOG_ID As Long

        Public Property PROCESS_DT As Integer

        <Required>
        <StringLength(9)>
        Public Property PROCESS_TM As String

        <Required>
        <StringLength(1)>
        Public Property PROCESS_TOOL_DIV As String

        <Required>
        <StringLength(2)>
        Public Property PROCESS_DIV As String

        Public Property RESERVE_ID As Long

        Public Property RESERVE_SUB_NO As Integer

        <Required>
        <StringLength(15)>
        Public Property REQ_EMP_CD As String

        <Required>
        <StringLength(15)>
        Public Property CHARGE_EMP_CD As String

        Public Property REQUEST_DT As Integer

        Public Property REAL_START_DT As Integer

        Public Property REAL_END_DT As Integer

        Public Property USER_START_DT As Integer

        Public Property USER_END_DT As Integer

        <Required>
        <StringLength(15)>
        Public Property LEASE_PLACE_CD As String

        <Required>
        <StringLength(30)>
        Public Property LEASE_USE As String

        <Required>
        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String

        '<Required>                             UPD sv sato 2022/10/03
        <Required(AllowEmptyStrings:=True)>
        <StringLength(16)>
        Public Property DEST_NAME1 As String

        <StringLength(16)>
        Public Property DEST_NAME2 As String

        <StringLength(16)>
        Public Property DEST_ADDRESS1 As String

        <StringLength(16)>
        Public Property DEST_ADDRESS2 As String

        <StringLength(16)>
        Public Property DEST_ADDRESS3 As String

        <StringLength(14)>
        Public Property DEST_PHONE_NUMBER As String

        <StringLength(8)>
        Public Property DEST_ZIP_CODE As String

        <StringLength(255)>
        Public Property REMARK As String

        <StringLength(255)>
        Public Property REMARK_INTERNAL As String

        <StringLength(100)>
        Public Property DEST_NO As String

        <StringLength(1)>
        Public Property RESERVE_STATUS As String

        <Required>
        <StringLength(24)>
        Public Property ASSET_NO As String

        Public Property DTL_REAL_START_DT As Integer

        Public Property DTL_REAL_END_DT As Integer

        <Required>
        <StringLength(15)>
        Public Property DTL_CHARGE_EMP_CD As String

        Public Property DTL_LENDING_DT As Integer?

        Public Property DTL_RETURN_DT As Integer?

        <StringLength(1)>
        Public Property DTL_HANDOVER_DIV As String

        <StringLength(1)>
        Public Property DTL_RESERVE_STATUS As String

        Public Property INS_DATETIME As Date? = Nothing

        <Required>
        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String = String.Empty

        <Required>
        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String = String.Empty

        Public Property UPD_DATETIME As Date? = Nothing

        <Required>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String = String.Empty

        <Required>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String = String.Empty

        <Required>
        <StringLength(255)>
        Public Property UPD_TASK As String = String.Empty
    End Class
End Namespace
