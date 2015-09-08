using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using FourRoads.Common.Interfaces;
using Ninject.Modules;

namespace FourRoads.Common.Web.Tests
{
	public class UniqueObject : ICacheable
	{
		public int ID;
		public int Version;

		private int _cacheRefreshInterval = 2;

		#region ICacheable Members

		public string CacheID
		{
			get { return ID.ToString(); }
		}

		public int CacheRefreshInterval
		{
			get { return _cacheRefreshInterval; }
		}

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

		public static PageCollectionMock Instance()
		{
			return new PageCollectionMock();
		}

		public uint PageIndex
		{
			get { return 0; }
		}

		int IPagedCollection<UniqueObject>.PageSize
		{
			get { return PageSize; }
		}

		public int PageSize
		{
			get { return _pageSize; }
			set { _pageSize = value; }
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
	}

	public class Bindings : NinjectModule
	{
		public override void Load()
		{
			Bind<ICache>().To<MockCache>();
			Bind<IPagedCollection<UniqueObject>>().To<PageCollectionMock>();
		}
	}

	public class MockCache : ICache
	{
		private Cache _cache = HttpRuntime.Cache;

		public void Insert(ICacheable value)
		{
			Insert(value.CacheID, value, null, value.CacheRefreshInterval);
		}

		public void Insert(ICacheable value, string[] additionalTags)
		{
			Insert(value.CacheID, value, null, value.CacheRefreshInterval);
		}

		public T Get<T>(string key)
		{
			return (T) _cache[key];
		}

		public void Remove(string key)
		{
			_cache.Remove(key);
		}

		public void RemoveByPattern(string pattern)
		{
			throw new NotImplementedException();
		}

		public void Insert(string key, object value, CacheDependency dependencies, int absoluteExpiration,
		                   CacheItemPriority priority)
		{
			_cache.Insert(key, value, dependencies, DateTime.Now + new TimeSpan(0, 0, 0, absoluteExpiration), TimeSpan.Zero,
			              priority, null);
		}

		public void Insert(string key, object value, CacheDependency dependencies, int absoluteExpiration)
		{
			_cache.Insert(key, value, dependencies, DateTime.Now + new TimeSpan(0, 0, 0, absoluteExpiration), TimeSpan.Zero,
			              CacheItemPriority.Normal, null);
		}

		#region ICache Members


		public void Insert(string key, object value)
		{
			_cache.Insert(key, value);
		}

		public void Insert(string key, object value, string[] tags)
		{
			_cache.Insert(key, value);
		}

		public void Insert(string key, object value, TimeSpan timeout)
		{
			_cache.Insert(key, value, null, DateTime.Now + timeout, Cache.NoSlidingExpiration);
		}

		public void Insert(string key, object value, string[] tags, TimeSpan timeout)
		{
			_cache.Insert(key, value, null, DateTime.Now + timeout, Cache.NoSlidingExpiration);
		}

		public void Insert(string key, object value, string[] tags, TimeSpan timeout, CacheScopeOption scope)
		{
			_cache.Insert(key, value, null, DateTime.Now + timeout, Cache.NoSlidingExpiration);
		}

		public object Get(string key)
		{
			return _cache.Get(key);
		}

		public void RemoveByTags(string[] tags)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class MockQuery : IPagedQueryV2
	{
		public uint PageIndex { get; set; }
		public int PageSize { get; set; }
		public SortOrder SortOrder { get; set; }
		public string CacheKey { get; set; }
		public bool UseCache { get; set; }
	}

	public class TestCachedCollection : CachedCollection<UniqueObject, MockQuery, TestCachedCollection>
	{
		public TestCachedCollection()
		{
			GetDataQuery = RefreshQueryMethod;
			GetDataSingle = RefreshSingleMethod;
		}

		public IPagedCollection<UniqueObject> RefreshQueryMethod(MockQuery query)
		{
			string[] ids = query.CacheKey.Split(new[] {','});

			PageCollectionMock collection = PageCollectionMock.Instance();

			foreach (string id in ids)
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
			UniqueObject obj = new UniqueObject
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
			Random rand = new Random(DateTime.Now.Millisecond);
			List<string> items = new List<string>();

			int pageSize = rand.Next(1, CacheCollectionTests._sourceData.Count);

			for (int i = 0; i < pageSize; i++)
			{
				int index = rand.Next(0, CacheCollectionTests._sourceData.Count - 1);

				items.Add(index.ToString());
			}

			count = items.Count;

			return string.Join(",", items.ToArray());
			;
		}

