using System.Collections.Generic;
using System.Linq;
using OB.DL.Common.Infrastructure;
using PO.BL.Contracts.Requests;
using PO.BL.Contracts.Responses;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using OB.DL.Common.Repositories.Interfaces.Entity;

namespace OB.DL.Common.Repositories.Impl.Entity
{
    internal class PortalRepository : IPortalRepository
    {
        public PortalRepository() : base()
        {

        }

        public List<SellRule> ListMarkupCommissionRules(ListMarkupCommissionRulesRequest request)
        {
            var data = new List<SellRule>();
            
            var response = RESTServicesFacade.Call<ListMarkupCommissionRulesResponse>(request,
                "OperatorMarkupCommission", "ListMarkupCommissionRules");

            if (response.Result != null && response.Status == PO.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

    }
}