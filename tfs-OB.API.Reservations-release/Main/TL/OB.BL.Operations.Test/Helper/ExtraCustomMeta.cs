using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Helper
{
    public partial class ExtraCustomMeta
    {
        public long UID { get; set; }

        public string Name { get; set; }

        public bool IsBoardType { get; set; }

        public Nullable<DateTime> DateFrom { get; set; }

        public Nullable<DateTime> DateTo { get; set; }

        public int Quantity { get; set; }

        public bool? IsIncluded { get; set; }
    }
}
