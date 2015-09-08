using System;
using System.Linq;
using System.Collections.Generic;

namespace FourRoads.Common.Extensions
{
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Creates a string from the sequence by concatenating the result
		/// of the specified string selector function for each element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <param name="stringSelector">The string selector.</param>
		/// <returns>A concatenated string.</returns>
		public static string Join<T>(this IEnumerable<T> source, Func<T, string> stringSelector)
		{
			return source.Join(stringSelector, String.Empty);
		}

		/// <summary>
		/// Creates a string from the sequence by concatenating the result
		/// of the specified string selector function for each element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <param name="stringSelector">The string selector.</param>
		/// <param name="separator">The string which separates each concatenated item.</param>
		/// <returns>A concatenated string.</returns>
		public static string Join<T>(this IEnumerable<T> source, Func<T, string> stringSelector, string separator)
		{
			return string.Join(separator, source.Select(stringSelector));
		}
	}
}