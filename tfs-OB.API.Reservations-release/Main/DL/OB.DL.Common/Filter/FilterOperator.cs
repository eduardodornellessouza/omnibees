using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Filter
{
    /// <summary>
    /// SQL Comparison Operators (including string logical operators).
    /// </summary>
    public enum FilterOperator
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
        IsNotContainedIn = 11,
        IsNull = 12,
        IsNotNull = 13
    }
}
