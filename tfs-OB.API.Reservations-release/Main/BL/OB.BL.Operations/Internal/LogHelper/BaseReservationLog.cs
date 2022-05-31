using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.LogHelper
{
    [DataContract]
    public class BaseReservationLog : BaseLogObject
    {
        public BaseReservationLog() : base() { }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long ReservationUID { get; set; }
    
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ReservationNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckIn { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckOut { get; set; }
    }
}
