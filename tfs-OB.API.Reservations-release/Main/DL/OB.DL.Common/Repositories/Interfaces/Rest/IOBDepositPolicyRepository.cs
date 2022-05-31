using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBDepositPolicyRepository : IRestRepository<OB.BL.Contracts.Data.Rates.GuaranteeType>
    {

        List<OB.BL.Contracts.Data.Rates.DepositPolicy> ListDepositPolicies(OB.BL.Contracts.Requests.ListDepositPoliciesRequest request);

        OB.BL.Contracts.Data.Rates.DepositPolicy CalculateMostRestrictiveDepositPolicy(OB.BL.Contracts.Requests.CalculateMostRestrictiveDepositPolicyRequest request);

        List<OB.BL.Contracts.Data.Rates.GuaranteeType> ListGuaranteeTypesFilter(OB.BL.Contracts.Requests.ListGuaranteeTypesFilterRequest request);

        List<OB.BL.Contracts.Data.Rates.DepositPoliciesGuaranteeType> ListDepositPoliciesGuaranteeTypes(OB.BL.Contracts.Requests.ListDepositPoliciesGuaranteeTypesRequest request);
    }
}
