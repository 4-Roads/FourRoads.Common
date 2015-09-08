using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace FourRoads.Common.Extensions
{

    public static class ExtensionsForXml
    {
        public static XmlAttribute RequiredAttribute(this System.Xml.XmlNode node, string name)
        {
            XmlAttribute attribute = node.Attributes[name];
            if (attribute == null)
            {
                throw new Exception(string.Format("The '{0}' node does not have the required attribute '{1}'.", node.Name, name));
            }
            return attribute;
        }

        public static XmlNode RequiredNode(this System.Xml.XmlNode node, string name)
        {
            XmlNode requiredNode = node.SelectSingleNode(name);
            if (requiredNode == null)
            {
                throw new Exception(string.Format("The '{0}' node does not have the required child node '{1}'.", node.Name, name));
            }
            return requiredNode;
        }

        public static string AttributeOrChildNodeValue(this System.Xml.XmlNode node, string name, bool isRequired)
        {
            string value = null;
            XmlAttribute attribute = node.Attributes[name];
            if (attribute != null)
                value = attribute.Value;
            XmlNode valueNode = node.SelectSingleNode(name);
            if (node != null)
                value = node.InnerText;
            if (value == null && isRequired)
                throw new Exception(string.Format("The '{0}' node does not have the required attribute or child node '{1}'.", node.Name, name));
            return value;
        }

        public static string AttributeValueOrNull(this System.Xml.XmlNode node, string name)
        {
            XmlAttribute attribute = node.Attributes[name];
            return (attribute == null ? null : attribute.Value);
        }

        public static T AttributeValue<T>(this System.Xml.XmlNode node, string name, T defaultValue)
        {
            string value = node.AttributeValueOrNull(name);
            if (value == null)
                return defaultValue;
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
                return (T)converter.ConvertFromString(value);
            else
                throw new Exception(string.Format("Cannot convert attribute value '{0}' to type '{1}'", value, typeof(T).Name));             
        }

    }
}
