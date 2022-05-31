using System;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.General
{
    [DataContract]
    public class BigPullAuthentication : ContractBase
    {
        public BigPullAuthentication()
        {
        }


        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool PropertiesRestricted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ChannelUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ChannelCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Username { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Password { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string IPList { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Properties { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string POSCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<byte> POSType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> PartnerId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PartnerName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public EnumUserType UserType { get; set; }
    }
}