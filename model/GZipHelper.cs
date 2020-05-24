using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;
using Web.Transfer.Helpers;

namespace model
{
    public static class GZipHelper
    {
        public static byte[] Pack(this byte[] input)
        {
            var buffer = new byte[1024];

            using (var inputStream = new MemoryStream(input)) {
                using (var outputStream = new MemoryStream()) {
                    using (var compressStream = new GZipStream(outputStream, CompressionMode.Compress)) {
                        StreamHelper.PumpAll(inputStream, compressStream, buffer);
                        //outputStream.Seek(0, SeekOrigin.Begin);
                        var output = outputStream.GetBuffer().Take((int) outputStream.Length).ToArray();
                        return output;
                    }
                }
            }
        }

        public static byte[] Unpack(this byte[] input)
        {
            var buffer = new byte[1024];

            using (var inputStream = new MemoryStream(input)) {
                using (var outputStream = new MemoryStream()) {
                    using (var decompressStream = new GZipStream(inputStream, CompressionMode.Decompress)) {
                        StreamHelper.PumpAll(decompressStream, outputStream, buffer);
                        //outputStream.Seek(0, SeekOrigin.Begin);
                        var output = outputStream.GetBuffer().Take((int)outputStream.Length).ToArray();
                        return output;
                    }
                }
            }
        }
    }
}
