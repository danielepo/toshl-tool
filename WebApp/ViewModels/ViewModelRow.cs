using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.ViewModels
{
    public class ViewModelRow
    {
        public IEnumerable<double> Entries { get; set; }
        public double Total => Entries.Sum();

        public double Mean
        {
            get
            {
                var aggregate = Entries.Aggregate(new Tuple<double, int>(0.0, 0), (agg, curr) =>
                {
                    if (Math.Abs(curr) < 0.0001)
                        return agg;
                    var counter = agg.Item2 + 1;
                    var sum = agg.Item1 + curr;
                    return new Tuple<double, int>(sum, counter);
                });
                return aggregate.Item2 == 0 ? aggregate.Item1 : aggregate.Item1 / aggregate.Item2;
            }
        }
    }
}