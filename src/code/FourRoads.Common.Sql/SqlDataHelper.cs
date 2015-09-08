// //------------------------------------------------------------------------------
// // <copyright company="Four Roads LLC">
// //     Copyright (c) Four Roads LLC.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;  
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Reflection;
using FourRoads.Common.Extensions;
 
#endregion

namespace FourRoads.Common.Sql
{
    
    public class SqlDataHelper : ISqlDataHelper
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

        public string MakeSafeValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return Regex.Replace(value, "(')", "'$1", RegexOptions.Compiled | RegexOptions.Multiline);
        }

        public string MakeSafeQueryString(string searchString)
        {
            return MakeSafeValue(searchString);
        }

        public string MakeSafeLikeQueryString(string fieldName, string searchString, WildCardLocation wildCardLocation)
        {
            if (string.IsNullOrEmpty(searchString))
                return null;

            searchString = Regex.Replace(searchString, "(\\\\)", "\\$1", RegexOptions.Compiled | RegexOptions.Multiline);
            searchString = Regex.Replace(searchString, "(')", "'$1", RegexOptions.Compiled | RegexOptions.Multiline);

            // escape known bad SQL characters 
            searchString = Regex.Replace(searchString, "(--|;|\"|#|\\[|\\])", "\\$1", RegexOptions.Compiled | RegexOptions.Multiline);

            // Finally remove any extra spaces from the string
            searchString = Regex.Replace(searchString, " {1,}", " ",
                                         RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

            // Do wild card replacements
            
            switch (wildCardLocation)
            {
                case WildCardLocation.Ignore:
                    break;
                case WildCardLocation.Custom:
                    searchString = Regex.Replace(searchString, "(%)", "\\$1", RegexOptions.Compiled | RegexOptions.Multiline);
                    searchString = searchString.Replace("*", "%");
                    break;
                case WildCardLocation.AtEnd:
                    searchString = Regex.Replace(searchString, "(%)", "\\$1", RegexOptions.Compiled | RegexOptions.Multiline);
                    searchString += "%";
                    break;
                case WildCardLocation.AtStart:
                    searchString = Regex.Replace(searchString, "(%)", "\\$1", RegexOptions.Compiled | RegexOptions.Multiline);
                    searchString = "%" + searchString;
                    break;
                default:
                    searchString = Regex.Replace(searchString, "(%)", "\\$1", RegexOptions.Compiled | RegexOptions.Multiline);
                    if ((int)wildCardLocation < searchString.Length)                        
                        searchString = searchString.Substring(0, (int)wildCardLocation) + "%" + searchString.Substring((int)wildCardLocation);
                    else
                        searchString += "%";
                    break;
            }
            
            return fieldName + " LIKE '" + searchString + "' ESCAPE '\\' ";

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

        /// <summary>
        /// Convert dates to a dictionary sortable string
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string SafeSqlDateTimeFormat(DateTime date)
        {
            return date.ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.SortableDateTimePattern);
        }

        #endregion

        #region ISqlDataHelper Members

        public T GetValue<T>(object value)
        {
            T defaultValue = default(T);
            return GetValue<T>(value, defaultValue);
        }

        public T GetValue<T>(object value, T defaultValue)
        {
            if (value != null && value != DBNull.Value)
            {
                try
                {
                    if (typeof(T).IsEnum)
                    {
                        if (value is int)
                            return (T)Enum.ToObject(typeof(T), (int)value);
                        else
                        {
                            EnumConverter converter = new EnumConverter(typeof(T));
                            if (converter.CanConvertFrom(value.GetType()))
                                return (T)converter.ConvertFrom(value);
                        }
                    }
                    else
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                        if (converter.CanConvertFrom(value.GetType()))
                            return (T)converter.ConvertFrom(value);
                        else
                            return (T)Convert.ChangeType(value, typeof(T));
                    }
                }
                catch { }
            }
            return defaultValue;
        }

        public int GetInt32(object value , int defaultValue)
        {
            if (Convert.IsDBNull(value) || value == null)
                return defaultValue;

            return value.ToInt32(defaultValue);
        }

        public Int16 GetInt16(object value, Int16 defaultValue)
        {
            if (Convert.IsDBNull(value) || value == null)
                return defaultValue;

            return value.ToInt16(defaultValue);
        }

        public long GetInt64(object value, long defaultValue)
        {
            if (Convert.IsDBNull(value) || value == null)
                return defaultValue;

            return value.ToInt64(defaultValue);
        }

        public UInt32 GetUInt32(object value, UInt32 defaultValue)
        {
            if (Convert.IsDBNull(value) || value == null)
                return defaultValue;

            return value.ToUInt32(defaultValue);
        }


        public bool GetBoolean(object value, bool defaultValue)
        {
            if (Convert.IsDBNull(value) || value == null)
                return defaultValue;

            return value.ToBoolean(defaultValue);
        }

        #endregion

        #region ISqlDataHelper Members

        public T XmlDeserialize<T>(object value)
        {
            return XmlDeserialize<T>(value, default(T));
        }

        public T XmlDeserialize<T>(object value, T defaultValue)
        {
            string xml = GetValue<string>(value); 
            if (string.IsNullOrEmpty(xml))
                return defaultValue;

            return Injector.XmlDeserialize<T>(xml);
        }

        public string XmlSerialize<T>(T value)
        {
            return XmlSerialize<T>(value, null); 
        }

        public string XmlSerialize<T>(T value, string defaultValue)
        {
            if (value == null)
                return defaultValue;
 
            return Injector.XmlSerialize<T>(value); 
        }

        #endregion

    }
}