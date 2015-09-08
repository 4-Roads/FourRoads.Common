// //------------------------------------------------------------------------------
// // <copyright company="Four Roads LLC">
// //     Copyright (c) Four Roads LLC.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;

#endregion

namespace FourRoads.Common.Interfaces
{
    [Obsolete("ISqlDataChecker is obsolete. Please use FourRoads.Common.Sql.ISqlDataHelper instead")] 
    public interface ISqlDataChecker
    {
        object MakeSafeValue(DateTime data, bool returnNull);
        object MakeSafeValue(int data);
        object MakeSafeValue(long data);
        string MakeSafeQueryString(string data);
        object ObjectOrNull(object value);
        object StringOrNull(string value);
    }
}