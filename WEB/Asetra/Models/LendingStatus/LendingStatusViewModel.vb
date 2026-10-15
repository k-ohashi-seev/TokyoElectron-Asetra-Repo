Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 予約状況照会用表示用モデル
    ''' </summary>
    Partial Public Class LendingStatusViewModel
        Inherits BaseModel

        Public Property CAT_L_CD As String

        Public Property CAT_M_CD As String

        Public Property CAT_S_CD As String

        Public Property CAT_L_NAME As String

        Public Property CAT_M_NAME As String

        Public Property CAT_S_NAME As String

        Public Property MNG_LOC_L_CD As String

        Public Property MNG_LOC_L_NAME As String

        <Key>
        Public Property ASSET_NO As String

        Public Property ASSET_NAME As String

        Public Property MODEL_NO As String          '2022/08/31 Add Kawamoto

        Public Property LEASE_STATUS As Byte = 0

        Public Property LEASE_CNT As Integer = 0

        '2021/11/30 upd nv Inoue
        'Public Property RESERVE_DSPSTAT As String
        Public Property ASSET_STATUS As Byte = 0

        Public Property DEST_NO As String

        Public Property UPD_FLG As String

        Public Property CAL_STR As String

        Public Property PRODUCT_NO As String

        '2022/08/31 Add Start Kawamoto
        ''' <summary>
        ''' 非稼働日(カンマ区切り)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property NotWorkDay As String
        '2022/08/31 Add End Kawamoto

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
        ''' 検索条件 担当者
        ''' </summary>
        ''' <returns>検索条件 担当者</returns>
        <NotMapped>
        Public Property SearchEmpName As String = String.Empty

        ''' <summary>
        ''' 検索条件 申請者
        ''' </summary>
        ''' <returns>検索条件 申請者</returns>
        <NotMapped>
        Public Property SearchReqEmpName As String = String.Empty

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
        ''' 検索条件 資産番号   '2022/02/25 nv miyaji add
        ''' </summary>
        ''' <returns>検索条件 資産番号</returns>
        <NotMapped>
        Public Property SearchAssetNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 資産名称   '2022/02/25 nv miyaji add
        ''' </summary>
        ''' <returns>検索条件 資産名称</returns>
        <NotMapped>
        Public Property SearchAssetName As String = String.Empty

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
        ''' 検索条件 表示開始日
        ''' </summary>
        ''' <returns>検索条件 表示開始日</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchDspStartDate As String = String.Empty

        ''' <summary>
        ''' 表示月数
        ''' </summary>
        ''' <returns>表示月数</returns>
        <NotMapped>
        Public Property DspMonthCount As Integer = 0

        ''' <summary>
        ''' 検索条件 予約なし
        ''' </summary>
        ''' <returns>検索条件 予約なし</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_NoReserve As Boolean = False

        ''' <summary>
        ''' 検索条件 未貸出
        ''' </summary>
        ''' <returns>検索条件 未貸出</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_NotRented As Boolean = False

        ''' <summary>
        ''' 検索条件 貸出中
        ''' </summary>
        ''' <returns>検索条件 貸出中</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_Lending As Boolean = False

        ''' <summary>
        ''' 検索条件 貸出中(遅延)
        ''' </summary>
        ''' <returns>検索条件 貸出中(遅延)</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_LendingDelay As Boolean = False

        ''' <summary>
        ''' 検索条件 返却済
        ''' </summary>
        ''' <returns>検索条件 返却済</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_Returned As Boolean = False

        ''' <summary>
        ''' 検索条件 メンテ
        ''' </summary>
        ''' <returns>検索条件 メンテ</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_Mainte As Boolean = False

        ''' <summary>
        ''' 検索条件 〇〇登録
        ''' </summary>
        ''' <returns>検索条件 〇〇登録</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_BlaRegist As Boolean = False

        ''' <summary>
        ''' 検索条件 予備登録１
        ''' </summary>
        ''' <returns>検索条件 予備登録１</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_PreRegist1 As Boolean = False

        ''' <summary>
        ''' 検索条件 予備登録２
        ''' </summary>
        ''' <returns>検索条件 予備登録２</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_PreRegist2 As Boolean = False

        ''' <summary>
        ''' 検索条件 未通知
        ''' </summary>
        ''' <returns>検索条件 未通知</returns>
        <NotMapped>
        Public Property SearchShippingNotification_NotNotified As Boolean = False

        ''' <summary>
        ''' 検索条件 通知済
        ''' </summary>
        ''' <returns>検索条件 通知済</returns>
        <NotMapped>
        Public Property SearchShippingNotification_Notified As Boolean = False

        ''' <summary>
        ''' 検索条件 並び替え
        ''' </summary>
        ''' <returns>検索条件 並び替え</returns>
        <NotMapped>
        Public Property SearchSort As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事番号
        ''' </summary>
        ''' <returns>検索条件 工事番号</returns>
        <NotMapped>
        Public Property SearchConstNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 品番
        ''' </summary>
        ''' <returns>検索条件 品番</returns>
        <NotMapped>
        Public Property SearchProductNo As String = String.Empty

        ''' <summary>
        ''' 非表示項目：メニュー開閉フラグ
        ''' </summary>
        ''' <returns>非表示項目：メニュー開閉フラグ</returns>
        <NotMapped>
        Public Property OpenFlg As String = String.Empty

        ''' <summary>
        ''' 非表示項目：一覧列が列毎に閉じられているかを示す
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>item2:表示月を管理、項目固定列は0</remarks>
        <NotMapped>
        Public Property isClosedColumns As New List(Of closedColumn)
        'Public Property isClosedColumns As New List(Of Tuple(Of Boolean, Integer))

        ''' <summary>
        ''' 選択キー
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property SelectKeys As String = String.Empty

        ''' <summary>
        ''' 予約IDキー
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property ReserveIdKey As String = String.Empty

        ''' <summary>
        ''' メンテナンスIDキー
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property MainteIdKey As String = String.Empty

        ''' <summary>
        ''' 予約状況照会情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer
    End Class

    ''' <summary>
    ''' 列の開閉状態クラス
    ''' </summary>
    ''' <remarks>isClosedColumnsで使用する。</remarks>
    Public Class closedColumn
        Public Property isClosed As Boolean
        Public Property columnName As String
    End Class



End Namespace
