using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace OB.Reservation.BL.Contracts.Data.VisualStates
{
    public class ReservationReadStatus
    {
        public long UserId { get; set; }
        public DateTime Date { get; set; }
        public bool Read { get; set; }

        public long ReservationId { get; set; }

        public ReservationReadStatus()
        {

        }

        public ReservationReadStatus(long userId, DateTime date, bool read)
        {
            this.UserId = userId;
            this.Date = date;
            this.Read = read;
        }
    }
}
