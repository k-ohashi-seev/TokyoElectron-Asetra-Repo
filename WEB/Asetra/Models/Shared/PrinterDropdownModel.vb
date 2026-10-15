Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' プリンタ選択ドロップダウン用
    ''' </summary>
    Partial Public Class PrinterDropdownModel

        <Key>
        <Column(Order:=0)>
        <StringLength(30)>
        Public Property KEY_DIV As String

        <Key>
        <Column(Order:=1)>
        <StringLength(30)>
        Public Property KEY1 As String

        <Key>
        <Column(Order:=2)>
        <StringLength(30)>
        Public Property KEY2 As String

        <Key>
        <Column(Order:=3)>
        <StringLength(30)>
        Public Property KEY3 As String

        <StringLength(255)>
        Public Property DATA1 As String

        <StringLength(255)>
        Public Property DATA2 As String

    End Class
End Namespace
