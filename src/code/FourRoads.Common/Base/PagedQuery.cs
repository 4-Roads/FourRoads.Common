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
        private int _pageSize = int.MaxValue;
        private SortOrder _sortOrder = SortOrder.Ascending;

    	protected PagedQueryBase()
        {
            UseCache = true;
        }

        public virtual uint PageIndex { get; set; }

        public virtual int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public virtual SortOrder SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }

        public abstract string CacheKey { get; }

        public bool UseCache { get; set; }
    }
}