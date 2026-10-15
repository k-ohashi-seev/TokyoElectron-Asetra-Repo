Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 予約照会メンテナンスCSV出力用モデル
    ''' </summary>
    Public Class LendingRefMainteCsvModel

        ''' <summary>
        ''' カテゴリー大分類コード
        ''' </summary>
        ''' <returns>カテゴリー大分類コード</returns>
        Public Property CAT_L_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリー中分類コード
        ''' </summary>
        ''' <returns>カテゴリー中分類コード</returns>
        Public Property CAT_M_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリー小分類コード
        ''' </summary>
        ''' <returns>カテゴリー小分類コード</returns>
        Public Property CAT_S_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリー大分類名称
        ''' </summary>
        ''' <returns>カテゴリー大分類名称</returns>
        Public Property CAT_L_NAME As String = String.Empty

        ''' <summary>
        ''' カテゴリー中分類名称
        ''' </summary>
        ''' <returns>カテゴリー中分類名称</returns>
        Public Property CAT_M_NAME As String = String.Empty

        ''' <summary>
        ''' カテゴリー小分類名称
        ''' </summary>
        ''' <returns>カテゴリー小分類名称</returns>
        Public Property CAT_S_NAME As String = String.Empty

        ''' <summary>
        ''' 管理ロケーションコード
        ''' </summary>
        ''' <returns>管理ロケーションコード</returns>
        Public Property MNG_LOC_L_CD As String = String.Empty

        ''' <summary>
        ''' 管理ロケーション名
        ''' </summary>
        ''' <returns>管理ロケーション名</returns>
        Public Property MNG_LOC_L_NAME As String = String.Empty

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        ''' <returns></returns>
        <Key>
        Public Property ASSET_NO As String

        ''' <summary>
        ''' 資産名称
        ''' </summary>
        ''' <returns></returns>
        Public Property ASSET_NAME As String = String.Empty

        '2022/08/31 Add Start Kawamoro
        ''' <summary>
        ''' 型番
        ''' </summary>
        ''' <returns></returns>
        Public Property MODEL_NO As String = String.Empty
        '2022/08/31 Add End Kawamoro

        ''' <summary>
        ''' 貸出状態
        ''' </summary>
        ''' <returns>貸出状態</returns>
        Public Property LEASE_STATUS As Byte = 0

        ''' <summary>
        ''' メンテナンス開始日
        ''' </summary>
        ''' <returns>メンテナンス開始日</returns>
        Public Property MAINTE_START_DT As Integer = 0

        ''' <summary>
        ''' メンテナンス終了日
        ''' </summary>
        ''' <returns>メンテナンス終了日</returns>
        Public Property MAINTE_END_DT As Integer = 0

        ''' <summary>
        ''' 次予約番号
        ''' </summary>
        ''' <returns>次予約番号</returns>
        Public Property NEXT_RESERVE_ID As Long?

        ''' <summary>
        ''' 次貸出予定日
        ''' </summary>
        ''' <returns>次貸出予定日</returns>
        Public Property NEXT_ST_DT As Integer?

    End Class
End Namespace
