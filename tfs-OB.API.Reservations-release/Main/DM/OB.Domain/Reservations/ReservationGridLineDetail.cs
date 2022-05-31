using OB.Events.Contracts.Data.UpdateRates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Domain.Reservations
{
    public class ReservationGridLineDetail : BaseGridLineDetail
    {
        public ReservationGridLineDetail() 
            : base()
        {

        }
        
        public string ReservationNumber { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string GuestName { get; set; }
        public string ChannelName { get; set; }
        public int ReservationStatus { get; set; }
        public string ReservationTotal { get; set; }

    }
}
