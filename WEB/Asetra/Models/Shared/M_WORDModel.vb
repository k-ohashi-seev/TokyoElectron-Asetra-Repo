Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models
    ''' <summary>
    ''' 多言語化用モデルクラス
    ''' </summary>
    Partial Public Class M_WORDModel

        <Key>
        <Required>
        Public Property MESSAGE_KEY As String = String.Empty

        <Required>
        <StringLength(255)>
        Public Property WORDING As String = String.Empty

    End Class
End Namespace
