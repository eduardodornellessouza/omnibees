using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;
using System.Linq;


namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that stores the response information for an RateRoomDetails RESTful operation.
    /// </summary>
    //[DataContract]
    public class ListRateRoomsDetailsResponse : PagedResponseBase
    {
        public ListRateRoomsDetailsResponse()
        {
            Result = new ObservableCollection<RateRoomDetails>();
        }

        [DataMember]
        public IList<RateRoomDetails> Result { get; set; }
    }
}