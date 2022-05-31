using OB.Domain.Reservations;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Entity
{
    public interface IVisualStateRepository : IRepository<VisualState>
    {
        IEnumerable<VisualState> FindVisualStateByCriteria(List<long> UIDs = null, List<string> LookupKey_1 = null, List<string> LookupKey_2 = null, List<string> LookupKey_3 = null);

    }
}