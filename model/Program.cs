using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Kw.Common;
using Web.Transfer.Base32;
using Web.Transfer.Helpers;
using Web.Transfer.Crypto;

namespace model
{
    [ExcludeFromCodeCoverage]
    partial class Program
    {
        static readonly byte[][] _data =
        {
            //new byte[] { },
            //new byte[] { 0xab },
            //new byte[] { 0x80, 0x52 },
            //new byte[] { 0x08, 0x00, 0x52 },
            //new byte[] { 0x13, 0x01, 0x19, 0x68 },
            //new byte[] { 0xab, 0x13, 0x01, 0x19, 0x68 },
            new byte[] { 0xab, 0x13, 0x01, 0x19, 0x68, 0xba, 0x31, 0x10, 0x91, 0x86 },
            new byte[] { 0xab, 0x13, 0x01, 0x19, 0x68, 0xba, 0x31, 0x10, 0x91, 0x86, 0xff },
        };

        private const int DATA_SIZE = 16;
        private const int BUFFER_SIZE = 2048;
        private static byte[] _sampleBytes;

        static void Main(string[] args)
        {
            const string PASSWORD = "zlp";

            var BUFFER_SIZE = 2048;
            var buffer = new byte[BUFFER_SIZE];

            //using (var inputStream = File.OpenRead("1.png")) {
            //    using (var outputStream = File.OpenWrite("1.png.encrypted")) {
            //        using (var crypting = new RijndaelStreamedCrypting(outputStream, PASSWORD, CryptoStreamMode.Write)) {
            //            var total = StreamHelper.PumpAll(inputStream, crypting.CryptoStream, buffer);
            //            Console.WriteLine($@"Total number of bytes read is {total}");
            //        }
            //    }
            //}


            //using (var inputStream = File.OpenRead("3.png.encrypted")) {
            //    using (var outputStream = File.OpenWrite("2.png")) {
            //        using (var crypting = new RijndaelStreamedCrypting(inputStream, PASSWORD, CryptoStreamMode.Read)) {
            //            var total = StreamHelper.PumpAll(crypting.CryptoStream, outputStream, buffer);
            //            Console.WriteLine($@"Total number of bytes written is {total}");
            //        }
            //    }
            //}

            using (var inputStream = File.OpenRead("2.png.wtp")) {
                using (var base32Decoder = new Base32DecodingReadStream(inputStream)) {
                    using (var writeStream = File.OpenWrite("2.png.encrypted")) {
                        var total = StreamHelper.PumpAll(base32Decoder, writeStream, buffer);
                        Console.WriteLine($@"Total number of bytes written is {total}");
                    }
                }
            }
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
