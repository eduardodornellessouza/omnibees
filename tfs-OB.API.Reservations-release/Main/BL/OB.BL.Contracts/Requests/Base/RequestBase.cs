using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    [KnownType(typeof(InsertReservationRequest))]
    [KnownType(typeof(UpdateReservationRequest))]
    [KnownType(typeof(ListReservationRequest))]
    [KnownType(typeof(CancelReservationRequest))]
    [KnownType(typeof(PagedRequestBase))]
    [KnownType(typeof(ListReservationRequest))]
    public class RequestBase : ContractBase
    {
        public RequestBase()
        {
            RequestGuid = Guid.NewGuid();
            RequestId = Guid.NewGuid().ToString();
        }

        [DataMember]
        [Obsolete("Use RequestId instead. This property will be removed on OB version 0.9.49")]
        public virtual Guid RequestGuid { get; set; }

        [DataMember]
        public virtual long? LanguageUID { get; set; }

        [DataMember]
        public virtual string LanguageCode { get; set; }
     
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual bool? IsTransactional { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Nullable<OB.Reservation.BL.Constants.RuleType> RuleType { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual Reservation.BL.Contracts.Data.Version Version { get; set; }

        /// <summary>
        /// Used to make a correlation between messages
        /// </summary>
        [DataMember]
        public virtual string RequestId { get; set; }

        /// <summary>
        /// Overrides the API Business Rules of the current RuleType if needed.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public virtual BusinessRulesOverrider BusinessRules { get; set; }
    }
}