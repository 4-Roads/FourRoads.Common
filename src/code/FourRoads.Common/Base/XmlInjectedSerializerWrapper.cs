// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public sealed class XmlInjectedSerializerWrapper<T> : IXmlSerializable
    {
        private readonly IObjectFactory _objectFactory;

        public XmlInjectedSerializerWrapper(IObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        public XmlInjectedSerializerWrapper(IObjectFactory objectFactory, T t) : this(objectFactory)
        {
            Value = t;
        }

        public T Value { get; set; }

        #region IXmlSerializable Members

        public void WriteXml(XmlWriter writer)
        {
            if (Value != null)
            {
                var serializer = new XmlInjectedSerializationFactory(_objectFactory).Create(typeof(T));
                serializer.Serialize(writer, Value);
            }
        }

        public void ReadXml(XmlReader reader)
        {
            try
            {
                using (var subReader = XmlReader.Create(reader.ReadSubtree(), reader.Settings))
                {
                    subReader.MoveToContent();

                    if (!subReader.IsEmptyElement) // (1)
                    {
                        if (subReader.Read())
                        {
                            var serializer = new XmlInjectedSerializationFactory(_objectFactory).Create(typeof(T));
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
            return null;
        }

        #endregion
    }
}