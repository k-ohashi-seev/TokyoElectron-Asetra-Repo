Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Public Class AssetLabelPrintViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 資産ラベル発行用モデル
        ''' </summary>

        <Key>
        <StringLength(24)>
        Public Property ASSET_NO As String

        <StringLength(24)>
        Public Property SUB_ASSET_NO As String

        <StringLength(60)>
        Public Property ASSET_NAME As String

        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String

        <StringLength(10)>
        Public Property LOC_L_CD As String

        <StringLength(10)>
        Public Property LOC_M_CD As String

        <StringLength(10)>
        Public Property LOC_S_CD As String

        <StringLength(8)>
        Public Property CAT_L_CD As String

        Public Property ASSET_STATUS As Byte = 0

        <StringLength(255)>
        Public Property FREE_ITEM1 As String

        <StringLength(255)>
        Public Property FREE_ITEM2 As String

        <StringLength(255)>
        Public Property FREE_ITEM3 As String

        <StringLength(255)>
        Public Property FREE_ITEM4 As String

        <StringLength(255)>
        Public Property FREE_ITEM5 As String

        <StringLength(255)>
        Public Property FREE_ITEM6 As String

        <StringLength(255)>
        Public Property FREE_ITEM7 As String

        <StringLength(255)>
        Public Property FREE_ITEM8 As String

        <StringLength(255)>
        Public Property FREE_ITEM9 As String

        <StringLength(255)>
        Public Property FREE_ITEM10 As String

        <StringLength(255)>
        Public Property FREE_ITEM11 As String

        <StringLength(255)>
        Public Property FREE_ITEM12 As String

        <StringLength(255)>
        Public Property FREE_ITEM13 As String

        <StringLength(255)>
        Public Property FREE_ITEM14 As String

        <StringLength(255)>
        Public Property FREE_ITEM15 As String

        <StringLength(255)>
        Public Property FREE_ITEM16 As String

        <StringLength(255)>
        Public Property FREE_ITEM17 As String

        <StringLength(255)>
        Public Property FREE_ITEM18 As String

        <StringLength(255)>
        Public Property FREE_ITEM19 As String

        <StringLength(255)>
        Public Property FREE_ITEM20 As String

        <StringLength(255)>
        Public Property FREE_ITEM21 As String

        <StringLength(255)>
        Public Property FREE_ITEM22 As String

        <StringLength(255)>
        Public Property FREE_ITEM23 As String

        <StringLength(255)>
        Public Property FREE_ITEM24 As String

        <StringLength(255)>
        Public Property FREE_ITEM25 As String

        <StringLength(255)>
        Public Property FREE_ITEM26 As String

        <StringLength(255)>
        Public Property FREE_ITEM27 As String

        <StringLength(255)>
        Public Property FREE_ITEM28 As String

        <StringLength(255)>
        Public Property FREE_ITEM29 As String

        <StringLength(255)>
        Public Property FREE_ITEM30 As String

        <StringLength(255)>
        Public Property FREE_ITEM31 As String

        <StringLength(255)>
        Public Property FREE_ITEM32 As String

        <StringLength(255)>
        Public Property FREE_ITEM33 As String

        <StringLength(255)>
        Public Property FREE_ITEM34 As String

        <StringLength(255)>
        Public Property FREE_ITEM35 As String

        <StringLength(255)>
        Public Property FREE_ITEM36 As String

        <StringLength(255)>
        Public Property FREE_ITEM37 As String

        <StringLength(255)>
        Public Property FREE_ITEM38 As String

        <StringLength(255)>
        Public Property FREE_ITEM39 As String

        <StringLength(255)>
        Public Property FREE_ITEM40 As String

        <StringLength(255)>
        Public Property FREE_ITEM41 As String

        <StringLength(255)>
        Public Property FREE_ITEM42 As String

        <StringLength(255)>
        Public Property FREE_ITEM43 As String

        <StringLength(255)>
        Public Property FREE_ITEM44 As String

        <StringLength(255)>
        Public Property FREE_ITEM45 As String

        <StringLength(255)>
        Public Property FREE_ITEM46 As String

        <StringLength(255)>
        Public Property FREE_ITEM47 As String

        <StringLength(255)>
        Public Property FREE_ITEM48 As String

        <StringLength(255)>
        Public Property FREE_ITEM49 As String

        <StringLength(255)>
        Public Property FREE_ITEM50 As String

        <StringLength(255)>
        Public Property FREE_ITEM51 As String

        <StringLength(255)>
        Public Property FREE_ITEM52 As String

        <StringLength(255)>
        Public Property FREE_ITEM53 As String

        <StringLength(255)>
        Public Property FREE_ITEM54 As String

        <StringLength(255)>
        Public Property FREE_ITEM55 As String

        <StringLength(255)>
        Public Property FREE_ITEM56 As String

        <StringLength(255)>
        Public Property FREE_ITEM57 As String

        <StringLength(255)>
        Public Property FREE_ITEM58 As String

        <StringLength(255)>
        Public Property FREE_ITEM59 As String

        <StringLength(255)>
        Public Property FREE_ITEM60 As String

        <StringLength(1)>
        Public Property IS_TANGIBLE As String

        Public Property IP_DATETIME As Long?

        <StringLength(15)>
        Public Property IP_EMPLOYEE_CD As String

        <StringLength(255)>
        Public Property IP_FILE_NAME As String

        Public Property INS_DATETIME As Date

        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String

        Public Property UPD_DATETIME As Date

        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String

        <StringLength(255)>
        Public Property UPD_TASK As String

        ''' <summary>
        ''' 検索条件 ロケ大区分
        ''' </summary>
        ''' <returns>検索条件 ロケ大区分</returns>
        <NotMapped>
        Public Property SearchLocL As String = String.Empty

        ''' <summary>
        ''' 検索条件 ロケ中区分（複数選択のため配列）
        ''' </summary>
        ''' <returns>検索条件 ロケ中区分</returns>
        <NotMapped>
        Public Property SearchLocM As String() = New String() {}

        ''' <summary>
        ''' 検索条件 ロケ小区分（複数選択のため配列）
        ''' </summary>
        ''' <returns>検索条件 ロケ小区分</returns>
        <NotMapped>
        Public Property SearchLocS As String() = New String() {}

        '[CC431] ADD Start 20261007
        ''' <summary>
        ''' 検索条件 ロケ小区分の「場所から選ぶ」（True：場所で原価センタを絞り込む、False：原価センタで場所を絞り込む）
        ''' </summary>
        ''' <returns>検索条件 場所から選ぶ</returns>
        <NotMapped>
        Public Property SearchLocReverse As Boolean = False
        '[CC431] ADD End 20261007

        ''' <summary>
        ''' 検索条件 カテゴリ大分類（複数選択のため配列）
        ''' </summary>
        ''' <returns>検索条件 カテゴリ大分類</returns>
        <NotMapped>
        Public Property SearchCatL As String() = New String() {}

        ''' <summary>
        ''' 検索条件 有形・無形区分 有形資産
        ''' </summary>
        ''' <returns>検索条件 有形・無形区分 有形資産</returns>
        <NotMapped>
        Public Property SearchIsTangibleY As Boolean = True

        ''' <summary>
        ''' 検索条件 有形・無形区分 無形資産
        ''' </summary>
        ''' <returns>検索条件 有形・無形区分 無形資産</returns>
        <NotMapped>
        Public Property SearchIsTangibleN As Boolean = True

        ''' <summary>
        ''' 検索条件 資産番号（「|」区切りで複数指定）
        ''' </summary>
        ''' <returns>検索条件 資産番号</returns>
        <NotMapped>
        Public Property SearchAssetNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 資産名称
        ''' </summary>
        ''' <returns>検索条件 資産名称</returns>
        <NotMapped>
        Public Property SearchAssetName As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目6（管理社員番号）
        ''' </summary>
        ''' <returns>検索条件 管理社員番号</returns>
        <NotMapped>
        Public Property SearchFreeItem6 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目15（資産グループコード）
        ''' </summary>
        ''' <returns>検索条件 資産グループコード</returns>
        <NotMapped>
        Public Property SearchFreeItem15 As String = String.Empty

        ''' <summary>
        ''' 検索条件 取込日From
        ''' </summary>
        ''' <returns>検索条件 リース期限期間From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchIpDateFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 取込日To
        ''' </summary>
        ''' <returns>検索条件 リース期限期間To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchIpDateTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 取込担当者コード
        ''' </summary>
        ''' <returns>検索条件 管理社員番号</returns>
        <NotMapped>
        Public Property SearchInsEmployeeCd As String = String.Empty

        ''' <summary>
        ''' 検索条件 並び順
        ''' </summary>
        ''' <returns>検索条件 並び順（AssetLabelPrintSort）</returns>
        ''' <remarks>初期値は「資産クラス昇順」。第2ソートキーは資産番号昇順（ORDER句の組み立て側で付与）。</remarks>
        <NotMapped>
        Public Property SearchSortOrder As Integer = AssetLabelPrintSort.CatLAsc

        ''' <summary>
        ''' 管理者社員名(フリー項目6のコードから社員Mを検索した社員名)
        ''' </summary>
        ''' <returns>管理者社員名</returns>
        ''' <remarks>SQLの別名(FREE_ITEM6_NAME)で取得するため属性は付けない。</remarks>
        Public Property FREE_ITEM6_NAME As String = String.Empty

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
        Public Property LocLName As String = String.Empty

        ''' <summary>
        ''' ロケ中区分名
        ''' </summary>
        ''' <returns>ロケ中区分名</returns>
        Public Property LocMName As String = String.Empty

        ''' <summary>
        ''' ロケ小区分名
        ''' </summary>
        ''' <returns>ロケ小区分名</returns>
        Public Property LocSName As String = String.Empty

        ''' <summary>
        '''  カテゴリー　大分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>大分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catLDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        ''' カテゴリ大分類名
        ''' </summary>
        ''' <returns>カテゴリ大分類名</returns>
        Public Property CatLName As String = String.Empty

        ''' <summary>
        ''' 取込担当者名
        ''' </summary>
        ''' <returns>取込担当者名</returns>
        Public Property IpEmployeeName As String = String.Empty

        ''' <summary>
        ''' プリンタ情報KEY1
        ''' </summary>
        ''' <returns>プリンタ情報KEY1</returns>
        <NotMapped>
        Public Property PrinterKey As String = String.Empty

        ''' <summary>
        ''' 非表示項目：メニュー開閉フラグ
        ''' </summary>
        ''' <returns>非表示項目：メニュー開閉フラグ</returns>
        <NotMapped>
        Public Property OpenFlg As String = String.Empty

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

        ''' <summary>
        ''' 管理ロケーション名
        ''' </summary>
        ''' <returns>表示項目：管理ロケーション名</returns>
        Public Property MngLocLName As String = String.Empty

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

        ''' <summary>
        ''' チェックボックスを有形/無形で初期化するかどうか
        ''' </summary>
        ''' <returns>チェックボックスを有形/無形で初期化するかどうか</returns>
        <NotMapped>
        Public Property needToInitCheckbox As Boolean = True

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' 項目配置情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)
    End Class
End Namespace
