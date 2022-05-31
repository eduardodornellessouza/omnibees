using OB.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ValidPromocodeParameters
    {
        public ValidPromocodeParameters()
        {
            OldDaysAppliedDiscount = new List<DateTime>();
            NewDaysToApplyDiscount = new List<DateTime>();
        }

        public List<ReservationRoomStayPeriod> ReservationRoomsPeriods { get; set; }
        public PromotionalCode PromoCodeObj { get; set; }

        public bool RejectReservation { get; set; }
        public List<DateTime> OldDaysAppliedDiscount { get; set; }
        public List<DateTime> NewDaysToApplyDiscount { get; set; }
    }
}