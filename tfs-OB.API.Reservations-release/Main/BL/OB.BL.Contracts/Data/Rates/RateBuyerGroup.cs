using OB.Reservation.BL.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateBuyerGroup : ContractBase
    {
        public RateBuyerGroup()
        {
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Rate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> BuyerGroup_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> TPI_UID { get; set; }

        [DataMember]
        public bool IsPercentage { get; set; }

        [DataMember]
        public bool IsValueDecrease { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Value { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> TPIAgencyCompany_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> GDSValue { get; set; }

        [DataMember]
        public bool GDSValueIsPercentage { get; set; }

        [DataMember]
        public bool GDSValueIsDecrease { get; set; }
    }
}
