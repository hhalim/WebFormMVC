using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace UI.Project1
{
    public class RouteConfig
    {
        public static void MapRoute(RouteCollection routes)
        {
            routes.MapRoute(
                name: "Default",
                url: "project1/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
