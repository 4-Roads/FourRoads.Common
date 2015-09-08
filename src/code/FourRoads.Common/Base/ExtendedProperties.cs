using System;
using System.Collections; 
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace FourRoads.Common
{
    /// <summary>
    /// ExtendedProperties class provides a way to create an xml serialized list of custom properties to extend your
    /// business objects with
    /// </summary>
    [Serializable, XmlRoot("Properties")]
    public class ExtendedProperties : IEnumerable<ExtendedProperties.Property>
    {

        #region Private members

        private Dictionary<string, Property> _Properties = new Dictionary<string, Property>();
        private List<Property> _EnumeratedList;

        #endregion

        public void Add(object value)
        {
            if (value is Property)
            {
                Add((Property)value);
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

        protected Property Get(string key)
        {
            if (_Properties.ContainsKey(key))
                return _Properties[key];
            return null;
        }

        public T Get<T>(string key)
        {
            T value = default(T);
            TryGet<T>(key, out value);
            return value;
        }

        public T Get<T>(string key, T defaultValue)
        {
            T value = default(T);
            if (TryGet<T>(key, out value))
                return value;
            else 
                return defaultValue; 
        }

        public bool TryGet<T>(string key, out T value)
        {
            value = default(T);
            Property prop = Get(key);
            if (prop != null && prop.Value is T)
            {
                value = (T)prop.Value;
                return true;
            }
            return false;
        }

        #region IEnumerable<ExtendedProperty> Members

        public IEnumerator<Property> GetEnumerator()
        {
            _EnumeratedList = _Properties.Values.ToList<Property>();
            return _EnumeratedList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Property Class

        [Serializable, XmlRoot("Property")]
        public class Property
        {
            public Property() { }
            public Property(string key, object value)
            {
                _Key = key;
                _Value = value;
            }
            private string _Key;
            [XmlAttribute("Key")]
            public string Key
            {
                get { return _Key; }
                set { _Key = value; }
            }
            private object _Value;
            public object Value
            {
                get { return _Value; }
                set { _Value = value; }
            }
        }

        #endregion

    }
}
