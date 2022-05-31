using OB.DL.Common.Impl;
using System.Data;
using OB.DL.Common.Repositories.Interfaces.SqlServer;
using OB.DL.Common.Criteria;
using OB.DL.Common.QueryResultObjects.TPIs;
using System.Collections.Generic;
using Dapper;
using System.Linq;

namespace OB.DL.Common.Repositories.Impl.SqlServer
{
    /// <summary>
    /// SQL Repository to do queries about TPIs. Uses Omnibees connection string.
    /// </summary>
    /// <seealso cref="OB.DL.Common.Impl.SqlRepository" />
    /// <seealso cref="OB.DL.Common.Repositories.Interfaces.SqlServer.IThirdPartyIntermediarySqlRepository" />
    internal class ThirdPartyIntermediarySqlRepository : SqlRepository, IThirdPartyIntermediarySqlRepository
    {
        public ThirdPartyIntermediarySqlRepository(IDbConnection context)
            : base(context)
        {
        }

        /// <summary>
        /// Increments or decrements the TPIProperty Credit Used or PrePaid Credit Used.
        /// </summary>
        /// <param name="criteria">The criteria to update TPIProperty Credit.
        /// Method will decrements if <see cref="UpdateCreditCriteria.IncrementValue" /> is negative otherwise increments.
        /// Remark: <see cref="UpdateCreditCriteria.ChannelUid" /> is not used on this method.</param>
        /// <returns>
        /// The value of CreditUsed and PrePaidCreditUsed after update.
        /// </returns>
        public UpdateTpiPropertyCreditQR1 UpdateTpiPropertyCredit(UpdateCreditCriteria criteria)
        {
            if (criteria == null || (!criteria.UpdateCreditUsed && !criteria.UpdatePrePaidCreditUsed))
                return new UpdateTpiPropertyCreditQR1();

            string valueStr = criteria.IncrementValue.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);

            // Set columns to update
            var updateColumns = new List<string>();
            if (criteria.UpdateCreditUsed)
                updateColumns.Add($"CreditUsed = ISNULL(CreditUsed,0) + { valueStr }");
            if (criteria.UpdatePrePaidCreditUsed)
                updateColumns.Add($"PrePaidCreditUsed = ISNULL(PrePaidCreditUsed,0) + { valueStr }");

            // Build query
            string query = $@"UPDATE TPIProperties
                              SET { string.Join(", ", updateColumns) }
                              OUTPUT inserted.UID AS TpiPropertyUId, inserted.CreditUsed, inserted.PrePaidCreditUsed
                              WHERE Property_UID = { criteria.PropertyUid } AND TPI_UID = { criteria.TpiUid }";

            return this._context.Query<UpdateTpiPropertyCreditQR1>(query).FirstOrDefault();
        }
    }
}