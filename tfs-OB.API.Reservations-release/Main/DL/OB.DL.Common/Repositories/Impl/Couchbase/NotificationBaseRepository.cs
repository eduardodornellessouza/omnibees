using Couchbase.Core;
using OB.DL.Common.Infrastructure.Impl;
using OB.DL.Common.Repositories.Interfaces.Couchbase;

namespace OB.DL.Common.Repositories.Impl.Couchbase
{
    class NotificationBaseRepository : CouchbaseRepository<OB.Domain.Reservations.NotificationBase>, INotificationBaseRepository
    {
        public NotificationBaseRepository(IBucket bucket)
            : base(bucket)
        {
            
        }
    }
}
