using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBRateBuyerGroupRepository : IRestRepository<OB.BL.Contracts.Data.Rates.RateBuyerGroup>
    {
        OB.BL.Contracts.Data.Rates.RateBuyerGroup GetRateBuyerGroup(long rateId, long tpiId);
        List<BL.Contracts.Data.Rates.RateChannel> ListRatesChannels(BL.Contracts.Requests.ListRatesChannelsRequest request);
    }
}
