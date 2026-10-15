Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class StockListViewModel
        Inherits BaseModel

        ''' <summary>
        ''' レコード番号
        ''' </summary>
        ''' <returns></returns>
        <Key>
        Public Property SEQNO As Int64

        ''' <summary>
        ''' キー(資産一覧へのパラメータ用)
        ''' </summary>
        ''' <returns></returns>
        Public Property KEY_VALUE As String

        ''' <summary>
        ''' 管理ロケーションコード
        ''' </summary>
        ''' <remarks>2021/10/06 nv miyaji add</remarks>
        ''' <returns></returns>
        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String

        ''' <summary>
        ''' 管理ロケーション名
        ''' </summary>
        ''' <remarks>2021/10/06 nv miyaji add</remarks>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property MNG_LOC_L_NAME As String

        ''' <summary>
        ''' ロケーション大区分コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String

        ''' <summary>
        ''' ロケーション大区分名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property LOC_L_NAME As String

        ''' <summary>
        ''' ロケーション中区分コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String

        ''' <summary>
        ''' ロケーション中区分名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property LOC_M_NAME As String

        ''' <summary>
        ''' ロケーション小区分コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String

        ''' <summary>
        ''' ロケーション小区分名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property LOC_S_NAME As String

        ''' <summary>
        ''' カテゴリ大区分コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(3)>
        Public Property CAT_L_CD As String

        ''' <summary>
        ''' カテゴリ大区分名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property CAT_L_NAME As String

        ''' <summary>
        ''' カテゴリ中区分コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(3)>
        Public Property CAT_M_CD As String

        ''' <summary>
        ''' カテゴリ中区分名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property CAT_M_NAME As String

        ''' <summary>
        ''' カテゴリ小区分コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(3)>
        Public Property CAT_S_CD As String

        ''' <summary>
        ''' カテゴリ小区分名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property CAT_S_NAME As String

        ''' <summary>
        ''' 数量
        ''' </summary>
        ''' <returns></returns>
        Public Property QUANTITY As Int32

        ''' <summary>
        ''' 検索条件 管理ロケーション
        ''' </summary>
        ''' <remarks>2021/10/06 nv miyaji add</remarks>
        ''' <returns>検索条件 管理ロケーション</returns>
        <NotMapped>
        Public Property SearchMngLocL As String = String.Empty

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
        ''' 検索条件 カテゴリ大区分（複数選択のため配列）
        ''' </summary>
        ''' <returns>検索条件 カテゴリ大区分</returns>
        <NotMapped>
        Public Property SearchCatL As String() = New String() {}

        ''' <summary>
        ''' 検索条件 カテゴリ中区分
        ''' </summary>
        ''' <returns>検索条件 カテゴリ中区分</returns>
        <NotMapped>
        Public Property SearchCatM As String = String.Empty

        ''' <summary>
        ''' 検索条件 カテゴリ小区分
        ''' </summary>
        ''' <returns>検索条件 カテゴリ小区分</returns>
        <NotMapped>
        Public Property SearchCatS As String = String.Empty

        ''' <summary>
        ''' 資産番号（「|」区切りで複数指定可）
        ''' </summary>
        ''' <returns>資産番号</returns>
        <NotMapped>
        Public Property SearchAssetNo As String = String.Empty

        ''' <summary>
        ''' 資産ID
        ''' </summary>
        ''' <returns>資産ID</returns>
        <NotMapped>
        Public Property SearchAssetID As String = String.Empty

        ''' <summary>
        ''' 資産名称
        ''' </summary>
        ''' <returns>資産名称</returns>
        <NotMapped>
        Public Property SearchAssetName As String = ""

        ''' <summary>
        ''' メーカ
        ''' </summary>
        ''' <returns>メーカ</returns>
        <NotMapped>
        Public Property SearchMaker As String = ""

        ''' <summary>
        ''' 品番
        ''' </summary>
        ''' <returns>品番</returns>
        <NotMapped>
        Public Property SearchProductNo As String = ""

        ''' <summary>
        ''' 型番
        ''' </summary>
        ''' <returns>型番</returns>
        <NotMapped>
        Public Property SearchModelNo As String = ""

        ''' <summary>
        ''' 検索条件 集約表示 ロケーション
        ''' </summary>
        ''' <returns>検索条件 表示指定</returns>
        <NotMapped>
        Public Property SearchDispSelectLocation As String = ""

        ''' <summary>
        ''' 検索条件 集約表示 カテゴリー
        ''' </summary>
        ''' <returns>検索条件 表示指定</returns>
        <NotMapped>
        Public Property SearchDispSelectCategory As String = ""

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
        '''  (カテゴリ)大区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>(カテゴリ)大区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catLDropdownData As List(Of CategoryDropdownModel)

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
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' 資産在庫照会情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
