using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests
{
    [DataContract]
    public class NestedFilterRequestBase
    {
        /// <summary>
        /// Object property name to filter by
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string FilterBy { get; set; }

        /// <summary>
        /// Set true only if the "FilterBy" field is an enumerable.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsEnumerable { get; set; }

        /// <summary>
        /// Filter method
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public FilterDescriptor Descriptor { get; set; }

        /// <summary>
        /// Value used on the filter
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Value { get; set; }

        /// <summary>
        /// Fiter Operator
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Nested Filters.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<NestedFilterRequestBase> Filters { get; set; }
    }
}
