using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Templates
{
    public class Template : ITemplate
    {

        #region ITemplate Members

        private Hashtable _Context;
        public Hashtable Context
        {
            get 
            {
                if (_Context == null)
                    _Context = new Hashtable();
                return _Context;
            }
            set { _Context = value; }
        }

        public string Name {get; set; }
        public string Body {get; set; }

        #endregion
    }
}
