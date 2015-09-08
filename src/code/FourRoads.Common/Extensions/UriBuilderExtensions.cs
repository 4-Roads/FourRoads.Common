using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;

namespace FourRoads.Common.Extensions
{
	public static class UriBuilderExtensions
	{
		/// <summary>
		/// Gets the query string key-value pairs of the URI.
		/// Note that the one of the keys may be null ("?123") and
		/// that one of the keys may be an empty string ("?=123").
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<string, string>> GetQueryParams(this UriBuilder uri)
		{
			return uri.ParseQuery().AsKeyValuePairs();
		}

		/// <summary>
		/// Parses the query string of the URI into a NameValueCollection.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static NameValueCollection ParseQuery(this UriBuilder uri)
		{
			return HttpUtility.ParseQueryString(uri.Query);
		}

		/// <summary>
		/// Removes the  specified query parameter key-value pair of the URI.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static UriBuilder RemoveQueryParam(this UriBuilder uri, string key)
		{
			NameValueCollection collection = uri.ParseQuery();

			// remove key-value pair
			collection.Remove(key);
			uri.Query = GetQueryString(collection);

			return uri;
		}

		/// <summary>
		/// Sets the specified query parameter key-value pair of the URI.
		/// If the key already exists, the value is overwritten.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static UriBuilder SetQueryParam(this UriBuilder uri, string key, string value)
		{
			var collection = uri.ParseQuery();

			// add (or replace existing) key-value pair
			collection.Set(key, value);
			uri.Query = GetQueryString(collection);

			return uri;
		}

		/// <summary>
		/// Converts the legacy NameValueCollection into a strongly-typed KeyValuePair sequence.
		/// </summary>
		private static IEnumerable<KeyValuePair<string, string>> AsKeyValuePairs(this NameValueCollection collection)
		{
			return collection.AllKeys.Select(key => new KeyValuePair<string, string>(key, collection.Get(key)));
		}

		private static string GetQueryString(NameValueCollection collection)
		{
			return collection
				.AsKeyValuePairs()
				.Join(pair => pair.Key == null ? pair.Value : string.Concat(pair.Key, "=", pair.Value), "&");
		}
	}

}