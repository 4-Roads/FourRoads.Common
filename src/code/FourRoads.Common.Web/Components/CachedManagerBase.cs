using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public abstract class CachedManagerBase<TItem, TQuery, TId, TCache, TData> :
        IManagerV2<TItem, TQuery, TId>
        where TQuery : class, IPagedQueryV2
        where TItem : class, ICacheable
        where TCache :  CachedCollection<TItem, TQuery, TCache>, ICachedCollectionData<TItem, TQuery>, new()
        where TData : class , IDataProvider<TItem, TQuery>
    {

        public CachedManagerBase()
        {
            _cachedCollection = CachedCollection<TItem, TQuery, TCache>.Cache();
        }

        private TCache _cachedCollection;
        protected virtual TCache CachedCollection
        {
            get
            {
                return _cachedCollection;
            }
        }

        private TData _dataProvider;
        protected virtual TData DataProvider
        {
            get
            {
                if (_dataProvider == null)
                    _dataProvider = Injector.Get<TData>();

                return _dataProvider;
            }
        }

        protected abstract string FormatCacheId(TId id);

        #region IManager<TItem,TQuery,TId> Members

        public virtual void Add(TItem item)
        {
            DataProvider.Add(item);
            CachedCollection.Add(item);
        }
        public virtual void Delete(TItem item)
        {
            DataProvider.Delete(item);
            CachedCollection.Remove(item);
        }
        public virtual void Update(TItem item)
        {
            CachedCollection.Remove(item);
            DataProvider.Update(item);
            CachedCollection.Add(item);
        }

        public virtual TItem Get(TId id)
        {
            return Get(id, true);
        }

        public TItem Get(TId id, bool useCache)
        {
            return CachedCollection.Get(FormatCacheId(id), useCache);
        }

        public virtual IPagedCollection<TItem> GetItems(TQuery query)
        {
            return CachedCollection.Get(query);
        }

        #endregion
    }
}
