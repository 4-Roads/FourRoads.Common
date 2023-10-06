// // //------------------------------------------------------------------------------
// // // <copyright company="4 Roads Ltd">
// // //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // // </copyright>
// // //------------------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FourRoads.Common.Interfaces;

#endregion

namespace FourRoads.Common
{
    public class CachedCollection<TContainerType, TQueryType> :
        SimpleCachedCollection<TContainerType>
        where TQueryType : class, IPagedQueryV2
        where TContainerType : class, ICacheable
    {
        #region Delegates

        public delegate IPagedCollection<TContainerType> RefreshQuery(TQueryType query);

        #endregion

        private RefreshQuery _getDataQuery; //Delegate to retrieve a single item
        private readonly IPagedCollectionFactory _pagedCollectionFactory;

        public CachedCollection(IPagedCollectionFactory pagedCollectionFactory, ICache cacheProvider) : base(cacheProvider)
        {
            _pagedCollectionFactory = pagedCollectionFactory;

            CacheRefreshInterval = 120;
            CacheTags = Array.Empty<string>();
            CacheScope = CacheScopeOption.All;
        }

        protected RefreshQuery GetDataQuery
        {
            set { _getDataQuery = value; }
        }

        protected int CacheRefreshInterval { get; set; }
        protected string[] CacheTags { get; set; }
        protected CacheScopeOption CacheScope { get; set; }

        /// <summary>
        ///     This property controls the limit as to how many items missing relative to page size requested before a full
        ///     re-query is performed
        /// </summary>
        public short ReQueryHueristicMarginPercentage { get; set; } = 80;

        protected CacheableDictionary GetCachedQueries()
        {
            return CacheProvider.Get(DerrivedTypeName + ":Queries") as CacheableDictionary ?? new CacheableDictionary(CacheRefreshInterval, CacheTags,CacheScope);
        }

        protected void SetCachedQueries(CacheableDictionary cachedQueries)
        {
            CacheProvider.Insert(DerrivedTypeName + ":Queries", cachedQueries);
        }

        public IPagedCollection<TOverrideContainerType> Get<TOverrideContainerType>(TQueryType query)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("Get:{0}", query.CacheKey));

            if (query == null)
            {
                throw new ArgumentNullException("query is null");
            }

            if (!query.UseCache)
            {
                var noCacheResults = _getDataQuery(query);

                return _pagedCollectionFactory.CreatedPagedCollection(query.PageIndex, query.PageSize, noCacheResults.Items.Cast<TOverrideContainerType>(), noCacheResults.TotalRecords);
            }

            IPagedCollection<TOverrideContainerType> results = null;

            ResultData resultInfo = null;
            var hasResultsList = false;
            var cacheKey = query.CacheKey;

            _lock.EnterReadLock();
            try
            {
                var cachedQueries = GetCachedQueries();

                hasResultsList = cachedQueries.ContainsKey(cacheKey) && cachedQueries.TryGetValue(cacheKey, out resultInfo);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (!hasResultsList)
            {
                //We have no cached data so we need to do a full query
                results = GetQueryResults<TOverrideContainerType>(query);
            }

            //At this point we have result infor, but we need to determine if it's more efficient to do a full query to the 
            //database rather than many single item queries
            if (CheckHeuristicInRange(resultInfo))
            {
                var items = new List<TContainerType>();

                for (var i = 0; i < resultInfo.Keys.Length; i++)
                {
                    var singleItem = Get(resultInfo.Keys[i]);

                    if (singleItem != null)
                    {
                        items.Add(singleItem);
                    }
                }

                results = _pagedCollectionFactory.CreatedPagedCollection(resultInfo.PageIndex, resultInfo.PageSize, items.Cast<TOverrideContainerType>(), resultInfo.TotalRecords);
            }


            if (results == null)
                return GetQueryResults<TOverrideContainerType>(query);

            return results;
        }

