using System;
using System.Collections.Generic;
using System.Linq;

namespace OB.BL.Operations.Helper
{
    /// <summary>
    /// Util methods for dealing with datetime periods, intervals and breaks.
    /// </summary>
    public static class DatePeriodHelper
    {
        /// <summary>
        /// Converts a set of dates into periods of subsequent dates by one day. Whenever there's a break, a new period (tuple)
        /// is created in the result.
        /// If a list of one item is given, only one period with the same begin and end date will be returned.
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime,DateTime>> CompressDatesIntoIntervals(List<DateTime> dates)
        {
            List<Tuple<DateTime, DateTime>> result = new List<Tuple<DateTime, DateTime>>();

            if (dates == null || dates.Count == 0)
                return result;

            dates = dates.Distinct().OrderBy(x => x).ToList();

            DateTime first = DateTime.MinValue;
            DateTime last = DateTime.MinValue;

            if (dates.Count == 1)
            {
                result.Add(new Tuple<DateTime, DateTime>(dates[0], dates[0]));
            }
            else
            {
                for (int i = 0; i < dates.Count; i++)
                {
                    var date = dates[i];
                    if (i == 0)
                        last = first = date;
                    else
                    {
                        if (date.Subtract(TimeSpan.FromDays(1)).Equals(dates[i - 1]))
                        {
                            last = date;
                            if ((i + 1) == dates.Count)
                            {
                                result.Add(new Tuple<DateTime, DateTime>(first, last));
                            }
                            continue;
                        }
                        result.Add(new Tuple<DateTime, DateTime>(first, last));

                        first = date;
                        last = date;
                        if ((i + 1) == dates.Count)
                        {
                            result.Add(new Tuple<DateTime, DateTime>(first, first));
                        }
                    }
                }
            }    
            return result;
        }
    }
}
