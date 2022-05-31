using OB.Reservation.BL.Contracts.Data.PMS;
using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of Channel objects.
    /// </summary>
    [DataContract]
    public class ListChildTermsResponse : PagedResponseBase
    {
        public ListChildTermsResponse()
        {
            Result = new List<ChildTerm>();
        }

        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public List<ChildTerm> Result { get; set; }
    }
}