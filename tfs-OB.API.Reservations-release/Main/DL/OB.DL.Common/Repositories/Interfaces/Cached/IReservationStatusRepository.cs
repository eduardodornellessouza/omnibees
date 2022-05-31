using System.Collections.Generic;
using OB.DL.Common.Criteria;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.Domain.Reservations;

namespace OB.DL.Common.Repositories.Interfaces.Cached
{
    public interface IReservationStatusRepository : IRepository<ReservationStatus>
    {
        IEnumerable<ReservationStatus> FindByCriteria(ListReservationStatusCriteria criteria);
    }
}