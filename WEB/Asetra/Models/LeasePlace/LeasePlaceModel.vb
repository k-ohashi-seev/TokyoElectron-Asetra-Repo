Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 貸出先メンテ用モデル
    ''' </summary>
    <Table("M_LEASE_PLACE")>
    Partial Public Class LeasePlaceModel

        ''' <summary>
        ''' 貸出先コード
        ''' </summary>
        ''' <returns>貸出先コード</returns>
        <Key>
        Public Property LEASE_PLACE_CD As String = String.Empty

        ''' <summary>
        ''' 貸出先名
        ''' </summary>
        ''' <returns>貸出先名</returns>
        Public Property LEASE_PLACE_NAME As String = String.Empty

        ''' <summary>
        ''' 電話番号			
        ''' </summary>
        ''' <returns>電話番号</returns>
        Public Property PHONE_NUMBER As String = String.Empty

        ''' <summary>
        ''' 郵便番号			
        ''' </summary>
        ''' <returns>郵便番号</returns>
        Public Property ZIP_CODE As String = String.Empty

        ''' <summary>
        ''' 住所			
        ''' </summary>
        ''' <returns>住所</returns>
        Public Property ADDRESS As String = String.Empty

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' 二次貸出先フラグ			
        ''' </summary>
        ''' <returns>二次貸出先フラグ</returns>
        <StringLength(1)>
        Public Property SEC_LEASE_PLACE_FLG As String = String.Empty
        '2023/03/22 Kawamoto ADD START

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
