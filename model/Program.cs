using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        static readonly byte[][] _data = new[]
        {
            new byte[] { 0xab },
            new byte[] { 0x80, 0x52 },
            new byte[] { 0x08, 0x00, 0x52 },
            new byte[] { 0x13, 0x01, 0x19, 0x68 },
            new byte[] { 0xab, 0x13, 0x01, 0x19, 0x68 },
        };

        static void Main()
        {
            var data = new []
            {
                new byte[] { 0xab },
                new byte[] { 0x80, 0x52 },
                new byte[] { 0x08, 0x00, 0x52 },
                new byte[] { 0x13, 0x01, 0x19, 0x68 },
                new byte[] { 0xab, 0x13, 0x01, 0x19, 0x68 },
            };

            foreach (var a in _data)
            {
                DoWithData(a);
            }

            for (int i = 0; i < 11; i++)
            {
                var encoded = Base32Core.GetEncodedLength(i);
                //Console.WriteLine($"Block size {i}, encoded size {encoded}");
            }
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
