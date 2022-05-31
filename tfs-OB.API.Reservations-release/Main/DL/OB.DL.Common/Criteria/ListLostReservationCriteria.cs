using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Criteria
{
    public class ListLostReservationCriteria 
    {
        public List<long> PropertyUids { get; set; }
        public List<long> Uids { get; set; }

        public bool OrderByDescending { get; set;}
        public List<Filter.SortByInfo> Order { get; set; }

        public Kendo.DynamicLinq.Filter NestedFilters { get; set;}
        public List<Filter.FilterByInfo> Filters { get; set;}
    }
}
