using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class DeployPMSReceipeRequest : RequestBase
    {
        [DataMember(EmitDefaultValue=false, IsRequired=true)]
        public long Property_UID { get; set; }
    
        public DeployPMSReceipeRequest()
        {
            RequestGuid = Guid.NewGuid();
        }
    }
}
