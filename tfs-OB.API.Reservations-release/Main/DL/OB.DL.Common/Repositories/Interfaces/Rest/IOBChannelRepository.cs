using System.Collections.Generic;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBChannelRepository : IRestRepository<OB.BL.Contracts.Data.Channels.Channel>
    {
        List<OB.BL.Contracts.Data.Channels.ChannelLight> ListChannelLight(OB.BL.Contracts.Requests.ListChannelLightRequest request);

        List<OB.BL.Contracts.Data.Channels.Channel> ListChannel(OB.BL.Contracts.Requests.ListChannelRequest request);

        List<OB.BL.Contracts.Data.Channels.ChannelOperator> ListChannelOperators(OB.BL.Contracts.Requests.ListChannelOperatorsRequest request);

        OB.BL.Contracts.Responses.UpdateOperatorCreditUsedResponse UpdateOperatorCreditUsed(OB.BL.Contracts.Requests.UpdateOperatorCreditUsedRequest request);

        OB.BL.Contracts.Responses.UpdateTPICreditUsedResponse UpdateTPICreditUsed(OB.BL.Contracts.Requests.UpdateTPICreditUsedRequest request);
    }
}
