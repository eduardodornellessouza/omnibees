using contractsReservation = OB.Reservation.BL.Contracts.Data.Reservations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public class ListMyAccountReservationsOverviewResponse : PagedResponseBase
    {
        [DataMember]
        public List<contractsReservation.ReservationBEOverview> Result { get; set; }
    }
}