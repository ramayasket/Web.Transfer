using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Web.Transfer.Crypto
{
    /// <summary>
    /// Encrypts and decrypts data using AES (Rijndael).
    /// </summary>
    public static class RijndaelEncryptorDecryptor
    {
        // This constant is used to determine the key size of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int KEYSIZE = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 5000;

        public static byte[] Encrypt(byte[] input, string passPhrase)
        {
            using (var password = new Rfc2898DeriveBytes(passPhrase, RijndaelParameters.SALT, DerivationIterations))
            {
                var keyBytes = password.GetBytes(KEYSIZE / 8);

                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, RijndaelParameters.IV))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(input, 0, input.Length);
                                cryptoStream.FlushFinalBlock();

                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var output = memoryStream.ToArray();

                                memoryStream.Close();
                                cryptoStream.Close();

                                return output;
                            }
                        }
                    }
                }
            }
        }

        public static byte[] Decrypt(byte[] input, string passPhrase)
        {
            using (var password = new Rfc2898DeriveBytes(passPhrase, RijndaelParameters.SALT, DerivationIterations))
            {
                var keyBytes = password.GetBytes(KEYSIZE / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, RijndaelParameters.IV))
                    {
                        using (var memoryStream = new MemoryStream(input))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var output = new byte[input.Length * 2];

                                var decryptedByteCount = cryptoStream.Read(output, 0, output.Length);

                                memoryStream.Close();
                                cryptoStream.Close();

                                output = output.Take(decryptedByteCount).ToArray();

                                return output;
                            }
                        }
                    }
                }
            }
        }
    }
}
