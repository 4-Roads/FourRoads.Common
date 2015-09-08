using System.Xml;
using FourRoads.Common.Interfaces;
using Ninject;

namespace FourRoads.Common
{
    public interface ISettings
    {
        string FileName { get; }
        XmlDocument ConfigurationDocument { get; }
        InjectionModules InjectionModules { get; }
        ConfigurationSettings ConfigurationSettings { get; }
        XmlNode GetConfigNode(string xpath);
        XmlNodeList GetConfigNodes(string xpath);
        IAppEventManager AppEventManager { get; }
        IKernel ParentKernel { get; }
    }
}