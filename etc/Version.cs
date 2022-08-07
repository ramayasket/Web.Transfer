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
    16.05.2020   0.0.1.0        Bx20 encoding done
    16.05.2020   0.0.2.0        Bx20 decoding done
    17.05.2020   0.0.3.0        Bx20 decode pipes
    18.05.2020   0.0.4.0        Randomized cutoffs
    18.05.2020   0.0.5.0        Encoding and decoding streams + tests
    23.05.2020   1.0.6.0        Web.Transfer.Engine + wtpfile + decoding read stream
    23.05.2020   1.0.6.1        removed test exception )))
    23.05.2020   1.0.6.2        Password in App.config
    24.05.2020   1.0.7.0        SharpZipLib replaced with GZipStream + BUG FIXED in Bx20DecodingReadStream
    24.05.2020   1.0.7.1        wtpfile using all three streams: GZip, Crypto, Bx20
    24.05.2020   1.0.7.2        GZip excluded (for performance) + refactoring
    24.05.2020   1.0.7.3        Preserve original file if exists
    29.05.2020   1.0.8.0        Compression and encrypting made optional
    29.05.2020   1.0.8.1        Kw.Common 1.2.3.0
    30.05.2020   1.0.9.0        Replacing Dictionary with array in decoding, File name detection bug fixed.
    29.07.2021   1.1.0.0        *Proofread silence.
    07.08.2022   1.2.0.0        .NET Core, Kw.Common 2.1.8
*/

[assembly: AssemblyVersion("1.2.0.0")]

[assembly: AssemblyProduct("Web Transfer Protocol (WTP)")]
[assembly: AssemblyCompany("Andrei Samoylov")]
[assembly: AssemblyCopyright("Copyright (C) Andrei Samoylov 2020-2022")]

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
