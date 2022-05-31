using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Channels
{
    [DataContract]
    public class ChannelOperator : ContractBase
    {
        public ChannelOperator()
        { 
        }

        [DataMember]
        public long Channel_UID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string CNPJ { get; set; }
        [DataMember]
        public Nullable<decimal> CreditUsed { get; set; }
        [DataMember]
        public string Address1 { get; set; }
        [DataMember]
        public string Address2 { get; set; }
        [DataMember]
        public string PostalCode { get; set; }
        [DataMember]
        public Nullable<long> Country_UID { get; set; }
        [DataMember]
        public string CountryName { get; set; }
        [DataMember]
        public Nullable<long> State_UID { get; set; }
        [DataMember]
        public string StateName { get; set; }
        [DataMember]
        public Nullable<long> City_UID { get; set; }
        [DataMember]
        public string CityName { get; set; }
        [DataMember]
        public System.DateTime CreatedDate { get; set; }
        [DataMember]
        public long CreatedBy { get; set; }
        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        [DataMember]
        public Nullable<long> ModifiedBy { get; set; }
        [DataMember]
        public string Email { get; set; }
    }
}
