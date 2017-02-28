using System;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Sql
{
    public abstract class EntityDataHandler<TEntity> : EntityDataPopulator<TEntity>, IEntityDataHandler<TEntity> where TEntity : class
    {
        protected EntityDataHandler(IDataHelper dataHelper, IPagedCollectionFactory pagedCollectionFactory, IObjectFactory objectFactory)
            : base(dataHelper, pagedCollectionFactory, objectFactory)
        {
        }

        #region IEntityDataHandler<TEntity> Members

        public virtual void Add(IDBFactory factory, TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual void Update(IDBFactory factory, TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(IDBFactory factory, TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual TEntity Get<TId>(IDBFactory factory, TId id)
        {
            throw new NotImplementedException();
        }

        public virtual IPagedCollection<TEntity> Get(IDBFactory factory, IPagedQueryV2 query)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}