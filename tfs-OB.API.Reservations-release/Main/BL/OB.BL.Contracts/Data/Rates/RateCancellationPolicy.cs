using OB.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateCancellationPolicy : ContractBase
    {
        public RateCancellationPolicy()
        {
            Periods = new List<ApplyToPeriod>();
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Rate_UID { get; set; }

        [DataMember]
        public CancellationPolicy CancellationPolicy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<ApplyToPeriod> Periods { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsDeleted { get; set; }

    }
}