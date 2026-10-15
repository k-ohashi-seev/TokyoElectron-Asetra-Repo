Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    Public Class ConstructionListReserveListViewModel
        <Key>
        <Column(Order:=0)>
        Public Property CONST_NO As String

        <Key>
        <Column(Order:=1)>
        Public Property ASSET_NO As String

        Public Property PRODUCT_NO As String

        Public Property ASSET_NAME As String

        Public Property ASSET_NUM As Integer

        Public Property RESERVATION_NUM As Integer

        Public Property AVAILABLE_RESERVATION_NUM As Integer
        Public Property WARN_MAINTE_NUM As String
    End Class
End Namespace