using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace model.Base32
{
    public partial class Base32Core
    {
        /// <summary>
        /// A FIFO pipe to decode Base-32 string that has been cut to fragments.
        /// </summary>
        public sealed class DecodePipe : IDisposable
        {
            private readonly Action<byte[]> _handler;

            private DecodingBlock _block;
            private readonly StringBuilder _builder = new StringBuilder();
            private readonly List<DecodingBlock> _accumulator = new List<DecodingBlock>();

            /// <summary>
            /// Initializes a new instance of the <seealso cref="DecodePipe"/> using the specified decoded data handler.
            /// </summary>
            /// <param name="handler">A handler to receive decoded data.</param>
            public DecodePipe(Action<byte[]> handler)
            {
                _handler = handler;
                _block = new DecodingBlock();
            }

            /// <summary>
            /// Feeds a fragment of Base-32 encoded string into the pipe.
            /// </summary>
            /// <param name="fragment">String fragment.</param>
            /// <param name="final">True if this is the last fragment.</param>
            public void FeedBase32Fragment(string fragment, bool final = false)
            {
                _builder.Append(fragment);

                var finished = BlockMarkup(ref _block, _accumulator, fragment, final);

                if (finished) {

                    var codeLength = _accumulator.Sum(x => x.CodeLength);

                    var input = _builder.ToString();
                    var output = DecodeBlockSequence(input, _accumulator);

                    var unfinished = input.Substring(codeLength, input.Length - codeLength);

                    _builder.Clear();
                    _builder.Append(unfinished);
                    _accumulator.Clear();

                    //
                    // Now feed output to the handler
                    //
                    _handler(output);
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                var fragment = _builder.ToString();

                if (!string.IsNullOrEmpty(fragment) || _block.CodeLength > 0)
                    throw new InvalidOperationException();
            }
        }
    }
}
