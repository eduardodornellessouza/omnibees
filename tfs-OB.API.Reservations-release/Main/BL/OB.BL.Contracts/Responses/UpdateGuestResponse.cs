using OB.Reservation.BL.Contracts.Data.CRM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that stores the response information for an InsertGuest RESTful operation.
    /// It provides the Result field that contains the UID of the created Reservation or business error code if there was one.
    /// </summary>
    [DataContract]
    public class UpdateGuestResponse : ResponseBase
    {

        [DataMember]
        public IList<GuestResult> GuestResults { get; set; }

  
    }
}