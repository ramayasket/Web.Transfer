using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using model.Base32;

namespace Web.Transfer.Tests
{
    using Block = Base32Core.DecodingBlock;

    [TestClass]
    public class DecodingBlockTests
    {
        [TestMethod]
        public void Add0()
        {
            var block = new Block();
            Assert.IsFalse(block.Finished);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add1()
        {
            var block = new Block();

            block.AddCode('a', true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add2()
        {
            var block = new Block();

            block.AddCode('a', false);
            block.AddCode('0', true);
        }

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

        [TestMethod]
        public void Add7()
        {
            var block = new Block();

            for (int i = 0; i < 7; i++) {

                block.AddCode('a', false);
                Assert.IsFalse(block.Finished);
            }
        }

        [TestMethod]
        public void Add8()
        {
            var block = new Block();

            for (int i = 0; i < 7; i++) {

                block.AddCode('a', false);
                Assert.IsFalse(block.Finished);
            }

            block.AddCode('a', false);

            Assert.IsTrue(block.Finished);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add9()
        {
            var block = new Block();

            for(int i=0; i<9; i++)
                block.AddCode('a', false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddX()
        {
            var block = new Block();

            block.AddCode('X', false);
        }
    }
}
