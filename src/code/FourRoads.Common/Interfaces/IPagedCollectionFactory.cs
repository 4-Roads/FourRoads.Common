using System.Collections.Generic;

namespace FourRoads.Common.Interfaces
{
    public interface IPagedCollectionFactory
    {
        IPagedCollection<TItem> CreatedPagedCollection<TItem>(uint pageIndex, int pageSize, IEnumerable<TItem> items, int? totalRecords = null);
    }
}