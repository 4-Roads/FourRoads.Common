using System;
using System.ComponentModel;
using System.Xml;

namespace FourRoads.Common.Extensions
{
    public static class ExtensionsForXml
    {
        public static XmlAttribute RequiredAttribute(this XmlNode node, string name)
        {
            var attribute = node.Attributes[name];
            if (attribute == null)
            {
                throw new Exception(string.Format("The '{0}' node does not have the required attribute '{1}'.", node.Name, name));
            }
            return attribute;
        }

        public static XmlNode RequiredNode(this XmlNode node, string name)
        {
            var requiredNode = node.SelectSingleNode(name);
            if (requiredNode == null)
            {
                throw new Exception(string.Format("The '{0}' node does not have the required child node '{1}'.", node.Name, name));
            }
            return requiredNode;
        }

        public static string AttributeOrChildNodeValue(this XmlNode node, string name, bool isRequired)
        {
            string value = null;
            var attribute = node.Attributes[name];
            if (attribute != null)
                value = attribute.Value;
            var valueNode = node.SelectSingleNode(name);
            if (node != null)
                value = node.InnerText;
            if (value == null && isRequired)
                throw new Exception(string.Format("The '{0}' node does not have the required attribute or child node '{1}'.", node.Name, name));
            return value;
        }

        public static string AttributeValueOrNull(this XmlNode node, string name)
        {
            var attribute = node.Attributes[name];
            return attribute == null ? null : attribute.Value;
        }

        public static T AttributeValue<T>(this XmlNode node, string name, T defaultValue)
        {
            var value = node.AttributeValueOrNull(name);
            if (value == null)
                return defaultValue;
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
                return (T) converter.ConvertFromString(value);
            throw new Exception(string.Format("Cannot convert attribute value '{0}' to type '{1}'", value, typeof(T).Name));
        }
    }
}