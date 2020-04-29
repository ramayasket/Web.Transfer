using System.Reflection;

/*
	Version history
	===============================================================================
	Date		Version		Comments
	===============================================================================
	29.04.2020	0.0.0.1		Project created
*/

[assembly: AssemblyVersion("0.0.0.1")]

[assembly: AssemblyProduct("Web Transfer Protocol (WTP)")]
[assembly: AssemblyCompany("Andrei Samoylov")]
[assembly: AssemblyCopyright("Copyright (C) Andrei Samoylov 2020")]

////
//// Public key as per Web.Transfer.pfx
////
namespace Web.Transfer.Properties
{
	/// <summary>
	/// Public key to use with InternalsVisibleTo attribute.
	/// </summary>
	internal class MainPublicKey
	{
		/// <summary>
		/// Public key value.
		/// </summary>
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