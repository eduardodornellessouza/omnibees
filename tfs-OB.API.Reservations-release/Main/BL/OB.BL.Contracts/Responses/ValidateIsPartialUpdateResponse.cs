using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ValidateIsPartialUpdateResponse : ResponseBase
    {
        /// <summary>
        /// Boolean with confirmation about update
        /// </summary>
        [DataMember]
        public bool IsPartialUpdate { get; set; }

        /// <summary>
        /// String with names of room type when child prices are zero
        /// </summary>
        [DataMember]
        public string RoomTypeNamesChildIsZero { get; set; }
    }
}
