using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ListSettingRequest : PagedRequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<string> Names { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<Tuple<string, OB.BL.Constants.SettingCategory>> NamesAndCategories { get; set; }
    }
}