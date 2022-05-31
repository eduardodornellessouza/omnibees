using OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListReservationHistoryResponse : PagedResponseBase
    {
        public ListReservationHistoryResponse()
        {
            Result = new ObservableCollection<ReservationHistory>();
        }

        [DataMember]
        public IList<ReservationHistory> Result { get; set; }
    }
}