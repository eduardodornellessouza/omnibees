using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System.Collections.Generic;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBChannelRepository : RestRepository<OB.BL.Contracts.Data.Channels.Channel>, IOBChannelRepository
    {
        public List<OB.BL.Contracts.Data.Channels.Channel> ListChannel(OB.BL.Contracts.Requests.ListChannelRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Channels.Channel>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListChannelResponse>(request, "Channels", "ListChannels");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<OB.BL.Contracts.Data.Channels.ChannelOperator> ListChannelOperators(OB.BL.Contracts.Requests.ListChannelOperatorsRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Channels.ChannelOperator>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListChannelOperatorsResponse>(request, "Channels", "ListChannelOperators");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }

        public List<OB.BL.Contracts.Data.Channels.ChannelLight> ListChannelLight(OB.BL.Contracts.Requests.ListChannelLightRequest request)

        {
            var data = new List<OB.BL.Contracts.Data.Channels.ChannelLight>();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListChannelLightResponse>(request, "Channels", "ListChannelLight");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result.ToList();

            return data;
        }


        /// <summary>
        /// Update Operator Credit
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="channelId"></param>
        /// <param name="paymentMethodCode">Payment Method Code - 4 (Invoicing), 7 (PrePayment)</param>
        /// <param name="creditValue">value to add or take from credit</param>
        /// <param name="sendCreditLimitExcededEmail">output parameter indicating if credit limit as been exceeded</param>
        /// <param name="channelName">output parameter indicating the channel name</param>
        /// <param name="creditLimit">output parameter indicating the excedeed limit</param>
        /// <returns>number of affected lines</returns>
        public OB.BL.Contracts.Responses.UpdateOperatorCreditUsedResponse UpdateOperatorCreditUsed(OB.BL.Contracts.Requests.UpdateOperatorCreditUsedRequest request)

        {
           return RESTServicesFacade.Call<OB.BL.Contracts.Responses.UpdateOperatorCreditUsedResponse>(request, "Channels", "UpdateOperatorCreditUsed");
        }

        /// <summary>
        /// Update TPI Credit
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="channelId"></param>
        /// <param name="paymentMethodCode">Payment Method Code - 4 (Invoicing), 7 (PrePayment)</param>
        /// <param name="creditValue">value to add or take from credit</param>
        /// <param name="sendCreditLimitExcededEmail">output parameter indicating if credit limit as been exceeded</param>
        /// <param name="paymentAproved">output parameter indicating if payment as been aproved</param>
        /// <returns>number of affected lines</returns>
        public OB.BL.Contracts.Responses.UpdateTPICreditUsedResponse UpdateTPICreditUsed(OB.BL.Contracts.Requests.UpdateTPICreditUsedRequest request)

        {
            return RESTServicesFacade.Call<OB.BL.Contracts.Responses.UpdateTPICreditUsedResponse>(request, "Channels", "UpdateTPICreditUsed");
        }
    }
}