		public void Run()
		{
			MockQuery q = new MockQuery();

			int counter = 0;

			while (!CacheCollectionTests.EndEvent.WaitOne(100) && counter < CacheCollectionTests._itterations)
			{
				int count = 0;
				q.CacheKey = GetCacheIDs(out count);
				q.PageSize = count;

				IPagedCollection<UniqueObject> results = TestCachedCollection.Cache().Get(q);

				DateTime dt = CacheCollectionTests.StartTime;
				CacheCollectionTests.StartTime = DateTime.Now;

				CacheCollectionTests._AverageDuration.IncrementBy(CacheCollectionTests.StartTime.Ticks - dt.Ticks);
				CacheCollectionTests._AverageDurationBase.Increment();
				CacheCollectionTests._TotalReadOperations.Increment();

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

			MockQuery q = new MockQuery();
			q.CacheKey = string.Join(",", CacheCollectionTests._sourceData.Keys.ToArray());

			IPagedCollection<UniqueObject> results = TestCachedCollection.Cache().Get(q);

			foreach (UniqueObject uniqueObject in results.Items)
			{
				if (uniqueObject.Version != CacheCollectionTests._sourceData[uniqueObject.ID.ToString()].Version)
				{
					Console.WriteLine("Object failed integrity test");

					Console.WriteLine(uniqueObject.Version + ":" + TestCachedCollection.Cache().Get(uniqueObject.CacheID).Version);
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
			int counter = 0;

			while (!CacheCollectionTests.EndEvent.WaitOne(100) && counter < CacheCollectionTests._itterations)
			{
				Random rand = new Random(DateTime.Now.Millisecond);

				int index = rand.Next(0, CacheCollectionTests._sourceData.Count - 1);

				UniqueObject obj = CacheCollectionTests._sourceData[index.ToString()];

				obj.Version++;

				//System.Console.WriteLine(obj.ID + ":" + obj.Version);

				TestCachedCollection.Cache().Add(obj);

				CacheCollectionTests._TotalWriteOperations.Increment();

				counter++;
			}

			Finshed.Set();
		}
	}

	public class CacheCollectionTests
	{
		public static System.Threading.EventWaitHandle EndEvent = new EventWaitHandle(false, EventResetMode.ManualReset,
		                                                                              "ExitEvent");

		public static int _itterations = 9999;
		public static object TestIntegrityLock = new object();

		public static ThreadSafeDictionary<string, UniqueObject> _sourceData =
			new ThreadSafeDictionary<string, UniqueObject>();

		public static DateTime StartTime = DateTime.Now;

		protected void CreateSeedObjects()
		{
			//Create a collection of 10000 unique objects
			for (int i = 0; i < 1000; i++)
			{
				UniqueObject newObj = new UniqueObject() {ID = i, Version = 1};
				_sourceData.Add(i.ToString(), newObj);
			}
		}

		public void ExecuteTests()
		{
			CreatePerformanceCounterCategory();
			//Create a mock cache class that has items that expire after 10 seconds
			InjectionSettings settings = new InjectionSettings("Test", string.Empty, new INinjectModule[] {new Bindings()});
			Injector.LoadBindings(settings);

			CreateSeedObjects();

			List<EventWaitHandle> threads = new List<EventWaitHandle>();

			Thread thread;
			//create 100 threads that are performing all of the queries randomly
			for (int i = 0; i < 50; i++)
			{
				ReaderThread thr = new ReaderThread();

				Thread.Sleep(400);

				threads.Add(thr.Finshed);

				thread = new Thread(thr.Run);

				thread.Start();
			}

			//Create 5 threads that are writting new objects
			for (int i = 0; i < 14; i++)
			{
				WriterThread thr = new WriterThread();

				Thread.Sleep(400);

				threads.Add(thr.Finshed);

				thread = new Thread(thr.Run);

				thread.Start();
			}

			Console.WriteLine("Waiting for key press");
			Console.ReadKey(true);

			EndEvent.Set();

			WaitHandle.WaitAll(threads.ToArray());

			IntegrityThread th = new IntegrityThread();
			thread = new Thread(th.Run);
			thread.Start();

			Console.WriteLine("Waiting for key press");
			Console.ReadKey(true);
		}

		public static PerformanceCounter _AverageDuration;
		public static PerformanceCounter _AverageDurationBase;
		public static PerformanceCounter _TotalReadOperations;
		public static PerformanceCounter _TotalWriteOperations;

		public void CreatePerformanceCounterCategory()
		{
			if (!PerformanceCounterCategory.Exists("CacheCollectionMetrics"))
			{
				CounterCreationDataCollection counters = new CounterCreationDataCollection();

				CounterCreationData totalOps = new CounterCreationData
				                               	{
				                               		CounterName = "# operations reads executed per second",
				                               		CounterHelp = "Total number of read operations executed per second",
				                               		CounterType = PerformanceCounterType.RateOfCountsPerSecond32
				                               	};
				counters.Add(totalOps);

				CounterCreationData totalWritesOps = new CounterCreationData
				                                     	{
				                                     		CounterName = "# operations writes executed per second",
				                                     		CounterHelp =
				                                     			"Total number of write operations executed per second",
				                                     		CounterType = PerformanceCounterType.RateOfCountsPerSecond32
				                                     	};
				counters.Add(totalWritesOps);

				// 3. counter for counting average time per operation:
				//                 PerformanceCounterType.AverageTimer32
				CounterCreationData avgDuration = new CounterCreationData
				                                  	{
				                                  		CounterName = "average time per operation",
				                                  		CounterHelp = "Average duration per operation execution",
				                                  		CounterType = PerformanceCounterType.AverageTimer32
				                                  	};
				counters.Add(avgDuration);

				// 4. base counter for counting average time
				//         per operation: PerformanceCounterType.AverageBase
				CounterCreationData avgDurationBase = new CounterCreationData
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
			CacheCollectionTests test = new CacheCollectionTests();

			test.ExecuteTests();

			return 0;
		}
	}
}
