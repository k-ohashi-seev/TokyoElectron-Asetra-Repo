Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class AssetStatusCountViewModel
        Inherits BaseModel

        <Key>
        Public Property ASSET_STATUS_COUNT As Int64? = 0

    End Class

End Namespace
