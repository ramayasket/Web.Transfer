using System;
using System.IO;
using System.Text;

namespace Web.Transfer.Bx20
{
    /// <summary>
    /// A stream which Bx20-encode data and writes the output to the underlying stream.
    /// </summary>
    public class Bx20EncodingStream : Stream
    {
        private const string NOT_SUPPORTED_MESSAGE = "The stream does not support this operation";

        private readonly Stream _underlyingStream;

        /// <summary>
        /// Initializes a new instance of the <seealso cref="Bx20EncodingStream"/> class using <seealso cref="Stream"/> object to write encoded string to.
        /// </summary>
        /// <param name="underlyingStream">The <seealso cref="Stream"/> object to write encoded string to. Must support writing.</param>
        public Bx20EncodingStream(Stream underlyingStream)
        {
            if (null == underlyingStream)
                throw new ArgumentNullException(nameof(underlyingStream));

            if (!underlyingStream.CanWrite)
                throw new ArgumentException("The underlying stream must support writing");

            _underlyingStream = underlyingStream;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            var bytes = new byte[count];
            Array.Copy(buffer, offset, bytes, 0, count);

            var encoded = Bx20Core.ToBx20String(bytes);
            var encodedBytes = Encoding.UTF8.GetBytes(encoded);

            _underlyingStream.Write(encodedBytes, 0, encodedBytes.Length);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                _underlyingStream.Flush();
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
