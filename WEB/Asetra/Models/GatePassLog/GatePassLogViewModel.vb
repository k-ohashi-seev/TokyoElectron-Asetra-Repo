Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ゲート持ち出し履歴用表示用モデル
    ''' </summary>
    Partial Public Class GatePassLogViewModel
        Inherits BaseModel
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

        <Required>
        <StringLength(2)>
        Public Property GATE_PASS_DIV As String

        '2020/03/16 出力しないので、コメント化　DELETE Inoue 生かす時は、NULLがあることを考慮すること
        'Public Property ANTENNA_NO As Integer
        'Public Property THERMO_NO As Integer

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

        ''' <summary>
        ''' 検索条件 ゲートコード
        ''' </summary>
        ''' <returns>検索条件 ゲートコード</returns>
        <NotMapped>
        Public Property SearchCode As String = String.Empty

        ''' <summary>
        ''' 検索条件 資産番号
        ''' </summary>
        ''' <returns>検索条件 資産番号</returns>
        <NotMapped>
        Public Property SearchAssetNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 通過日(From)
        ''' </summary>
        ''' <returns>検索条件 通過日(From)</returns>
        <NotMapped>
        Public Property SearchGatePassDtFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 通過日(To)
        ''' </summary>
        ''' <returns>検索条件 通過日(To)</returns>
        <NotMapped>
        Public Property SearchGatePassDtTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 ゲート通過区分 持出/返却エラー
        ''' </summary>
        ''' <returns>検索条件 ゲート通過区分 持出/返却エラー</returns>
        <NotMapped>
        Public Property SearchGatePassDivER As Boolean = True

        ''' <summary>
        ''' 検索条件 ゲート通過区分 入退室
        ''' </summary>
        ''' <returns>検索条件 ゲート通過区分 入退室</returns>
        <NotMapped>
        Public Property SearchGatePassDivIO As Boolean = False

        ''' <summary>
        ''' 検索条件 ゲート通過区分 持出/返却
        ''' </summary>
        ''' <returns>検索条件 ゲート通過区分 持出/返却</returns>
        <NotMapped>
        Public Property SearchGatePassDivLR As Boolean = False

        '20220328 ADD Start Kawamoto v2.1
        ''' <summary>
        ''' ゲート区分
        ''' </summary>
        ''' <returns>ゲート区分</returns>
        <NotMapped>
        Public Property GATE_DIV As String = String.Empty

        ''' <summary>
        ''' 検索条件 ゲート通過区分 システムエラー
        ''' </summary>
        ''' <returns>検索条件 ゲート通過区分 システムエラー</returns>
        <NotMapped>
        Public Property SearchGatePassDivSE As Boolean = True

        ''' <summary>
        ''' エラー内容
        ''' </summary>
        ''' <returns>エラー内容</returns>
        Public Property ERR_CONTENTS As String = String.Empty

        ''' <summary>
        ''' 検索条件 資産名称
        ''' </summary>
        ''' <returns>検索条件 資産名称</returns>
        <NotMapped>
        Public Property SearchAssetName As String = String.Empty

        ''' <summary>
        ''' 処理時刻
        ''' </summary>
        ''' <returns></returns>
        Public Property PROCESS_DATETIME As String = ""
        '20220328 ADD End Kawamoto v2.1

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' ゲート持ち出し履歴情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace