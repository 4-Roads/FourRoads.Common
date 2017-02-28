using System;
using FourRoads.Common.Interfaces;
using SimpleInjector;

namespace FourRoads.Common.Web.Tests
{
    public class Injector : IObjectFactory
    {
        // 1. Create a new Simple Injector container
        private Container container = new Container();

        private Injector()
        {
            // 2. Configure the container (register)
            container.Register<ICache, MockCache>(Lifestyle.Singleton);
            container.Register<IPagedCollectionFactory, PagedCollectionFactoryMock>(Lifestyle.Singleton);

            // 3. Optionally verify the container's configuration.
            container.Verify();
        }

        public void SetContainer(Container container)
        {
            this.container = container;
        }

        public static Injector Instance { get; } = new Injector();

        public T Get<T>() where T : class
        {
            return container.GetInstance<T>();
        }

        public object Get(Type type)
        {
            return container.GetInstance(type);
        }
    }
}