using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;
using Web.Transfer.Crypto;
using Web.Transfer.Helpers;

namespace model
{
    partial class Program
    {
        static void CryptoStreaming()
        {
            string INPUT = TestData.testpath("input");
            string TRANSFORMED = TestData.testpath("secret");
            string OUTPUT = TestData.testpath("output");

            TestData.Cleanup();

            Console.WriteLine("Starting \"to\" transform...");

            var BUFFER_SIZE = 1024;
            var buffer = new byte[BUFFER_SIZE];

            using (var inputStream = File.OpenRead(INPUT))
            {
                using (var outputStream = File.OpenWrite(TRANSFORMED))
                {
                    //using (var zipStream = new SingularDeflaterStream(outputStream))
                    //{
                    //    var total = StreamHelper.ReadAndWriteAll(inputStream, zipStream, buffer);
                    //    Console.WriteLine($@"Total number of bytes read/deflated is {total}");
                    //}

                    using (var crypting = new RijndaelStreamedCrypting(outputStream, "Zlp!", CryptoStreamMode.Write))
                    {
                        var total = StreamHelper.PumpAll(inputStream, crypting.CryptoStream, buffer);
                        Console.WriteLine($@"Total number of bytes read is {total}");
                    }
                }
            }

            Console.WriteLine("Starting \"from\" transform...");

            //var my_secret = File.ReadAllBytes(TRANSFORMED);
            //var my_unsecret = RijndaelCrypting.Decrypt2(my_secret, "Zlp!");

            using (var inputStream = File.OpenRead(TRANSFORMED))
            {
                using (var outputStream = File.OpenWrite(OUTPUT))
                {
                    using (var crypting = new RijndaelStreamedCrypting(inputStream, "Zlp!", CryptoStreamMode.Read))
                    {
                        var total = StreamHelper.PumpAll(crypting.CryptoStream, outputStream, buffer);
                        Console.WriteLine($@"Total number of bytes written is {total}");
                    }
                    //using (var inflater = new SingularInflater(inputStream))
                    //{
                    //    using (var inflaterStream = inflater.InputStream)
                    //    {
                    //        var total = StreamHelper.ReadAndWriteAll(inflaterStream, outputStream, buffer);
                    //        Console.WriteLine($@"Total number of bytes read/inflated is {total}");
                    //    }
                    //}
                }
            }

            Console.WriteLine("Press ENTER to quit...");
            Console.ReadLine();
        }
    }
}
