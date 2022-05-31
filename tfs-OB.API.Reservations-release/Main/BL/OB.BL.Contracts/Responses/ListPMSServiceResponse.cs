using OB.Reservation.BL.Contracts.Data.PMS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that corresponds to a paged set of PMSServices configuration objects.
    /// </summary>
    [DataContract]
    public class ListPMSServiceResponse : PagedResponseBase
    {
        public ListPMSServiceResponse()
        {
            Result = new ObservableCollection<PMSService>();
        }

        [DataMember]
        public IList<PMSService> Result { get; set; }

        [DataMember]
        public Dictionary<long, PMS> PMSLookup { get; set; }
    }
}