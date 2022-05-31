using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public partial class ReservationPartialPaymentDetail : ContractBase
    {
        public ReservationPartialPaymentDetail()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Reservation_UID { get; set; }

        [DataMember]
        public int InstallmentNo { get; set; }

        [DataMember]
        public Nullable<decimal> InterestRate { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public bool IsPaid { get; set; }

        [DataMember]
        public System.DateTime CreatedDate { get; set; }

        [DataMember]
        public System.DateTime ModifiedDate { get; set; }
    }
}