using System;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public class AppEventArgs : EventArgs, IAppEventArgs
    {
        public AppEventArgs()
        {
        }

        public AppEventArgs(AppEventType type, string state)
        {
            Type = type;
            State = state;
        }

        #region IAppEventArgs Members

        public string State { get; set; }

        public AppEventType Type { get; set; }

        #endregion
    }
}