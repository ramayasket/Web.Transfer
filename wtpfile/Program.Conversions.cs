using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Kw.Common;
using Web.Transfer.Base32;
using Web.Transfer.Crypto;

namespace Web.Transfer
{
    partial class Program
    {
        /// <summary>
        /// Opens input and output streams and calls conversion operation with them as parameters.
        /// </summary>
        /// <param name="source">Input file.</param>
        /// <param name="target">Output file.</param>
        /// <param name="operation">Conversion operation.</param>
        /// <returns></returns>
        static bool WithStreams(string source, string target, Func<Stream, Stream, bool> operation)
        {
            var readStream = GuardedFileOpen(source, false);

            if (null == readStream)
                return false;

            using (readStream) {

                var writeStream = GuardedFileOpen(target, true);

                if (null == writeStream)
                    return false;

                using (writeStream) {

                    Console.WriteLine();
                    var op = operation(readStream, writeStream);

                    return op;
                }
            }
        }

        /// <summary>
        /// Opens file for reading/writing and reports exceptions.
        /// </summary>
        /// <param name="f">File name.</param>
        /// <param name="write">True for write.</param>
        /// <returns>File stream.</returns>
        static Stream GuardedFileOpen(string f, bool write)
        {
            try {
                Stream stream;

                if (write) {
                    File.Delete(f);
                    stream = File.OpenWrite(f);
                }
                else
                    stream = File.OpenRead(f);

                var op = write ? "writing" : "reading";
                Console.WriteLine($"File '{f}' opened for {op}");
                return stream;
            }
            catch (Exception x) {
                Console.WriteLine(x.Message);
                return null;
            }
        }

        ////
        //// Stream factories
        ////
        private static readonly Func<Stream, Stream> Base32DecodeFactory = (ws) => new Base32DecodingReadStream(ws);
        private static readonly Func<Stream, Stream> CryptoDecodeFactory = (ws) => new RijndaelStreamedCrypting(ws, _password, CryptoStreamMode.Read).CryptoStream;
        private static readonly Func<Stream, Stream> DecompressFactory = (ws) => new GZipStream(ws, CompressionMode.Decompress);

        private static readonly Func<Stream, Stream> BaseEncode32Factory = (ws) => new Base32EncodingStream(ws);
        private static readonly Func<Stream, Stream> CryptoEncodeFactory = (ws) => new RijndaelStreamedCrypting(ws, _password, CryptoStreamMode.Write).CryptoStream;
        private static readonly Func<Stream, Stream> CompressFactory = (ws) => new GZipStream(ws, CompressionMode.Compress);

        /// <summary>
        /// Constructs sequence of streams within using blocks and pumps data between streams.
        /// </summary>
        /// <param name="factories">Collection of stream factory methods.</param>
        /// <param name="factory">Index of the factory to use.</param>
        /// <param name="first">First stream.</param>
        /// <param name="last">Last stream.</param>
        /// <param name="pump">Pump method.</param>
        /// <returns>The result of pumping.</returns>
        static bool ConstructAndPump(List<Func<Stream, Stream>> factories, int factory, Stream first, Stream last, Func<Stream, Stream, bool> pump)
        {
            if (factory == factories.Count) {
                return pump(first, last);
            }

            using (var next = factories[factory](first)) {
                return ConstructAndPump(factories, factory + 1, next, last, pump);
            }
        }

        static void PumpIntervention(int pumped)
        {
            _bytesPumped += pumped;

            Console.Write($"\rPumped {_bytesPumped:##,###} bytes: ");
        }

        /// <summary>
        /// Converts input file to WTP file by pumping through sequence of streams.
        /// </summary>
        /// <param name="readStream">Stream to input file.</param>
        /// <param name="writeStream">Stream to output file.</param>
        /// <returns>True of conversion was successful.</returns>
        static bool PumpConvertFrom(Stream readStream, Stream writeStream)
        {
            var factories = new List<Func<Stream, Stream>> { Base32DecodeFactory };

            if (_encrypt)
                factories.Add(CryptoDecodeFactory);

            if (_compress)
                factories.Add(DecompressFactory);

            try {
                return ConstructAndPump(factories, 0, readStream, writeStream, (r, w) =>
                {
                    StreamHelper.PumpAll(r, w, _buffer, PumpIntervention);
                    Console.WriteLine("success");
                    return true;
                });
            }
            catch (Exception x) {
                Console.WriteLine(x.Message);
                return false;
            }
        }

        /// <summary>
        /// Converts input file to WTP file by pumping through sequence of streams.
        /// </summary>
        /// <param name="readStream">Stream to input file.</param>
        /// <param name="writeStream">Stream to output file.</param>
        /// <returns>True of conversion was successful.</returns>
        static bool PumpConvertTo(Stream readStream, Stream writeStream)
        {
            var factories = new List<Func<Stream, Stream>> { BaseEncode32Factory };

            if (_encrypt)
                factories.Add(CryptoEncodeFactory);

            if (_compress)
                factories.Add(CompressFactory);

            try {
                return ConstructAndPump(factories, 0, writeStream, readStream, (w, r) =>
                {
                    StreamHelper.PumpAll(r, w, _buffer, PumpIntervention);
                    Console.WriteLine("success");
                    return true;
                });
            }
            catch (Exception x) {
                Console.WriteLine(x.Message);
                return false;
            }
        }
    }
}
