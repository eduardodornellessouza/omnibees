using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Data.Reservations
{
    [DataContract]
    public partial class LostReservationGuest : ContractBase
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string FirstName { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LastName { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Email { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PhoneNumber { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string VATNumber { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CountryName { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CountryUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string StateName { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long StateUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CityName { get; set; }
        
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CityUID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Address { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PostalCode { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime Birthday { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string IpAddress { get; set; }

    }
}
