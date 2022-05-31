using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Data.Rates
{
    /// <summary>
    /// RateRoomPriceByOccupancyCustom Data Transfer Object
    /// </summary>
    [DataContract]
    public class RateRoomPriceByOccupancyCustom: ContractBase
    {
        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public long RoomType_UID { get; set; }

        [DataMember]
        public long Rate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? Allotment { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string RoomType_Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsFriday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsMonday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsSaturday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsSunday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsThursday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsTuesday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsWednesday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? AcceptsExtraBed { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? ExtraBedPrice { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<decimal?> AdultPriceList { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<decimal?> ChildPriceList { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? NrOfAdults { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? NrOfChilds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? MaxValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal? MinValue { get; set; }

    }
}
