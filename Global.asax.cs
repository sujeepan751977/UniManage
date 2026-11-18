using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using UniManage.Models;
using UniManage.Migrations;

namespace UniManage
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Configure Anti-Forgery to use ASP.NET Identity claims
            System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;

            // Database initializer to migrate to latest version automatically
            // Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
        }

        protected void Application_BeginRequest()
        {
            // Security Headers
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["X-Frame-Options"] = "DENY";
            Response.Headers["X-XSS-Protection"] = "1; mode=block";
            Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        }
    }
}
