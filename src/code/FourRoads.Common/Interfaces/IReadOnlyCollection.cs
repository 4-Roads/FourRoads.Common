// //------------------------------------------------------------------------------
// // <copyright company="Four Roads LLC">
// //     Copyright (c) Four Roads LLC.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System.Collections.Generic;

#endregion

namespace FourRoads.Common.Interfaces
{
    public interface IReadOnlyCollection<T>
    {
        int Count { get; }
        T this[int index] { get; }
        bool Contains(T item);
        void CopyTo(T[] source, int count);
        IEnumerator<T> GetEnumerator();
        int IndexOf(T item);
    }
}