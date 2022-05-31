using Couchbase.Core;
using OB.DL.Common.Repositories.Interfaces.Couchbase;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.Couchbase
{
    internal class ReservationLogRepository : Infrastructure.Impl.CouchbaseRepository<OB.Domain.Reservations.ReservationGridLineDetail>, IReservationLogRepository
    {
        public ReservationLogRepository(IBucket bucket)
            : base(bucket)
        {

        }

        public override IQueryable<OB.Domain.Reservations.ReservationGridLineDetail> GetQuery()
        {
            return base.GetQuery(q => q.Action == (int)OB.Events.Contracts.Action.Reservation);
        }
    }
}
