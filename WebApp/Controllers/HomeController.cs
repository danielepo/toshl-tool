using System.Web.Mvc;

namespace WebApp.Controllers
{

    [RequireHttps]
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
    }
}