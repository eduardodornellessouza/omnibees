using OB.Reservation.BL.Contracts.Data.CRM;
using OB.Reservation.BL.Contracts.Data.General;
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
    public class OperatorAuthenticationResponse : ResponseBase
    {
        [DataMember]
        public BigPullAuthentication AuthenticationResult { get; set; }

        [DataMember]
        public int TotalRecords { get; set; }
  
    }
}