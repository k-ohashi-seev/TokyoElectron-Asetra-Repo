Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 工事マスタ取得用モデルクラス
    ''' </summary>
    Public Class MstConstructionModel

        <Key>
        Public Property CONST_NO As String = String.Empty

        Public Property CONST_START_DT As Long? = Nothing

        Public Property CONST_END_DT As Long? = Nothing

        Public Property CONST_LENT_START_DT As Long? = Nothing

        Public Property CONST_LENT_END_DT As Long? = Nothing

        Public Property CONST_STATUS As String = String.Empty

        Public Property CONST_CONTENTS As String = String.Empty

        Public Property CONST_EMPLOYEE_CD As String = String.Empty

        Public Property CONST_EMPLOYEE_NAME As String = String.Empty

        Public Property MNG_LOC_L_CD As String = String.Empty

        Public Property CONST_INFO1 As String = String.Empty

        Public Property CONST_INFO2 As String = String.Empty

        Public Property CONST_INFO3 As String = String.Empty

        Public Property CONST_INFO4 As String = String.Empty

        Public Property CONST_INFO5 As String = String.Empty

        Public Property CONST_INFO6 As String = String.Empty

        Public Property CONST_INFO7 As String = String.Empty

        Public Property CONST_INFO8 As String = String.Empty

        Public Property CONST_INFO9 As String = String.Empty

        Public Property CONST_INFO10 As String = String.Empty

        Public Property CONST_INFO11 As String = String.Empty

        Public Property CONST_INFO12 As String = String.Empty

        Public Property CONST_INFO13 As String = String.Empty

        Public Property CONST_INFO14 As String = String.Empty

        Public Property CONST_INFO15 As String = String.Empty

        Public Property CONST_INFO16 As String = String.Empty

        Public Property CONST_INFO17 As String = String.Empty

        Public Property CONST_INFO18 As String = String.Empty

        Public Property CONST_INFO19 As String = String.Empty

        Public Property CONST_INFO20 As String = String.Empty

        Public Property PREV_TEMP_NUM As Integer? = Nothing

        Public Property CONST_IN_NUM As Integer? = Nothing

        Public Property NOW_TEMP_NUM As Integer? = Nothing

        Public Property CONST_OUT_NUM As Integer? = Nothing

        Public Property CUR_LENT_NUM As Integer? = Nothing

        Public Property CUR_TEMP_NUM As Integer? = Nothing

        Public Property SITE_EMPLOYEE_CD As String = String.Empty

        Public Property SITE_EMPLOYEE_NAME As String = String.Empty

        Public Property CONST_CONFIRM_TS As Long? = Nothing

        Public Property CONST_CONFIRM_EMPLOYEE_CD As String = String.Empty

        Public Property CONST_CONFIRM_EMPLOYEE_NAME As String = String.Empty

        Public Property CONST_CONFIRM_REMARK As String = String.Empty
    End Class
End Namespace
