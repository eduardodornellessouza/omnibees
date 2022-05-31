using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;


namespace OB.Reservation.BL.Contracts.Responses
{

    [DataContract]
    public class RegistTokenizedCreditCardsReadsPerMonthResponse : ResponseBase
    {
        [DataMember]
        public Dictionary<long, long> PropertyUIDs_NrReadsToIncrement { get; set; }
    }

}