using System.Collections.Generic;
using System.Data;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Sql
{
    public interface IEntityDataPopulator<T>
    {
        T CreateEntity(IDataReader dataReader);
        T CreateEntityWithRead(IDataReader dataReader);
        void PopulateEntityData(T entity, IDataReader dataReader);
        IPagedCollection<T> CreateEntityCollection(IPagedQueryV2 query, IDataReader dataReader, int total);
        IPagedCollection<T> CreateEntityCollection(IPagedQueryV2 query, IDataReader dataReader, IDataParameter totalParameter);
        IEnumerable<T> CreateEntityCollection(IDataReader dataReader);
    }
}