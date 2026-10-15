Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models
    Partial Public Class ReserveDetailViewModel
        <Key>
        <Column(Order:=0)>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property RESERVE_ID As Long

        <Key>
        <Column(Order:=1)>
        <StringLength(24)>
        Public Property ASSET_NO As String

        <Key>
        <Column(Order:=2)>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property REAL_START_DT As Integer

        Public Property REAL_END_DT As Integer

        <Required>
        <StringLength(15)>
        Public Property CHARGE_EMP_CD As String

        Public Property LENDING_DT As Integer?

        Public Property RETURN_DT As Integer?

        Public Property PR_ST_DT As Integer

        Public Property PR_END_DT As Integer

        Public Property LEASE_CHG_TIMES As Integer

        <StringLength(1)>
        Public Property HANDOVER_DIV As String

        Public Property HANDOVER_RESERVE_ID As Long?

        <StringLength(1)>
        Public Property RESERVE_STATUS As String

        Public Property INS_DATETIME As Date

        <Required>
        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String

        <Required>
        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String

        Public Property UPD_DATETIME As Date

        <Required>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String

        <Required>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String

        <Required>
        <StringLength(255)>
        Public Property UPD_TASK As String

        ''' <summary>
        ''' 表示項目：資産名
        ''' </summary>
        ''' <returns>表示項目：資産名</returns>
        Public Property AssetName As String = String.Empty

        ''' <summary>
        ''' 表示項目：シリアルNo
        ''' </summary>
        ''' <returns>表示項目：シリアルNo</returns>
        Public Property SerialNo As String = String.Empty

        ''' <summary>
        ''' 表示項目：貸出期限日
        ''' </summary>
        ''' <returns>表示項目：貸出期限日</returns>
        <NotMapped>
        Public Property LeaseKigen As String = String.Empty

        ''' <summary>
        ''' 表示項目：担当者
        ''' </summary>
        ''' <returns>表示項目：担当者</returns>
        Public Property EmployeeName As String = String.Empty

        ''' <summary>
        ''' 表示項目：実貸出開始日
        ''' </summary>
        ''' <returns>表示項目：実貸出開始日</returns>
        <NotMapped>
        Public Property RealStartDtString As String = String.Empty

        ''' <summary>
        ''' 表示項目：実貸出終了日
        ''' </summary>
        ''' <returns>表示項目：実貸出終了日</returns>
        <NotMapped>
        Public Property RealEndDtString As String = String.Empty

        ''' <summary>
        ''' 隠し項目：重複有フラグ（From）
        ''' </summary>
        ''' <returns>隠し項目：重複有フラグ（From）</returns>
        <NotMapped>
        Public Property JuufukuAriFrom As Boolean = False

        ''' <summary>
        ''' 隠し項目：重複有フラグ（From+1～To-1）
        ''' </summary>
        ''' <returns>隠し項目：重複有フラグ（From+1～To-1）</returns>
        <NotMapped>
        Public Property JuufukuAriNaka As Boolean = False

        ''' <summary>
        ''' 隠し項目：重複有フラグ（To）
        ''' </summary>
        ''' <returns>隠し項目：重複有フラグ（To）</returns>
        <NotMapped>
        Public Property JuufukuAriTo As Boolean = False

        ''' <summary>
        ''' 隠し項目：引継有時の次予約明細の実貸出開始日
        ''' </summary>
        ''' <returns>隠し項目：引継有時の次予約明細の実貸出開始日</returns>
        <NotMapped>
        Public Property NextRealStartDt As Integer = 0

    End Class
End Namespace

