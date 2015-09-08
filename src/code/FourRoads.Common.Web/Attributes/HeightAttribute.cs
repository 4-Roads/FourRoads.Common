using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common.Attributes
{
    public class HeightAttribute : Attribute
    {
        int _value;
        public int Value { get { return _value; } set { _value = value; } }
        public HeightAttribute(int value) { Value = value; }
    }
}
