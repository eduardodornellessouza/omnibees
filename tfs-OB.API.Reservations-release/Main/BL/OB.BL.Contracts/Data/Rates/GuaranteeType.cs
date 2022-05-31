using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.BL.Contracts.Data.Rates
{
    /// <summary>
    /// GuaranteeType Data Transfer Object
    /// </summary>
    [DataContract]
    public class GuaranteeType : ContractBase
    {
        public GuaranteeType()
        {

        }
  
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long DepositPolicyGuaranteeType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActive { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long GuaranteeType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GuaranteeTypeName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Guarantee_TypeCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long DepositPolicy_UID { get; set; }

    }
}
