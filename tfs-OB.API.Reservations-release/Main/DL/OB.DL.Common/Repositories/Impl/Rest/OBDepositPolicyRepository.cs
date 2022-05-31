using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBDepositPolicyRepository : RestRepository<OB.BL.Contracts.Data.Rates.GuaranteeType>, IOBDepositPolicyRepository
    {
        public List<OB.BL.Contracts.Data.Rates.DepositPolicy> ListDepositPolicies(OB.BL.Contracts.Requests.ListDepositPoliciesRequest request)
        {
            var data = new List<OB.BL.Contracts.Data.Rates.DepositPolicy>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListDepositPoliciesResponse>(request, "DepositPolicy", "ListDepositPolicies");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }


        public OB.BL.Contracts.Data.Rates.DepositPolicy CalculateMostRestrictiveDepositPolicy(OB.BL.Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest request)

        {
            var data = new OB.BL.Contracts.Data.Rates.DepositPolicy();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.CalculateMostRestrictiveDepositPolicyResponse>(request, "DepositPolicy", "CalculateMostRestrictiveDepositPolicy");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }


        public List<OB.BL.Contracts.Data.Rates.GuaranteeType> ListGuaranteeTypesFilter(OB.BL.Contracts.Requests.ListGuaranteeTypesFilterRequest request)
        {            
            var data = new List<OB.BL.Contracts.Data.Rates.GuaranteeType>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListGuaranteeTypeResponse>(request, "GuaranteeType", "ListGuaranteeTypesFilter");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public List<OB.BL.Contracts.Data.Rates.DepositPoliciesGuaranteeType> ListDepositPoliciesGuaranteeTypes(OB.BL.Contracts.Requests.ListDepositPoliciesGuaranteeTypesRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Rates.DepositPoliciesGuaranteeType>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListDepositPoliciesGuaranteeTypesResponse>(request, "DepositPolicy", "ListDepositPoliciesGuaranteeTypes");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Results;

            return data;
        }
    }
}