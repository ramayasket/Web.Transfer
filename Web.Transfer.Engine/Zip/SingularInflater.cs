using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace Web.Transfer.Zip
{
    [ExcludeFromCodeCoverage]
    public class SingularInflater : ZipFile
    {
        private readonly ZipEntry _entry;

        public Stream InputStream => GetInputStream(_entry);

        /// <inheritdoc />
        public SingularInflater(Stream stream, string password = null) : base(stream)
        {
            Password = password;
            _entry = GetEntry("0");
        }
    }
}
