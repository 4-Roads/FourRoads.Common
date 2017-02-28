using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Sql
{
    public interface IEntityDataHandler<TEntity> : IEntityDataPopulator<TEntity>
    {
        void Add(IDBFactory factory, TEntity entity);
        void Update(IDBFactory factory, TEntity entity);
        void Delete(IDBFactory factory, TEntity entity);
        TEntity Get<TId>(IDBFactory factory, TId id);
        IPagedCollection<TEntity> Get(IDBFactory factory, IPagedQueryV2 query);
    }
}