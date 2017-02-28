using System.Globalization;
using FourRoads.Common.Interfaces;
using FourRoads.Common.Web.Tests.Collections;
using FourRoads.Common.Web.Tests.Entities;
using FourRoads.Common.Web.Tests.Interfaces;
using FourRoads.Common.Web.Tests.Queries;

namespace FourRoads.Common.Web.Tests.Managers
{
    public class DirectoryManager :
        CachedManagerBase<Directory, DirectoryQuery, int, DirectoryCollection, IDirectoryDataProvider>, IDirectoryManager
    {
        #region Overrides of CachedManagerBase<Directory,DirectoryQuery,int,DirectoryCollection,IDirectoryDataProvider>

        protected override string FormatCacheId(int id)
        {
            return id.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        public DirectoryManager(IObjectFactory objectFactory) : base(objectFactory)
        {
        }
    }
}