using System;

namespace Web.Transfer.Bx20
{
    public partial class Bx20Core
    {
        private static unsafe string ToBx20StringInternal(byte[] array)
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

            while (offset < imprintedBits)
            {

                var bits = Math.Min(ENCODING_BITS, imprintedBits - offset);

                var mask = CutoffMasks[bits];

                var encode = imprinted & mask;
                var encoded = EncodeTable[encode];

                if (bits < ENCODING_BITS)
                {
                    output[ox++] = MakeCutoffCharacter();
                }

                output[ox++] = encoded;

                imprinted >>= bits;
                offset += bits;
            }

            return output;
        }
    }
}
