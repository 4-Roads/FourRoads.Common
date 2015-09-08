using System.Collections.Generic;
using System.Linq;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public class PagedCollectionFactory : IPagedCollectionFactory
    {
        public IPagedCollection<TItem> CreatedPagedCollection<TItem>(uint pageIndex, int pageSize, IEnumerable<TItem> items, int? totalRecords = null)
        {
            ICollection<TItem> collection = items.ToList();
 
            int total;

            if (totalRecords.HasValue)
                total = totalRecords.Value;
            else
                total = collection.Count;

            return new PagedCollection<TItem>(collection , pageIndex, pageSize, total);
        }
    }
}