using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;

namespace OB.BL.Operations
{
    /// <summary>
    /// BusinessPOCO interface that exposes Admin operations.
    /// </summary>
    public interface IAdminManagerPOCO : IBusinessPOCOBase
    {
        /// <summary>
        /// RESTful implementation of the UpdateCache operation.
        /// This operation updates the entries in the internal/distributed cache provider, given a list of cache entry keys or all if RefreshAllEntries is true.
        /// </summary>
        /// <param name="request">A UpdateCacheRequest object containing the cache entry keys to update and/or the flag to update the cache immediatelly</param>
        /// <returns>A ListLanguageResponse containing the List of matching Language objects</returns>
        UpdateCacheResponse UpdateCache(UpdateCacheRequest request);
    }
}
