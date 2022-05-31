using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Domain
{
    public class RatesIncentive
    {
        public long UID { get; set; }
        public long Incentive_UID { get; set; }
        public long Rate_UID { get; set; }
        public bool IsCumulative { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool IsAvailableForDiferentPeriods { get; set; }
    }
}
