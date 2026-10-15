Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 貸出先資産照会画面表示用モデル
    ''' </summary>
    Public Class AssetLeasePlaceDetailViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 貸出先コード
        ''' </summary>
        ''' <returns>貸出先コード</returns>
        <Key>
        <Column(Order:=0)>
        Public Property LEASE_PLACE_CD As String = String.Empty

        ''' <summary>
        ''' 貸出先名
        ''' </summary>
        ''' <returns>貸出先名</returns>
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
        ''' 検索条件 過去貸出含む
        ''' </summary>
        ''' <returns>検索条件 過去貸出含む</returns>
        <NotMapped>
        Public Property SearchLeasePast As Boolean = False

        ''' <summary>
        ''' 検索条件 貸出備考
        ''' </summary>
        ''' <returns>検索条件 貸出備考</returns>
        <NotMapped>
        Public Property SearchLeaseBiko As String = String.Empty

        ''' <summary>
        ''' 検索条件 貸出担当者コード
        ''' </summary>
        ''' <returns>検索条件 貸出担当者コード</returns>
        <NotMapped>
        Public Property SearchLeaseEmployeeCd As String = String.Empty

        ''' <summary>
        ''' 表示項目 貸出状態
        ''' </summary>
        ''' <returns>表示項目 貸出状態</returns>
        Public Property DispLeaseStatus As Integer? = Nothing

        ''' <summary>
        ''' 表示項目 貸出期間(From)
        ''' </summary>
        ''' <returns>表示項目 貸出期間(From)</returns>
        Public Property DispLeaseKikanFrom As Integer? = Nothing

        ''' <summary>
        ''' 表示項目 貸出期間(To)
        ''' </summary>
        ''' <returns>表示項目 貸出期間(To)</returns>
        Public Property DispLeaseKikanTo As Integer? = Nothing

        ''' <summary>
        ''' 表示項目 貸出先備考
        ''' </summary>
        ''' <returns>表示項目 貸出先備考</returns>
        Public Property DispLeaseBiko As String = String.Empty

        ''' <summary>
        ''' 表示項目 貸出先担当社員コード
        ''' </summary>
        ''' <returns>表示項目 貸出先担当社員コード</returns>
        Public Property DispLeaseEmployeeCd As String = String.Empty

        ''' <summary>
        ''' 表示項目 貸出先担当社員名
        ''' </summary>
        ''' <returns>表示項目 貸出先担当社員名</returns>
        Public Property DispLeaseEmployeeNm As String = String.Empty

        ''' <summary>
        ''' 表示項目 資産番号
        ''' </summary>
        ''' <returns>表示項目 資産番号</returns>
        <Key>
        <Column(Order:=1)>
        Public Property ASSET_NO As String = String.Empty

        ''' <summary>
        ''' 表示項目 資産名称
        ''' </summary>
        ''' <returns>表示項目 資産名称</returns>
        Public Property ASSET_NAME As String = String.Empty

        ''' <summary>
        ''' 表示項目 資産製造元
        ''' </summary>
        ''' <returns>表示項目 資産製造元</returns>
        Public Property MAKER As String = String.Empty

        ''' <summary>
        ''' 表示項目 品番
        ''' </summary>
        ''' <returns>表示項目 品番</returns>
        Public Property PRODUCT_NO As String = String.Empty

        ''' <summary>
        ''' 表示項目 型番
        ''' </summary>
        ''' <returns>表示項目 型番</returns>
        Public Property MODEL_NO As String = String.Empty

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

