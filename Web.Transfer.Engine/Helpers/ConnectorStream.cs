using System;
using System.IO;

namespace Web.Transfer.Helpers
{
    /// <summary>
    /// Simple stream which converts writing into reading.
    /// </summary>
    // TODO: rework with Bx20DecodingReadStream as an example
    public class ConnectorStream : Stream
    {
        private const string NOT_SUPPORTED_MESSAGE = "The stream does not support this operation";

        public int Capacity { get; }

        private readonly byte[] _buffer;
        private int _currentSize = 0;

        public ConnectorStream(int capacity)
        {
            Capacity = capacity;
            _buffer = new byte[capacity];
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (null == buffer)
                throw new ArgumentNullException(nameof(buffer));

            if (count > Capacity)
                throw new ArgumentOutOfRangeException(nameof(count), "Size of write operation must not be greater than capacity");

            var length = Math.Min(_currentSize, count);

            Array.Copy(_buffer, 0, buffer, 0, length);
            _currentSize = 0;

            return length;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            if(null == buffer)
                throw new ArgumentNullException(nameof(buffer));

            if (count > Capacity)
                throw new ArgumentOutOfRangeException(nameof(count), "Size of write operation must not be greater than capacity");

            Array.Copy(buffer, 0, _buffer, 0, count);
            _currentSize = count;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override void Flush() { /* this stream has no buffers to flush */ }

        #region Unsupported operations

        /// <inheritdoc />
        public override long Length => _currentSize;

        /// <inheritdoc />
        public override long Position {
            get => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);
            set => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);

        /// <inheritdoc />
        public override void SetLength(long value) => throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);

        #endregion
    }
}
