using System;
using System.Runtime.Caching;

namespace OB.DL.Common.Cache
{
    public class CacheEntry
    {
        public string CacheKey { get; set; }

        public Func<object> GetDataCallback { get; set; }

        public TimeSpan UpdateInterval { get; set; }

        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Callback to support ObjectCache UpdateCachePolicy.
        /// Not used in distributed caching.
        /// </summary>
        /// <param name="args"></param>
        public void UpdateCallback(CacheEntryUpdateArguments args)
        {
            var updatedCacheItem = new CacheItem(CacheKey, GetDataCallback());
            args.UpdatedCacheItem = updatedCacheItem;

            args.UpdatedCacheItemPolicy = new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(UpdateInterval),
                UpdateCallback = UpdateCallback
            };

            LastUpdated = DateTime.Now;
        }
    }
}