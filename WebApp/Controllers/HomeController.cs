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
        public List<SelectListItem> Rules;
    }
    public class LoadVM
    {
        public IEnumerable<ToshClient.Entry> saved;
        public IEnumerable<ToshClient.Entry> error;
    }
    public class HomeController : Controller
    {
            string path = System.Web.HttpContext.Current.Request.PhysicalApplicationPath;
        private HomeVM BuildVMM()
        {

            var homeVM = new HomeVM();
            homeVM.Reports = new List<Parser.ReportVm>(Parser.Movimenti(path));
            var Tags = new List<ToshClient.Tag>(ToshClient.GetTags());
            List<SelectListItem> tagsVm;
            homeVM.Tags = Tags.Where(x => !x.deleted).Select(tag => new SelectListItem() { Text = tag.name, Value = tag.id }).ToList();
            homeVM.Rules = new List<SelectListItem>()
            {
               new SelectListItem() {Text = "Ignore", Value = ((int) Parser.RuleType.Ignore).ToString() },
               new SelectListItem() {Text = "Tag", Value = ((int)Parser.RuleType.Tagged).ToString() }
            };
            return homeVM;
        }
        public ActionResult Index()
        {
            
            return View(BuildVMM());
        }
        [HttpPost]
        public ActionResult Index(string start,string tag,string rule)
        {
            switch (rule)
            {
                case "1":
                    Parser.addRule(tag, start, path);
                    break;
                case "0":
                    Parser.addIgnore(start, path);
                    break;
            }
            return View(BuildVMM());

        }
        public ActionResult Load()
        {
            var entries = ToshClient.SaveEntries(path);
            var vm = new LoadVM()
            {
                saved = entries.Item1,
                error = entries.Item2
            };
            
            return View(vm);
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