Imports System.Web.Optimization
Imports Asetra.Misc

Public Class MvcApplication
    Inherits System.Web.HttpApplication


    ''' <summary>
    ''' アプリケーション開始時に発生します
    ''' </summary>
    Protected Sub Application_Start()
        AreaRegistration.RegisterAllAreas()
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
        RouteConfig.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles(BundleTable.Bundles)

        'log4netの設定
        log4net.Config.XmlConfigurator.Configure()
        AreaRegistration.RegisterAllAreas()

        ''設定情報読込    2019/08/25 MessageRfid に組み込む（メッセージ取得時に設定情報の読込が必要なため）
        'Dim setting As SettingRfid = New SettingRfid()
        'Application.Lock()
        'setting.Load()
        'Application(CommonConst.KEY_APP_SETTING) = setting
        'Application.UnLock()

        '多言語化設定
        Dim multi As MessageRfid = New MessageRfid()
        Application.Lock()
        multi.Load()
        Application(CommonConst.KEY_APP_LANGUAGE) = multi

        'サーバー側モジュールでも MessageRfid を使用できるようここで移送
        Misc.CommonRfid.MessageRfid = multi

        Application.UnLock()

    End Sub

    ''' <summary>
    ''' セッションが開始されたときに発生します
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)

        'ログイン画面以外の場合
        If Me.Request.RawUrl.IndexOf("/Login") < 0 Then
            'ユーザIDが存在しない場合
            If IsNothing(Session("UserId")) Then
                'セッションタイムアウト画面に遷移
                'Response.Redirect("~/Error/NoSession")
            End If

        End If

    End Sub

End Class
