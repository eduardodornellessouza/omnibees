using System;

namespace OB.DL.Common.QueryResultObjects
{
    [Serializable]
    public partial class ReservationRoomTaxPolicyQR1
    {
        public long ReservationRoom_UID { get; set; }

        public decimal? TaxCalculatedValue { get; set; }

        public decimal? TaxDefaultValue { get; set; }

        public string TaxDescription { get; set; }

        public long? TaxId { get; set; }

        public bool? TaxIsPercentage { get; set; }

        public string TaxName { get; set; }

        public long UID { get; set; }

        public string BillingType { get; set; }

        public string TaxCalculatedValueFormated { get; set; }
    }
}