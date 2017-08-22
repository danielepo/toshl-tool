using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.ViewModels
{
    public class ViewModelRow
    {
        public IEnumerable<double> Entries { get; set; }
        public string CategoryType { get; internal set; }
        public double Total { get; internal set; }
        public string Category { get; internal set; }
    }
}