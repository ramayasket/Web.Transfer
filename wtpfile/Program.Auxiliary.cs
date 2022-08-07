using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Web.Transfer
{
    partial class Program
    {
        /// <summary>
        /// Prints assembly title, executable name, version and usage information, if needed.
        /// </summary>
        /// <param name="abortMessage">Why to abort conversion.</param>
        static void PrintHeader(string abortMessage)
        {
            var entry = Assembly.GetEntryAssembly();

            Debug.Assert(null != entry);

            var parser = new Regex(@"(?<name>.+), Version=(?<version>.+), Culture=(?<culture>.+), PublicKeyToken=(?<token>.+)");
            var parsed = parser.Match(entry.FullName!);

            var version = parsed.Groups["version"].Value;
            var exe = parsed.Groups["name"].Value;

            var description = GetAssemblyAttribute(entry, typeof(AssemblyDescriptionAttribute));
            var copyright = GetAssemblyAttribute(entry, typeof(AssemblyCopyrightAttribute));

            var header = $"{description} ({exe}) {version}";

            Console.WriteLine(header);
            Console.WriteLine(copyright);
            Console.WriteLine();

            if (!string.IsNullOrWhiteSpace(abortMessage))
            {
                Console.WriteLine(abortMessage);
                Console.WriteLine();
                Console.WriteLine($"Usage: {exe} <filename1> [<filename2> ...] [-z:{{Y|N}}] [-c:{{Y|N}}]");
            }
            else
            {
                Console.WriteLine($"Conversion mode: encryption={(_encrypt ? 'Y' : 'N')} compression={(_compress ? 'Y' : 'N')}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Extracts attribute from assembly.
        /// </summary>
        /// <returns>Attribute value.</returns>
        static string GetAssemblyAttribute(Assembly a, Type attributeType)
        {
            return (string)a
                    .CustomAttributes
                    .Single(x => x.AttributeType == attributeType)
                    .ConstructorArguments[0]
                    .Value!
                ;
        }

        /// <summary>
        /// Checks if argument is (can be) a file name.
        /// </summary>
        /// <param name="x">Argument.</param>
        static bool IsFileName(string x)
        {
            try
            {
                if (x.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                    return false;

                var fp = Path.GetFullPath(x);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if argument is a valid option.
        /// </summary>
        /// <param name="x">Argument.</param>
        static bool IsOption(string x) => Option.Parser.IsMatch(x);

        /// <summary>
        /// Command-line option for this application.
        /// </summary>
        class Option
        {
            public static Regex Parser { get; } = new Regex("^-(?<name>[zc]):(?<value>[YN])$");

            public enum Names { z, c, } // z - compression, c - encryption
            public enum Values { Y, N, } // Y - enabled, N - disabled

            public Names Name { get; }
            public Values Value { get; }

            /// <summary>
            /// Fills <seealso cref="Name"/> and <seealso cref="Value"/> for newly created option.
            /// </summary>
            public Option(string s)
            {
                var parsed = Parser.Match(s);

                Name = (Names)Enum.Parse(typeof(Names), parsed.Groups["name"].Value);
                Value = (Values)Enum.Parse(typeof(Values), parsed.Groups["value"].Value);
            }
        }

        /// <summary>
        /// Checks validity of command-line arguments and returns array of file names.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <param name="abort">Flag to abort processing.</param>
        /// <returns>File names.</returns>
        static string[] ProcessArguments(string[] args, out bool abort)
        {
            string abortMessage = null!;

            var filenames = args.Where(IsFileName).ToArray();
            var options = args.Where(IsOption).Select(x => new Option(x)).ToArray();

            //// options grouped by name
            var optionsByName = options.GroupBy(o => o.Name).ToArray();

            foreach (var x in optionsByName)
            {

                // different values for a single option
                var values = x.Select(z => z.Value).Distinct().ToArray();

                if (values.Length > 1)
                { // only 1 value is allowed
                    abortMessage = "Conflicting options specified.";
                    break;
                }
            }

            if (string.IsNullOrEmpty(abortMessage))
            {

                // there should not be anything except file names and options
                if (filenames.Length + options.Length < args.Length || args.Any(string.IsNullOrEmpty))
                    abortMessage = "Incorrect parameter list";

                else
                {
                    // should be at least 1 file name
                    if (!filenames.Any())
                        abortMessage = "Specified no files to process";
                }
            }

            foreach (var option in options)
                switch (option.Name)
                {
                    case Option.Names.c:
                        _encrypt = option.Value == Option.Values.Y;
                        break;
                    case Option.Names.z:
                        _compress = option.Value == Option.Values.Y;
                        break;
                }

            // with all arguments processed, we can print header
            PrintHeader(abortMessage);

            abort = !string.IsNullOrWhiteSpace(abortMessage);

            return filenames;
        }

        /// <summary>
        /// For a filename 'name.ext', finds next free filename in format 'name (N).ext'.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <returns>Resultant file name.</returns>
        private static string GetFreeFileName(string filename)
        {
            var directory = Path.GetDirectoryName(filename) ?? "";

            var original = Path.GetFileNameWithoutExtension(filename);

            var unchanged = Path.Combine(directory, original);

            if (!File.Exists(unchanged))
                return unchanged;

            var originalFileName = Path.GetFileNameWithoutExtension(original);
            var originalExtension = Path.GetExtension(original);

            string changed = unchanged;

            for (int i = 1; i < FREE_FILENAME_ATTEMPTS; i++)
            {
                changed = Path.Combine(directory, $"{originalFileName} ({i}){originalExtension}");
                if (!File.Exists(changed))
                    return changed;
            }

            return changed;
        }
    }
}
