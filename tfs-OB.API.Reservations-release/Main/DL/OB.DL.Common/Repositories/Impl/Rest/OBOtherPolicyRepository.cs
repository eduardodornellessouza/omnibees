using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBOtherPolicyRepository : RestRepository<OB.BL.Contracts.Data.Rates.OtherPolicy>, IOBOtherPolicyRepository
    {
        public OB.BL.Contracts.Data.Rates.OtherPolicy GetOtherPoliciesByRateId(OB.BL.Contracts.Requests.GetOtherPoliciesRequest request)

        {
            var data = new OB.BL.Contracts.Data.Rates.OtherPolicy();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.GetOtherPoliciesResponse>(request, "Rates", "GetOtherPoliciesByRateId");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<OB.BL.Contracts.Data.Rates.TaxPolicy> ListTaxPoliciesByRateIds(OB.BL.Contracts.Requests.ListTaxPoliciesByRateIdsRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Rates.TaxPolicy>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListTaxPoliciesResponse>(request, "Rates", "ListTaxPoliciesByRateIds");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

    }
}
