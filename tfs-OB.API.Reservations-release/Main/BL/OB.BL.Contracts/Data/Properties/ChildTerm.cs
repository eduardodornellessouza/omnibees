using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Properties
{
    [DataContract]
    public class ChildTerm : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsActivePriceVariation { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsValueDecrease { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsCountForOccupancy { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? ModifiedDate { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? CreatedDate { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsFree { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual List<ChildTermLanguage> ChildTermLanguages { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDeleted { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsPercentage { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? Value { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool CountsAsAdult { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int AgeTo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int AgeFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsValuePerNight { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual List<ChildTermsCurrency> ChildTermsCurrencies { get; set; }
    }
}
