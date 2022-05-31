using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data
{
    [DataContract]
    public class DatesRange : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime EndDate { get; set; }
    }

    [DataContract]
    public class NullableDatesRange : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? StartDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? EndDate { get; set; }
    }
}