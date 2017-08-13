using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace WebApp.ViewModels
{
    public class LoadExpensesViewModel
    {
        public List<SelectListItem> Accounts;
        public List<SharedTypes.ReportVm> IgnoredReports;
        public List<SharedTypes.ReportVm> Reports;
        public List<SelectListItem> Rules;
        public List<SelectListItem> Tags;
        public Dictionary<ToshlTypes.Category, List<ToshlTypes.Tag>> CategoryTags { get; internal set; }
        internal static LoadExpensesViewModel BuildLoadExpensesVM(Stream inputStream, string path, Types.CsvType csvType)
        {
            var tags = new List<ToshlTypes.Tag>(ToshClient.Entities.getTags());
            var cathegories = new List<ToshlTypes.Category>(ToshClient.Entities.getCategories());
            var categoryTags = tags.GroupBy(x => x.category).ToDictionary(x => cathegories.First(y => y.id == x.Key), x => x.ToList());

            var Accounts = new List<ToshlTypes.Account>(ToshClient.Entities.getAccounts());
            var reports = MovimentiModelBuilder.Movimenti(path, inputStream, csvType);

            var loadExpenses = new LoadExpensesViewModel
            {
                Reports = reports.OrderBy(x => x.Date).ToList(),
                IgnoredReports = MovimentiModelBuilder.Ignorati(path, inputStream, csvType),
                Tags =
                    tags.Where(x => !x.deleted)
                        .Select(tag => new SelectListItem() { Text = tag.name, Value = tag.id })
                        .ToList(),
                Rules = new List<SelectListItem>()
                {
                    new SelectListItem() {Text = "Ignore", Value = ((int) SharedTypes.RuleType.Ignore).ToString()},
                    new SelectListItem() {Text = "Tag", Value = ((int) SharedTypes.RuleType.Tagged).ToString()}
                },
                Accounts = Accounts.Select(x => new SelectListItem() { Text = x.name, Value = x.id.ToString() }).ToList(),
                CategoryTags = categoryTags
            };

            return loadExpenses;

        }
        
    }
}