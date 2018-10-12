
Imports System.IO
Imports System.Web.Mvc
Imports System.Web.Routing

Public Class Global_asax
    Inherits HttpApplication

    Sub Application_Start(sender As Object, e As EventArgs)
        ' Fires when the application is started
        InitializeMvc()
    End Sub

    Sub InitializeMvc()
        ViewEngines.Engines.Clear()
        Dim razorEngine As RazorViewEngine = New RazorViewEngine()

        'Register the ViewPaths from projects outside the VB.NET web form
        Dim viewpath As String = Server.MapPath("~/ViewPaths.csv")
        Using sr As StreamReader = New StreamReader(viewpath)
            Dim line As String
            While sr.Peek() >= 0
                line = sr.ReadLine()
                Dim array As String() = line.Split({","}, StringSplitOptions.RemoveEmptyEntries).Select(Function(s) s.Trim()).ToArray()
                razorEngine.ViewLocationFormats = razorEngine.ViewLocationFormats.Concat(array).ToArray()
                razorEngine.PartialViewLocationFormats = razorEngine.PartialViewLocationFormats.Concat(array).ToArray()
            End While
        End Using

        If (HttpContext.Current.IsDebuggingEnabled) Then
            razorEngine.ViewLocationCache = DefaultViewLocationCache.Null 'No caching in debug mode
        Else
            razorEngine.ViewLocationCache = New DefaultViewLocationCache(TimeSpan.FromMinutes(30)) 'extend caching view URL location To 30min, instead Of 15min
        End If

        ViewEngines.Engines.Add(razorEngine)

        AreaRegistration.RegisterAllAreas()
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
        RouteConfig.RegisterRoutes(RouteTable.Routes)

    End Sub

End Class

