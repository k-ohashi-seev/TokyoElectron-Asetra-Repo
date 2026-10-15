Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class InventorySummaryHedViewModel
        Inherits BaseModel

        <Key>
        <Column(Order:=0)>
        <StringLength(1)>
        Public Property PKEY As String

        '20231002 DEL Start Shirakawa
        'Public Property NUMBER_OF_ASSETS As Int64? = 0
        'Public Property NUMBER_OF_STOCKS As Int64? = 0
        'Public Property NUMBER_OF_LOANS As Int64? = 0
        'Public Property REAL_SHELF As Int64? = 0
        'Public Property UNCONFIRMED As Int64? = 0
        '20231002 DEL Start Shirakawa

    End Class

End Namespace
