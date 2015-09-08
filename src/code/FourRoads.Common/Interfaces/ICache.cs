// //------------------------------------------------------------------------------
// // <copyright company="Four Roads LLC">
// //     Copyright (c) Four Roads LLC.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.Web.Caching;

#endregion

namespace FourRoads.Common.Interfaces
{
    public interface ICache
    {
        void Insert(ICachable value);

        void Insert(string key, object value, CacheDependency dependencies, int absoluteExpiration);

        void Insert(string key, object value, CacheDependency dependencies, int absoluteExpiration, CacheItemPriority priority);

        T Get<T>(string key);

        void Remove(string key);

        void RemoveByPattern(string pattern);
    }
}