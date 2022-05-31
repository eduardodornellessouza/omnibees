using System;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.ProactiveActions
{
    [DataContract]
    public class PropertyEvent : ContractBase
    {
        public PropertyEvent()
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long SystemEvent_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long SystemAction_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<long> PropertyTemplate_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string OtherEmails { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Subject { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Link { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long Property_UID { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsDelete { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsGuest { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsTravelAgent { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<bool> IsCorporate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Nullable<short> ReservationOption { get; set; }
    }
}