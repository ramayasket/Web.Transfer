using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;

namespace model.Base32
{
    /// <summary>
    /// Base-32 encoding and decoding.
    /// </summary>
    public class Base32Core
    {
        ////
        //// A byte array to encode is divided into
        //// zero or more 'native' blocks of 5 bytes
        //// and remaining 1-4 bytes are called cutoff block.
        ////
        //// Each native block is encoded with 8 base-32 characters
        //// (see _baseTable), the cutoff block is encoded with...
        ////

        /// <summary>
        /// Conversion table. Will add shuffling later on.
        /// </summary>
        internal static readonly char[] BaseTable = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h',
            'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '1', '2', '3', '4', '5', '6', };

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

        public static string ToBase32String(byte[] array)
        {
            if (null == array)
                throw new ArgumentNullException(nameof(array));

            if(0 == array.Length)
                return String.Empty;

            return ToBase32StringInternal(array);
        }

        public static byte[] FromBase32String(string input)
        {
            if(null == input)
                throw new ArgumentNullException(nameof(input));

            if(string.IsNullOrEmpty(input))
                return new byte[0];

            return FromBase32StringInternal(input);
        }


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

        internal class DecodingBlock
        {
            private readonly byte[] Body = new byte[NATIVE_BLOCK_SIZE];
            private readonly char[] Code = new char[NATIVE_CODE_SIZE];

            private int CodeIndex = 0;
            private int CutoffIndex = 0;

            public bool AddCode(char code, bool final)
            {
                if (CUTOFF == code) {

                    if(0 == CodeIndex || (NATIVE_CODE_SIZE-1) == CodeIndex)
                        throw new InvalidOperationException("Cutoff character in a wrong position");
                }

                Code[CodeIndex++] = code;

                return true;
            }
        }

        private static unsafe byte[] FromBase32StringInternal(string input)
        {
            const string ERROR_MESSAGE = "The input string was not made by converting a byte array into Base-32 string";

            var inputLength = input.Length;

            var blocks = new List<DecodingBlock>();

            fixed (char* characters = &input.ToCharArray()[0]) {

                var current = new DecodingBlock();

                for (int i = 0; i < inputLength; i++)
                {
                    var is_final = (i == inputLength - 1); // is it the final character?

                    var c = *(characters + i);

                    if (!IsEncodeCharacter(c))
                        throw new ArgumentException(ERROR_MESSAGE, nameof(input)); // not a character from BaseTable

                    bool eob;

                    try {
                        eob = current.AddCode(c, is_final);
                    }
                    catch (InvalidOperationException) {
                        throw new ArgumentException(ERROR_MESSAGE, nameof(input)); // wrong placement of cutoff character
                    }

                    if (eob) {
                        blocks.Add(current);
                        current = new DecodingBlock();
                    }
                }
            }

            return new byte[0];
        }

        /// <summary>
        /// Verifies that given character is a valid Base-32 character.
        /// </summary>
        /// <param name="c">Character to verify.</param>
        /// <returns>True for valid character, False otherwise.</returns>
        private static bool IsEncodeCharacter(char c)
        {
            foreach (var b in BaseTable)
                if (c == b || CUTOFF == c)
                    return true;

            return false;
        }

        private static unsafe string ToBase32StringInternal(byte[] array)
        {
            var parameters = new ArrayEncodingParameters(array.Length);
            var encoded = new char[parameters.TotalCode];
            var encodedOffset = 0;

            fixed (byte* buffer = &array[0])
            {
                //
                // input is processed in blocks of ≤ 5 bytes
                //
                var block = 0;
                var length = array.Length;

                // go through all blocks in buffer
                while (block < length)
                {
                    // get size of next block
                    var size = Math.Min(NATIVE_BLOCK_SIZE, length - block);

                    ulong* imprint = (ulong*)(buffer + block);
                    ulong imprinted = *imprint;

                    // limit imprinted bytes to the current block's only
                    imprinted &= BlockMasks[size];

                    var encodedBlock = BlockToBase32String(imprinted, size);
                    Array.Copy(encodedBlock, 0, encoded, encodedOffset, encodedBlock.Length);

                    encodedOffset += encodedBlock.Length;

                    // skip to the next block
                    block += NATIVE_BLOCK_SIZE;
                }
            }

            return new string(encoded);
        }

        private static char[] BlockToBase32String(ulong imprinted, int size)
        {
            var parameters = new ArrayEncodingParameters(size);
            var output = new char[parameters.TotalCode];
            var ox = 0; // output index

            // number of bits to encode
            var imprintedBits = size * 8;
            var offset = 0;

            while (offset < imprintedBits)
            {
                var bits = Math.Min(ENCODING_BITS, imprintedBits - offset);

                var mask = CutoffMasks[bits];

                var encode = imprinted & mask;
                var encoded = BaseTable[encode];

                if (bits < ENCODING_BITS)
                {
                    output[ox++] = CUTOFF;
                }

                output[ox++] = encoded;

                imprinted >>= bits;
                offset += bits;
            }

            return output;
        }
    }
}
