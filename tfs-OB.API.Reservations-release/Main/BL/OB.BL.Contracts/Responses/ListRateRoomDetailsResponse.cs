using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using OB.Reservation.BL.Contracts.Data.Rates;
using System.Collections.Generic;


namespace OB.Reservation.BL.Contracts.Responses
{
    /// <summary>
    /// Class that stores the response information for an RateRoomDetails RESTful operation.
    /// </summary>
    [DataContract]
    public class ListRateRoomDetailsResponse : ListGenericPagedResponse<RateRoomDetail>
    {
        public ListRateRoomDetailsResponse()
        {
            Result = new ObservableCollection<RateRoomDetail>();
        }

        [DataMember]
        public bool HasRRDForAllDays;

    }
}