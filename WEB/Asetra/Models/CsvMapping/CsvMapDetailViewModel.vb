Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' CSVマッピング明細マスタビュー用モデル
    ''' </summary>
    Partial Public Class CsvMapDetailViewModel
        Inherits BaseModel
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

        '2023/02/06 ADD Start Shirakawa 
        <Required>
        <StringLength(10)>
        Public Property SPEC_CAN_DIV As String
        '2023/02/06 ADD END Shirakawa

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

        Public Property CSV_KOUMOKU As String

        ''' <summary>
        ''' コンボボックス表示 文字コード
        ''' </summary>
        ''' <returns>コンボボックス表示 文字コード</returns>
        <NotMapped>
        Public Property CHAR_CD As String = String.Empty

        ''' <summary>
        ''' 検索項目：CSV区分
        ''' </summary>
        ''' <returns>検索項目：CSV区分</returns>
        <NotMapped>
        Public Property SearchCsvDiv As String = CsvDiv.Asset

        ''' <summary>
        ''' グリッド適用情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)
    End Class

End Namespace


