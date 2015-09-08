using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FourRoads.Common.Interfaces;
using FourRoads.Common;
using System.Data.SqlClient;
using System.Data;
  
namespace FourRoads.Common.Sql
{
    public class DataLayer : IDBFactory
    {
        protected string _databaseOwner;
        protected string _connectionString;
        private ISqlDataHelper _checker = new SqlDataHelper();

        public DataLayer(ISettings configuration , string providerName)
        {
            DataProvider provider = configuration.GetDataProviderByName(providerName);

            _databaseOwner = provider.DatabaseOwner;

            _connectionString = ConfigurationManager.ConnectionStrings[provider.ConnectionStringName].ConnectionString;
        }

        public DataLayer(string databaseOwner, string connectionString)
        {
            _databaseOwner = databaseOwner;
            _connectionString = connectionString;
        }

        public ISqlDataHelper Checker
        {
            get { return _checker; }
        }

        public virtual string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public virtual string DatabaseOwner
        {
            get
            {
                return _databaseOwner;
            }
        }

        public virtual SqlConnection GetSqlConnection()
        {

            try
            {
                return new SqlConnection(ConnectionString);
            }
            catch
            {
                throw new Exception("SQL Connection String is invalid.");
            }

        }

        protected SqlCommand CreateSprocCommand(string sprocName, SqlConnection connection)
        {
            SqlCommand command = new SqlCommand(DatabaseOwner + "." + sprocName, connection);
            command.CommandType = CommandType.StoredProcedure;

            return command;
        }

        protected virtual T CreateDataEntityWithRead<T>(IDataReader reader)
        {
            IEntityDataPopulator<T> populator = GetPopulator<T>();
            return populator.CreateEntityWithRead(reader);
        }

        protected virtual T CreateDataEntity<T>(IDataReader reader)
        {
            IEntityDataPopulator<T> populator = GetPopulator<T>();
            return populator.CreateEntity(reader);
        }

        protected virtual void PopulateDataEntity<T>(T entity, IDataReader reader)
        {
            IEntityDataPopulator<T> populator = GetPopulator<T>();
            populator.PopulateEntityData(entity, reader);
        }

        protected virtual IPagedCollection<T> CreateEntityCollection<T>(IPagedQueryV2 query, IDataReader reader, int total)
        {
            IEntityDataPopulator<T> populator = GetPopulator<T>();
            return populator.CreateEntityCollection(query, reader, total); 
        }

        protected virtual IEnumerable<T> CreateEntityCollection<T>(IDataReader reader)
        {
            IEntityDataPopulator<T> populator = GetPopulator<T>();
            return populator.CreateEntityCollection(reader);
        }

        protected virtual IPagedCollection<T> CreateEntityCollection<T>(IPagedQueryV2 query, IDataReader reader, IDataParameter totalParameter)
        {
            IEntityDataPopulator<T> populator = GetPopulator<T>();
            return populator.CreateEntityCollection(query, reader, totalParameter);
        }

        protected virtual void AddEntity<T>(T entity)
        {
            IEntityDataHandler<T> handler = GetEntityDataHandler<T>();
            handler.Add(this, entity);  
        }

        protected virtual void UpdateEntity<T>(T entity)
        {
            IEntityDataHandler<T> handler = GetEntityDataHandler<T>();
            handler.Update(this, entity);
        }

        protected virtual void DeleteEntity<T>(T entity)
        {
            IEntityDataHandler<T> handler = GetEntityDataHandler<T>();
            handler.Delete(this, entity);
        }

        protected virtual T GetEntity<T, TId>(TId id)
        {
            IEntityDataHandler<T> handler = GetEntityDataHandler<T>();
            return handler.Get<TId>(this, id);
        }

        protected virtual IPagedCollection<T> GetEntities<T>(IPagedQueryV2 query)
        {
            IEntityDataHandler<T> handler = GetEntityDataHandler<T>();
            return handler.Get(this, query);
        }

        private IEntityDataPopulator<T> GetPopulator<T>()
        {
            IEntityDataPopulator<T> populator = null;
            try
            {
                populator = Injector.Get<IEntityDataPopulator<T>>();
                if (populator == null)
                    throw new Exception(string.Format("Failed to bind object of IEntityDataPopulator<{0}>", typeof(T).Name));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("You must first define and bind an implementation of IEntityDataPopulator<{0}>", typeof(T).Name), ex);
            }
            return populator;
        }

        private IEntityDataHandler<T> GetEntityDataHandler<T>()
        {
            IEntityDataHandler<T> handler = null;
            try
            {
                handler = Injector.Get<IEntityDataHandler<T>>();
                if (handler == null)
                    throw new Exception(string.Format("Failed to bind object of IEntityDataHandler<{0}>", typeof(T).Name));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("You must first define and bind an implementation of IEntityDataHandler<{0}>", typeof(T).Name), ex);
            }
            return handler;
        }

        #region IDBFactory Members

        IDbCommand IDBFactory.CreateCommand(string commandName, IDbConnection connection)
        {
            return CreateSprocCommand(commandName, connection as SqlConnection); 
        }

        IDbConnection IDBFactory.CreateConnection()
        {
            return GetSqlConnection(); 
        }

        IDbDataParameter IDBFactory.CreateParameter(string name, DbType dataType)
        {
            return new SqlParameter(name, dataType.ToSqlDbType()); 
        }

        IDbDataParameter IDBFactory.CreateParameter(string name, DbType dataType, int size)
        {
            return new SqlParameter(name, dataType.ToSqlDbType(), size); 
        }

        IDbDataParameter IDBFactory.CreateParameter(string name, DbType dataType, ParameterDirection direction)
        {
            IDbDataParameter parameter = ((IDBFactory)this).CreateParameter(name, dataType);
            parameter.Direction = direction;
            return parameter;
        }

        IDbDataParameter IDBFactory.CreateParameter(string name, DbType dataType, int size, ParameterDirection direction)
        {
            IDbDataParameter parameter = ((IDBFactory)this).CreateParameter(name, dataType, size);
            parameter.Direction = direction;
            return parameter;
        }

        IDbDataParameter IDBFactory.CreateParameter(string name, object value, DbType dataType)
        {
            IDbDataParameter parameter = ((IDBFactory)this).CreateParameter(name, dataType);
			parameter.Value = value ?? DBNull.Value;
            return parameter;
        }

        IDbDataParameter IDBFactory.CreateParameter(string name, object value, DbType dataType, int size)
        {
            IDbDataParameter parameter = ((IDBFactory)this).CreateParameter(name, dataType, size);
        	parameter.Value = value ?? DBNull.Value;
            return parameter;
        }

        string IDBFactory.DatabaseOwner
        {
            get { return DatabaseOwner; }
        }

        #endregion

    }
}
