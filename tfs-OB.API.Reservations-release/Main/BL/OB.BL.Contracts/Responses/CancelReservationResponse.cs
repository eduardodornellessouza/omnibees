using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that stores the response information for an CancelReservation RESTful operation.
    /// It provides the Result field that contains the Cancel reservation return code or the error code if there was one.
    /// <see cref="Result"/>
    /// </summary>
    [DataContract]
    public class CancelReservationResponse : ResponseBase
    {
        public CancelReservationResponse()
        {
            Costs = new List<ReservationRoomCost>();
        }

        /// <summary>
        ///Cancel reservation error code:
        ///<para/> 1 OK
        ///<para/> 0 NOT OK
        ///<para/> -100 rooms could not be return same day wait till 24:00 - abort ttansactions
        ///<para/> -200 transaction invalid consult maxipago - not abort transaction
        ///<para/> -199 cancelad in omnibees but not cancelleed in maxipago - not abort trnsaction
        /// </summary>
        [DataMember(EmitDefaultValue = true, IsRequired = true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate | DefaultValueHandling.Include)]
        public long Result { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public List<ReservationRoomCost> Costs { get; set; }

        [DataMember]
        public long? ReservationStatus { get; set; }
    }

    [DataContract]
    public class ReservationRoomCost
    {
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string RoomNumber { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int RoomStatus { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public decimal RoomCost { get; set; }

        [DataMember]
        public long? CurrencyUid { get; set; }
    }
}