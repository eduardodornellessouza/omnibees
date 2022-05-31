using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ValidateIsPartialUpdateRequest : RequestBase
    {
        /// <summary>
        /// List of rate room price by occupancy
        /// </summary>
        [DataMember]
        public List<RateRoomPriceByOccupancyCustom> RateRoomPriceByOccList { get;set; }

    }
}
