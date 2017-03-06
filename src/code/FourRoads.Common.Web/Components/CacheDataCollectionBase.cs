using System.Linq;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public abstract class CachedDataCollectionBase<TItem, TQuery, TData> :
        CachedCollection<TItem, TQuery>,
        ICachedCollectionData<TItem, TQuery>
        where TItem : class, ICacheable
        where TQuery : class, IPagedQueryV2
        where TData : class, IDataProvider<TItem, TQuery>
    {
        private TData _dataProvider;
        private IObjectFactory _objectFactory;

        public CachedDataCollectionBase(IPagedCollectionFactory pagedCollectionFactory, ICache cacheProvider, IObjectFactory objectFactory) : base(pagedCollectionFactory, cacheProvider)
        {
            _objectFactory = objectFactory;

            GetDataQuery = ((ICachedCollectionData<TItem, TQuery>) this).GetQueryNoCache;
            GetDataSingle = ((ICachedCollectionData<TItem, TQuery>) this).GetSingleNoCache;
        }

        protected TData DataProvider
        {
            get
            {
                if (_dataProvider == null)
                    _dataProvider = _objectFactory.Get<TData>();

                return _dataProvider;
            }
        }

        protected abstract void SetQuery(TQuery query, string cacheID);

        #region ICachedCollectionData<TItem,TQuery> Members

        TItem ICachedCollectionData<TItem, TQuery>.GetSingleNoCache(string cacheID)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("GetSingleNoCache:{0}", cacheID));
            var query = _objectFactory.Get<TQuery>();
            SetQuery(query, cacheID);
            query.PageSize = 1;
            return DataProvider.Get(query).Items.FirstOrDefault();
            ;
        }

        IPagedCollection<TItem> ICachedCollectionData<TItem, TQuery>.GetQueryNoCache(TQuery query)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("GetQueryNoCache:{0}", query.CacheKey));

            return DataProvider.Get(query);
        }

        #endregion
    }
}