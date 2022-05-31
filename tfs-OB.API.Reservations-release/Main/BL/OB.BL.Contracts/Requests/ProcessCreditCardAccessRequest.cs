using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ProcessCreditCardAccessRequest : RequestBase
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string UserIP { get; set; }

        [DataMember]
        public long ReservationUID { get; set; }
        
        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public long PropertyUID { get; set; }

        [DataMember]
        public bool ValidatePermissionOnBookingPrint { get; set; }
    }
}
