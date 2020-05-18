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
using model.Base32;
using model.Crypto;
using model.Helpers;
using model.Zip;

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

        static void Main(string[] args)
        {
            MemoryEncodeStreaming();
            //TransformationModel();
            //CharArrayingTest();
            //SplitStringDecodeTest();
            //GrossTest();
            //MiddleTest();
        }

        private static void MemoryEncodeStreaming()
        {
            Console.WriteLine("Starting \"to\" transform...");

            var BUFFER_SIZE = 1024 * 256;
            var sourceData = new byte[BUFFER_SIZE];

            for (int i = 0; i < BUFFER_SIZE; i++) {
                sourceData[i] = (byte) i;
            }

            Console.WriteLine("Writing to source stream");

            using (var source = new MemoryStream()) {

                source.Write(sourceData, 0, BUFFER_SIZE);
                source.Seek(0, SeekOrigin.Begin);

                using (var target = new MemoryStream()) {

                    using (var encodingStream = new Base32EncodingStream(target)) {

                        var buffer = new byte[1024];

                        var total = StreamHelper.ReadAndWriteAll(source, encodingStream, buffer);
                        Console.WriteLine($@"Total number of bytes read/encoded is {total}");
                    }

                    target.Seek(0, SeekOrigin.Begin);
                    var targetData = target.GetBuffer();

                    var encoded = Encoding.UTF8.GetString(targetData);

                    var checkData = Base32Core.FromBase32String(encoded);

                    var isok = checkData.SequenceEqual(sourceData);
                }
            }

            //source.Write();

            //using (var inputStream = File.OpenRead(INPUT)) {

            //    using (var outputStream = File.OpenWrite(BASE32)) {

            //        using (var encodingStream = new Base32EncodingStream(outputStream)) {

            //            var total = StreamHelper.ReadAndWriteAll(inputStream, encodingStream, buffer);
            //            Console.WriteLine($@"Total number of bytes read/deflated is {total}");
            //        }
            //    }
            //}
        }

        private static void DecodeStreaming()
        {
            string BASE32 = TestData.testpath("base32");
            string OUTPUT = TestData.testpath("output");

            Console.WriteLine("Starting \"from\" transform...");

            var BUFFER_SIZE = 1024;
            var buffer = new byte[BUFFER_SIZE];

            using (var inputStream = File.OpenRead(BASE32)) {

                using (var outputStream = File.OpenWrite(OUTPUT)) {

                    using (var decodingStream = new Base32DecodingStream(outputStream)) {

                        var total = StreamHelper.ReadAndWriteAll(inputStream, decodingStream, buffer);
                        Console.WriteLine($@"Total number of bytes read/decoded is {total}");
                    }
                }
            }
        }

        private static void EncodeStreaming()
        {
            string INPUT = TestData.testpath("input");
            string BASE32 = TestData.testpath("base32");

            TestData.Cleanup();

            Console.WriteLine("Starting \"to\" transform...");

            var BUFFER_SIZE = 1024;
            var buffer = new byte[BUFFER_SIZE];

            using (var inputStream = File.OpenRead(INPUT)) {

                using (var outputStream = File.OpenWrite(BASE32)) {

                    using (var encodingStream = new Base32EncodingStream(outputStream)) {

                        var total = StreamHelper.ReadAndWriteAll(inputStream, encodingStream, buffer);
                        Console.WriteLine($@"Total number of bytes read/encoded is {total}");
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

        private static void PortionTest()
        {
            var bytes1 = File.ReadAllBytes("C:\\1.input");
            var length1 = bytes1.Length;

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

                offset += PORTION;
            }

            var soutput = output.ToString();

            File.WriteAllText($"C:\\1.base32-{PORTION}", soutput);

            var bytes3 = Base32Core.FromBase32String(soutput);
            var length3 = bytes3.Length;

            File.WriteAllBytes("C:\\1.output", bytes3);

            var isok = bytes1.SequenceEqual(bytes3);

            Console.WriteLine($"isok {isok}: {total}/{length3}");
        }

        private static void GrossTest(bool report = false)
        {
            var bytes1 = File.ReadAllBytes("C:\\1.input");
            var length1 = bytes1.Length;

            string base32 = null;
            byte[] bytes2 = null;

            var encodeTime = ExecutionTimings.Measure(() => { base32 = Base32Core.ToBase32String(bytes1); }).TotalMilliseconds;

            var slength = base32.Length;

            File.WriteAllText("C:\\1.base32", base32);

            var decodeTime = ExecutionTimings.Measure(() => { bytes2 = Base32Core.FromBase32String(base32); }).TotalMilliseconds;

            if (report) {

                Console.WriteLine($"{length1} bytes encoded in {encodeTime} ms: {encodeTime / (1.0 * length1 / (1024 * 1024))} ms/MB");
                Console.WriteLine($"{slength} characters decoded in {decodeTime} ms: {decodeTime / (1.0 * slength / (1024 * 1024))} ms/MB");

                Console.WriteLine($"{length1} bytes encoded in {encodeTime} ms: {encodeTime / (1.0 * length1 / (1024 * 1024 * 1024))} ms/GB");
                Console.WriteLine($"{slength} characters decoded in {decodeTime} ms: {decodeTime / (1.0 * slength / (1024 * 1024 * 1024))} ms/GB");
            }

            File.WriteAllBytes("C:\\1.output", bytes2);

            var isok = bytes1.SequenceEqual(bytes2);

            Console.WriteLine($"Encode/decode is consistent: {isok}");
        }

        private static void CharArrayingTest()
        {
            const int MBYTE = 1024 * 1024;
            const int SAMPLE = 128 * MBYTE;

            var l = new List<char>();

            var t1 = ExecutionTimings.Measure(() => {
                for (int i = 0; i < SAMPLE; i++)
                    l.Add((char)(i + '0'));
            }).TotalMilliseconds;

            var t2 = ExecutionTimings.Measure(() => { var array = l.ToArray(); }).TotalMilliseconds;

            Console.WriteLine($"{SAMPLE} characters collected in {t1} ms: {t1 / (1.0 * SAMPLE / (1024 * 1024))} ms/MB");
            Console.WriteLine($"{SAMPLE} characters arrayed in {t2} ms: {t2 / (1.0 * SAMPLE / (1024 * 1024))} ms/MB");
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
