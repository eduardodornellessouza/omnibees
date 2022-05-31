

using System.Collections.Generic;

namespace OB.BL.Operations.Test.Domain.CRM
{
    public partial class PropertyEvent
    {    
        public long UID { get; set; }
        public long SystemEvent_UID { get; set; }
        public long SystemAction_UID { get; set; }
        public long? PropertyTemplate_UID { get; set; }
        public string OtherEmails { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
        public string Link { get; set; }
        public long Property_UID { get; set; }
        public bool IsDelete { get; set; }
        public bool? IsGuest { get; set; }
        public bool? IsTravelAgent { get; set; }
        public bool? IsCorporate { get; set; }
        public short? ReservationOption { get; set; }
    
        public virtual List<PropertyEventActivity> PropertyEventActivitys { get; set; }
        public virtual List<PropertyEventAttraction> PropertyEventAttractions { get; set; }
        public virtual List<PropertyEventCondition> PropertyEventConditions { get; set; }
        public virtual List<PropertyEventOccupancyAlert> PropertyEventOccupancyAlerts { get; set; }
        public virtual SystemAction SystemAction { get; set; }
        public virtual SystemEvent SystemEvent { get; set; }
        public virtual SystemTemplate SystemTemplate { get; set; }
        public virtual List<PropertyEventSetting> PropertyEventSettings { get; set; }
        public virtual List<PropertyQueue> PropertyQueues { get; set; }
    }
}
