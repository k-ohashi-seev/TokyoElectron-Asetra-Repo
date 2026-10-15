Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 貸出先一覧画面表示用モデル
    ''' </summary>
    Public Class AssetLeasePlaceListViewModel
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

        '''' <summary>
        '''' 登録日時
        '''' </summary>
        '''' <returns>登録日時</returns>
        'Public Property INS_DATETIME As Date? = Nothing

        '''' <summary>
        '''' 登録者社員コード
        '''' </summary>
        '''' <returns>登録者社員コード</returns>
        '<StringLength(15)>
        'Public Property INS_EMPLOYEE_CD As String = String.Empty

        '''' <summary>
        '''' 登録者氏名
        '''' </summary>
        '''' <returns>登録者氏名</returns>
        '<StringLength(60)>
        'Public Property INS_EMPLOYEE_NAME As String = String.Empty

        '''' <summary>
        '''' 最終登録更新日時
        '''' </summary>
        '''' <returns>最終登録更新日時</returns>
        'Public Property UPD_DATETIME As Date? = Nothing

        '''' <summary>
        '''' 最終登録更新者社員コード
        '''' </summary>
        '''' <returns>最終登録更新者社員コード</returns>
        '<StringLength(15)>
        'Public Property UPD_EMPLOYEE_CD As String = String.Empty

        '''' <summary>
        '''' 最終登録更新者氏名
        '''' </summary>
        '''' <returns>最終登録更新者氏名</returns>
        '<StringLength(60)>
        'Public Property UPD_EMPLOYEE_NAME As String = String.Empty

        '''' <summary>
        '''' 最終登録更新処理
        '''' </summary>
        '''' <returns>最終登録更新処理</returns>
        '<StringLength(255)>
        'Public Property UPD_TASK As String = String.Empty

        ''' <summary>
        ''' 検索条件 コード
        ''' </summary>
        ''' <returns>検索条件 コード</returns>
        <NotMapped>
        Public Property SearchCode As String = String.Empty

        ''' <summary>
        ''' 検索条件 貸出期限切れ
        ''' </summary>
        ''' <returns>検索条件 貸出期限切れ</returns>
        <NotMapped>
        Public Property SearchLeaseLimitExpired As Boolean = False

        ''' <summary>
        ''' 検索条件 現在貸出有り
        ''' </summary>
        ''' <returns>検索条件 現在貸出有り</returns>
        <NotMapped>
        Public Property SearchLeaseNow As Boolean = False

        ''' <summary>
        ''' 表示項目 貸出数
        ''' </summary>
        ''' <returns>表示項目 貸出数</returns>
        Public Property DispLeaseCount As Integer = 0

        ''' <summary>
        ''' 表示項目 期限切れ
        ''' </summary>
        ''' <returns>表示項目 期限切れ</returns>
        Public Property DispLeaseLimitExpiredCount As Integer = 0

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' カテゴリマスタ情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
