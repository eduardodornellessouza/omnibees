using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.Domain.ProactiveActions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace OB.DL.Common.Impl
{
    internal class PropertyEventsRepository : Repository<PropertyEvent>, IPropertyEventsRepository
    {
        public PropertyEventsRepository(IObjectContext context)
            : base(context)
        {
        }


        public IEnumerable<PropertyEvent> FindByCriteria(out int totalRecords, List<long> propertyEventUIDs, List<long> propertyQueueUIDs,                         
                                                         int pageIndex = 0, int pageSize = 0, bool returnTotal = false)
        {
            var result = GetQuery();
            totalRecords = -1;

            if (propertyEventUIDs != null && propertyEventUIDs.Count > 0)
                result = result.Where(x => propertyEventUIDs.Contains(x.UID));

            if (propertyQueueUIDs != null && propertyQueueUIDs.Count > 0)
                result = result.Where(x => x.PropertyQueues.Any(y => propertyQueueUIDs.Contains(y.UID)));

            if (returnTotal)
                totalRecords = result.Count();

            if (pageIndex > 0 && pageSize > 0)
                result = result.OrderBy(x => x.UID).Skip(pageIndex * pageSize);

            if (pageSize > 0)
                result = result.Take(pageSize);

            return result.ToList();
        }

        public IEnumerable<PropertyEventQR1> FindByPropertyUID_And_SystemEventCode(long propertyUID, string strSystemEventCode)
        {
            return _context.Context.Database.SqlQuery<PropertyEventQR1>(FINDEVENTS_BY_PROPERTYUID_AND_SYSTEMEVENTCODE,
                new SqlParameter("propertyUID", propertyUID),
            new SqlParameter("strSystemEventCode", SqlDbType.NVarChar, 4)
            {
                Value = strSystemEventCode
            });
        }

        private static readonly string FINDEVENTS_BY_PROPERTYUID_AND_SYSTEMEVENTCODE = @"
                SELECT
                    1 AS [C1],
                    [Project1].[UID] AS [UID],
                    [Project1].[ReservationOption] AS [ReservationOption]
                    FROM ( SELECT
                        [Extent1].[UID] AS [UID],
                        [Extent1].[PropertyTemplate_UID] AS [PropertyTemplate_UID],
                        [Extent1].[Property_UID] AS [Property_UID],
                        [Extent1].[IsDelete] AS [IsDelete],
                        [Extent1].[ReservationOption] AS [ReservationOption],
                        [Extent2].[UID] AS [UID1],
                        [Extent2].[Code] AS [Code],
                        (SELECT
                            COUNT(1) AS [A1]
                            FROM [dbo].[PropertyEventConditions] AS [Extent3]
                            WHERE [Extent1].[UID] = [Extent3].[PropertyEvent_UID]) AS [C1]
                        FROM  [dbo].[PropertyEvents] AS [Extent1]
                        INNER JOIN [dbo].[SystemEvents] AS [Extent2] ON [Extent1].[SystemEvent_UID] = [Extent2].[UID]
                    )  AS [Project1]
                    WHERE ([Project1].[Property_UID] = @propertyUID) AND (0 = [Project1].[IsDelete]) AND ([Project1].[Code] = @strSystemEventCode)
                    AND ([Project1].[PropertyTemplate_UID] IS NOT NULL)
                    --AND ([Project1].[C1] > 0)
            ";
    }
}