using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class HomeVM
    {
        public List<Parser.ReportVm> Reports;
        public List<SelectListItem> Tags;
    }
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var homeVM = new HomeVM();
            homeVM.Reports = new List<Parser.ReportVm>(Parser.Movimenti());
            var Tags = new List<ToshClient.Tag>(ToshClient.GetTags());
            List<SelectListItem> tagsVm;
            homeVM.Tags = Tags.Where(x => !x.deleted).Select(tag => new SelectListItem() {Text = tag.name, Value = tag.id}).ToList();

            return View(homeVM);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}