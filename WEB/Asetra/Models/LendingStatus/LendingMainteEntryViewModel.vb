Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    Partial Public Class LendingMainteEntryViewModel
        Inherits BaseModel
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.Identity)>
        Public Property MAINTE_ID As Long = 0

        Public Property RESERVE_ID As Long = 0

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(24)>
        Public Property ASSET_NO As String = String.Empty

        Public Property MAINTE_START_DT As Integer = 0

        Public Property MAINTE_END_DT As Integer = 0

        <StringLength(255)>
        Public Property REMARK As String = String.Empty

        Public Property MAINTE_KBN As String = String.Empty

        Public Property INS_DATETIME As Date? = Nothing

        Public Property INS_EMPLOYEE_CD As String = String.Empty

        Public Property INS_EMPLOYEE_NAME As String = String.Empty

        Public Property UPD_DATETIME As Date? = Nothing

        Public Property UPD_EMPLOYEE_CD As String = String.Empty

        Public Property UPD_EMPLOYEE_NAME As String = String.Empty

        Public Property UPD_TASK As String = String.Empty

        ''' <summary>
        ''' 表示項目：管理ロケーション
        ''' </summary>
        ''' <returns>表示項目：管理ロケーション</returns>
        <NotMapped>
        Public Property MngLocLName As String = String.Empty

        ''' <summary>
        ''' 表示項目：シリアルNo
        ''' </summary>
        ''' <returns>表示項目：シリアルNo</returns>
        <NotMapped>
        Public Property SerialNo As String = String.Empty

        ''' <summary>
        ''' 表示項目：資産名
        ''' </summary>
        ''' <returns>表示項目：資産名</returns>
        <NotMapped>
        Public Property AssetName As String = String.Empty

        ''' <summary>
        ''' メンテナンス開始日
        ''' </summary>
        ''' <returns>メンテナンス開始日</returns>
        <NotMapped>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_MAINTE_START_DT_EMPTY)>
        Public Property MainteStartDtString As String = String.Empty

        ''' <summary>
        ''' メンテナンス終了日
        ''' </summary>
        ''' <returns>メンテナンス終了日</returns>
        <NotMapped>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_MAINTE_END_DT_EMPTY)>
        Public Property MainteEndDtString As String = String.Empty

        ''' <summary>
        ''' 隠し項目：詳細権限区分
        ''' </summary>
        ''' <returns>隠し項目：詳細権限区分</returns>
        <NotMapped>
        Public Property AuthProcFlg As String = String.Empty

    End Class

End Namespace
