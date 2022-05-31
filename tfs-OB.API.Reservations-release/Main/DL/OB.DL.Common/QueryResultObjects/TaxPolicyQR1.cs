using System;
namespace OB.DL.Common.QueryResultObjects
{
    public class TaxPolicyQR1
    {
        public TaxPolicyQR1()
        {

        }

        public string BillingType { get; set; }
        public long Rate_UID { get; set; }
        public long ReservationRoom_UID { get; set; }
        public decimal? TaxCalculatedValue { get; set; }
        public decimal? TaxDefaultValue { get; set; }
        public string TaxDescription { get; set; }
        public long? TaxId { get; set; }
        public bool? TaxIsPercentage { get; set; }
        public string TaxName { get; set; }
        //public long UID { get; set; }
        public bool IsPerNight { get; set; }
        public bool IsPerPerson { get; set; }
    }
}