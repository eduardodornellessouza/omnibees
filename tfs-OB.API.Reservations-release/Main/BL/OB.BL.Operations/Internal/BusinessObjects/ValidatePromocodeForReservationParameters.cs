using System;
using System.Collections.Generic;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ValidatePromocodeForReservationParameters
    {
        public List<ReservationRoomStayPeriod> ReservationRooms { get; set; }
        public long? PromocodeUID { get; set; }
        public string PromoCode { get; set; }
        public long? CurrencyUID { get; set; }
        public List<DateTime> OldAppliedDiscountDays { get; set; }
    }
}
