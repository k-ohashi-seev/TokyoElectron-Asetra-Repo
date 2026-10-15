Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' 予約申請登録画面表示用モデル
    ''' </summary>
    Partial Public Class LendingEntryViewModel
        Inherits BaseModel
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.Identity)>
        Public Property RESERVE_ID As Long = 0

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(15)>
        Public Property REQ_EMP_CD As String = String.Empty

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(15)>
        Public Property CHARGE_EMP_CD As String = String.Empty

        Public Property REQUEST_DT As Integer = 0

        Public Property REAL_START_DT As Integer = 0

        Public Property REAL_END_DT As Integer = 0

        Public Property USER_START_DT As Integer = 0

        Public Property USER_END_DT As Integer = 0

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(15)>
        Public Property LEASE_PLACE_CD As String = String.Empty

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(30)>
        Public Property LEASE_USE As String = String.Empty

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String = String.Empty

        '<Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>   DEL sv sato 2022/10/03 未入力チェックはコントローラで行うため削除
        <StringLength(16, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEST_NAME1 As String = String.Empty

        <StringLength(16, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEST_NAME2 As String = String.Empty

        '2023/03/22 Kawamoto ADD START
        <StringLength(16, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property TO_DEST_NAME As String = String.Empty
        '2023/03/22 Kawamoto ADD END

        '<Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>   DEL sv sato 2022/10/03 未入力チェックはコントローラで行うため削除
        <StringLength(16, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEST_ADDRESS1 As String = String.Empty

        <StringLength(16, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEST_ADDRESS2 As String = String.Empty

        <StringLength(16, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEST_ADDRESS3 As String = String.Empty

        <StringLength(14, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEST_PHONE_NUMBER As String = String.Empty

        <StringLength(8, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEST_ZIP_CODE As String = String.Empty

        '2023/03/22 Kawamoto ADD START
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(15)>
        Public Property RET_LEASE_PLACE_CD As String = String.Empty

        <StringLength(16, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property RET_ADDRESS1 As String = String.Empty

        <StringLength(16, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property RET_ADDRESS2 As String = String.Empty

        <StringLength(16, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property RET_ADDRESS3 As String = String.Empty

        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(8, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property RET_ZIP_CODE As String = String.Empty
        '2023/03/22 Kawamoto ADD END

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK As String = String.Empty

        <StringLength(255, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property REMARK_INTERNAL As String = String.Empty

        <StringLength(100, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property DEST_NO As String = String.Empty

        Public Property LEASE_CHG_TIMES As Integer = 0

        <StringLength(1)>
        Public Property RESERVE_STATUS As String = String.Empty

        Public Property INS_DATETIME As Date? = Nothing

        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String = String.Empty

        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String = String.Empty

        Public Property UPD_DATETIME As Date? = Nothing

        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String = String.Empty

        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String = String.Empty

        <StringLength(255)>
        Public Property UPD_TASK As String = String.Empty

        ''' <summary>
        ''' 表示項目：貸出番号
        ''' 2022/07/25 sv sato del ※NotMapped属性を削除 
        ''' </summary>
        ''' <returns>表示項目：貸出番号</returns>
        <StringLength(18, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property ReserveIdString As String = String.Empty

        ''' <summary>
        ''' 表示項目：管理ロケーション
        ''' </summary>
        ''' <returns>表示項目：管理ロケーション</returns>
        <NotMapped>
        Public Property LocLName As String = String.Empty

        ''' <summary>
        ''' 実貸出開始日
        ''' </summary>
        ''' <returns>実貸出開始日</returns>
        <NotMapped>
        Public Property RealStartDtString As String = String.Empty

        ''' <summary>
        ''' 実貸出終了日
        ''' </summary>
        ''' <returns>実貸出終了日</returns>
        <NotMapped>
        Public Property RealEndDtString As String = String.Empty

        ''' <summary>
        ''' ユーザー貸出開始日
        ''' </summary>
        ''' <returns>ユーザー貸出開始日</returns>
        <NotMapped>
        Public Property UserStartDtString As String = String.Empty

        ''' <summary>
        ''' ユーザー貸出終了日
        ''' </summary>
        ''' <returns>ユーザー貸出終了日</returns>
        <NotMapped>
        Public Property UserEndDtString As String = String.Empty

        ''' <summary>
        '''  グリッドリストデータ
        ''' </summary>
        ''' <returns>グリッドリストデータ</returns>
        <NotMapped>
        Public Property ReserveDetailData As List(Of ReserveDetailViewModel)

        ''' <summary>
        ''' 表示項目：追加する資産No
        ''' </summary>
        ''' <returns>表示項目：追加する資産No</returns>
        <NotMapped>
        Public Property AddAssetNo As String = String.Empty

        ''' <summary>
        ''' 表示項目：貸出終了日
        ''' </summary>
        ''' <returns>表示項目：貸出終了日</returns>
        <NotMapped>
        Public Property EndDtString As String = String.Empty

        ''' <summary>
        ''' 隠し項目：引継ぎ担当者コード
        ''' </summary>
        ''' <returns>隠し項目：引継ぎ担当者コード</returns>
        <NotMapped>
        Public Property HikiEmployeeCd As String = String.Empty

        ''' <summary>
        ''' 表示項目：引継ぎ担当者名
        ''' </summary>
        ''' <returns>表示項目：引継ぎ担当者名</returns>
        <NotMapped>
        Public Property HikiEmployeeNm As String = String.Empty

        ''' <summary>
        ''' 表示項目：受渡日
        ''' </summary>
        ''' <returns>表示項目：受渡日</returns>
        <NotMapped>
        Public Property HikiDtString As String = String.Empty

        ''' <summary>
        ''' 隠し項目：遷移元画面
        ''' </summary>
        ''' <returns>隠し項目：遷移元画面</returns>
        <NotMapped>
        Public Property SenimotoKbn As Integer = 0

        ''' <summary>
        ''' 隠し項目：詳細権限区分
        ''' </summary>
        ''' <returns>隠し項目：詳細権限区分</returns>
        <NotMapped>
        Public Property AuthProcFlg As String = String.Empty

        ''' <summary>
        ''' 隠し項目：実貸出開始日を非活性にするフラグ
        ''' </summary>
        ''' <returns>隠し項目：実貸出開始日を非活性にするフラグ</returns>
        <NotMapped>
        Public Property isNotEnableRealStartDt As Boolean = False

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
        ''' 警告メンテ区分
        ''' </summary>
        ''' <returns>警告メンテ区分</returns>
        <NotMapped>
        Public Property WarnMainteKbn As String = String.Empty

    End Class
End Namespace
