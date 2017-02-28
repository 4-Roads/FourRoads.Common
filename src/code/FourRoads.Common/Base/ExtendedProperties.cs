using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FourRoads.Common
{
    /// <summary>
    ///     ExtendedProperties class provides a way to create an xml serialized list of custom properties to extend your
    ///     business objects with
    /// </summary>
    [Serializable, XmlRoot("Properties")]
    public class ExtendedProperties : IEnumerable<ExtendedProperties.Property>
    {
        [XmlElement("Property")]
        public Property this[string key]
        {
            get
            {
                if (_Properties.ContainsKey(key))
                {
                    return _Properties[key];
                }
                return null;
            }
            set
            {
                if (value.Key.CompareTo(key) == 0)
                {
                    if (_Properties.ContainsKey(key))
                        _Properties[key] = value;
                    else
                        _Properties.Add(key, value);
                }
                else
                    throw new Exception("The Key parameter and Key value of ExtendedProperties.Property are different");
            }
        }

        #region IEnumerable<ExtendedProperty> Members

        public IEnumerator<Property> GetEnumerator()
        {
            _EnumeratedList = _Properties.Values.ToList();
            return _EnumeratedList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public void Add(object value)
        {
            if (value is Property)
            {
                Add((Property) value);
            }
            else
                throw new Exception("Value must be of type ExtendedProperties.Property");
        }

        public void Add(Property value)
        {
            this[value.Key] = value;
        }

        public void Set<T>(string key, T value)
        {
            this[key] = new Property(key, value);
        }

        protected Property Get(string key)
        {
            if (_Properties.ContainsKey(key))
                return _Properties[key];
            return null;
        }

        public T Get<T>(string key)
        {
            var value = default(T);
            TryGet(key, out value);
            return value;
        }

        public T Get<T>(string key, T defaultValue)
        {
            var value = default(T);
            if (TryGet(key, out value))
                return value;
            return defaultValue;
        }

        public bool TryGet<T>(string key, out T value)
        {
            value = default(T);
            var prop = Get(key);
            if (prop != null && prop.Value is T)
            {
                value = (T) prop.Value;
                return true;
            }
            return false;
        }

        #region Property Class

        [Serializable, XmlRoot("Property")]
        public class Property
        {
            private string _Key;
            private object _Value;

            public Property()
            {
            }

            public Property(string key, object value)
            {
                _Key = key;
                _Value = value;
            }

            [XmlAttribute("Key")]
            public string Key
            {
                get { return _Key; }
                set { _Key = value; }
            }

            public object Value
            {
                get { return _Value; }
                set { _Value = value; }
            }
        }

        #endregion

        #region Private members

        private Dictionary<string, Property> _Properties = new Dictionary<string, Property>();
        private List<Property> _EnumeratedList;

        #endregion
    }
}