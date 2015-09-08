using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common.Attributes
{
    public class WidthAttribute : Attribute
    {
        int _value;
        public int Value { get { return _value; } set { _value = value; } }
        public WidthAttribute(int value) { Value = value; }
    }
}
