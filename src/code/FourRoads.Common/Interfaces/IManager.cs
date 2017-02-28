namespace FourRoads.Common.Interfaces
{
    public interface IManagerV2<TItem, TQuery, TId>
    {
        TItem Get(TId id);
        TItem Get(TId id, bool useCache);
        IPagedCollection<TItem> GetItems(TQuery query);
        void Add(TItem item);
        void Delete(TItem item);
        void Update(TItem item);
    }
}