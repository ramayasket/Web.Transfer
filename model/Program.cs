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
        static void Main()
        {
            var input = new []
            {
                new byte[] { 0x00 },
                new byte[] { 0x80, 0x52 },
                new byte[] { 0x08, 0x00, 0x52 },
                new byte[] { 0x13, 0x01, 0x19, 0x68 },
                new byte[] { 0xab, 0x13, 0x01, 0x19, 0x68 },
            };

            for (int i = 0; i < 11; i++)
            {
                var encoded = Base32Core.GetEncodedLength(i);
                //Console.WriteLine($"Block size {i}, encoded size {encoded}");
            }

            //foreach (var bytes in input)
            //{
            //}
        }
    }
}
