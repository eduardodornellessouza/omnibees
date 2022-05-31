using OB.DL.Common.Cache;
using System;
using System.Collections.Generic;

namespace OB.DL.Common.Infrastructure
{
    public interface ICacheProvider
    {
        IDisposable AcquireLock(string key);

        IDisposable AcquireLock(string key, TimeSpan timeOut);

        bool Add<T>(string key, T value, DateTime expiresAt);

        bool Add<T>(string key, T value, TimeSpan expiresIn);

        long Decrement(string key, uint amount);

        void FlushAll();

        T Get<T>(string key) where T : class;

        IDictionary<string, T> GetAll<T>(IEnumerable<string> keys);

        List<string> GetAllKeys();

        long Increment(string key, uint amount);

        bool Remove(string key);

        void RemoveAll(IEnumerable<string> keys);

        bool Replace<T>(string key, T value);

        bool Replace<T>(string key, T value, DateTime expiresAt);

        bool Replace<T>(string key, T value, TimeSpan expiresIn);

        bool Set<T>(string key, T value);

        bool Set<T>(string key, T value, DateTime expiresAt);

        bool Set<T>(string key, T value, TimeSpan expiresIn);

        void SetAll<T>(IDictionary<string, T> values);

        void Set(string key, CacheEntry entry);

        void Invalidate(string key, bool forceUpdate = false);
    }
}