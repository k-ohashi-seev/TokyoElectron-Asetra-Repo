Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' カテゴリーメンテ用モデル
    ''' </summary>
    <Table("M_CATEGORY")>
    Partial Public Class CategoryModel

        ''' <summary>
        ''' 分類区分
        ''' </summary>
        ''' <returns>分類区分</returns>
        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property CAT_DIV As String = String.Empty

        ''' <summary>
        ''' カテゴリ大分類コード
        ''' </summary>
        ''' <returns>カテゴリ大分類コード</returns>
        <Key>
        <Column(Order:=1)>
        <StringLength(8)>
        Public Property CAT_L_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリ中分類コード
        ''' </summary>
        ''' <returns>カテゴリ中分類コード</returns>
        <Key>
        <Column(Order:=2)>
        <StringLength(8)>
        Public Property CAT_M_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリ小分類コード
        ''' </summary>
        ''' <returns>カテゴリ小分類コード</returns>
        <Key>
        <Column(Order:=3)>
        <StringLength(8)>
        Public Property CAT_S_CD As String = String.Empty

        ''' <summary>
        ''' 分類名称
        ''' </summary>
        ''' <returns>分類名称</returns>
        Public Property CAT_NAME As String = String.Empty

        ''' <summary>
        ''' リース期限アラート区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property LEASE_LIMIT_ALERT_DIV As String = String.Empty

        ''' <summary>
        ''' リース期限何日前
        ''' </summary>
        ''' <returns></returns>
        Public Property LEASE_LIMIT_BEF_DAYS As Integer? = Nothing

        ''' <summary>
        ''' ○○期限アラート区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property GENERIC_LIMIT_ALERT_DIV As String = String.Empty

        ''' <summary>
        ''' ○○期限何日前
        ''' </summary>
        ''' <returns></returns>
        Public Property GENERIC_LIMIT_BEF_DAYS As Integer? = Nothing

        ''' <summary>
        ''' 貸出アラート区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property LEASE_ALERT_DIV As String = String.Empty

        ''' <summary>
        ''' 貸出アラート回数
        ''' </summary>
        ''' <returns></returns>
        Public Property LEASE_ALERT_CNT As Integer? = Nothing

        ''' <summary>
        ''' 貸出上限数
        ''' </summary>
        ''' <returns></returns>
        Public Property LEASE_UPPER_LIMIT As Integer? = Nothing

        ''' <summary>
        ''' 貸出上限エラー区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property LEASE_UPPER_ERR_DIV As String = String.Empty

        ''' <summary>
        ''' 資産ID登録方法
        ''' </summary>
        ''' <returns>資産ID登録方法</returns>
        Public Property ASSET_ID_REG_DIV As Byte? = Nothing

        ''' <summary>
        ''' EPC領域サイズ
        ''' </summary>
        ''' <returns>EPC領域サイズ</returns>
        Public Property EPC_SIZE As Integer? = Nothing

        ''' <summary>
        ''' EPC事業所キー
        ''' </summary>
        ''' <returns></returns>
        Public Property EPC_COMPANY_KEY As String = String.Empty

        ''' <summary>
        ''' EPC資産タイプ
        ''' </summary>
        ''' <returns></returns>
        Public Property EPC_ASSET_TYPE As String = String.Empty

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

        ''' <summary>
        ''' カテゴリ大分類名
        ''' </summary>
        ''' <returns>カテゴリ大分類名</returns>
        <NotMapped>
        Public Property CatLName As String = String.Empty

        ''' <summary>
        ''' カテゴリ中分類名
        ''' </summary>
        ''' <returns>カテゴリ中分類名</returns>
        <NotMapped>
        Public Property CatMName As String = String.Empty

        ''' <summary>
        ''' カテゴリ小分類名
        ''' </summary>
        ''' <returns>カテゴリ小分類名</returns>
        <NotMapped>
        Public Property CatSName As String = String.Empty

        ''' <summary>
        ''' 資産ID登録方法表示非表示
        ''' </summary>
        ''' <returns>資産ID登録方法表示非表示</returns>
        <NotMapped>
        Public Property AssetIdDivReg As Boolean = True

        ''' <summary>
        ''' EPC事業所キードロップダウンリストデータ
        ''' </summary>
        ''' <returns>EPC事業所キードロップダウンリストデータ</returns>
        <NotMapped>
        Public Property epcCompanyKeyDropdownData As List(Of EpcCompanyKeyDropdownModel)
    End Class
End Namespace
