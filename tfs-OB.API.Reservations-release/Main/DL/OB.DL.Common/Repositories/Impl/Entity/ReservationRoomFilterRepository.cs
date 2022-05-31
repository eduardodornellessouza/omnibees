using OB.Domain.Reservations;
using OB.Api.Core;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.Entity;

namespace OB.DL.Common.Repositories.Impl.Entity
{
    internal class ReservationRoomFilterRepository : Repository<ReservationRoomFilter>, IReservationRoomFilterRepository
    {
        public ReservationRoomFilterRepository(IObjectContext context)
            : base(context)
        {
        }



       
    }
}
