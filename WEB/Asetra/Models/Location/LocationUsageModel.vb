Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' ロケーションマスタ(ロケ使用区分)
    ''' </summary>
    Partial Public Class LocationUsageModel
        Inherits BaseModel

        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property LocUsage_L As String = String.Empty

        <Key>
        <Column(Order:=1)>
        <StringLength(1)>
        Public Property LocUsage_M As String = String.Empty

        <Key>
        <Column(Order:=2)>
        <StringLength(1)>
        Public Property LocUsage_S As String = String.Empty

    End Class
End Namespace
