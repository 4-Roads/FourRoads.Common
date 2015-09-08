using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public abstract class CachedDataCollectionBase<TItem, TQuery, TData, TCollection> :
        CachedCollection<TItem, TQuery, TCollection>,
        ICachedCollectionData<TItem, TQuery>
        where TItem : class, ICacheable
        where TQuery : class, IPagedQueryV2
        where TCollection : class, new()
        where TData : class, IDataProvider<TItem, TQuery>
    {

        public CachedDataCollectionBase()
        {
            GetDataQuery = new RefreshQuery(((ICachedCollectionData<TItem, TQuery>)this).GetQueryNoCache);
            GetDataSingle = new RefreshSingle(((ICachedCollectionData<TItem, TQuery>)this).GetSingleNoCache);
        }

        protected abstract void SetQuery(TQuery query, string cacheID);

        private TData _dataProvider;
        protected TData DataProvider
        {
            get
            {
                if (_dataProvider == null)
                    _dataProvider = Injector.Get<TData>();

                return _dataProvider;
            }
        }

        
        #region ICachedCollectionData<TItem,TQuery> Members

        TItem ICachedCollectionData<TItem, TQuery>.GetSingleNoCache(string cacheID)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("GetSingleNoCache:{0}", cacheID));

            TQuery query = Injector.Get<TQuery>();
            SetQuery(query, cacheID);
            query.PageSize = 1;
            return (TItem)DataProvider.Get(query).Items.FirstOrDefault(); ;
        }

        IPagedCollection<TItem> ICachedCollectionData<TItem, TQuery>.GetQueryNoCache(TQuery query)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("GetQueryNoCache:{0}", query.CacheKey));

            return DataProvider.Get(query);
        }

        #endregion
    }
}
