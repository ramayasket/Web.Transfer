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
        public SingularDeflaterStream(Stream stream, string password = null) : base(stream)
        {
            Password = password;
            
            // we want compression over speed
            SetLevel(9);

            var entry = new ZipEntry("0") { IsCrypted = !string.IsNullOrWhiteSpace(password) };
            PutNextEntry(entry);
        }
    }
}
