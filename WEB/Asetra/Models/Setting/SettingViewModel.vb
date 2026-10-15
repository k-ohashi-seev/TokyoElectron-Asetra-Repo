Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class SettingViewModel
        Inherits BaseModel

        ''' <summary>
        ''' KEY種別
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=0)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property KEY_DIV As String

        ''' <summary>
        ''' KEY1
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=1)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property KEY1 As String

        ''' <summary>
        ''' KEY2
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=2)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property KEY2 As String

        ''' <summary>
        ''' KEY3
        ''' </summary>
        ''' <returns></returns>
        <Key>
        <Column(Order:=3)>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property KEY3 As String

        ''' <summary>
        ''' 備考
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK As String

        ''' <summary>
        ''' DATA1
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA1 As String

        ''' <summary>
        ''' DATA2
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA2 As String

        ''' <summary>
        ''' DATA3
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA3 As String

        ''' <summary>
        ''' DATA4
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA4 As String

        ''' <summary>
        ''' DATA5
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA5 As String

        ''' <summary>
        ''' DATA6
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA6 As String

        ''' <summary>
        ''' DATA7
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA7 As String

        ''' <summary>
        ''' DATA8
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA8 As String

        ''' <summary>
        ''' DATA9
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA9 As String

        ''' <summary>
        ''' DATA10
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DATA10 As String

        ''' <summary>
        ''' 特定データ区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255)>
        Public Property SPECIAL_DATA_DIV As String

        ''' <summary>
        ''' 機能分類名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(90)>
        Public Property FUNC_CAT_NAME As String

        ''' <summary>
        ''' 機能分類名1
        ''' </summary>
        ''' <returns></returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FUNC_CAT_NAME1 As String

        ''' <summary>
        ''' 機能分類名2
        ''' </summary>
        ''' <returns></returns>
        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FUNC_CAT_NAME2 As String

        ''' <summary>
        ''' 機能分類名3
        ''' </summary>
        ''' <returns></returns>
        <StringLength(30, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property FUNC_CAT_NAME3 As String

        ''' <summary>
        ''' 設定対象区分
        ''' </summary>
        ''' <returns></returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(5, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property SETTING_OBJECT_DIV As String

        ''' <summary>
        ''' 機能分類名_並び順
        ''' </summary>
        ''' <returns></returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property FUNC_CAT_SORT As Integer?

        ''' <summary>
        ''' 更新可否
        ''' </summary>
        ''' <returns></returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property UPD_AUTHORITY_FLG As String

        ''' <summary>
        ''' 削除可否
        ''' </summary>
        ''' <returns></returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        Public Property DEL_AUTHORITY_FLG As String

        ''' <summary>
        ''' 登録日時
        ''' </summary>
        ''' <returns></returns>
        Public Property INS_DATETIME As Date

        ''' <summary>
        ''' 登録社員コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String

        ''' <summary>
        ''' 登録者氏名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String

        ''' <summary>
        ''' 最終登録更新日時
        ''' </summary>
        ''' <returns></returns>
        Public Property UPD_DATETIME As Date

        ''' <summary>
        ''' 最終登録更新社員コード
        ''' </summary>
        ''' <returns></returns>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String

        ''' <summary>
        ''' 最終登録更新者氏名
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String

        ''' <summary>
        ''' 最終登録更新処理
        ''' </summary>
        ''' <returns></returns>
        <StringLength(255)>
        Public Property UPD_TASK As String

        ''' <summary>
        ''' 検索条件 種別
        ''' </summary>
        ''' <returns>検索条件 種別</returns>
        <NotMapped>
        Public Property SearchKeyDiv As String = String.Empty

        ''' <summary>
        ''' 検索条件 機能分類名
        ''' </summary>
        ''' <returns>検索条件 機能分類名</returns>
        <NotMapped>
        Public Property SearchFuncCatName As String = String.Empty

        ''' <summary>
        ''' 検索条件 設定対象区分 HT
        ''' </summary>
        ''' <returns>検索条件 設定対象区分 HT</returns>
        <NotMapped>
        Public Property SearchSettingObjectDivHT As Boolean = True

        ''' <summary>
        ''' 検索条件 設定対象区分 PC
        ''' </summary>
        ''' <returns>検索条件 設定対象区分 PC</returns>
        <NotMapped>
        Public Property SearchSettingObjectDivPC As Boolean = True

        ''' <summary>
        ''' 検索条件 設定対象区分 WEB
        ''' </summary>
        ''' <returns>検索条件 設定対象区分 WEB</returns>
        <NotMapped>
        Public Property SearchSettingObjectDivWEB As Boolean = True

        ''' <summary>
        ''' 検索条件 設定対象区分 ALL
        ''' </summary>
        ''' <returns>検索条件 設定対象区分 ALL</returns>
        <NotMapped>
        Public Property SearchSettingObjectDivALL As Boolean = True

        ''' <summary>
        ''' 削除データキー
        ''' </summary>
        ''' <returns>削除データキー</returns>
        <NotMapped>
        Public Property DeleteKeys As String = String.Empty

        ''' <summary>
        ''' コピー登録フラグ
        ''' </summary>
        ''' <returns>コピー登録フラグ</returns>
        <NotMapped>
        Public Property CopyFlag As String = String.Empty

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' 設定マスタ情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' キー種別残数1フラグ
        ''' </summary>
        ''' <returns>キー種別残数1フラグ</returns>
        <NotMapped>
        Public Property keyDivOnlyFlg As String = String.Empty

    End Class

End Namespace
