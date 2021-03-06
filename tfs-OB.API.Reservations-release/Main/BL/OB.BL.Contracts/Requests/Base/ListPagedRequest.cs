using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Base class for Requests that use Paging.
    /// </summary>
    [DataContract]
    public class ListPagedRequest : RequestBase
    {
        /// <summary>
        /// Size of the Page (number of records returned).
        /// </summary>
        [DataMember]
        public int PageSize { get; set; }

        /// <summary>
        /// Number of the Page starting at 0.
        /// </summary>
        [DataMember]
        public int PageIndex { get; set; }

        /// <summary>
        /// Gets or Sets the flag to calculate the total number of records for the given criteria.
        /// </summary>
        [DataMember]
        public bool ReturnTotal { get; set; }

        /// <summary>
        /// Gets or sets the list of Primary Keys to search for
        /// </summary>
        [DataMember]
        public List<long> UIDs { get; set; }
    }
}