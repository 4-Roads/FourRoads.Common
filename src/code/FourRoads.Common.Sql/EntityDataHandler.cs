using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FourRoads.Common.Interfaces;
using Ninject.Parameters;
  
namespace FourRoads.Common.Sql
{
    public abstract class EntityDataHandler<TEntity> : EntityDataPopulator<TEntity>, IEntityDataHandler<TEntity> where TEntity : class
    {
        protected EntityDataHandler(ISqlDataHelper dataHelper, IPagedCollectionFactory pagedCollectionFactory)
            : base(dataHelper, pagedCollectionFactory)
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
