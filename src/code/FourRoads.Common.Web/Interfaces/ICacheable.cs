using System;
using System.Web.Caching;

namespace FourRoads.Common.Interfaces
{
    public interface ICacheable
    {
        string CacheID { get; }

        int CacheRefreshInterval { get; }

    	string[] CacheTags { get; }

        CacheScopeOption CacheScope { get; }
    }
}
