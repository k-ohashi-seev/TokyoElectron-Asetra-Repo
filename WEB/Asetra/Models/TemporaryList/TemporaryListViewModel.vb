Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial
Imports Asetra.Misc

Namespace Models
    Partial Public Class TemporaryListViewModel
        Inherits BaseModel

        <Key>
        Public Property TEMP_ID As Long

        <StringLength(20, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REG_CONST_NO As String

        <StringLength(20, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REL_CONST_NO As String

        Public Property TEMP_DT As Integer

        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property WORK_EMPLOYEE_CD As String

        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property WORK_EMPLOYEE_NAME As String

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

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TEMP_REMARK As String

        Public Property TASK_TS As Long?

        Public Property CONST_CONTENTS As String

        Public Property TEMP_NUM As Integer? = 0

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
        ''' 検索条件 仮置き紐付状態
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchRelConstStatus As String = String.Empty

        ''' <summary>
        ''' 検索条件 紐付工事番号
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchRelConstNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 登録工事番号
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchRegConstNo As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き日
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempDt As String = String.Empty

        ''' <summary>
        ''' 検索条件 工事内容
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchConstContents As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き情報1
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempInfo1 As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き情報2
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempInfo2 As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き情報3
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempInfo3 As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き情報4
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempInfo4 As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き情報5
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempInfo5 As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き情報6
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempInfo6 As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き情報7
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempInfo7 As String = String.Empty

        ''' <summary>
        ''' 検索条件 仮置き備考
        ''' </summary>
        ''' <returns>検索条件 </returns>
        <NotMapped>
        Public Property SearchTempRemark As String = String.Empty

        ''' <summary>
        ''' 表示項目：更新権限フラグ
        ''' </summary>
        ''' <returns>更新権限フラグ</returns>
        Public Property UPD_FLG As String = String.Empty

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
