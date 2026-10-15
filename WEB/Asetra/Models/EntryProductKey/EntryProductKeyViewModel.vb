Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' プロダクトキー入力画面表示用モデル
    ''' </summary>
    ''' 
    Partial Public Class EntryProductKeyViewModel
        Inherits BaseModel

        ''' <summary>
        ''' プロダクト番号
        ''' </summary>
        ''' <returns>プロダクト番号</returns>
        <Key>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(25, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property PRODUCT_CD As String = String.Empty

        ''' <summary>
        ''' プロダクトキー１
        ''' </summary>
        ''' <returns>プロダクトキー１</returns>
        <StringLength(5, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property PRODUCT_KEY1 As String = String.Empty

        ''' <summary>
        ''' プロダクトキー２
        ''' </summary>
        ''' <returns>プロダクトキー２</returns>
        <StringLength(5, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property PRODUCT_KEY2 As String = String.Empty

        ''' <summary>
        ''' プロダクトキー３
        ''' </summary>
        ''' <returns>プロダクトキー３</returns>
        <StringLength(5, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property PRODUCT_KEY3 As String = String.Empty

        ''' <summary>
        ''' プロダクトキー４
        ''' </summary>
        ''' <returns>プロダクトキー４</returns>
        <StringLength(5, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property PRODUCT_KEY4 As String = String.Empty

        ''' <summary>
        ''' プロダクトキー５
        ''' </summary>
        ''' <returns>プロダクトキー５</returns>
        <StringLength(5, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property PRODUCT_KEY5 As String = String.Empty

        ''' <summary>
        ''' 試用期間
        ''' </summary>
        ''' <returns>試用期間</returns>
        Public Property TRIAL_LIMIT As Integer? = Nothing
    End Class
End Namespace
