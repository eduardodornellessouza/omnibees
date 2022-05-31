using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Domain.Reservations
{
    public partial class LostReservationDetail : DocumentDomainObject
    {
        public LostReservationGuest Guest { get; set; }
        public List<LostReservationRoom> Rooms { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal Total { get; set; }
        public decimal RoomsTotalAmountOnlyRates { get; set; }

        public long CurrencyUID { get; set; }
        public string CurrencyName { get; set; }
        public string Comments { get; set; }
        public string Request1 { get; set; }
        public string Request2 { get; set; }
        public string Request3 { get; set; }
        public string Request4 { get; set; }
        public long PropertyUID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string IPAddress { get; set; }
    }
}
