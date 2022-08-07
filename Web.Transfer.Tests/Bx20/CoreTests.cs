using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Web.Transfer.Bx20;

namespace Web.Transfer.Tests.Bx20
{
    /// <summary>
    /// Tests core encoding/decoding functionality.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CoreTests
    {
        /// <summary>
        /// Simple sample data. Index to the outer array is number of bytes in the inner array.
        /// </summary>
        static readonly byte[][] SampleData =
        {
            new byte[] { },
            new byte[] { 0xab },
            new byte[] { 0x80, 0x52 },
            new byte[] { 0x08, 0x00, 0x52 },
            new byte[] { 0x13, 0x01, 0x19, 0x68 },
            new byte[] { 0xab, 0x13, 0x01, 0x19, 0x68 },
        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EncodeNull()
        {
            var encoded = Bx20Core.ToBx20String(null);
        }

        [TestMethod]
        public void EncodeEmpty()
        {
            var encoded = Bx20Core.ToBx20String(new byte[0]);
            Assert.AreEqual(String.Empty, encoded);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DecodeNull()
        {
            var decoded = Bx20Core.FromBx20String(null);
        }

        [TestMethod]
        public void DecodeEmpty()
        {
            var decoded = Bx20Core.FromBx20String(string.Empty);
            Assert.IsTrue(decoded.SequenceEqual(new byte[0]));
        }

        [TestMethod]
        public void SingleByteEncode()
        {
            for (int i = 0; i < 256; i++) {

                var x = (byte) i;

                var low = x & 0x1f;
                var high = x >> 5;

                var clow = Bx20Core.EncodeTable[low];
                var chigh = Bx20Core.EncodeTable[high];

                var encoded = Bx20Core.ToBx20String(new[] { x });

                Assert.AreEqual(3, encoded.Length);
                Assert.AreEqual(clow, encoded[0]);
                Assert.AreEqual(chigh, encoded[2]);

                var c = encoded[1];

                Assert.IsTrue((c >= '7' && c <= '9') || c == '0');
            }
        }

        [TestMethod]
        public void SingleByteDecode()
        {
            for (int i = 0; i < 256; i++) {

                var x = new[] { (byte) i };

                var encoded = Bx20Core.ToBx20String(x);
                var decoded = Bx20Core.FromBx20String(encoded);

                Assert.IsTrue(x.SequenceEqual(decoded));
            }
        }

        [TestMethod]
        public void Cutoff1Bytes()
        {
            var data = SampleData[1];
            var encoded = Bx20Core.ToBx20String(data);

            Assert.AreEqual(3, encoded.Length);
            Assert.IsTrue(HasCutoff(encoded));
            Assert.IsTrue(Common.IsCutoffCharacter(encoded[1]));
        }

        [TestMethod]
        public void Cutoff2Bytes()
        {
            var data = SampleData[2];
            var encoded = Bx20Core.ToBx20String(data);

            Assert.AreEqual(5, encoded.Length);
            Assert.IsTrue(HasCutoff(encoded));
            Assert.IsTrue(Common.IsCutoffCharacter(encoded[3]));
        }

        [TestMethod]
        public void Cutoff3Bytes()
        {
            var data = SampleData[3];
            var encoded = Bx20Core.ToBx20String(data);

            Assert.AreEqual(6, encoded.Length);
            Assert.IsTrue(HasCutoff(encoded));
            Assert.IsTrue(Common.IsCutoffCharacter(encoded[4]));
        }

        [TestMethod]
        public void Cutoff4Bytes()
        {
            var data = SampleData[4];
            var encoded = Bx20Core.ToBx20String(data);

            Assert.AreEqual(8, encoded.Length);
            Assert.IsTrue(HasCutoff(encoded));
            Assert.IsTrue(Common.IsCutoffCharacter(encoded[6]));
        }

        [TestMethod]
        public void EncodeSmallestNative()
        {
            var data = SampleData[5];
            var encoded = Bx20Core.ToBx20String(data);

            Assert.AreEqual(8, encoded.Length);
            Assert.IsFalse(HasCutoff(encoded));
        }

        private bool HasCutoff(string s)
        {
            var chars = s.ToCharArray();
            var intersect = chars.Intersect(Bx20Core.CutoffTable).ToArray();

            return intersect.Any();
        }
    }
}
