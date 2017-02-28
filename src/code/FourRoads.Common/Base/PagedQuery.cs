// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System.Data.SqlClient;
using FourRoads.Common.Interfaces;

#endregion

namespace FourRoads.Common
{
    public abstract class PagedQueryBase : IPagedQueryV2
    {
        protected PagedQueryBase()
        {
            UseCache = true;
        }

        public virtual uint PageIndex { get; set; }

        public virtual int PageSize { get; set; } = int.MaxValue;

        public virtual SortOrder SortOrder { get; set; } = SortOrder.Ascending;

        public abstract string CacheKey { get; }

        public bool UseCache { get; set; }
    }
}