using OB.DL.Common.Criteria;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.Domain.Reservations;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Cached
{
    public interface IGroupRulesRepository : IRepository<GroupRule>
    {
        /// <summary>
        /// Get GroupRule with business rules specific for the rule type.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        GroupRule GetGroupRule(GetGroupRuleCriteria criteria);
    }
}