Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class IASInventoryViewModel
        Inherits BaseModel

        <Key>
        Public Property IAS_START_DEC_TS As Int64 = 0

        ''' <summary>
        ''' í†ïÎç›å…êî
        ''' </summary>
        ''' <returns></returns>
        Public Property IAS_LOGIC_INV_NUM As Int64 = 0

        ''' <summary>
        ''' é¿íIê≥èÌêî
        ''' </summary>
        ''' <returns></returns>
        Public Property IAS_PHYINV_NUM As Int64 = 0

        ''' <summary>
        ''' ÉçÉPà·Ç¢êî
        ''' </summary>
        ''' <returns></returns>
        Public Property IAS_LOC_DIFF_NUM As Int64 = 0

        ''' <summary>
        ''' åªï®Ç»Çµêî
        ''' </summary>
        ''' <returns></returns>
        Public Property IAS_NO_ITEM_NUM As Int64 = 0

    End Class

End Namespace
