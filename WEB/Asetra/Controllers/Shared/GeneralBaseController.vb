Imports Asetra.Models

Namespace Controllers
    ''' <summary>
    ''' 一般的な表示を利用するための基底クラス
    ''' BaseControllerを継承します
    ''' </summary>
    Public MustInherit Class GeneralBaseController
        Inherits BaseController

#Region "一覧表示処理"
        ''' <summary>
        ''' ページ遷移のクエリを作成します
        ''' </summary>
        ''' <param name="PageNum">クリックされたページ番号</param>
        ''' <param name="model">モデル</param>
        Protected Overrides Sub ViewQueryPager(ByVal PageNum As Integer, ByVal model As BaseModel)
        End Sub
#End Region

    End Class
End Namespace
