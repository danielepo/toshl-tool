using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize()]
    [RequireHttps]
    public class LoadController : Controller
    {
        private const string streamKey = "stream";
        private const string isContoCorrenteKey = "isContoCorrente";
        private const string vm = "vm";
        private Stream inputStream;
        private string path = System.Web.HttpContext.Current.Request.PhysicalApplicationPath;

        public ActionResult Index()
        {
            if (Session[streamKey] == null)
                return RedirectToAction("Index", "Home");

            inputStream = (Stream)Session[streamKey];
            var viewModel = LoadExpensesViewModel.BuildLoadExpensesVM(inputStream, path, TipoCsv());
            Session[vm] = viewModel.Reports;

            return View(viewModel);
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
            if (Session[streamKey] != null)
                inputStream = (Stream)Session[streamKey];
            var viewModel = LoadExpensesViewModel.BuildLoadExpensesVM(inputStream, path, TipoCsv());
            Session[vm] = viewModel.Reports;
            return View(viewModel);
        }

  

        [HttpPost]
        public ActionResult SaveEntries(List<SaveEntriesViewModel> model)
        {
            var selected = model.Where(x => x.Tag != 0);
            var reports = (List<SharedTypes.ReportVm>)Session[vm];

            var expences = new List<SharedTypes.ReportVm>();
            foreach (var row in selected)
            {
                var expese = reports.First(x => x.Hash == row.Id);
                // a
                var report = new SharedTypes.ReportVm(expese.Ammount,
                    expese.Date, expese.Description, expese.Causale,
                    expese.Type, true, row.Tag, expese.Hash, 0, row.Account);
                expences.Add(report);
            }

            ToshClient.SaveRecords(expences);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, string isContoCorrente)
        {
            Session[isContoCorrenteKey] = isContoCorrente ?? "true";

            if (file != null && file.ContentLength > 0)
            {
                inputStream = file.InputStream;
                Session[streamKey] = inputStream;
            }
            else
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Load");
        }
        public ActionResult Upload()
        {
            return View();
        }

        private Types.CsvType TipoCsv()
        {
            bool result;
            if (bool.TryParse(Session[isContoCorrenteKey] as string, out result))
            {
                return result ? Types.CsvType.ContoCorrente : Types.CsvType.CartaCredito;
            }
            return Types.CsvType.ContoCorrente;
        }

    }

}