using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common.Interfaces
{
    public delegate void AppEventHandler(object eventItem, IAppEventArgs e);
    public delegate void AppExceptionHandler(Exception ex);
}
