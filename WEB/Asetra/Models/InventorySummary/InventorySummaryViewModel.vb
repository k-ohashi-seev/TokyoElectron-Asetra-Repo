Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class InventorySummaryViewModel
        Inherits BaseModel

        <Key>
        <Column(Order:=0)>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String

        <StringLength(60)>
        Public Property LOC_L_NAME As String

        '更新可否フラグ
        Public Property UPD_FLG As String

        '棚卸名
        Public Property INV_NAME_LIST As String = ""

        '棚卸ID
        Public Property INV_ID As Int64

        '<Key>
        '<Column(Order:=1)>
        '<StringLength(10)>      '20210510 UPD Suzuki v1.3
        'Public Property LOC_M_CD As String

        '<StringLength(60)>
        'Public Property LOC_M_NAME As String

        '<Key>
        '<Column(Order:=2)>
        '<StringLength(10)>      '20210510 UPD Suzuki v1.3
        'Public Property LOC_S_CD As String

        '<StringLength(60)>
        'Public Property LOC_S_NAME As String

        '20231002 DEL Start Shirakawa
        'Public Property NUMBER_OF_ASSETS As Int64? = 0
        'Public Property NUMBER_OF_STOCKS As Int64? = 0
        'Public Property NUMBER_OF_LOANS As Int64? = 0
        'Public Property REAL_SHELF As Int64? = 0
        'Public Property UNCONFIRMED As Int64? = 0
        '20231002 DEL End Shirakawa

        '20231002 ADD Start Shirakawa
        <StringLength(16)>
        Public Property INVENTORY_STATUS As String

        Public Property INVENTORY_STARTDAY As Int64? = Nothing
        '20231002 ADD End Shirakawa

        Public Property BOOK_INVENTORY_NUMBER As Int64? = 0
        Public Property NUMBER_OF_LOANS_WITHIN As Int64? = 0
        Public Property ACTUAL_NUMBER_OF_SHELVES As Int64? = 0
        Public Property NORMAL_NUMBER As Int64? = 0
        Public Property LOCATION_DIFFERENCE_NUMBER As Int64? = 0
        Public Property OUT_OF_STOCK As Int64? = 0

        ''' <summary>
        ''' 直近棚卸日
        ''' </summary>
        ''' <returns></returns>
        Public Property NEAR_INVENTORY_DATE As Date? = Nothing

        <NotMapped>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property INV_NAME As String = ""

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
        ''' 検索条件 棚卸未実施
        ''' </summary>
        ''' <returns>検索条件 棚卸未実施</returns>
        <NotMapped>
        Public Property SearchNoStockTaking As Boolean = False

        ''' <summary>
        '''  (ロケーション)大区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>(ロケーション)大区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locLDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  (ロケーション)中区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>(ロケーション)中区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locMDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  (ロケーション)小区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>(ロケーション)小区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locSDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        ''' CSVパス
        ''' </summary>
        ''' <returns>CSVパス</returns>
        <NotMapped>
        Public Property CsvPath As String = String.Empty

        '2019/09/10 DELETE inoue
        '202231002 DEL Start Shirakawa
        '<NotMapped>
        'Public Property SHOW_NUMBER_OF_ASSETS As Int64? = 0
        '<NotMapped>
        'Public Property SHOW_NUMBER_OF_STOCKS As Int64? = 0
        '<NotMapped>
        'Public Property SHOW_NUMBER_OF_LOANS As Int64? = 0
        '<NotMapped>
        'Public Property SHOW_REAL_SHELF As Int64? = 0
        '<NotMapped>
        'Public Property SHOW_UNCONFIRMED As Int64? = 0
        '202231002 DEL End Shirakawa

        <NotMapped>
        Public Property INVENTORY_COUNT As Int64? = 0
        <NotMapped>
        Public Property INVENTORY_MESSAGE As String = ""
        <NotMapped>
        Public Property START_DEC_TS As Int64? = 0

        ''' <summary>
        ''' ログイン権限
        ''' </summary>
        ''' <returns>ログイン権限</returns>
        <NotMapped>
        Public Property LOGIN_KENGEN As String = ""

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
