using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace model.Zip
{
    public class SingularDeflaterStream : ZipOutputStream
    {
        /// <inheritdoc />
        public SingularDeflaterStream(Stream baseOutputStream) : base(baseOutputStream)
        {
            // we want compression over speed
            SetLevel(9);

            var entry = new ZipEntry("0");
            PutNextEntry(entry);
        }
    }
}
