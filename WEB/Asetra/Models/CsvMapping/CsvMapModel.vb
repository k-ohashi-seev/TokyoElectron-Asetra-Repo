Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models
    ''' <summary>
    ''' CSVマッピングマスタモデル
    ''' </summary>
    <Table("M_CSVMAP")>
    Partial Public Class CsvMapModel
        <Key>
        <StringLength(30)>
        Public Property CSV_DIV As String

        <Required>
        <StringLength(20)>
        Public Property CHAR_CD As String

        Public Property INS_DATETIME As Date

        <Required>
        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String

        <Required>
        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String

        Public Property UPD_DATETIME As Date

        <Required>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String

        <Required>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String

        <Required>
        <StringLength(255)>
        Public Property UPD_TASK As String
    End Class

End Namespace
