using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common
{
    public class InjectionLoadException : SystemException
    { 
        public InjectionLoadException ()
        {
        }

        public InjectionLoadException(string message)
            : base(message)
        {

        }

        public InjectionLoadException(string message, Exception innerException)
            : base(message , innerException)
        {

        }
    }

    public class InjectionModuleLoadException : InjectionLoadException
    {
        public InjectionModuleLoadException(string moduleName , string message)
            : base(string.Format("{0} falied to load due to: {1}", moduleName, message))
        {

        }

        public InjectionModuleLoadException(string moduleName, string message, Exception innerException)
            : base(string.Format("{0} falied to load due to: {1}", moduleName, message), innerException)
        {

        }
    }
}
