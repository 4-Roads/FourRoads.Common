using System.IO;
using System.Text;

namespace FourRoads.Common.Extensions
{
    public static class MemoryStreamExtension
    {
        public static string ConvertToUTF8String(this MemoryStream stream)
        {
            var encoding = new UTF8Encoding();

            var data = stream.ToArray();

            if (data.Length > 3)
                return encoding.GetString(data);

            return string.Empty;
        }
    }
}