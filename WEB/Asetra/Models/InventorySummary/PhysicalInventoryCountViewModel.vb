Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class PhysicalInventoryCountViewModel
        Inherits BaseModel

        <Key>
        Public Property PHYINV_STATUS_COUNT As Int64? = 0

    End Class

End Namespace
