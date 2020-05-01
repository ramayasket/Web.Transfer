using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace model
{
	/// <summary>
	/// Converts between byte arrays and URL-compliant strings not compatible with Base64 by 
	/// </summary>
	public static class ConfidentialConverter
	{
		public static string ToConfidentialString(this byte[] data)
		{
			var base64 = Convert.ToBase64String(data, Base64FormattingOptions.None);
			// TODO: preallocate string and replace via char array
			var confidential = base64.Replace("/", "_").Replace("+", "-").Replace("=", "!") + "$";
			// TODO: invert case
			return confidential;
		}

		public static byte[] FromConfidentialString(this string data)
		{
			if(string.IsNullOrWhiteSpace(data))
				throw new ArgumentNullException(nameof(data));

			if('$' != data[data.Length-1])
				throw new ArgumentException("Invalid confidential format", nameof(data));

			// TODO: preallocate string and replace via char array
			var base64 = data.Substring(0, data.Length - 1).Replace("_", "/").Replace("-", "+").Replace("!", "=");
			// TODO: invert case
			var bytes = Convert.FromBase64String(base64);

			return bytes;
		}
	}
}
