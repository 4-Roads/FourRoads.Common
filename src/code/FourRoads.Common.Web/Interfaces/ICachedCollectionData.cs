using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common.Interfaces
{
    public interface ICachedCollectionData<TItem, TQuery>
    {
        TItem GetSingleNoCache(string cacheID);
        IPagedCollection<TItem> GetQueryNoCache(TQuery query);
    }
}
