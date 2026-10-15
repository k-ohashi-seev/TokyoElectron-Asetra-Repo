Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 部署メンテ用モデル
    ''' </summary>
    <Table("M_DEPARTMENT")>
    Partial Public Class DepartmentModel

        ''' <summary>
        ''' 部署コード
        ''' </summary>
        ''' <returns>部署コード</returns>
        <Key>
        Public Property DEPARTMENT_CD As String = String.Empty

        ''' <summary>
        ''' 部署名
        ''' </summary>
        ''' <returns>部署名</returns>
        Public Property DEPARTMENT_NAME As String = String.Empty

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

        ''' <summary>
        ''' 送付部署名			
        ''' </summary>
        ''' <returns>送付部署名</returns>
        Public Property REQ_DEP_NAME As String = String.Empty

        ''' <summary>
        ''' 電話番号			
        ''' </summary>
        ''' <returns>電話番号</returns>
        Public Property PHONE_NUMBER As String = String.Empty

        ''' <summary>
        ''' FAX			
        ''' </summary>
        ''' <returns>FAX</returns>
        Public Property FAX As String = String.Empty

        ''' <summary>
        ''' 本部/拠点
        ''' </summary>
        ''' <returns>本部/拠点</returns>
        Public Property HEAD_OFFICE_FLG As String = String.Empty

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
        <Required>
        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' 登録者氏名
        ''' </summary>
        ''' <returns>登録者氏名</returns>
        <Required>
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
        <Required>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' 最終登録更新者氏名
        ''' </summary>
        ''' <returns>最終登録更新者氏名</returns>
        <Required>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' 最終登録更新処理
        ''' </summary>
        ''' <returns>最終登録更新処理</returns>
        <Required>
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
