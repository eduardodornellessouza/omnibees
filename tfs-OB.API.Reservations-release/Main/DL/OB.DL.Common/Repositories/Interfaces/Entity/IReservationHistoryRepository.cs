using OB.DL.Common.QueryResultObjects;
using System;
using System.Collections.Generic;
using domain = OB.Domain.Reservations;

namespace OB.DL.Common.Repositories.Interfaces.Entity
{
    public interface IReservationHistoryRepository : IRepository<domain.ReservationsHistory>
    {
        IEnumerable<ReservationHistoryQR1> FindByCriteria(out int totalRecords, List<long> reservationHistoryUIDs, List<long> reservationUids, List<string> reservationNumbers, 
            List<string> statuses,
            DateTime? minChangedDate,
            DateTime? maxChangedDate,
            int pageIndex = -1, int pageSize = -1,
            bool returnTotal = false);
    }
}