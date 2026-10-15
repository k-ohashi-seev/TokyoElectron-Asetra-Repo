Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace Models
    ''' <summary>
    ''' 予約メンテナンスメンテ用モデル
    ''' </summary>
    <Table("T_RESERVE_MAINTE")>
    Partial Public Class LendingMainteEntryModel
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.Identity)>
        Public Property MAINTE_ID As Long = 0

        Public Property RESERVE_ID As Long = 0

        Public Property ASSET_NO As String = String.Empty

        Public Property MAINTE_START_DT As Integer = 0

        Public Property MAINTE_END_DT As Integer = 0

        Public Property REMARK As String = String.Empty

        Public Property MAINTE_KBN As String = String.Empty

        Public Property INS_DATETIME As Date? = Nothing

        Public Property INS_EMPLOYEE_CD As String = String.Empty

        Public Property INS_EMPLOYEE_NAME As String = String.Empty

        Public Property UPD_DATETIME As Date? = Nothing

        Public Property UPD_EMPLOYEE_CD As String = String.Empty

        Public Property UPD_EMPLOYEE_NAME As String = String.Empty

        Public Property UPD_TASK As String = String.Empty
    End Class

End Namespace
