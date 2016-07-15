using System.Web.Mvc;

namespace WebApp.Controllers
{
    [Authorize]
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