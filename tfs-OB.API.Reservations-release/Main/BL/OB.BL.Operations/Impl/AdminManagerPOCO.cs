using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.DL.Common.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace OB.BL.Operations.Impl
{
    /// <summary>
    /// BusinessPOCO class that implements the Admin operations.
    /// </summary>
    public class AdminManagerPOCO : BusinessPOCOBase, IAdminManagerPOCO
    {

        /// <summary>
        /// RESTful implementation of the UpdateCache operation.
        /// This operation updates the entries in the internal/distributed cache provider, given a list of cache entry keys or all if RefreshAllEntries is true.
        /// </summary>
        /// <param name="request">A UpdateCacheRequest object containing the cache entry keys to update and/or the flag to update the cache immediatelly</param>
        /// <returns>A ListLanguageResponse containing the List of matching Language objects</returns>
        public UpdateCacheResponse UpdateCache(UpdateCacheRequest request)
        {
            var response = new UpdateCacheResponse();

            try
            {
                Contract.Requires(request != null, "Request object instance is expected");

                response.RequestGuid = request.RequestGuid;

                var cacheProvider = this.Resolve<ICacheProvider>();

                List<string> allKeys = new List<string>();

                if (!request.RefreshAllEntries.GetValueOrDefault()
                    &&
                    (request.CacheEntryKeys != null && request.CacheEntryKeys.Count > 0))
                {
                    allKeys = request.CacheEntryKeys;
                }
                else allKeys = cacheProvider.GetAllKeys();

                var forceUpdate = request.ForceUpdate.GetValueOrDefault();
                foreach (var key in allKeys)
                {
                    cacheProvider.Invalidate(key, forceUpdate);
                }
                if (!forceUpdate)
                {
                    response.Warnings.Add(new Warning("The cache entries were invalidated, but their values are only updated after the cache refresh algorithm executes"));
                }
                response.UpdatedCacheEntries = allKeys;
                response.NumberOfUpdatedCacheEntries = allKeys.Count;
                response.Succeed();
            }
            catch (Exception ex)
            {
                response.Failed();
                response.Errors.Add(new Reservation.BL.Contracts.Responses.Error(ex));
            }
            return response;

        }

    }
}
