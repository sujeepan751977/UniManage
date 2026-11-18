using System.Web;
using System.Web.Optimization;

namespace UniManage
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            // Use standard Bundle instead of StyleBundle to prevent WebGrease from crashing on modern CSS variables var(--xxx)
            bundles.Add(new Bundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/unimanage.css"));
            
            bundles.Add(new ScriptBundle("~/bundles/unimanage").Include(
                      "~/Scripts/unimanage.js"));
        }
    }
}
