using FourRoads.Common.Interfaces;
using FourRoads.Common.Web.Tests.Entities;
using FourRoads.Common.Web.Tests.Queries;

namespace FourRoads.Common.Web.Tests.Interfaces
{
    public interface IDirectoryDataProvider : IDataProvider<Directory, DirectoryQuery>
    {
    }
}