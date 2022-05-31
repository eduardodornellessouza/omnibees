using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBRateBuyerGroupRepository: RestRepository<OB.BL.Contracts.Data.Rates.RateBuyerGroup>, IOBRateBuyerGroupRepository
    {
        public OB.BL.Contracts.Data.Rates.RateBuyerGroup GetRateBuyerGroup(long rateId, long tpiId)

        {
            OB.BL.Contracts.Data.Rates.RateBuyerGroup data = null;
            var request = new OB.BL.Contracts.Requests.GetRateBuyerGroupRequest
            {
                RateId = rateId,
                TpiId = tpiId
            };

            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.GetRateBuyerGroupResponse>(request, "Rates", "GetRateBuyerGroup");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }



        public List<BL.Contracts.Data.Rates.RateChannel> ListRatesChannels(BL.Contracts.Requests.ListRatesChannelsRequest request)

        {
            var data = new List<BL.Contracts.Data.Rates.RateChannel>();
            var response = RESTServicesFacade.Call<BL.Contracts.Responses.ListRatesChannelsResponse>(request, "Rates", "ListRatesChannels");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

    }

}
