using OB.DL.Common.QueryResultObjects;
using OB.Domain.ProactiveActions;
using System.Collections.Generic;

namespace OB.DL.Common.Interfaces
{
    public interface IPropertyEventsRepository : IRepository<PropertyEvent>
    {
        IEnumerable<PropertyEventQR1> FindByPropertyUID_And_SystemEventCode(long propertyUID, string strSystemEventCode);
        IEnumerable<PropertyEvent> FindByCriteria(out int totalRecords, List<long> propertyEventUIDs, List<long> propertyQueueUIDs,
                                                         int pageIndex = 0, int pageSize = 0, bool returnTotal = false);
        
    }
}