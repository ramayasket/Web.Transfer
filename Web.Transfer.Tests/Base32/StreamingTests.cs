using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using model.Base32;
using model.Helpers;

namespace Web.Transfer.Tests.Base32
{
    /// <summary>
    /// Tests Base-32 encoding and decoding streams.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StreamingTests
    {
        private const int BUFFER_SIZE = 20000; // buffer size must be multiple of 5 so that we have no cutoff blocks.
        private byte[] _sampleBytes;
        private string _sampleString;

        [TestInitialize]
        public void CreateSampleData()
        {
            _sampleBytes = new byte[BUFFER_SIZE];

            for (int i = 0; i < BUFFER_SIZE; i++) {
                _sampleBytes[i] = (byte)i;
            }

            _sampleString = Base32Core.ToBase32String(_sampleBytes);
        }

        [TestMethod]
        public void StreamedEncoding()
        {
            using (var source = new MemoryStream()) {

                source.Write(_sampleBytes, 0, BUFFER_SIZE);
                source.Seek(0, SeekOrigin.Begin);

                using (var target = new MemoryStream()) {

                    using (var encodingStream = new Base32EncodingStream(target)) {

                        var buffer = new byte[1000]; // also must be multiple of 5
                        StreamHelper.ReadAndWriteAll(source, encodingStream, buffer);
                    }

                    target.Seek(0, SeekOrigin.Begin);

                    var targetLength = Convert.ToInt32(target.Length);
                    var targetData = target.GetBuffer().Take(targetLength).ToArray();

                    var encoded = Encoding.UTF8.GetString(targetData);
                    var checkData = Base32Core.FromBase32String(encoded);

                    Assert.AreEqual(encoded, _sampleString);
                    Assert.IsTrue(checkData.SequenceEqual(_sampleBytes));
                }
            }
        }
    }
}
