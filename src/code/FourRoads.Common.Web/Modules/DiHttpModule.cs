using System;
using System.Diagnostics;
using System.Web;

namespace FourRoads.Common
{
    /// <summary>
    /// HttpModule that loads Ninject bindings from configuration file
    /// </summary>
    public abstract class InjectorHttpModule<T> : IHttpModule where T : Settings<T>, new()
    {
        #region IHttpModule Members

        public void Dispose()
        {
        }

        /// <summary>
        /// IHttpModule initilization handler, thread safe
        /// </summary>
        /// <param name="context"></param>
        public virtual void Init(HttpApplication context)
        {
            Debug.Assert(Settings<T>.Instance() != null);

            Injector.LoadBindingsFromSettings(Settings<T>.Instance());
        }

        #endregion
    }
}
