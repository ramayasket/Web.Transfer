using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Transfer.Tests
{
    /// <summary>
    /// Methods common for all tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Common
    {
        ////
        //// These two methods are replicas of those from Bx20Core.
        //// But formally we need to have them here in order to test those in Bx20Core.
        ////

        /// <summary>
        /// Verifies that given character is a valid Base-32 character.
        /// </summary>
        /// <param name="c">Character to verify.</param>
        /// <returns>True for valid character, False otherwise.</returns>
        internal static bool IsEncodeCharacter(char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= '1' && c <= '6') || IsCutoffCharacter(c))
                return true;

            return false;
        }

        /// <summary>
        /// Verifies that given character is a valid Base-32 cutoff character.
        /// </summary>
        /// <param name="c">Character to verify.</param>
        /// <returns>True for valid character, False otherwise.</returns>
        internal static bool IsCutoffCharacter(char c)
        {
            if ((c >= '7' && c <= '9') || c == '0')
                return true;

            return false;
        }
    }
}
