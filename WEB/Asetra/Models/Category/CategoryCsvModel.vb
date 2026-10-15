Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' カテゴリーマスタ(CSV用)
    ''' </summary>
    Partial Public Class CategoryCsvModel
        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property CAT_DIV As String

        <Key>
        <Column(Order:=1)>
        <StringLength(8)>
        Public Property CAT_L_CD As String

        <Key>
        <Column(Order:=2)>
        <StringLength(8)>
        Public Property CAT_M_CD As String

        <Key>
        <Column(Order:=3)>
        <StringLength(8)>
        Public Property CAT_S_CD As String

        <StringLength(60)>
        Public Property CAT_L_NAME As String

        <StringLength(60)>
        Public Property CAT_M_NAME As String

        <StringLength(60)>
        Public Property CAT_S_NAME As String

        ''' <summary>
        ''' リース期限アラート区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property LEASE_LIMIT_ALERT_DIV As String = String.Empty

        ''' <summary>
        ''' リース期限何日前
        ''' </summary>
        ''' <returns></returns>
        Public Property LEASE_LIMIT_BEF_DAYS As Integer? = Nothing

        ''' <summary>
        ''' ○○期限アラート区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property GENERIC_LIMIT_ALERT_DIV As String = String.Empty

        ''' <summary>
        ''' ○○期限何日前
        ''' </summary>
        ''' <returns></returns>
        Public Property GENERIC_LIMIT_BEF_DAYS As Integer? = Nothing

        ''' <summary>
        ''' 貸出アラート区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property LEASE_ALERT_DIV As String = String.Empty

        ''' <summary>
        ''' 貸出アラート回数
        ''' </summary>
        ''' <returns></returns>
        Public Property LEASE_ALERT_CNT As Integer? = Nothing

        ''' <summary>
        ''' 貸出上限数
        ''' </summary>
        ''' <returns></returns>
        Public Property LEASE_UPPER_LIMIT As Integer? = Nothing

        ''' <summary>
        ''' 貸出上限エラー区分
        ''' </summary>
        ''' <returns></returns>
        <StringLength(1)>
        Public Property LEASE_UPPER_ERR_DIV As String = String.Empty

        ''' <summary>
        ''' 資産ID登録方法
        ''' </summary>
        ''' <returns>資産ID登録方法</returns>
        Public Property ASSET_ID_REG_DIV As Byte? = Nothing

        ''' <summary>
        ''' EPC領域サイズ
        ''' </summary>
        ''' <returns>EPC領域サイズ</returns>
        Public Property EPC_SIZE As Integer? = Nothing

        ''' <summary>
        ''' EPC事業所キー
        ''' </summary>
        ''' <returns></returns>
        <StringLength(30)>
        Public Property EPC_COMPANY_KEY As String = String.Empty

        ''' <summary>
        ''' EPC資産タイプ
        ''' </summary>
        ''' <returns></returns>
        <StringLength(60)>
        Public Property EPC_ASSET_TYPE As String = String.Empty
    End Class
End Namespace
