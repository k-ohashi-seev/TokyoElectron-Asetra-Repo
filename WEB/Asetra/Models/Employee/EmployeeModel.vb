Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' 社員メンテ用モデル
    ''' </summary>
    <Table("M_EMPLOYEE")>
    Partial Public Class EmployeeModel
        ''' <summary>
        ''' 社員コード
        ''' </summary>
        ''' <returns>社員コード</returns>
        <Key>
        <Required(ErrorMessage:=CommonErrorMsgMember.NOT_ENTERD_ID)>
        <StringLength(15, ErrorMessage:=CommonErrorMsgMember.MAXLENGTH_ID)>
        Public Property EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' 社員名
        ''' </summary>
        ''' <returns>社員名</returns>
        <Required(ErrorMessage:=CommonErrorMsgMember.NOT_ENTERD_NAME)>
        <StringLength(60, ErrorMessage:=CommonErrorMsgMember.MAXLENGTH_NAME)>
        Public Property EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' パスワード
        ''' </summary>
        ''' <returns>パスワード</returns>
        <StringLength(60)>
        Public Property PASSWORD As String = String.Empty

        ''' <summary>
        ''' 部署コード
        ''' </summary>
        ''' <returns>部署コード</returns>
        <StringLength(15)>
        Public Property DEPARTMENT_CD As String = String.Empty

        ''' <summary>
        ''' 権限
        ''' </summary>
        ''' <returns>権限</returns>
        <Required>
        <StringLength(1)>
        Public Property AUTH_DIV As String = String.Empty

        ''' <summary>
        ''' 退職フラグ			
        ''' </summary>
        ''' <returns>退職フラグ</returns>
        Public Property RETIRE_FLG As String = String.Empty

        ''' <summary>
        ''' メールアドレス
        ''' </summary>
        ''' <returns>メールアドレス</returns>
        <StringLength(itemlength.MailAddress, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property MAIL_ADDRESS As String = String.Empty

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
