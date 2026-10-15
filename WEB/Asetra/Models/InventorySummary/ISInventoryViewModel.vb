Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class ISInventoryViewModel
        Inherits BaseModel

        '20230929 ADD Shirakawa
        <Key>
        Public Property INV_ID As Int64? = 0
        Public Property START_DEC_TS As Int64? = 0
        Public Property START_DEC_EMPLOYEE_CD As String = ""
        Public Property START_DEC_NAME As String = ""
        Public Property INV_NAME As String = ""
        Public Property INV_CONFIRM_TS As Int64? = 0
        Public Property INV_CONFIRM_EMPLOYEE_CD As String = ""
        Public Property INV_CONFIRM_NAME As String = ""
        '20230929 DEL Start Shirakawa
        'Public Property LOGIC_INV_NUM As Int64? = 0
        'Public Property PHYINV_NUM As Int64? = 0
        'Public Property LOC_DIFF_NUM As Int64? = 0
        'Public Property NO_ITEM_NUM As Int64? = 0
        '20230929 DEL End Shirakawa
        Public Property INV_ALL_CONFIRM_TS As Int64? = 0
        Public Property UPD_DATETIME As DateTime?
        Public Property UPD_EMPLOYEE_CD As String = ""
        Public Property UPD_EMPLOYEE_NAME As String = ""
        Public Property UPD_TASK As String = ""

    End Class

End Namespace
