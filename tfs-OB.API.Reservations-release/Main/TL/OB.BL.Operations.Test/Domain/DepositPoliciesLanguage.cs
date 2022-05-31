using OB.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Domain
{
    public class DepositPoliciesLanguage
    {
        public long UID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<long> DepositPolicy_UID { get; set; }
        public Nullable<long> Language_UID { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public byte[] Revision { get; set; }

        public DepositPolicy DepositPolicy { get; set; }
    }
}
