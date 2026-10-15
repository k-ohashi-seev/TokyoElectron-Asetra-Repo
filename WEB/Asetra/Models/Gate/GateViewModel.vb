Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ゲートマスタメンテ用表示用モデル
    ''' </summary>
    Partial Public Class GateViewModel
        Inherits BaseModel
        ''' <summary>
        ''' ゲートコード
        ''' </summary>
        ''' <returns>ゲートコード</returns>
        <Key>
        <Column(Order:=1)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(10, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property GATE_CD As String = String.Empty

        ''' <summary>
        ''' ゲート名
        ''' </summary>
        ''' <returns>ゲート名</returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(100, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property GATE_NAME As String = String.Empty

        ''' <summary>
        ''' ゲートIPアドレス
        ''' </summary>
        ''' <returns>ゲートIPアドレス</returns>
        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property GATE_IPADDRESS As String = String.Empty

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
        Public Property PATLITE_VALID_FLAG As String = String.Empty

        ''' <summary>
        ''' ゲート有効フラグ
        ''' </summary>
        ''' <returns>ゲート有効フラグ</returns>
        <Required>
        <StringLength(1)>
        Public Property GATE_VALID_FLAG As String = String.Empty

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
        ''' 表示項目：アンテナ１有効フラグ
        ''' </summary>
        ''' <returns>表示項目：アンテナ有効フラグ</returns>
        <NotMapped>
        Public Property ANTENNA1_VALID_FLAG As String = String.Empty

        ''' <summary>
        ''' 表示項目：アンテナ１電波強度
        ''' </summary>
        ''' <returns>表示項目：アンテナ１電波強度</returns>
        <NotMapped>
        <Range(1, 81, ErrorMessage:=CommonLabel.MSG_ERR_INPUT_RANGE)>
        Public Property ANTENNA1_RF_POWER As String = String.Empty

        ''' <summary>
        ''' 表示項目：アンテナ２有効フラグ
        ''' </summary>
        ''' <returns>表示項目：アンテナ２有効フラグ</returns>
        <NotMapped>
        Public Property ANTENNA2_VALID_FLAG As String = String.Empty

        ''' <summary>
        ''' 表示項目：アンテナ２電波強度
        ''' </summary>
        ''' <returns>表示項目：アンテナ２電波強度</returns>
        <NotMapped>
        <Range(1, 81, ErrorMessage:=CommonLabel.MSG_ERR_INPUT_RANGE)>
        Public Property ANTENNA2_RF_POWER As String = String.Empty

        ''' <summary>
        ''' 表示項目：アンテナ３有効フラグ
        ''' </summary>
        ''' <returns>表示項目：アンテナ３有効フラグ</returns>
        <NotMapped>
        Public Property ANTENNA3_VALID_FLAG As String = String.Empty

        ''' <summary>
        ''' 表示項目：アンテナ３電波強度
        ''' </summary>
        ''' <returns>表示項目：アンテナ３電波強度</returns>
        <NotMapped>
        <Range(1, 81, ErrorMessage:=CommonLabel.MSG_ERR_INPUT_RANGE)>
        Public Property ANTENNA3_RF_POWER As String = String.Empty

        ''' <summary>
        ''' 表示項目：アンテナ４有効フラグ
        ''' </summary>
        ''' <returns>表示項目：アンテナ４有効フラグ</returns>
        <NotMapped>
        Public Property ANTENNA4_VALID_FLAG As String = String.Empty

        ''' <summary>
        ''' 表示項目：アンテナ４電波強度
        ''' </summary>
        ''' <returns>表示項目：アンテナ４電波強度</returns>
        <NotMapped>
        <Range(1, 81, ErrorMessage:=CommonLabel.MSG_ERR_INPUT_RANGE)>
        Public Property ANTENNA4_RF_POWER As String = String.Empty

        ''' <summary>
        ''' 表示項目：サーモセンサ１有効フラグ
        ''' </summary>
        ''' <returns>表示項目：サーモセンサ１有効フラグ</returns>
        Public Property THERMO1_VALID_FLAG As String = String.Empty

        ''' <summary>
        ''' 表示項目：サーモセンサ２有効フラグ
        ''' </summary>
        ''' <returns>表示項目：サーモセンサ２有効フラグ</returns>
        Public Property THERMO2_VALID_FLAG As String = String.Empty

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

        '20220328 ADD Start Kawamoto v2.1
        ''' <summary>
        ''' ゲート区分
        ''' </summary>
        ''' <returns>ゲート区分</returns>
        <Key>
        <Column(Order:=0)>
        <StringLength(1, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property GATE_DIV As String = String.Empty

        ''' <summary>
        ''' カメラ有効フラグ
        ''' </summary>
        ''' <returns>カメラ有効フラグ</returns>
        <StringLength(1, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CAMERA_VALID_FLAG As String = String.Empty

        ''' <summary>
        ''' 感度
        ''' </summary>
        ''' <returns>感度</returns>
        <StringLength(5, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property SENSITIVITY As String = String.Empty

        ''' <summary>
        ''' セッション
        ''' </summary>
        ''' <returns>セッション</returns>
        <StringLength(1, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        <Range(0, 3, ErrorMessage:=CommonLabel.MSG_ERR_INPUT_RANGE)>
        Public Property SESSION As String = String.Empty

        ''' <summary>
        ''' 周波数1 
        ''' </summary>
        ''' <returns>周波数1 </returns>
        Public Property FREQUENCY1 As String

        ''' <summary>
        ''' 周波数2 
        ''' </summary>
        ''' <returns>周波数2 </returns>
        Public Property FREQUENCY2 As String

        ''' <summary>
        ''' 周波数3 
        ''' </summary>
        ''' <returns>周波数3 </returns>
        Public Property FREQUENCY3 As String

        ''' <summary>
        ''' 周波数4 
        ''' </summary>
        ''' <returns>周波数4 </returns>
        Public Property FREQUENCY4 As String

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
        <Range(1, 65535, ErrorMessage:=CommonLabel.MSG_ERR_INPUT_RANGE)>
        Public Property TAG_POPULATION As String = String.Empty

        ''' <summary>
        ''' 機器名(カメラ用)
        ''' </summary>
        ''' <returns>機器名(カメラ用)</returns>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEVICE_NAME_C As String = String.Empty

        ''' <summary>
        ''' bit位置(カメラ用)
        ''' </summary>
        ''' <returns>ポート番号(カメラ用)</returns>
        <NotMapped>
        <Range(0, 7, ErrorMessage:=CommonLabel.MSG_ERR_INPUT_RANGE)>
        Public Property BIT_POS_C As String = String.Empty

        ''' <summary>
        ''' 機器名(パトライト用)
        ''' </summary>
        ''' <returns>機器名(パトライト用)</returns>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEVICE_NAME_P As String = String.Empty

        ''' <summary>
        ''' bit位置(パトライト用)
        ''' </summary>
        ''' <returns>ポート番号(パトライト用)</returns>
        <NotMapped>
        <Range(0, 7, ErrorMessage:=CommonLabel.MSG_ERR_INPUT_RANGE)>
        Public Property BIT_POS_P As String = String.Empty

        ''' <summary>
        ''' 一覧表示用周波数
        ''' </summary>
        ''' <returns>一覧表示用周波数</returns>
        Public Property DISP_FREQUENCY As String = String.Empty

        ''' <summary>
        ''' 一覧表示用電波強度
        ''' </summary>
        ''' <returns>一覧表示用電波強度</returns>
        Public Property DISP_ANTENNA_POWER As Integer? = Nothing

        ''' <summary>
        ''' 無条件で発報フラグ(カメラ)
        ''' </summary>
        ''' <returns>無条件で発報フラグ(カメラ)</returns>
        Public Property UNCONDITIONAL_C As String = String.Empty

        ''' <summary>
        ''' 無条件で発報フラグ(パトライト)
        ''' </summary>
        ''' <returns>無条件で発報フラグ(パトライト)</returns>
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

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer


        ''' <summary>
        ''' ゲートマスタ情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
