using System;
using System.IO;
using System.Text;

namespace Web.Transfer.Bx20
{
    /// <summary>
    /// A stream which Bx20-decode data from underlying stream and returns them to the reader stream.
    /// </summary>
    public class Bx20DecodingReadStream : Stream
    {
        private const string NOT_SUPPORTED_MESSAGE = "The stream does not support this operation";
        public const int BUFFER_SIZE = 1024 * 1024; // 1 MB is a decent buffer size

        private readonly Stream _underlyingStream;
        private readonly Bx20DecodePipe _decodePipe;

        /// <summary>
        /// Initializes a new instance of the <seealso cref="Bx20DecodingStream"/> class using <seealso cref="Stream"/> object to write encoded string to.
        /// </summary>
        /// <param name="underlyingStream">The <seealso cref="Stream"/> object to write encoded string to. Must support writing.</param>
        /// <param name="bufferSize"></param>
        public Bx20DecodingReadStream(Stream underlyingStream, int bufferSize = BUFFER_SIZE)
        {
            if (null == underlyingStream)
                throw new ArgumentNullException(nameof(underlyingStream));

            if (!underlyingStream.CanRead)
                throw new ArgumentException("The underlying stream must support reading");

            if (bufferSize < 1024)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer is too small, must be greater or equal than 1024");

            _underlyingStream = underlyingStream;
            _decodePipe = new Bx20DecodePipe(OnDecodedData);

            _inputBuffer = new byte[bufferSize];

            _decodeBuffer = new byte[bufferSize];
            _decodeOffset = bufferSize; // emulate 'all data has been read out' situation
        }

        private readonly byte[] _inputBuffer; // buffer that holds encoded bytes read from underlying stream
        private readonly byte[] _decodeBuffer; // buffer that holds decoded bytes to return to the reader stream

        private int _decodeDataLength = 0; // size of the last decoded portion of data
        private int _decodeOffset = 0; // offset from which to return data to the reader stream

        private bool HasData => _decodeOffset < _decodeDataLength; // do we need to read the underlying stream?
        private bool _endOfStream = false; // EOF!

        /// <summary>
        /// Receives decoded data from the pipe.
        /// </summary>
        private void OnDecodedData(byte[] data)
        {
            Array.Copy(data, 0, _decodeBuffer, 0, data.Length);

            _decodeDataLength = data.Length;
            _decodeOffset = 0;
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_endOfStream)
                return 0;

            if (!HasData)
            { // if all decoded data has been returned to the reader

                // go for a new portion
                var inputCount = _underlyingStream.Read(_inputBuffer, 0, _inputBuffer.Length);

                if (0 == inputCount)
                {
                    _endOfStream = true;
                    return 0;
                }

                // push the new data to the pipe which is putting the output to _decodeBuffer
                _decodePipe.FeedFragment(Encoding.UTF8.GetString(_inputBuffer, 0, inputCount));
            }

            var remainingLength = _decodeDataLength - _decodeOffset;
            var readingLength = Math.Min(remainingLength, count);

            Array.Copy(_decodeBuffer, _decodeOffset, buffer, offset, readingLength);

            _decodeOffset += readingLength;

            return readingLength;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                _decodePipe.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override void Flush() { /* this stream has no buffers to flush */ }

        #region Unsupported operations

        /// <inheritdoc />
        public override long Length => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);

        /// <inheritdoc />
        public override long Position
        {
            get => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);
            set => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);

        /// <inheritdoc />
        public override void SetLength(long value) => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);

        #endregion
    }
}
