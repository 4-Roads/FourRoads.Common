// //------------------------------------------------------------------------------
// // <copyright company="Four Roads LLC">
// //     Copyright (c) Four Roads LLC.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FourRoads.Common
{
    public sealed class XmlInjectedSerializerWrapper<T> : IXmlSerializable
    {
        public XmlInjectedSerializerWrapper()
        {
        }

        public XmlInjectedSerializerWrapper(T t)
        {
            Value = t;
        }

        public T Value { get; set; }

        #region IXmlSerializable Members

        public void WriteXml(XmlWriter writer)
        {
            if (Value != null)
            {
                XmlSerializer serializer = XmlInjectedSerializationFactory.Create(typeof (T));
                serializer.Serialize(writer, Value);
            }
        }

        public void ReadXml(XmlReader reader)
        {
            try
            {
                using (XmlReader subReader = XmlReader.Create(reader.ReadSubtree(), reader.Settings))
                {
                    subReader.MoveToContent();

                    if (!subReader.IsEmptyElement) // (1)
                    {
                        if (subReader.Read())
                        {
                            XmlSerializer serializer = XmlInjectedSerializationFactory.Create(typeof (T));
                            Value = (T) serializer.Deserialize(subReader);
                        }
                    }
                }
            }
            finally
            {
                reader.Skip();
            }
        }

        public XmlSchema GetSchema()
        {
            return (null);
        }

        #endregion
    }
}