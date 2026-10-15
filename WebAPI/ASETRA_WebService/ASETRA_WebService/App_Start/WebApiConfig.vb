Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web.Http

''' <summary>
''' Web API の設定およびサービス
''' </summary>
Public Module WebApiConfig
    Public Sub Register(ByVal config As HttpConfiguration)
        ' Web API ルート
        config.MapHttpAttributeRoutes()

        'クロスサイトスクリプティングを許容する
        config.EnableCors()

        config.Routes.MapHttpRoute(
            name:="DefaultApi",
            routeTemplate:="api/{controller}/{id}",
            defaults:=New With {.id = RouteParameter.Optional}
        )
    End Sub
End Module
