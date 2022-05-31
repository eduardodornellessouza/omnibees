using OB.Reservation.BL.Contracts.Data.Rates;
using OB.Reservation.BL.Contracts.Data.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class ValidatePromocodeForReservationRequest : RequestBase
    {
        public List<ReservationRoomStayPeriod> ReservationRooms { get; set; }
        public long? PromocodeUID { get; set; }
        public string PromoCode { get; set; }
        public long? CurrencyUID { get; set; }
        public List<DateTime> OldAppliedDiscountDays { get; set; }
    }
}
