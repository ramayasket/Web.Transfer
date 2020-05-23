using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Web.Transfer.Base32;

namespace Web.Transfer.Tests.Base32
{
    /// <summary>
    /// Tests <seealso cref="Base32DecodePipe"/>
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DecodePipeTests
    {
        private readonly List<byte> _output = new List<byte>();

        private void Handler(byte[] data)
        {
            foreach (var x in data)
                _output.Add(x);
        }

        private byte[] Output => _output.ToArray();

        [TestInitialize]
        public void MyTestInitialize()
        {
            _output.Clear();
        }

        [TestMethod]
        public void CtorTest()
        {
            var pipe = new Base32DecodePipe(Handler);
        }
    }
}
