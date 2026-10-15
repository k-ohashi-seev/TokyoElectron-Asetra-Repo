Imports System.Collections.Generic

Public Class ApiResponse
    Public Property ok As Boolean
    Public Property message As String
    Public Property err As String
    '[CC431] ADD Start 20261009
    ''' <summary>エラーの項目（詳細モーダルでエラー項目を赤く表示するため。値は ChangeAssetResponse.errColumn と同じ）</summary>
    Public Property errColumn As String
    '[CC431] ADD End 20261009
End Class
