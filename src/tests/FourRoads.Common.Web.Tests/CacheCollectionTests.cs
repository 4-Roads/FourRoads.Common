using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FourRoads.Common.Interfaces;
using SimpleInjector;

namespace FourRoads.Common.Web.Tests
{
    public class UniqueObject : ICacheable
    {
        public int ID;
        public int Version;

        #region ICacheable Members

        public string CacheID
        {
            get { return ID.ToString(); }
        }

        public int CacheRefreshInterval { get; } = 10;

        public string[] CacheTags
        {
            get { return new string[] {}; }
        }

        public CacheScopeOption CacheScope
        {
            get { return CacheScopeOption.All; }
        }

        #endregion
    }


    public class PagedCollectionFactoryMock : IPagedCollectionFactory
    {
        public IPagedCollection<TItem> CreatedPagedCollection<TItem>(uint pageIndex, int pageSize, IEnumerable<TItem> items, int? totalRecords = null)
        {
            ICollection<TItem> collection = items.ToList();

            int total;

            if (totalRecords.HasValue)
                total = totalRecords.Value;
            else
                total = collection.Count;

            return (IPagedCollection<TItem>) new PageCollectionMock(pageIndex, pageSize, total, collection.Cast<UniqueObject>().ToArray());
        }
    }

    public class PageCollectionMock : IPagedCollection<UniqueObject>
    {
        public List<UniqueObject> _items = new List<UniqueObject>();
        protected int _pageSize;

        public PageCollectionMock(uint pageIndex, int pageSize, int totalRecords, UniqueObject[] items)
        {
            _items = new List<UniqueObject>(items);
            _pageSize = _items.Count;
        }

        private PageCollectionMock()
        {
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public uint PageIndex
        {
            get { return 0; }
        }

        int IPagedCollection<UniqueObject>.PageSize
        {
            get { return PageSize; }
        }


        public IEnumerable<UniqueObject> Items
        {
            get { return _items; }
        }

        public int TotalRecords
        {
            get { return _pageSize; }
        }

        public IPagedCollection<TCast> Cast<TCast>()
        {
            throw new NotImplementedException();
        }

        public static PageCollectionMock Instance()
        {
            return new PageCollectionMock();
        }
    }

    public class MockQuery : IPagedQueryV2
    {
        public MockQuery()
        {
            UseCache = true;
        }

        public uint PageIndex { get; set; }
        public int PageSize { get; set; }
        public SortOrder SortOrder { get; set; }
        public string CacheKey { get; set; }
        public bool UseCache { get; set; }
    }

    public class TestCachedCollection : CachedCollection<UniqueObject, MockQuery>
    {
        public TestCachedCollection(IPagedCollectionFactory pagedCollectionFactory, ICache cacheProvider) : base(pagedCollectionFactory, cacheProvider)
        {
            GetDataQuery = RefreshQueryMethod;
            GetDataSingle = RefreshSingleMethod;
        }

        public bool GotFromDatabase { get; set; }

        public IPagedCollection<UniqueObject> RefreshQueryMethod(MockQuery query)
        {
            GotFromDatabase = true;

            var ids = query.CacheKey.Split(',');

            var collection = PageCollectionMock.Instance();

            foreach (var id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    collection._items.Add(RefreshSingleMethod(id));
                }
            }

            collection.PageSize = ids.Count();
            return collection;
        }

        public UniqueObject RefreshSingleMethod(string id)
        {
            GotFromDatabase = true;

            var obj = new UniqueObject
            {
                Version = CacheCollectionTests._sourceData[id].Version,
                ID = CacheCollectionTests._sourceData[id].ID
            };


            return obj;
        }
    }

    public class ReaderThread
    {
        public EventWaitHandle Finshed = new EventWaitHandle(false, EventResetMode.AutoReset);

        public string GetCacheIDs(out int count)
        {
            var rand = new Random(DateTime.Now.Millisecond);
            var items = new List<string>();

            var pageSize = rand.Next(1, CacheCollectionTests._sourceData.Count);

            for (var i = 0; i < pageSize; i++)
            {
                var index = rand.Next(0, CacheCollectionTests._sourceData.Count - 1);

                items.Add(index.ToString());
            }

            count = items.Count;

            return string.Join(",", items.ToArray());
            ;
        }

