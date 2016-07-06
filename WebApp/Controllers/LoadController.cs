using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace WebApp.Controllers
{
    public class LoadVM
    {
        public IEnumerable<ToshClient.Entry> saved;
        public IEnumerable<ToshClient.Entry> error;
    }
    public class HomeVM
    {
        public List<Parser.ReportVm> Reports;
        public List<Parser.ReportVm> IgnoredReports;
        public List<SelectListItem> Tags;
        public List<SelectListItem> Rules;
        public List<SelectListItem> Accounts;
    }
    public class LoadController : Controller
    {
        const string stream = "stream";
        string path = System.Web.HttpContext.Current.Request.PhysicalApplicationPath;
        private HomeVM BuildVMM()
        {

            var Tags = new List<ToshClient.Tag>(ToshClient.GetTags());
            var Accounts = new List<ToshClient.Account>(ToshClient.GetAccounts());
            var homeVM = new HomeVM
            {
                Reports = new List<Parser.ReportVm>(Parser.Movimenti(path, inputStream)),
                IgnoredReports = new List<Parser.ReportVm>(Parser.Ignorati(path, inputStream)),
                Tags =
                    Tags.Where(x => !x.deleted)
                        .Select(tag => new SelectListItem() {Text = tag.name, Value = tag.id})
                        .ToList(),
                Rules = new List<SelectListItem>()
                {
                    new SelectListItem() {Text = "Ignore", Value = ((int) Parser.RuleType.Ignore).ToString()},
                    new SelectListItem() {Text = "Tag", Value = ((int) Parser.RuleType.Tagged).ToString()}
                },
                Accounts = Accounts.Select(x => new SelectListItem() {Text = x.name, Value = x.id.ToString()}).ToList()
            };

            return homeVM;
        }

        public ActionResult Load()
        {
            inputStream = (Stream)Session[stream];
             ToshClient.SaveRecords("RID", path, inputStream);
            var vm = new LoadVM();

            return View(vm);
        }
        public ActionResult Index()
        {
            if (Session[stream] == null)
                return RedirectToAction("Index", "Home");

            inputStream = (Stream)Session[stream];
            return View(BuildVMM());
        }
        [HttpPost]
        public ActionResult Index(string start, string tag, string rule)
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
            if (Session[stream] != null)
                inputStream = (Stream)Session[stream];
            return View(BuildVMM());

        }
        [HttpPost]
        public ActionResult Save(string account)
        {
            if (Session[stream] != null)
            {
                inputStream = (Stream)Session[stream];
                ToshClient.SaveRecords(account, path, inputStream);
            }
            return View();

        }
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                inputStream = file.InputStream;
                Session[stream] = inputStream;
            }
            else
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Load");
        }

        private Stream inputStream;
    }
}