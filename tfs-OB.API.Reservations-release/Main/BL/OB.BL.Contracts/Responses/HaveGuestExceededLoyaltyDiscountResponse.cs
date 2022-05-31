using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class HaveGuestExceededLoyaltyDiscountResponse : ResponseBase
    {
        [DataMember]
        public bool Result { get; set; }
    }
}
