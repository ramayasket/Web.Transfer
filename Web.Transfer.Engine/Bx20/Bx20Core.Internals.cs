using Kw.Common;
using System.Diagnostics.CodeAnalysis;

namespace Web.Transfer.Bx20
{
    public partial class Bx20Core
    {
        ////
        ////   A byte array to encode is divided into
        ////   zero or more 'native' blocks of 5 bytes
        ////   and remaining 1-4 bytes are called cutoff block.
        ////
        ////   Each native block is encoded with 8 Bx20 characters (see EncodeTable)
        ////   Each cutoff block of size (1,2,3,4) is encoded with (3,5,6,8) Bx20 characters, respectively.
        ////

        ////
        ////   Cutoff parameters for possible cutoff block sizes
        ////
        ////   Length   Cutoff   CutoffBits   CutoffCode   CutoffExtraBits   CutoffExtraCode   Native   NativeCode   TotalCode
        ////   1        1        8            1            3                 2                 0        0            3
        ////   2        2        16           3            1                 2                 0        0            5
        ////   3        3        24           4            4                 2                 0        0            6
        ////   4        4        32           6            2                 2                 0        0            8
        ////   5        0        0            0            0                 0                 1        8            8
        ////

        [ExcludeFromCodeCoverage]
        internal readonly struct ArrayEncodingParameters
        {
            /// <summary> Number of native blocks. </summary>
            public readonly int Native;

            /// <summary> Size of cutoff block in bytes. </summary>
            public readonly int Cutoff;

            /// <summary> Size of cutoff block in bits. </summary>
            public readonly int CutoffBits;

            /// <summary> Number of extra bits at the end of cutoff block. </summary>
            public readonly int CutoffExtraBits;

            /// <summary> Number of characters to encode native blocks. </summary>
            public readonly int NativeCode;

            /// <summary> Number of characters to encode cutoff block. </summary>
            public readonly int CutoffCode;

            /// <summary> Number of extra characters to encode cutoff block. </summary>
            public readonly int CutoffExtraCode;

            /// <summary> Total number of characters to encode the input. </summary>
            public readonly int TotalCode;

            public ArrayEncodingParameters(int size)
            {
                Native = size / NATIVE_BLOCK_SIZE;
                Cutoff = size - Native * NATIVE_BLOCK_SIZE;

                CutoffBits = Cutoff * 8;
                NativeCode = Native * NATIVE_CODE_SIZE;
                CutoffCode = CutoffBits / ENCODING_BITS;

                CutoffExtraBits = CutoffBits - CutoffCode * ENCODING_BITS;
                CutoffExtraCode = CutoffCode == 0 ? 0 : 2; // one character for extra bits + one cutoff character

                TotalCode = NativeCode + CutoffCode + CutoffExtraCode;
            }
        }

        /// <summary>
        /// Conversion table from <seealso cref="int"/> (as array index) into <seealso cref="char"/>.
        /// </summary>
        /// <remarks>
        /// Characters '0', '7', '8', '9' are used as cutoff indicators.
        /// This produces encoding table of 36 characters (not a power of two)
        /// which is to further delay attempts to break the encoding
        /// </remarks>
        internal static readonly char[] EncodeTable = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h',
            'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '1', '2', '3', '4', '5', '6',
        };

        /// <summary>
        /// Possible cutoff characters randomly selected when needed.
        /// </summary>
        internal static readonly char[] CutoffTable = { '0', '7', '8', '9' };

        /// <summary>
        /// Conversion dictionary from <seealso cref="char"/> (as key) into <seealso cref="byte"/>.
        /// </summary>
        internal static readonly byte[] DecodeTable;

        /// <summary>
        /// Generates a random index into the array of possible cutoff characters: { '0', '7', '8', '9' }.
        /// </summary>
        internal static readonly Randomizer<int> CutoffRandomizer = new Randomizer<int>(0, 4);

        /// <summary>
        /// Initializes decode table.
        /// </summary>
        static Bx20Core()
        {
            DecodeTable = new byte['z' + 1];

            for (int i = 0; i < DecodeTable.Length; i++)
                DecodeTable[i] = 0;

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
        /// Number of bits per encoding character.
        /// </summary>
        internal const int ENCODING_BITS = 5;

        /// <summary>
        /// Byte array size which is 'native' to Bx20 encoding.
        /// Arrays of this size are encoded without cut-off characters.
        /// </summary>
        internal const int NATIVE_BLOCK_SIZE = 5;

        /// <summary>
        /// Number of characters to encode 'native' block.
        /// </summary>
        internal const int NATIVE_CODE_SIZE = 8;

        /// <summary>
        /// Copies some bytes from one address to another.
        /// </summary>
        /// <param name="pFrom">Source address.</param>
        /// <param name="pTo">Destination address.</param>
        /// <param name="length">Number of bytes to copy.</param>
        internal static unsafe void Copy(byte* pFrom, byte* pTo, int length)
        {
            for (int i = 0; i < length; i++)
                *(pTo + i) = *(pFrom + i);
        }

        /// <summary>
        /// Verifies that given character is a valid Bx20 character.
        /// </summary>
        /// <param name="c">Character to verify.</param>
        /// <returns>True for valid character, False otherwise.</returns>
        internal static bool IsEncodeCharacter(char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= '1' && c <= '6') || IsCutoffCharacter(c))
                return true;

            return false;
        }

        /// <summary>
        /// Verifies that given character is a valid Bx20 cutoff character.
        /// </summary>
        /// <param name="c">Character to verify.</param>
        /// <returns>True for valid character, False otherwise.</returns>
        internal static bool IsCutoffCharacter(char c)
        {
            if ((c >= '7' && c <= '9') || c == '0')
                return true;

            return false;
        }

        /// <summary>
        /// Returns randomly selected cutoff character.
        /// </summary>
        /// <returns>
        /// An integer index into <seealso cref="CutoffTable"/>.
        /// </returns>
        internal static char MakeCutoffCharacter()
        {
            var x = CutoffRandomizer.Next();
            var y = CutoffTable[x];

            return y;
        }
    }
}
