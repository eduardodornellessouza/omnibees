namespace OB.BL.Contracts.Data.General
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class Image : ContractBase
    {
        public Image()
        {
        }

        [DataMember(IsRequired = true)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Image1 { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> MediaCategory_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GeoLocationLat { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GeoLocationLng { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> RoomType_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Package_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Attraction_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Extras_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> Client_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> CalenderEvents_UID { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ThirdPartyIntermediary_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool isBeDefault { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string FileExtension { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }


    }
}
