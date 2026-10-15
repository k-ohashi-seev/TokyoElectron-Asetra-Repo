Imports Asetra.Misc
''' <summary>
''' フィルタ処理クラス
''' </summary>
Public Class AllFilterAttribute
    Inherits ActionFilterAttribute
    Implements IActionFilter, IExceptionFilter

    'ログファイル
    Private logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    ''' <summary>
    ''' アクション メソッドの実行前に ASP.NET MVC フレームワークによって呼び出されます
    ''' </summary>
    ''' <param name="filterContext">コンテキスト</param>
    Private Sub IActionFilter_OnActionExecuting(ByVal filterContext As ActionExecutingContext) _
        Implements IActionFilter.OnActionExecuting

        '通過(開始)ログファイル出力
        logger.Info($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}:{filterContext.RouteData.Values("controller")}:{filterContext.ActionDescriptor.ActionName}:START")

        'コントローラー名を取得する
        Dim controllerName As String = filterContext.RouteData.Values("controller").ToString()

        '前回のコントローラー名を取得する
        Dim beforeControllerName As String = filterContext.HttpContext.Session(CommonSessionKey.CONTROLLER_NAME)

        '前回と今回のコントローラー名が異なる場合は、選択状態保持用のセッションをクリアする
        If controllerName <> beforeControllerName Then
            filterContext.HttpContext.Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)    '選択キー
            filterContext.HttpContext.Session.Remove(CommonSessionKey.LIST_VIEW_SQL)        '一覧表示SQL
            filterContext.HttpContext.Session.Remove(CommonSessionKey.LIST_VIEW_PARAM)      '一覧表示SQLパラメーター
            '全選択ボタン押下フラグをクリアする  '2020/02/12 nv miyaji　　
            filterContext.HttpContext.Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
            '棚卸進捗状況照会用
            filterContext.HttpContext.Session.Remove(CommonSessionKey.LIST_EDITED_CONTAINS) '選択キー
        End If

        'コントローター名を更新する
        filterContext.HttpContext.Session(CommonSessionKey.CONTROLLER_NAME) = controllerName

    End Sub

    ''' <summary>
    ''' アクション メソッドの実行後に ASP.NET MVC フレームワークによって呼び出されます
    ''' </summary>
    ''' <param name="filterContext">コンテキスト</param>
    Private Sub IActionFilter_OnActionExecuted(ByVal filterContext As ActionExecutedContext) _
        Implements IActionFilter.OnActionExecuted

        '通過(終了)ログファイル出力
        logger.Info($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}:{filterContext.RouteData.Values("controller")}:{filterContext.ActionDescriptor.ActionName}:END")

    End Sub

    ''' <summary>
    ''' 例外が発生したときに呼び出されます
    ''' </summary>
    ''' <param name="filterContext">コンテキスト</param>
    Private Sub IExceptionFilter_OnException(ByVal filterContext As ExceptionContext) Implements IExceptionFilter.OnException

        'エラーログファイル出力
        logger.Error($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}:{filterContext.Exception.Message}:{filterContext.Exception.StackTrace}")

    End Sub

End Class