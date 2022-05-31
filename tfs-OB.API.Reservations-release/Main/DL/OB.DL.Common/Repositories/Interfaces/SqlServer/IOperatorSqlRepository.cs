using OB.DL.Common.Criteria;
using OB.DL.Common.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Repositories.Interfaces.SqlServer
{
    /// <summary>
    /// SQL Repository to do queries about Operators. Uses Omnibees connection string.
    /// </summary>
    /// <seealso cref="OB.DL.Common.Infrastructure.ISqlRepository" />
    public interface IOperatorSqlRepository : ISqlRepository
    {
        /// <summary>
        /// Updates the operator Credit Used or PrePaid Credit Used.
        /// </summary>
        /// <param name="criteria">The criteria to update operator.</param>
        /// <returns>
        /// </returns>
        void UpdateOperatorCredit(UpdateCreditCriteria criteria);
    }
}
