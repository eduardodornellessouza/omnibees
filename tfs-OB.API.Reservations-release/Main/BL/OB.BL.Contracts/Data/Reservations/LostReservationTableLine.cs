using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public class LostReservationTableLine : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public decimal Value { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CurrencyName { get; set; }
    }
}
