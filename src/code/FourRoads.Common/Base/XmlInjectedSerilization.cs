using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public class XmlInjectedSerializationFactory
    {
        private readonly XmlSerializerFactory _factory = new XmlSerializerFactory();
        private readonly IObjectFactory _objectFactory;

        public XmlInjectedSerializationFactory(IObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        public XmlSerializer Create(Type type)
        {
            var realType = GetRealType(type);
            var xs = _factory.CreateSerializer(realType);
            if (xs == null)
                xs = new XmlSerializer(realType);
            return xs;
        }

        public XmlSerializer Create(Type type, string defaultNamespace)
        {
            var realType = GetRealType(type);
            var xs = _factory.CreateSerializer(realType, defaultNamespace);
            if (xs == null)
                xs = new XmlSerializer(realType, defaultNamespace);
            return xs;
        }

        public XmlSerializer Create(XmlTypeMapping xmlTypeMapping)
        {
            var xs = _factory.CreateSerializer(xmlTypeMapping);
            if (xs == null)
                xs = new XmlSerializer(xmlTypeMapping);
            return xs;
        }

        public XmlSerializer Create(Type type, XmlAttributeOverrides overrides)
        {
            var realType = GetRealType(type);
            var xs = _factory.CreateSerializer(realType, overrides);
            if (xs == null)
                xs = new XmlSerializer(realType, overrides);
            return xs;
        }

        public XmlSerializer Create(Type type, Type[] extraTypes)
        {
            var realType = GetRealType(type);
            var realTypes = GetRealTypes(extraTypes);
            var xs = _factory.CreateSerializer(realType, realTypes);
            if (xs == null)
                xs = new XmlSerializer(realType, realTypes);
            return xs;
        }

        public XmlSerializer Create(Type type, XmlRootAttribute root)
        {
            var realType = GetRealType(type);
            var xs = _factory.CreateSerializer(realType, root);
            if (xs == null)
                xs = new XmlSerializer(realType, root);
            return xs;
        }

        private Type[] GetRealTypes(Type[] types)
        {
            var realTypes = new List<Type>();
            foreach (var type in types)
                realTypes.Add(GetRealType(type));

            return realTypes.ToArray();
        }

        private Type GetRealType(Type type)
        {
            var realType = type;

            if (realType.IsInterface)
            {
                var instance = _objectFactory.Get(realType);

                realType = instance.GetType();
            }

            return realType;
        }
    }
}