using OB.Reservation.BL.Contracts.Data.PMS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListPMSMappingResponse : PagedResponseBase
    {
        [DataMember]
        public IList<PMSMapping> Result { get; set; }

        public ListPMSMappingResponse()
        {
            Result = new ObservableCollection<PMSMapping>();
        }
    }
}