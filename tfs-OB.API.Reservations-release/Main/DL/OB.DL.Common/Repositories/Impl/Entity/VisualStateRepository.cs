using OB.Domain.Reservations;
using System.Collections.Generic;
using System.Linq;
using OB.Api.Core;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.Entity;

namespace OB.DL.Common.Repositories.Impl.Entity
{
    internal class VisualStateRepository : Repository<VisualState>, IVisualStateRepository
    {
        public VisualStateRepository(IObjectContext context)
            : base(context)
        {
        }

        public IEnumerable<VisualState> FindVisualStateByCriteria(List<long> UIDs = null, List<string> LookupKey_1 = null, List<string> LookupKey_2 = null, List<string> LookupKey_3 = null)
        {
            var result = GetQuery();

            if (UIDs != null && UIDs.Count > 0)
                result = result.Where(x => UIDs.Contains(x.UID));

            if (LookupKey_1 != null && LookupKey_1.Count > 0)
                result = result.Where(x => LookupKey_1.Contains(x.LookupKey_1));

            if (LookupKey_2 != null && LookupKey_2.Count > 0)
                result = result.Where(x => LookupKey_2.Contains(x.LookupKey_2));

            if (LookupKey_3 != null && LookupKey_3.Count > 0)
                result = result.Where(x => LookupKey_3.Contains(x.LookupKey_3));

            return result;
        }

    }
}