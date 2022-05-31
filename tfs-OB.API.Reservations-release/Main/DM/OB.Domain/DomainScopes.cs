using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.Api.Core;

namespace OB.Domain
{
    public static class DomainScopes
    {
        public static readonly DomainScope Reservations = new DomainScope("Reservations", "ReservationsContext", "ReservationsContextReadOnly", DomainScope.DomainScopeType.EntityFramework);
        public static readonly DomainScope Payments = new DomainScope("Payments", "PaymentsContext", "PaymentsContextReadOnly", DomainScope.DomainScopeType.EntityFramework);
        public static readonly DomainScope Omnibees = new DomainScope("Omnibees", "OmnibeesConnectionString", "OmnibeesConnectionStringReadOnly", DomainScope.DomainScopeType.SqlServer);

        public static IEnumerable<DomainScope> GetAll()
        {
            return new List<DomainScope> { Reservations, Payments };
        }
    }  
}
