using System;
using System.Web;
using System.Web.Caching;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Web.Tests
{
    public class MockCache : ICache
    {
        private readonly Cache _cache = HttpRuntime.Cache;

        public void Insert(ICacheable value)
        {
            Insert(value.CacheID, value, null, value.CacheRefreshInterval);
        }

        public void Insert(ICacheable value, string[] additionalTags)
        {
            Insert(value.CacheID, value, null, value.CacheRefreshInterval);
        }

        public T Get<T>(string key)
        {
            return (T) _cache[key];
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
            throw new NotImplementedException();
        }

        public void Insert(string key, object value, CacheDependency dependencies, int absoluteExpiration,
            CacheItemPriority priority)
        {
            _cache.Insert(key, value, dependencies, DateTime.Now + new TimeSpan(0, 0, 0, absoluteExpiration), TimeSpan.Zero,
                priority, null);
        }

        public void Insert(string key, object value, CacheDependency dependencies, int absoluteExpiration)
        {
            _cache.Insert(key, value, dependencies, DateTime.Now + new TimeSpan(0, 0, 0, absoluteExpiration), TimeSpan.Zero,
                CacheItemPriority.Normal, null);
        }

        #region ICache Members

        public void Insert(string key, object value)
        {
            _cache.Insert(key, value);
        }

        public void Insert(string key, object value, string[] tags)
        {
            _cache.Insert(key, value);
        }

        public void Insert(string key, object value, TimeSpan timeout)
        {
            _cache.Insert(key, value, null, DateTime.Now + timeout, Cache.NoSlidingExpiration);
        }

        public void Insert(string key, object value, string[] tags, TimeSpan timeout)
        {
            _cache.Insert(key, value, null, DateTime.Now + timeout, Cache.NoSlidingExpiration);
        }

        public void Insert(string key, object value, string[] tags, TimeSpan timeout, CacheScopeOption scope)
        {
            _cache.Insert(key, value, null, DateTime.Now + timeout, Cache.NoSlidingExpiration);
        }

        public object Get(string key)
        {
            return _cache.Get(key);
        }

        public void RemoveByTags(string[] tags)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}