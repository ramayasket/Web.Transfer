using System;
using System.Collections.Generic;
using System.Linq;

namespace Web.Transfer.Base32
{
    /// <summary>
    /// Base-32 encoding and decoding.
    /// </summary>
    public static partial class Base32Core
    {
        /// <summary>
        /// Converts the value of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with Base-32 digits.
        /// </summary>
        /// <param name="array">Input array.</param>
        /// <returns>Base-32 encoded string.</returns>
        public static string ToBase32String(byte[] array)
        {
            if (null == array)
                throw new ArgumentNullException(nameof(array));

            if (0 == array.Length)
                return String.Empty;

            return ToBase32StringInternal(array);
        }

        /// <summary>
        /// Converts the specified string, which encodes binary data as Base-32 digits, to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>Array of decoded data.</returns>
        public static byte[] FromBase32String(string input)
        {
            if (null == input)
                throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrEmpty(input))
                return new byte[0];

            return FromBase32StringInternal(input);
        }
    }
}
