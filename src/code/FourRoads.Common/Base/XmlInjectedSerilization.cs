using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FourRoads.Common
{
    public static class XmlInjectedSerializationFactory  
    {
        private static XmlSerializerFactory _factory = new XmlSerializerFactory();

        public static XmlSerializer Create(Type type)
        {
            Type realType = GetRealType(type);
            XmlSerializer xs = _factory.CreateSerializer(realType);
            if (xs == null)
                xs = new XmlSerializer(realType);
            return xs;
        }

        public static XmlSerializer Create(Type type, string defaultNamespace)
        {
            Type realType = GetRealType(type);
            XmlSerializer xs = _factory.CreateSerializer(realType, defaultNamespace);
            if (xs == null)
                xs = new XmlSerializer(realType, defaultNamespace);
            return xs;
        }

        public static XmlSerializer Create(XmlTypeMapping xmlTypeMapping)
        {
            XmlSerializer xs = _factory.CreateSerializer(xmlTypeMapping);
            if (xs == null)
                xs = new XmlSerializer(xmlTypeMapping);
            return xs;
        }

        public static XmlSerializer Create(Type type, XmlAttributeOverrides overrides)
        {
            Type realType = GetRealType(type);
            XmlSerializer xs = _factory.CreateSerializer(realType, overrides);
            if (xs == null)
                xs = new XmlSerializer(realType, overrides);
            return xs;
        }

        public static XmlSerializer Create(Type type, System.Type[] extraTypes)
        {
            Type realType = GetRealType(type);
            System.Type[] realTypes = GetRealTypes(extraTypes); 
            XmlSerializer xs = _factory.CreateSerializer(realType, realTypes);
            if (xs == null)
                xs = new XmlSerializer(realType, realTypes);
            return xs;
        }

        public static XmlSerializer Create(Type type, XmlRootAttribute root)
        {
            Type realType = GetRealType(type);
            XmlSerializer xs = _factory.CreateSerializer(realType, root);
            if (xs == null)
                xs = new XmlSerializer(realType, root);
            return xs;
        }

        private static Type[] GetRealTypes(Type[] types)
        {
            List<Type> realTypes = new List<Type>();
            foreach (Type type in types)
                realTypes.Add(GetRealType(type));

            return realTypes.ToArray();
        }

        private static Type GetRealType(Type type)
        {
            Type realType = type;

            if (realType.IsInterface)
            {
                object instance = Injector.Get(realType);
                realType = instance.GetType();
            }

            return realType;
        }
    }
}
