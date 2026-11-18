using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace UniManage.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Administrator")) return RedirectToAction("Dashboard", "Admin");
                if (User.IsInRole("Lecturer")) return RedirectToAction("Dashboard", "Lecturer");
                if (User.IsInRole("Student")) return RedirectToAction("Dashboard", "Student");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "UniManage - Comprehensive Course Management.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact page.";
            return View();
        }
    }
}
