using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Kw.Common;
using Web.Transfer.Base32;
using Web.Transfer.Crypto;
using Web.Transfer.Helpers;

namespace Web.Transfer
{
    /// <summary>
    /// WTP File Converter.
    /// </summary>
    [ExcludeFromCodeCoverage]
    partial class Program
    {
        /// <summary>
        /// Prints assembly title, executable name, version and usage information, if needed.
        /// </summary>
        /// <param name="assembly">Entry assembly.</param>
        /// <param name="usage">True to print usage information.</param>
        static void PrintHeader(Assembly assembly, bool usage = false)
        {
            var parser = new Regex(@"(?<name>.+), Version=(?<version>.+), Culture=(?<culture>.+), PublicKeyToken=(?<token>.+)");
            var parsed = parser.Match(assembly.FullName);

            var version = parsed.Groups["version"].Value;
            var exe = parsed.Groups["name"].Value;

            var description = GetAssemblyAttribute(assembly, typeof(AssemblyDescriptionAttribute));
            var copyright = GetAssemblyAttribute(assembly, typeof(AssemblyCopyrightAttribute));

            var header = $"{description} ({exe}) {version}";

            Console.WriteLine(header);
            Console.WriteLine(copyright);
            Console.WriteLine();

            if (usage) {
                Console.WriteLine($"Usage: {exe} <filename>");
            }
        }

        static string GetAssemblyAttribute(Assembly a, Type attributeType)
        {
            return (string)a
                    .CustomAttributes
                    .Single(x => x.AttributeType == attributeType)
                    .ConstructorArguments[0]
                    .Value
                ;
        }

        static void Main(string[] args)
        {
            const string WTP_EXTENSION = ".wtp";

            var entry = Assembly.GetEntryAssembly();

            Debug.Assert(null != entry);

            var fileName = args.FirstOrDefault();
            var hasFileName = !string.IsNullOrWhiteSpace(fileName);

            PrintHeader(entry, !hasFileName);

            if (!hasFileName) return;

            if (!File.Exists(fileName)) {

                Console.WriteLine($"File '{fileName}' doesn't exist");
                return;
            }

            try {
                _password = AppConfig.RequiredSetting("Password");
            }
            catch (Exception x) {
                Console.WriteLine(x.Message);
                return;
            }

            var fileInfo = new FileInfo(fileName);
            _bytesTotal = fileInfo.Length;

            Func<Stream, Stream, bool> operation;
            string direction;
            string newFileName;

            if (WTP_EXTENSION == fileInfo.Extension.ToLower()) {

                newFileName = Path.GetFileNameWithoutExtension(fileName);
                direction = "from";
                operation = PumpConvertFromProtocol;
            }
            else {

                newFileName = fileName + WTP_EXTENSION;
                direction = "to";
                operation = PumpConvertToProtocol;
            }

            Console.WriteLine($"Converting '{fileName}' {direction} WTP format");
            Console.WriteLine();

            bool ok = false;

            var t = ExecutionTimings.Measure(() => { ok = WithStreams(fileName, newFileName, operation); });

            Console.WriteLine((ok ? "Conversion complete" : "Conversion failed") + $" {t}");
        }

        private const int BUFFER_SIZE = 1024 * 1024;
        private static readonly byte[] _buffer = new byte[BUFFER_SIZE];

        private static long _bytesPumped = 0;
        private static long _bytesTotal = 0;

        private static string _password;

        static void PumpIntervention(int pumped)
        {
            _bytesPumped += pumped;

            Console.Write($"\rPumped {_bytesPumped:##,###} bytes: ");
        }

        static bool PumpConvertFromProtocol(Stream readStream, Stream writeStream)
        {
            try {
                using (var base32Decoder = new Base32DecodingReadStream(readStream)) {
                    using (var cryptoDecoder = new RijndaelStreamedCrypting(base32Decoder, _password, CryptoStreamMode.Read)) {
                        StreamHelper.PumpAll(cryptoDecoder.CryptoStream, writeStream, _buffer, PumpIntervention);
                        Console.WriteLine("success");
                        return true;
                    }
                }
            }
            catch (Exception x) {
                Console.WriteLine(x.Message);
                return false;
            }
        }

        static bool PumpConvertToProtocol(Stream readStream, Stream writeStream)
        {
            try {
                using (var base32Encoder = new Base32EncodingStream(writeStream)) {
                    using (var cryptoEncoder = new RijndaelStreamedCrypting(base32Encoder, _password, CryptoStreamMode.Write)) {
                        StreamHelper.PumpAll(readStream, cryptoEncoder.CryptoStream, _buffer, PumpIntervention);
                        Console.WriteLine("success");
                        return true;
                    }
                }
            }
            catch (Exception x) {
                Console.WriteLine(x.Message);
                return false;
            }
        }

        /// <summary>
        /// Opens input and output streams and calls conversion operation with them as parameters.
        /// </summary>
        /// <param name="source">Input file.</param>
        /// <param name="target">Output file.</param>
        /// <param name="operation">Conversion operation.</param>
        /// <returns></returns>
        static bool WithStreams(string source, string target, Func<Stream, Stream, bool> operation)
        {
            var readStream = GuardedFileOpen(source, false);

            if (null == readStream)
                return false;

            using (readStream) {

                var writeStream = GuardedFileOpen(target, true);

                if (null == writeStream)
                    return false;

                using (writeStream) {

                    Console.WriteLine();
                    var op = operation(readStream, writeStream);

                    return op;
                }
            }
        }

        static Stream GuardedFileOpen(string f, bool write)
        {
            try {
                Stream stream;

                if (write) {
                    File.Delete(f);
                    stream = File.OpenWrite(f);
                }
                else
                    stream = File.OpenRead(f);

                var op = write ? "writing" : "reading";
                Console.WriteLine($"File '{f}' opened for {op}");
                return stream;
            }
            catch (Exception x) {
                Console.WriteLine(x.Message);
                return null;
            }
        }
    }
}
