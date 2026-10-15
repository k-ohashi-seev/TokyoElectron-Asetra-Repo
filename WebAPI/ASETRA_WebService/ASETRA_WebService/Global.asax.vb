Imports System.Web.Http

Public Class WebApiApplication
    Inherits System.Web.HttpApplication

    Protected Sub Application_Start()
        GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
        GlobalConfiguration.Configuration.Filters.Add(New ContlloerLogFilterAttribute())

        'log4netの設定
        log4net.Config.XmlConfigurator.Configure()
    End Sub

End Class
