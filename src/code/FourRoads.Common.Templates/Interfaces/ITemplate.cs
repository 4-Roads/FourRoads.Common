using System;
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common.Interfaces
{
    public interface ITemplate
    {
        Hashtable Context { get; set; } 
        string Name { get; set; }
        string Body { get; set; }
        
    }
}
