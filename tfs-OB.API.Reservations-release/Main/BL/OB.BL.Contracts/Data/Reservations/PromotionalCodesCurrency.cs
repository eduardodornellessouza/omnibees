using System;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    public partial class PromotionalCodesCurrency : ContractBase
    {
        public PromotionalCodesCurrency()
        {
        }

        public long UID { get; set; }

        public long PromotionalCode_UID { get; set; }

        public long Currency_UID { get; set; }

        public Nullable<decimal> Value { get; set; }

        public byte[] Revision { get; set; }
    }
}