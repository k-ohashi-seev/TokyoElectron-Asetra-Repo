Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 予約履歴用表示用モデル
    ''' </summary>
    Partial Public Class LendingHistoryViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 予約履歴ID
        ''' </summary>
        ''' <returns>予約履歴ID</returns>
        <Key>
        Public Property RESERVE_LOG_ID As Long

        ''' <summary>
        ''' 予約ID
        ''' </summary>
        ''' <returns>予約ID</returns>
        Public Property RESERVE_ID As Long

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
        ''' 申請社員コード
        ''' </summary>
        ''' <returns>申請社員コード</returns>
        Public Property REQ_EMP_CD As String = String.Empty

        ''' <summary>
        ''' 申請社員名
        ''' </summary>
        ''' <returns>申請社員名</returns>
        Public Property REQ_EMP_NAME As String = String.Empty

        ''' <summary>
        ''' 処理日
        ''' </summary>
        ''' <returns>処理日</returns>
        Public Property PROCESS_DT As Integer = 0

        ''' <summary>
        ''' 処理時刻
        ''' </summary>
        ''' <returns>処理時刻</returns>
        Public Property PROCESS_TM As String = String.Empty

        ''' <summary>
        ''' 処理ツール区分
        ''' </summary>
        ''' <returns>処理ツール区分</returns>
        Public Property PROCESS_TOOL_DIV As String = String.Empty

        ''' <summary>
        ''' 処理区分
        ''' </summary>
        ''' <returns>処理区分</returns>
        Public Property PROCESS_DIV As String = String.Empty

        ''' <summary>
        ''' 実貸出開始日
        ''' </summary>
        ''' <returns>実貸出開始日</returns>
        Public Property REAL_START_DT As Integer?

        ''' <summary>
        ''' 実貸出終了日
        ''' </summary>
        ''' <returns>実貸出終了日</returns>
        Public Property REAL_END_DT As Integer?

        ''' <summary>
        ''' ユーザー貸出開始日
        ''' </summary>
        ''' <returns>ユーザー貸出開始日</returns>
        Public Property USER_START_DT As Integer?

        ''' <summary>
        ''' ユーザー貸出終了日
        ''' </summary>
        ''' <returns>ユーザー貸出終了日</returns>
        Public Property USER_END_DT As Integer?

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
        ''' 貸出用途
        ''' </summary>
        ''' <returns>貸出用途</returns>
        Public Property LEASE_USE As String = String.Empty

        ''' <summary>
        ''' 送り先名
        ''' </summary>
        ''' <returns>送り先名</returns>
        Public Property DEST_NAME As String = String.Empty

        ''' <summary>
        ''' 送り先住所
        ''' </summary>
        ''' <returns>送り先住所</returns>
        Public Property DEST_ADDRESS As String = String.Empty

        ''' <summary>
        ''' 備考
        ''' </summary>
        ''' <returns>備考</returns>
        Public Property REMARK As String = String.Empty

        ''' <summary>
        ''' 社内用備考
        ''' </summary>
        ''' <returns>社内用備考</returns>
        Public Property REMARK_INTERNAL As String = String.Empty

        ''' <summary>
        ''' 送り状番号
        ''' </summary>
        ''' <returns>送り状番号</returns>
        Public Property DEST_NO As String = String.Empty

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        ''' <returns>資産番号</returns>
        Public Property ASSET_NO As String = String.Empty

        ''' <summary>
        ''' 資産名称
        ''' </summary>
        ''' <returns>資産名称</returns>
        Public Property ASSET_NAME As String = String.Empty

        ''' <summary>
        ''' シリアルNo
        ''' </summary>
        ''' <returns>シリアルNo</returns>
        Public Property SERIAL_NO As String = String.Empty

        ''' <summary>
        ''' 明細_実貸出開始日
        ''' </summary>
        ''' <returns>明細_実貸出開始日</returns>
        Public Property DTL_REAL_START_DT As Integer?

        ''' <summary>
        ''' 明細_実貸出終了日
        ''' </summary>
        ''' <returns>明細_実貸出終了日</returns>
        Public Property DTL_REAL_END_DT As Integer?

        ''' <summary>
        ''' 明細_貸出日
        ''' </summary>
        ''' <returns>明細_貸出日</returns>
        Public Property DTL_LENDING_DT As Integer?

        ''' <summary>
        ''' 明細_返却日
        ''' </summary>
        ''' <returns>明細_返却日</returns>
        Public Property DTL_RETURN_DT As Integer?

        ''' <summary>
        ''' 明細_担当社員コード
        ''' </summary>
        ''' <returns>明細_担当社員コード</returns>
        Public Property DTL_CHARGE_EMP_CD As String = String.Empty

        ''' <summary>
        ''' 明細_担当社員名
        ''' </summary>
        ''' <returns>明細_担当社員名</returns>
        Public Property DTL_CHARGE_EMP_NAME As String = String.Empty

        ''' <summary>
        ''' 固定列(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntTop As Integer

        ''' <summary>
        ''' 予約一覧情報(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoTop As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' 固定列(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntBottom As Integer

        ''' <summary>
        ''' 予約一覧情報(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoBottom As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
