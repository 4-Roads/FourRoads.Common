using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace FourRoads.Common
{
    /// <summary>
    ///     A chainable query string helper class.
    ///     Example usage :
    ///     string strQuery = QueryStringBuilder.Current.Add("id", "179").ToString();
    ///     string strQuery = new QueryStringBuilder().Add("id", "179").ToString();
    /// </summary>
    public class QueryStringBuilder : NameValueCollection
    {
        public QueryStringBuilder()
        {
        }

        public QueryStringBuilder(string value)
        {
            if (!TryParse(value, this))
                throw new InvalidOperationException("Value does not contain a valid QueryString");
        }

        /// <summary>
        ///     overrides the default
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the associated decoded value for the specified name</returns>
        public new string this[string name]
        {
            get { return HttpUtility.UrlDecode(base[name]); }
        }

        /// <summary>
        ///     overrides the default indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns>the associated decoded value for the specified index</returns>
        public new string this[int index]
        {
            get { return HttpUtility.UrlDecode(base[index]); }
        }

        /// <summary>
        ///     add a name value pair to the collection
        /// </summary>
        /// <param name="name">the name</param>
        /// <param name="value">the value associated to the name</param>
        /// <returns>the QueryStringBuilder object </returns>
        public new QueryStringBuilder Add(string name, string value)
        {
            return Add(name, value, false);
        }

        /// <summary>
        ///     adds a name value pair to the collection
        /// </summary>
        /// <param name="name">the name</param>
        /// <param name="value">the value associated to the name</param>
        /// <param name="isUnique">true if the name is unique within the querystring. This allows us to override existing values</param>
        /// <returns>the QueryStringBuilder object </returns>
        public QueryStringBuilder Add(string name, string value, bool isUnique)
        {
            var existingValue = base[name];
            if (string.IsNullOrEmpty(existingValue)) base.Add(name, HttpUtility.UrlEncode(value));
            else if (isUnique) base[name] = HttpUtility.UrlEncode(value);
            else base[name] += "," + HttpUtility.UrlEncode(value);
            return this;
        }

        /// <summary>
        ///     removes a name value pair from the querystring collection
        /// </summary>
        /// <param name="name">name of the querystring value to remove</param>
        /// <returns>the QueryStringBuilder object</returns>
        public new QueryStringBuilder Remove(string name)
        {
            var existingValue = base[name];
            if (!string.IsNullOrEmpty(existingValue)) base.Remove(name);
            return this;
        }

        /// <summary>
        ///     clears the collection
        /// </summary>
        /// <returns>the QueryStringBuilder object </returns>
        public QueryStringBuilder Reset()
        {
            Clear();
            return this;
        }

        /// <summary>
        ///     checks if a name already exists within the query string collection
        /// </summary>
        /// <param name="name">the name to check</param>
        /// <returns>a boolean if the name exists</returns>
        public bool Contains(string name)
        {
            var existingValue = base[name];
            return !string.IsNullOrEmpty(existingValue);
        }

        public QueryStringBuilder Append(QueryStringBuilder builder)
        {
            for (var i = 0; i < builder.Keys.Count; i++)
            {
                var key = builder.Keys[i];
                if (!string.IsNullOrEmpty(key))
                {
                    Add(key, builder[key]);
                }
            }
            return builder;
        }

        public Uri AppendTo(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                return new Uri(AppendTo(uri.ToString()), UriKind.RelativeOrAbsolute);

            string query = null;
            if (!string.IsNullOrEmpty(uri.Query))
                query = new QueryStringBuilder(uri.Query).Append(this).ToString();
            else
                query = ToString();

            return new Uri(uri.GetLeftPart(UriPartial.Path)
                           + (string.IsNullOrEmpty(query) ? "" : "?" + query));
        }

        public string AppendTo(string url)
        {
            var query = new QueryStringBuilder(url).Append(this);

            var path = url;
            if (!string.IsNullOrEmpty(url))
            {
                if (url.Contains("?"))
                    path = url.Substring(0, url.IndexOf("?"));

                if (!string.IsNullOrEmpty(path))
                    return path + (query.Count == 0 ? "" : "?" + query);
            }
            return query.ToString();
        }

        /// <summary>
        ///     outputs the querystring object to a string
        /// </summary>
        /// <returns>the encoded querystring as it would appear in a browser</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < Keys.Count; i++)
            {
                if (!string.IsNullOrEmpty(Keys[i]))
                {
                    foreach (var val in base[Keys[i]].Split(','))
                        builder.Append(builder.Length == 0 ? "" : "&")
                            .Append(HttpUtility.UrlEncode(Keys[i]))
                            .Append("=").Append(val);
                }
            }
            return builder.ToString();
        }

        #region Static Methods

        public static QueryStringBuilder Current
        {
            get
            {
                var qb = new QueryStringBuilder();
                if (HttpContext.Current != null)
                {
                    var query = HttpContext.Current.Request.QueryString;
                    for (var i = 0; i < query.Keys.Count; i++)
                    {
                        var key = query.Keys[i];
                        if (!string.IsNullOrEmpty(key))
                            qb.Add(key, query[key]);
                    }
                }
                return qb;
            }
        }

        /// <summary>
        ///     Returns a <see cref="QueryStringBuilder" /> object based on a string
        /// </summary>
        /// <param name="value">the string to parse</param>
        /// <returns>
        ///     the QueryStringBuilder object
        /// </returns>
        public static QueryStringBuilder Parse(string value)
        {
            return new QueryStringBuilder(value);
        }

        public static bool TryParse(string value, QueryStringBuilder builder)
        {
            if (builder == null)
                builder = new QueryStringBuilder();

            if (value == null)
                return false;

            if (value == string.Empty)
                return true;

            string query = null;
            if (value.StartsWith("?") || value.StartsWith("&"))
                query = value.Substring(1);
            else
                query = value.Contains("?") ? value.Substring(value.IndexOf("?") + 1) : value;

            if (string.IsNullOrEmpty(query))
                return true;

            foreach (var keyValuePair in query.Split('&'))
            {
                if (string.IsNullOrEmpty(keyValuePair)) continue;
                var split = keyValuePair.Split('=');
                builder.Add(HttpUtility.UrlDecode(split[0]), split.Length == 2 ? HttpUtility.UrlDecode(split[1]) : "");
            }

            if (!string.IsNullOrEmpty(query) && builder.Count == 0)
                return false;

            return true;
        }

        #endregion
    }
}