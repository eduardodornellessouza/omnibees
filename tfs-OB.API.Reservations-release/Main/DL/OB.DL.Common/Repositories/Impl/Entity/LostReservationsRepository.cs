using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using OB.Api.Core;
using OB.DL.Common.Criteria;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.Entity;


namespace OB.DL.Common.Repositories.Impl.Entity
{
    internal class LostReservationsRepository : Repository<Domain.Reservations.LostReservation>, ILostReservationsRepository
    {
        public LostReservationsRepository(IObjectContext context) : base(context)
        {
        }

        public IQueryable<Domain.Reservations.LostReservation> FindByCriteria(ListLostReservationCriteria criteria)
        {
            var result = GetQuery();

            // Query with PropertyUids
            if (criteria.PropertyUids.Any()) // TODO:
            {
                result = result.Where(q => criteria.PropertyUids.Contains(q.Property_UID));
            }

            // Query with Uids
            if (criteria.Uids.Any())
            {
                result = result.Where(q => criteria.Uids.Contains(q.UID));
            }

            result = result.Where(x => !string.IsNullOrEmpty(x.GuestEmail) && !string.IsNullOrEmpty(x.ReservationTotal));

            // Apply Filters
            if (criteria.NestedFilters != null)
            {
                result = result.FilterBy(criteria.NestedFilters);
            }
            else if (criteria.Filters != null && criteria.Filters.Any())
            {
                result = result.FilterBy(criteria.Filters);
            }

            // Apply Order
            if (criteria.OrderByDescending)
            {
                result = result.OrderByDescending(q => q.UID);
            }
            else
            {
                result = result.OrderBy(criteria.Order);
            }

            return result;
        }

    }
}
