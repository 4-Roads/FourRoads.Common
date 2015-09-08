using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FourRoads.Common
{
    public class ThreadSafeDictionary<TId, TValue> : IDictionary<TId, TValue>
        where TValue : class
    {
        private Dictionary<TId, TValue> _dict = new Dictionary<TId, TValue>();
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        #region IDictionary<TId,TValue> Members

        public void Add(TId key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                _dict.Add(key, value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool ContainsKey(TId key)
        {
            bool ret = false;

            _lock.EnterReadLock();
            try
            {
                ret = _dict.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return ret;
        }

        public ICollection<TId> Keys
        {
            get
            {
                TId[] ret = null;

                _lock.EnterReadLock();
                try
                {
                    ret = (from ele in _dict
                           select ele.Key).ToArray();
                }
                finally
                {
                    _lock.ExitReadLock();
                }

                return ret;
            }
        }

        public bool Remove(TId key)
        {
            bool ret = false;

            _lock.EnterWriteLock();
            try
            {
                ret = _dict.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return ret;
        }

        public bool TryGetValue(TId key, out TValue value)
        {
            TValue outVal = null;
            bool ret = false;

            _lock.EnterReadLock();
            try
            {
                ret = _dict.TryGetValue(key, out outVal);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            value = outVal;
            return ret;
        }

        public ICollection<TValue> Values
        {
            get
            {
                TValue[] ret = null;

                _lock.EnterWriteLock();
                try
                {
                    ret = new TValue[_dict.Count];

                    int i = 0;
                    foreach (KeyValuePair<TId, TValue> couple in _dict)
                    {
                        ret[i] = couple.Value;
                        ++i;
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                return ret;
            }
        }

        public TValue this[TId key]
        {
            get
            {
                TValue ret = null;

                _lock.EnterReadLock();
                try
                {
                    ret = _dict[key];
                }
                finally
                {
                    _lock.ExitReadLock();
                }

                return ret;
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    _dict[key] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public void Add(KeyValuePair<TId, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _dict.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(KeyValuePair<TId, TValue> item)
        {
            bool ret = false;

            _lock.EnterReadLock();
            try
            {
                ret = _dict.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return ret;
        }

        public void CopyTo(KeyValuePair<TId, TValue>[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                int arrLen = array.Length - arrayIndex;
                if (arrLen < _dict.Count)
                    throw new ArgumentException("Array too short.");

                int i = arrayIndex;
                foreach (KeyValuePair<TId, TValue> couple in _dict)
                {
                    array[i] = couple;
                    ++arrayIndex;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public int Count
        {
            get
            {
                int ret = 0;

                _lock.EnterReadLock();
                try
                {
                    ret = _dict.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }

                return ret;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TId, TValue> item)
        {
            bool ret = false;

            _lock.EnterWriteLock();
            try
            {
                ret = _dict.Remove(item.Key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return ret;
        }

        public IEnumerator<KeyValuePair<TId, TValue>> GetEnumerator()
        {
            _lock.EnterReadLock();
            return new LockedEnumerator<TId, TValue>(_lock, _dict.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            _lock.EnterReadLock();
            return new LockedEnumerator<TId, TValue>(_lock, _dict.GetEnumerator());
        }

        #endregion

        /// <summary>
        ///   Add a value with a certain key only if no other value with the same key is already present.
        /// </summary>
        public void AddIfDoesntContain(TId key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_dict.ContainsKey(key))
                    _dict.Add(key, value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddIfDoesntContainFunc(TId key, Func<TValue> values)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_dict.ContainsKey(key))
                    _dict.Add(key, values());
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        ///   Returns a dictionary with an exclusive write lock on the original collection.
        ///   Can be used to get quick and coherent access to the dictionary, without fine grained locking on each method.
        /// </summary>
        public LockedDictionary<TId, TValue> GetExclusiveDictionary()
        {
            _lock.EnterWriteLock();
            return new LockedDictionary<TId, TValue>(_dict, _lock);
        }

        /// <summary>
        ///   Allows to manipulate the dictionary directly after having acquired an exclusive write lock.
        /// </summary>
        public void ManipulateWithWriteLock(Action<IDictionary<TId, TValue>> operation)
        {
            _lock.EnterWriteLock();
            try
            {
                operation(_dict);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        ///   Allows to manipulate the dictionary directly after having acquired a read lock.
        ///   Be extra careful to NOT change the dictionary while accessing it.
        /// </summary>
        public void ManipulateWithReadLock(Action<IDictionary<TId, TValue>> operation)
        {
            _lock.EnterReadLock();
            try
            {
                operation(_dict);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        #region Helpers

        private void DoWithReaderLock(Action action)
        {
            _lock.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private void DoWithWriterLock(Action action)
        {
            _lock.EnterWriteLock();
            try
            {
                action();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        #endregion
    }

    /// <summary>
    ///   Simple wrapper around a dictionary that releases a write lock when disposed.
    /// </summary>
    public class LockedDictionary<TId, TValue> : IDictionary<TId, TValue>, IDisposable
    {
        private IDictionary<TId, TValue> _dict;
        private ReaderWriterLockSlim _lock;

        public LockedDictionary(IDictionary<TId, TValue> dict, ReaderWriterLockSlim lck)
        {
            _dict = dict;
            _lock = lck;
        }

        #region IDictionary<TId,TValue> Members

        public void Add(TId key, TValue value)
        {
            _dict.Add(key, value);
        }

        public bool ContainsKey(TId key)
        {
            return _dict.ContainsKey(key);
        }

        public ICollection<TId> Keys
        {
            get { return _dict.Keys; }
        }

        public bool Remove(TId key)
        {
            return _dict.Remove(key);
        }

        public bool TryGetValue(TId key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return _dict.Values; }
        }

        public TValue this[TId key]
        {
            get { return _dict[key]; }
            set { _dict[key] = value; }
        }

        public void Add(KeyValuePair<TId, TValue> item)
        {
            _dict.Add(item);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<TId, TValue> item)
        {
            return _dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TId, TValue>[] array, int arrayIndex)
        {
            _dict.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return _dict.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<TId, TValue> item)
        {
            return _dict.Remove(item);
        }

        public IEnumerator<KeyValuePair<TId, TValue>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _lock.ExitWriteLock();
        }

        #endregion
    }

    /// <summary>
    ///   Wrapper around an IEnumerator that releases a lock when disposed.
    /// </summary>
    internal class LockedEnumerator<TId, TValue> : IEnumerator<KeyValuePair<TId, TValue>>, IEnumerator
    {
        private IEnumerator<KeyValuePair<TId, TValue>> _enumerator;
        private ReaderWriterLockSlim _lock;

        public LockedEnumerator(ReaderWriterLockSlim lockedLock, IEnumerator<KeyValuePair<TId, TValue>> enumerator)
        {
            _lock = lockedLock;
            _enumerator = enumerator;
        }

        #region IEnumerator<KeyValuePair<TId,TValue>> Members

        public KeyValuePair<TId, TValue> Current
        {
            get { return _enumerator.Current; }
        }

        public void Dispose()
        {
            _enumerator.Dispose();
            _lock.ExitReadLock();
        }

        object IEnumerator.Current
        {
            get { return _enumerator.Current; }
        }

        bool IEnumerator.MoveNext()
        {
            return _enumerator.MoveNext();
        }

        void IEnumerator.Reset()
        {
            _enumerator.Reset();
        }

        #endregion

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }
    }
}