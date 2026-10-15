Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 貸出履歴照会用表示用モデル
    ''' </summary>
    Partial Public Class AssetLeaseHitoryViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 管理ロケ大区分コード
        ''' </summary>
        ''' <returns>管理ロケ大区分コード</returns>
        Public Property MNG_LOC_L_CD As String = String.Empty

        ''' <summary>
        ''' 管理ロケ大区分名称
        ''' </summary>
        ''' <returns>管理ロケ大区分名称</returns>
        Public Property MNG_LOC_L_NAME As String = String.Empty

        ''' <summary>
        ''' カテゴリ大分類コード
        ''' </summary>
        ''' <returns>カテゴリ大分類コード</returns>
        Public Property CAT_L_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリ中分類コード
        ''' </summary>
        ''' <returns>カテゴリ中分類コード</returns>
        Public Property CAT_M_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリ小分類コード
        ''' </summary>
        ''' <returns>カテゴリ小分類コード</returns>
        Public Property CAT_S_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリ大分類名称
        ''' </summary>
        ''' <returns>カテゴリ大分類名称</returns>
        Public Property CAT_L_NAME As String = String.Empty

        ''' <summary>
        ''' カテゴリ中分類名称
        ''' </summary>
        ''' <returns>カテゴリ中分類名称</returns>
        Public Property CAT_M_NAME As String = String.Empty

        ''' <summary>
        ''' カテゴリ小分類名称
        ''' </summary>
        ''' <returns>カテゴリ小分類名称</returns>
        Public Property CAT_S_NAME As String = String.Empty

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        ''' <returns>資産番号</returns>
        <Key>
        Public Property ASSET_NO As String = String.Empty

        ''' <summary>
        ''' 資産名称
        ''' </summary>
        ''' <returns>資産名称</returns>
        Public Property ASSET_NAME As String = String.Empty

        ''' <summary>
        ''' 除却
        ''' </summary>
        ''' <returns>除却</returns>
        Public Property DISPOSE As String = String.Empty

        ''' <summary>
        ''' 貸出回数
        ''' </summary>
        ''' <returns>貸出回数</returns>
        Public Property LEASE_CNT As Integer = 0

        ''' <summary>
        ''' 貸出日数
        ''' </summary>
        ''' <returns>貸出日数</returns>
        Public Property LEASE_DAY As Integer = 0

        '2022/8/31 Add Start Kawamoto
        ''' <summary>
        ''' メンテ日数
        ''' </summary>
        ''' <returns>メンテ日数</returns>
        Public Property MAINTE_DAY As Integer = 0
        '2022/8/31 Add End Kawamoto 

        ''' <summary>
        ''' 営業日数
        ''' </summary>
        ''' <returns>営業日数</returns>
        Public Property EIGYO_DAY As Integer = 0

        ''' <summary>
        ''' 検索条件 抽出期間From
        ''' </summary>
        ''' <returns>検索条件 抽出期間From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchTargetPeriodFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 抽出期間To
        ''' </summary>
        ''' <returns>検索条件 抽出期間To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchTargetPeriodTo As String = String.Empty

        ''' <summary>
        ''' 検索項目：除却　含めるか含めないか
        ''' </summary>
        ''' <returns>検索項目：除却　含めるか含めないか</returns>
        <NotMapped>
        Public Property SearchDispose As Boolean = False

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
        '''  管理ロケーションチェックボックスリストリストデータ
        ''' </summary>
        ''' <returns>管理ロケーションチェックボックスリストリストデータ</returns>
        <NotMapped>
        Public Property mngLocCheckBoxListData As List(Of LocationDropdownModel)

        ''' <summary>
        ''' 管理ロケーションの選択キーリスト(カンマ区切り)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property SelectMngLocKeys As String = String.Empty

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' 項目配置情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)
    End Class
End Namespace
