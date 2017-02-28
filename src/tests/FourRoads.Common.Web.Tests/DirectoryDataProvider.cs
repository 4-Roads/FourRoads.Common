using System.Data;
using System.Data.SqlClient;
using FourRoads.Common.Interfaces;
using FourRoads.Common.Sql;
using FourRoads.Common.Web.Tests.Entities;
using FourRoads.Common.Web.Tests.Interfaces;
using FourRoads.Common.Web.Tests.Queries;

namespace FourRoads.Common.Web.Tests.DataProviders
{
    public class DirectoryDataProvider : DataLayer, IDirectoryDataProvider
    {
        public DirectoryDataProvider(IObjectFactory objectFactory) : base(objectFactory)
        {
        }

        #region Implementation of IDataProvider<Directory,DirectoryQuery>

        public void Add(Directory item)
        {
            AddEntity(item);
        }

        public void Delete(Directory item)
        {
            DeleteEntity(item);
        }

        public void Update(Directory item)
        {
            UpdateEntity(item);
        }

        public IPagedCollection<Directory> Get(DirectoryQuery query)
        {
            return GetEntities<Directory>(query);
        }

        #endregion

        public override IDataHelper Checker { get; }

        public override SqlConnection GetSqlConnection()
        {
            throw new System.NotImplementedException();
        }

        protected override SqlCommand CreateSprocCommand(string sprocName, SqlConnection connection)
        {
            throw new System.NotImplementedException();
        }

        public override IDbDataParameter CreateParameter(string name, DbType dataType)
        {
            throw new System.NotImplementedException();
        }

        public override IDbDataParameter CreateParameter(string name, DbType dataType, int size)
        {
            throw new System.NotImplementedException();
        }
    }
}