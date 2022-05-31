using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    public class ReservationRoomCancelationCost
    {
        public string Number { get; set; }
        public int Status { get; set; }
        public decimal CancelationCosts { get; set; }

        /// <summary>
        /// NotApply is true when is not possible to calculate de cancellation costs of the reservation room.
        /// </summary>
        public bool NotApply { get; set; }

        /// <summary>
        /// Rate currency uid
        /// </summary>
        public long? CurrencyUid { get; set; }
    }
}
