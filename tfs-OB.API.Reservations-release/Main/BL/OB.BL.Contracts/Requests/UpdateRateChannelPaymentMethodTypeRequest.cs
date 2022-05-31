using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class UpdateRateChannelPaymentMethodTypeRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? ModifiedBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<long, List<long>> ChannelPropertyVSPaymentMethodTypeUIDs { get; set; }
    }
}
