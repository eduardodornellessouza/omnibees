using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    public class RequestOrderBase
    {
        /// <summary>
        /// Order descriptor. ascending or descending
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Object property name to order by
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string OrderBy { get; set; }

        /// <summary>
        /// Start ordering by this
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool Initial { get; set; }
    }

    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1
    }
}
