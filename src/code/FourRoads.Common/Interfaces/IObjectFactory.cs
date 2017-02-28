using System;

namespace FourRoads.Common.Interfaces
{
    public interface IObjectFactory
    {
        T Get<T>() where T : class;
        object Get(Type type);
    }
}