Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' CSVマッピング明細マスタモデル
    ''' </summary>
    <Table("M_CSVMAP_DETAIL")>
    Partial Public Class CsvMapDetailModel
        <Key>
        <Column(Order:=0)>
        <StringLength(30)>
        Public Property CSV_DIV As String

        <Key>
        <Column(Order:=1)>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property CSV_NO As Integer

        <Required>
        <StringLength(1)>
        Public Property TOOL_DIV As String

        <Required>
        <StringLength(1)>
        Public Property WORD_DIV As String

        <Required>
        <StringLength(6)>
        Public Property WORD_CD As String

        <Required>
        <StringLength(1)>
        Public Property REQUIRED_ITEM_FLAG As String

        <Required>
        <StringLength(1)>
        Public Property SPECIFIED_DIV As String

        Public Property CSV_COL_NO As Integer?

        <StringLength(255)>
        Public Property CSV_FIX_VAL As String

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

