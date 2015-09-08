// //------------------------------------------------------------------------------
// // <copyright company="Four Roads LLC">
// //     Copyright (c) Four Roads LLC.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.Xml.Serialization;

#endregion

namespace FourRoads.Common
{
    [Serializable, XmlRoot("injectionModules")]
    public class InjectionModules
    {
        [XmlAttribute("overridesPath")]
        public string OverridesPath { get; set; }

        [XmlAttribute("sharedModulePath")]
        public string SharedModulePath { get; set; }
        
        [XmlElement("injectionModule")]
        public InjectionModule[] Modules { get; set; }

        [XmlElement("bindingOverrides")]
        public XmlBindingOverride[] BindingOverrides { get; set; }
    }

    [Serializable]
    public class InjectionModule
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
    }

    [Serializable]
    public class XmlBindingOverride
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("bind")]
        public XmlBinding[] Bindings { get; set; }
    }

    [Serializable]
    public class XmlBinding
    {
        [XmlAttribute("service")]
        public string Service { get; set; }

        [XmlAttribute("to")]
        public string Type { get; set; }

        [XmlAttribute("scope")]
        public string Scope { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("rebind")]
        public bool ReBind { get; set; }

        [XmlElement("metadata")]
        public XmlBindingMetaData[] MetaData { get; set; }
    }

    [Serializable]
    public class XmlBindingMetaData
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}