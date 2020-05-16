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
            var s = "aaaaaaaa";
            var b = Base32Core.FromBase32String(s);

            //foreach (var x in _data)
            //{
            //    var s = Base32Core.ToBase32String(x);
            //    Console.WriteLine($"Base32 encoding: source data '{BitConverter.ToString(x)}' ({x.Length} bytes), encoded string '{s}' ({s.Length} characters)");

            //    var x1 = Base32Core.FromBase32String(s);
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
