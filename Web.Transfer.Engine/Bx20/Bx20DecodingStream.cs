using System;
using System.IO;
using System.Text;

namespace Web.Transfer.Bx20
{
    /// <summary>
    /// A stream which Bx20-decode data and writes the output to the underlying stream.
    /// </summary>
    public class Bx20DecodingStream : Stream
    {
        private const string NOT_SUPPORTED_MESSAGE = "The stream does not support this operation";

        private readonly Stream _underlyingStream;
        private readonly Bx20DecodePipe _decodePipe;

        /// <summary>
        /// Initializes a new instance of the <seealso cref="Bx20DecodingStream"/> class using <seealso cref="Stream"/> object to write encoded string to.
        /// </summary>
        /// <param name="underlyingStream">The <seealso cref="Stream"/> object to write encoded string to. Must support writing.</param>
        public Bx20DecodingStream(Stream underlyingStream)
        {
            if (null == underlyingStream)
                throw new ArgumentNullException(nameof(underlyingStream));

            if (!underlyingStream.CanWrite)
                throw new ArgumentException("The underlying stream must support writing");

            _underlyingStream = underlyingStream;
            _decodePipe = new Bx20DecodePipe(OnDecodedData);
        }

        private void OnDecodedData(byte[] data)
        {
            _underlyingStream.Write(data, 0, data.Length);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            var bytes = new byte[count];
            Array.Copy(buffer, offset, bytes, 0, count);

            var encoded = Encoding.UTF8.GetString(bytes);

            _decodePipe.FeedFragment(encoded);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                _underlyingStream.Flush();
                _decodePipe.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override bool CanRead => false;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => true;

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
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);

        /// <inheritdoc />
        public override void SetLength(long value) => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);

        #endregion
    }
}
