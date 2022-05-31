using OB.Reservation.BL.Contracts.Data.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class GetPayPalPaymentURLRequest : RequestBase
    {
        [DataMember]
        public string PropertyName { get; set; }

        [DataMember]
        public Reservation.BL.Contracts.Data.Reservations.Reservation Reservation { get; set; }

        [DataMember]
        public List<ReservationRoom> ReservationRoom { get; set; }

        [DataMember]
        public string UniqueId { get; set; }

        [DataMember]
        public string LanguageIso { get; set; }

        [DataMember]
        public string ReturnUrl { get; set; }

        [DataMember]
        public string CancelUrl { get; set; }
    }    
}

