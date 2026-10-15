Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ゲート持ち出し履歴モデル
    ''' </summary>
    <Table("T_GATE_PASS_LOG")>
    Partial Public Class GatePassLogModel
        <Key>
        Public Property GATE_LOG_ID As Long

        Public Property GATE_PASS_DT As Integer

        <Required>
        <StringLength(9)>
        Public Property GATE_PASS_TM As String

        <Required>
        <StringLength(10)>
        Public Property GATE_CD As String

        <Required>
        <StringLength(100)>
        Public Property GATE_NAME As String

        Public Property ANTENNA_NO As Integer
        Public Property THERMO_NO As Integer

        <Required>
        <StringLength(2)>
        Public Property GATE_PASS_DIV As String

        <StringLength(24)>
        Public Property ASSET_NO As String

        <StringLength(128)>
        Public Property ASSET_ID As String

        <StringLength(60)>
        Public Property ASSET_NAME As String

        <StringLength(60)>
        Public Property MAKER As String

        <StringLength(30)>
        Public Property PRODUCT_NO As String

        <StringLength(30)>
        Public Property MODEL_NO As String

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
    End Class
End Namespace
