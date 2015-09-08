using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FourRoads.Common.Interfaces;
using Ninject.Parameters;

namespace FourRoads.Common.Sql
{
    public abstract class EntityDataPopulator<TEntity> : IEntityDataPopulator<TEntity> where TEntity : class 
    {
        public EntityDataPopulator(ISqlDataHelper dataHelper , IPagedCollectionFactory pagedCollectionFactory)
        {
            if (dataHelper == null)
                throw new ArgumentNullException("dataHelper");

            if (pagedCollectionFactory == null)
                throw new ArgumentNullException("pagedCollectionFactory");     

            _dataHelper = dataHelper;
            _pagedCollectionFactory = pagedCollectionFactory;
        }

        private IPagedCollectionFactory _pagedCollectionFactory = null;
        private ISqlDataHelper _dataHelper = null;

        public IPagedCollectionFactory PagedCollectionFactory {
            get { return _pagedCollectionFactory; }
        }

        public ISqlDataHelper DataHelper
        {
            get 
            {
                return _dataHelper;
            }
        }

        public virtual TEntity CreateEntityWithRead(IDataReader dataReader)
        {
            if (dataReader.Read())
                return CreateEntity(dataReader);

            return null;
        }

        public virtual TEntity CreateEntity(IDataReader dataReader)
        {
            TEntity entity = Injector.Get<TEntity>();
            PopulateEntityData(entity, dataReader);
            return entity;
        }

        public abstract void PopulateEntityData(TEntity entity, IDataReader dataReader);

        public virtual IPagedCollection<TEntity> CreateEntityCollection(IPagedQueryV2 query, IDataReader dataReader, int total)
        {
            IEnumerable<TEntity> items = CreateEntityCollection(dataReader);

            return _pagedCollectionFactory.CreatedPagedCollection(query.PageIndex, query.PageSize, items ,  total > 0 ? total : default(int?) );
        }

        public virtual IPagedCollection<TEntity> CreateEntityCollection(IPagedQueryV2 query, IDataReader dataReader, IDataParameter totalParameter)
        {
            IEnumerable<TEntity> items = CreateEntityCollection(dataReader);
 
            //To get the output parameter the redare must be closed
            if (totalParameter.Direction == ParameterDirection.ReturnValue)
                dataReader.Close();
            else
               while (totalParameter.Value == null && dataReader.NextResult()) { }

            int totalItems = totalParameter.Value != null ? (int)totalParameter.Value : 0;

            return _pagedCollectionFactory.CreatedPagedCollection(query.PageIndex, query.PageSize, items, totalItems);
        }

        public virtual IEnumerable<TEntity> CreateEntityCollection(IDataReader dataReader)
        {
            List<TEntity> list = new List<TEntity>();
            while (dataReader.Read())
            {
                list.Add(CreateEntity(dataReader));
            }

            return list;
        }
    }
}
