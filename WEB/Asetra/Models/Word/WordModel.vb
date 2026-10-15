Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ï∂åæê›íËÉ}ÉXÉ^
    ''' </summary>
    <Table("M_WORD")>
    Partial Public Class WordModel
        '<Column(Order:=0)>
        '<Column(Order:=1)>
        '<Column(Order:=2)>
        '<Column(Order:=3)>
        <Key>
        <Column(Order:=0)>
        <StringLength(2)>
        Public Property LANG_DIV As String

        <Key>
        <Column(Order:=1)>
        <StringLength(1)>
        Public Property TOOL_DIV As String

        <Key>
        <Column(Order:=2)>
        <StringLength(1)>
        Public Property WORD_DIV As String

        <Key>
        <Column(Order:=3)>
        <StringLength(6)>
        Public Property WORD_CD As String

        <StringLength(2048)>
        Public Property WORDING As String

        <Required>
        Public Property INS_DATETIME As Date

        <StringLength(15)>
        <Required>
        Public Property INS_EMPLOYEE_CD As String

        <StringLength(60)>
        <Required>
        Public Property INS_EMPLOYEE_NAME As String

        <Required>
        Public Property UPD_DATETIME As Date

        <StringLength(15)>
        <Required>
        Public Property UPD_EMPLOYEE_CD As String

        <StringLength(60)>
        <Required>
        Public Property UPD_EMPLOYEE_NAME As String

        <StringLength(255)>
        <Required>
        Public Property UPD_TASK As String

    End Class

End Namespace
