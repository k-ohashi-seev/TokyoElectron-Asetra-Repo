Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' ログイン用表示用モデル
    ''' </summary>
    Partial Public Class LoginViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 社員コード
        ''' </summary>
        ''' <returns>社員コード</returns>
        <Key>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' 社員名
        ''' </summary>
        ''' <returns>社員名</returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' パスワード
        ''' </summary>
        ''' <returns>パスワード</returns>
        <StringLength(60)>
        <DataType(DataType.Password)>
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
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(1)>
        Public Property AUTH_DIV As String = String.Empty

        ''' <summary>
        ''' 登録日時
        ''' </summary>
        ''' <returns>登録日時</returns>
        <DisplayFormat(DataFormatString:="{0:yyyy/MM/dd HH:mm}")>
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
        <DisplayFormat(DataFormatString:="{0:yyyy/MM/dd HH:mm}")>
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


        '----------ログイン画面で使用
        ''' <summary>
        ''' 入力項目：新パスワード
        ''' </summary>
        ''' <returns>入力項目：新パスワード</returns>
        <NotMapped>
        <StringLength(60)>
        <DataType(DataType.Password)>
        Public Property NewPassword As String = String.Empty

        ''' <summary>
        ''' 入力項目：確認パスワード
        ''' </summary>
        ''' <returns>入力項目：確認パスワード</returns>
        <NotMapped>
        <StringLength(60)>
        <DataType(DataType.Password)>
        Public Property KakuninPassword As String = String.Empty

        ''' <summary>
        ''' 非表示項目：メニュー開閉フラグ
        ''' </summary>
        ''' <returns>非表示項目：メニュー開閉フラグ</returns>
        <NotMapped>
        Public Property OpenFlg As String = String.Empty

        ''' <summary>
        ''' お知らせCD
        ''' </summary>
        ''' <returns>お知らせCD</returns>
        <NotMapped>
        Public Property NEWS_CD As Integer?

        ''' <summary>
        ''' 表示名
        ''' </summary>
        ''' <returns>表示名</returns>
        <NotMapped>
        <StringLength(100)>
        Public Property DISPLAY_NAME As String

        ''' <summary>
        ''' ダウンロードURL
        ''' </summary>
        ''' <returns>ダウンロードURL</returns>
        <NotMapped>
        <StringLength(255)>
        Public Property DOWNLOAD_URL As String

    End Class

End Namespace
