using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Kw.Common;
using model.Base32;
using model.Crypto;
using model.Helpers;
using model.Zip;

namespace model
{
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

        static void Main(string[] args)
        {
            GrossTest();
        }

        private static void MiddleTest()
        {
            var PORTION = 1024;
            var portions = 15;
            var extras = 3;

            var total = PORTION * portions + extras;

            var bytes1 = new byte[total];

            for (int i = 0; i < total; i++)
                bytes1[i] = (byte) i;

            var offset = 0;
            var output = new StringBuilder();

            while (offset < total) {
                var length = Math.Min(PORTION, total - offset);
                var buffer = new byte[length];

                Array.Copy(bytes1, offset, buffer, 0, length);

                var s = Base32Core.ToBase32String(buffer);
                output.Append(s);

                offset += PORTION;
            }

            var soutput = output.ToString();

            var bytes3 = Base32Core.FromBase32String(soutput);
            var length3 = bytes3.Length;

            var isok = bytes1.SequenceEqual(bytes3);

            Console.WriteLine($"isok {isok}: {total}/{length3}");
        }

        private static void SmallTest()
        {
            var bytes1 = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
            var swhole = Base32Core.ToBase32String(bytes1); // "k4oz3o2460h"

            string compound = String.Empty;

            foreach (var x in bytes1) {

                var xa = new[] { x };
                var sxa = Base32Core.ToBase32String(xa);

                compound += sxa;
            }

            var bytes2 = Base32Core.FromBase32String(compound);
        }

        private static void GrossTest()
        {
            var bytes1 = File.ReadAllBytes("C:\\1.input");
            var length1 = bytes1.Length;

            //var base32 = Base32Core.ToBase32String(bytes1);

            //File.WriteAllText("C:\\1.base32", base32);

            //var bytes2 = Base32Core.FromBase32String(base32);

            //File.WriteAllBytes("C:\\1.output", bytes2);

            const int PORTION = 1024;
            var offset = 0;

            var total = bytes1.Length;
            var output = new StringBuilder();

            while (offset < total) {

                var length = Math.Min(PORTION, total - offset);
                var buffer = new byte[length];

                Array.Copy(bytes1, offset, buffer, 0, length);

                var s = Base32Core.ToBase32String(buffer);
                output.Append(s);
                //Console.WriteLine($"offset {offset} length {length}");

                offset += PORTION;
            }

            var soutput = output.ToString();

            File.WriteAllText($"C:\\1.base32-{PORTION}", soutput);

            var bytes3 = Base32Core.FromBase32String(soutput);
            var length3 = bytes3.Length;

            File.WriteAllBytes("C:\\1.output", bytes3);

            var isok = bytes1.SequenceEqual(bytes3);

            Console.WriteLine($"isok {isok}: {total}/{length3}");

            //foreach (var x in _data)
            //{
            //    Console.WriteLine("Base32 encoding:");
            //    Console.WriteLine($"bytes (1): '{BitConverter.ToString(x)}' ({x.Length} bytes)");

            //    var s = Base32Core.ToBase32String(x);
            //    Console.WriteLine($"B/32 text: '{s}' ({s.Length} characters)");

            //    var x1 = Base32Core.FromBase32String(s);
            //    Console.WriteLine($"bytes (2): '{BitConverter.ToString(x1)}' ({x1.Length} bytes)");

            //    var equality = x.SequenceEqual(x1) ? "equal" : "not equal";
            //    Console.WriteLine($"bytes (2) are {equality} to bytes (1)");
            //}
        }

        static unsafe void DoWithData(byte[] input)
        {
            fixed (byte* pInput = &input[0])
            {
                int* pInt2 = (int*)pInput;
                Console.WriteLine($"Int at address of the array is: {*pInt2}");
            }
        }
    }
}
