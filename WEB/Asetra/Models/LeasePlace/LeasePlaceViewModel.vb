Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 貸出先メンテ用表示用モデル
    ''' </summary>
    Partial Public Class LeasePlaceViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 貸出先コード
        ''' </summary>
        ''' <returns>貸出先コード</returns>
        <Key>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property LEASE_PLACE_CD As String = String.Empty

        ''' <summary>
        ''' 貸出先名
        ''' </summary>
        ''' <returns>貸出先名</returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property LEASE_PLACE_NAME As String = String.Empty

        ''' <summary>
        ''' 電話番号			
        ''' </summary>
        ''' <returns>電話番号</returns>
        <StringLength(itemlength.PhoneNumber, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property PHONE_NUMBER As String = String.Empty

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

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' 二次貸出先フラグ			
        ''' </summary>
        ''' <returns>二次貸出先フラグ</returns>
        Public Property SEC_LEASE_PLACE_FLG As String = String.Empty
        '2023/03/22 Kawamoto ADD END

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

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' 表示項目：二次貸出先フラグ　チェックボックス表示用
        ''' </summary>
        ''' <returns>二次貸出先フラグ　チェックボックス表示用</returns>
        <NotMapped>
        Public Property DispSecLeasePlaceFlg As Boolean = False
        '2023/03/22 Kawamoto ADD END

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' 貸出先マスタ情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
