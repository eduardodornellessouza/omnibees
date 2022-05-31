using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace OB.Reservation.BL.Contracts.Requests
{
    public class RequestFilterBase
    {
        /// <summary>
        /// Filter method
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public FilterDescriptor Filter { get; set; }
        
        /// <summary>
        /// Object property name to filter by
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string FilterBy { get; set; }

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
    }

    public enum FilterDescriptor
    {
        IsLessThan = 0,
        IsLessThanOrEqualTo = 1,
        IsEqualTo = 2,
        IsNotEqualTo = 3,
        IsGreaterThanOrEqualTo = 4,
        IsGreaterThan = 5,
        StartsWith = 6,
        EndsWith = 7,
        Contains = 8,
        DoesNotContain = 9,
        IsContainedIn = 10,
        IsNotContainedIn = 11
    }

    public enum FilterOperator
    {
        AND = 0,
        OR = 1
    }
}
