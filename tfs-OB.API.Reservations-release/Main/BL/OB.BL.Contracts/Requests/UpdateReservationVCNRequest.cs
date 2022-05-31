using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateReservationVCNRequest : RequestBase
    {
        /// <summary>
        /// The Omnibees Reservation UID.
        /// </summary>
        [DataMember]
        public long ReservationId { get; set; }

        /// <summary>
        /// The ReservationId of VCN.
        /// </summary>
        [DataMember]
        public string VcnReservationId { get; set; }

        /// <summary>
        /// The token of VCN.
        /// </summary>
        [DataMember]
        public string VcnTokenId { get; set; }

        /// <summary>
        /// The Credit Card Holder Name.
        /// </summary>
        [DataMember]
        public string CreditCardHolderName { get; set; }

        /// <summary>
        /// Credit Card number.
        /// </summary>
        [DataMember]
        public string CreditCardNumber { get; set; }

        /// <summary>
        /// Credit Card CVV code.
        /// </summary>
        [DataMember]
        public string CreditCardCVV { get; set; }

        /// <summary>
        /// Credit Card Expiration Date. 
        /// Only are considered the Year and the Month. 
        /// </summary>
        [DataMember]
        public DateTime CreditCardExpirationDate { get; set; }
    }
}