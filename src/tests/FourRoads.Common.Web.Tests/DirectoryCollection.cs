using System;
using FourRoads.Common.Interfaces;
using FourRoads.Common.Web.Tests.Entities;
using FourRoads.Common.Web.Tests.Interfaces;
using FourRoads.Common.Web.Tests.Queries;

namespace FourRoads.Common.Web.Tests.Collections
{
    public class DirectoryCollection : CachedDataCollectionBase
        <Directory, DirectoryQuery, IDirectoryDataProvider>
    {
        public DirectoryCollection(IPagedCollectionFactory pagedCollectionFactory, ICache cacheProvider, IObjectFactory objectFactory) : base(pagedCollectionFactory, cacheProvider, objectFactory)
        {

        }

        #region Overrides of CachedDataCollectionBase<Directory,DirectoryQuery,IDirectoryDataProvider,DirectoryCollection>

        protected override void SetQuery(DirectoryQuery query, string cacheId)
        {
            query.Id = Convert.ToInt32(cacheId.Replace("", string.Empty));
        }

        #endregion
    }
}