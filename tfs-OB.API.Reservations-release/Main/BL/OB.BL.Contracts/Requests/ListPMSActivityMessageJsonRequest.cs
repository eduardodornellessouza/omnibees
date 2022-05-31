using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Base class for Requests that use Paging.
    /// </summary>
    [DataContract]
    public class ListPMSActivityMessageJsonRequest : ListPagedRequest
    {
        /// <summary>
        /// Gets or sets the list of Primary Keys to search for
        /// </summary>
        [DataMember]
        public List<Guid> Uids { get; set; }
    }
}