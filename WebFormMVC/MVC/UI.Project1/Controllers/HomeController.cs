﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Project1.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var svcUrl = Helpers.GetRestServicesUrl(HttpContext.Request);
            ViewBag.ServiceBaseUrl = svcUrl;

            return View();
        }
    }
}