Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 資産履歴照会用表示用モデル
    ''' </summary>
    Public Class AssetHistoryViewModel
        Inherits BaseModel

        <Key>
        Public Property PROCESS_LOG_ID As Long

        Public Property PROCESS_DT As Integer

        Public Property PROCESS_TM As String

        <StringLength(24)>
        Public Property ASSET_NO As String

        <StringLength(128)>
        Public Property ASSET_ID As String

        <StringLength(1)>
        Public Property PROCESS_TOOL_DIV As String

        <StringLength(2)>
        Public Property PROCESS_DIV As String

        <StringLength(15)>
        Public Property LEASE_PLACE_CD As String

        <StringLength(60)>
        Public Property LEASE_PLACE_NAME As String

        <StringLength(255)>
        Public Property REMARK As String    '2019/11/19 nv miyaji add

        <StringLength(15)>
        Public Property LEASE_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property LEASE_EMPLOYEE_NAME As String

        Public Property LEASE_DT As Integer?    '2020/03/06 nv miyaji add

        Public Property LEASE_DUE_DT As Integer?

        Public Property LEASE_STATUS As Byte?    '2021/10/05 nv Inoue add

        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String      '2021/10/05 nv Inoue add

        <StringLength(60)>
        Public Property MNG_LOC_L_NAME As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String

        <StringLength(60)>
        Public Property LOC_L_NAME As String

        <StringLength(60)>
        Public Property LOC_M_NAME As String

        <StringLength(60)>
        Public Property LOC_S_NAME As String

        Public Property ASSET_NAME As String

        Public Property MAKER As String

        Public Property PRODUCT_NO As String

        Public Property MODEL_NO As String

        '2023/03/22 Kawamoto ADD START
        Public Property DUE_QTY As Decimal?

        Public Property WORKER_EMPLOYEE_CD As String

        Public Property WORKER_EMPLOYEE_NAME As String

        Public Property WORKER_MINUTE As Int16?

        Public Property WORKER_OVERVIEW As String

        Public Property WORKER_REMARK1 As String

        Public Property WORKER_REMARK2 As String
        '2023/03/22 Kawamoto ADD END

        Public Property REMARK1 As String

        Public Property REMARK2 As String

        Public Property REMARK3 As String

        Public Property REMARK4 As String

        Public Property REMARK5 As String

        Public Property REMARK6 As String

        Public Property REMARK7 As String

        Public Property REMARK8 As String

        Public Property REMARK9 As String

        Public Property REMARK10 As String


        Public Property CAT_L_CD As String

        Public Property CAT_NAME As String

        Public Property SUB_ASSET_NO As String

        Public Property TASK_RESULT As String

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
        ''' 検索条件 期間(From)
        ''' </summary>
        ''' <returns>検索条件 期間(From)</returns>
        <NotMapped>
        Public Property SearchDateFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 期間(To)
        ''' </summary>
        ''' <returns>検索条件 期間(To)</returns>
        <NotMapped>
        Public Property SearchDateTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 処理区分 入庫予定
        ''' </summary>
        ''' <returns>検索条件 処理区分 入庫予定</returns>
        <NotMapped>
        Public Property SearchProcessDivLP As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 登録(ラベル発行)
        ''' </summary>
        ''' <returns>検索条件 処理区分 登録(ラベル発行)</returns>
        <NotMapped>
        Public Property SearchProcessDivLI As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 登録(ラベル不要)
        ''' </summary>
        ''' <returns>検索条件 処理区分 登録(ラベル発行)</returns>
        <NotMapped>
        Public Property SearchProcessDivUI As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 登録(メンテ)
        ''' </summary>
        ''' <returns>検索条件 処理区分 登録(メンテ)</returns>
        <NotMapped>
        Public Property SearchProcessDivMI As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 登録(取込)
        ''' </summary>
        ''' <returns>検索条件 処理区分 登録(取込)</returns>
        <NotMapped>
        Public Property SearchProcessDivCI As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 更新(メンテ)
        ''' </summary>
        ''' <returns>検索条件 処理区分 更新(メンテ)</returns>
        <NotMapped>
        Public Property SearchProcessDivMU As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 更新(取込)
        ''' </summary>
        ''' <returns>検索条件 処理区分 更新(取込)</returns>
        <NotMapped>
        Public Property SearchProcessDivCU As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 除却(メンテ)
        ''' </summary>
        ''' <returns>検索条件 処理区分 除却(メンテ)</returns>
        <NotMapped>
        Public Property SearchProcessDivMR As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 除却(取込)
        ''' </summary>
        ''' <returns>検索条件 処理区分 除却(取込)</returns>
        <NotMapped>
        Public Property SearchProcessDivCR As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 タグラベル発行＆紐付
        ''' </summary>
        ''' <returns>検索条件 処理区分 タグラベル発行＆紐付</returns>
        <NotMapped>
        Public Property SearchProcessDivT1 As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 タグ紐付
        ''' </summary>
        ''' <returns>検索条件 処理区分 タグ紐付</returns>
        <NotMapped>
        Public Property SearchProcessDivT2 As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 保管
        ''' </summary>
        ''' <returns>検索条件 処理区分 保管</returns>
        <NotMapped>
        Public Property SearchProcessDivBS As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 貸出
        ''' </summary>
        ''' <returns>検索条件 処理区分 貸出</returns>
        <NotMapped>
        Public Property SearchProcessDivBO As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 返却
        ''' </summary>
        ''' <returns>検索条件 処理区分 返却</returns>
        <NotMapped>
        Public Property SearchProcessDivBI As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 除却
        ''' </summary>
        ''' <returns>検索条件 処理区分 除却</returns>
        <NotMapped>
        Public Property SearchProcessDivBR As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 現ロケ変更
        ''' </summary>
        ''' <returns>検索条件 処理区分 現ロケ変更</returns>
        ''' <remarks>2019/10/15 nv miyaji add</remarks>
        <NotMapped>
        Public Property SearchProcessDivLC As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 予約登録
        ''' </summary>
        ''' <returns>検索条件 処理区分 予約登録</returns>
        ''' <remarks>2021/11/09 nv miyaji add</remarks>
        <NotMapped>
        Public Property SearchProcessDivRI As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 予約変更
        ''' </summary>
        ''' <returns>検索条件 処理区分 予約変更</returns>
        ''' <remarks>2021/11/09 nv miyaji add</remarks>
        <NotMapped>
        Public Property SearchProcessDivRU As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 予約削除
        ''' </summary>
        ''' <returns>検索条件 処理区分 予約削除</returns>
        ''' <remarks>2021/11/09 nv miyaji add</remarks>
        <NotMapped>
        Public Property SearchProcessDivRD As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 予約登録(取込) 2022/07/25 sv sato add
        ''' </summary>
        ''' <returns>検索条件 処理区分 予約登録(取込)</returns>
        ''' <remarks>2022/07/25 sv sato add</remarks>
        <NotMapped>
        Public Property SearchProcessDivXI As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 予約変更(取込) 2022/07/25 sv sato add
        ''' </summary>
        ''' <returns>検索条件 処理区分 予約変更(取込)</returns>
        ''' <remarks>2022/07/25 sv sato add</remarks>
        <NotMapped>
        Public Property SearchProcessDivXU As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 予約削除(取込) 2022/07/25 sv sato add
        ''' </summary>
        ''' <returns>検索条件 処理区分 予約削除(取込)</returns>
        ''' <remarks>2022/07/25 sv sato add</remarks>
        <NotMapped>
        Public Property SearchProcessDivXD As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 残量確認 2023/04/30 sv sato v2.4 add
        ''' </summary>
        ''' <returns>検索条件 処理区分 残量確認</returns>
        ''' <remarks></remarks>
        <NotMapped>
        Public Property SearchProcessDivDC As Boolean = True

        ''' <summary>
        ''' 検索条件 処理ツール区分
        ''' </summary>
        ''' <returns>検索条件 処理ツール区分</returns>
        <NotMapped>
        Public Property SearchProcessToolDiv As String = String.Empty

        ''' <summary>
        ''' 検索条件 資産番号
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
        ''' 検索条件 メーカ
        ''' </summary>
        ''' <returns>検索条件 メーカ</returns>
        <NotMapped>
        Public Property SearchMaker As String = String.Empty

        ''' <summary>
        ''' 検索条件 品番
        ''' </summary>
        ''' <returns>検索条件 品番</returns>
        <NotMapped>
        Public Property SearchProductNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 型番
        ''' </summary>
        ''' <returns>検索条件 型番</returns>
        <NotMapped>
        Public Property SearchModelNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 ログイン者名
        ''' </summary>
        ''' <returns>検索条件 ログイン者名</returns>
        <NotMapped>
        Public Property SearchEmployeeNm As String = String.Empty

        ''' <summary>
        ''' 検索条件 処理区分 削除(メンテ)
        ''' </summary>
        ''' <returns>検索条件 処理区分 削除(メンテ)</returns>
        ''' <remarks>2019/12/20 nv miyaji add</remarks>
        <NotMapped>
        Public Property SearchProcessDivMD As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 削除(取込)
        ''' </summary>
        ''' <returns>検索条件 処理区分 削除(取込)</returns>
        ''' <remarks>2019/12/20 nv miyaji add</remarks>
        <NotMapped>
        Public Property SearchProcessDivCD As Boolean = True

        ''' <summary>
        ''' 検索条件 処理区分 棚卸確定
        ''' </summary>
        ''' <returns>検索条件 処理区分 棚卸確定</returns>
        <NotMapped>
        Public Property SearchProcessDivIC As Boolean = True

        ''' <summary>
        ''' 非表示項目：詳細検索エリア開閉フラグ
        ''' </summary>
        ''' <returns>非表示項目：詳細検索エリア開閉フラグ</returns>
        ''' <remarks>
        ''' リンクの表示文言をそのまま状態として保持する。
        ''' 　"▲詳細検索"(LABEL_ASSET_DETAIL_CLOSE)　：開いている
        ''' 　上記以外(空文字・"▼詳細検索")　　　　　：閉じている
        ''' 画面内のポストバックでは hidden 項目で値が往復するため状態を保持し、
        ''' メニューからの遷移や初期起動では ViewModel が新規生成されるため既定値(空文字＝閉)となる。
        ''' </remarks>
        <NotMapped>
        Public Property DetailOpenFlg As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考1
        ''' </summary>
        ''' <returns>検索条件 備考1</returns>
        <NotMapped>
        Public Property SearchRemark1 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考2
        ''' </summary>
        ''' <returns>検索条件 備考2</returns>
        <NotMapped>
        Public Property SearchRemark2 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考3
        ''' </summary>
        ''' <returns>検索条件 備考3</returns>
        <NotMapped>
        Public Property SearchRemark3 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考4
        ''' </summary>
        ''' <returns>検索条件 備考4</returns>
        <NotMapped>
        Public Property SearchRemark4 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考5
        ''' </summary>
        ''' <returns>検索条件 備考5</returns>
        <NotMapped>
        Public Property SearchRemark5 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考6
        ''' </summary>
        ''' <returns>検索条件 備考6</returns>
        <NotMapped>
        Public Property SearchRemark6 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考7
        ''' </summary>
        ''' <returns>検索条件 備考7</returns>
        <NotMapped>
        Public Property SearchRemark7 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考8
        ''' </summary>
        ''' <returns>検索条件 備考8</returns>
        <NotMapped>
        Public Property SearchRemark8 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考9
        ''' </summary>
        ''' <returns>検索条件 備考9</returns>
        <NotMapped>
        Public Property SearchRemark9 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考10
        ''' </summary>
        ''' <returns>検索条件 備考10</returns>
        <NotMapped>
        Public Property SearchRemark10 As String = String.Empty


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
        '''  カテゴリー　大分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>大分類ドロップダウンリストデータ</returns>
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
        ''' 資産履歴照会情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
