Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models

    ''' <summary>
    ''' 工事資産取得用モデル(工事番号、資産番号)
    ''' </summary>
    Partial Public Class ConstAssetModel
        '工事番号
        Public Property CONST_NO As String = String.Empty

        '資産番号
        Public Property ASSET_NO As String = String.Empty


    End Class
End Namespace
