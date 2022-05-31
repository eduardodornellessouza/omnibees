using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateGuaranteeTypeRequest : RequestBase
    {
        [DataMember]
        public List<GuaranteeType> GuaranteeTypeList { get; set; }

        [DataMember]
        public long UserUID { get; set; }
    }
}
