Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' ロケーションマスタメンテ用表示用モデル
    ''' </summary>
    Partial Public Class LocationViewModel
        Inherits BaseModel

        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property LOC_DIV As String = String.Empty

        <Key>
        <Column(Order:=1)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(10, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>     '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String = String.Empty

        <Key>
        <Column(Order:=2)>
        <StringLength(10, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>     '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String = String.Empty

        <Key>
        <Column(Order:=3)>
        <StringLength(10, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>     '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String = String.Empty

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property LOC_NAME As String = String.Empty

        <StringLength(60)>
        Public Property LOC_L_NAME As String = String.Empty
        <StringLength(60)>
        Public Property LOC_M_NAME As String = String.Empty
        <StringLength(60)>
        Public Property LOC_S_NAME As String = String.Empty

        ''' <summary>
        ''' 共通拠点フラグ  '2025/10/14 DEL Kudo 
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property LOC_SHARE_FLG As String = String.Empty

        ''' <summary>
        '''  RFIDタグ拠点  '2025/10/14 ADD Kudo RFIDタグ拠点
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property LOC_RFID_FLG As String = String.Empty

        ''' <summary>
        ''' 管理資産棚卸区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property OWN_INV_FLG As String = String.Empty

        ''' <summary>
        ''' メール送信先
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1000)>
        Public Property MAIL_SEND_ADDRESS As String = String.Empty

        ''' <summary>
        ''' メンテ発生貸出回数
        ''' </summary>
        ''' <returns></returns>
        Public Property MAINTE_LEASE_NUM As Integer? = Nothing

        ''' <summary>
        ''' メンテ発生貸出日数
        ''' </summary>
        ''' <returns></returns>
        Public Property MAINTE_LEASE_DAY As Integer? = Nothing

        ''' <summary>
        ''' メンテ発生貸出日数稼働日フラグ
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property MAINTE_LEASE_DAY_WORK_FLAG As String = String.Empty

        ''' <summary>
        ''' メンテ日数
        ''' </summary>
        ''' <returns></returns>
        Public Property MAINTE_DAY As Integer? = Nothing

        ''' <summary>
        ''' メンテ日数稼働日フラグ
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property MAINTE_DAY_WORK_FLAG As String = String.Empty

        '2023/02/08 ADD Start Shirakawa
        ''' <summary>
        ''' 指定数量
        ''' </summary>
        ''' <returns></returns>
        Public Property SPECIFIED_QTY As Decimal? = Nothing
        '2023/02/08 ADD End Shirakawa

        ''' <summary>
        ''' 登録日時
        ''' </summary>
        ''' <returns>登録日時</returns>
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
        ''' 検索条件 表示指定
        ''' </summary>
        ''' <returns>検索条件 表示指定</returns>
        <NotMapped>
        Public Property SearchDispSelect As String = String.Empty

        ''' <summary>
        '''  大区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>大区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locLDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  中区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>中区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locMDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  小区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>小区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locSDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        ''' ロケ大区分名
        ''' </summary>
        ''' <returns>ロケ大区分名</returns>
        <NotMapped>
        Public Property DispLocLName As String = String.Empty

        ''' <summary>
        ''' ロケ中区分名
        ''' </summary>
        ''' <returns>ロケ中区分名</returns>
        <NotMapped>
        Public Property DispLocMName As String = String.Empty

        ''' <summary>
        ''' ロケ小区分名
        ''' </summary>
        ''' <returns>ロケ小区分名</returns>
        <NotMapped>
        Public Property DispLocSName As String = String.Empty

        ''' <summary>
        ''' プリンタ情報KEY1
        ''' </summary>
        ''' <returns>プリンタ情報KEY1</returns>
        <NotMapped>
        Public Property PrinterKey As String = String.Empty

        ''' <summary>
        ''' 登録変更画面　コンボボックス表示 ロケ大区分
        ''' </summary>
        ''' <returns>登録変更画面 ロケ大区分</returns>
        <NotMapped>
        Public Property ModalLocL As String = String.Empty

        ''' <summary>
        ''' 登録変更画面　コンボボックス表示 ロケ中区分
        ''' </summary>
        ''' <returns>登録変更画面 ロケ中区分</returns>
        <NotMapped>
        Public Property ModalLocM As String = String.Empty

        ''' <summary>
        ''' 変更対象ロケ大区分
        ''' </summary>
        ''' <returns>変更対象ロケ大区分</returns>
        <NotMapped>
        Public Property EditLocLCd As String = String.Empty

        '--2020/02/12 miyaji add start
        ''' <summary>
        ''' 選択キーリスト(カンマ区切り)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property SelectKeys As String

        ''' <summary>
        ''' 未選択キーリスト(カンマ区切り)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property NoneSelectKeys As String
        '--2020/02/12 miyaji add end

        ''' <summary>
        ''' 共通拠点フラグ　チェックボックス表示用
        ''' </summary>
        ''' <returns>共通拠点フラグ　チェックボックス表示用</returns>
        <NotMapped>
        Public Property LocShareFlg As Boolean = False

        ''' <summary>
        ''' RFIDタグ拠点フラグ　チェックボックス表示用　'2025/10/14 ADD Kudo
        ''' </summary>
        ''' <returns>RFIDタグ拠点フラグ　チェックボックス表示用</returns>
        <NotMapped>
        Public Property LocRfidFlg As Boolean = False

        ''' <summary>
        '''管理資産棚卸区分　チェックボックス表示用
        ''' </summary>
        ''' <returns>管理資産棚卸区分　チェックボックス表示用</returns>
        <NotMapped>
        Public Property OwnInvFlg As Boolean = False

        ''' <summary>
        ''' メンテ発生貸出日数稼働日フラグ　チェックボックス表示用
        ''' </summary>
        ''' <returns>メンテ発生貸出日数稼働日フラグ　チェックボックス表示用</returns>
        <NotMapped>
        Public Property MainteLeaseDayWorkFlg As Boolean = False

        ''' <summary>
        ''' メンテ日数稼働日フラグ　チェックボックス表示用
        ''' </summary>
        ''' <returns>メンテ日数稼働日フラグ　チェックボックス表示用</returns>
        <NotMapped>
        Public Property MainteDayWorkFlg As Boolean = False


        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' ロケマスタ情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

        '検索時にロケ大区分の検索条件を保存2025/10/15 Kudo
        ''' <summary>
        ''' 自画面内の検索条件復帰有無
        ''' </summary>
        ''' <returns>自画面内の検索条件復帰有無</returns>
        <NotMapped>
        Public Property IsRestoreOwnSearchConditions As Boolean = False
        '検索時にロケ大区分の検索条件を保存2025/10/15 Kudo
        ''' <summary>
        ''' ローカルストレージがある場合の初回検索
        ''' </summary>
        ''' <returns>ローカルストレージがある場合の初回検索</returns>
        <NotMapped>
        Public Property FirstSearch As Boolean = False
    End Class

End Namespace

