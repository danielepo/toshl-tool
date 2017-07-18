using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace WebApp.Controllers
{
    public class Graphic
    {
        public IEnumerable<ToshlTypes.TaggedEntry> taggedEntries { get; set; }
        public double Income { get; internal set; }
        public Dictionary<string, List<double>> Entries { get; internal set; }
    }
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
                return View((Graphic)Session["vm"]);
            }
            var entriesByTag = ToshClient.GetEntriesByTag(startDate, startDate.AddYears(1)).OrderBy(o => o.Key.name);
            //var income = entriesByTag.Select(tag => tag.Value.Where(x => x.amount > 0).Select(x => x.amount).Sum()).Sum();
            var entries = new Dictionary<string, List<double>>();
            foreach (var tag in entriesByTag)
            {
                //var ann = tag.Value.Where(x => x.amount <= 0).Select(x =>
                var ann = tag.Value.Select(x =>
                {
                    var date = DateTime.Parse(x.date);
                    var month = date.Month;
                    return new { x.amount, month };
                });
                var dic = new Dictionary<int, double>();
                foreach (var a in ann)
                {
                    if (dic.ContainsKey(a.month))
                    {
                        dic[a.month] += a.amount;
                    }
                    else
                    {
                        dic.Add(a.month, a.amount);
                    }
                }
                var months = new[]
                {
                    "January", "February", "March", "April", "May", "June", "July", "August", "September",
                    "October", "November", "Dicember"
                };
                var values = new List<double>();
                for (int index = 0; index < months.Length; index++)
                {
                    double value;
                    dic.TryGetValue(index + 1, out value);

                    values.Add(value);
                }

                entries.Add(tag.Key.name, values);
            }
            var vm = new Graphic
            {
                taggedEntries = entriesByTag,
                //Income = income,
                Entries = entries

            };
            Session["vm"] = vm;
            return View(vm);
        }
    }
}