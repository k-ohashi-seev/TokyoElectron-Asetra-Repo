Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    Partial Public Class AssetViewModel
        Inherits BaseModel

        <Key>
        <StringLength(24)>
        Public Property ASSET_NO As String
        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String

        Public Property ASSET_STATUS As Byte

        <StringLength(60)>
        Public Property LOC_L_NAME As String

        <StringLength(60)>
        Public Property LOC_M_NAME As String

        <StringLength(60)>
        Public Property LOC_S_NAME As String

    End Class

End Namespace
