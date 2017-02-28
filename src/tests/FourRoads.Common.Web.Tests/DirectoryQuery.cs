using System;
using System.Data.SqlClient;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Web.Tests.Queries
{
    public class DirectoryQuery : PagedQueryBase
    {
        [CacheKey("Id")]
        public int? Id { get; set; }

        public uint PageIndex { get; set; }
        public int PageSize { get; set; }
        public SortOrder SortOrder { get; set; }
        public override string CacheKey {
            get { return "test"; }
        }
        public bool UseCache { get; set; }
    }
}