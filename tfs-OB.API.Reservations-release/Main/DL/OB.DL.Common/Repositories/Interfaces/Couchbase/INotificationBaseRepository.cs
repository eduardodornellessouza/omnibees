using System;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Couchbase
{
    public interface INotificationBaseRepository : IRepository<OB.Domain.Reservations.NotificationBase>
    {
        Dictionary<Guid, string> GetDocumentByUids(out int totalRecords, List<Guid> Uids, int pageIndex = 0, int pageSize = 0, bool returnTotal = false);
    }
}
