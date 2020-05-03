using System;
using System.IO;
using model.Helpers;
using model.Zip;

namespace model
{
    partial class Program
    {
        static void ZipStreaming()
        {
            string INPUT = TestData.testpath("input");
            string TRANSFORMED = TestData.testpath("zip");
            string OUTPUT = TestData.testpath("output");

            TestData.Cleanup();

            Console.WriteLine("Starting \"to\" transform...");

            var BUFFER_SIZE = 1024;
            var buffer = new byte[BUFFER_SIZE];

            using (var inputStream = File.OpenRead(INPUT))
            {
                using (var outputStream = File.OpenWrite(TRANSFORMED))
                {
                    using (var zipStream = new SingularDeflaterStream(outputStream, "Zlp!"))
                    {
                        var total = StreamHelper.ReadAndWriteAll(inputStream, zipStream, buffer);
                        Console.WriteLine($@"Total number of bytes read/deflated is {total}");
                    }
                }
            }

            //return;

            Console.WriteLine("About to transform from");

            using (var inputStream = File.OpenRead(TRANSFORMED))
            {
                using (var outputStream = File.OpenWrite(OUTPUT))
                {
                    using (var inflater = new SingularInflater(inputStream, "Zlp!"))
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
