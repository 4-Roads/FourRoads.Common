using System;
using System.Collections.Generic;
using System.Data;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Sql
{
    public abstract class EntityDataPopulator<TEntity> : IEntityDataPopulator<TEntity> where TEntity : class
    {
        public EntityDataPopulator(IDataHelper dataHelper, IPagedCollectionFactory pagedCollectionFactory, IObjectFactory objectFactory)
        {
            if (dataHelper == null)
                throw new ArgumentNullException("dataHelper");

            if (pagedCollectionFactory == null)
                throw new ArgumentNullException("pagedCollectionFactory");

            if (pagedCollectionFactory == null)
                throw new ArgumentNullException("objectFactory");

            DataHelper = dataHelper;
            PagedCollectionFactory = pagedCollectionFactory;
            ObjectFactory = objectFactory;
        }

        private IObjectFactory ObjectFactory { get; }
        public IPagedCollectionFactory PagedCollectionFactory { get; }
        public IDataHelper DataHelper { get; }

        public virtual TEntity CreateEntityWithRead(IDataReader dataReader)
        {
            if (dataReader.Read())
                return CreateEntity(dataReader);

            return null;
        }

        public virtual TEntity CreateEntity(IDataReader dataReader)
        {
            var entity = ObjectFactory.Get<TEntity>();
            PopulateEntityData(entity, dataReader);
            return entity;
        }

        public abstract void PopulateEntityData(TEntity entity, IDataReader dataReader);

        public virtual IPagedCollection<TEntity> CreateEntityCollection(IPagedQueryV2 query, IDataReader dataReader, int total)
        {
            var items = CreateEntityCollection(dataReader);

            return PagedCollectionFactory.CreatedPagedCollection(query.PageIndex, query.PageSize, items, total > 0 ? total : default(int?));
        }

        public virtual IPagedCollection<TEntity> CreateEntityCollection(IPagedQueryV2 query, IDataReader dataReader, IDataParameter totalParameter)
        {
            var items = CreateEntityCollection(dataReader);

            //To get the output parameter the redare must be closed
            if (totalParameter.Direction == ParameterDirection.ReturnValue)
                dataReader.Close();
            else
                while (totalParameter.Value == null && dataReader.NextResult())
                {
                }

            var totalItems = totalParameter.Value != null ? (int) totalParameter.Value : 0;

            return PagedCollectionFactory.CreatedPagedCollection(query.PageIndex, query.PageSize, items, totalItems);
        }

        public virtual IEnumerable<TEntity> CreateEntityCollection(IDataReader dataReader)
        {
            var list = new List<TEntity>();
            while (dataReader.Read())
            {
                list.Add(CreateEntity(dataReader));
            }

            return list;
        }
    }
}