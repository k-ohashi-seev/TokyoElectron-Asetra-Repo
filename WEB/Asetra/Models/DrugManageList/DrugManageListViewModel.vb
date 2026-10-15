Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    Public Class DrugManageListViewModel
        Inherits BaseModel

        <Key>
        <StringLength(24, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_NO As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_NAME As String

        Public Property LOT_NO As String

        Public Property PURCHASER As String

        Public Property DUE_DATE As String

        Public Property LATEST_CHECK_DATE As String

        Public Property LOC_M_NAME As String

        ''' <summary>
        ''' 出力条件 期間(From)
        ''' </summary>
        ''' <returns>出力条件 期間(From)</returns>
        <NotMapped>
        Public Property SearchDateFrom As String = String.Empty

        ''' <summary>
        ''' 出力条件 期間(To)
        ''' </summary>
        ''' <returns>出力条件 期間(To)</returns>
        <NotMapped>
        Public Property SearchDateTo As String = String.Empty

        ''' <summary>
        ''' 出力条件 ロケ大区分
        ''' </summary>
        ''' <returns>出力条件 ロケ大区分</returns>
        <NotMapped>
        Public Property SearchLocL As String = String.Empty

        ''' <summary>
        ''' 出力条件 ロケ中区分
        ''' </summary>
        ''' <returns>出力条件 ロケ中区分</returns>
        <NotMapped>
        Public Property SearchLocM As String = String.Empty

        ''' <summary>
        ''' 出力条件 ロケ小区分
        ''' </summary>
        ''' <returns>出力条件 ロケ小区分</returns>
        <NotMapped>
        Public Property SearchLocS As String = String.Empty

        ''' <summary>
        ''' 出力条件 カテゴリ大分類
        ''' </summary>
        ''' <returns>出力条件 カテゴリ大分類</returns>
        <NotMapped>
        Public Property SearchCatL As String = String.Empty

        ''' <summary>
        ''' 出力条件 カテゴリ中分類
        ''' </summary>
        ''' <returns>出力条件 カテゴリ中分類</returns>
        <NotMapped>
        Public Property SearchCatM As String = String.Empty

        ''' <summary>
        ''' 出力条件 カテゴリ小分類
        ''' </summary>
        ''' <returns>出力条件 カテゴリ小分類</returns>
        <NotMapped>
        Public Property SearchCatS As String = String.Empty

        ''' <summary>
        ''' 出力条件 薬品番号
        ''' </summary>
        ''' <returns>出力条件 薬品番号</returns>
        <StringLength(24, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        <NotMapped>
        Public Property SearchDrugNo As String = String.Empty

        ''' <summary>
        ''' 出力条件 薬品名称
        ''' </summary>
        ''' <returns>出力条件 薬品名称</returns>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        <NotMapped>
        Public Property SearchDrugName As String = String.Empty

        ''' <summary>
        '''  大区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>大区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locLDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  中区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>中区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locMDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  小区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>小区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locSDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  カテゴリー　大分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>大分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catLDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        '''  カテゴリー　中分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>中分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catMDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        '''  カテゴリー　小分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>小分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catSDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        ''' 非表示項目：メニュー開閉フラグ
        ''' </summary>
        ''' <returns>非表示項目：メニュー開閉フラグ</returns>
        <NotMapped>
        Public Property OpenFlg As String = String.Empty

        ''' <summary>
        ''' 選択キーリスト(カンマ区切り)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property SelectKeys As String

        ''' <summary>
        ''' 未選択キーリスト(カンマ区切り)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property NoneSelectKeys As String

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

