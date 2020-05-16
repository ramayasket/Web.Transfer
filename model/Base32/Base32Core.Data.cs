using System.Collections.Generic;

namespace model.Base32
{
    /// <summary>
    /// Base-32 encoding and decoding.
    /// </summary>
    public partial class Base32Core
    {
        /// <summary>
        /// Conversion table from <seealso cref="int"/> (as array index) into <seealso cref="char"/>.
        /// </summary>
        internal static readonly char[] EncodeTable = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h',
            'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '1', '2', '3', '4', '5', '6',
        };

        /// <summary>
        /// Conversion dictionary from <seealso cref="char"/> (as key) into <seealso cref="byte"/>.
        /// </summary>
        internal static readonly Dictionary<char, byte> DecodeTable;

        /// <summary>
        /// Initializes decode table.
        /// </summary>
        static Base32Core()
        {
            DecodeTable = new Dictionary<char, byte>();

            for (int i = 0; i < EncodeTable.Length; i++)
            {

                var key = EncodeTable[i];
                var value = (byte)i;

                DecodeTable[key] = value;
            }
        }

        /// <summary>
        /// Masks to apply to imprinted value.
        /// </summary>
        internal static readonly ulong[] BlockMasks = {
            0x0, // unused value
            0xff,
            0xffff,
            0xffffff,
            0xffffffff,
            0xffffffffff,
        };

        /// <summary>
        /// Masks to apply to cutoffs.
        /// </summary>
        internal static readonly ulong[] CutoffMasks = {
            0x00, // unused value
            0x01,
            0x03,
            0x07,
            0x0f,
            0x1f,
        };

        /// <summary>
        /// Cut-off character indicates byte sequence shorter than 5.
        /// </summary>
        internal const char CUTOFF = '0';

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
    }
}
