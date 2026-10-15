Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 検索条件、入力設定用
    ''' </summary>
    Partial Public Class SearchInputItemModel

        <Key>
        <Column(Order:=0)>
        <StringLength(30)>
        Public Property KEY_DIV As String

        <Key>
        <Column(Order:=1)>
        <StringLength(30)>
        Public Property KEY1 As String

        <StringLength(255)>
        Public Property DATA1 As String

        <StringLength(255)>
        Public Property DATA2 As String

        <StringLength(255)>
        Public Property DATA3 As String

        <StringLength(255)>
        Public Property DATA4 As String

        <StringLength(255)>
        Public Property DATA5 As String

        Public Property DropdownModel As List(Of DropdownModel)

        Public Property DropdownList As SelectList
    End Class
End Namespace
