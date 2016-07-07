using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace WebApp.Controllers
{
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
            var startDate = new DateTime(2016,1,1,0,0,0);
            var fSharpOption = new List<ToshClient.Entry>(ToshClient.GetEntries(startDate, startDate.AddYears(1)));
            return View();
        }
    }
}