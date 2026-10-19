Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class InventoryInquiryViewModel
        Inherits BaseModel

#Region "グリッド項目"
        Public Property ASSET_STATUS As Byte
        Public Property ASSET_STATUS_NAME As String
        Public Property LOCK_EMPLOYEE_CD_NAME As String
        Public Property LOCK_EMPLOYEE_CD As String
        Public Property LOCK_EMPLOYEE_NAME As String
        Public Property PHYINV_STATUS As String
        Public Property PHYINV_STATUS_NAME As String
        Public Property LOC_L_CD_NAME As String
        Public Property LOC_M_CD_NAME As String
        Public Property LOC_S_CD_NAME As String
        Public Property LOC_L_CD As String
        Public Property LOC_M_CD As String
        Public Property LOC_S_CD As String
        Public Property PHYINV_LOC_L_CD_NAME As String
        Public Property PHYINV_LOC_M_CD_NAME As String
        Public Property PHYINV_LOC_S_CD_NAME As String
        Public Property PHYINV_LOC_L_CD As String
        Public Property PHYINV_LOC_M_CD As String
        Public Property PHYINV_LOC_S_CD As String
        Public Property LOC_L_NAME As String
        Public Property LOC_M_NAME As String
        Public Property LOC_S_NAME As String
        Public Property PHYINV_LOC_L_NAME As String
        Public Property PHYINV_LOC_M_NAME As String
        Public Property PHYINV_LOC_S_NAME As String

        Public Property CAT_L_CD_NAME As String
        Public Property CAT_L_CD As String
        Public Property CAT_L_NAME As String

        Public Property IS_TANGIBLE As String

        <Key>
        Public Property ASSET_NO As String
        Public Property SUB_ASSET_NO As String
        Public Property ASSET_NAME As String
        Public Property PHYINV_TS As Long?
        Public Property PHYINV_EMPLOYEE_CD As String
        Public Property PHYINV_EMPLOYEE_NAME As String
        Public Property FREE_ITEM6 As String
        Public Property FREE_ITEM6_NAME As String
        Public Property FREE_ITEM1 As String
        Public Property FREE_ITEM2 As String
        Public Property FREE_ITEM3 As String
        Public Property FREE_ITEM4 As String
        Public Property FREE_ITEM5 As String
        Public Property FREE_ITEM7 As String
        Public Property FREE_ITEM8 As String
        Public Property FREE_ITEM9 As String
        Public Property FREE_ITEM10 As String
        Public Property FREE_ITEM11 As String
        Public Property FREE_ITEM12 As String
        Public Property FREE_ITEM13 As String
        Public Property FREE_ITEM14 As String
        Public Property FREE_ITEM15 As String
        Public Property FREE_ITEM16 As String
        Public Property FREE_ITEM17 As String
        Public Property FREE_ITEM18 As String
        Public Property FREE_ITEM19 As String
        Public Property FREE_ITEM20 As String
        Public Property FREE_ITEM21 As String
        Public Property FREE_ITEM22 As String
        Public Property FREE_ITEM23 As String
        Public Property FREE_ITEM24 As String
        Public Property FREE_ITEM25 As String
        Public Property FREE_ITEM26 As String
        Public Property FREE_ITEM27 As String
        Public Property FREE_ITEM28 As String
        Public Property FREE_ITEM29 As String
        Public Property FREE_ITEM30 As String
        Public Property FREE_ITEM31 As String
        Public Property FREE_ITEM32 As String
        Public Property FREE_ITEM33 As String
        Public Property FREE_ITEM34 As String
        Public Property FREE_ITEM35 As String
        Public Property FREE_ITEM36 As String
        Public Property FREE_ITEM37 As String
        Public Property FREE_ITEM38 As String
        Public Property FREE_ITEM39 As String
        Public Property FREE_ITEM40 As String
        Public Property FREE_ITEM41 As String
        Public Property FREE_ITEM42 As String
        Public Property FREE_ITEM43 As String
        Public Property FREE_ITEM44 As String
        Public Property FREE_ITEM45 As String
        Public Property FREE_ITEM46 As String
        Public Property FREE_ITEM47 As String
        Public Property FREE_ITEM48 As String
        Public Property FREE_ITEM49 As String
        Public Property FREE_ITEM50 As String
        Public Property FREE_ITEM51 As String
        Public Property FREE_ITEM52 As String
        Public Property FREE_ITEM53 As String
        Public Property FREE_ITEM54 As String
        Public Property FREE_ITEM55 As String
        Public Property FREE_ITEM56 As String
        Public Property FREE_ITEM57 As String
        Public Property FREE_ITEM58 As String
        Public Property FREE_ITEM59 As String
        Public Property FREE_ITEM60 As String
        Public Property TAG_LINK_STATUS As Byte = 0
        Public Property OPE_STATUS As String
        Public Property OPE_STATUS_NAME As String
        Public Property DIVERSION_REMOVE As String
        Public Property REMOVE_DATE As String
        Public Property REMOVE_REASON As String
        Public Property REMOVE_REASON_NAME As String
        Public Property REMOVE_CONTENT As String
        Public Property CHANGE_LOC_M_CD As String
        Public Property CHANGE_LOC_M_NAME As String
        Public Property CHANGE_LOC_S_CD As String
        Public Property CHANGE_LOC_S_NAME As String
        Public Property CHANGE_EMPLOYEE As String
        Public Property CHANGE_EMPLOYEE_NAME As String
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
        '[CC431] ADD Start 20261002
        ' 棚卸実施端末（実棚処理ツール区分 H:HT、W:Web）
        Public Property PHYINV_TOOL_DIV As String
        ' 入力作業者コード／名（名称は社員M）
        Public Property ENT_EMPLOYEE_CD As String
        Public Property ENT_EMPLOYEE_NAME As String
        '[CC431] ADD End 20261002
