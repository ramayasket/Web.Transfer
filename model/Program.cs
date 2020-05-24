using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Kw.Common;
using Web.Transfer.Base32;
using Web.Transfer.Helpers;
using Web.Transfer.Crypto;

namespace model
{
    [ExcludeFromCodeCoverage]
    partial class Program
    {
        static void GZipHelperTest()
        {
            const int BUFFER_SIZE = 20000;
            byte[] sample;

            sample = new byte[BUFFER_SIZE];

            for (int i = 0; i < BUFFER_SIZE; i++) {
                sample[i] = (byte)i;
            }

            var packed = sample.Pack();
            var unpacked = packed.Unpack();

            var ok = sample.SequenceEqual(unpacked);
        }

        static void Main(string[] args)
        {
            var wf = "1.bin";

            return;

            long i = 12345678909876;

            var s = $"{i:##,###}";

            return;
            
            GZipHelperTest();
            return;

            const string PASSWORD = "zlp";

            var BUFFER_SIZE = 1024;
            var buffer = new byte[BUFFER_SIZE];

            string INPUT = TestData.testpath("input");
            string WTP = TestData.testpath("wtp");
            string OUTPUT = TestData.testpath("output");

            var input = File.ReadAllBytes(INPUT);

            TestData.Cleanup();

            Console.WriteLine("Conversion to protocol");

            using (var readStream = File.OpenRead(INPUT)) {
                using (var writeStream = File.OpenWrite(WTP)) {
                    using (var base32Encoder = new Base32EncodingStream(writeStream)) {
                        using (var cryptoEncoder = new RijndaelStreamedCrypting(base32Encoder, PASSWORD, CryptoStreamMode.Write)) {
                            using (var compressStream = new GZipStream(cryptoEncoder.CryptoStream, CompressionMode.Compress)) {
                                StreamHelper.PumpAll(readStream, compressStream, buffer);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Conversion from protocol");

            using (var readStream = File.OpenRead(WTP)) {
                using (var writeStream = File.OpenWrite(OUTPUT)) {
                    using (var base32Decoder = new Base32DecodingReadStream(readStream)) {
                        using (var cryptoDecoder = new RijndaelStreamedCrypting(base32Decoder, PASSWORD, CryptoStreamMode.Read)) {
                            using (var decompressStream = new GZipStream(cryptoDecoder.CryptoStream, CompressionMode.Decompress)) {
                                StreamHelper.PumpAll(decompressStream, writeStream, buffer);
                            }
                        }
                    }
                }
            }

            var output = File.ReadAllBytes(OUTPUT);
            var isok = input.SequenceEqual(output);

            Console.WriteLine($"Conversion to/from: {isok}");
        }

        private static void SplitStringDecodeTest()
        {
            const int DATA_SIZE = 13;
            const int FRAGMENT_SIZE = 10;

            byte[] data1 = new byte[DATA_SIZE];

            for (int i = 0; i < DATA_SIZE; i++)
                data1[i] = (byte) i;

            string base32 = Base32Core.ToBase32String(data1) + "a";
            var length32 = base32.Length;

            var accumulator = new List<byte>();
            Action<byte[]> handler = (i) => { foreach (var x in i) accumulator.Add(x); };

            using (var pipe = new Base32DecodePipe(handler)) {

                var offset = 0;
                while (offset < base32.Length)
                {

                    var length = Math.Min(FRAGMENT_SIZE, base32.Length - offset);
                    var progress = length + offset;

                    var final = progress == base32.Length;

                    var fragment = base32.Substring(offset, length);
                    pipe.FeedFragment(fragment, final);

                    offset += FRAGMENT_SIZE;
                }
            }

            var data2 = accumulator.ToArray();

            var isok = data2.SequenceEqual(data1);

        }
    }
}
