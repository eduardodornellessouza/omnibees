using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Domain
{
    public class PromotionalCodesCurrency
    {
        public long UID { get; set; }
        public long Currency_UID { get; set; }

        public long PromotionalCode_UID { get; set; }
        public decimal? Value { get; set; }
    }
}
