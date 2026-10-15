Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' 工事一覧用モデル
    ''' </summary>
    <Table("M_CONSTRUCTION")>
    Partial Public Class ConstructionListModel
        <Key>
        <StringLength(20)>
        Public Property CONST_NO As String

        Public Property CONST_START_DT As Integer

        Public Property CONST_END_DT As Integer

        Public Property CONST_LENT_START_DT As Integer

        Public Property CONST_LENT_END_DT As Integer

        <StringLength(1)>
        Public Property CONST_STATUS As String

        <StringLength(255)>
        Public Property CONST_CONTENTS As String

        <StringLength(15)>
        Public Property CONST_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property CONST_EMPLOYEE_NAME As String

        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String

        <StringLength(255)>
        Public Property CONST_INFO1 As String

        <StringLength(255)>
        Public Property CONST_INFO2 As String

        <StringLength(255)>
        Public Property CONST_INFO3 As String

        <StringLength(255)>
        Public Property CONST_INFO4 As String

        <StringLength(255)>
        Public Property CONST_INFO5 As String

        <StringLength(255)>
        Public Property CONST_INFO6 As String

        <StringLength(255)>
        Public Property CONST_INFO7 As String

        <StringLength(255)>
        Public Property CONST_INFO8 As String

        <StringLength(255)>
        Public Property CONST_INFO9 As String

        <StringLength(255)>
        Public Property CONST_INFO10 As String

        <StringLength(255)>
        Public Property CONST_INFO11 As String

        <StringLength(255)>
        Public Property CONST_INFO12 As String

        <StringLength(255)>
        Public Property CONST_INFO13 As String

        <StringLength(255)>
        Public Property CONST_INFO14 As String

        <StringLength(255)>
        Public Property CONST_INFO15 As String

        <StringLength(255)>
        Public Property CONST_INFO16 As String

        <StringLength(255)>
        Public Property CONST_INFO17 As String

        <StringLength(255)>
        Public Property CONST_INFO18 As String

        <StringLength(255)>
        Public Property CONST_INFO19 As String

        <StringLength(255)>
        Public Property CONST_INFO20 As String

        Public Property PREV_TEMP_NUM As Integer

        Public Property CONST_IN_NUM As Integer

        Public Property NOW_TEMP_NUM As Integer

        Public Property CONST_OUT_NUM As Integer

        Public Property CUR_LENT_NUM As Integer

        Public Property CUR_TEMP_NUM As Integer

        <StringLength(15)>
        Public Property SITE_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property SITE_EMPLOYEE_NAME As String

        Public Property CONST_CONFIRM_TS As Long?

        <StringLength(15)>
        Public Property CONST_CONFIRM_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property CONST_CONFIRM_EMPLOYEE_NAME As String

        <StringLength(255)>
        Public Property CONST_CONFIRM_REMARK As String

        Public Property TASK_TS As Long?

        ''' <summary>
        ''' 登録日時
        ''' </summary>
        ''' <returns>登録日時</returns>
        <DataType(DataType.Date)>
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
        <DataType(DataType.Date)>
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
        ''' 登録日時 文字列
        ''' </summary>
        ''' <returns>登録日時 文字列</returns>
        <NotMapped>
        Public Property InsDatetimeString As String = String.Empty

        ''' <summary>
        ''' 最終登録更新日時 文字列
        ''' </summary>
        ''' <returns>最終登録更新日時 文字列</returns>
        <NotMapped>
        Public Property UpdDatetimeString As String = String.Empty

        ''' <summary>
        ''' 工事開始日表示文字列
        ''' </summary>
        ''' <returns>工事開始日表示文字列</returns>
        <NotMapped>
        Public Property ConstStartDtString As String = String.Empty

        ''' <summary>
        ''' 工事終了日表示文字列
        ''' </summary>
        ''' <returns>工事終了日表示文字列</returns>
        <NotMapped>
        Public Property ConstEndDtString As String = String.Empty

        ''' <summary>
        ''' 工事貸出開始日表示文字列
        ''' </summary>
        ''' <returns>工事貸出開始日表示文字列</returns>
        <NotMapped>
        Public Property ConstLentStartDtString As String = String.Empty

        ''' <summary>
        ''' 工事貸出終了日表示文字列
        ''' </summary>
        ''' <returns>工事貸出終了日表示文字列</returns>
        <NotMapped>
        Public Property ConstLentEndDtString As String = String.Empty

        ''' <summary>
        ''' 工事情報1 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報1 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO1_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報1 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報1 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO1_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報2 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報2 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO2_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報2 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報2 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO2_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報3 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報3 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO3_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報3 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報3 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO3_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報4 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報4 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO4_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報4 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報4 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO4_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報5 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報5 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO5_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報5 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報5 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO5_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報6 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報6 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO6_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報6 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報6 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO6_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報7 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報7 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO7_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報7 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報7 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO7_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報8 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報8 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO8_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報8 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報8 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO8_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報9 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報9 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO9_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報9 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報9 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO9_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報10 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報10 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO10_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報10 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報10 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO10_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報11 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報11 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO11_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報11 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報11 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO11_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報12 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報12 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO12_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報12 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報12 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO12_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報13 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報13 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO13_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報13 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報13 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO13_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報14 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報14 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO14_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報14 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報14 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO14_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報15 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報15 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO15_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報15 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報15 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO15_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報16 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報16 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO16_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報16 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報16 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO16_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報17 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報17 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO17_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報17 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報17 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO17_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報18 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報18 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO18_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報18 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報18 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO18_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報19 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報19 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO19_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報19 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報19 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO19_TO As String = String.Empty

        ''' <summary>
        ''' 工事情報20 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報20 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO20_FROM As String = String.Empty

        ''' <summary>
        ''' 工事情報20 範囲入力の場合
        ''' </summary>
        ''' <returns>工事情報20 範囲入力の場合</returns>
        <NotMapped>
        Public Property CONST_INFO20_TO As String = String.Empty

        ''' <summary>
        ''' 工事貸出開始日 警告表示用
        ''' </summary>
        ''' <returns>工事貸出開始日 警告表示用</returns>
        <NotMapped>
        Public Property BeforeConstLentStartDtString As String = String.Empty

        ''' <summary>
        ''' 工事貸出終了日 警告表示用
        ''' </summary>
        ''' <returns>工事貸出終了日 警告表示用</returns>
        <NotMapped>
        Public Property BeforeConstLentEndDtString As String = String.Empty

    End Class

End Namespace
