using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBCancellationPolicyRepository : RestRepository<OB.BL.Contracts.Data.Rates.CancellationPolicy>, IOBCancellationPolicyRepository
    {
        public List<OB.BL.Contracts.Data.Rates.CancellationPolicy> ListCancelationPolicies(OB.BL.Contracts.Requests.ListCancellationPoliciesRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Rates.CancellationPolicy>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListCancellationPoliciesResponse>(request, "CancellationPolicy", "ListCancelationPolicies");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }


        public OB.BL.Contracts.Data.Rates.CancellationPolicy CalculateMostRestrictiveCancellationPolicy(OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest request)

        {
            var data = new OB.BL.Contracts.Data.Rates.CancellationPolicy();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.CalculateMostRestrictiveCancellationPolicyResponse>(request, "CancellationPolicy", "CalculateMostRestrictiveCancellationPolicy");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }
    }
}
