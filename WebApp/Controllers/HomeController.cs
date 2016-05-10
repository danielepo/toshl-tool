using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace WebApp.Controllers
{
    public class HomeVM
    {
        public List<Parser.ReportVm> Reports;
        public List<SelectListItem> Tags;
        public List<SelectListItem> Rules;
        public List<SelectListItem> Accounts;
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
            var Accounts = new List<ToshClient.Account>(ToshClient.GetAccounts());
            homeVM.Accounts = Accounts.Select(x => new SelectListItem() {Text = x.name, Value = x.id.ToString()}).ToList();
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
        [HttpPost]
        public ActionResult Save(string account)
        {
            ToshClient.SaveRecords(account, path);
            return View();

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