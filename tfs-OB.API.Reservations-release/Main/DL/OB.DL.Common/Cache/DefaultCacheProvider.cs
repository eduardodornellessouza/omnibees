using OB.DL.Common.Infrastructure;
using OB.Log;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace OB.DL.Common.Cache
{
    public class DefaultCacheProvider : ICacheProvider
    {
        private static readonly ObjectCache _cache = MemoryCache.Default;
        private static readonly ConcurrentDictionary<string, CacheEntry> _cacheEntryRegistry = new ConcurrentDictionary<string, CacheEntry>();
        private static bool _usePeriodicCacheUpdate = false;
        private static readonly ConcurrentDictionary<string, string> _concurrentLocks = new ConcurrentDictionary<string, string>();
        private static ILogger _logger = LogsManager.CreateLogger(typeof(ICacheProvider));

        private static IDisposable updateCacheDisposable = new Func<IEnumerable<CacheEntry>>(() =>
        {
            var registryValues = _cacheEntryRegistry.Values.ToList();
            List<CacheEntry> updatedEntries = registryValues
                .AsParallel()
                .Select(entry =>
                {
                    if ((DateTime.Now - entry.LastUpdated).TotalSeconds >= entry.UpdateInterval.TotalSeconds)
                    {
                        try
                        {
                            Update(entry);
                            return entry;
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "DefaultCacheProvider, error in Background UpdateCache Task. Key: {0}", entry.CacheKey);
                        }
                    }
                    return null;
                })
                .Where(e => e != null)
                .ToList();

            return updatedEntries;
        }).StartPeriodic(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(15));

        public DefaultCacheProvider()
        {
        }

        private ObjectCache Cache { get { return _cache; } }

        public void Set(string key, CacheEntry entry)
        {
            if (_cacheEntryRegistry.ContainsKey(key))
                return;

            Update(entry, true);

            _cacheEntryRegistry.AddOrUpdate(key, entry, (k, existingentry) =>
            {
                return entry;
            });

            _logger.Debug("Added Cache with key {0}", key);
        }

        private static void Update(CacheEntry entry, bool added = false)
        {
            //rough way of getting the used memory...
            long memBefore = GC.GetTotalMemory(true);
            var data = entry.GetDataCallback.Invoke();
            int size = (int)(GC.GetTotalMemory(true) - memBefore);

            if (_usePeriodicCacheUpdate)
            {
                var cacheItem = new CacheItem(entry.CacheKey, data);

                var callback = new CacheEntryUpdateCallback(entry.UpdateCallback);

                var cacheItemPolicy = new CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.Add(entry.UpdateInterval),
                    UpdateCallback = callback,
                };

                _cache.Set(cacheItem, cacheItemPolicy);
            }
            else
            {
                _cache.Set(entry.CacheKey, data, DateTimeOffset.MaxValue);
            }

            if (size < 0)
                size = 0;

            int dataCount = -1;
            if (data is ICollection)
                dataCount = ((ICollection)data).Count;
            else if (data is IEnumerable<object>)
                dataCount = ((IEnumerable<object>)data).Count();

            _logger.Debug("CacheProvider: " + (added ? "Added" : "Updated") + " \"" + entry.CacheKey + "\"  size: " + size + " bytes " + (dataCount > -1 ? " #listItems:" + dataCount : ""));

            entry.LastUpdated = DateTime.Now;
        }

        public T Get<T>(string key) where T : class
        {
            return _cache.Get(key) as T;
        }

        public void Invalidate(string key, bool forceUpdate = false)
        {
            var cacheEntry = _cacheEntryRegistry[key];

            if (cacheEntry != null)
            {
                cacheEntry.LastUpdated = DateTime.MinValue;
                if (forceUpdate)
                    Update(cacheEntry);
                //_cacheEntryRegistry.TryRemove(key, out cacheEntry);
                //this.Set(key, cacheEntry);
            }
        }

        public IDisposable AcquireLock(string key)
        {
            //TODO: Use database lock table
            throw new NotImplementedException();
        }

        public IDisposable AcquireLock(string key, TimeSpan timeOut)
        {
            //TODO: Use database lock table
            throw new NotImplementedException();
        }

        public bool Add<T>(string key, T value, DateTime expiresAt)
        {
            return _cache.Add(key, value, expiresAt);
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            return _cache.Add(key, value, DateTime.Now.AddTicks(expiresIn.Ticks));
        }

        public long Decrement(string key, uint amount)
        {
            //TODO: Use database lock table
            throw new NotImplementedException();
        }

        public void FlushAll()
        {
            var cacheEntries = _cacheEntryRegistry.ToList();
            foreach (var cacheEntryPair in cacheEntries)
            {
                cacheEntryPair.Value.LastUpdated = DateTime.MinValue;
            }
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            throw new NotImplementedException();
        }

        public List<string> GetAllKeys()
        {
            return _cacheEntryRegistry.Keys.ToList();
        }

        public long Increment(string key, uint amount)
        {
            //TODO: Use database lock table
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            CacheEntry cacheEntry = null;
            _cacheEntryRegistry.TryRemove(key, out cacheEntry);
            return _cache.Remove(key) != null;
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                CacheEntry cacheEntry = null;
                _cacheEntryRegistry.TryRemove(key, out cacheEntry);
                _cache.Remove(key);
            }
        }

        public bool Replace<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            throw new NotImplementedException();
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            throw new NotImplementedException();
        }

        public bool Set<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public bool Set<T>(string key, T value, DateTime expiresAt)
        {
            throw new NotImplementedException();
        }

        public bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            throw new NotImplementedException();
        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            throw new NotImplementedException();
        }
    }
}
