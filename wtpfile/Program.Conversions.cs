using Kw.Common;
using System.IO.Compression;
using System.Security.Cryptography;
using Web.Transfer.Bx20;
using Web.Transfer.Crypto;

namespace Web.Transfer
{
    partial class Program
    {
        ////
        //// Stream factories
        ////
        private static readonly Func<Stream, Stream> Bx20DecodeFactory = (ws) => new Bx20DecodingReadStream(ws);
        private static readonly Func<Stream, Stream> CryptoDecodeFactory = (ws) => new RijndaelStreamedCrypting(ws, _password, CryptoStreamMode.Read).CryptoStream;
        private static readonly Func<Stream, Stream> DecompressFactory = (ws) => new GZipStream(ws, CompressionMode.Decompress);

        private static readonly Func<Stream, Stream> Bx20EncodeFactory = (ws) => new Bx20EncodingStream(ws);
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
            if (factory == factories.Count)
            {
                return pump(first, last);
            }

            using (var next = factories[factory](first))
            {
                return ConstructAndPump(factories, factory + 1, next, last, pump);
            }
        }

        static bool PumpConvertFrom(Stream readStream, Stream writeStream)
        {
            var factories = new List<Func<Stream, Stream>> { Bx20DecodeFactory };

            if (_encrypt)
                factories.Add(CryptoDecodeFactory);

            if (_compress)
                factories.Add(DecompressFactory);

            try
            {
                return ConstructAndPump(factories, 0, readStream, writeStream, (r, w) =>
                {
                    StreamHelper.PumpAll(r, w, _buffer);
                    return true;
                });
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
                return false;
            }
        }

        static bool PumpConvertTo(Stream readStream, Stream writeStream)
        {
            var factories = new List<Func<Stream, Stream>> { Bx20EncodeFactory };

            if (_encrypt)
                factories.Add(CryptoEncodeFactory);

            if (_compress)
                factories.Add(CompressFactory);

            try
            {
                return ConstructAndPump(factories, 0, writeStream, readStream, (w, r) =>
                {
                    StreamHelper.PumpAll(r, w, _buffer);
                    return true;
                });
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
                return false;
            }
        }
    }
}
