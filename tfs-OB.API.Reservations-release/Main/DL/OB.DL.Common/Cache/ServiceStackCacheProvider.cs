using OB.DL.Common.Infrastructure;
using OB.Log;
using ServiceStack.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace OB.DL.Common.Cache
{
    /// <summary>
    /// CacheProvider that makes use of the Redis server distributed cache and NoSQL storage.
    /// </summary>
    public class ServiceStackCacheProvider : ICacheProvider, IDisposable
    {
        private static readonly ConcurrentDictionary<string, CacheEntry> _cacheEntryRegistry = new ConcurrentDictionary<string, CacheEntry>();

        private static ILogger _logger = LogsManager.CreateLogger(typeof(ICacheProvider));
        private static PooledRedisClientManager _clientManager;

        private IEnumerable<string> readWriteHosts;
        private IEnumerable<string> readHosts;
        private IEnumerable<string> failoverReadWriteHosts;
        private IEnumerable<string> failoverReadHosts;

        /// <summary>
        /// Function responsible for updating the values in the cache. It runs every 15 seconds.
        /// </summary>
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
                            _logger.Error(e, "ServiceStackCacheProvider, error in Background UpdateCache Task.");
                        }
                    }
                    return null;
                })
                .Where(e => e != null)
                .ToList();
            return updatedEntries;
        }).StartPeriodic(TimeSpan.Zero, TimeSpan.FromSeconds(15));

        public ServiceStackCacheProvider()
        {
            var readWriteHostsSetting = ConfigurationManager.AppSettings["OB.DL.Common.Cache.ServiceStackCacheProvider.ReadWriteHosts"];
            var failoverReadWriteHostsSetting = ConfigurationManager.AppSettings["OB.DL.Common.Cache.ServiceStackCacheProvider.FailoverReadWriteHosts"];
            var readHostsSetting = ConfigurationManager.AppSettings["OB.DL.Common.Cache.ServiceStackCacheProvider.ReadHosts"];
            var failoverReadHostsSetting = ConfigurationManager.AppSettings["OB.DL.Common.Cache.ServiceStackCacheProvider.FailoverReadHosts"];

            if (string.IsNullOrEmpty(readWriteHostsSetting))
            {
                _clientManager = new PooledRedisClientManager("127.0.0.1:6379");
            }
            else
            {
                readWriteHosts = readWriteHostsSetting.Split(',');
                readHosts = string.IsNullOrEmpty(readHostsSetting) ? Enumerable.Empty<string>() : readHostsSetting.Split(',');
                failoverReadWriteHosts = string.IsNullOrEmpty(failoverReadWriteHostsSetting) ? Enumerable.Empty<string>() : failoverReadWriteHostsSetting.Split(',');
                failoverReadHosts = string.IsNullOrEmpty(failoverReadHostsSetting) ? Enumerable.Empty<string>() : failoverReadHostsSetting.Split(',');

                if (readHosts.Count() == 0)
                    readHosts = readWriteHosts;

                _clientManager = new PooledRedisClientManager(readWriteHosts.AsEnumerable(), readHosts.AsEnumerable(),
                    new RedisClientManagerConfig()
                    {
                        MaxReadPoolSize = 50,
                        MaxWritePoolSize = 50
                    });

                //if (failoverReadWriteHosts.Count() > 0 && failoverReadHostsSplitted.Count() == 0)
                //    _clientManager.FailoverTo(failoverReadWriteHosts);
                //else if (failoverReadHostsSplitted.Count() > 0)
                //    _clientManager.FailoverTo(failoverReadWriteHostsSplitted, failoverReadHostsSplitted);

                //_clientManager.FailoverTo(failoverReadWriteHostsSplitted, failoverReadHostsSplitted);

                //_clientManager.OnFailover.Add((manager) =>
                //{
                //    int i = 0;

                //});
            }
        }

        public void Set(string key, CacheEntry entry)
        {
            if (_cacheEntryRegistry.Keys.Contains(key))
                return;

            _cacheEntryRegistry.AddOrUpdate(key, entry, (k, existingentry) =>
            {
                return entry;
            });

            Update(entry);
        }

        private static void Update(CacheEntry entry)
        {
            //rough way of getting the used memory...
            long memBefore = GC.GetTotalMemory(true);
            var data = entry.GetDataCallback.Invoke();
            int size = (int)(GC.GetTotalMemory(true) - memBefore);

            using (var redisClient = _clientManager.GetClient())
            {
                redisClient.Set(entry.CacheKey, data);
            }

            if (size < 0)
                size = 0;
            _logger.Info("CacheProvider: Added/Updated \"" + entry.CacheKey + "\"  size: " + size + " bytes");

            entry.LastUpdated = DateTime.Now;
        }

        //private IRedisClient GetFailoverClient()
        //{
        //}

        public T Get<T>(string key) where T : class
        {
            IRedisClient redisClient = null;
            try
            {
                using (redisClient = _clientManager.GetClient())
                {
                    //if (!(redisClient as RedisNativeClient).HadExceptions)
                    return redisClient.Get<T>(key);
                }
            }
            catch (RedisException)
            {
                if (failoverReadWriteHosts.Count() > 0 && failoverReadHosts.Count() == 0)
                    _clientManager.FailoverTo(failoverReadWriteHosts, failoverReadWriteHosts);
                else if (failoverReadHosts.Count() > 0)
                    _clientManager.FailoverTo(failoverReadWriteHosts, failoverReadHosts);

                using (redisClient = _clientManager.GetClient())
                {
                    //if (!(redisClient as RedisNativeClient).HadExceptions)
                    return redisClient.Get<T>(key);
                }

                ////try with a Read/Write client instead
                //var client = _clientManager.GetClient();

                //var r = client.Get<T>(key);
                //_clientManager.DisposeClient(client as RedisNativeClient);
                //return r;
            }
            finally
            {
                //redisClient.Dispose();
            }
        }

        public void Invalidate(string key, bool forceUpdate = false)
        {
            var cacheEntry = _cacheEntryRegistry[key];
            if (cacheEntry != null)
            {
                cacheEntry.LastUpdated = DateTime.MinValue;

                if (forceUpdate)
                    Update(cacheEntry);
            }
        }

        /// <summary>
        /// Distributed Locks. It's preferable to use the other one with TimeSpan always.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IDisposable AcquireLock(string key)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.AcquireLock(key);
            }
        }

        /// <summary>
        /// Distributed Locks for a specific timespan period.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IDisposable AcquireLock(string key, TimeSpan timeOut)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.AcquireLock(key, timeOut);
            }
        }

        public bool Add<T>(string key, T value)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Add(key, value);
            }
        }

        public bool Add<T>(string key, T value, DateTime expiresAt)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Add(key, value, expiresAt);
            }
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Add(key, value, expiresIn);
            }
        }

        public long Decrement(string key, uint amount)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Decrement(key, amount);
            }
        }

        public void FlushAll()
        {
            using (var redisClient = _clientManager.GetClient())
            {
                redisClient.FlushAll();
            }
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            using (var redisClient = _clientManager.GetReadOnlyClient())
            {
                return redisClient.GetAll<T>(keys);
            }
        }

        public List<string> GetAllKeys()
        {
            using (var redisClient = _clientManager.GetReadOnlyClient())
            {
                return redisClient.GetAllKeys();
            }
        }

        public long Increment(string key, uint amount)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Increment(key, amount);
            }
        }

        public bool Remove(string key)
        {
            CacheEntry cacheEntry = null;
            _cacheEntryRegistry.TryRemove(key, out cacheEntry);

            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Remove(key);
            }
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                foreach (var key in keys)
                {
                    CacheEntry cacheEntry = null;
                    _cacheEntryRegistry.TryRemove(key, out cacheEntry);
                }
                redisClient.RemoveAll(keys);
            }
        }

        public bool Replace<T>(string key, T value)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Replace<T>(key, value);
            }
        }

        public bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Replace<T>(key, value, expiresAt);
            }
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Replace<T>(key, value, expiresIn);
            }
        }

        public bool Set<T>(string key, T value)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Set<T>(key, value);
            }
        }

        public bool Set<T>(string key, T value, DateTime expiresAt)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Set<T>(key, value, expiresAt);
            }
        }

        public bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                return redisClient.Set<T>(key, value, expiresIn);
            }
        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            using (var redisClient = _clientManager.GetClient())
            {
                redisClient.SetAll<T>(values);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _clientManager.Dispose();
                }
                disposed = true;
            }
        }
    }
}
