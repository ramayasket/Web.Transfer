using Kw.Common;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618

namespace Web.Transfer
{
    /// <summary>
    /// WTP File Converter.
    /// </summary>
    [ExcludeFromCodeCoverage]
    partial class Program
    {
        private const int FREE_FILENAME_ATTEMPTS = short.MaxValue;
        private const int BUFFER_SIZE = 1024 * 1024;
        private static readonly byte[] _buffer = new byte[BUFFER_SIZE];

        private static string _password;

        private static bool _encrypt = true; // encryption is on by default
        private static bool _compress = true; // encryption is on by default

        /// <summary>
        /// Orchestrates the application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        static void Main(string[] args)
        {
            // get file names and abort flag
            var filenames = ProcessArguments(args, out bool abort);

            if (abort)
                return;

            try
            {
                _password = AppConfig.RequiredSetting("Password");
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
                return;
            }

            // convert each file name
            foreach (var fileName in filenames)
            {
                ProcessFileName(fileName);
            }
        }

        private static void ProcessFileName(string fileName)
        {
            const string WTP_EXTENSION = ".wtp";

            if (!File.Exists(fileName))
            {

                Console.WriteLine($"File '{fileName}' doesn't exist");
                return;
            }

            var fileInfo = new FileInfo(fileName);

            Func<Stream, Stream, bool> operation;
            string direction;
            string newFileName;

            if (WTP_EXTENSION == fileInfo.Extension.ToLower())
            {

                newFileName = GetFreeFileName(fileName);
                direction = "from";
                operation = PumpConvertFrom;
            }
            else
            {

                newFileName = fileName + WTP_EXTENSION;
                direction = "to";
                operation = PumpConvertTo;
            }

            Console.WriteLine($"Converting '{fileName}' {direction} WTP format");
            Console.WriteLine();

            bool ok = false;

            var t = ExecutionTimings.Measure(() => { ok = WithStreams(fileName, newFileName, operation); });

            Console.WriteLine((ok ? "Conversion complete" : "Conversion failed") + $" {t}");
            Console.WriteLine();
        }
    }
}
