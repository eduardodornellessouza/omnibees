using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.LogHelper
{
    [DataContract]
    public class BaseLogObject
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Guid? RequestGuid { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public object RequestObject { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RuleType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ErrorCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DateTimeUTC { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public object Details { get; set; }
        
        public BaseLogObject()
        {
            DateTimeUTC = DateTime.UtcNow;
        }
    }
}
