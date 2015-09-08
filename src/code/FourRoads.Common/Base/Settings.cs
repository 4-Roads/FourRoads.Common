using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FourRoads.Common.Interfaces;
using Ninject;

namespace FourRoads.Common
{
    public class Settings<Derived> : ISettings where Derived : Settings<Derived>, new()
    {
        private static readonly object configLocker = new object();
        private static Derived _instance;
        private string _fileName = string.Empty;
        private IAppEventManager _eventManager;

        protected Settings()
        {

        }

        public IAppEventManager AppEventManager
        {
            get { return _eventManager; }
        }

        #region ISettings Members

        public XmlNode GetConfigNode(string xpath)
        {
            return ConfigurationDocument.SelectSingleNode(xpath);
        }

        public XmlNodeList GetConfigNodes(string xpath)
        {
            return ConfigurationDocument.SelectNodes(xpath);
        }

        public virtual IKernel ParentKernel
        {
            get { return null; }
        }

        public string FileName
        {
            get { return _fileName; }
            protected set
            {
                _fileName = value;
            }
        }

        public XmlDocument ConfigurationDocument { get; protected set; }

        public InjectionModules InjectionModules { get; protected set; }

        public ConfigurationSettings ConfigurationSettings { get; protected set; }

        #endregion

        public virtual void LoadConfiguration()
        {
            lock (configLocker)
            {
                InjectionModules = new InjectionModules();
                ConfigurationSettings = new ConfigurationSettings();

                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
                if (!string.IsNullOrEmpty(file))
                {
                    ConfigurationDocument = new XmlDocument();
                    ConfigurationDocument.Load(file);
                    if (ConfigurationDocument.DocumentElement != null)
                    {
                        XmlNode node = ConfigurationDocument.SelectSingleNode("configuration/injectionModules");
                        if (node != null)
                        {
                            XmlSerializer sz = new XmlSerializer(typeof (InjectionModules));
                            XmlNodeReader reader = new XmlNodeReader(node);

                            InjectionModules = (InjectionModules) sz.Deserialize(reader);
                        }

                        node = ConfigurationDocument.SelectSingleNode("configuration/settings");
                        if (node != null)
                        {
                            XmlSerializer sz = new XmlSerializer(typeof (ConfigurationSettings));
                            XmlNodeReader reader = new XmlNodeReader(node);

                            ConfigurationSettings = (ConfigurationSettings) sz.Deserialize(reader);
                        }

                        node = ConfigurationDocument.SelectSingleNode("configuration/appEventHandlers");
                        if (node != null)
                        {
                            _eventManager = new AppEventManager(node);
                        }
                    }
                }
                else
                    throw new SettingsException("Cannot find " + FileName);
            }
        }

        public static Derived Instance()
        {
            if (_instance == null)
            {
                Derived dt = new Derived();

                _instance = dt;

                _instance.LoadConfiguration();
            }

            return _instance;
        }
    }
}