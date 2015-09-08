using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common.Interfaces
{
    [Obsolete("Use IManagerV2")]
    public interface IManagerGetNoCache<TItem, TQuery, TId>
    {
        TItem Get(TId id);
        IPagedCollection<TItem> GetItems(TQuery query);
    }

    [Obsolete("Use IManagerV2")]
    public interface IManager<TItem, TQuery, TId>
    {
        TItem Get(TId id);
        IPagedCollection<TItem> GetItems(TQuery query);
        void Add(TItem item);
        void Delete(TItem item);
        void Update(TItem item);
        IManagerGetNoCache<TItem, TQuery, TId> NoCache();
    }


    public interface IManagerV2<TItem, TQuery, TId>
    {
        TItem Get(TId id);
        TItem Get(TId id , bool useCache);
        IPagedCollection<TItem> GetItems(TQuery query);
        void Add(TItem item);
        void Delete(TItem item);
        void Update(TItem item);
    }
}
