using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Kw.Common;
using model.Streaming;
using model.Zip;

namespace model
{
    partial class Program
    {
        static void Main()
        {
            const string INPUT = "C:\\1.input";
            const string PACKED = "C:\\1.zip";
            const string OUTPUT = "C:\\1.output";

            File.Delete(PACKED);
            File.Delete(OUTPUT);

            Console.WriteLine($"Files deleted: {PACKED} {OUTPUT}");

            Console.WriteLine("Starting \"to\" transform...");

            var BUFFER_SIZE = 1024;
            var buffer = new byte[BUFFER_SIZE];

            using (var inputStream = File.OpenRead(INPUT))
            {
                using (var outputStream = File.OpenWrite(PACKED))
                {
                    using (var zipStream = new SingularDeflaterStream(outputStream))
                    {
                        var total = StreamHelper.ReadAndWriteAll(inputStream, zipStream, buffer);
                        Console.WriteLine($@"Total number of bytes read/deflated is {total}");
                    }
                }
            }

            Console.WriteLine("About to transform from");

            using (var inputStream = File.OpenRead(PACKED))
            {
                using (var outputStream = File.OpenWrite(OUTPUT))
                {
                    using (var inflater = new SingularInflater(inputStream))
                    {
                        using (var inflaterStream = inflater.InputStream)
                        {
                            var total = StreamHelper.ReadAndWriteAll(inflaterStream, outputStream, buffer);
                            Console.WriteLine($@"Total number of bytes read/inflated is {total}");
                        }
                    }
                }
            }
        }
    }
}
