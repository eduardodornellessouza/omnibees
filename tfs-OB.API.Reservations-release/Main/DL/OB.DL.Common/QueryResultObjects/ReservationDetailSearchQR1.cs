using System.Collections.Generic;

namespace OB.DL.Common.QueryResultObjects
{
    public class ReservationDetailSearchQR1
    {
        public ReservationDetailQR1 MainSearch { get; set; }

        public List<OtherPolicyQR1> Policies { get; set; }

        public List<GuestActivityQR1> GuestActivities { get; set; }

        public string GuestStateName { get; set; }

        public string BillingStateName { get; set; }
    }
}