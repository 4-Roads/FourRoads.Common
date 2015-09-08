using System;

namespace FourRoads.Common.Interfaces
{
    public interface IAppEventManager
    {
        void RegisterHandler(Type objectType, AppEventHandler addEvent);
     
        event AppExceptionHandler ExceptionHandler;

        void ExecuteEvent(AppEventType type, string state, object eventItem);
    }
}