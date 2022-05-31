using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public partial class LostReservationRoom : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RateUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string RateName { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string GuestName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int Nights { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int Adults { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int Childs { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ChildsAges { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckIn { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime CheckOut { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CurrencyUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CurrencyName { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal RateTotal { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal TotalAmountOnlyRate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<LostReservationTableLine> Extras { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<LostReservationTableLine> Incentives { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal Total
        {
            get
            {
                decimal value = 0;

                foreach(var extra in Extras)
                {
                    value = value + extra.Value;
                }

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
