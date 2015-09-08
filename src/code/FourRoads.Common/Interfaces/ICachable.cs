using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;

namespace FourRoads.Common.Interfaces
{
    public interface ICachable
    {
        string CacheID { get; }

        int CacheRefreshInterval { get; }

        int CacheSlidingExpiration { get; }

        int CachePriority { get; }

        CacheDependency CacheDependancies { get; } 
    }
}
