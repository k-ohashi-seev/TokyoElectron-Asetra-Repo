Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' ロケーションマスタ(名称取得用)
    ''' </summary>
    Partial Public Class LocationNameModel

        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property LOC_DIV As String

        <Key>
        <Column(Order:=1)>
        <StringLength(5)>
        Public Property LOC_L_CD As String

        <Key>
        <Column(Order:=2)>
        <StringLength(5)>
        Public Property LOC_M_CD As String

        <Key>
        <Column(Order:=3)>
        <StringLength(5)>
        Public Property LOC_S_CD As String

        <StringLength(60)>
        Public Property LOC_L_NAME As String

        <StringLength(60)>
        Public Property LOC_M_NAME As String

        <StringLength(60)>
        Public Property LOC_S_NAME As String

    End Class
End Namespace
