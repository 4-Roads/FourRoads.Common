// // //------------------------------------------------------------------------------
// // // <copyright company="Four Roads LLC">
// // //     Copyright (c) Four Roads LLC.  All rights reserved.
// // // </copyright>
// // //------------------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Linq;
using FourRoads.Common.Interfaces;
using Ninject.Parameters;

#endregion

namespace FourRoads.Common
{
    public class CachedCollection<TContainerType, TQueryType, TDerivedType> :
        SimpleCachedCollection<TContainerType, TDerivedType>
        where TQueryType : class, IPagedQueryV2
        where TContainerType : class, ICacheable
        where TDerivedType : class, new()
    {
        #region Delegates

        public delegate IPagedCollection<TContainerType> RefreshQuery(TQueryType query);

        #endregion

        private RefreshQuery _getDataQuery; //Delegate to retrieve a single item
        private short _reQueryHueristicMarginPercentage = 80;
        private IPagedCollectionFactory _pagedCollectionFactory = null;

        protected CachedCollection()
        {
            _pagedCollectionFactory = Injector.Get<IPagedCollectionFactory>();
        }

        protected RefreshQuery GetDataQuery
        {
            set { _getDataQuery = value; }
        }

        /// <summary>
        /// This property controls the limit as to how many items missing relative to page size requested before a full re-query is performed
        /// </summary>
        public short ReQueryHueristicMarginPercentage
        {
            get { return _reQueryHueristicMarginPercentage; }
            set { _reQueryHueristicMarginPercentage = value; }
        }

        protected Dictionary<string, ResultData<string>> GetCachedQueries()
        {
            return CacheProvider.Get<Dictionary<string, ResultData<string>>>(DerrivedTypeName + ":Queries") ?? new Dictionary<string, ResultData<string>>();
        }

        protected void SetCachedQueries(Dictionary<string, ResultData<string>> cachedQueries)
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
                IPagedCollection<TContainerType> noCacheResults = _getDataQuery(query);

                return _pagedCollectionFactory.CreatedPagedCollection(query.PageIndex,query.PageSize,noCacheResults.Items.Cast<TOverrideContainerType>(),noCacheResults.TotalRecords);
            }

            IPagedCollection<TOverrideContainerType> results = null;

            ResultData<string> resultInfo = null;
            bool hasResultsList = false;
            string cacheKey = query.CacheKey;

            _lock.EnterReadLock();
            try
            {
                Dictionary<string, ResultData<string>> cachedQueries = GetCachedQueries();

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
                List<TContainerType> items = new List<TContainerType>();

                for (int i = 0; i < resultInfo.Keys.Length; i++)
                {
                    TContainerType singleItem = Get(resultInfo.Keys[i]);

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

            int target = Math.Min(resultInfo.TotalRecords, resultInfo.PageSize);
            int actual = 0;
            int keyLength = resultInfo.Keys.Length;
            for (int i = 0; i < keyLength; i++)
            {
                TContainerType result = CacheProvider.Get<TContainerType>(resultInfo.Keys[i]);

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
                percentage = (actual/target)*100;
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

            List<string> keys = new List<string>();
            IPagedCollection<TContainerType> results = null;
            string cacheKey = query.CacheKey;

            results = _getDataQuery(query);

            List<TContainerType> replacementCollection = new List<TContainerType>();

            foreach (TContainerType item in results.Items)
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

            ResultData<string> resultInfo = new ResultData<string>(keys.ToArray(), query.PageIndex, query.PageSize, results.TotalRecords);

            _lock.EnterWriteLock();
            try
            {
                Dictionary<string, ResultData<string>> cachedQueries = GetCachedQueries();

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
            //Clear all of the queries
            Dictionary<string, ResultData<string>> cachedQueries = GetCachedQueries();

            cachedQueries.Clear();

            SetCachedQueries(cachedQueries);
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

            public uint PageIndex { get; set; }
            public int PageSize { get; set; }
            public TKey[] Keys { get; set; }
            public int TotalRecords { get; set; }
        }

        #endregion
    }
}