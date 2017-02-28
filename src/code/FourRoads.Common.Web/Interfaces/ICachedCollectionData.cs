namespace FourRoads.Common.Interfaces
{
    public interface ICachedCollectionData<TItem, TQuery>
    {
        TItem GetSingleNoCache(string cacheID);
        IPagedCollection<TItem> GetQueryNoCache(TQuery query);
    }
}