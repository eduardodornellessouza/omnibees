using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Filter
{
    public class SortByInfo
    {
        public string OrderBy { get; set; }
        public SortDirection Direction { get; set; }
        public bool Initial { get; set; }
    }
}
