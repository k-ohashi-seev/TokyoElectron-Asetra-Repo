Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ロケーションマスタ
    ''' </summary>
    <Table("M_LOCATION")>
    Partial Public Class LocationModel
        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property LOC_DIV As String = String.Empty

        <Key>
        <Column(Order:=1)>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String = String.Empty

        <Key>
        <Column(Order:=2)>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String = String.Empty

        <Key>
        <Column(Order:=3)>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String = String.Empty

        <StringLength(60)>
        Public Property LOC_NAME As String = String.Empty

        <StringLength(1)>
        Public Property LOC_SHARE_FLG As String = String.Empty

        <StringLength(1000)>
        Public Property MAIL_SEND_ADDRESS As String = String.Empty

        Public Property MAINTE_LEASE_NUM As Integer? = Nothing

        Public Property MAINTE_LEASE_DAY As Integer? = Nothing

        <StringLength(1)>
        Public Property MAINTE_LEASE_DAY_WORK_FLAG As String = String.Empty

        Public Property MAINTE_DAY As Integer? = Nothing

        Public Property SPECIFIED_QTY As Decimal? = Nothing             '2023/02/08 ADD Shirakawa

        <StringLength(1)>
        Public Property MAINTE_DAY_WORK_FLAG As String = String.Empty

        Public Property LOC_RFID_FLG As String = String.Empty   '2025/10/14 ADD Kudo RFIDタグ拠点

        Public Property OWN_INV_FLG As String = String.Empty
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

        ''' <summary>
        ''' 登録日時 文字列
        ''' </summary>
        ''' <returns>登録日時 文字列</returns>
        <NotMapped>
        Public Property InsDatetimeString As String = String.Empty

        ''' <summary>
        ''' 最終登録更新日時 文字列
        ''' </summary>
        ''' <returns>最終登録更新日時 文字列</returns>
        <NotMapped>
        Public Property UpdDatetimeString As String = String.Empty

        ''' <summary>
        ''' ロケ大区分名
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property LocLName As String = String.Empty

        ''' <summary>
        ''' ロケ中区分名
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property LocMName As String = String.Empty

        ''' <summary>
        ''' ロケ小区分名
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property LocSName As String = String.Empty
    End Class

End Namespace
