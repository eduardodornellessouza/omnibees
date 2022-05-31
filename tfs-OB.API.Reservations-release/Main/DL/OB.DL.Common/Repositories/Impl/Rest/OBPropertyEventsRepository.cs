using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.Repositories.Interfaces.Rest;

namespace OB.DL.Common.Repositories.Impl.Rest
{
    internal class OBPropertyEventsRepository : RestRepository<OB.BL.Contracts.Data.ProactiveActions.ProactiveAction>, IOBPropertyEventsRepository
    {
        public long InsertPropertyQueue(InsertPropertyQueueRequest request)
        {
            long data = 0;
            var response = RESTServicesFacade.Call<InsertPropertyQueueResponse>(request, "PropertyEvents", "InsertPropertyQueue");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;

        }

        public long CancelPropertyQueueEvent(CancelPropertyQueueEventRequest request)
        {
            long data = 0;
            var response = RESTServicesFacade.Call<CancelPropertyQueueEventResponse>(request, "PropertyEvents", "CancelPropertyQueueEvent");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public bool PreparePropertyQueueToSendEmail(PreparePropertyQueueRequest request)
        {
            var data = false;
            var response = RESTServicesFacade.Call<PreparePropertyQueueResponse>(request, "PropertyEvents", "PreparePropertyQueueToSendEmail");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response.Result;

            return data;
        }

        public SendmailToNewGuestResponse SendmailToNewGuest(SendmailToNewGuestRequest request)
        {
            var data = new SendmailToNewGuestResponse();
            var response = RESTServicesFacade.Call<SendmailToNewGuestResponse>(request, "PropertyEvents", "SendmailToNewGuest");

            return data;
        }

    }
}
