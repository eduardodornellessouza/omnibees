using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class ReservationRoomIncentive : ContractBase
    {
        public ReservationRoomIncentive()
        {
        }

        [DataMember]
        public long IncentiveId { get; set; }

        [DataMember]
        public List<DatesRange> Periods { get; set; }
    }
}