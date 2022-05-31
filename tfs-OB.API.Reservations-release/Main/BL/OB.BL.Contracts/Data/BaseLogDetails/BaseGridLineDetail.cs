using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.BaseLogDetails
{
    [DataContract]
    public class BaseGridLineDetail
    {
        public BaseGridLineDetail()
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int Action { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CreatedDate { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CreatedByUsername { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CreatedBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GridLineId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LogId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long PropertyId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<int> SubActions { get; set; }
    }
}
