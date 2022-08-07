using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Web.Transfer.Bx20
{
    public partial class Bx20Core
    {
        #region Decoding block

        internal class DecodingBlock
        {
            private int _codeLength; // number of code characters
            private int _cutoffIndex; // position of cutoff character

            /// <summary>
            /// For tests only: if True, the block will accept no more code characters.
            /// </summary>
            internal bool Finished { get; private set; }

            /// <summary>
            /// Number of decoded bytes in the block.
            /// </summary>
            public int DataLength {
                get {
                    if (0 == _codeLength)
                        return 0;

                    if(!Finished)
                        throw new InvalidOperationException("The block is not finished and has no valid data length");
                    
                    if (IsCutoff) {
                        return
                            (_codeLength == 3) ? 1 :
                            (_codeLength == 5) ? 2 :
                            (_codeLength == 6) ? 3 : 4;
                    }
                    else
                        return 5;
                }
            }

            /// <summary>
            /// Number of code characters in the block.
            /// </summary>
            public int CodeLength => _codeLength;

            /// <summary>
            /// True if this is a cutoff block.
            /// </summary>
            public bool IsCutoff => _cutoffIndex > 0;

            /// <summary>
            /// Cutoff character can be at particular positions only.
            /// </summary>
            /// <param name="x"></param>
            /// <returns></returns>
            private bool VerifyCutoffPosition(int x) => (1 == x || 3 == x || 4 == x || 6 == x);

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

                if (IsCutoffCharacter(code))
                {
                    _cutoffIndex = _codeLength;

                    if(!VerifyCutoffPosition(_cutoffIndex))
                        throw new InvalidOperationException("Cutoff character in a wrong position");
                }

                _codeLength++;

                if (final && _codeLength < NATIVE_CODE_SIZE && 0 == _cutoffIndex)
                    throw new InvalidOperationException("Incomplete block without cutoff");

                if (final && 0 != _cutoffIndex && _cutoffIndex + 2 != _codeLength)
                    throw new InvalidOperationException("Incomplete block with cutoff at the end");

                Finished = Finished
                           || final // we're told it's finished
                           || NATIVE_CODE_SIZE == _codeLength // we have all 8 characters
                           || (_cutoffIndex + 2 == _codeLength && 0 != _cutoffIndex); // we have cutoff and 1 extra character

                return Finished;
            }
        }

        #endregion

        const string DECODE_ERROR_MESSAGE = "The input was not made by converting byte array into Bx20 string";

        internal static bool MarkupBlocks(ref DecodingBlock current, List<DecodingBlock> accumulator, string input, bool finalize)
        {
            var inputLength = input.Length;

            for (int i = 0; i < inputLength; i++)
            {
                var is_final = finalize && (i == inputLength - 1); // is it the final character?

                var c = input[i];

                bool eob;

                try
                {
                    // verify next character against block descriptor
                    // and adjust block parameters
                    eob = current.AddCode(c, is_final); // end of block
                }
                catch (InvalidOperationException iox)
                {
                    throw new ArgumentException(DECODE_ERROR_MESSAGE, nameof(input), iox); // invalid code(s) added to the block
                }

                if (eob)
                { // if block is finished

                    accumulator.Add(current); // add to the accumulator

                    current = new DecodingBlock(); // and start a new block
                }
            }

            return accumulator.Any();
        }

        private static byte[] FromBx20StringInternal(string input)
        {
            var accumulator = new List<DecodingBlock>();
            var current = new DecodingBlock();

            MarkupBlocks(ref current, accumulator, input, true);

            return DecodeBlockSequence(input, accumulator);
        }

        internal static unsafe byte[] DecodeBlockSequence(string input, List<DecodingBlock> accumulator)
        {
            var dataLength = accumulator.Sum(x => x.DataLength);
            var blocks = accumulator.Count;

            var output = new byte[dataLength];

            fixed (byte* data = &output[0]) {

                fixed (char* code = &input.ToCharArray()[0]) {

                    var codeOffset = 0; // offset in code buffer
                    var dataOffset = 0; // offset in data buffer

                    for (var i = 0; i < blocks; i++) {

                        var pcode = (code + codeOffset);
                        var block = accumulator[i];

                        Debug.Assert(block.Finished);

                        var imprinted = DecodeBlock(pcode, block);

                        byte* pimpmrinted = (byte*) &imprinted;

                        Copy(pimpmrinted, (data + dataOffset), block.DataLength);

                        codeOffset += block.CodeLength;
                        dataOffset += block.DataLength;
                    }
                }
            }

            return output;
        }

        private static unsafe ulong DecodeBlock(char* codeBuffer, DecodingBlock block)
        {
            ulong receptor = 0; // receives imprinted 
            var imprintOffset = 0;

            for (int i = 0; i < block.CodeLength; i++) {

                var code = *(codeBuffer + i);

                if (IsCutoffCharacter(code))
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
    }
}
