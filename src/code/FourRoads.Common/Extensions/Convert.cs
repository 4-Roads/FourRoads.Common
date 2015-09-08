using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common.Extensions
{
    public static class ConvertExtensions 
    {
        public static int ToInt32(this object value , int defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static int ToInt32(this object value, IFormatProvider provider, int defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToInt32(value, provider);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static Int16 ToInt16(this object value, Int16 defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToInt16(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static Int16 ToInt16(this object value, IFormatProvider provider, Int16 defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToInt16(value, provider);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static UInt32 ToUInt32(this object value, UInt32 defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToUInt32(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static UInt32 ToUInt32(this object value, IFormatProvider provider, UInt32 defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToUInt32(value, provider);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static long ToInt64(this object value, long defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToInt64(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static long ToInt64(this object value, IFormatProvider provider, long defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToInt64(value, provider);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string ToString(this object value, string defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToString(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string ToString(this object value, IFormatProvider provider, string defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToString(value, provider);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static bool ToBoolean(this object value, bool defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return System.Convert.ToBoolean(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
