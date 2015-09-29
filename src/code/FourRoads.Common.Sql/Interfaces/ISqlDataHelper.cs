// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;

#endregion

namespace FourRoads.Common.Sql
{
    public enum WildCardLocation
    {
        Ignore = -3,
        Custom = -2,
        AtEnd = -1,
        AtStart = 0
    }

    public interface ISqlDataHelper
    {
        object MakeSafeValue(DateTime data, bool returnNull);
        object MakeSafeValue(int data);
        object MakeSafeValue(long data);
        string MakeSafeValue(string data);
        string MakeSafeQueryString(string data);
        string MakeSafeLikeQueryString(string fieldName, string searchString, WildCardLocation wildCardLocation);
        object ObjectOrNull(object value);
        object StringOrNull(string value);
        string SafeSqlDateTimeFormat(DateTime date);
        
        T GetValue<T>(object value);
        T GetValue<T>(object value, T defaultValue);

        int GetInt32(object value, int defaultValue);
        Int16 GetInt16(object value, Int16 defaultValue);
        long GetInt64(object value, long defaultValue);
        UInt32 GetUInt32(object value, UInt32 defaultValue);
        bool GetBoolean(object value, bool defaultValue);

        T XmlDeserialize<T>(object value);
        T XmlDeserialize<T>(object value, T defaultValue);
        string XmlSerialize<T>(T value);
        string XmlSerialize<T>(T value, string defaultValue);
    }
}