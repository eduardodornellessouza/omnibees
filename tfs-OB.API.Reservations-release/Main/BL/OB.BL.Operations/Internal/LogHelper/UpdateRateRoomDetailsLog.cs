using OB.BL.Contracts.Responses;
using OB.Log.Messages;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Operations.Internal.LogHelper
{
    [DataContract]
    public class UpdateRateRoomDetailsLog : LogMessageBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ErrorName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CorrelationId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<UpdateResult> FailedFixedPrices { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<UpdateResult> FailedVariationPrices { get; set; }
    }
}