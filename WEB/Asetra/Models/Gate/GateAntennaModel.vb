Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ゲートアンテナマスタモデル
    ''' </summary>
    <Table("M_GATE_ANTENNA")>
    Partial Public Class GateAntennaModel
        ''' <summary>
        ''' ゲート区分
        ''' </summary>
        ''' <returns>ゲート区分</returns>
        <Key>
        <Column(Order:=0)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(1)>
        Public Property GATE_DIV As String = String.Empty

        ''' <summary>
        ''' ゲートコード
        ''' </summary>
        ''' <returns>ゲートコード</returns>
        <Key>
        <Column(Order:=1)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(10)>
        Public Property GATE_CD As String = String.Empty

        ''' <summary>
        ''' アンテナ連番
        ''' </summary>
        ''' <returns>アンテナ連番</returns>
        <Key>
        <Column(Order:=2)>
        <Required>
        Public Property ANTENNA_NO As Integer = 0

        ''' <summary>
        ''' アンテナ有効フラグ
        ''' </summary>
        ''' <returns>アンテナ有効フラグ</returns>
        <Required>
        <StringLength(1)>
        Public Property ANTENNA_VALID_FLAG As String = String.Empty

        ''' <summary>
        ''' アンテナ電波強度
        ''' </summary>
        ''' <returns>アンテナ電波強度</returns>
        Public Property ANTENNA_RF_POWER As Integer? = Nothing

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
    End Class
End Namespace
