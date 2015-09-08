// // //------------------------------------------------------------------------------
// // // <copyright company="Four Roads LLC">
// // //     Copyright (c) Four Roads LLC.  All rights reserved.
// // // </copyright>
// // //------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;
using FourRoads.Common.Interfaces;
using System.Web;

namespace FourRoads.Common
{
    public class SimpleCachedCollection<TContainerType, TDerivedType>
        where TContainerType : class, ICacheable
        where TDerivedType : class, new()
    {
        #region Delegates

        public delegate TContainerType RefreshSingle(string id);

        #endregion

        private readonly ICache _cacheProvider = Injector.Get<ICache>();
        protected ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private RefreshSingle _getDataSingle; // Delegate to retrieve multiple items
        private string _derrivedTypeName;

        protected RefreshSingle GetDataSingle
        {
            set { _getDataSingle = value; }
        }

        protected ICache CacheProvider
        {
            get { return _cacheProvider; }
        }

        protected string DerrivedTypeName
        {
            get
            {
                if (string.IsNullOrEmpty(_derrivedTypeName))
                {
                    _derrivedTypeName = typeof (TDerivedType).FullName;
                }

                return _derrivedTypeName;
            }
        }

        public static TDerivedType Cache()
        {
            ICache cacheProvider = Injector.Get<ICache>();
            //Get this object from the cache of create a new one
            TDerivedType obj = cacheProvider.Get<TDerivedType>(typeof(TDerivedType).FullName);

            if (obj == null)
            {
                obj = new TDerivedType();
                cacheProvider.Insert(typeof(TDerivedType).FullName, obj, new TimeSpan(0,12,0,0));
            }

            return obj;
        }

        public virtual TOverrideContainerType Get<TOverrideContainerType>(string id) where TOverrideContainerType : class
        {
            return Get<TOverrideContainerType>(id, true);
        }

        public virtual TOverrideContainerType Get<TOverrideContainerType>(string id , bool useCache) where TOverrideContainerType : class
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
                    result = _cacheProvider.Get<TContainerType>(id);
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

        public virtual TContainerType Get(string id )
        {
            return Get<TContainerType>(id , true);
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
                 _cacheProvider.Remove(obj.CacheID);
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
                 _cacheProvider.Insert(obj , new string[]{DerrivedTypeName});
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
                _cacheProvider.RemoveByTags(new string[] { DerrivedTypeName });
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}