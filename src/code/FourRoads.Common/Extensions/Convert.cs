using System;

namespace FourRoads.Common.Extensions
{
    public static class ConvertExtensions
    {
        public static int ToInt32(this object value, int defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToInt32(value);
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
                return Convert.ToInt32(value, provider);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static short ToInt16(this object value, short defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToInt16(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static short ToInt16(this object value, IFormatProvider provider, short defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToInt16(value, provider);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static uint ToUInt32(this object value, uint defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToUInt32(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static uint ToUInt32(this object value, IFormatProvider provider, uint defaultValue)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToUInt32(value, provider);
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
                return Convert.ToInt64(value);
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
                return Convert.ToInt64(value, provider);
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
                return Convert.ToString(value);
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
                return Convert.ToString(value, provider);
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
                return Convert.ToBoolean(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}