Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' ロケーションマスタ(CSV用)
    ''' </summary>
    Partial Public Class LocationCsvModel

        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property LOC_DIV As String

        <Key>
        <Column(Order:=1)>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String

        <Key>
        <Column(Order:=2)>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String

        <Key>
        <Column(Order:=3)>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String

        <StringLength(60)>
        Public Property LOC_L_NAME As String

        <StringLength(60)>
        Public Property LOC_M_NAME As String

        <StringLength(60)>
        Public Property LOC_S_NAME As String

        <StringLength(1)>
        Public Property LOC_SHARE_FLG As String = String.Empty

        '2025/10/14 ADD Kudo RFIDタグ拠点
        Public Property LOC_RFID_FLG As String = String.Empty
        Public Property OWN_INV_FLG As String = String.Empty
        <StringLength(1000)>
        Public Property MAIL_SEND_ADDRESS As String = String.Empty

        Public Property MAINTE_LEASE_NUM As Integer? = Nothing

        Public Property MAINTE_LEASE_DAY As Integer? = Nothing

        <StringLength(1)>
        Public Property MAINTE_LEASE_DAY_WORK_FLAG As String = String.Empty

        Public Property MAINTE_DAY As Integer? = Nothing

        Public Property SPECIFIED_QTY As Decimal? = Nothing   '2023/02/08 ADD Shirakawa

        <StringLength(1)>
        Public Property MAINTE_DAY_WORK_FLAG As String = String.Empty

    End Class
End Namespace
