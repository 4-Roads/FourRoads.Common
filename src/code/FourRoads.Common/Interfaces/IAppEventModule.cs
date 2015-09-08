using System.Xml;

namespace FourRoads.Common.Interfaces
{
    public interface IAppEventModule
    {
        void Initialize(IAppEventManager em, XmlNode node);
    }
}