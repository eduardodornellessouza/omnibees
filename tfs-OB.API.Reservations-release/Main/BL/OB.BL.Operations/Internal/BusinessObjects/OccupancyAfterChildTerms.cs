using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class OccupancyAfterChildTerms
    {
        public int AdultCount { get; set; }
        public int ChildCountForPrice { get; set; }
        public int ChildCountForOccupancy { get; set; }
        public int FreeChildCount { get; set; }
    }
}
