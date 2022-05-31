using OB.Reservation.BL.Contracts.Data.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Responses
{
    public class GetPropertyBaseCurrencyByPropertyUIDResponse : ResponseBase
    {
        [DataMember]
        public Currency Result { get; set; }
    }
}
