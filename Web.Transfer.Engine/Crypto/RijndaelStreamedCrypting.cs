using System;
using System.IO;
using System.Security.Cryptography;

namespace Web.Transfer.Crypto
{
    /// <summary>
    /// Provides stream-based AES (Rijndael) encryption/decryption.
    /// </summary>
    public class RijndaelStreamedCrypting : IDisposable
    {
        ////
        //// Members to dispose
        ////
        private readonly Rfc2898DeriveBytes _key;
        private readonly RijndaelManaged _symmetricKey;
        private readonly ICryptoTransform _transform;

        /// <summary>
        /// <seealso cref="CryptoStream"/> to write the data to be encrypted to.
        /// </summary>
        /// <remarks>
        /// This stream is disposed as part of <seealso cref="RijndaelStreamedCrypting.Dispose"/>.
        /// </remarks>
        public CryptoStream CryptoStream { get; }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="RijndaelStreamedCrypting"/> class using target stream, password and stream mode.
        /// </summary>
        /// <param name="stream">Target stream object.</param>
        /// <param name="password">Password to encrypt the data with.</param>
        /// <param name="mode">Stream mode.</param>
        public RijndaelStreamedCrypting(Stream stream, string password, CryptoStreamMode mode)
        {
            if (null == stream)
                throw new ArgumentNullException(nameof(stream));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            _key = new Rfc2898DeriveBytes(password, RijndaelParameters.SALT, RijndaelParameters.ITERATIONS);
            _symmetricKey = new RijndaelManaged { BlockSize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };

            var keyBytes = _key.GetBytes(RijndaelParameters.KEYSIZE / 8);

            _transform = (CryptoStreamMode.Read == mode) ?
                _symmetricKey.CreateDecryptor(keyBytes, RijndaelParameters.IV) :
                _symmetricKey.CreateEncryptor(keyBytes, RijndaelParameters.IV);

            CryptoStream = new CryptoStream(stream, _transform, mode);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            CryptoStream.Flush();
            CryptoStream.Dispose();

            _transform.Dispose();
            _symmetricKey.Dispose();
            _key.Dispose();
        }
    }
}
