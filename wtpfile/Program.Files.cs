namespace Web.Transfer
{
    partial class Program
    {
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

            using (readStream)
            {

                var writeStream = GuardedFileOpen(target, true);

                if (null == writeStream)
                    return false;

                using (writeStream)
                {

                    Console.WriteLine();
                    var op = operation(readStream, writeStream);

                    return op;
                }
            }
        }

        /// <summary>
        /// Opens file for reading/writing and reports exceptions.
        /// </summary>
        /// <param name="f">File name.</param>
        /// <param name="write">True for write.</param>
        /// <returns>File stream.</returns>
        static Stream? GuardedFileOpen(string f, bool write)
        {
            try
            {
                Stream stream;

                if (write)
                {
                    File.Delete(f);
                    stream = File.OpenWrite(f);
                }
                else
                    stream = File.OpenRead(f);

                var op = write ? "writing" : "reading";
                Console.WriteLine($"File '{f}' opened for {op}");
                return stream;
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
                return null;
            }
        }
    }
}