#End Region

        ''' <summary>
        ''' 検索条件 ロケ大区分
        ''' </summary>
        ''' <returns>検索条件 ロケ大区分</returns>
        <NotMapped>
        Public Property SearchLocL As String = String.Empty
        <NotMapped>
        Public Property SearchLocLSelected As String = String.Empty

        ''' <summary>
        ''' 検索条件 ロケ中区分
        ''' </summary>
        ''' <returns>検索条件 ロケ中区分</returns>
        <NotMapped>
        Public Property SearchLocM As String() = New String() {}   '複数選択のため配列

        ''' <summary>
        ''' 検索条件 ロケ小区分
        ''' </summary>
        ''' <returns>検索条件 ロケ小区分</returns>
        <NotMapped>
        Public Property SearchLocS As String() = New String() {}   '複数選択のため配列

        '[CC431] ADD Start 20261007
        ''' <summary>
        ''' 検索条件 ロケ小区分の「場所から選ぶ」（True：場所で原価センタを絞り込む、False：原価センタで場所を絞り込む）
        ''' </summary>
        ''' <returns>検索条件 場所から選ぶ</returns>
        <NotMapped>
        Public Property SearchLocReverse As Boolean = False
        '[CC431] ADD End 20261007

        ''' <summary>
        ''' 検索条件 カテゴリー大分類
        ''' </summary>
        ''' <returns>検索条件 カテゴリー大分類</returns>
        <NotMapped>
        Public Property SearchCatL As String() = New String() {}   '複数選択のため配列


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
        Public Property SearchAssetNo As String = String.Empty   '「|」区切りで複数入力するため桁数制限なし

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
        ''' 検索条件 有形（有形・無形）
        ''' </summary>
        ''' <returns>検索条件 有形（有形・無形）</returns>
        <NotMapped>
        Public Property SearchTangibleCheck As Boolean = False

        ''' <summary>
        ''' 検索条件 無形（有形・無形）
        ''' </summary>
        ''' <returns>検索条件 無形（有形・無形）</returns>
        <NotMapped>
        Public Property SearchNotTangibleCheck As Boolean = False

        ''' <summary>
        ''' 検索条件 資産管理者
        ''' </summary>
        ''' <returns>検索条件 資産管理者</returns>
        <NotMapped>
        Public Property SearchAssetAdmin As String = String.Empty

        ''' <summary>
        ''' 検索条件 ロック担当者
        ''' </summary>
        ''' <returns>検索条件 ロック担当者</returns>
        <NotMapped>
        Public Property SearchLockEmployee As String = String.Empty

        ''' <summary>
        ''' 検索条件 転用or除去
        ''' </summary>
        ''' <returns>検索条件 転用or除去</returns>
        <NotMapped>
        Public Property SearchDiversionRemove As String = String.Empty

        ''' <summary>
        ''' 検索条件 稼働区分
        ''' </summary>
        ''' <returns>検索条件 稼働区分</returns>
        <NotMapped>
        Public Property SearchOpeStatus As String = String.Empty


        '[CC431] DEL 20261015 除却時期を年月の単一入力から年月の範囲に変更（SearchRemoveDate を SearchRemoveDateFrom／To に置き換え）
        '[CC431] ADD Start 20261015
        ''' <summary>
        ''' 検索条件 除却時期From
        ''' </summary>
        ''' <returns>検索条件 除却時期From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM}")>
        Public Property SearchRemoveDateFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 除却時期To
        ''' </summary>
        ''' <returns>検索条件 除却時期To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM}")>
        Public Property SearchRemoveDateTo As String = String.Empty
        '[CC431] ADD End 20261015

        ''' <summary>
        ''' 検索条件 除去理由
        ''' </summary>
        ''' <returns>検索条件 除去理由</returns>
        <NotMapped>
        Public Property SearchRemoveReason As String = String.Empty

        ''' <summary>
        ''' 検索条件 除去内容
        ''' </summary>
        ''' <returns>検索条件 除去内容</returns>
        <NotMapped>
        Public Property SearchRemoveContent As String = String.Empty

        ''' <summary>
        ''' 検索条件 変更後部課コード
        ''' </summary>
        ''' <returns>検索条件 変更後部課コード</returns>
        <NotMapped>
        Public Property SearchChangeLocMCd As String() = New String() {}   '複数選択のため配列

        ''' <summary>
        ''' 検索条件 変更後場所コード
        ''' </summary>
        ''' <returns>検索条件 変更後場所コード</returns>
        <NotMapped>
        Public Property SearchChangeLocSCd As String() = New String() {}   '複数選択のため配列

        '[CC431] ADD Start 20261007
        ''' <summary>
        ''' 検索条件 変更後場所コードの「場所から選ぶ」（True：変更後場所で変更後部課を絞り込む、False：変更後部課で変更後場所を絞り込む）
        ''' </summary>
        ''' <returns>検索条件 変更後場所コードの場所から選ぶ</returns>
        <NotMapped>
        Public Property SearchChangeLocReverse As Boolean = False
        '[CC431] ADD End 20261007

        ''' <summary>
        ''' 検索条件 変更後社員番号
        ''' </summary>
        ''' <returns>検索条件 変更後社員番号</returns>
        <NotMapped>
        Public Property SearchChangeEmployee As String = String.Empty

        ''' <summary>
        ''' 検索条件 貸与状態資産区分（Free10）
        ''' </summary>
        ''' <returns>検索条件 貸与状態資産区分</returns>
        <NotMapped>
        Public Property SearchFreeItem10 As String = String.Empty

        '[CC431] DEL 20261002 資本化日付を年月の単一入力から日付範囲に変更（SearchFreeItem2 を SearchFreeItem2From／To に置き換え）
        '[CC431] ADD Start 20261002
        ''' <summary>
        ''' 検索条件 資本化日付From（Free2）
        ''' </summary>
        ''' <returns>検索条件 資本化日付From</returns>
        <NotMapped>
        Public Property SearchFreeItem2From As String = String.Empty

        ''' <summary>
        ''' 検索条件 資本化日付To（Free2）
        ''' </summary>
        ''' <returns>検索条件 資本化日付To</returns>
        <NotMapped>
        Public Property SearchFreeItem2To As String = String.Empty
        '[CC431] ADD End 20261002

        ''' <summary>
        ''' 検索条件 稼働区分名(FreeItem4)
        ''' </summary>
        ''' <returns>検索条件 稼働区分名</returns>
        <NotMapped>
        Public Property SearchFreeItem4 As String = String.Empty


        ''' <summary>
        ''' 検索条件 型式(Free5)
        ''' </summary>
        ''' <returns>検索条件 型式</returns>
        <NotMapped>
        Public Property SearchFreeItem5 As String = String.Empty

        ''' <summary>
        ''' 検索条件 管理社員番号(Free6)
        ''' </summary>
        ''' <returns>検索条件 管理社員番号</returns>
        <NotMapped>
        Public Property SearchFreeItem6 As String = String.Empty

        ''' <summary>
        ''' 検索条件 シリアルNo(Free7)
        ''' </summary>
        ''' <returns>検索条件 シリアルNo</returns>
        <NotMapped>
        Public Property SearchFreeItem7 As String = String.Empty

        ''' <summary>
        ''' 検索条件 WBS番号(Free8)
        ''' </summary>
        ''' <returns>検索条件 WBS番号</returns>
        <NotMapped>
        Public Property SearchFreeItem8 As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考(Free9)
        ''' </summary>
        ''' <returns>検索条件 備考</returns>
        <NotMapped>
        Public Property SearchFreeItem9 As String = String.Empty

        ''' <summary>
        ''' 検索条件 所得金額From(Free11)
        ''' </summary>
        ''' <returns>検索条件 所得金額From</returns>
        <NotMapped>
        Public Property SearchFreeItem11From As String = String.Empty

        ''' <summary>
        ''' 検索条件 所得金額To(Free11)
        ''' </summary>
        ''' <returns>検索条件 所得金額To</returns>
        <NotMapped>
        Public Property SearchFreeItem11To As String = String.Empty

        ''' <summary>
        ''' 検索条件 当月末薄荷From(Free12)
        ''' </summary>
        ''' <returns>検索条件 当月末薄荷From</returns>
        <NotMapped>
        Public Property SearchFreeItem12From As String = String.Empty

        ''' <summary>
        ''' 検索条件 当月末薄荷To(Free12)
        ''' </summary>
        ''' <returns>検索条件 当月末薄荷To</returns>
        <NotMapped>
        Public Property SearchFreeItem12To As String = String.Empty

        ''' <summary>
        ''' 検索条件 装置管理番号(Free14)
        ''' </summary>
        ''' <returns>検索条件 装置管理番号</returns>
        <NotMapped>
        Public Property SearchFreeItem14 As String = String.Empty

        ''' <summary>
        ''' 検索条件 資産グループ(Free15)
        ''' </summary>
        ''' <returns>検索条件 資産グループ</returns>
        <NotMapped>
        Public Property SearchFreeItem15 As String = String.Empty

        ''' <summary>
        ''' 検索条件 貸与先(Free16)
        ''' </summary>
        ''' <returns>検索条件 貸与先</returns>
        <NotMapped>
        Public Property SearchFreeItem16 As String = String.Empty

        ''' <summary>
        ''' 検索条件 貸与先名(Free17)
        ''' </summary>
        ''' <returns>検索条件 貸与先名</returns>
        <NotMapped>
        Public Property SearchFreeItem17 As String = String.Empty

        '[CC431] DEL 20261002 開始日(貸与)・終了日(貸与)を単一の日付から日付範囲に変更（SearchFreeItem18／19 を From／To に置き換え）
        '[CC431] ADD Start 20261002
        ''' <summary>
        ''' 検索条件 開始日From(Free18)
        ''' </summary>
        ''' <returns>検索条件 開始日From</returns>
        <NotMapped>
        Public Property SearchFreeItem18From As String = String.Empty

        ''' <summary>
        ''' 検索条件 開始日To(Free18)
        ''' </summary>
        ''' <returns>検索条件 開始日To</returns>
        <NotMapped>
        Public Property SearchFreeItem18To As String = String.Empty

        ''' <summary>
        ''' 検索条件 終了日From(Free19)
        ''' </summary>
        ''' <returns>検索条件 終了日From</returns>
        <NotMapped>
        Public Property SearchFreeItem19From As String = String.Empty

        ''' <summary>
        ''' 検索条件 終了日To(Free19)
        ''' </summary>
        ''' <returns>検索条件 終了日To</returns>
        <NotMapped>
        Public Property SearchFreeItem19To As String = String.Empty
        '[CC431] ADD End 20261002

        ''' <summary>
        ''' 検索条件 貸与回数From(Free20)
        ''' </summary>
        ''' <returns>検索条件 貸与回数From</returns>
        <NotMapped>
        Public Property SearchFreeItem20From As String = String.Empty

        ''' <summary>
        ''' 検索条件 貸与回数To(Free20)
        ''' </summary>
        ''' <returns>検索条件 貸与回数To</returns>
        <NotMapped>
        Public Property SearchFreeItem20To As String = String.Empty

        ''' <summary>
        ''' 検索条件 備考欄(Free21)
        ''' </summary>
        ''' <returns>検索条件 備考欄</returns>
        <NotMapped>
        Public Property SearchFreeItem21 As String = String.Empty

        ''' 検索条件 フリー項目37 資産リース区分
        ''' </summary>
        ''' <returns>検索条件 フリー項目37</returns>
        <NotMapped>
        Public Property SearchFreeItem37 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目39 市区町村コード名称
        ''' </summary>
        ''' <returns>検索条件 フリー項目39</returns>
        <NotMapped>
        Public Property SearchFreeItem39 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目42 CAPS番号
        ''' </summary>
        ''' <returns>検索条件 フリー項目42</returns>
        <NotMapped>
        Public Property SearchFreeItem42 As String = String.Empty

        ''' <summary>
        ''' 検索条件 タグ紐付状態
        ''' </summary>
        ''' <returns>検索条件 タグ紐付状態</returns>
        <NotMapped>
        Public Property SearchTagLinkStatus As String = String.Empty

        '[CC431] ADD Start 20261002
        ''' <summary>
        ''' 検索条件 実棚日From
        ''' </summary>
        ''' <returns>検索条件 実棚日From</returns>
        <NotMapped>
        Public Property SearchPhyinvDateFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 実棚日To
        ''' </summary>
        ''' <returns>検索条件 実棚日To</returns>
        <NotMapped>
        Public Property SearchPhyinvDateTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 実棚者コード
        ''' </summary>
        ''' <returns>検索条件 実棚者コード</returns>
        <NotMapped>
        Public Property SearchPhyinvEmployee As String = String.Empty

        ''' <summary>
        ''' 検索条件 棚卸実施端末（実棚処理ツール区分）
        ''' </summary>
        ''' <returns>検索条件 棚卸実施端末</returns>
        <NotMapped>
        Public Property SearchPhyinvToolDiv As String = String.Empty

        ''' <summary>
        ''' 検索条件 ロック資産ステータス（InvLockStatus）
        ''' </summary>
        ''' <returns>検索条件 ロック資産ステータス</returns>
        <NotMapped>
        Public Property SearchLockStatus As String = String.Empty

        ''' <summary>
        ''' 検索条件 棚卸ノート(Free27)
        ''' </summary>
        ''' <returns>検索条件 棚卸ノート</returns>
        <NotMapped>
        Public Property SearchFreeItem27 As String = String.Empty

        ''' <summary>
        ''' 検索条件 入力作業者
        ''' </summary>
        ''' <returns>検索条件 入力作業者</returns>
        <NotMapped>
        Public Property SearchEntEmployee As String = String.Empty

        ''' <summary>
        ''' 検索条件 並び順（InventoryInquirySort）
        ''' </summary>
        ''' <returns>検索条件 並び順</returns>
        <NotMapped>
        Public Property SearchSortOrder As Integer = InventoryInquirySort.CatLAsc

        ''' <summary>
        ''' 検索条件 備考1～備考10（「|」区切りで複数入力するため桁数制限なし）
        ''' </summary>
        <NotMapped>
        Public Property SearchRemark1 As String = String.Empty
        <NotMapped>
        Public Property SearchRemark2 As String = String.Empty
        <NotMapped>
        Public Property SearchRemark3 As String = String.Empty
        <NotMapped>
        Public Property SearchRemark4 As String = String.Empty
        <NotMapped>
        Public Property SearchRemark5 As String = String.Empty
        <NotMapped>
        Public Property SearchRemark6 As String = String.Empty
        <NotMapped>
        Public Property SearchRemark7 As String = String.Empty
        <NotMapped>
        Public Property SearchRemark8 As String = String.Empty
        <NotMapped>
        Public Property SearchRemark9 As String = String.Empty
        <NotMapped>
        Public Property SearchRemark10 As String = String.Empty

        ''' <summary>
        ''' 詳細検索エリアの開閉状態（リンクの文言。ポストバック後も開閉状態を保つため hidden で往復させる）
        ''' </summary>
        <NotMapped>
        Public Property DetailOpenFlg As String = String.Empty
        '[CC431] ADD End 20261002

        '[CC431] ADD Start 20261005
        ''' <summary>
        ''' 検索条件エリアの開閉状態（リンクの文言。ポストバック後も開閉状態を保つため hidden で往復させる）
        ''' </summary>
        <NotMapped>
        Public Property OpenFlg As String = String.Empty
        '[CC431] ADD End 20261005

        ''' <summary>
        ''' CSVパス
        ''' </summary>
        ''' <returns>CSVパス</returns>
        <NotMapped>
        Public Property CsvPath As String = String.Empty

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
        '''  (カテゴリー)大分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>(カテゴリー)大分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catLDropdownData As List(Of CategoryDropdownModel)


        '<NotMapped>
        'Public Property SelectAssetNo As String

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
        '''  社員マスタ一覧
        ''' </summary>
        <NotMapped>
        Public Property EmployeeList As List(Of EmployeeModel)

        ''' <summary>
        '''  ロケーションLマスタ一覧
        ''' </summary>
        <NotMapped>
        Public Property LocationList As List(Of LocationModel)

        ''' <summary>
        '''  ロケーションMマスタ一覧
        ''' </summary>
        <NotMapped>
        Public Property LocationMList As List(Of LocationModel)

        ''' <summary>
        '''  ロケーションSマスタ一覧
        ''' </summary>
        <NotMapped>
        Public Property LocationSList As List(Of LocationModel)

        ''' <summary>
        '''  編集内容一覧
        ''' </summary>
        <NotMapped>
        Public Property EditedContainListJson As String

        <NotMapped>
        Public Property EDITED_OPE_STATUS As String
        <NotMapped>
        Public Property EDITED_DIVERSION_REMOVE As String

        <NotMapped>
        Public Property EditJson As String

        <NotMapped>
        Public Property EditedContain As String

    End Class

End Namespace
