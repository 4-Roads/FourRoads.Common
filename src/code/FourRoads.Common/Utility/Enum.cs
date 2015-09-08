using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FourRoads.Common
{
    public static class Enum<T> where T : struct
    {
        public static bool CanConvertFrom(object value)
        {
            if (!typeof(T).IsEnum)
                throw new InvalidCastException("Type T must be an Enum");
            if (value == null)
                throw new NullReferenceException("value cannot be null");
  
            System.ComponentModel.EnumConverter converter = new System.ComponentModel.EnumConverter(typeof(T));
            return converter.CanConvertFrom(value.GetType());
        }

        public static T ConvertFrom(object value)   
        {
            if (!typeof(T).IsEnum)
                throw new InvalidCastException("Type T must be an Enum");
            if (value == null)
                throw new NullReferenceException("value cannot be null");
  
            System.ComponentModel.EnumConverter converter = new System.ComponentModel.EnumConverter(typeof(T));
            if (converter.CanConvertFrom(value.GetType())) 
                return (T)converter.ConvertFrom(value);
            else if (value is Int32)
                return (T)Enum.ToObject(typeof(T), (int)value);

            throw new InvalidCastException("Cannot convert type '" + value.GetType().Name + "' to type '" + typeof(T).Name + "'."); 
        }

        public static bool CanConvertTo<TypeTo>()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidCastException("Type T must be an Enum");

            System.ComponentModel.EnumConverter converter = new System.ComponentModel.EnumConverter(typeof(T));
            return converter.CanConvertTo(typeof(TypeTo));
        }

        public static TypeTo ConvertTo<TypeTo>(T value)
        {
            if (!typeof(T).IsEnum)
                throw new InvalidCastException("Type T must be an Enum");

            System.ComponentModel.EnumConverter converter = new System.ComponentModel.EnumConverter(typeof(T));
            if (converter.CanConvertTo(typeof(TypeTo)))
                return (TypeTo)converter.ConvertTo(value, typeof(TypeTo));

            throw new InvalidCastException("Cannot convert type '" + typeof(T).Name + "' to type '" + typeof(TypeTo).Name + "'."); 
        }

        public static IList<T> GetValues()
        {
            IList<T> list = new List<T>();
            foreach (object value in Enum.GetValues(typeof(T)))
            {
                list.Add((T)value);
            }
            return list;
        }
    }
}
