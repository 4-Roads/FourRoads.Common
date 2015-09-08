// //------------------------------------------------------------------------------
// // <copyright company="Four Roads LLC">
// //     Copyright (c) Four Roads LLC.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.Data.SqlClient;

#endregion

namespace FourRoads.Common.Interfaces
{
    [Obsolete("Use IPagedQueryV2")]
    public interface IPagedQuery
    {
        uint PageIndex { get; set; }
        int PageSize { get; set; }
		SortOrder SortOrder { get; set; }
        string CacheKey { get; }
    }

    public interface IPagedQueryV2 
    {
        uint PageIndex { get; set; }
        int PageSize { get; set; }
        SortOrder SortOrder { get; set; }
        string CacheKey { get; }
        bool UseCache { get; set; }
    }
}