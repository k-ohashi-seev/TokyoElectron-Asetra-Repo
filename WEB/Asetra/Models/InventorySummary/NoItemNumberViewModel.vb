Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class NoItemNumberViewModel
        Inherits BaseModel

        <Key>
        Public Property NO_ITEM_NUMBER As Int64? = 0

    End Class

End Namespace
