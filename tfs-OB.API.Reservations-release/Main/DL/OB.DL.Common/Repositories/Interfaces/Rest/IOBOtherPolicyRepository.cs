using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBOtherPolicyRepository : IRestRepository<OB.BL.Contracts.Data.Rates.OtherPolicy>
    {
        OB.BL.Contracts.Data.Rates.OtherPolicy GetOtherPoliciesByRateId(OB.BL.Contracts.Requests.GetOtherPoliciesRequest request);

        List<OB.BL.Contracts.Data.Rates.TaxPolicy> ListTaxPoliciesByRateIds(OB.BL.Contracts.Requests.ListTaxPoliciesByRateIdsRequest request);
    }
}
