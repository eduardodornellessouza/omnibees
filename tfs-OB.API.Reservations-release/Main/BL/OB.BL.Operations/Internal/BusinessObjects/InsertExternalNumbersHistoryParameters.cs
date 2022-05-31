using OB.Reservation.BL.Contracts.Data.PMS;
using System;
using System.Collections.Generic;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class InsertExternalNumbersHistoryParameters
    {
        public string RequestId { get; set; }
        public long ReservationStatus { get; set; }
        public long PropertyId { get; set; }
        public long ClientId { get; set; }
        public long PmsId { get; set; }
        public DateTime CheckInDate { get; set; }
        public bool IsByReservationRoom { get; set; }
        public ReservationExternalIdentifier ReservationExternalNumbers { get; set; }
        public List<ReservationRoomExternalIdentifier> ReservationRoomExternalNumbers { get; set; }
        public List<int?> ReservationRoomStatus { get; set; }
    }
}