        public void Run()
        {
            var q = new MockQuery();

            var counter = 0;

            while (!CacheCollectionTests.EndEvent.WaitOne(100) && counter < CacheCollectionTests._itterations)
            {
                var count = 0;
                q.CacheKey = GetCacheIDs(out count);
                q.PageSize = count;

                var results = Injector.Instance.Get<TestCachedCollection>().Get(q);

                var dt = CacheCollectionTests.StartTime;
                CacheCollectionTests.StartTime = DateTime.Now;

                CacheCollectionTests._AverageDuration.IncrementBy(CacheCollectionTests.StartTime.Ticks - dt.Ticks);
                CacheCollectionTests._AverageDurationBase.Increment();
                CacheCollectionTests._TotalReadOperations.Increment();

                if (counter % 24 == 0)
                    Injector.Instance.Get<TestCachedCollection>().Clear();

                counter++;
            }

            Finshed.Set();
        }
    }

    public class IntegrityThread
    {
        public void Run()
        {
            //Just let the other threads complete
            Thread.Sleep(100);

            var q = new MockQuery();
            q.CacheKey = string.Join(",", CacheCollectionTests._sourceData.Keys.ToArray());

            var results = Injector.Instance.Get<TestCachedCollection>().Get(q);

            foreach (var uniqueObject in results.Items)
            {
                if (uniqueObject.Version != CacheCollectionTests._sourceData[uniqueObject.ID.ToString()].Version)
                {
                    Console.WriteLine("Object failed integrity test");

                    Console.WriteLine(uniqueObject.Version + ":" + Injector.Instance.Get<TestCachedCollection>().Get(uniqueObject.CacheID).Version);
                }
            }

            Console.WriteLine("integrity test complete");
        }
    }

    public class WriterThread
    {
        public EventWaitHandle Finshed = new EventWaitHandle(false, EventResetMode.AutoReset);

        public void Run()
        {
            var counter = 0;

            while (!CacheCollectionTests.EndEvent.WaitOne(100) && counter < CacheCollectionTests._itterations)
            {
                var rand = new Random(DateTime.Now.Millisecond);

                var index = rand.Next(0, CacheCollectionTests._sourceData.Count - 1);

                var obj = CacheCollectionTests._sourceData[index.ToString()];

                obj.Version++;

                //System.Console.WriteLine(obj.ID + ":" + obj.Version);

                Injector.Instance.Get<TestCachedCollection>().Add(obj);

                CacheCollectionTests._TotalWriteOperations.Increment();

                counter++;
            }

            Finshed.Set();
        }
    }

    public class CacheCollectionTests
    {
        public static EventWaitHandle EndEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "ExitEvent");

        public static int _itterations = 9999;
        public static object TestIntegrityLock = new object();

        public static ThreadSafeDictionary<string, UniqueObject> _sourceData =
            new ThreadSafeDictionary<string, UniqueObject>();

        public static DateTime StartTime = DateTime.Now;

        public static PerformanceCounter _AverageDuration;
        public static PerformanceCounter _AverageDurationBase;
        public static PerformanceCounter _TotalReadOperations;
        public static PerformanceCounter _TotalWriteOperations;

        protected void CreateSeedObjects()
        {
            //Create a collection of 10000 unique objects
            for (var i = 0; i < 1000; i++)
            {
                var newObj = new UniqueObject {ID = i, Version = 1};
                _sourceData.Add(i.ToString(), newObj);
            }
        }

