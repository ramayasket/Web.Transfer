using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Web.Transfer.Bx20;

namespace Web.Transfer.Tests.Bx20
{
    using Block = Bx20Core.DecodingBlock;

    /// <summary>
    /// Tests how decoding block deals with codes.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DecodingBlockTests
    {
        /// <summary>
        /// Tests that block is unfinished upon creation.
        /// </summary>
        [TestMethod]
        public void Add0()
        {
            var block = new Block();
            Assert.IsFalse(block.Finished);
        }

        /// <summary>
        /// Tests incomplete block detection.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add1()
        {
            var block = new Block();

            block.AddCode('a', true);
        }

        /// <summary>
        /// Tests incomplete block data length exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add1GetDataLength()
        {
            var block = new Block();

            block.AddCode('a', false);
            _ = block.DataLength;
        }

        /// <summary>
        /// Tests incomplete cutoff block detection.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add2()
        {
            var block = new Block();

            block.AddCode('a', false);
            block.AddCode('0', true);
        }

        /// <summary>
        /// Tests incorrect cutoff position detection.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add3()
        {
            var block = new Block();

            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('0', false);
        }

        /// <summary>
        /// Tests incomplete block detection after a complete cutoff block.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add4()
        {
            var block = new Block();

            block.AddCode('a', false);
            block.AddCode('0', false);
            block.AddCode('a', false);
            block.AddCode('a', false);
        }

        /// <summary>
        /// Tests that a native block isn't finished before 8 codes present.
        /// </summary>
        [TestMethod]
        public void Add7()
        {
            var block = new Block();

            for (int i = 0; i < 7; i++) {

                block.AddCode('a', false);
                Assert.IsFalse(block.Finished);
            }
        }

        /// <summary>
        /// Tests that native block is finished when 8 codes present.
        /// </summary>
        [TestMethod]
        public void Add8()
        {
            var block = new Block();

            for (int i = 0; i < 7; i++)
                block.AddCode('a', false);

            block.AddCode('a', false);

            Assert.IsTrue(block.Finished);
            Assert.AreEqual(5, block.DataLength);
        }

        /// <summary>
        /// Tests that extra codes cannot be added to a finished block.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add9()
        {
            var block = new Block();

            for(int i=0; i<9; i++)
                block.AddCode('a', false);
        }

        /// <summary>
        /// Tests that MarkupBlocks processes errors from <seealso cref="Bx20Core.DecodingBlock"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MarkupErrorTest()
        {
            var block = new Block();
            var accumulator = new List<Block>();

            Bx20Core.MarkupBlocks(ref block, accumulator, "a", false);
            Bx20Core.MarkupBlocks(ref block, accumulator, "a", true);
        }

        /// <summary>
        /// Tests that non-Base-32 codes cannot be added to a block.
        /// </summary>
        [TestMethod]
        public void AddX()
        {
            var block = new Block();

            var nonBx20 = new List<char>(); // all the invalid characters

            for (int x = char.MinValue; x <= char.MaxValue; x++) {

                var c = (char) x;

                if(Common.IsEncodeCharacter(c)) // valid Base-32 characters
                    continue;

                nonBx20.Add(c);
            }

            foreach (var c in nonBx20) {
                try {
                    block.AddCode(c, false);
                    Assert.Fail("A non-Base-32 character was added to block");
                }
                catch (InvalidOperationException) {
                }
            }

            Assert.AreEqual(0, block.DataLength);
            Assert.AreEqual(0, block.CodeLength);
        }

        /// <summary>
        /// Tests valid cutoff block of 3 codes.
        /// </summary>
        [TestMethod]
        public void Add3Cutoff()
        {
            var block = new Block();

            block.AddCode('a', false);
            block.AddCode('0', false);
            block.AddCode('a', false);

            Assert.IsTrue(block.Finished);
            Assert.IsTrue(block.IsCutoff);
            Assert.AreEqual(3, block.CodeLength);
            Assert.AreEqual(1, block.DataLength);
        }

        /// <summary>
        /// Tests valid cutoff block of 5 codes.
        /// </summary>
        [TestMethod]
        public void Add5Cutoff()
        {
            var block = new Block();

            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('0', false);
            block.AddCode('a', false);

            Assert.IsTrue(block.Finished);
            Assert.IsTrue(block.IsCutoff);
            Assert.AreEqual(5, block.CodeLength);
            Assert.AreEqual(2, block.DataLength);
        }

        /// <summary>
        /// Tests valid cutoff block of 6 codes.
        /// </summary>
        [TestMethod]
        public void Add6Cutoff()
        {
            var block = new Block();

            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('0', false);
            block.AddCode('a', false);

            Assert.IsTrue(block.Finished);
            Assert.IsTrue(block.IsCutoff);
            Assert.AreEqual(6, block.CodeLength);
            Assert.AreEqual(3, block.DataLength);
        }

        /// <summary>
        /// Tests valid cutoff block of 8 codes.
        /// </summary>
        [TestMethod]
        public void Add8Cutoff()
        {
            var block = new Block();

            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('a', false);
            block.AddCode('0', false);
            block.AddCode('a', false);

            Assert.IsTrue(block.Finished);
            Assert.IsTrue(block.IsCutoff);
            Assert.AreEqual(8, block.CodeLength);
            Assert.AreEqual(4, block.DataLength);
        }
    }
}
