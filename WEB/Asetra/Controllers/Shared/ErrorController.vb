Namespace Controllers
    ''' <summary>
    ''' エラーページの表示に関するクラス
    ''' </summary>
    Public Class ErrorController
        Inherits System.Web.Mvc.Controller

        ''' <summary>
        ''' エラーページ表示(システムエラー)
        ''' </summary>
        ''' <returns>View表示内容</returns>
        Public Function Index() As ActionResult
            Return View()
        End Function

        ''' <summary>
        ''' エラーページ表示(ページが存在しない)
        ''' </summary>
        ''' <returns>View表示内容</returns>
        Public Function NotFound() As ActionResult
            Return View()
        End Function

        ''' <summary>
        ''' エラーページ表示(セッションタイムアウト)
        ''' </summary>
        ''' <returns>View表示内容</returns>
        Public Function NoSession() As ActionResult
            Return View()
        End Function

    End Class
End Namespace
