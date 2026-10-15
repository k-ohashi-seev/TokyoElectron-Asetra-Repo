Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace Models
    ''' <summary>
    ''' 拠点情報取得用モデルクラス
    ''' </summary>
    Public Class AuthLocationSFModel
        <Key>
        Public Property LOC_CD As String = String.Empty
        Public Property LOC_NAME As String = String.Empty
        Public Property SHARED_FLG As String = String.Empty
        Public Property UPD_FLG As String = String.Empty
    End Class
End Namespace

