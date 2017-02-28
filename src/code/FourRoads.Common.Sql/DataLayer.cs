using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Sql
{
    public abstract class DataLayer : IDBFactory
    {
        private readonly IObjectFactory _objectFactory;

        public DataLayer(IObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        public abstract IDataHelper Checker { get; }

        public virtual string ConnectionString { get; set; }

        public virtual string DatabaseOwner { get; set; }

        public abstract SqlConnection GetSqlConnection();

        protected abstract SqlCommand CreateSprocCommand(string sprocName, SqlConnection connection);

        protected virtual T CreateDataEntityWithRead<T>(IDataReader reader)
        {
            var populator = GetPopulator<T>();
            return populator.CreateEntityWithRead(reader);
        }

        protected virtual T CreateDataEntity<T>(IDataReader reader)
        {
            var populator = GetPopulator<T>();
            return populator.CreateEntity(reader);
        }

        protected virtual void PopulateDataEntity<T>(T entity, IDataReader reader)
        {
            var populator = GetPopulator<T>();
            populator.PopulateEntityData(entity, reader);
        }

        protected virtual IPagedCollection<T> CreateEntityCollection<T>(IPagedQueryV2 query, IDataReader reader, int total)
        {
            var populator = GetPopulator<T>();
            return populator.CreateEntityCollection(query, reader, total);
        }

        protected virtual IEnumerable<T> CreateEntityCollection<T>(IDataReader reader)
        {
            var populator = GetPopulator<T>();
            return populator.CreateEntityCollection(reader);
        }

        protected virtual IPagedCollection<T> CreateEntityCollection<T>(IPagedQueryV2 query, IDataReader reader, IDataParameter totalParameter)
        {
            var populator = GetPopulator<T>();
            return populator.CreateEntityCollection(query, reader, totalParameter);
        }

        protected virtual void AddEntity<T>(T entity)
        {
            var handler = GetEntityDataHandler<T>();
            handler.Add(this, entity);
        }

        protected virtual void UpdateEntity<T>(T entity)
        {
            var handler = GetEntityDataHandler<T>();
            handler.Update(this, entity);
        }

        protected virtual void DeleteEntity<T>(T entity)
        {
            var handler = GetEntityDataHandler<T>();
            handler.Delete(this, entity);
        }

        protected virtual T GetEntity<T, TId>(TId id)
        {
            var handler = GetEntityDataHandler<T>();
            return handler.Get(this, id);
        }

        protected virtual IPagedCollection<T> GetEntities<T>(IPagedQueryV2 query)
        {
            var handler = GetEntityDataHandler<T>();
            return handler.Get(this, query);
        }

        private IEntityDataPopulator<T> GetPopulator<T>()
        {
            IEntityDataPopulator<T> populator = null;
            try
            {
                populator = _objectFactory.Get<IEntityDataPopulator<T>>();
                if (populator == null)
                    throw new Exception($"Failed to bind object of IEntityDataPopulator<{typeof(T).Name}>");
            }
            catch (Exception ex)
            {
                throw new Exception($"You must first define and bind an implementation of IEntityDataPopulator<{typeof(T).Name}>", ex);
            }
            return populator;
        }

        private IEntityDataHandler<T> GetEntityDataHandler<T>()
        {
            IEntityDataHandler<T> handler = null;
            try
            {
                handler = _objectFactory.Get<IEntityDataHandler<T>>();
                if (handler == null)
                    throw new Exception($"Failed to bind object of IEntityDataHandler<{typeof(T).Name}>");
            }
            catch (Exception ex)
            {
                throw new Exception($"You must first define and bind an implementation of IEntityDataHandler<{typeof(T).Name}>", ex);
            }
            return handler;
        }

        #region IDBFactory Members

        public IDbCommand CreateCommand(string commandName, IDbConnection connection)
        {
            return CreateSprocCommand(commandName, connection as SqlConnection);
        }

        public IDbConnection CreateConnection()
        {
            return GetSqlConnection();
        }

        public abstract IDbDataParameter CreateParameter(string name, DbType dataType);

        public abstract IDbDataParameter CreateParameter(string name, DbType dataType, int size);

        public IDbDataParameter CreateParameter(string name, DbType dataType, ParameterDirection direction)
        {
            var parameter = ((IDBFactory) this).CreateParameter(name, dataType);
            parameter.Direction = direction;
            return parameter;
        }

        public IDbDataParameter CreateParameter(string name, DbType dataType, int size, ParameterDirection direction)
        {
            var parameter = ((IDBFactory) this).CreateParameter(name, dataType, size);
            parameter.Direction = direction;
            return parameter;
        }

        public IDbDataParameter CreateParameter(string name, object value, DbType dataType)
        {
            var parameter = ((IDBFactory) this).CreateParameter(name, dataType);
            parameter.Value = value ?? DBNull.Value;
            return parameter;
        }

        public IDbDataParameter CreateParameter(string name, object value, DbType dataType, int size)
        {
            var parameter = ((IDBFactory) this).CreateParameter(name, dataType, size);
            parameter.Value = value ?? DBNull.Value;
            return parameter;
        }

        #endregion
    }
}