using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;

namespace model.Base32
{
    /// <summary>
    /// Base-32 encoding and decoding.
    /// </summary>
    public partial class Base32Core
    {
        ////
        ////   A byte array to encode is divided into
        ////   zero or more 'native' blocks of 5 bytes
        ////   and remaining 1-4 bytes are called cutoff block.
        ////
        ////   Each native block is encoded with 8 base-32 characters (see EncodeTable)
        ////   Each cutoff block of size (1,2,3,4) is encoded with (3,5,6,8) base-32 characters, respectively.
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

            if (0 == array.Length)
                return String.Empty;

            return ToBase32StringInternal(array);
        }

        public static byte[] FromBase32String(string input)
        {
            if (null == input)
                throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrEmpty(input))
                return new byte[0];

            return FromBase32StringInternal(input);
        }

        internal struct DecodingBlock
        {
            private int _code; // number of code characters
            private int _cutoff; // position of cutoff character

            /// <summary>
            /// For tests only: if True, the block will accept no more code characters.
            /// </summary>
            internal bool Finished { get; private set; }

            /// <summary>
            /// Number of decoded bytes in the block.
            /// </summary>
            public int DataLength =>
                (_code == 3) ? 1 :
                (_code == 5) ? 2 :
                (_code == 6) ? 3 :
                (_code == 8 && _cutoff == 6) ? 4 : 5;

            /// <summary>
            /// Number of code characters in the block.
            /// </summary>
            public int CodeLength => _code;
            
            /// <summary>
            /// Verifies a candidate character which wants to be added to the block.
            /// </summary>
            /// <param name="code">Candidate character.</param>
            /// <param name="final">True if candidate is the very last one.</param>
            /// <returns>True if block is finished, False otherwise.</returns>
            public bool AddCode(char code, bool final)
            {
                if (!IsEncodeCharacter(code))
                    throw new InvalidOperationException("Adding invalid code");

                if (Finished)
                    throw new InvalidOperationException("Adding code to finished block");

                if (CUTOFF == code) {

                    if (0 == _code || (NATIVE_CODE_SIZE - 1) == _code || 0 != _cutoff)
                        throw new InvalidOperationException("Cutoff character in a wrong position");

                    _cutoff = _code;
                }

                _code++;

                if (final && _code < NATIVE_CODE_SIZE && 0 == _cutoff)
                    throw new InvalidOperationException("Incomplete block without cutoff");

                if (final && 0 != _cutoff && _cutoff + 2 != _code)
                    throw new InvalidOperationException("Incomplete block with cutoff at the end");

                Finished = false
                           || final // we're told it's finished
                           || NATIVE_CODE_SIZE == _code // we have all 8 characters
                           || (_cutoff + 2 == _code && 0 != _cutoff); // we have cutoff and 1 extra character

                return Finished;
            }
        }

        private static unsafe byte[] FromBase32StringInternal(string input)
        {
            const string ERROR_MESSAGE = "The input string was not made by converting a byte array into Base-32 string";

            var inputLength = input.Length;

            var accumulator = new List<DecodingBlock>();

            fixed (char* code = &input.ToCharArray()[0]) {

                var current = new DecodingBlock();

                for (int i = 0; i < inputLength; i++)
                {
                    var is_final = (i == inputLength - 1); // is it the final character?

                    var c = *(code + i);

                    bool eob;

                    try
                    {
                        // verify next character against block descriptor
                        // and adjust block parameters
                        eob = current.AddCode(c, is_final); // end of block
                    }
                    catch (InvalidOperationException iox)
                    {
                        throw new ArgumentException(ERROR_MESSAGE, nameof(input), iox); // invalid code(s) added to the block
                    }

                    if (eob)
                    { // if block is finished

                        accumulator.Add(current); // add to the accumulator
                        current = new DecodingBlock(); // and start a new block
                    }
                }

                fixed (DecodingBlock* blocks = &(accumulator.ToArray())[0])
                {

                    var totalDecodeLength = accumulator.Sum(b => b.DataLength);
                    var totalBlocks = accumulator.Count;

                    var output = new byte[totalDecodeLength];

                    fixed (byte* data = &output[0])
                    {

                        var codeOffset = 0; // offset in code buffer
                        var dataOffset = 0; // offset in data buffer

                        for (var b = 0; b < totalBlocks; b++)
                        {

                            var pblock = (blocks + b);
                            var pcode = (code + codeOffset);
                            var block = *pblock;

                            var imprinted = DecodeBlock(pcode, pblock);

                            byte* pimpmrinted = (byte*)&imprinted;

                            Copy(pimpmrinted, (data + dataOffset), block.DataLength);

                            codeOffset += block.CodeLength;
                            dataOffset += block.DataLength;
                        }
                    }

                    return output;
                }
            }
        }

        private static unsafe void Copy(byte* pFrom, byte* pTo, int length)
        {
            for (int i = 0; i < length; i++)
                *(pTo + i) = *(pFrom + i);
        }

        private static unsafe ulong DecodeBlock(char* codeBuffer, DecodingBlock* pblock)
        {
            var block = *pblock;

            ulong receptor = 0; // receives imprinted 
            var imprintOffset = 0;

            for (int i = 0; i < block.CodeLength; i++) {

                var code = *(codeBuffer + i);

                if (CUTOFF == code)
                    continue;

                var data = DecodeTable[code];

                ulong imprinted = 0;

                imprinted |= data;
                imprinted <<= imprintOffset;

                receptor |= imprinted;
                imprintOffset += ENCODING_BITS;
            }

            return receptor;
        }

        /// <summary>
        /// Verifies that given character is a valid Base-32 character.
        /// </summary>
        /// <param name="c">Character to verify.</param>
        /// <returns>True for valid character, False otherwise.</returns>
        private static bool IsEncodeCharacter(char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= '1' && c <= '6') || c == CUTOFF)
                return true;

            return false;
        }

        private static unsafe string ToBase32StringInternal(byte[] array)
        {
            var parameters = new ArrayEncodingParameters(array.Length);
            var encoded = new char[parameters.TotalCode];
            var encodedOffset = 0;

            fixed (byte* buffer = &array[0]) {
                //
                // input is processed in blocks of ≤ 5 bytes
                //
                var block = 0;
                var length = array.Length;

                // go through all blocks in buffer
                while (block < length) {
                    // get size of next block
                    var size = Math.Min(NATIVE_BLOCK_SIZE, length - block);

                    ulong* imprint = (ulong*) (buffer + block);
                    ulong imprinted = *imprint;

                    // limit imprinted bytes to the current block's only
                    imprinted &= BlockMasks[size];

                    var encodedBlock = EncodeBlock(imprinted, size);
                    Array.Copy(encodedBlock, 0, encoded, encodedOffset, encodedBlock.Length);

                    encodedOffset += encodedBlock.Length;

                    // skip to the next block
                    block += NATIVE_BLOCK_SIZE;
                }
            }

            return new string(encoded);
        }

        private static char[] EncodeBlock(ulong imprinted, int size)
        {
            var parameters = new ArrayEncodingParameters(size);
            var output = new char[parameters.TotalCode];
            var ox = 0; // output index

            // number of bits to encode
            var imprintedBits = size * 8;
            var offset = 0;

            while (offset < imprintedBits) {

                var bits = Math.Min(ENCODING_BITS, imprintedBits - offset);

                var mask = CutoffMasks[bits];

                var encode = imprinted & mask;
                var encoded = EncodeTable[encode];

                if (bits < ENCODING_BITS) {
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
