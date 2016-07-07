using System.Web.Mvc;

namespace WebApp.Controllers
{
    [RequireHttps]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}