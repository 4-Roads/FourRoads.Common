using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FourRoads.Common.Extensions
{
    public static class MemoryStreamExtension
    {
        public static string ConvertToUTF8String(this MemoryStream stream)
        {
            UTF8Encoding encoding = new UTF8Encoding();

            byte[] data = stream.ToArray();

            if (data.Length > 3)
                return encoding.GetString(data);

            return string.Empty;
        }
    }
}