        public void ExecuteTests()
        {
            CreatePerformanceCounterCategory();

            CreateSeedObjects();

               // 1. Create a new Simple Injector container
         Container container = new Container();

            // 2. Configure the container (register)
            container.Register<TestCachedCollection, TestCachedCollection>(Lifestyle.Singleton);
            container.Register<ICache, MockCache>(Lifestyle.Singleton);
            container.Register<IPagedCollectionFactory, PagedCollectionFactoryMock>(Lifestyle.Singleton);

            // 3. Optionally verify the container's configuration.
            container.Verify();

            Injector.Instance.SetContainer(container);

            var threads = new List<EventWaitHandle>();

            Thread thread;
            //create 100 threads that are performing all of the queries randomly
            for (var i = 0; i < 50; i++)
            {
                var thr = new ReaderThread();

                Thread.Sleep(400);

                threads.Add(thr.Finshed);

                thread = new Thread(thr.Run);

                thread.Start();
            }

            //Create 5 threads that are writting new objects
            for (var i = 0; i < 14; i++)
            {
                var thr = new WriterThread();

                Thread.Sleep(400);

                threads.Add(thr.Finshed);

                thread = new Thread(thr.Run);

                thread.Start();
            }

            Console.WriteLine("Waiting for key press");
            Console.ReadKey(true);

            EndEvent.Set();

            WaitHandle.WaitAll(threads.ToArray());

            var th = new IntegrityThread();
            thread = new Thread(th.Run);
            thread.Start();

            //Cache lifetime tests
            TestCachedCollection lifetimeTest = Injector.Instance.Get<TestCachedCollection>();

            lifetimeTest.GotFromDatabase = false;

            lifetimeTest.Get("2");

            lifetimeTest.GotFromDatabase = false;

            lifetimeTest.Get("2");

            if (lifetimeTest.GotFromDatabase == false)
            {
                Console.WriteLine("Should have used cache");
            }

            System.Threading.SpinWait.SpinUntil(() => false,60 * 100);

            lifetimeTest.GotFromDatabase = false;

            lifetimeTest.Get("2");

            if (lifetimeTest.GotFromDatabase == true)
            {
                Console.WriteLine("Should have not used cache");
            }

            var q = new MockQuery();

            var count = 4;
            q.CacheKey = "1,2,3,4";
            q.PageSize = count;

            var results = Injector.Instance.Get<TestCachedCollection>().Get(q);

            lifetimeTest.GotFromDatabase = false;

            results = Injector.Instance.Get<TestCachedCollection>().Get(q);

            if (lifetimeTest.GotFromDatabase == false)
            {
                Console.WriteLine("Should have used cache collection");
            }

            System.Threading.SpinWait.SpinUntil(() => false, 120 * 100);

            lifetimeTest.GotFromDatabase = false;

            results = Injector.Instance.Get<TestCachedCollection>().Get(q);

            if (lifetimeTest.GotFromDatabase == true)
            {
                Console.WriteLine("Should have not used cache collection");
            }

            Console.WriteLine("Waiting for key press");
            Console.ReadKey(true);
        }

        public void CreatePerformanceCounterCategory()
        {
            if (!PerformanceCounterCategory.Exists("CacheCollectionMetrics"))
            {
                var counters = new CounterCreationDataCollection();

                var totalOps = new CounterCreationData
                {
                    CounterName = "# operations reads executed per second",
                    CounterHelp = "Total number of read operations executed per second",
                    CounterType = PerformanceCounterType.RateOfCountsPerSecond32
                };
                counters.Add(totalOps);

                var totalWritesOps = new CounterCreationData
                {
                    CounterName = "# operations writes executed per second",
                    CounterHelp =
                        "Total number of write operations executed per second",
                    CounterType = PerformanceCounterType.RateOfCountsPerSecond32
                };
                counters.Add(totalWritesOps);

                // 3. counter for counting average time per operation:
                //                 PerformanceCounterType.AverageTimer32
                var avgDuration = new CounterCreationData
                {
                    CounterName = "average time per operation",
                    CounterHelp = "Average duration per operation execution",
                    CounterType = PerformanceCounterType.AverageTimer32
                };
                counters.Add(avgDuration);

                // 4. base counter for counting average time
                //         per operation: PerformanceCounterType.AverageBase
                var avgDurationBase = new CounterCreationData
                {
                    CounterName = "average time per operation base",
                    CounterHelp = "Average duration per operation execution base",
                    CounterType = PerformanceCounterType.AverageBase
                };
                counters.Add(avgDurationBase);

                PerformanceCounterCategory.Create("CacheCollectionMetrics", "Cache Collection Counters",
                    PerformanceCounterCategoryType.Unknown, counters);
            }

            _TotalReadOperations = new PerformanceCounter
            {
                CategoryName = "CacheCollectionMetrics",
                CounterName = "# operations reads executed per second",
                MachineName = ".",
                ReadOnly = false
            };

            _TotalWriteOperations = new PerformanceCounter
            {
                CategoryName = "CacheCollectionMetrics",
                CounterName = "# operations writes executed per second",
                MachineName = ".",
                ReadOnly = false
            };

            _AverageDuration = new PerformanceCounter
            {
                CategoryName = "CacheCollectionMetrics",
                CounterName = "average time per operation",
                MachineName = ".",
                ReadOnly = false
            };

            _AverageDurationBase = new PerformanceCounter
            {
                CategoryName = "CacheCollectionMetrics",
                CounterName = "average time per operation base",
                MachineName = ".",
                ReadOnly = false
            };
        }

        public static int Main()
        {
            var test = new CacheCollectionTests();

            test.ExecuteTests();

            return 0;
        }
    }
}