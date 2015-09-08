using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common.Interfaces
{
    public interface ITemplateManager
    {
        Dictionary<string, ITemplate> Templates { get; }
        ITemplate CreateTemplate(string name, string Body);
        ITemplate CreateTemplateFromFile(string name, string fileName);
        string ProcessTemplate(ITemplate template);
        string ProcessTemplateFromFile(string fileName, Hashtable context);
        string ProcessTemplate(string templateValaue, Hashtable context);
    }
}
