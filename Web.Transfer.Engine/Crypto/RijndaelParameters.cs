using System.Diagnostics.CodeAnalysis;

namespace Web.Transfer.Crypto
{
    /// <summary>
    /// Defines salt and IV bytes (static rather than newly-generated each time).
    /// </summary>
    /// <remarks>
    /// At the time of inception of this class it is not decided should these bytes be permanent or transient for a while.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public class RijndaelParameters
    {
        /// <summary> Key size in bits. </summary>
        public const int KEYSIZE = 256;

        /// <summary> Number of key derivation iterations. </summary>
        public const int ITERATIONS = 5000;

        /// <summary>
        /// Salt (256 bits of random entropy).
        /// </summary>
        public static readonly byte[] SALT = new byte[]
        {
            0xe0, 0xe2, 0x17, 0x65, 0xb3, 0xaa, 0x9b, 0x8e,
            0x20, 0xa1, 0x33, 0x20, 0x76, 0x4b, 0x55, 0x24,
            0x71, 0xac, 0xc2, 0x2e, 0x58, 0xcb, 0x4a, 0xf8,
            0x21, 0x24, 0x2d, 0xd3, 0xd0, 0x87, 0xfb, 0x54
        };

        /// <summary>
        /// Salt (256 bits of random entropy).
        /// </summary>
        public static readonly byte[] IV = new byte[]
        {
            0x28, 0x78, 0x61, 0xcb, 0x94, 0x5c, 0x5e, 0xeb,
            0x34, 0xf9, 0xc3, 0x30, 0xa4, 0xe0, 0xef, 0xa5,
            0xb8, 0xff, 0x31, 0x59, 0xe6, 0x45, 0x8f, 0xd0,
            0xf3, 0x09, 0x53, 0x69, 0x11, 0x0b, 0x8e, 0x7b
        };
    }
}
