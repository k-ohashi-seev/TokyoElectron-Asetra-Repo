Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    Partial Public Class SearchMainteViewModel
        Inherits BaseModel
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.Identity)>
        Public Property MAINTE_ID As Long = 0

        Public Property RESERVE_ID As Long = 0

        <StringLength(24)>
        Public Property ASSET_NO As String = String.Empty

    End Class

End Namespace

