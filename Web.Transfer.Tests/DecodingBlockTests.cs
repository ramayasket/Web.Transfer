using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using model.Base32;

namespace Web.Transfer.Tests
{
    [TestClass]
    public class DecodingBlockTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var block = new Base32Core.DecodingBlock();

            block.AddCode('a', false);
        }
    }
}
