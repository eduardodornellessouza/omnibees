using OB.BL.Contracts.Data.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateIncentive : ContractBase
    {
        public RateIncentive()
        {
            Periods = new List<ApplyToPeriod>();
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Rate_UID { get; set; }
        
        [DataMember]
        public long Incentive_UID { get; set; }

        [DataMember]
        public Incentive Incentive { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsAvailableForDiferentPeriods { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsCumulative { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public IList<ApplyToPeriod> Periods { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

    }
}