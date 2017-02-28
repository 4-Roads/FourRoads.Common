using FourRoads.Common.Extensions;

namespace FourRoads.Common
{
    /// <summary>
    ///     Provides CRC-32 support
    /// </summary>
    internal static class Crc32
    {
        private static readonly uint[] _table;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Crc32" /> class.
        /// </summary>
        static Crc32()
        {
            const uint poly = 0xedb88320;

            _table = new uint[256];

            for (uint i = 0; i < _table.Length; ++i)
            {
                var temp = i;

                for (var j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (temp >> 1) ^ poly;
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }

                _table[i] = temp;
            }
        }

        /// <summary>
        ///     Computes the checksum.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns></returns>
        public static uint ComputeChecksum(string input)
        {
            return ComputeChecksum(input.ConvertToUTF8ByteArray());
        }

        /// <summary>
        ///     Computes the checksum.
        /// </summary>
        /// <param name="bytes">The input byte array.</param>
        /// <returns></returns>
        public static uint ComputeChecksum(byte[] bytes)
        {
            var crc = 0xffffffff;

            for (var i = 0; i < bytes.Length; ++i)
            {
                var index = (byte) ((crc & 0xff) ^ bytes[i]);

                crc = (crc >> 8) ^ _table[index];
            }

            return ~crc;
        }
    }
}