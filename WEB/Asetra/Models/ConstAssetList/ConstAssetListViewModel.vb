Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Public Class ConstAssetListViewModel
        Inherits BaseModel
        <Key>
        <Column(Order:=0)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(20, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_NO As String

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property CONST_START_DT As Integer

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property CONST_END_DT As Integer

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property CONST_LENT_START_DT As Integer

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property CONST_LENT_END_DT As Integer

        Public Property CONST_STATUS As String

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_CONTENTS As String

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_EMPLOYEE_CD As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_EMPLOYEE_NAME As String

        <StringLength(10)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property MNG_LOC_L_CD As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO1 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO2 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO3 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO4 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO5 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO6 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO7 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO8 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO9 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO10 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO11 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO12 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO13 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO14 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO15 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO16 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO17 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO18 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO19 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_INFO20 As String

        Public Property PREV_TEMP_NUM As Integer

        Public Property CONST_IN_NUM As Integer

        Public Property NOW_TEMP_NUM As Integer

        Public Property CONST_OUT_NUM As Integer

        Public Property CUR_LENT_NUM As Integer

        Public Property CUR_TEMP_NUM As Integer

        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property SITE_EMPLOYEE_CD As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property SITE_EMPLOYEE_NAME As String

        Public Property CONST_CONFIRM_TS As Long?

        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_CONFIRM_EMPLOYEE_CD As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_CONFIRM_EMPLOYEE_NAME As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property CONST_CONFIRM_REMARK As String

        <Key>
        <Column(Order:=1)>
        <StringLength(24, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_NO As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_NAME As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ASSET_DIV As String

        <StringLength(1)>
        Public Property CONST_ASSET_STATUS As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TEMP_INFO1 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TEMP_INFO2 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TEMP_INFO3 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TEMP_INFO4 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TEMP_INFO5 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TEMP_INFO6 As String

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TEMP_INFO7 As String

        <StringLength(255)>
        Public Property TEMP_REMARK As String

        <StringLength(10)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property MNG_LOC_L_CD_ASSET As String

        <StringLength(10)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property LOC_L_CD As String

        <StringLength(10)>
        Public Property LOC_M_CD As String

        <StringLength(10)>
        Public Property LOC_S_CD As String

        <StringLength(3)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property CAT_L_CD As String

        <StringLength(3)>
        Public Property CAT_M_CD As String

        <StringLength(3)>
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

        Public Property ASSET_STATUS As Byte = 0

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

        Public Property LEASE_STATUS As Byte = 0

        Public Property LEASE_DT As Integer?

        Public Property LEASE_DUE_DT As Integer?

        Public Property MAINTE_LEASE_NUM As Integer?

        Public Property MAINTE_LEASE_DAY As Integer?

        <StringLength(1)>
        Public Property MAINTE_LEASE_DAY_WORK_FLAG As String

        Public Property MAINTE_DAY As Integer?

        <StringLength(1)>
        Public Property MAINTE_DAY_WORK_FLAG As String

        Public Property GENERIC_LIMIT As Integer?

        Public Property TEMP_ID As Long?

        Public Property LEASE_CNT As Integer = 0

        <StringLength(1)>
        Public Property PREV_TEMP_FLAG As String

        Public Property TASK_TS As Long?

        ''' <summary>
        '''  工事データ
        ''' </summary>
        ''' <returns>工事データ</returns>
        <NotMapped>
        Public Property tblConstructionListData As List(Of ConstructionListViewModel)

        ''' <summary>
        ''' キー
        ''' </summary>
        ''' <returns></returns>
        Public Property KEY_VALUE As String

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
        ''' 検索条件 工事番号
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事開始日
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstStartDt As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事終了日
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstEndDt As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事貸出開始日
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstLentStartDt As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事貸出終了日
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstLentEndDt As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事状態
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstStatus As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事内容
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstContents As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事責任者氏名
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstEmployeeName As String = String.Empty

        ''' <summary>
        ''' 検索条件 拠点ロケ
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchMngLocL As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報1
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo1 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報2
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo2 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報3
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo3 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報4
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo4 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報5
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo5 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報6
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo6 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報7
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo7 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報8
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo8 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報9
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo9 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報10
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo10 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報11
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo11 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報12
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo12 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報13
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo13 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報14
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo14 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報15
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo15 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報16
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo16 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報17
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo17 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報18
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo18 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報19
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo19 As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事情報20
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstInfo20 As String = String.Empty

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
        ''' 検索条件 状態
        ''' </summary>
        ''' <returns>検索条件 状態</returns>
        <NotMapped>
        Public Property SearchConstAssetStatus As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置有無
        ''' </summary>
        ''' <returns>検索条件 仮置有無</returns>
        <NotMapped>
        Public Property SearchTempYesOrNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き場所
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempInfo As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き備考
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempRemark As String = String.Empty

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
        ''' 検索条件 カテゴリ大分類
        ''' </summary>
        ''' <returns>検索条件 カテゴリ大分類</returns>
        <NotMapped>
        Public Property SearchCatL As String = String.Empty

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
        ''' 検索条件 リース管理番号
        ''' </summary>
        ''' <returns>検索条件 リース管理番号</returns>
        <NotMapped>
        Public Property SearchLeaseNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 リース期限期間
        ''' </summary>
        ''' <returns>検索条件 リース期限期間</returns>
        <NotMapped>
        <DisplayFormat(DataFormatString:="{0:yyyy-MM-dd}")>
        Public Property SearchLeaseLimit As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目1
        ''' </summary>
        ''' <returns>検索条件 フリー項目1</returns>
        <NotMapped>
        Public Property SearchFreeItem1 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目2
        ''' </summary>
        ''' <returns>検索条件 フリー項目2</returns>
        <NotMapped>
        Public Property SearchFreeItem2 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目3
        ''' </summary>
        ''' <returns>検索条件 フリー項目3</returns>
        <NotMapped>
        Public Property SearchFreeItem3 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目4
        ''' </summary>
        ''' <returns>検索条件 フリー項目4</returns>
        <NotMapped>
        Public Property SearchFreeItem4 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目5
        ''' </summary>
        ''' <returns>検索条件 フリー項目5</returns>
        <NotMapped>
        Public Property SearchFreeItem5 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目6
        ''' </summary>
        ''' <returns>検索条件 フリー項目6</returns>
        <NotMapped>
        Public Property SearchFreeItem6 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目7
        ''' </summary>
        ''' <returns>検索条件 フリー項目7</returns>
        <NotMapped>
        Public Property SearchFreeItem7 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目8
        ''' </summary>
        ''' <returns>検索条件 フリー項目8</returns>
        <NotMapped>
        Public Property SearchFreeItem8 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目9
        ''' </summary>
        ''' <returns>検索条件 フリー項目9</returns>
        <NotMapped>
        Public Property SearchFreeItem9 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目10
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
        ''' 検索条件 フリー項目12
        ''' </summary>
        ''' <returns>検索条件 フリー項目12</returns>
        <NotMapped>
        Public Property SearchFreeItem12 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目13
        ''' </summary>
        ''' <returns>検索条件 フリー項目13</returns>
        <NotMapped>
        Public Property SearchFreeItem13 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目14
        ''' </summary>
        ''' <returns>検索条件 フリー項目14</returns>
        <NotMapped>
        Public Property SearchFreeItem14 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目15
        ''' </summary>
        ''' <returns>検索条件 フリー項目15</returns>
        <NotMapped>
        Public Property SearchFreeItem15 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目16
        ''' </summary>
        ''' <returns>検索条件 フリー項目16</returns>
        <NotMapped>
        Public Property SearchFreeItem16 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目17
        ''' </summary>
        ''' <returns>検索条件 フリー項目17</returns>
        <NotMapped>
        Public Property SearchFreeItem17 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目18
        ''' </summary>
        ''' <returns>検索条件 フリー項目18</returns>
        <NotMapped>
        Public Property SearchFreeItem18 As String = String.Empty

        ''' <summary>
        ''' 検索条件 フリー項目19
        ''' </summary>
        ''' <returns>検索条件 フリー項目19</returns>
        <NotMapped>
        Public Property SearchFreeItem19 As String = String.Empty

        ''' <summary>
        ''' 管理ロケーション名_工事
        ''' </summary>
        ''' <returns>表示項目：管理ロケーション名_工事</returns>
        Public Property MngLocLName As String = String.Empty

        ''' <summary>
        ''' 管理ロケーション名_資産
        ''' </summary>
        ''' <returns>表示項目：管理ロケーション名_資産</returns>
        Public Property MngLocLNameAsset As String = String.Empty

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
        ''' 固定列(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntTop As Integer

        ''' <summary>
        ''' 工事資産一覧情報(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoTop As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' 固定列(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntBottom As Integer

        ''' <summary>
        ''' 工事資産一覧情報(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoBottom As Dictionary(Of String, SlickGridInfo)
    End Class
End Namespace
