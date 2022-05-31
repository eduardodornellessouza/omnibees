using System;
using System.Collections.Generic;
using System.Linq;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class DateRange
    {
        public DateTime DateFrom { get { return Dates.First(); } }
        public DateTime DateTo { get { return Dates.Last(); } }
        public List<DateTime> Dates = new List<DateTime>();
        public List<DateRange> GetRanges(List<DateTime> dates)
        {
            List<DateRange> ranges = new List<DateRange>();
            DateRange currentRange = null;
            dates = dates.OrderBy(d => d.Date).ToList();

            // this presumes a list of dates ordered by day, if not then the list will need sorting first
            for (int i = 0; i < dates.Count; ++i)
            {
                var currentDate = dates[i];
                if (i == 0 || dates[i - 1] != currentDate.AddDays(-1))
                {
                    // it's either the first date or the current date isn't consecutive to the previous so a new range is needed
                    currentRange = new DateRange();
                    ranges.Add(currentRange);
                }

                currentRange.Dates.Add(currentDate);
            }
            return ranges;
        }
    }
}
