using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Helper
{
    public partial class BookingEngineRateRoomDetailsRateRestrictions
    {
        // ids
        public long RateRoomDetailsRateRestrictionsUID { get; set; }
        public long RateRoomDetailsUID { get; set; }
        public long RateRoomUID { get; set; }

        // rate restriction type
        public long RateRestrictionUID { get; set; }
        public string RateRestrictionName { get; set; }
        public string RateRestrictionDescription { get; set; }
        public string RateRestrictionShortName { get; set; }

        // rate restriction details
        public int? Value { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? IsTrue { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
