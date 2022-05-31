using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Filter
{
    public class FilterByInfo
    {
        public string FilterBy { get; set; }
        public object Value { get; set; }
        public FilterOperator Operator { get; set; }
        public FilterConjunction Conjunction { get; set; }
    }
}
