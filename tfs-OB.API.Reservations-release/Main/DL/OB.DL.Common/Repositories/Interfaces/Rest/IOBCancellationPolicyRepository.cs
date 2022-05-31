using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBCancellationPolicyRepository : IRestRepository<OB.BL.Contracts.Data.Rates.CancellationPolicy>
    {
        List<OB.BL.Contracts.Data.Rates.CancellationPolicy> ListCancelationPolicies(OB.BL.Contracts.Requests.ListCancellationPoliciesRequest request);

        OB.BL.Contracts.Data.Rates.CancellationPolicy CalculateMostRestrictiveCancellationPolicy(OB.BL.Contracts.Requests.CalculateMostRestrictiveCancellationPolicyRequest request);
    }
}
