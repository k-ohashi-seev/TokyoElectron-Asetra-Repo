Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema


Namespace Models
    ''' <summary>
    ''' 項目配置マスタモデル
    ''' </summary>
    <Table("M_COL_SET")>
    Partial Public Class ColSetModel
        <Key>
        <Column(Order:=0)>
        <StringLength(100)>
        Public Property DISP_ID As String

        <Key>
        <Column(Order:=1)>
        <StringLength(10)>
        Public Property LOC_L_CD As String

        <Required>
        Public Property DISP_ORDER As Integer

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
        Public Property DISP_FLG As String

        <Required>
        <StringLength(1)>
        Public Property FIX_COL_FLG As String

        <Required>
        Public Property FIX_COL_CNT As Integer

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