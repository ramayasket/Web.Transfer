using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/*
    Version scheme as of 1.5.2020:

    Release.Technology.Classes.Minor

    Release:     0 - prerelease, 1 - postrelease
    Technology:  Platform, language version, etc.
    Major:       major changes
    Minor:       minor changes

    Version history
    ===============================================================================
    Date         Version        Comments
    ===============================================================================
    29.04.2020   0.0.0.1        Project created
    16.05.2020   0.0.1.0        Base-32 encoding done
    16.05.2020   0.0.2.0        Base-32 decoding done
    17.05.2020   0.0.3.0        Base-32 decode pipes
    18.05.2020   0.0.4.0        Randomized cutoffs
    18.05.2020   0.0.5.0        Encoding and decoding streams + tests
    23.05.2020   1.0.6.0        Web.Transfer.Engine + wtpfile + decoding read stream
    23.05.2020   1.0.6.1        removed test exception )))
    23.05.2020   1.0.6.2        Password in App.config
    24.05.2020   1.0.7.0        SharpZipLib replaced with GZipStream + BUG FIXED in Base32DecodingReadStream
    24.05.2020   1.0.7.1        wtpfile using all three streams: GZip, Crypto, Base32
    24.05.2020   1.0.7.2        GZip excluded (for performance) + refactoring
*/

[assembly: AssemblyVersion("1.0.7.2")]

[assembly: AssemblyProduct("Web Transfer Protocol (WTP)")]
[assembly: AssemblyCompany("Andrei Samoylov")]
[assembly: AssemblyCopyright("Copyright (C) Andrei Samoylov 2020")]

////
//// Public key as per Web.Transfer.pfx
////
namespace Web.Transfer
{
    /// <summary>
    /// Product's public key.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class PublicKey
    {
        /// <summary/>
        internal const string HexValue =
            @"00240000048000009400000006020000" +
            @"00240000525341310004000001000100" +
            @"FD708BA3DEE556731C23AF73BFC6C561" +
            @"D0A0C60A3CEE2429D80493454C40095D" +
            @"DCEE07E92C67D2D7D60A9C0EA65B5D11" +
            @"95F418B852DEBB49001BAC43B10C2560" +
            @"E9A577C7E46AAFCC3A0D8C9610CCCCE2" +
            @"07492390A31E7ED1A5369E9F3B63D348" +
            @"AA482DE8288021FC0D7F8A20D789491B" +
            @"CBECF8D5EB343A9C9EB4A7391B9B32E1";
    }
}
