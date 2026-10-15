Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Namespace Models
    ''' <summary>
    ''' ëÄçÏÉçÉOópÉÇÉfÉã
    ''' </summary>
    <Table("T_OPERATION_LOG")>
    Partial Public Class OperationLogModel
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property OPE_LOG_ID As Long

        Public Property PROCESS_DT As Integer?

        <StringLength(9)>
        Public Property PROCESS_TM As String

        <StringLength(1)>
        Public Property PROCESS_TOOL_DIV As String

        <StringLength(2)>
        Public Property PROCESS_CAT As String

        <StringLength(5)>
        Public Property LOC_L_CD As String

        <StringLength(5)>
        Public Property LOC_M_CD As String

        <StringLength(5)>
        Public Property LOC_S_CD As String

        <StringLength(60)>
        Public Property LOC_L_NAME As String

        <StringLength(60)>
        Public Property LOC_M_NAME As String

        <StringLength(60)>
        Public Property LOC_S_NAME As String

        <StringLength(255)>
        Public Property PROCESS_CONTENTS As String

        <Required>
        <StringLength(15)>
        Public Property OPE_EMPLOYEE_CD As String

        <Required>
        <StringLength(60)>
        Public Property OPE_EMPLOYEE_NAME As String

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
