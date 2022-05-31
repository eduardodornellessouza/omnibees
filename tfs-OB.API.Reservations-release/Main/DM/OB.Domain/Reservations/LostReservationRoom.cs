using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Domain.Reservations
{
    public partial class LostReservationRoom
    {
        public long UID { get; set; }
        public string Name { get; set; }
        public long RateUID { get; set; }
        public string RateName { get; set; }
        public string GuestName { get; set; }

        public int Nights { get; set; }
        public int Adults { get; set; }
        public int Childs { get; set; }
        public string ChildsAges { get; set; }

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public long CurrencyUID { get; set; }
        public string CurrencyName { get; set; }

        public decimal RateTotal { get; set; }

        public decimal TotalAmountOnlyRate { get; set; }
        
        public List<LostReservationTableLine> Extras { get; set; }
        public List<LostReservationTableLine> Incentives { get; set; }

        public decimal Total
        {
            get
            {
                decimal value = 0;

                Extras.Select(x => value = value + x.Value);
                Incentives.Select(x => value = value + x.Value);

                return value + RateTotal;
            }
        }

        public LostReservationRoom()
        {
            Extras = new List<LostReservationTableLine>();
            Incentives = new List<LostReservationTableLine>();
        }
    }
}
