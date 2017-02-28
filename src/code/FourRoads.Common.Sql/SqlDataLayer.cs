using System;
using System.Data;
using System.Data.SqlClient;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Sql
{
    public class SqlDataLayer : DataLayer
    {
        private IObjectFactory _objectFactory;
        private IDataHelper _checker =null;

        public SqlDataLayer(IObjectFactory objectFactory):base(objectFactory)
        {

        }

        public override IDataHelper Checker
        {
            get
            {
                if ( _checker  == null)
                {
                    _checker = new SqlDataHelper(_objectFactory);
                }

                return _checker;
            }
        }

        public override SqlConnection GetSqlConnection()
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

        protected override SqlCommand CreateSprocCommand(string sprocName, SqlConnection connection)
        {
            var command = new SqlCommand(DatabaseOwner + "." + sprocName, connection);
            command.CommandType = CommandType.StoredProcedure;

            return command;
        }

        public override IDbDataParameter CreateParameter(string name, DbType dataType)
        {
            return new SqlParameter(name, dataType.ToSqlDbType());
        }

        public override IDbDataParameter CreateParameter(string name, DbType dataType, int size)
        {
            return new SqlParameter(name, dataType.ToSqlDbType(), size);
        }
    }
}