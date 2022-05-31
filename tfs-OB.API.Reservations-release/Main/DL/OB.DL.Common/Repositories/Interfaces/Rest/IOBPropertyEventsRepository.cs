using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;

namespace OB.DL.Common.Repositories.Interfaces.Rest
{
    public interface IOBPropertyEventsRepository : IRestRepository<OB.BL.Contracts.Data.ProactiveActions.ProactiveAction>
    {
        long InsertPropertyQueue(InsertPropertyQueueRequest request);


        long CancelPropertyQueueEvent(CancelPropertyQueueEventRequest request);


        bool PreparePropertyQueueToSendEmail(PreparePropertyQueueRequest request);


        SendmailToNewGuestResponse SendmailToNewGuest(SendmailToNewGuestRequest request);
    }
}
