Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class InventoryLocHistoryViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 開始宣言日時
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=0)>
        Public Property START_DEC_TS As Int64

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
        ''' ロケ大コード
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=1)>
        <StringLength(10)>
        Public Property LOC_L_CD As String

        ''' <summary>
        ''' ロケ大名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property LOC_NAME As String

        ''' <summary>
        ''' 帳簿在庫数
        ''' </summary>
        ''' <returns></returns>
        Public Property BOOK_INVENTORY_NUMBER As Int64? = 0

        ''' <summary>
        ''' 内貸出数
        ''' </summary>
        ''' <returns></returns>
        Public Property NUMBER_OF_LOANS_WITHIN As Int64? = 0

        ''' <summary>
        ''' 実棚数
        ''' </summary>
        ''' <returns></returns>
        Public Property ACTUAL_NUMBER_OF_SHELVES As Int64? = 0

        ''' <summary>
        ''' 正常数
        ''' </summary>
        ''' <returns></returns>
        Public Property NORMAL_NUMBER As Int64? = 0

        ''' <summary>
        ''' ロケ違い数
        ''' </summary>
        ''' <returns></returns>
        Public Property LOCATION_DIFFERENCE_NUMBER As Int64? = 0

        ''' <summary>
        ''' 在庫なし数
        ''' </summary>
        ''' <returns></returns>
        Public Property OUT_OF_STOCK As Int64? = 0

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

        'ヘッダーエリア--------------------------------
        ''' <summary>
        ''' 棚卸開始宣言日時
        ''' </summary>
        <NotMapped>
        Public Property SHOW_START_DEC_TS As Int64? = 0
        ''' <summary>
        ''' 開始宣言者
        ''' </summary>
        <NotMapped>
        Public Property SHOW_START_DEC_NAME As String = ""
        ''' <summary>
        ''' 棚卸名
        ''' </summary>
        <NotMapped>
        Public Property SHOW_INV_NAME As String = ""
        ''' <summary>
        ''' 棚卸確定日時
        ''' </summary>
        <NotMapped>
        Public Property SHOW_INV_CONFIRM_TS As Long? = 0
        ''' <summary>
        ''' 棚卸確定者
        ''' </summary>
        <NotMapped>
        Public Property SHOW_INV_CONFIRM_NAME As String = ""

        '----------------------------------------------

        '''' <summary>
        '''' 棚卸ID
        '''' </summary>
        '<NotMapped>
        'Public Property SEARCH_INV_ID As String = ""

        ''' <summary>
        ''' CSVパス
        ''' </summary>
        ''' <returns>CSVパス</returns>
        <NotMapped>
        Public Property CsvPath As String = ""

        ''' <summary>
        ''' 固定列(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntTop As Integer

        ''' <summary>
        ''' 固定列(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntBottom As Integer

        ''' <summary>
        ''' 棚卸ロケ別実績一覧(上部)情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoTop As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' 棚卸ロケ別実績一覧(下部)情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoBottom As Dictionary(Of String, SlickGridInfo)

    End Class

End Namespace
