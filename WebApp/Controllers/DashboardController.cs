using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Extensions;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize]
    [RequireHttps]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {

            // scegli anno corrente
            // carica tutte le spese fino a fine anno
            // visualizzali in un grafico lineare
            //  . colori differenti per tag differenti
            //  . per i mesi a venire indica il valore medio dell'anno
            //  . calcola i costi totali nell'anno per tag
            //  .. per ogni tag poter indicare il tipo ricorrenza (o i mesi in cui ricorrere)
            var startDate = new DateTime(2017, 1, 1, 0, 0, 0);

            if (Session["vm"] != null)
            {
                return View((ViewModelTable)Session["vm"]);
            }
            var entriesByTag = ToshClient.GetEntriesByTag(startDate, startDate.AddYears(1)).OrderBy(o => o.Key.name);
            
            //var income = entriesByTag.Select(tag => tag.Value.Where(x => x.amount > 0).Select(x => x.amount).Sum()).Sum();
            var entries = ComputeEntries(entriesByTag);
          
            var vm = new ViewModelTable
            {
                TagValues = entries
            };
            Session["vm"] = vm;
            return View(vm);
        }

        private static Dictionary<string, ViewModelRow> ComputeEntries(IOrderedEnumerable<ToshlTypes.TaggedEntry> entriesByTag)
        {
            var tags = new List<ToshlTypes.Tag>(ToshClient.Entities.getTags());
            var cathegories = new List<ToshlTypes.Category>(ToshClient.Entities.getCategories());

            var entries = new Dictionary<string, ViewModelRow>();
            foreach (var entry in entriesByTag)
            {
                //var ann = tag.Value.Where(x => x.amount <= 0).Select(x =>
                var row = entry.Value.Select(x =>
                {
                    var date = DateTime.Parse(x.date);
                    var month = date.Month;
                    return new {month, x.amount};
                }).Aggregate(new Dictionary<int, double>(), (dic, a) =>
                {
                    dic.EnhancedAdd(a.month - 1, a.amount, (x, y) => x + y);
                    return dic;
                });
                var array = new double[12];
                foreach (var monthValue in row)
                {
                    array[monthValue.Key] = monthValue.Value;
                }
                var cat = cathegories.First(x => x.id == entry.Key.category);
                var viewModelRow = new ViewModelRow
                {
                    Entries = array,
                    Category = cat.name,
                    CategoryType = cat.type,
                    Total = array.Sum()
                };
                entries.Add(entry.Key.name, viewModelRow);
            }
            entries = 
                entries
                .OrderBy(x => x.Value.Total)
                .OrderBy(x => x.Value.Category)
                .OrderBy(x => x.Value.CategoryType)
                .ToDictionary(x => x.Key, x => x.Value);
            return entries;
        }
    }
}