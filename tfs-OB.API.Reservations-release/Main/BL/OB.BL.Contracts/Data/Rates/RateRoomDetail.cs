using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateRoomDetail : ContractBase
    {
        public RateRoomDetail()
        {
        }


        [DataMember]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public System.DateTime Date { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public System.DateTime DateFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public System.DateTime DateTo { get; set; }

        [DataMember]
        public long RateRoom_UID { get; set; }

        [DataMember]
        public Nullable<int> Allotment { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CreatedDate { get; set; }
        [DataMember]
        public Nullable<long> CreateBy { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> ModifyBy { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<System.DateTime> ModifyDate { get; set; }
        
        [DataMember]
        public Nullable<int> AllotmentUsed { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> ExtraBedPrice { get; set; }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string correlationID { get; set; }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_1 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_2 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_3 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_4 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_5 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_6 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_7 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_8 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_9 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_10 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_11 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_12 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_13 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_14 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_15 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_16 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_17 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_18 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_19 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Adult_20 { get; set; }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_1 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_2 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_3 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_4 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_5 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_6 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_7 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_8 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_9 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_10 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_11 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_12 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_13 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_14 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_15 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_16 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_17 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_18 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_19 { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<decimal> Child_20 { get; set; }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> MinimumLengthOfStay { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> StayThrough { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> ClosedOnArrival { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> ClosedOnDeparture { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> ReleaseDays { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<int> MaximumLengthOfStay { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string BlockedChannelsListUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> isBookingEngineBlocked { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ChannelsListUID { get; set; }

        public Nullable<long> DepositPolicy_UID { get; set; }
        public Nullable<long> CancellationPolicy_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] Revision { get; set; }


        //public Nullable<bool> IsOccupancy { get; set; }

        //public decimal Price { get; set; }

        //public bool IsDifferentThanRate { get; set; }
    }
}