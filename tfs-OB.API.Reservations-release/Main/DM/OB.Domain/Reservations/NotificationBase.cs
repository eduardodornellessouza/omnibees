using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Domain.Reservations
{
    public class NotificationBase : DomainObject
    {
        public NotificationBase()
        {
            SubActions = new List<Events.Contracts.SubAction>();
        }

        public OB.Events.Contracts.Action Action { get; set; }
        public string ActionName { get; set; }
        public Dictionary<string, List<long>> AssociatedEntities { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; }
        public List<OB.Events.Contracts.EntityDelta> EntityDeltas { get; set; }
        public string NotificationGuid { get; set; }
        public OB.Events.Contracts.Operations Operation { get; set; }
        public Guid? ParentNotificationGuid { get; set; }
        public long PropertyUID { get; set; }
        public List<OB.Events.Contracts.SubAction> SubActions { get; set; }
        public int Version { get; set; }
    }
}
