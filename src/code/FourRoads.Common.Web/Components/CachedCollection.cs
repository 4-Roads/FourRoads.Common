// // //------------------------------------------------------------------------------
// // // <copyright company="4 Roads Ltd">
// // //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // // </copyright>
// // //------------------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Linq;
using FourRoads.Common.Interfaces;

#endregion

namespace FourRoads.Common
{
    [Serializable]
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

            CacheRefreshInterval = 30;
            CacheTags = new string[0];
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

        protected CacheableDictionary<string, ResultData<string>> GetCachedQueries()
        {
            return CacheProvider.Get<CacheableDictionary<string, ResultData<string>>>(DerrivedTypeName + ":Queries") ?? new CacheableDictionary<string, ResultData<string>>(CacheRefreshInterval, CacheTags,CacheScope);
        }

        protected void SetCachedQueries(CacheableDictionary<string, ResultData<string>> cachedQueries)
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

            ResultData<string> resultInfo = null;
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

        private bool CheckHeuristicInRange(ResultData<string> resultInfo)
        {
            if (resultInfo == null)
                return false;

            //IF there are no records easy 
            if (resultInfo.TotalRecords == 0)
                return true;

            bool returnResult;

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

            returnResult = percentage > ReQueryHueristicMarginPercentage;

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

            var replacementCollection = new List<TContainerType>();

            foreach (var item in results.Items)
            {
                keys.Add(item.CacheID);
                replacementCollection.Add(item);

                //The cache has the latest non contextual version
                if (CacheProvider.Get<TContainerType>(item.CacheID) == null)
                {
#if _TRACING
                    System.Diagnostics.Debug.WriteLine("From Cache");
#endif
                    AddItemToCacheProvider(item);
                }
            }

            var resultInfo = new ResultData<string>(keys.ToArray(), query.PageIndex, query.PageSize, results.TotalRecords);

            _lock.EnterWriteLock();
            try
            {
                var cachedQueries = GetCachedQueries();

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
        protected sealed class CacheableDictionary<k, v> : Dictionary<k, v>, ICacheable where k : class
        {
            public CacheableDictionary(int cacheRefreshInterval, string[] cacheTags, CacheScopeOption cacheScope)
            {
                CacheRefreshInterval = cacheRefreshInterval;
                CacheTags = cacheTags;
                CacheScope = cacheScope;
            }

            public string CacheID => string.Join("-", Array.ConvertAll<object, string>(Keys.ToArray(), Convert.ToString));

            public int CacheRefreshInterval { get; }

            public string[] CacheTags { get; }

            public CacheScopeOption CacheScope { get; }
        }

        [Serializable]
        public sealed class ResultData<TKey>
        {
            public ResultData()
            {
            }

            public ResultData(TKey[] keys, uint pageIndex, int pageSize, int totalRecords)
            {
                PageIndex = pageIndex;
                PageSize = pageSize;
                Keys = keys;
                TotalRecords = totalRecords;
            }

            public uint PageIndex { get; }
            public int PageSize { get; }
            public TKey[] Keys { get; }
            public int TotalRecords { get; }
        }

        #endregion
    }
}