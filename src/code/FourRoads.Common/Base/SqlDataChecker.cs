// //------------------------------------------------------------------------------
// // <copyright company="Four Roads LLC">
// //     Copyright (c) Four Roads LLC.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using FourRoads.Common.Interfaces;

#endregion

namespace FourRoads.Common
{
    [Obsolete("SqlDataChecker is obsolete. Please use FourRoads.Common.Sql.SqlDataHelper instead")]
    public class SqlDataChecker : ISqlDataChecker
    {
        #region ISqlDataChecker Members

        public object MakeSafeValue(DateTime date, bool returnNull)
        {
            if (date < (DateTime) SqlDateTime.MinValue)
            {
                if (returnNull)
                    return DBNull.Value;

                return (DateTime) SqlDateTime.MinValue;
            }
            else if (date > (DateTime) SqlDateTime.MaxValue)
            {
                if (returnNull)
                    return DBNull.Value;

                return (DateTime) SqlDateTime.MaxValue;
            }
            return date;
        }

        public object MakeSafeValue(int value)
        {
            if (value <= (int) SqlInt32.MinValue)
                return (int) SqlInt32.MinValue + 1;
            else if (value >= (int) SqlInt32.MaxValue)
                return (int) SqlInt32.MaxValue - 1;
            return value;
        }

        public object MakeSafeValue(long value)
        {
            if (value <= (long) SqlInt64.MinValue)
                return (long) SqlInt64.MinValue + 1;
            else if (value >= (long) SqlInt64.MaxValue)
                return (long) SqlInt64.MaxValue - 1;
            return value;
        }

        public string MakeSafeQueryString(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return string.Empty;

            // Do wild card replacements
            searchString = searchString.Replace("*", "%");

            // Remove known bad SQL characters
            searchString = Regex.Replace(searchString, "--|;|'|\"", " ", RegexOptions.Compiled | RegexOptions.Multiline);

            // Finally remove any extra spaces from the string
            searchString = Regex.Replace(searchString, " {1,}", " ",
                                         RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

            return searchString;
        }

        public object ObjectOrNull(object value)
        {
            if (value == null)
                return DBNull.Value;

            return value;
        }

        public object StringOrNull(string value)
        {
            if (string.IsNullOrEmpty(value))
                return DBNull.Value;

            return value;
        }

        
        #endregion
    }
}