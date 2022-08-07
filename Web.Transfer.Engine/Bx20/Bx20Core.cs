using System;

namespace Web.Transfer.Bx20
{
    /// <summary>
    /// Bx20 encoding and decoding.
    /// </summary>
    public static partial class Bx20Core
    {
        /// <summary>
        /// Converts the value of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with Bx20 digits.
        /// </summary>
        /// <param name="array">Input array.</param>
        /// <returns>Bx20 encoded string.</returns>
        public static string ToBx20String(byte[] array)
        {
            if (null == array)
                throw new ArgumentNullException(nameof(array));

            if (0 == array.Length)
                return String.Empty;

            return ToBx20StringInternal(array);
        }

        /// <summary>
        /// Converts the specified string, which encodes binary data as Bx20 digits, to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>Array of decoded data.</returns>
        public static byte[] FromBx20String(string input)
        {
            if (null == input)
                throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrEmpty(input))
                return new byte[0];

            return FromBx20StringInternal(input);
        }
    }
}
