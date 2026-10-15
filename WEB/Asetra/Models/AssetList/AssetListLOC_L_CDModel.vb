Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

'2023/02/10 ADD Shirakawa
Namespace Models

    ''' <summary>
    ''' 資産用モデル
    ''' </summary>
    Public Class AssetListLOC_L_CDModel
        <Key>
        <StringLength(10)>
        Public Property LOC_L_CD As String

    End Class
End Namespace
