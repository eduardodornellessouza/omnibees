using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class FindTokenizedCreditCardsReadsPerMonthByCriteriaRequest : RequestBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> UIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> propertyUIDs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<int> years { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<short> months { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int pageIndex { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int pageSize { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool returnTotal { get; set; }
    }
}
