// // //------------------------------------------------------------------------------
// // // <copyright company="4 Roads Ltd">
// // //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // // </copyright>
// // //------------------------------------------------------------------------------

using System;
using System.Threading;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public class SimpleCachedCollection<TContainerType>
        where TContainerType : class, ICacheable
    {
        #region Delegates

        public delegate TContainerType RefreshSingle(string id);

        #endregion

        private string _derrivedTypeName;
        private RefreshSingle _getDataSingle; // Delegate to retrieve multiple items
        protected ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public SimpleCachedCollection(ICache cacheProvider)
        {
            CacheProvider = cacheProvider;
        }

        protected RefreshSingle GetDataSingle
        {
            set { _getDataSingle = value; }
        }

        protected ICache CacheProvider { get; }

        protected string DerrivedTypeName
        {
            get
            {
                if (string.IsNullOrEmpty(_derrivedTypeName))
                {
                    _derrivedTypeName = GetType().FullName;
                }

                return _derrivedTypeName;
            }
        }

        public virtual TOverrideContainerType Get<TOverrideContainerType>(string id) where TOverrideContainerType : class
        {
            return Get<TOverrideContainerType>(id, true);
        }

        public virtual TOverrideContainerType Get<TOverrideContainerType>(string id, bool useCache) where TOverrideContainerType : class
        {
            TContainerType result = null;
            if (id == null)
            {
                throw new ArgumentNullException("id is null");
            }

            if (useCache)
            {
                _lock.EnterReadLock();
                try
                {
                    result = CacheProvider.Get<TContainerType>(id);
                }
                finally
                {
                    _lock.ExitReadLock();
                }

                if (result == null)
                {
                    result = _getDataSingle(id);

                    if (result != null) //Caching could be out of sync
                        AddItemToCacheProvider(result);
                }
                else
                {
                    AddItemToCacheProvider(result);
                }
            }
            else
            {
                result = _getDataSingle(id);
            }

            return result as TOverrideContainerType;
        }

        public virtual TContainerType Get(string id, bool useCache)
        {
            return Get<TContainerType>(id, useCache);
        }

        public virtual TContainerType Get(string id)
        {
            return Get<TContainerType>(id, true);
        }

        public virtual void Add(TContainerType obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj is null");
            }

            AddItemToCacheProvider(obj);
        }

        public virtual void Remove(TContainerType obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj is null");
            }

            RemoveItemFromCacheProvider(obj);
        }

        protected virtual void RemoveItemFromCacheProvider(TContainerType obj)
        {
            _lock.EnterWriteLock();
            try
            {
                CacheProvider.Remove(obj.CacheID);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        protected virtual void AddItemToCacheProvider(TContainerType obj)
        {
            _lock.EnterWriteLock();
            try
            {
                CacheProvider.Insert(obj, new[] {DerrivedTypeName});
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public virtual void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                CacheProvider.RemoveByTags(new[] {DerrivedTypeName});
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}