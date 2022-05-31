using OB.Reservation.BL.Contracts.Data.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    public class ListClientResponse : PagedResponseBase
    {
        public ListClientResponse()
        {
            Result = new ObservableCollection<ClientCustom>();
        }

        [DataMember]
        public IList<ClientCustom> Result { get; set; }
    }
}