using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common.Attributes
{
    public class ImageUrlAttribute : Attribute
    {
        string _value;
        public string Value { get { return _value; } set { _value = value; } }
        public ImageUrlAttribute(string value) { Value = value; }
    }
}
