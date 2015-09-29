// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.Web.Caching;

#endregion

namespace FourRoads.Common.Interfaces
{
    [Flags]
    public enum CacheScopeOption
    {
        Context =1,
        Local =2,
        Distributed=4,
        All = Context | Local | Distributed
    }

    public interface ICache
    {
		void Insert(ICacheable value);

        void Insert(ICacheable value, string[] additionalTags);

		void Insert(string key, object value);

		void Insert(string key, object value, string[] tags);

		void Insert(string key, object value, TimeSpan timeout);

		void Insert(string key, object value, string[] tags, TimeSpan timeout);

        void Insert(string key, object value, string[] tags, TimeSpan timeout , CacheScopeOption scope);

		object Get(string key);

        T Get<T>(string key);

		void Remove(string key);

    	void RemoveByTags(string[] tags);

    	void Clear();

	}
}