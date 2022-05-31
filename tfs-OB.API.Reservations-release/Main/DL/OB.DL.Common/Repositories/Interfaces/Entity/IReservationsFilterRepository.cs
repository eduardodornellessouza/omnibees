using System.Collections.Generic;
using domain = OB.Domain.Reservations;

namespace OB.DL.Common.Repositories.Interfaces.Entity
{
    public interface IReservationsFilterRepository : IRepository<domain.ReservationFilter>
    {
        IEnumerable<domain.ReservationFilter> FindByReservationUIDs(List<long> reservation_UIDs);
        void UpdateReservationFilterStatus(long reservationId, int reservationStatus, long? userId);

        IEnumerable<long> FindByCriteria(ListReservationFilterCriteria request, out int totalRecords,
            bool returnTotal = false);

        IEnumerable<domain.ReservationFilter> FindReservationFilterByCriteria(ListReservationFilterCriteria request, out int totalRecords, bool returnTotal = false);

    }
}