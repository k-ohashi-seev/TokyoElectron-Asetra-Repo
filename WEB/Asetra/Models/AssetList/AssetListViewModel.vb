Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Public Class AssetListViewModel
        Inherits BaseModel

        <Key>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(24, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_NO As String

        <StringLength(128, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_ID As String

        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property SUB_ASSET_NO As String

        Public Property EPC_SIZE As Integer?

        <StringLength(64)>
        Public Property GEN_ASSET_ID As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_DIV As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_NAME As String

        <StringLength(10)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property MNG_LOC_L_CD As String '2021/09/16 nv miyaji add

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String

        <StringLength(8)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property CAT_L_CD As String

        <StringLength(8)>
        Public Property CAT_M_CD As String

        <StringLength(8)>
        Public Property CAT_S_CD As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property MAKER As String

        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property PRODUCT_NO As String

        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property MODEL_NO As String

        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property LEASE_NO As String

        Public Property LEASE_LIMIT As Integer?

        Public Property GENERIC_LIMIT As Integer?

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM1 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM2 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM3 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM4 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM5 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM6 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM7 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM8 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM9 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM10 As String

        '2023/02/08 Kawamoto ADD START
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM11 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM12 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM13 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM14 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM15 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM16 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM17 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM18 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM19 As String
        '2023/02/08 Kawamoto ADD END

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM20 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM21 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM22 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM23 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM24 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM25 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM26 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM27 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM28 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM29 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM30 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM31 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM32 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM33 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM34 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM35 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM36 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM37 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM38 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM39 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM40 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM41 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM42 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM43 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM44 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM45 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM46 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM47 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM48 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM49 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM50 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM51 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM52 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM53 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM54 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM55 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM56 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM57 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM58 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM59 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FREE_ITEM60 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_PIC_FILENAME As String

        Public Property LEASE_CNT As Integer = 0

        Public Property ASSET_STATUS As Byte = 0

        Public Property EPC_ENC_STATUS As Byte = 0

        Public Property TAG_LINK_STATUS As Byte = 0

        Public Property LEASE_STATUS As Byte = 0 '2021/09/16 nv miyaji add

        <StringLength(15)>
        Public Property LEASE_PLACE_CD As String

        <StringLength(60)>
        Public Property LEASE_PLACE_NAME As String

        <StringLength(255)>
        Public Property REMARK As String

        <StringLength(15)>
        Public Property LEASE_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property LEASE_EMPLOYEE_NAME As String

        Public Property LEASE_DT As Integer?

        Public Property LEASE_DUE_DT As Integer?

        '--2021/09/16 nv miyaji add start
        Public Property MAINTE_LEASE_NUM As Integer?

        Public Property MAINTE_LEASE_DAY As Integer?

        <StringLength(1)>
        Public Property MAINTE_LEASE_DAY_WORK_FLAG As String

        Public Property MAINTE_DAY As Integer?

        <StringLength(1)>
        Public Property MAINTE_DAY_WORK_FLAG As String
        '--2021/09/16 nv miyaji add end

        Public Property RETIRE_TS As Long?

        <StringLength(15)>
        Public Property RETIRE_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property RETIRE_EMPLOYEE_NAME As String

        <StringLength(128)>
        Public Property RETIRE_ASSET_ID As String

        Public Property PHYINV_TS As Long?

        <StringLength(1)>
        Public Property PHYINV_STATUS As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property PHYINV_LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property PHYINV_LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property PHYINV_LOC_S_CD As String

        <StringLength(60)>
        Public Property PHYINV_LOC_L_NAME As String

        <StringLength(60)>
        Public Property PHYINV_LOC_M_NAME As String

        <StringLength(60)>
        Public Property PHYINV_LOC_S_NAME As String

        <StringLength(15)>
        Public Property PHYINV_EMPLOYEE_CD As String

        Public Property TASK_TS As Long?

        Public Property PHYINV_EMPLOYEE_NAME As String

        ''' <summary>
        ''' リース期限何日前
        ''' </summary>
        ''' <returns>リース期限何日前</returns>
        Public Property LEASE_LIMIT_BEF_DAYS As Integer

        ''' <summary>
        ''' ○○期限何日前
        ''' </summary>
        ''' <returns>○○期限何日前</returns>
        Public Property GENERIC_LIMIT_BEF_DAYS As Integer

        ''' <summary>
        ''' 貸出アラート回数
        ''' </summary>
        ''' <returns>貸出アラート回数</returns>
        Public Property LEASE_ALERT_CNT As Integer

        ''' <summary>
        ''' 有形・無形区分
        ''' </summary>
        ''' <returns>有形・無形区分</returns>
        <StringLength(1)>
        Public Property IS_TANGIBLE As String

        ''' <summary>
        ''' 備考1
        ''' </summary>
        ''' <returns>備考1</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK1 As String

        ''' <summary>
        ''' 備考2
        ''' </summary>
        ''' <returns>備考2</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK2 As String

        ''' <summary>
        ''' 備考3
        ''' </summary>
        ''' <returns>備考3</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK3 As String

        ''' <summary>
        ''' 備考4
        ''' </summary>
        ''' <returns>備考4</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK4 As String

        ''' <summary>
        ''' 備考5
        ''' </summary>
        ''' <returns>備考5</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK5 As String

        ''' <summary>
        ''' 備考6
        ''' </summary>
        ''' <returns>備考6</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK6 As String

        ''' <summary>
        ''' 備考7
        ''' </summary>
        ''' <returns>備考7</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK7 As String

        ''' <summary>
        ''' 備考8
        ''' </summary>
        ''' <returns>備考8</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK8 As String

        ''' <summary>
        ''' 備考9
        ''' </summary>
        ''' <returns>備考9</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK9 As String

        ''' <summary>
        ''' 備考10
        ''' </summary>
        ''' <returns>備考10</returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK10 As String

        ''' <summary>
        ''' 管理者社員名(フリー項目6のコードから社員Mを検索した社員名)
        ''' </summary>
        ''' <returns>管理者社員名</returns>
        ''' <remarks>SQLの別名(FREE_ITEM6_NAME)で取得するため、LocLName等と同様に属性は付けない。</remarks>
        Public Property FREE_ITEM6_NAME As String = String.Empty

        ''' <summary>
        ''' 登録日時
        ''' </summary>
        ''' <returns>登録日時</returns>
        <DisplayFormat(DataFormatString:="{0:yyyy/MM/dd HH:mm}")>
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
        <DisplayFormat(DataFormatString:="{0:yyyy/MM/dd HH:mm}")>
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
        ''' 検索条件 カテゴリ中分類
        ''' </summary>
        ''' <returns>検索条件 カテゴリ中分類</returns>
        <NotMapped>
        Public Property SearchCatM As String = String.Empty

        ''' <summary>
        ''' 検索条件 カテゴリ小分類
        ''' </summary>
        ''' <returns>検索条件 カテゴリ小分類</returns>
        <NotMapped>
        Public Property SearchCatS As String = String.Empty

        ''' <summary>
        ''' 検索条件 メーカー
        ''' </summary>
        ''' <returns>検索条件 メーカー</returns>
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
        ''' 検索条件 貸出先場所コード
        ''' </summary>
        ''' <returns>検索条件 貸出先場所コード</returns>
        <NotMapped>
        Public Property SearchLeasePlaceCd As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目1
        ''' </summary>
        ''' <returns>検索条件 フリー項目1</returns>
        <NotMapped>
        Public Property SearchFreeItem1 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目2 資本化日付
        ''' </summary>
        ''' <returns>検索条件 フリー項目2</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM}")>
        Public Property SearchFreeItem2 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目3
        ''' </summary>
        ''' <returns>検索条件 フリー項目3</returns>
        <NotMapped>
        Public Property SearchFreeItem3 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目4 稼働区分名
        ''' </summary>
        ''' <returns>検索条件 フリー項目4</returns>
        <NotMapped>
        Public Property SearchFreeItem4 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目5 型式
        ''' </summary>
        ''' <returns>検索条件 フリー項目5</returns>
        <NotMapped>
        Public Property SearchFreeItem5 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目6 管理社員番号
        ''' </summary>
        ''' <returns>検索条件 フリー項目6</returns>
        <NotMapped>
        Public Property SearchFreeItem6 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目7 シリアルNo
        ''' </summary>
        ''' <returns>検索条件 フリー項目7</returns>
        <NotMapped>
        Public Property SearchFreeItem7 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目8 WBS番号
        ''' </summary>
        ''' <returns>検索条件 フリー項目8</returns>
        <NotMapped>
        Public Property SearchFreeItem8 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目9 備考
        ''' </summary>
        ''' <returns>検索条件 フリー項目9</returns>
        <NotMapped>
        Public Property SearchFreeItem9 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目10 貸与資産状態区分
        ''' </summary>
        ''' <returns>検索条件 フリー項目10</returns>
        <NotMapped>
        Public Property SearchFreeItem10 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目11
        ''' </summary>
        ''' <returns>検索条件 フリー項目11</returns>
        <NotMapped>
        Public Property SearchFreeItem11 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目11From　取得金額
        ''' </summary>
        ''' <returns>検索条件 フリー項目11From</returns>
        <NotMapped>
        Public Property SearchFreeItem11From As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目11To　取得金額
        ''' </summary>
        ''' <returns>検索条件 フリー項目11To</returns>
        <NotMapped>
        Public Property SearchFreeItem11To As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目2From　資本化日付
        ''' </summary>
        ''' <returns>検索条件 フリー項目2From</returns>
        <NotMapped>
        Public Property SearchFreeItem2From As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目2To　資本化日付
        ''' </summary>
        ''' <returns>検索条件 フリー項目2To</returns>
        <NotMapped>
        Public Property SearchFreeItem2To As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目12
        ''' </summary>
        ''' <returns>検索条件 フリー項目12</returns>
        <NotMapped>
        Public Property SearchFreeItem12 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目12From　当月末簿価
        ''' </summary>
        ''' <returns>検索条件 フリー項目12From</returns>
        <NotMapped>
        Public Property SearchFreeItem12From As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目12To　当月末簿価
        ''' </summary>
        ''' <returns>検索条件 フリー項目12To</returns>
        <NotMapped>
        Public Property SearchFreeItem12To As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目13
        ''' </summary>
        ''' <returns>検索条件 フリー項目13</returns>
        <NotMapped>
        Public Property SearchFreeItem13 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目14 シリアル番号
        ''' </summary>
        ''' <returns>検索条件 フリー項目14</returns>
        <NotMapped>
        Public Property SearchFreeItem14 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目15 資産グループコード
        ''' </summary>
        ''' <returns>検索条件 フリー項目15</returns>
        <NotMapped>
        Public Property SearchFreeItem15 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目16 貸与先
        ''' </summary>
        ''' <returns>検索条件 フリー項目16</returns>
        <NotMapped>
        Public Property SearchFreeItem16 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目17 貸与先名
        ''' </summary>
        ''' <returns>検索条件 フリー項目17</returns>
        <NotMapped>
        Public Property SearchFreeItem17 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目18 開始日(貸与)
        ''' </summary>
        ''' <returns>検索条件 フリー項目18</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchFreeItem18 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目19 終了日(貸与)
        ''' </summary>
        ''' <returns>検索条件 フリー項目19</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchFreeItem19 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目18From　開始日(貸与)
        ''' </summary>
        ''' <returns>検索条件 フリー項目18From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchFreeItem18From As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目18To　開始日(貸与)
        ''' </summary>
        ''' <returns>検索条件 フリー項目18To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchFreeItem18To As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目19From　終了日(貸与)
        ''' </summary>
        ''' <returns>検索条件 フリー項目19From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchFreeItem19From As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目19To　終了日(貸与)
        ''' </summary>
        ''' <returns>検索条件 フリー項目19To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchFreeItem19To As String = String.Empty
        '2023/02/08 Kawamoto ADD START

        ''' <summary>
        ''' 検索条件 フリー項目20
        ''' </summary>
        ''' <returns>検索条件 フリー項目20</returns>
        <NotMapped>
        Public Property SearchFreeItem20 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目20From　貸与回数
        ''' </summary>
        ''' <returns>検索条件 フリー項目20From</returns>
        <NotMapped>
        Public Property SearchFreeItem20From As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目20To　貸与回数
        ''' </summary>
        ''' <returns>検索条件 フリー項目20To</returns>
        <NotMapped>
        Public Property SearchFreeItem20To As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目21 備考欄
        ''' </summary>
        ''' <returns>検索条件 フリー項目21</returns>
        <NotMapped>
        Public Property SearchFreeItem21 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目27 棚卸ノート
        ''' </summary>
        ''' <returns>検索条件 フリー項目27</returns>
        <NotMapped>
        Public Property SearchFreeItem27 As String = String.Empty

        ''' <summary>
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
        ''' 検索条件 貸出回数From
        ''' </summary>
        ''' <returns>検索条件 貸出回数From</returns>
        <NotMapped>
        Public Property SearchLeaseCntFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 貸出回数To
        ''' </summary>
        ''' <returns>検索条件 貸出回数To</returns>
        <NotMapped>
        Public Property SearchLeaseCntTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 リース期限 期限切れ
        ''' </summary>
        ''' <returns>検索条件 リース期限 期限切れ</returns>
        <NotMapped>
        Public Property SearchLeaseLimitExpired As Boolean = False

        ''' <summary>
        ''' 検索条件 リース期限 アラート
        ''' </summary>
        ''' <returns>検索条件 リース期限 アラート</returns>
        <NotMapped>
        Public Property SearchLeaseLimitAlert As Boolean = False

        ''' <summary>
        ''' 検索条件 リース期限の期間選択チェックボックス
        ''' </summary>
        ''' <returns>検索条件 リース期限の期間選択チェックボックス</returns>
        <NotMapped>
        Public Property SearchLeaseLimitKikan As Boolean = False

        ''' <summary>
        ''' 検索条件 リース期限期間From
        ''' </summary>
        ''' <returns>検索条件 リース期限期間From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchLeaseLimitFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 リース期限期間To
        ''' </summary>
        ''' <returns>検索条件 リース期限期間To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchLeaseLimitTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 貸出 アラート
        ''' </summary>
        ''' <returns>検索条件 貸出 アラート</returns>
        <NotMapped>
        Public Property SearchLeaseAlert As Boolean = False

        ''' <summary>
        ''' 検索条件 返却期限 期限切れ
        ''' </summary>
        ''' <returns>検索条件 返却期限 期限切れ</returns>
        <NotMapped>
        Public Property SearchHenkyakuLimitExpired As Boolean = False

        ''' <summary>
        ''' 検索条件 返却期限の期間選択チェックボックス
        ''' </summary>
        ''' <returns>検索条件 返却期限の期間選択チェックボックス</returns>
        <NotMapped>
        Public Property SearchHenkyakuLimitKikan As Boolean = False

        ''' <summary>
        ''' 検索条件 返却期限期間From
        ''' </summary>
        ''' <returns>検索条件 返却期限期間From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchHenkyakuLimitFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 返却期限期間To
        ''' </summary>
        ''' <returns>検索条件 返却期限期間To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchHenkyakuLimitTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 資産状態
        ''' </summary>
        ''' <returns>検索条件 資産状態</returns>
        <NotMapped>
        Public Property SearchAssetStatus As String = String.Empty

        ''' <summary>
        ''' 検索条件 □□期限 期限切れ
        ''' </summary>
        ''' <returns>検索条件 □□期限 期限切れ</returns>
        <NotMapped>
        Public Property SearchGenericLimitExpired As Boolean = False

        ''' <summary>
        ''' 検索条件 □□期限 アラート
        ''' </summary>
        ''' <returns>検索条件 □□期限 アラート</returns>
        <NotMapped>
        Public Property SearchGenericLimitAlert As Boolean = False

        ''' <summary>
        ''' 検索条件 □□期限の期間選択チェックボックス
        ''' </summary>
        ''' <returns>検索条件 □□期限の期間選択チェックボックス</returns>
        <NotMapped>
        Public Property SearchGenericLimitKikan As Boolean = False

        ''' <summary>
        ''' 検索条件 □□期限期間From
        ''' </summary>
        ''' <returns>検索条件 □□期限期間From</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchGenericLimitFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 □□期限期間To
        ''' </summary>
        ''' <returns>検索条件 □□期限期間To</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchGenericLimitTo As String = String.Empty

        ''' <summary>
        ''' 検索条件 資産ID状態
        ''' </summary>
        ''' <returns>検索条件 資産ID状態</returns>
        <NotMapped>
        Public Property SearchEpcEncStatus As String = String.Empty

        ''' <summary>
        ''' 検索条件 タグ紐付状態
        ''' </summary>
        ''' <returns>検索条件 タグ紐付状態</returns>
        <NotMapped>
        Public Property SearchTagLinkStatus As String = String.Empty

        ''' <summary>
        ''' 検索条件 表示順（並び順）
        ''' </summary>
        ''' <returns>検索条件 表示順</returns>
        <NotMapped>
        Public Property SearchDispSelect As Integer = AssetDispSelect.CatLAsc

        ''' <summary>
        ''' 非表示項目：詳細検索エリア開閉フラグ
        ''' </summary>
        ''' <returns>非表示項目：詳細検索エリア開閉フラグ</returns>
        ''' <remarks>
        ''' 既存の OpenFlg と同じく、リンクの表示文言をそのまま状態として保持する。
        ''' 　"▲詳細検索"(LABEL_ASSET_DETAIL_CLOSE)　：開いている
        ''' 　上記以外(空文字・"▼詳細検索")　　　　　：閉じている
        ''' OpenFlg は検索条件エリアの開閉状態で使用しているため、詳細検索エリアは本項目で別に保持する。
        ''' 画面内のポストバック(検索・ページング・子画面からの戻り)では hidden 項目で値が往復するため状態を保持し、
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
        ''' 検索条件 貸出先備考
        ''' </summary>
        ''' <returns>検索条件 貸出先備考</returns>
        <NotMapped>
        Public Property SearchRemarks As String = String.Empty

        ''' <summary>
        ''' 検索条件 貸出状態 可能
        ''' </summary>
        ''' <returns>検索条件 貸出状態 可能</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_Ok As Boolean = True

        ''' <summary>
        ''' 検索条件 貸出状態 不可
        ''' </summary>
        ''' <returns>検索条件 貸出状態 不可</returns>
        <NotMapped>
        Public Property SearchLeaseStatus_Ng As Boolean = True

        ''' <summary>
        ''' 検索条件 貸出状態
        ''' </summary>
        ''' <returns>検索条件 貸出状態</returns>
        <NotMapped>
        Public Property SearchLeaseStatus As String = String.Empty

        ''' <summary>
        ''' 検索条件 管理ロケーション
        ''' </summary>
        ''' <returns>検索条件 管理ロケーション</returns>
        <NotMapped>
        Public Property SearchMngLocL As String = String.Empty

        ''' <summary>
        '''  管理ロケーションドロップダウンリストデータ
        ''' </summary>
        ''' <returns>管理ロケーションドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property MnglocLDropdownData As List(Of LocationDropdownModel)

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
        '''  カテゴリー　中分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>中分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catMDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        '''  カテゴリー　小分類ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>小分類ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property catSDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        ''' カテゴリ大分類名
        ''' </summary>
        ''' <returns>カテゴリ大分類名</returns>
        Public Property CatLName As String = String.Empty

        ''' <summary>
        ''' カテゴリ中分類名
        ''' </summary>
        ''' <returns>カテゴリ中分類名</returns>
        Public Property CatMName As String = String.Empty

        ''' <summary>
        ''' カテゴリ小分類名
        ''' </summary>
        ''' <returns>カテゴリ小分類名</returns>
        Public Property CatSName As String = String.Empty

        ''' <summary>
        ''' プリンタ情報KEY1
        ''' </summary>
        ''' <returns>プリンタ情報KEY1</returns>
        <NotMapped>
        Public Property PrinterKey As String = String.Empty

        ''' <summary>
        ''' 資産情報名
        ''' </summary>
        ''' <returns>資産情報名</returns>
        <NotMapped>
        Public Property AssetStatusNm As String = String.Empty

        ''' <summary>
        ''' EPCエンコード状態名
        ''' </summary>
        ''' <returns>EPCエンコード状態名</returns>
        <NotMapped>
        Public Property EpcEncStatusNm As String = String.Empty

        ''' <summary>
        ''' タグ紐付け状態名
        ''' </summary>
        ''' <returns>タグ紐付け状態名</returns>
        <NotMapped>
        Public Property TagLinkStatusNm As String = String.Empty

        ''' <summary>
        ''' 資産ID表示非表示
        ''' </summary>
        ''' <returns>資産ID表示非表示</returns>
        <NotMapped>
        Public Property AssetIdDivReg As Boolean = True

        ''' <summary>
        ''' 除却日時表示文字列
        ''' </summary>
        ''' <returns>除却日時表示文字列</returns>
        <NotMapped>
        Public Property RetireTsString As String = String.Empty

        ''' <summary>
        ''' リース期限表示文字列
        ''' </summary>
        ''' <returns>リース期限表示文字列</returns>
        <NotMapped>
        Public Property LeaseLimitString As String = String.Empty

        ''' <summary>
        ''' ○○期限表示文字列
        ''' </summary>
        ''' <returns>期限表示文字列</returns>
        <NotMapped>
        Public Property GenLimitString As String = String.Empty

        ''' <summary>
        ''' 資産購入日表示文字列
        ''' </summary>
        ''' <returns>資産購入日表示文字列</returns>
        <NotMapped>
        Public Property AssetKounyuDateString As String = String.Empty

        ''' <summary>
        ''' 非表示項目：メニュー開閉フラグ
        ''' </summary>
        ''' <returns>非表示項目：メニュー開閉フラグ</returns>
        <NotMapped>
        Public Property OpenFlg As String = String.Empty

        ''' <summary>
        ''' 非表示項目：現在の画像ファイルURL格納用
        ''' </summary>
        ''' <returns>非表示項目：現在の画像ファイルURL格納用</returns>
        <NotMapped>
        Public Property NowImageUrl As String = String.Empty

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
        ''' 表示項目：貸出日数　稼働日フラグ　チェックボックス表示用
        ''' </summary>
        ''' <returns>貸出日数　稼働日フラグ　チェックボックス表示用</returns>
        <NotMapped>
        Public Property DispLeaseDayWorkFlg As Boolean = False

        ''' <summary>
        ''' 表示項目：メンテ日数　稼働日フラグ　チェックボックス表示用
        ''' </summary>
        ''' <returns>メンテ日数　稼働日フラグ　チェックボックス表示用</returns>
        <NotMapped>
        Public Property DispMainteDayWorkFlg As Boolean = False

        ''' <summary>
        ''' 管理ロケーション名
        ''' </summary>
        ''' <returns>表示項目：管理ロケーション名</returns>
        Public Property MngLocLName As String = String.Empty

        ''' <summary>
        ''' 表示項目：更新権限フラグ
        ''' </summary>
        ''' <returns>更新権限フラグ</returns>
        Public Property UPD_FLG As String = String.Empty

        '2023/02/08 Kawamoto ADD START
        ''' <summary>
        ''' 薬品機能（True：ON, False:OFF）
        ''' </summary>
        ''' <returns>薬品機能（True：ON, False:OFF）</returns>
        <NotMapped>
        Public Property DrugsFunc As Boolean = False
        '2023/02/08 Kawamoto ADD END

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' 添付ファイル件数
        ''' </summary>
        ''' <returns>添付ファイル件数</returns>
        <NotMapped>
        Public Property ATTACHMENT_NUM As Integer?
        '2023/03/22 Kawamoto ADD END

        '2023/05/10 Kawamoto ADD START
        ''' <summary>
        ''' 計測器連携する計測器のリスト
        ''' </summary>
        ''' <returns>計測器連携する計測器のリスト</returns>
        <NotMapped>
        Public Property WeighingMachineLinkList As List(Of String)

        ''' <summary>
        ''' 残量(NET重量)
        ''' </summary>
        ''' <returns>残量(NET重量)</returns>
        <NotMapped>
        Public Property RemainingAmount As String
        '2023/05/10 Kawamoto ADD END

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
