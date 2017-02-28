using System.Data;

namespace FourRoads.Common.Sql
{
    public interface IDBFactory
    {
        string DatabaseOwner { get; }
        IDbCommand CreateCommand(string commandName, IDbConnection connection);
        IDbConnection CreateConnection();
        IDbDataParameter CreateParameter(string name, DbType dataType);
        IDbDataParameter CreateParameter(string name, DbType dataType, int size);
        IDbDataParameter CreateParameter(string name, DbType dataType, ParameterDirection direction);
        IDbDataParameter CreateParameter(string name, DbType dataType, int size, ParameterDirection direction);
        IDbDataParameter CreateParameter(string name, object value, DbType dataType);
        IDbDataParameter CreateParameter(string name, object value, DbType dataType, int size);
    }
}