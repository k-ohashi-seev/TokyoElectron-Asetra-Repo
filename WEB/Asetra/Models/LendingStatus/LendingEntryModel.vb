Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models

    <Table("T_RESERVE")>
    Partial Public Class LendingEntryModel
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>  '2022/07/25 upd sv sato Identity Å® NoneÇ…ïœçX
        Public Property RESERVE_ID As Long = 0

        <Required>
        <StringLength(15)>
        Public Property REQ_EMP_CD As String = String.Empty

        <Required>
        <StringLength(15)>
        Public Property CHARGE_EMP_CD As String = String.Empty

        Public Property REQUEST_DT As Integer = 0

        Public Property REAL_START_DT As Integer = 0

        Public Property REAL_END_DT As Integer = 0

        Public Property USER_START_DT As Integer = 0

        Public Property USER_END_DT As Integer = 0

        <Required>
        <StringLength(15)>
        Public Property LEASE_PLACE_CD As String = String.Empty

        <Required>
        <StringLength(30)>
        Public Property LEASE_USE As String = String.Empty

        <Required>
        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String = String.Empty

        '<Required>                             UPD sv sato 2022/10/03
        <Required(AllowEmptyStrings:=True)>
        <StringLength(16)>
        Public Property DEST_NAME1 As String = String.Empty

        <StringLength(16)>
        Public Property DEST_NAME2 As String = String.Empty

        '2023/03/22 Kawamoto ADD START
        <Required(AllowEmptyStrings:=True)>
        <StringLength(16)>
        Public Property TO_DEST_NAME As String = String.Empty
        '2023/03/22 Kawamoto ADD END

        <StringLength(16)>
        Public Property DEST_ADDRESS1 As String = String.Empty

        <StringLength(16)>
        Public Property DEST_ADDRESS2 As String = String.Empty

        <StringLength(16)>
        Public Property DEST_ADDRESS3 As String = String.Empty

        <StringLength(14)>
        Public Property DEST_PHONE_NUMBER As String = String.Empty

        <StringLength(8)>
        Public Property DEST_ZIP_CODE As String = String.Empty

        '2023/03/22 Kawamoto ADD START
        <StringLength(15)>
        Public Property RET_LEASE_PLACE_CD As String = String.Empty

        <Required(AllowEmptyStrings:=True)>
        <StringLength(16)>
        Public Property RET_ADDRESS1 As String = String.Empty

        <StringLength(16)>
        Public Property RET_ADDRESS2 As String = String.Empty

        <StringLength(16)>
        Public Property RET_ADDRESS3 As String = String.Empty

        <StringLength(8)>
        Public Property RET_ZIP_CODE As String = String.Empty
        '2023/03/22 Kawamoto ADD END

        <StringLength(255)>
        Public Property REMARK As String = String.Empty

        <StringLength(255)>
        Public Property REMARK_INTERNAL As String = String.Empty

        <StringLength(100)>
        Public Property DEST_NO As String = String.Empty

        Public Property LEASE_CHG_TIMES As Integer = 0

        <StringLength(1)>
        Public Property RESERVE_STATUS As String = String.Empty

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
