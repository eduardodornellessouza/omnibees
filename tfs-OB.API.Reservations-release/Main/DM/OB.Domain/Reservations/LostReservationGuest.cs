using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Domain.Reservations
{
    public partial class LostReservationGuest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string VATNumber { get; set; }
        public string CountryName { get; set; }
        public long CountryUID { get; set; }
        public string StateName { get; set; }
        public long StateUID { get; set; }
        public string CityName { get; set; }
        public long CityUID { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public DateTime Birthday { get; set; }
        public string IpAddress { get; set; }

    }
}
