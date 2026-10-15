Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 部署メンテ用表示用モデル
    ''' </summary>
    Partial Public Class DepartmentViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 部署コード
        ''' </summary>
        ''' <returns>部署コード</returns>
        <Key>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(itemlength.DepartmentCd, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEPARTMENT_CD As String = String.Empty

        ''' <summary>
        ''' 部署名
        ''' </summary>
        ''' <returns>部署名</returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(itemlength.DepartmentNm, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEPARTMENT_NAME As String = String.Empty

        ''' <summary>
        ''' 郵便番号			
        ''' </summary>
        ''' <returns>郵便番号</returns>
        <StringLength(itemlength.ZipCode, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ZIP_CODE As String = String.Empty

        ''' <summary>
        ''' 住所			
        ''' </summary>
        ''' <returns>住所</returns>
        <StringLength(itemlength.Address, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ADDRESS As String = String.Empty

        ''' <summary>
        ''' 送付部署名			
        ''' </summary>
        ''' <returns>送付部署名</returns>
        <StringLength(itemlength.ReqDepName, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REQ_DEP_NAME As String = String.Empty

        ''' <summary>
        ''' 電話番号			
        ''' </summary>
        ''' <returns>電話番号</returns>
        <StringLength(itemlength.PhoneNumber, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property PHONE_NUMBER As String = String.Empty

        ''' <summary>
        ''' FAX			
        ''' </summary>
        ''' <returns>FAX</returns>
        <StringLength(itemlength.Fax, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
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
        ''' 検索条件 コード
        ''' </summary>
        ''' <returns>検索条件 コード</returns>
        <NotMapped>
        Public Property SearchCode As String = String.Empty

        ''' <summary>
        ''' 検索条件 名前
        ''' </summary>
        ''' <returns>検索条件 名前</returns>
        <NotMapped>
        Public Property SearchName As String = String.Empty

        ''' <summary>
        ''' 本部フラグ　チェックボックス表示用
        ''' </summary>
        ''' <returns>本部フラグ　チェックボックス表示用</returns>
        <NotMapped>
        Public Property HonbuFlg As Boolean = False

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' 部署マスタ情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)


    End Class
End Namespace
