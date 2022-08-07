using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Kw.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Web.Transfer.Bx20;
using Web.Transfer.Helpers;

namespace Web.Transfer.Tests.Bx20
{
    /// <summary>
    /// Tests Base-32 encoding and decoding streams.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StreamingTests
    {
        ////
        //// Buffer size must be multiple of 5 so that we have no cutoff blocks.
        //// Because of randomized cutoffs, verifying encoded string with cutoffs would become unreasonably difficult.
        //// Since this class tests streams and not encoding/decoding functionality itself,
        //// this 
        private const int BUFFER_SIZE = 20000;
        private byte[] _sampleBytes;
        private string _sampleString;

        [TestInitialize]
        public void CreateSampleData()
        {
            _sampleBytes = new byte[BUFFER_SIZE];

            for (int i = 0; i < BUFFER_SIZE; i++) {
                _sampleBytes[i] = (byte)i;
            }

            _sampleString = Bx20Core.ToBx20String(_sampleBytes);
        }

        [TestMethod]
        public void StreamedEncoding()
        {
            using (var source = new MemoryStream()) {

                source.Write(_sampleBytes, 0, BUFFER_SIZE);
                source.Seek(0, SeekOrigin.Begin);

                using (var target = new MemoryStream()) {

                    using (var encodingStream = new Bx20EncodingStream(target)) {

                        var buffer = new byte[1000]; // also must be multiple of 5
                        StreamHelper.PumpAll(source, encodingStream, buffer);
                    }

                    target.Seek(0, SeekOrigin.Begin);

                    var targetLength = Convert.ToInt32(target.Length);
                    var targetData = target.GetBuffer().Take(targetLength).ToArray();

                    var encoded = Encoding.UTF8.GetString(targetData);
                    var checkData = Bx20Core.FromBx20String(encoded);

                    Assert.AreEqual(encoded, _sampleString);
                    Assert.IsTrue(checkData.SequenceEqual(_sampleBytes));
                }
            }
        }

        [TestMethod]
        public void StreamedDecoding()
        {
            using (var source = new MemoryStream()) {

                var sourceBytes = Encoding.UTF8.GetBytes(_sampleString);

                source.Write(sourceBytes, 0, sourceBytes.Length);
                source.Seek(0, SeekOrigin.Begin);

                using (var target = new MemoryStream()) {

                    using (var decodingStream = new Bx20DecodingStream(target)) {

                        var buffer = new byte[1024];
                        StreamHelper.PumpAll(source, decodingStream, buffer);
                    }

                    target.Seek(0, SeekOrigin.Begin);

                    var targetLength = Convert.ToInt32(target.Length);
                    var targetData = target.GetBuffer().Take(targetLength).ToArray();

                    Assert.IsTrue(targetData.SequenceEqual(_sampleBytes));
                }
            }
        }
    }
}
