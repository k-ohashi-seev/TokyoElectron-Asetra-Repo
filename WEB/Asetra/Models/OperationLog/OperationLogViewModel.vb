Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' 操作ログ照会用ビューモデル
    ''' </summary>
    Partial Public Class OperationLogViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 操作ログID
        ''' </summary>
        ''' <returns></returns>
        <Key>
        Public Property OPE_LOG_ID As Int64 = 0

        ''' <summary>
        ''' 処理日
        ''' </summary>
        ''' <returns></returns>
        Public Property PROCESS_DT As Integer = 0

        ''' <summary>
        ''' 処理時刻
        ''' </summary>
        ''' <returns></returns>
        Public Property PROCESS_TM As String = ""

        ''' <summary>
        ''' 処理時刻
        ''' </summary>
        ''' <returns></returns>
        Public Property PROCESS_DATETIME As String = ""

        ''' <summary>
        ''' 処理ツール区分
        ''' </summary>
        ''' <returns></returns>
        Public Property PROCESS_TOOL_DIV As String = ""

        ''' <summary>
        ''' 処理種別
        ''' </summary>
        ''' <returns></returns>
        Public Property PROCESS_CAT As String = ""

        ''' <summary>
        ''' ロケ大区分コード
        ''' </summary>
        ''' <returns></returns>
        Public Property LOC_L_CD As String = ""

        ''' <summary>
        ''' ロケ中区分コード
        ''' </summary>
        ''' <returns></returns>
        Public Property LOC_M_CD As String = ""

        ''' <summary>
        ''' ロケ小区分コード
        ''' </summary>
        ''' <returns></returns>
        Public Property LOC_S_CD As String = ""

        ''' <summary>
        ''' ロケ大区分名称
        ''' </summary>
        ''' <returns></returns>
        Public Property LOC_L_NAME As String = ""

        ''' <summary>
        ''' ロケ中区分名称
        ''' </summary>
        ''' <returns></returns>
        Public Property LOC_M_NAME As String = ""

        ''' <summary>
        ''' ロケ小区分名称
        ''' </summary>
        ''' <returns></returns>
        Public Property LOC_S_NAME As String = ""

        ''' <summary>
        ''' 処理内容
        ''' </summary>
        ''' <returns></returns>
        Public Property PROCESS_CONTENTS As String = ""

        ''' <summary>
        ''' 操作者社員コード
        ''' </summary>
        ''' <returns></returns>
        Public Property OPE_EMPLOYEE_CD As String = ""

        ''' <summary>
        ''' 操作者氏名
        ''' </summary>
        ''' <returns></returns>
        Public Property OPE_EMPLOYEE_NAME As String = ""

        ''' <summary>
        ''' 最終登録更新日時
        ''' </summary>
        ''' <returns></returns>
        Public Property UPD_DATETIME As DateTime?

        ''' <summary>
        ''' 最終登録更新者社員コード
        ''' </summary>
        ''' <returns></returns>
        Public Property UPD_EMPLOYEE_CD As String = ""

        ''' <summary>
        ''' 最終登録更新者氏名
        ''' </summary>
        ''' <returns></returns>
        Public Property UPD_EMPLOYEE_NAME As String = ""

        ''' <summary>
        ''' 最終登録更新処理
        ''' </summary>
        ''' <returns></returns>
        Public Property UPD_TASK As String = ""

        ''' <summary>
        '''  (ロケーション)大区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>(ロケーション)大区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locLDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  (ロケーション)中区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>(ロケーション)中区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locMDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  (ロケーション)小区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>(ロケーション)小区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locSDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        ''' 検索条件 期間(From)
        ''' </summary>
        ''' <returns>検索条件 期間(From)</returns>
        <NotMapped>
        Public Property SearchDateFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 期間(To)
        ''' </summary>
        ''' <returns>検索条件 期間(To)</returns>
        <NotMapped>
        Public Property SearchDateTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 ロケ大区分
        ''' </summary>
        ''' <returns>検索条件 ロケ大区分</returns>
        <NotMapped>
        Public Property SearchLocL As String = String.Empty

        ''' <summary>
        ''' 検索条件 ロケ中区分
        ''' </summary>
        ''' <returns>検索条件 ロケ中区分</returns>
        <NotMapped>
        Public Property SearchLocM As String = String.Empty

        ''' <summary>
        ''' 検索条件 ロケ小区分
        ''' </summary>
        ''' <returns>検索条件 ロケ小区分</returns>
        <NotMapped>
        Public Property SearchLocS As String = String.Empty

        ''' <summary>
        ''' 検索条件 処理ツール
        ''' </summary>
        ''' <returns>検索条件 処理ツール</returns>
        <NotMapped>
        Public Property SearchProcessTool As String = String.Empty

        ''' <summary>
        ''' 検索条件 ログイン者名
        ''' </summary>
        ''' <returns>検索条件 ログイン者名</returns>
        <NotMapped>
        Public Property SearchEmployeeName As String = String.Empty

        ''' <summary>
        ''' 検索条件 ログ種別 操作
        ''' </summary>
        ''' <returns>検索条件 ログ種別 操作</returns>
        <NotMapped>
        Public Property SearchProcessCategoryOP As Boolean = True

        ''' <summary>
        ''' 検索条件 ログ種別 イベント
        ''' </summary>
        ''' <returns>検索条件 ログ種別 イベント</returns>
        <NotMapped>
        Public Property SearchProcessCategoryEV As Boolean = True

        ''' <summary>
        ''' 検索条件 ログ種別 エラー
        ''' </summary>
        ''' <returns>検索条件 ログ種別 エラー</returns>
        <NotMapped>
        Public Property SearchProcessCategoryER As Boolean = True

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
