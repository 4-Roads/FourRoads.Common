using System;

namespace FourRoads.Common.Extensions
{
    /// <summary>
    ///     DateTime extension methods
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        ///     Converts the value of the current <see cref="DateTime" /> object to its equivalent string representation using the
        ///     ISO-8601 format.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>Should be used for any literal DateTime representations in dynamic SQL queries.</remarks>
        /// <returns></returns>
        public static string ToSqlString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }
    }
}