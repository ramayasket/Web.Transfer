using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Kw.Common;
using model.Crypto;
using model.Helpers;
using model.Zip;

namespace model
{
    partial class Program
    {
        static void Main()
        {
            File.WriteAllBytes("C:\\salt", RijndaelParameters.SALT);
            File.WriteAllBytes("C:\\iv", RijndaelParameters.IV);
        }
    }
}
