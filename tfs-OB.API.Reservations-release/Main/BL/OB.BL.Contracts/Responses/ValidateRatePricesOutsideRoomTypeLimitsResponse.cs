using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ValidateRatePricesOutsideRoomTypeLimitsResponse : ResponseBase
    {
        /// <summary>
        /// String with names of room type when prices are outside the room type limits
        /// </summary>
        [DataMember]
        public string RoomName { get; set; }

        /// <summary>
        /// List of Rates UID that will be updated
        /// </summary>
        [DataMember]
        public List<long> RatesUID { get; set; }

    }
}
