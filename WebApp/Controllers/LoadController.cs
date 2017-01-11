using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class HomeVM
    {
        public List<SelectListItem> Accounts;
        public List<MovimentiModelBuilder.ReportVm> IgnoredReports;
        public List<MovimentiModelBuilder.ReportVm> Reports;
        public List<SelectListItem> Rules;
        public List<SelectListItem> Tags;
    }
    [Authorize()]
    [RequireHttps]
    public class LoadController : Controller
    {
        private const string stream = "stream";
        private Stream inputStream;
        private string path = System.Web.HttpContext.Current.Request.PhysicalApplicationPath;

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
                    TagsRuleManager.AddTagRule(tag, start, path);
                    break;

                case "0":
                    TagsRuleManager.AddIgnoreRule(start, path);
                    break;
            }
            if (Session[stream] != null)
                inputStream = (Stream)Session[stream];
            return View(BuildVMM());
        }

        public ActionResult Load()
        {
            inputStream = (Stream)Session[stream];
            ToshClient.SaveRecords("RID", path, inputStream);
            var vm = new LoadVM();

            return View(vm);
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
        public ActionResult Upload()
        {
            return View();
        }

        private HomeVM BuildVMM()
        {
            var Tags = new List<ToshlTypes.Tag>(ToshClient.Entities.getTags());
            var Accounts = new List<ToshlTypes.Account>(ToshClient.Entities.getAccounts());
            var homeVM = new HomeVM
            {
                Reports = MovimentiModelBuilder.Movimenti(path, inputStream),
                IgnoredReports = MovimentiModelBuilder.Ignorati(path, inputStream),
                Tags =
                    Tags.Where(x => !x.deleted)
                        .Select(tag => new SelectListItem() { Text = tag.name, Value = tag.id })
                        .ToList(),
                Rules = new List<SelectListItem>()
                {
                    new SelectListItem() {Text = "Ignore", Value = ((int) MovimentiModelBuilder.RuleType.Ignore).ToString()},
                    new SelectListItem() {Text = "Tag", Value = ((int) MovimentiModelBuilder.RuleType.Tagged).ToString()}
                },
                Accounts = Accounts.Select(x => new SelectListItem() { Text = x.name, Value = x.id.ToString() }).ToList()
            };

            return homeVM;
        }
    }

    public class LoadVM
    {
        public IEnumerable<ToshlTypes.Entry> error;
        public IEnumerable<ToshlTypes.Entry> saved;
    }
}