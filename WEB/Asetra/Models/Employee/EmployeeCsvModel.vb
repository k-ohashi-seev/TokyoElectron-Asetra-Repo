Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' 社員メンテ用モデル
    ''' </summary>
    Partial Public Class EmployeeCsvModel
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
        ''' 部署コード
        ''' </summary>
        ''' <returns>部署コード</returns>
        <StringLength(15)>
        Public Property DEPARTMENT_CD As String = String.Empty

        ''' <summary>
        ''' メールアドレス
        ''' </summary>
        ''' <returns>メールアドレス</returns>
        <StringLength(itemlength.MailAddress, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property MAIL_ADDRESS As String = String.Empty

        ''' <summary>
        ''' 部署名
        ''' </summary>
        ''' <returns>部署名</returns>
        Public Property DEPARTMENT_NAME As String = String.Empty

    End Class

End Namespace
