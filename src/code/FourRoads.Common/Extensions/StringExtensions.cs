using System.Security.Cryptography;
using System.Text;

namespace FourRoads.Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        ///     Converts this instance to a UTF-8 byte array.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static byte[] ConvertToUTF8ByteArray(this string source)
        {
            var encoding = new UTF8Encoding();

            return encoding.GetBytes(source);
        }

        /// <summary>
        ///     Returns the MD5 hash of this instance.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>An MD5 hash</returns>
        public static string MD5Hash(this string text)
        {
            var hashBuffer = MD5.Create().ComputeHash(text.ConvertToUTF8ByteArray());

            // Method A:
            var sb = new StringBuilder();

            foreach (var b in hashBuffer)
            {
                sb.Append(b.ToString("x2")); //'X2' for uppercase
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Gets the CRC-32 checksum of this instance.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static uint GetCRC32(this string text)
        {
            return Crc32.ComputeChecksum(text);
        }
    }
}