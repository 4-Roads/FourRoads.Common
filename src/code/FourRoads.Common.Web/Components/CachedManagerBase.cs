using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public abstract class CachedManagerBase<TItem, TQuery, TId, TCache, TData> :
        IManagerV2<TItem, TQuery, TId>
        where TQuery : class, IPagedQueryV2
        where TItem : class, ICacheable
        where TCache : CachedCollection<TItem, TQuery>, ICachedCollectionData<TItem, TQuery>
        where TData : class, IDataProvider<TItem, TQuery>
    {
        private readonly IObjectFactory _objectFactory;
        private TData _dataProvider;


        public CachedManagerBase(IObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
            CachedCollection = objectFactory.Get<TCache>();
        }

        protected virtual TCache CachedCollection { get; }

        protected virtual TData DataProvider
        {
            get
            {
                if (_dataProvider == null)
                    _dataProvider = _objectFactory.Get<TData>();

                return _dataProvider;
            }
        }

        protected abstract string FormatCacheId(TId id);

        #region IManager<TItem,TQuery,TId> Members

        public virtual void Add(TItem item)
        {
            DataProvider.Add(item);
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