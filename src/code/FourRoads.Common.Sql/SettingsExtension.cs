using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FourRoads.Common.Sql
{
    [Serializable, XmlRoot("dataProviders")]
    public class DataProviders
    {
        [XmlElement("dataProvider")]
        public DataProvider[] Providers { get; set; }
    }

    [Serializable]
    public class DataProvider
    {
        [XmlAttribute("name")]
        public string Name{ get; set;}

        [XmlAttribute("connectionStringName")]
        public string ConnectionStringName{ get; set;}

        [XmlAttribute("databaseOwner")]
        public string DatabaseOwner{ get; set; }
    }


    public static class SettingsExtension
    {
        private static Dictionary<string, DataProvider> _dataProviderKeys = new Dictionary<string, DataProvider>();
        private static List<string> _configurationSettingsLoaded = new List<string>();
        private static object _SyncObj = new object();

        public static DataProvider GetDataProviderByName(this ISettings settings, string name)
        {
            if (!_configurationSettingsLoaded.Contains(settings.FileName))
            {
                lock (_SyncObj)
                {
                    if (!_configurationSettingsLoaded.Contains(settings.FileName))
                    {
                        // Load the Providers for the configuration file

                        XmlNode node = settings.ConfigurationDocument.SelectSingleNode("configuration/dataProviders");

                        if (node != null)
                        {
                            XmlSerializer sz = new XmlSerializer(typeof (DataProviders));

                            DataProviders providers = (DataProviders) sz.Deserialize(new XmlNodeReader(node));

                            if (providers != null)
                            {
                                foreach (DataProvider prov in providers.Providers)
                                {
                                    if (!_dataProviderKeys.ContainsKey(settings.FileName + "." + prov.Name))
                                        _dataProviderKeys.Add(settings.FileName + "." + prov.Name, prov);
                                }
                            }
                        }
                        _configurationSettingsLoaded.Add(settings.FileName);
                    }
                }
            }

            return _dataProviderKeys[settings.FileName + "." + name];
        }

    }
}
