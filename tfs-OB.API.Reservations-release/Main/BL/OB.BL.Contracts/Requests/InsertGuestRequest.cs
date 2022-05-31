using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that stores the arguments for the InsertGuest RESTfull operation.
    /// The InsertGuest Request object that contains all the required objects
    /// (Guest)
    /// to create and insert a Guest into the database.
    /// The relationship between the objects are maintained using the UIDs despite the fact that they are not Database UIDs but logical UIDs (valid in the scope of the object graph).
    /// </summary>
    [DataContract]
    public class InsertGuestRequest : RequestBase
    {
        [DataMember]
        public List<Guest> Guests { get; set; }

        [DataMember]
        public long UserUID { get; set; }

        [DataMember]
        public bool IsFromChannel { get; set; }


    }
}
