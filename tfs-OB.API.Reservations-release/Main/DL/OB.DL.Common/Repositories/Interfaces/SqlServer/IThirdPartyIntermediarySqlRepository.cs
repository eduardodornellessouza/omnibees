using OB.DL.Common.Criteria;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.QueryResultObjects.TPIs;
using System.Data;

namespace OB.DL.Common.Repositories.Interfaces.SqlServer
{
    /// <summary>
    /// SQL Repository to do queries about TPIs. Uses Omnibees connection string.
    /// </summary>
    /// <seealso cref="OB.DL.Common.Infrastructure.ISqlRepository" />
    public interface IThirdPartyIntermediarySqlRepository : ISqlRepository
    {
        /// <summary>
        /// Increments or decrements the TPIProperty Credit Used or PrePaid Credit Used.
        /// </summary>
        /// <param name="criteria">The criteria to update TPIProperty Credit.
        /// Method will decrements if <see cref="UpdateCreditCriteria.IncrementValue" /> is negative otherwise increments.
        /// Remark: <see cref="UpdateCreditCriteria.ChannelUid" /> is not used on this method.</param>
        /// <returns>
        /// The value of CreditUsed and PrePaidCreditUsed after update.
        /// </returns>
        UpdateTpiPropertyCreditQR1 UpdateTpiPropertyCredit(UpdateCreditCriteria criteria);
    }
}
