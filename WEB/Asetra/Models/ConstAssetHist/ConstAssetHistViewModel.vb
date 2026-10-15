Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 工事資産履歴一覧表示用モデル
    ''' </summary>
    Public Class ConstAssetHistViewModel
        Inherits BaseModel

        <Key>
        <Column(Order:=0)>
        Public Property ASSET_NO As String = String.Empty

        Public Property ASSET_NAME As String = String.Empty

        <StringLength(2)>
        Public Property CONST_WORK_DIV As String

        <StringLength(1)>
        Public Property CONST_MATCH_DIV As String

        Public Property CONST_MATCH_STATUS As Byte

        <StringLength(1)>
        Public Property TEMP_FLAG As String

        <StringLength(255)>
        Public Property TEMP_INFO1 As String

        <StringLength(255)>
        Public Property TEMP_INFO2 As String

        <StringLength(255)>
        Public Property TEMP_INFO3 As String

        <StringLength(255)>
        Public Property TEMP_INFO4 As String

        <StringLength(255)>
        Public Property TEMP_INFO5 As String

        <StringLength(255)>
        Public Property TEMP_INFO6 As String

        <StringLength(255)>
        Public Property TEMP_INFO7 As String

        <StringLength(255)>
        Public Property TEMP_REMARK As String

        Public Property INS_DATETIME As Date

        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String

        Public Property UPD_DATETIME As Date

        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String

        <StringLength(255)>
        Public Property UPD_TASK As String

        ''' <summary>
        '''  工事データ
        ''' </summary>
        ''' <returns>工事データ</returns>
        <NotMapped>
        Public Property tblConstructionListData As List(Of ConstructionListViewModel)

        ''' <summary>
        '''  工事履歴データ
        ''' </summary>
        ''' <returns>工事履歴データ</returns>
        <NotMapped>
        Public Property tblConstructionHistData As List(Of ConstructionHistViewModel)

        ''' <summary>
        ''' 固定列(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntTop As Integer

        ''' <summary>
        ''' 工事資産履歴情報(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoTop As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' 固定列(中部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntMiddle As Integer

        ''' <summary>
        ''' 工事資産履歴情報(中部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoMiddle As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' 固定列(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntBottom As Integer

        ''' <summary>
        ''' 工事資産履歴情報(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoBottom As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
