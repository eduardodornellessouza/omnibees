using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Base class for Requests that use Paging, filtering and sorting.
    /// </summary>
    [DataContract]
    public class GridPagedRequest : RequestBase
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
        /// List with Order By
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<RequestOrderBase> Orders { get; set; }

        /// <summary>
        /// List with Filter By
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<RequestFilterBase> Filters { get; set; }

        /// <summary>
        /// Filter with nested filters.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public NestedFilterRequestBase NestedFilters { get; set; }
    }
}