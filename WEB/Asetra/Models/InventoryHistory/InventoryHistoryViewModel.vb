Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class InventoryHistoryViewModel
        Inherits BaseModel

        '

        ''' <summary>
        ''' 開始宣言日時
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=0)>
        Public Property START_DEC_TS As Int64

        '20231013 ADD Start Shirakawa
        ''' <summary>
        ''' 棚卸ID
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=1)>
        Public Property INV_ID As Int64

        ''' <summary>
        ''' ロケ大コード
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=2)>
        <StringLength(10)>
        Public Property LOC_L_CD As String

        ''' <summary>
        ''' ロケ大名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property LOC_L_NAME As String

        ''' <summary>
        ''' ロケコード+ロケ名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(71)>
        Public Property LOC_CD_NAME As String


        '20231013 ADD End Shirakawa

        ''' <summary>
        ''' 開始宣言者社員コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(15)>
        Public Property START_DEC_EMPLOYEE_CD As String

        ''' <summary>
        ''' 開始宣言者氏名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property START_DEC_NAME As String

        ''' <summary>
        ''' 棚卸名
        ''' </summary>
        ''' <returns></returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property INV_NAME As String

        ''' <summary>
        ''' 棚卸確定日時
        ''' </summary>
        ''' <returns></returns>
        Public Property INV_CONFIRM_TS As Long? = 0

        ''' <summary>
        ''' 棚卸確定者社員コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(15)>
        Public Property INV_CONFIRM_EMPLOYEE_CD As String

        ''' <summary>
        ''' 棚卸確定者氏名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property INV_CONFIRM_NAME As String

        ''' <summary>
        ''' 帳簿在庫数
        ''' </summary>
        ''' <returns></returns>
        Public Property LOGIC_INV_NUM As Int64? = 0

        ''' <summary>
        ''' 実棚正常数
        ''' </summary>
        ''' <returns></returns>
        Public Property PHYINV_NUM As Int64? = 0

        ''' <summary>
        ''' ロケ違い数
        ''' </summary>
        ''' <returns></returns>
        Public Property LOC_DIFF_NUM As Int64? = 0

        ''' <summary>
        ''' 現物なし数
        ''' </summary>
        ''' <returns></returns>
        Public Property NO_ITEM_NUM As Int64? = 0

        ''' <summary>
        ''' 棚卸全体確定日時
        ''' </summary>
        ''' <returns></returns>
        Public Property INV_ALL_CONFIRM_TS As Int64? = 0

        ''' <summary>
        ''' 最終登録更新日時
        ''' </summary>
        ''' <returns>最終登録更新日時</returns>
        <DataType(DataType.Date)>
        <DisplayFormat(DataFormatString:="{0:yyyy/MM/dd HH:mm}")>
        Public Property UPD_DATETIME As Date? = Nothing

        ''' <summary>
        ''' 最終登録更新者社員コード
        ''' </summary>
        ''' <returns>最終登録更新者社員コード</returns>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String = ""

        ''' <summary>
        ''' 最終登録更新者氏名
        ''' </summary>
        ''' <returns>最終登録更新者氏名</returns>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String = ""

        ''' <summary>
        ''' 最終登録更新処理
        ''' </summary>
        ''' <returns>最終登録更新処理</returns>
        <StringLength(255)>
        Public Property UPD_TASK As String = ""


        ''' <summary>
        ''' 開始宣言日時
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property SelectStartDecTS As String = ""

        ''' <summary>
        ''' 棚卸名
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property SelectInvName As String = ""

        ''' <summary>
        ''' 検索条件 ロケ小区分
        ''' </summary>
        ''' <returns>検索条件 ロケ小区分</returns>
        <NotMapped>
        Public Property SearchLocS As String = ""

        ''' <summary>
        ''' 検索条件 実棚ステータス 全て
        ''' </summary>
        ''' <returns>検索条件 実棚ステータス 全て</returns>
        <NotMapped>
        Public Property SearchPhyinvStatusAll As Boolean = True

        ''' <summary>
        ''' 検索条件 実棚ステータス 正常
        ''' </summary>
        ''' <returns>検索条件 実棚ステータス 正常</returns>
        <NotMapped>
        Public Property SearchPhyinvStatusN As Boolean = False

        ''' <summary>
        ''' 検索条件 実棚ステータス 現物あり
        ''' </summary>
        ''' <returns>検索条件 実棚ステータス 現物あり</returns>
        <NotMapped>
        Public Property SearchPhyinvStatusW As Boolean = False

        ''' <summary>
        ''' 検索条件 実棚ステータス 現物なし
        ''' </summary>
        ''' <returns>検索条件 実棚ステータス 現物なし</returns>
        <NotMapped>
        Public Property SearchPhyinvStatusE As Boolean = False

        ''' <summary>
        ''' 検索条件 資産番号
        ''' </summary>
        ''' <returns>検索条件 資産番号</returns>
        <NotMapped>
        Public Property SearchAssetNo As String = ""

        ''' <summary>
        ''' 検索条件 資産名称
        ''' </summary>
        ''' <returns>検索条件 資産名称</returns>
        <NotMapped>
        Public Property SearchAssetName As String = ""

        ''' <summary>
        ''' 検索条件 メーカ
        ''' </summary>
        ''' <returns>検索条件 メーカ</returns>
        <NotMapped>
        Public Property SearchMaker As String = ""

        ''' <summary>
        ''' 検索条件 品番
        ''' </summary>
        ''' <returns>検索条件 品番</returns>
        <NotMapped>
        Public Property SearchProductNo As String = ""

        ''' <summary>
        ''' 検索条件 型番
        ''' </summary>
        ''' <returns>検索条件 型番</returns>
        <NotMapped>
        Public Property SearchModelNo As String = ""

        ''' <summary>
        ''' CSVパス
        ''' </summary>
        ''' <returns>CSVパス</returns>
        <NotMapped>
        Public Property CsvPath As String = ""

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

        <NotMapped>
        Public Property SelectAssetNo As String

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' カテゴリマスタ情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' 検索条件 ロケ大区分
        ''' </summary>
        ''' <returns>検索条件 ロケ大区分</returns>
        <NotMapped>
        Public Property SearchLocL As String = String.Empty

        ''' <summary>
        ''' 自画面内の検索条件復帰有無
        ''' </summary>
        ''' <returns>自画面内の検索条件復帰有無</returns>
        <NotMapped>
        Public Property IsRestoreOwnSearchConditions As Boolean = False

        ''' <summary>
        ''' ローカルストレージがある場合の初回検索
        ''' </summary>
        ''' <returns>ローカルストレージがある場合の初回検索</returns>
        <NotMapped>
        Public Property FirstSearch As Boolean = False

    End Class

End Namespace
