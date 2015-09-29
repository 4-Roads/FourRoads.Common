// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System.Collections.Generic;

#endregion

namespace FourRoads.Common.Interfaces
{
    public interface IPagedCollection<ContainerType>
    {
        uint PageIndex { get; }
        int PageSize { get; }
        IEnumerable<ContainerType> Items { get; }
        int TotalRecords { get; }
        IPagedCollection<TCast> Cast<TCast>();
    }
}