using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace FourRoads.Common.Sql
{
    public interface IDBFactory
    {
        IDbCommand CreateCommand(string commandName, IDbConnection connection);
        IDbConnection CreateConnection();
        IDbDataParameter CreateParameter(string name, System.Data.DbType dataType);
        IDbDataParameter CreateParameter(string name, System.Data.DbType dataType, int size);
        IDbDataParameter CreateParameter(string name, System.Data.DbType dataType, ParameterDirection direction);
        IDbDataParameter CreateParameter(string name, System.Data.DbType dataType, int size, ParameterDirection direction);
        IDbDataParameter CreateParameter(string name, object value, System.Data.DbType dataType);
        IDbDataParameter CreateParameter(string name, object value, System.Data.DbType dataType, int size);
        string DatabaseOwner { get; }
    }
}
