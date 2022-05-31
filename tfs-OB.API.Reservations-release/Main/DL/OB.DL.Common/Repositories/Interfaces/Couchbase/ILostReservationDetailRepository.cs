using OB.Domain.Reservations;
using System;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Couchbase
{
    public interface ILostReservationDetailRepository : IRepository<LostReservationDetail>
    {
        Dictionary<Guid, LostReservationDetail> FindByUids(out int totalRecords, List<string> uids, int pageIndex = 0, int pageSize = 0, bool returnTotal = false);
    }
}
