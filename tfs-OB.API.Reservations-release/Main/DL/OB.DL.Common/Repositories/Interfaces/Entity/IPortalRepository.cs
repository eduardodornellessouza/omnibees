using OB.DL.Common.Infrastructure;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using PO.BL.Contracts.Requests;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Entity
{
    public interface IPortalRepository : IRepository
    {
        List<SellRule> ListMarkupCommissionRules(ListMarkupCommissionRulesRequest request);
    }
}