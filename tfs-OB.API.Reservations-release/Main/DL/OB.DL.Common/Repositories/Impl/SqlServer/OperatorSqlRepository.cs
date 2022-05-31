using OB.DL.Common.Criteria;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.SqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Globalization;

namespace OB.DL.Common.Repositories.Impl.SqlServer
{

    /// <summary>
    /// SQL Repository to do queries about Operators. Uses Omnibees connection string.
    /// </summary>
    /// <seealso cref="OB.DL.Common.Impl.SqlRepository" />
    /// <seealso cref="OB.DL.Common.Repositories.Interfaces.SqlServer.IOperatorSqlRepository" />
    internal class OperatorSqlRepository : SqlRepository, IOperatorSqlRepository
    {
        public OperatorSqlRepository(IDbConnection context)
            : base(context)
        {

        }

        /// <summary>
        /// Updates the operator Credit Used or PrePaid Credit Used.
        /// </summary>
        /// <param name="criteria">The criteria to update operator. The property TpiUid will never filled in this context.</param>
        /// <returns>
        /// </returns>
        public void UpdateOperatorCredit(UpdateCreditCriteria criteria)
        {
            if (criteria != null && (criteria.UpdateCreditUsed || criteria.UpdatePrePaidCreditUsed))
            {
                string valueStr = criteria.IncrementValue.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);

                // Set columns to update
                var updateColumns = new List<string>();
                if (criteria.UpdateCreditUsed)
                    updateColumns.Add($"OperatorCreditUsed = { valueStr }");
                if (criteria.UpdatePrePaidCreditUsed)
                    updateColumns.Add($"PrePaymentCreditUsed = { valueStr }");

                // Build query
                string query = $@"UPDATE ChannelsProperties
                              SET { string.Join(", ", updateColumns) }
                              WHERE Property_UID = { criteria.PropertyUid } AND Channel_UID = { criteria.ChannelUid }";

                this._context.Query(query);
            }
        }
    }
}
