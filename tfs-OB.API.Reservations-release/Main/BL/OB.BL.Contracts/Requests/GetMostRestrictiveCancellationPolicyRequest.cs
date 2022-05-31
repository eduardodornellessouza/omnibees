using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Request class that contains the criteria that is used in searches for Channels.
    /// </summary>
    [DataContract]
    public class GetMostRestrictiveCancellationPolicyRequest : RequestBase
    {
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public DateTime checkIn { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime checkOut { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long rateId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long currencyId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long propertyId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long roomTypeId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long channelId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int adultCount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int childCount { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<int> ages { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? promotionalCodeId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string promotionalCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? TpiId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long languageId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public OB.BL.Constants.RuleType? RuleType { get; set; }
    }
}