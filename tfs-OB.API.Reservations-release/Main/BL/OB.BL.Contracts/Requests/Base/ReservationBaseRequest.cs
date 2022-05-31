using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ReservationBaseRequest : RequestBase
    {
        /// <summary>
        /// Required
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual long ChannelId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual string TransactionId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<Reservation.BL.Constants.ReservationTransactionType> TransactionType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<Reservation.BL.Constants.ReservationTransactionAction> TransactionAction { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<long> UserId { get; set; }
    }
}