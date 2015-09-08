using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace FourRoads.Common.Attributes
{
    public static class AttributeUtility
    {
        public static string GetImageUrl(Enum value)
        {
            try
            {
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

                ImageUrlAttribute[] attributes =
                    (ImageUrlAttribute[])fieldInfo.GetCustomAttributes(typeof(ImageUrlAttribute), false);

                return (attributes.Length > 0) ? attributes[0].Value : value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }

        public static int GetHeight(Enum value)
        {
            int v = 0;

            try
            {
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

                HeightAttribute[] attributes =
                    (HeightAttribute[])fieldInfo.GetCustomAttributes(typeof(HeightAttribute), false);

                v = (attributes.Length > 0) ? attributes[0].Value : 0;
            }
            catch { }

            return v;
        }

        public static int GetWidth(Enum value)
        {
            int v = 0;

            try
            {
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

                WidthAttribute[] attributes =
                    (WidthAttribute[])fieldInfo.GetCustomAttributes(typeof(WidthAttribute), false);

                v = (attributes.Length > 0) ? attributes[0].Value : 0;
            }
            catch { }

            return v;
        }
    }
}


