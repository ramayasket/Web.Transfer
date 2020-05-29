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
using System.Text.RegularExpressions;
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
        static void Main(string[] args)
        {
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
