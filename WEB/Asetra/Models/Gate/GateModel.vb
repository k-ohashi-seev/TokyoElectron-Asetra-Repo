Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ゲートマスタモデル
    ''' </summary>
    <Table("M_GATE")>
    Partial Public Class GateModel
        ''' <summary>
        ''' ゲートコード
        ''' </summary>
        ''' <returns>ゲートコード</returns>
        <Key>
        <Column(Order:=1)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(10, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property GATE_CD As String

        ''' <summary>
        ''' ゲート名
        ''' </summary>
        ''' <returns>ゲート名</returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(100, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property GATE_NAME As String

        ''' <summary>
        ''' ゲートIPアドレス
        ''' </summary>
        ''' <returns>ゲートIPアドレス</returns>
        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property GATE_IPADDRESS As String

        ''' <summary>
        ''' ゲートチャンネル
        ''' </summary>
        ''' <returns>ゲートチャンネル</returns>
        Public Property GATE_CHANNEL As Integer? = Nothing

        ''' <summary>
        ''' パトライト有効フラグ
        ''' </summary>
        ''' <returns>パトライト有効フラグ</returns>
        <Required>
        <StringLength(1)>
        Public Property PATLITE_VALID_FLAG As String

        ''' <summary>
        ''' ゲート有効フラグ
        ''' </summary>
        ''' <returns>ゲート有効フラグ</returns>
        <Required>
        <StringLength(1)>
        Public Property GATE_VALID_FLAG As String

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

        '20220328 ADD Start Kawamoto v2.1
        ''' <summary>
        ''' ゲート区分
        ''' </summary>
        ''' <returns>ゲート区分</returns>
        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property GATE_DIV As String = String.Empty

        ''' <summary>
        ''' カメラ有効フラグ
        ''' </summary>
        ''' <returns>カメラ有効フラグ</returns>
        <Required>
        <StringLength(1)>
        Public Property CAMERA_VALID_FLAG As String = String.Empty

        ''' <summary>
        ''' 感度
        ''' </summary>
        ''' <returns>感度</returns>
        <StringLength(5)>
        Public Property SENSITIVITY As String = String.Empty

        ''' <summary>
        ''' セッション
        ''' </summary>
        ''' <returns>セッション</returns>
        <StringLength(1)>
        Public Property SESSION As String = String.Empty

        ''' <summary>
        ''' Searchモード
        ''' </summary>
        ''' <returns>Searchモード</returns>
        Public Property SEARCH_MODE As Integer? = Nothing

        ''' <summary>
        ''' Readerモード
        ''' </summary>
        ''' <returns>Readerモード</returns>
        Public Property READER_MODE As Integer? = Nothing

        ''' <summary>
        ''' Tag Population
        ''' </summary>
        ''' <returns>Tag Population</returns>
        <StringLength(5, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TAG_POPULATION As String = String.Empty

        ''' <summary>
        ''' 機器名(カメラ用)
        ''' </summary>
        ''' <returns>機器名(カメラ用)</returns>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEVICE_NAME_C As String = String.Empty

        ''' <summary>
        ''' ポート番号(カメラ用)
        ''' </summary>
        ''' <returns>ポート番号(カメラ用)</returns>
        Public Property BIT_POS_C As Integer? = Nothing

        ''' <summary>
        ''' 機器名(パトライト用)
        ''' </summary>
        ''' <returns>機器名(パトライト用)</returns>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEVICE_NAME_P As String = String.Empty

        ''' <summary>
        ''' ポート番号(パトライト用)
        ''' </summary>
        ''' <returns>ポート番号(パトライト用)</returns>
        Public Property BIT_POS_P As Integer? = Nothing

        ''' <summary>
        ''' 周波数1
        ''' </summary>
        ''' <returns>周波数1</returns>
        Public Property FREQUENCY1 As Decimal? = Nothing

        ''' <summary>
        ''' 周波数2
        ''' </summary>
        ''' <returns>周波数2</returns>
        Public Property FREQUENCY2 As Decimal? = Nothing

        ''' <summary>
        ''' 周波数3
        ''' </summary>
        ''' <returns>周波数3</returns>
        Public Property FREQUENCY3 As Decimal? = Nothing

        ''' <summary>
        ''' 周波数4
        ''' </summary>
        ''' <returns>周波数4</returns>
        Public Property FREQUENCY4 As Decimal? = Nothing

        ''' <summary>
        ''' 無条件で発報(カメラ)有効フラグ
        ''' </summary>
        ''' <returns>無条件で発報(カメラ)有効フラグ</returns>
        <Required>
        <StringLength(1)>
        Public Property UNCONDITIONAL_C As String = String.Empty

        ''' <summary>
        ''' 無条件で発報(パトライト)有効フラグ
        ''' </summary>
        ''' <returns>無条件で発報(パトライト)有効フラグ</returns>
        <Required>
        <StringLength(1)>
        Public Property UNCONDITIONAL_P As String = String.Empty
        '20220328 ADD End Kawamoto v2.1

        '20220511 ADD Start Kawamoto
        ''' <summary>
        ''' 機種
        ''' </summary>
        ''' <returns>機種</returns>
        Public Property DEVICE_TYPE As String = String.Empty

        ''' <summary>
        ''' GPIO機器
        ''' </summary>
        ''' <returns>機種</returns>
        Public Property GPIO_DEVICE As String = String.Empty

        ''' <summary>
        ''' GPOポート(カメラ)
        ''' </summary>
        ''' <returns>GPOポート(カメラ)</returns>
        Public Property GPO_PORT_C As Integer? = Nothing

        ''' <summary>
        ''' GPOポート(パトライト)
        ''' </summary>
        ''' <returns>GPOポート(パトライト)</returns>
        Public Property GPO_PORT_P As Integer? = Nothing
        '20220511 ADD End Kawamoto

    End Class
End Namespace
