using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListGuaranteeTypeResponse : ResponseBase
    {
        [DataMember]
        public List<GuaranteeType> Result { get; set; }
    }
}
