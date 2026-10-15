Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Mvc
Imports System.Web.Routing

Public Module RouteConfig
    Public Sub RegisterRoutes(ByVal routes As RouteCollection)
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")

        '--2019/07/04 nv miyaji upd start
        '--2019/12/24 nv miyaji upd start
        routes.MapRoute(
            name:="Default",
            url:="{controller}/{action}/{id}",
            defaults:=New With {.controller = "License", .action = "Index", .id = UrlParameter.Optional}
        )
        'routes.MapRoute(
        '    name:="Default",
        '    url:="{controller}/{action}/{id}",
        '    defaults:=New With {.controller = "Login", .action = "Index", .id = UrlParameter.Optional}
        ')
        '--2019/12/24 nv miyaji upd end
        'routes.MapRoute(
        '    name:="Default",
        '    url:="{controller}/{action}/{id}",
        '    defaults:=New With {.controller = "Member", .action = "Index", .id = UrlParameter.Optional}
        ')
        '--2019/07/04 nv miyaji upd end
    End Sub
End Module