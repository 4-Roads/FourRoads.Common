using System;
using System.Threading;


namespace FourRoads.Common
{
    public class AsyncResultBase<T> : IAsyncResult, IDisposable
    {
        private readonly object _asyncState;
        private readonly AsyncCallback _callback;
        private readonly object _syncRoot;
        private readonly ManualResetEvent _waitHandle;
        private bool _completed;
        private bool _completedSynchronously;
        private Exception _e;
        private T _result;

        public AsyncResultBase(AsyncCallback cb, object state)
            : this(cb, state, false)
        {
        }

        public AsyncResultBase(AsyncCallback cb, object state,
                               bool completed)
        {
            _callback = cb;
            _asyncState = state;
            _completed = completed;
            _completedSynchronously = completed;
            _waitHandle = new ManualResetEvent(false);
            _syncRoot = new object();
        }


        public bool Aborted { get; set; }

        public Exception Exception
        {
            get
            {
                lock (_syncRoot)
                {
                    return _e;
                }
            }
        }

        public T Result
        {
            get
            {
                lock (_syncRoot)
                {
                    return _result;
                }
            }
            protected set
            {
                lock (_syncRoot)
                {
                    _result = value;
                }
            }
        }

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return _asyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _waitHandle; }
        }

        public bool CompletedSynchronously
        {
            get
            {
                lock (_syncRoot)
                {
                    return _completedSynchronously;
                }
            }
        }

        public bool IsCompleted
        {
            get
            {
                lock (_syncRoot)
                {
                    return _completed;
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_syncRoot)
                {
                    if (_waitHandle != null)
                    {
                        ((IDisposable) _waitHandle).Dispose();
                    }
                }
            }
        }

        protected void Complete(T result, bool completedSynchronously)
        {
            lock (_syncRoot)
            {
                _completed = true;
                _completedSynchronously = completedSynchronously;
                _result = result;
            }
            SignalCompletion();
        }

        protected void HandleException(Exception e, bool completedSynchronously)
        {
            lock (_syncRoot)
            {
                _completed = true;
                _completedSynchronously = completedSynchronously;
                _e = e;
            }
            SignalCompletion();
        }

        private void SignalCompletion()
        {
            _waitHandle.Set();
            ThreadPool.QueueUserWorkItem(new WaitCallback(InvokeCallback));
        }

        private void InvokeCallback(object state)
        {
            if (_callback != null)
            {
                _callback(this);
            }
        }
    }
}