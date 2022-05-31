using Couchbase.Core;
using OB.DL.Common.Infrastructure.Impl;
using OB.DL.Common.Repositories.Interfaces.Couchbase;
using OB.Domain.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.Couchbase
{
    internal class LostReservationDetailRepository : CouchbaseRepository<LostReservationDetail>, ILostReservationDetailRepository
    {
        public LostReservationDetailRepository(IBucket bucket)
            : base(bucket)
        {
        }

        public Dictionary<Guid, LostReservationDetail> FindByUids(out int totalRecords, List<string> uids, int pageIndex = 0, int pageSize = 0, bool returnTotal = false)
        {
            return FindByUIDs<LostReservationDetail>(out totalRecords, uids.Select(x => x.ToString()).ToList(), pageIndex, pageSize, returnTotal);
        }        
    }
}
