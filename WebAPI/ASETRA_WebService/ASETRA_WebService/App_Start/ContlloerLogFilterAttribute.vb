
Imports System.Web.Http.Controllers
Imports System.Web.Http.Filters
''' <summary>
''' コントローラーログ出力フィルタ処理クラス
''' </summary>
<AttributeUsage(AttributeTargets.Method Or AttributeTargets.Class, Inherited:=True, AllowMultiple:=False)>
Public Class ContlloerLogFilterAttribute
    Inherits ActionFilterAttribute

    'ログファイル
    Private logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    ''' <summary>
    ''' アクション メソッドの実行前に呼び出されます
    ''' </summary>
    ''' <param name="filterContext">コンテキスト</param>
    Public Overrides Sub OnActionExecuting(ByVal filterContext As HttpActionContext)

        '通過(開始)ログファイル出力
        logger.Info($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}:{filterContext.ControllerContext.Controller}:{filterContext.ActionDescriptor.ActionName}:START")

    End Sub

    ''' <summary>
    ''' アクション メソッドの実行後に呼び出されます
    ''' </summary>
    ''' <param name="filterContext">コンテキスト</param>
    Public Overrides Sub OnActionExecuted(ByVal filterContext As HttpActionExecutedContext)

        '通過(終了)ログファイル出力
        logger.Info($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}:{filterContext.ActionContext.ControllerContext.Controller}:{filterContext.ActionContext.ActionDescriptor.ActionName}:END")

    End Sub
End Class