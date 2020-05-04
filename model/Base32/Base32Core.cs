using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace model.Base32
{
    public static class Base32Core
    {
        /// <summary>
        /// Conversion table. Will add shuffling later on.
        /// </summary>
        internal static readonly char[] _baseTable = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h',
            'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '0', '1', '2', '3', '4', '5', };

        /// <summary>
        /// Cut-off character indicates byte sequence shorter than 5.
        /// </summary>
        internal const char CUTOFF = '6';

        /// <summary>
        /// Number of bits per encoding character.
        /// </summary>
        internal const int ENCODING_BITS = 5;

        /// <summary>
        /// Byte array size which is 'native' to Base32 encoding.
        /// Arrays of this size are encoded without cut-off characters.
        /// </summary>
        internal const int NATIVE_BLOCK_SIZE = 5;

        /// <summary>
        /// Number of characters to encode 'native' block.
        /// </summary>
        internal const int NATIVE_CODE_SIZE = 8;

        internal static int GetEncodedLength(int size)
        {
            var native = size / NATIVE_BLOCK_SIZE;
            var cutoff = size - native * NATIVE_BLOCK_SIZE;

            var cutoffBits = cutoff * 8;
            var nativeCode = native * NATIVE_CODE_SIZE;
            var cutoffCode = cutoffBits / ENCODING_BITS;

            /*
             * this is for illustration only
            var cutoffExtraBits = cutoffBits - cutoffCode * ENCODING_BITS;
            */

            var cutoffExtra = cutoffCode == 0 ? 0 : 2; // one character for extra bits + one cutoff character

            var length = nativeCode + cutoffCode + cutoffExtra;

            //Console.WriteLine($"size {size} native {native} nativeCode {nativeCode} cutoffCode {cutoffCode} cutoffExtraBits {cutoffExtraBits} length {length}");
            //Console.WriteLine($"cutoff {cutoff} cutoffBits {cutoffBits} cutoffCode {cutoffCode} cutoffExtraBits {cutoffExtraBits} length {length}");

            return length;
        }

        public static string ToBase32String(byte[] input)
        {
            return "";
        }

        public static byte[] FromBase32String(string input)
        {
            return new byte[0];
        }
    }
}
