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
        public List<SharedTypes.ReportVm> IgnoredReports;
        public List<SharedTypes.ReportVm> Reports;
        public List<SelectListItem> Rules;
        public List<SelectListItem> Tags;
    }
    [Authorize()]
    [RequireHttps]
    public class LoadController : Controller
    {
        private const string stream = "stream";
        private const string vm = "vm";
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

        public class Some
        {
            public int Tag { get; set; }
            public string Id { get; set; }
            public int Account { get; set; }
        }

        [HttpPost]
        public ActionResult SaveEntries(List<Some> model)
        {
            var selected = model.Where(x => x.Tag != 0);
            var reports = (List<SharedTypes.ReportVm>) Session[vm];

            var expences = new List<SharedTypes.ReportVm>();
            foreach (var row in selected)
            {
                var expese = reports.First(x => x.Hash == row.Id);
                // a
                var report = new SharedTypes.ReportVm(expese.Ammount,
                    expese.Date, expese.Description, expese.Causale,
                    expese.Type, true, row.Tag, expese.Hash,0, row.Account);
                expences.Add(report);
            }

            ToshClient.SaveRecords(expences);

            return RedirectToAction("Index");
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
            var reports = MovimentiModelBuilder.Movimenti(path, inputStream);
            Session[vm] = reports;
            var homeVM = new HomeVM
            {
                Reports = reports.OrderBy(x => x.Date).ToList(),
                IgnoredReports = MovimentiModelBuilder.Ignorati(path, inputStream),
                Tags =
                    Tags.Where(x => !x.deleted)
                        .Select(tag => new SelectListItem() { Text = tag.name, Value = tag.id })
                        .ToList(),
                Rules = new List<SelectListItem>()
                {
                    new SelectListItem() {Text = "Ignore", Value = ((int) SharedTypes.RuleType.Ignore).ToString()},
                    new SelectListItem() {Text = "Tag", Value = ((int) SharedTypes.RuleType.Tagged).ToString()}
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