Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 仮置資産一覧表示用モデル
    ''' </summary>
    Public Class TempAssetListViewModel
        Inherits BaseModel

        <Key>
        <Column(Order:=0)>
        Public Property TEMP_ID As Long

        <Key>
        <StringLength(24)>
        <Column(Order:=1)>
        Public Property ASSET_NO As String = String.Empty

        Public Property ASSET_NAME As String = String.Empty

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
        '''  仮置き一覧データ
        ''' </summary>
        ''' <returns>仮置き一覧データ</returns>
        <NotMapped>
        Public Property tblTemporaryListData As List(Of TemporaryListViewModel)

        ''' <summary>
        ''' 固定列(上部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCntTop As Integer

        ''' <summary>
        ''' 仮置資産情報(上部)
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
        ''' 仮置資産情報(下部)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfoBottom As Dictionary(Of String, SlickGridInfo)

    End Class
End Namespace
