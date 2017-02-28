using FourRoads.Common.Interfaces;
using FourRoads.Common.Web.Tests.Entities;
using FourRoads.Common.Web.Tests.Queries;

namespace FourRoads.Common.Web.Tests.Interfaces
{
    public interface IDirectoryManager : IManagerV2<Directory, DirectoryQuery, int>
    {
    }
}