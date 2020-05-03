using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace model
{
    public static class TestData
    {
        public const string TEST_DIRECTORY = "C:\\";
        public const string TEST_PATTERN = "1.*";

        /// <summary>
        /// Returns test file name based on given kind.
        /// </summary>
        /// <param name="kind">Test file kind.</param>
        public static string testfile(string kind) => "1." + kind;

        /// <summary>
        /// Returns test file path based on given kind.
        /// </summary>
        /// <param name="kind">Test file kind.</param>
        public static string testpath(string kind) => TEST_DIRECTORY + testfile(kind);

        public static void Cleanup()
        {
            var testNamed = Directory.EnumerateFiles(TEST_DIRECTORY, TEST_PATTERN)
                .Except(new[] { testpath("cmd"), testpath("input") })
                .ToArray()
                ;

            foreach (var f in testNamed)
            {
                File.Delete(f);
                Console.WriteLine($"Deleted test file {f}");
            }
        }
    }
}
