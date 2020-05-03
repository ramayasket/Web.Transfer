using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace model.Zip
{
    public class SingularInflater : ZipFile
    {
        private readonly ZipEntry _entry;

        public Stream InputStream => GetInputStream(_entry);

        /// <inheritdoc />
        public SingularInflater(Stream stream) : base(stream)
        {
            _entry = GetEntry("0");
        }
    }
}
