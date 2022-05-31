using System;
using System.Collections.Generic;
namespace OB.DL.Common.QueryResultObjects
{
    public class IncentiveQR1
    {
        public IncentiveQR1()
        {
            DayDiscount = new List<decimal>();
        }
        public long Rate_UID { get; set; }
        public long UID { get; set; }
        public long IncentiveType_UID { get; set; }
        public int DiscountPercentage { get; set; }
        public Nullable<int> FreeDays { get; set; }
        public int Days { get; set; }
        public Nullable<bool> IsFreeDaysAtBegin { get; set; }
        public Nullable<System.DateTime> IncentiveFrom { get; set; }
        public Nullable<System.DateTime> IncentiveTo { get; set; }
        public Nullable<bool> IsCumulative { get; set; }
        public string IncentiveName { get; set; }

        //Used on calculations
        public decimal TotalDiscounted { get; set; }
        public List<decimal> DayDiscount { get; set; }
    }
}