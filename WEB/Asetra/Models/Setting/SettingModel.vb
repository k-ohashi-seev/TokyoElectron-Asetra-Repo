Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ê›íËÉ}ÉXÉ^
    ''' </summary>
    <Table("M_SETTING")>
    Partial Public Class SettingModel
        '<Column(Order:=0)>
        '<Column(Order:=1)>
        '<Column(Order:=2)>
        '<Column(Order:=3)>
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
        Public Property REMARK As String

        <StringLength(255)>
        Public Property DATA1 As String

        <StringLength(255)>
        Public Property DATA2 As String

        <StringLength(255)>
        Public Property DATA3 As String

        <StringLength(255)>
        Public Property DATA4 As String

        <StringLength(255)>
        Public Property DATA5 As String

        <StringLength(255)>
        Public Property DATA6 As String

        <StringLength(255)>
        Public Property DATA7 As String

        <StringLength(255)>
        Public Property DATA8 As String

        <StringLength(255)>
        Public Property DATA9 As String

        <StringLength(255)>
        Public Property DATA10 As String

        <StringLength(255)>
        Public Property SPECIAL_DATA_DIV As String

        <StringLength(30)>
        <Required>
        Public Property FUNC_CAT_NAME1 As String

        <StringLength(30)>
        Public Property FUNC_CAT_NAME2 As String

        <StringLength(30)>
        Public Property FUNC_CAT_NAME3 As String

        <StringLength(5)>
        <Required>
        Public Property SETTING_OBJECT_DIV As String

        <Required>
        Public Property FUNC_CAT_SORT As Integer?

        <StringLength(1)>
        <Required>
        Public Property UPD_AUTHORITY_FLG As String

        <StringLength(1)>
        <Required>
        Public Property DEL_AUTHORITY_FLG As String

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

        <NotMapped>
        Public Property EDIT_SPECIAL_DATA_DIV As String
        <NotMapped>
        Public Property EDIT_INS_DATETIME As String
        <NotMapped>
        Public Property EDIT_INS_EMPLOYEE_CD As String
        <NotMapped>
        Public Property EDIT_INS_EMPLOYEE_NAME As String

        <NotMapped>
        Public Property EDIT_UPD_DATETIME As String
        <NotMapped>
        Public Property EDIT_UPD_EMPLOYEE_CD As String
        <NotMapped>
        Public Property EDIT_UPD_EMPLOYEE_NAME As String
        <NotMapped>
        Public Property EDIT_UPD_TASK As String

    End Class

End Namespace
