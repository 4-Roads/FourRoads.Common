// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using FourRoads.Common.Extensions;
using FourRoads.Common.Interfaces;

#endregion

namespace FourRoads.Common.Sql
{
    public class SqlDataHelper : IDataHelper
    {
        private IObjectFactory _objectFactory;
        public SqlDataHelper(IObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        #region ISqlDataChecker Members

        public object MakeSafeValue(DateTime date, bool returnNull)
        {
            if (date < (DateTime) SqlDateTime.MinValue)
            {
                if (returnNull)
                    return DBNull.Value;

                return (DateTime) SqlDateTime.MinValue;
            }
            if (date > (DateTime) SqlDateTime.MaxValue)
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
            if (value >= (int) SqlInt32.MaxValue)
                return (int) SqlInt32.MaxValue - 1;
            return value;
        }

        public object MakeSafeValue(long value)
        {
            if (value <= (long) SqlInt64.MinValue)
                return (long) SqlInt64.MinValue + 1;
            if (value >= (long) SqlInt64.MaxValue)
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
                    if ((int) wildCardLocation < searchString.Length)
                        searchString = searchString.Substring(0, (int) wildCardLocation) + "%" + searchString.Substring((int) wildCardLocation);
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
        ///     Convert dates to a dictionary sortable string
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string SafeSqlDateTimeFormat(DateTime date)
        {
            return date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.SortableDateTimePattern);
        }

        #endregion

        #region IDataHelper Members

        public T GetValue<T>(object value)
        {
            var defaultValue = default(T);
            return GetValue(value, defaultValue);
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
                            return (T) Enum.ToObject(typeof(T), (int) value);
                        var converter = new EnumConverter(typeof(T));
                        if (converter.CanConvertFrom(value.GetType()))
                            return (T) converter.ConvertFrom(value);
                    }
                    else
                    {
                        var converter = TypeDescriptor.GetConverter(typeof(T));
                        if (converter.CanConvertFrom(value.GetType()))
                            return (T) converter.ConvertFrom(value);
                        return (T) Convert.ChangeType(value, typeof(T));
                    }
                }
                catch
                {
                }
            }
            return defaultValue;
        }

        public int GetInt32(object value, int defaultValue)
        {
            if (Convert.IsDBNull(value) || value == null)
                return defaultValue;

            return value.ToInt32(defaultValue);
        }

        public short GetInt16(object value, short defaultValue)
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

        public uint GetUInt32(object value, uint defaultValue)
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

        #region IDataHelper Members

        public T XmlDeserialize<T>(object value)
        {
            return XmlDeserialize(value, default(T));
        }

        public T XmlDeserialize<T>(object value, T defaultValue)
        {
            var xml = GetValue<string>(value);

            if (string.IsNullOrEmpty(xml))
                return defaultValue;

            using (TextReader tr = new StringReader(xml))
            {
                using (XmlReader sr = XmlReader.Create(tr))
                {
                    return (T)_objectFactory.Get<XmlInjectedSerializationFactory>().Create(typeof(T)).Deserialize(sr);
                }
            }

        }

        public string XmlSerialize<T>(T value)
        {
            return XmlSerialize(value, null);
        }

        public string XmlSerialize<T>(T value, string defaultValue)
        {
            if (value == null)
                return defaultValue;
            StringBuilder sb = new StringBuilder(100);

            using (StringWriter sw = new StringWriter(sb))
            {
                _objectFactory.Get<XmlInjectedSerializationFactory>().Create(typeof(T)).Serialize(sw , value);
            }
            return sb.ToString();
        }

        #endregion
    }
}