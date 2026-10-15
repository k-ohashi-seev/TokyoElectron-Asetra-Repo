Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' 仮置一覧用モデル
    ''' </summary>
    <Table("T_TEMPORARY")>
    Partial Public Class TemporaryListModel
        <Key>
        Public Property TEMP_ID As Long

        <StringLength(20)>
        Public Property REG_CONST_NO As String

        <StringLength(20)>
        Public Property REL_CONST_NO As String

        Public Property TEMP_DT As Integer

        <StringLength(15)>
        Public Property WORK_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property WORK_EMPLOYEE_NAME As String

        <StringLength(255)>
        Public Property TEMP_INFO1 As String

        <StringLength(255)>
        Public Property TEMP_INFO2 As String

        <StringLength(255)>
        Public Property TEMP_INFO3 As String

        <StringLength(255)>
        Public Property TEMP_INFO4 As String

        <StringLength(255)>
        Public Property TEMP_INFO5 As String

        <StringLength(255)>
        Public Property TEMP_INFO6 As String

        <StringLength(255)>
        Public Property TEMP_INFO7 As String

        <StringLength(255)>
        Public Property TEMP_REMARK As String

        Public Property TASK_TS As Long?

        ''' <summary>
        ''' 登録日時
        ''' </summary>
        ''' <returns>登録日時</returns>
        <DataType(DataType.Date)>
        Public Property INS_DATETIME As Date? = Nothing

        ''' <summary>
        ''' 登録者社員コード
        ''' </summary>
        ''' <returns>登録者社員コード</returns>
        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' 登録者氏名
        ''' </summary>
        ''' <returns>登録者氏名</returns>
        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' 最終登録更新日時
        ''' </summary>
        ''' <returns>最終登録更新日時</returns>
        <DataType(DataType.Date)>
        Public Property UPD_DATETIME As Date? = Nothing

        ''' <summary>
        ''' 最終登録更新者社員コード
        ''' </summary>
        ''' <returns>最終登録更新者社員コード</returns>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' 最終登録更新者氏名
        ''' </summary>
        ''' <returns>最終登録更新者氏名</returns>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' 最終登録更新処理
        ''' </summary>
        ''' <returns>最終登録更新処理</returns>
        <StringLength(255)>
        Public Property UPD_TASK As String = String.Empty

        ''' <summary>
        ''' 登録日時 文字列
        ''' </summary>
        ''' <returns>登録日時 文字列</returns>
        <NotMapped>
        Public Property InsDatetimeString As String = String.Empty

        ''' <summary>
        ''' 最終登録更新日時 文字列
        ''' </summary>
        ''' <returns>最終登録更新日時 文字列</returns>
        <NotMapped>
        Public Property UpdDatetimeString As String = String.Empty
    End Class

End Namespace
