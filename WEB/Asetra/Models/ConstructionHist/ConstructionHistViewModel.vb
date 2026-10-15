Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Public Class ConstructionHistViewModel
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
        Public Property CONST_WORK_TS As Long

        <Key>
        <Column(Order:=2)>
        <StringLength(2)>
        Public Property CONST_WORK_DIV As String

        <StringLength(1)>
        Public Property CONST_MATCH_DIV As String

        Public Property LENT_NUM As Integer

        Public Property LEASE_NUM As Integer

        Public Property TEMP_REG_NUM As Integer

        Public Property CONST_MATCH_TTLNUM As Integer

        Public Property CONST_IN_MATCH_NUM As Integer

        Public Property CONST_OUT_MATCH_NUM As Integer

        Public Property CONST_IN_NUM_HIST As Integer

        Public Property TEMP_NUM As Integer

        Public Property CONST_OUT_NUM_HIST As Integer

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
        ''' 検索条件 作業日時
        ''' </summary>
        ''' <returns>検索条件 作業日時</returns>
        <NotMapped>
        Public Property SearchConstWorkTs As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事作業区分
        ''' </summary>
        ''' <returns>検索条件 工事作業区分</returns>
        <NotMapped>
        Public Property SearchConstWorkDiv As String = String.Empty

        ''' <summary>
        ''' 検索条件 照合結果区分
        ''' </summary>
        ''' <returns>検索条件 照合結果区分</returns>
        <NotMapped>
        Public Property SearchConstMatchDiv As String = String.Empty

        ''' <summary>
        ''' 管理ロケーション名
        ''' </summary>
        ''' <returns>表示項目：管理ロケーション名</returns>
        Public Property MngLocLName As String = String.Empty

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
