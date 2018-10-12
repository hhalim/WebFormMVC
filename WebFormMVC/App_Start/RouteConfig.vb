Imports System.Web.Routing
Imports System.Web.Mvc

Imports UI

Public Class RouteConfig

    Public Shared Sub RegisterRoutes(routes As RouteCollection)

        'Ignore Web Form pages
        routes.IgnoreRoute("{resource}.aspx/{*pathInfo}")
        routes.IgnoreRoute("{resource}.ashx/{*pathInfo}")
        routes.IgnoreRoute("{resource}.asmx/{*pathInfo}")
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
        routes.IgnoreRoute("")
        routes.IgnoreRoute("api/{*pathInfo}")

        'Register MVC projects routes
        UI.Project1.RouteConfig.MapRoute(routes) 'Project1 MVC

    End Sub

End Class
