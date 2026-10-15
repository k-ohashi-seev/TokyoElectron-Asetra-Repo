Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' カテゴリーメンテ用表示用モデル
    ''' </summary>
    Partial Public Class CategoryViewModel
        Inherits BaseModel

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
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(8, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CAT_L_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリ中分類コード
        ''' </summary>
        ''' <returns>カテゴリ中分類コード</returns>
        <Key>
        <Column(Order:=2)>
        <StringLength(8, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CAT_M_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリ小分類コード
        ''' </summary>
        ''' <returns>カテゴリ小分類コード</returns>
        <Key>
        <Column(Order:=3)>
        <StringLength(8, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CAT_S_CD As String = String.Empty

        ''' <summary>
        ''' 分類名称
        ''' </summary>
        ''' <returns>分類名称</returns>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CAT_NAME As String = String.Empty

        <StringLength(60)>
        Public Property CAT_L_NAME As String = String.Empty
        <StringLength(60)>
        Public Property CAT_M_NAME As String = String.Empty
        <StringLength(60)>
        Public Property CAT_S_NAME As String = String.Empty

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
        <StringLength(30)>
        Public Property EPC_COMPANY_KEY As String = String.Empty

        ''' <summary>
        ''' EPC事業所キー表示名
        ''' </summary>
        ''' <returns></returns>
        Public Property EPC_COMPANY_KEY_NAME As String = String.Empty

        ''' <summary>
        ''' EPC資産タイプ
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
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
        ''' 検索条件 カテゴリ大分類
        ''' </summary>
        ''' <returns>検索条件 カテゴリ大分類</returns>
        <NotMapped>
        Public Property SearchCatL As String = String.Empty

        ''' <summary>
        ''' 検索条件 カテゴリ中分類
        ''' </summary>
        ''' <returns>検索条件 カテゴリ中分類</returns>
        <NotMapped>
        Public Property SearchCatM As String = String.Empty

        ''' <summary>
        ''' 検索条件 カテゴリ小分類
        ''' </summary>
        ''' <returns>検索条件 カテゴリ小分類</returns>
        <NotMapped>
        Public Property SearchCatS As String = String.Empty

        ''' <summary>
        ''' 検索条件 表示指定
        ''' </summary>
        ''' <returns>検索条件 表示指定</returns>
        <NotMapped>
        Public Property SearchDispSelect As String = String.Empty

        ''' <summary>
        '''  大分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>大分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catLDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        '''  中分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>中分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catMDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        '''  小分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>小分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catSDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        ''' 資産ID登録方法ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>資産ID登録方法ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property assetIdRegDivDropdownData As List(Of AssetIdRegDivDropdownModel)

        ''' <summary>
        ''' EPC領域サイズドロップダウンリストデータ
        ''' </summary>
        ''' <returns>EPC領域サイズドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property epcSizeDropdownData As List(Of EpcSizeDropdownModel)

        ''' <summary>
        ''' EPC事業所キードロップダウンリストデータ
        ''' </summary>
        ''' <returns>EPC事業所キードロップダウンリストデータ</returns>
        <NotMapped>
        Public Property epcCompanyKeyDropdownData As List(Of EpcCompanyKeyDropdownModel)

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
        ''' 登録変更画面　コンボボックス表示 カテゴリ大分類
        ''' </summary>
        ''' <returns>登録変更画面 カテゴリ大分類</returns>
        <NotMapped>
        Public Property ModalCatL As String = String.Empty

        ''' <summary>
        ''' 登録変更画面　コンボボックス表示 カテゴリ中分類
        ''' </summary>
        ''' <returns>登録変更画面 カテゴリ中分類</returns>
        <NotMapped>
        Public Property ModalCatM As String = String.Empty


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
