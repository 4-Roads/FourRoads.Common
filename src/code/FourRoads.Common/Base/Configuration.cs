// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FourRoads.Common.Interfaces;
using Ninject;

#endregion

namespace FourRoads.Common
{
    [Obsolete("This class is no longer used please use Setttings<Derived> class instead", false)]
    public class Configuration : ISettings
    {
        private static readonly object configLocker = new object();
        private static Hashtable _Configurations;
        private XmlDocument _ConfigurationDocument;
        private ConfigurationSettings _ConfigurationSettings = new ConfigurationSettings();
        private InjectionModules _injectionModules = new InjectionModules();

        public Configuration()
        {
        }

        public Configuration(string fileName)
        {
            FileName = fileName;

            if (_Configurations.ContainsKey(FileName))
                _Configurations.Remove(fileName);

            _Configurations.Add(FileName, this);
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
            if (!string.IsNullOrEmpty(file))
            {
                lock (configLocker)
                {
                    _ConfigurationDocument = new XmlDocument();
                    _ConfigurationDocument.Load(file);
                    if (_ConfigurationDocument.DocumentElement != null)
                    {
                        XmlNode node = _ConfigurationDocument.SelectSingleNode("configuration/injectionModules");
                        if (node != null)
                        {
                            XmlSerializer sz = new XmlSerializer(typeof (InjectionModules));
                            XmlNodeReader reader = new XmlNodeReader(node);

                            _injectionModules = (InjectionModules) sz.Deserialize(reader);
                        }

                        node = _ConfigurationDocument.SelectSingleNode("configuration/settings");
                        if (node != null)
                        {
                            XmlSerializer sz = new XmlSerializer(typeof (ConfigurationSettings));
                            XmlNodeReader reader = new XmlNodeReader(node);

                            _ConfigurationSettings = (ConfigurationSettings) sz.Deserialize(reader);
                        }
                    }
                }
            }
            else
                throw new SettingsException("Cannot find " + FileName);
        }

        public ConfigurationSettings Settings
        {
            get { return _ConfigurationSettings; }
        }

        public XmlElement Document
        {
            get { return _ConfigurationDocument.DocumentElement; }
        }

        #region ISettings Members

        public string FileName { get; private set; }

        public InjectionModules InjectionModules
        {
            get { return _injectionModules; }
        }

        public XmlNode GetConfigNode(string xpath)
        {
            return Document.SelectSingleNode(xpath);
        }

        public XmlNodeList GetConfigNodes(string xpath)
        {
            return Document.SelectNodes(xpath);
        }

        public IAppEventManager AppEventManager
        {
            get { throw new NotImplementedException(); }
        }

        public IKernel ParentKernel
        {
            get { return null; }
        }

        public XmlDocument ConfigurationDocument
        {
            get { return _ConfigurationDocument; }
        }

        public ConfigurationSettings ConfigurationSettings
        {
            get { return _ConfigurationSettings; }
        }

        #endregion

        public static T GetInstance<T>(string fileName) where T : Configuration
        {
            Configuration config = null;
            string configKey = fileName.ToLower() + typeof (T).FullName.ToLower();

            if (_Configurations == null)
                _Configurations = new Hashtable();

            if (_Configurations.ContainsKey(configKey) && _Configurations[configKey].GetType() == typeof (T))
                config = _Configurations[configKey] as T;
            else
            {
                if (_Configurations.ContainsKey(configKey))
                    _Configurations.Remove(configKey);

                config = TypeUtility.CreateObject<T>(fileName);

                _Configurations.Add(configKey, config);
            }

            return config as T;
        }
    }

    [Serializable, XmlRoot("settings")]
    public class ConfigurationSettings
    {
        private ConfigurationSetting[] _Values;
        private Hashtable _settings = new Hashtable();

        [XmlElement("add")]
        public ConfigurationSetting[] Values
        {
            get { return _Values; }
            set
            {
                _Values = value;
                if (_Values != null)
                {
                    foreach (ConfigurationSetting setting in _Values)
                        _settings.Add(setting.Name, setting.Value);
                }
            }
        }

        [XmlIgnore]
        public string this[string name]
        {
            get { return _settings[name] as string; }
        }

        [XmlIgnore]
        public string this[int index]
        {
            get { return _Values[index].Value; }
        }

        [XmlIgnore]
        public int Count
        {
            get { return _Values.Length; }
        }
    }

    [Serializable]
    public class ConfigurationSetting
    {
        private string _Value;

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value
        {
            get
            {
                if (_Value == null && InnerText != null)
                    return InnerText;
                return _Value;
            }
            set { _Value = value; }
        }

        [XmlText]
        public string InnerText { get; set; }
    }
}