        private bool CheckHeuristicInRange(ResultData resultInfo)
        {
            if (resultInfo == null)
                return false;

            //IF there are no records easy 
            if (resultInfo.TotalRecords == 0)
                return true;

            var target = Math.Min(resultInfo.TotalRecords, resultInfo.PageSize);
            var actual = 0;
            var keyLength = resultInfo.Keys.Length;
            for (var i = 0; i < keyLength; i++)
            {
                var result = CacheProvider.Get<TContainerType>(resultInfo.Keys[i]);

                if (result != null)
                {
                    actual++;
                }

                if (i > 10000)
                    break;
            }

            double percentage = 0;

            if (actual > 0 && target > 0)
            {
                percentage = actual/target*100;
            }

            var returnResult = percentage > ReQueryHueristicMarginPercentage;

#if _TRACING
            System.Diagnostics.Debug.WriteLine("HueristicMarginPercentage:" +returnResult.ToString());
#endif
            return returnResult;
        }

        public override void Add(TContainerType obj)
        {
            _lock.EnterWriteLock();
            try
            {
                ClearQueries();
                base.Add(obj);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Remove(TContainerType obj)
        {
            _lock.EnterWriteLock();
            try
            {
                base.Remove(obj);
                ClearQueries();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        protected IPagedCollection<TOverrideContainerType> GetQueryResults<TOverrideContainerType>(TQueryType query)
        {
            IPagedCollection<TOverrideContainerType> overrideResults = null;

            var keys = new List<string>();
            IPagedCollection<TContainerType> results = null;
            var cacheKey = query.CacheKey;

            results = _getDataQuery(query);

            foreach (var item in results.Items)
            {
                keys.Add(item.CacheID);

                //The cache has the latest non contextual version
                if (CacheProvider.Get<TContainerType>(item.CacheID) == null)
                {
#if _TRACING
                    System.Diagnostics.Debug.WriteLine("From Cache");
#endif
                    AddItemToCacheProvider(item);
                }
            }

            var resultInfo = new ResultData(keys.ToArray(), query.PageIndex, query.PageSize, results.TotalRecords);

            _lock.EnterWriteLock();
            try
            {
                var cachedQueries = GetCachedQueries();

                //Clear the cached queries, to keep the inprocess cache in sync
                CacheProvider.Remove(DerrivedTypeName + ":Queries");

                if (cachedQueries.ContainsKey(cacheKey))
                {
                    cachedQueries[cacheKey] = resultInfo;
                }
                else
                {
                    cachedQueries.Add(cacheKey, resultInfo);
                }

                SetCachedQueries(cachedQueries);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            overrideResults = _pagedCollectionFactory.CreatedPagedCollection(query.PageIndex, query.PageSize, results.Items.Cast<TOverrideContainerType>(), results.TotalRecords);

            return overrideResults;
        }

        public IPagedCollection<TContainerType> Get(TQueryType query)
        {
            return Get<TContainerType>(query);
        }

        public void ClearQueries()
        {
            _lock.EnterWriteLock();
            try
            {
                //Clear all of the queries
                var cachedQueries = GetCachedQueries();

                CacheProvider.Remove(DerrivedTypeName + ":Queries");

                cachedQueries.Clear();

                SetCachedQueries(cachedQueries);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                base.Clear();
                ClearQueries();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        #region Nested type: ResultData

        [Serializable]
        public sealed class CacheableDictionary : Dictionary<string, ResultData>, ICacheable
        {
            protected CacheableDictionary(SerializationInfo information, StreamingContext context)
                : base(information, context)
            {
            }

            public CacheableDictionary()
            {
                CacheRefreshInterval = 120;
                CacheScope = CacheScopeOption.All;
            }

            public CacheableDictionary(int cacheRefreshInterval, string[] cacheTags, CacheScopeOption cacheScope)
            {
                CacheRefreshInterval = cacheRefreshInterval;
                CacheTags = cacheTags;
                CacheScope = cacheScope;
            }

            public string CacheID => string.Join("-", Keys.ToArray());

            public int CacheRefreshInterval { get; }

            public string[] CacheTags { get; }

            public CacheScopeOption CacheScope { get; }
        }

        [Serializable]
        public sealed class ResultData
        {
            public ResultData()
            {
            }

            public ResultData(string[] keys, uint pageIndex, int pageSize, int totalRecords)
            {
                PageIndex = pageIndex;
                PageSize = pageSize;
                Keys = keys;
                TotalRecords = totalRecords;
            }

            public uint PageIndex { get; }
            public int PageSize { get; }
            public string[] Keys { get; }
            public int TotalRecords { get; }
        }

        #endregion
    }
}