Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 予約一覧用表示用モデル
    ''' </summary>
    Partial Public Class LendingListViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 予約ID
        ''' </summary>
        ''' <returns>予約ID</returns>
        <Key>
        Public Property RESERVE_ID As Long

        ''' <summary>
        ''' 予約ステータス
        ''' </summary>
        ''' <returns>予約ステータス</returns>
        Public Property RESERVE_STATUS As String = String.Empty

        ''' <summary>
        ''' 申請社員コード
        ''' </summary>
        ''' <returns>申請社員コード</returns>
        Public Property REQ_EMP_CD As String = String.Empty

        ''' <summary>
        ''' 担当社員コード
        ''' </summary>
        ''' <returns>担当社員コード</returns>
        Public Property CHARGE_EMP_CD As String = String.Empty

        ''' <summary>
        ''' 担当社員名
        ''' </summary>
        ''' <returns>担当社員名</returns>
        Public Property CHARGE_EMP_NAME As String = String.Empty

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
        ''' 貸出先コード
        ''' </summary>
        ''' <returns>貸出先コード</returns>
        Public Property LEASE_PLACE_CD As String = String.Empty

        ''' <summary>
        ''' 貸出先名
        ''' </summary>
        ''' <returns>貸出先名</returns>
        Public Property LEASE_PLACE_NAME As String = String.Empty

        ''' <summary>
        ''' 送り先名
        ''' </summary>
        ''' <returns>送り先名</returns>
        Public Property DEST_NAME As String = String.Empty

        ''' <summary>
        ''' 実貸出開始日
        ''' </summary>
        ''' <returns>実貸出開始日</returns>
        Public Property REAL_START_DT As Integer = 0

        ''' <summary>
        ''' 実貸出終了日
        ''' </summary>
        ''' <returns>実貸出終了日</returns>
        Public Property REAL_END_DT As Integer = 0

        '2023/03/22 Kawamoto ADD START
            ''' <summary>
        ''' 送り状番号
        ''' </summary>
        ''' <returns>送り状番号</returns>
        Public Property DEST_NO As String = String.Empty
        '2023/03/22 Kawamoto ADD END

        '2024/04/25 Tomatsuri ADD START
        ''' <summary>
        ''' 貸出用途
        ''' </summary>
        ''' <returns>貸出用途</returns>
        Public Property LEASE_USE As String = String.Empty
        '2024/04/25 Tomatsuri ADD END

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
        ''' 検索条件 予約番号
        ''' </summary>
        ''' <returns>検索条件 予約番号</returns>
        <NotMapped>
        Public Property SearchReserveId As String = String.Empty

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
        ''' 検索条件 返却済
        ''' </summary>
        ''' <returns>検索条件 返却済</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_Returned As Boolean = False

        ''' <summary>
        ''' 検索条件 返却遅延
        ''' </summary>
        ''' <returns>検索条件 返却遅延</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_ReturnDelay As Boolean = False

        ''' <summary>
        ''' 検索条件 日付抽出
        ''' </summary>
        ''' <returns>検索条件 日付抽出</returns>
        <NotMapped>
        Public Property SearchDateExtract As String = String.Empty

        ''' <summary>
        ''' 検索条件 実質貸出期間From
        ''' </summary>
        ''' <returns>検索条件 実質貸出期間From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchRealLendingPeriodFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 実質貸出期間To
        ''' </summary>
        ''' <returns>検索条件 実質貸出期間To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchRealLendingPeriodTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 実質貸出開始日
        ''' </summary>
        ''' <returns>検索条件 実質貸出開始日</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchRealLendingStartDate As String = String.Empty

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
        ''' 送り状作成情報KEY1
        ''' </summary>
        ''' <returns>送り状作成情報KEY1</returns>
        <NotMapped>
        Public Property InvoiceKey As String = String.Empty

        ''' <summary>
        ''' プリンタ情報KEY1
        ''' </summary>
        ''' <returns>プリンタ情報KEY1</returns>
        <NotMapped>
        Public Property PrinterKey As String = String.Empty

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
