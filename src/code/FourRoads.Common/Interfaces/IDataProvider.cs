namespace FourRoads.Common.Interfaces
{
    public interface IDataProvider<TItem, TQuery>
    {
        void Add(TItem item);
        void Delete(TItem item);
        void Update(TItem item);
        IPagedCollection<TItem> Get(TQuery query);
    }
}