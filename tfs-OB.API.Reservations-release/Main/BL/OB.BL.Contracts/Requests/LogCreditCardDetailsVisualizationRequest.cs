using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class LogCreditCardDetailsVisualizationRequest : RequestBase
    {
        [DataMember]
        public long UserUID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public long ReservationUID { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        [Obsolete("Use ActionType instead")]
        public string Action { get; set; }

        [DataMember]
        public Constants.CreditCardVisualizationAction ActionType { get; set; }

        [DataMember]
        public string Warnings { get; set; }

        [DataMember]
        public string UserIP { get; set; }
    }
}
